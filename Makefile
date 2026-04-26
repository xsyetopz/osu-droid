SOLUTION := OsuDroid.sln
GAME_TEST_PROJECT := tests/OsuDroid.Game.Tests/OsuDroid.Game.Tests.csproj
APP_PROJECT := src/OsuDroid.App/OsuDroid.App.csproj
DOTNET_BUILD_FLAGS := -nr:false
LOCAL_AUDIT_BYPASS_FLAGS := -p:NuGetAudit=false -p:WarningsNotAsErrors=NU1900

ANDROID_PACKAGE := moe.osudroid
ANDROID_APK := src/OsuDroid.App/bin/Debug/net9.0-android/moe.osudroid-Signed.apk

IOS_BUNDLE_ID := moe.osudroid
IOS_BUILD_SCRIPT := scripts/build-ios-device.sh
IOS_VERIFY_BUNDLE_SCRIPT := scripts/verify-ios-bundle.sh
IOS_VERIFY_INSTALL_SCRIPT := scripts/verify-ios-install.sh
IOS_UNINSTALL_SCRIPT := scripts/uninstall-ios-device.sh
IOS_DOCTOR_SCRIPT := scripts/doctor-ios-device.sh
IOS_XCODE_SCRIPT := scripts/xcode-ios-device.sh
IOS_APP_PATH_SCRIPT := $(IOS_BUILD_SCRIPT) --print-app-path
IOS_DEVELOPER_DIR ?= /Applications/Xcode_26.3.app/Contents/Developer
IOS_PROVISIONING_PROFILE ?=
IOS_CODESIGN_KEY ?=
IOS_TEAM_ID ?=

ifdef ANDROID_SERIAL
ADB := adb -s $(ANDROID_SERIAL)
else
ADB := adb
endif

.PHONY: \
	bootstrap restore build test format check architecture-check boundary-check localization-check stale-term-check clean \
	test-no-build \
	build-android build-ios verify-android-bass \
	verify-ios-bundle \
	install-android uninstall-android reinstall-android launch-android \
	install-ios uninstall-ios reinstall-ios launch-ios doctor-ios xcode-ios \
	run-android run-ios

require-ios-signing:
	@[ -n "$(IOS_CODESIGN_KEY)" ] || (echo "IOS_CODESIGN_KEY is required. Discover it with: security find-identity -v -p codesigning"; exit 1)
	@[ -n "$(IOS_TEAM_ID)" ] || [ -n "$(IOS_PROVISIONING_PROFILE)" ] || (echo "IOS_TEAM_ID is required unless IOS_PROVISIONING_PROFILE is provided."; exit 1)

bootstrap:
	./scripts/bootstrap-third-party.sh
	dotnet workload restore $(APP_PROJECT) -p:BuildMobile=true

restore:
	dotnet restore $(SOLUTION)
	dotnet tool restore

build:
	dotnet build $(SOLUTION) --warnaserror $(DOTNET_BUILD_FLAGS)

test:
	dotnet test $(GAME_TEST_PROJECT) --verbosity normal

test-no-build:
	dotnet test $(GAME_TEST_PROJECT) --no-build --verbosity normal

format:
	dotnet tool restore
	dotnet csharpier format .
	dotnet format $(SOLUTION)

architecture-check:
	python3 scripts/dev/architecture_audit.py --fail-on-findings >/dev/null

boundary-check:
	python3 scripts/dev/check-boundaries.py

localization-check:
	python3 scripts/dev/generate-osudroid-localization.py --check

stale-term-check:
	python3 scripts/dev/check-stale-terms.py

check: architecture-check boundary-check localization-check stale-term-check
	dotnet tool restore
	dotnet csharpier check .
	dotnet format $(SOLUTION) --verify-no-changes --verbosity diagnostic

clean:
	dotnet clean $(SOLUTION) -v q
	rm -rf src/OsuDroid.App/bin src/OsuDroid.App/obj
	rm -rf src/OsuDroid.Game/bin src/OsuDroid.Game/obj
	rm -rf tests/OsuDroid.Game.Tests/bin tests/OsuDroid.Game.Tests/obj

build-android:
	OSUDROID_VERSION_NAME=$$(date -u +%Y.%-m%d.0); OSUDROID_VERSION_CODE=$$(date -u +%s); dotnet build $(APP_PROJECT) -c Debug -f net9.0-android $(DOTNET_BUILD_FLAGS) $(LOCAL_AUDIT_BYPASS_FLAGS) -p:BuildMobile=true -p:MobileTarget=android -p:ApplicationDisplayVersion=$$OSUDROID_VERSION_NAME -p:ApplicationVersion=$$OSUDROID_VERSION_CODE

verify-android-bass:
	@echo "BASS verification skipped: MonoGame build does not package osu-framework BASS natives."

build-ios: require-ios-signing
	IOS_DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" IOS_PROVISIONING_PROFILE="$(IOS_PROVISIONING_PROFILE)" IOS_CODESIGN_KEY="$(IOS_CODESIGN_KEY)" IOS_TEAM_ID="$(IOS_TEAM_ID)" ./$(IOS_BUILD_SCRIPT)

