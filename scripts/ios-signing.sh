#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'TXT'
Usage:
  scripts/ios-signing.sh profile-info [--full] <profile.mobileprovision|profile.provisionprofile>
  scripts/ios-signing.sh audit [profile...]
  scripts/ios-signing.sh install-profile <profile.mobileprovision|profile.provisionprofile>
  scripts/ios-signing.sh resolve-build-settings <profile.mobileprovision|profile.provisionprofile> <bundle-id> <device-udid> [codesign-sha1]
TXT
}

die() {
  echo "ERROR: $*" >&2
  exit 1
}

decode_profile() {
  local profile="$1"
  if security cms -D -i "$profile" 2>/dev/null; then
    return 0
  fi
  openssl smime -inform der -verify -noverify -in "$profile" 2>/dev/null
}

profile_json() {
  local profile="$1"
  python3 - "$profile" <<'PY'
import base64, hashlib, json, plistlib, subprocess, sys, tempfile
profile = sys.argv[1]

def decode(path: str) -> bytes:
    p = subprocess.run(['security', 'cms', '-D', '-i', path], stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
    if p.returncode == 0 and p.stdout:
        return p.stdout
    p = subprocess.run(['openssl', 'smime', '-inform', 'der', '-verify', '-noverify', '-in', path], stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
    if p.returncode == 0 and p.stdout:
        return p.stdout
    raise SystemExit(f"decode failed: {path}")

def cert_info(der: bytes) -> dict[str, str]:
    sha1 = hashlib.sha1(der).hexdigest().upper()
    with tempfile.NamedTemporaryFile(prefix='osudroid_profile_cert_', suffix='.der', delete=True) as tf:
        tf.write(der)
        tf.flush()
        p = subprocess.run(['openssl', 'x509', '-inform', 'DER', '-in', tf.name, '-noout', '-subject'], stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, text=True)
    subject = p.stdout.strip().removeprefix('subject=').strip() if p.returncode == 0 else ''
    parts = {}
    for chunk in subject.split(', '):
        if '=' in chunk:
            key, value = chunk.split('=', 1)
            parts[key.strip()] = value.strip()
    return {
        'sha1': sha1,
        'subject': subject,
        'subject_ou': parts.get('OU', ''),
        'subject_cn': parts.get('CN', ''),
    }

obj = plistlib.loads(decode(profile))
ent = obj.get('Entitlements') or {}
app_id = ent.get('application-identifier') or ent.get('com.apple.application-identifier') or ''
bundle_id = app_id.split('.', 1)[1] if isinstance(app_id, str) and '.' in app_id else ''
certs = obj.get('DeveloperCertificates') or []
cert = cert_info(certs[0]) if certs else {'sha1': '', 'subject': '', 'subject_ou': '', 'subject_cn': ''}
out = {
    'path': profile,
    'name': obj.get('Name', ''),
    'uuid': obj.get('UUID', ''),
    'team_identifiers': obj.get('TeamIdentifier') or [],
    'team_name': obj.get('TeamName', ''),
    'app_id': app_id,
    'bundle_id': bundle_id,
    'platform': obj.get('Platform') or [],
    'provisioned_devices': obj.get('ProvisionedDevices') or [],
    'developer_certificate': cert,
    'expiration_date': str(obj.get('ExpirationDate', '')),
}
print(json.dumps(out, indent=2, sort_keys=True))
PY
}

profile_field() {
  local profile="$1" expr="$2"
  profile_json "$profile" | python3 -c "import json,sys; obj=json.load(sys.stdin); val=$expr; print(val if val is not None else '')"
}

find_profiles() {
  if [ "$#" -gt 0 ]; then
    printf '%s\n' "$@"
    return 0
  fi
  find "$HOME/Library/MobileDevice/Provisioning Profiles" "$HOME/Documents/Profiles" "$HOME/Documents/profiles" \
    -maxdepth 1 -type f \( -name '*.mobileprovision' -o -name '*.provisionprofile' \) -print 2>/dev/null || true
}

cmd_profile_info() {
  local full=0
  if [ "${1:-}" = "--full" ]; then
    full=1
    shift
  fi
  local profile="${1:-}"
  [ -n "$profile" ] || die "profile path required"
  [ -f "$profile" ] || die "profile not found: $profile"

  if [ "$full" -eq 1 ]; then
    profile_json "$profile"
    return 0
  fi

  profile_json "$profile" | python3 -c 'import json, sys
obj = json.load(sys.stdin)
cert = obj["developer_certificate"]
print("profile: {}".format(obj["path"]))
print("name: {}".format(obj["name"]))
print("uuid: {}".format(obj["uuid"]))
print("team_identifiers: {}".format(", ".join(obj["team_identifiers"])))
print("team_name: {}".format(obj["team_name"]))
print("app_id: {}".format(obj["app_id"]))
print("bundle_id: {}".format(obj["bundle_id"]))
print("devices: {}".format(len(obj["provisioned_devices"])))
print("cert_sha1: {}".format(cert["sha1"]))
print("cert_subject_ou: {}".format(cert["subject_ou"]))
print("cert_subject_cn: {}".format(cert["subject_cn"]))'
}

cmd_audit() {
  local bundle_id="${IOS_BUNDLE_ID:-moe.osudroid}"
  local device_id="${IOS_DEVICE_ID:-00008110-0005149C2132401E}"
  local codesign_sha1="${IOS_CODESIGN_KEY:-}"
  local profiles
  profiles="$(find_profiles "$@")"
  [ -n "$profiles" ] || die "no provisioning profiles found"
  while IFS= read -r profile; do
    [ -n "$profile" ] && [ -f "$profile" ] || continue
    profile_json "$profile" | python3 -c 'import json, sys
want_bundle, want_device, want_sha1 = sys.argv[1:4]
obj = json.load(sys.stdin)
cert = obj["developer_certificate"]
bundle = obj["bundle_id"]
devices = set(obj["provisioned_devices"])
cert_sha1 = cert["sha1"].upper()
want_sha1 = want_sha1.upper()
team = obj["team_identifiers"][0] if obj["team_identifiers"] else ""
checks = []
checks.append(("bundle", bundle == "*" or bundle == want_bundle))
checks.append(("device", not want_device or want_device in devices))
checks.append(("cert", not want_sha1 or cert_sha1 == want_sha1))
checks.append(("team-cert-ou", not team or cert["subject_ou"] == team))
status = "OK" if all(ok for _, ok in checks) else "NO"
print("==> {} ({}) [{}]".format(obj["name"], obj["uuid"], status))
print("  path: {}".format(obj["path"]))
print("  team: {} cert_ou: {}".format(team, cert["subject_ou"]))
print("  bundle: {} want: {}".format(bundle, want_bundle))
print("  devices: {} has_want_device: {}".format(len(devices), want_device in devices if want_device else "n/a"))
print("  cert_sha1: {} matches: {}".format(cert_sha1, cert_sha1 == want_sha1 if want_sha1 else "n/a"))
for name, ok in checks:
    if not ok:
        print("  FAIL: {}".format(name))' "$bundle_id" "$device_id" "$codesign_sha1"
  done <<< "$profiles"
}

cmd_install_profile() {
  local profile="${1:-}"
  [ -n "$profile" ] || die "profile path required"
  [ -f "$profile" ] || die "profile not found: $profile"
  local uuid dst
  uuid="$(profile_field "$profile" "obj.get('uuid','')")"
  [ -n "$uuid" ] || die "could not read profile UUID"
  dst="$HOME/Library/MobileDevice/Provisioning Profiles/${uuid}.mobileprovision"
  mkdir -p "$(dirname "$dst")"
  cp -f "$profile" "$dst"
  echo "$dst"
}

cmd_resolve_build_settings() {
  local profile="${1:-}" bundle_id="${2:-}" device_id="${3:-}" codesign_sha1="${4:-}"
  [ -n "$profile" ] || die "profile path required"
  [ -n "$bundle_id" ] || die "bundle id required"
  [ -f "$profile" ] || die "profile not found: $profile"
  local installed
  installed="$(cmd_install_profile "$profile")"
  profile_json "$installed" | python3 -c 'import json, sys
bundle_id, device_id, codesign_sha1 = sys.argv[1:4]
obj = json.load(sys.stdin)
cert = obj["developer_certificate"]
team = obj["team_identifiers"][0] if obj["team_identifiers"] else ""
profile_bundle = obj["bundle_id"]
devices = set(obj["provisioned_devices"])
cert_sha1 = cert["sha1"].upper()
if profile_bundle not in ("*", bundle_id):
    raise SystemExit("ERROR: profile bundle mismatch: profile has {}, expected {} or *".format(profile_bundle, bundle_id))
if device_id and device_id not in devices:
    raise SystemExit("ERROR: profile does not include device UDID: {}".format(device_id))
if team and cert["subject_ou"] != team:
    raise SystemExit("ERROR: profile TeamIdentifier {} does not match certificate Subject OU {}".format(team, cert["subject_ou"]))
if codesign_sha1 and cert_sha1 != codesign_sha1.upper():
    raise SystemExit("ERROR: profile embedded certificate SHA1 {} does not match IOS_CODESIGN_KEY {}".format(cert_sha1, codesign_sha1.upper()))
print("CODESIGN_PROVISION={}".format(obj["name"]))
print("CODESIGN_TEAM_ID={}".format(team))
print("CODESIGN_KEY={}".format(cert_sha1))
print("CODESIGN_PROFILE_UUID={}".format(obj["uuid"]))' "$bundle_id" "$device_id" "$codesign_sha1"
}

cmd="${1:-}"
shift || true
case "$cmd" in
  profile-info) cmd_profile_info "$@" ;;
  audit) cmd_audit "$@" ;;
  install-profile) cmd_install_profile "$@" ;;
  resolve-build-settings) cmd_resolve_build_settings "$@" ;;
  -h|--help|help|'') usage ;;
  *) usage; die "unknown command: $cmd" ;;
esac
