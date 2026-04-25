# Localization

English strings are generated from upstream osu!droid sources:

- `third_party/osu-droid-legacy/res/values/*.xml`
- `third_party/osu-droid-language-pack/language-pack/src/main/res/values/*.xml`
- curated hardcoded osu!droid UI literals in `scripts/dev/generate-osudroid-localization.py`

Do not hand-edit `src/OsuDroid.Game/Localization/Strings.resx`. Run:

```sh
rtk python3 scripts/dev/generate-osudroid-localization.py
```

To add a locale, copy `src/OsuDroid.Game/Localization/Strings.locale.resx.template` to `Strings.<culture>.resx`, then translate only `<value>` text. Keep key names identical.

Check generated files with:

```sh
rtk python3 scripts/dev/generate-osudroid-localization.py --check
```