verify-ios-bundle:
	@IOS_APP_PATH=$$(IOS_DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" IOS_PROVISIONING_PROFILE="$(IOS_PROVISIONING_PROFILE)" IOS_CODESIGN_KEY="$(IOS_CODESIGN_KEY)" IOS_TEAM_ID="$(IOS_TEAM_ID)" $(IOS_APP_PATH_SCRIPT)); \
	test -d "$$IOS_APP_PATH"; \
	IOS_DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" IOS_PROVISIONING_PROFILE="$(IOS_PROVISIONING_PROFILE)" IOS_CODESIGN_KEY="$(IOS_CODESIGN_KEY)" ./$(IOS_VERIFY_BUNDLE_SCRIPT) "$$IOS_APP_PATH"

install-android: build-android verify-android-bass
	test -f "$(ANDROID_APK)"
	$(ADB) install -r "$(ANDROID_APK)"

uninstall-android:
	@output="$$( $(ADB) uninstall $(ANDROID_PACKAGE) 2>&1 )" && status=0 || status=$$?; \
	printf '%s\n' "$$output"; \
	if [ "$$status" -ne 0 ] && ! printf '%s\n' "$$output" | grep -E "Unknown package|not installed" >/dev/null; then \
		exit "$$status"; \
	fi

reinstall-android: uninstall-android install-android

launch-android:
	$(ADB) shell monkey -p $(ANDROID_PACKAGE) -c android.intent.category.LAUNCHER 1

run-android: install-android launch-android

install-ios: build-ios verify-ios-bundle
	@[ -n "$(IOS_DEVICE_ID)" ] || (echo "IOS_DEVICE_ID is required. Example: make install-ios IOS_DEVICE_ID=<device-id>"; exit 1)
	@IOS_APP_PATH=$$(IOS_DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" IOS_PROVISIONING_PROFILE="$(IOS_PROVISIONING_PROFILE)" IOS_CODESIGN_KEY="$(IOS_CODESIGN_KEY)" IOS_TEAM_ID="$(IOS_TEAM_ID)" $(IOS_APP_PATH_SCRIPT)); \
	test -d "$$IOS_APP_PATH"; \
	DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" xcrun devicectl device install app --device "$(IOS_DEVICE_ID)" "$$IOS_APP_PATH" --timeout 60 --json-output /tmp/osudroid-devicectl-install.json --log-output /tmp/osudroid-devicectl-install.log; \
	IOS_DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" IOS_DEVICE_ID="$(IOS_DEVICE_ID)" IOS_BUNDLE_ID="$(IOS_BUNDLE_ID)" ./$(IOS_VERIFY_INSTALL_SCRIPT) "$$IOS_APP_PATH"

uninstall-ios:
	@[ -n "$(IOS_DEVICE_ID)" ] || (echo "IOS_DEVICE_ID is required. Example: make uninstall-ios IOS_DEVICE_ID=<device-id>"; exit 1)
	@IOS_DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" IOS_DEVICE_ID="$(IOS_DEVICE_ID)" IOS_BUNDLE_ID="$(IOS_BUNDLE_ID)" ./$(IOS_UNINSTALL_SCRIPT)

reinstall-ios: uninstall-ios install-ios

launch-ios:
	@[ -n "$(IOS_DEVICE_ID)" ] || (echo "IOS_DEVICE_ID is required. Example: make launch-ios IOS_DEVICE_ID=<device-id>"; exit 1)
	@if IOS_DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" IOS_DEVICE_ID="$(IOS_DEVICE_ID)" ./$(IOS_DOCTOR_SCRIPT); then \
		DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" xcrun devicectl device process launch --device "$(IOS_DEVICE_ID)" --terminate-existing "$(IOS_BUNDLE_ID)" --timeout 60 --json-output /tmp/osudroid-devicectl-launch.json --log-output /tmp/osudroid-devicectl-launch.log; \
	else \
		echo "CoreDevice is unhealthy on this machine. Falling back to the supported Xcode workflow."; \
		IOS_DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" IOS_DEVICE_ID="$(IOS_DEVICE_ID)" IOS_BUNDLE_ID="$(IOS_BUNDLE_ID)" ./$(IOS_XCODE_SCRIPT); \
	fi

doctor-ios:
	@[ -n "$(IOS_DEVICE_ID)" ] || (echo "IOS_DEVICE_ID is required. Example: make doctor-ios IOS_DEVICE_ID=<device-id>"; exit 1)
	@IOS_DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" IOS_DEVICE_ID="$(IOS_DEVICE_ID)" ./$(IOS_DOCTOR_SCRIPT)

xcode-ios:
	@IOS_DEVELOPER_DIR="$(IOS_DEVELOPER_DIR)" IOS_DEVICE_ID="$(IOS_DEVICE_ID)" IOS_BUNDLE_ID="$(IOS_BUNDLE_ID)" ./$(IOS_XCODE_SCRIPT)

run-ios: install-ios launch-ios
