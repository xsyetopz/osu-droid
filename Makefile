SOLUTION := OsuDroid.sln
GAME_TEST_PROJECT := tests/OsuDroid.Game.Tests/OsuDroid.Game.Tests.csproj
ANDROID_PROJECT := src/OsuDroid.Android/OsuDroid.Android.csproj
IOS_PROJECT := src/OsuDroid.iOS/OsuDroid.iOS.csproj
DOTNET_BUILD_FLAGS := -nr:false
LOCAL_AUDIT_BYPASS_FLAGS := -p:NuGetAudit=false -p:WarningsNotAsErrors=NU1900

ANDROID_PACKAGE := moe.osudroid.android
ANDROID_APK := src/OsuDroid.Android/bin/Debug/net8.0-android/moe.osudroid.android-Signed.apk

IOS_BUNDLE_ID := moe.osudroid.ios
IOS_BUILD_SCRIPT := scripts/build-ios-device.sh
IOS_DOCTOR_SCRIPT := scripts/doctor-ios-device.sh
IOS_XCODE_SCRIPT := scripts/xcode-ios-device.sh
IOS_APP_PATH_SCRIPT := $(IOS_BUILD_SCRIPT) --print-app-path
IOS_CODESIGN_KEY ?=
IOS_TEAM_ID ?=

ifdef ANDROID_SERIAL
ADB := adb -s $(ANDROID_SERIAL)
else
ADB := adb
endif

.PHONY: \
	bootstrap restore build test format check clean \
	test-no-build \
	build-android build-ios verify-android-bass \
	install-android launch-android \
	install-ios launch-ios doctor-ios xcode-ios \
	run-android run-ios

require-ios-signing:
	@[ -n "$(IOS_CODESIGN_KEY)" ] || (echo "IOS_CODESIGN_KEY is required. Discover it with: security find-identity -v -p codesigning"; exit 1)
	@[ -n "$(IOS_TEAM_ID)" ] || (echo "IOS_TEAM_ID is required. Discover it with: security find-identity -v -p codesigning"; exit 1)

bootstrap:
	./scripts/bootstrap-third-party.sh
	dotnet workload restore $(SOLUTION)

restore:
	dotnet restore $(SOLUTION)

build:
	dotnet build $(SOLUTION) --warnaserror $(DOTNET_BUILD_FLAGS)

test:
	dotnet test $(GAME_TEST_PROJECT) --verbosity normal

test-no-build:
	dotnet test $(GAME_TEST_PROJECT) --no-build --verbosity normal

format:
	dotnet format $(SOLUTION)

check:
	dotnet format $(SOLUTION) --verify-no-changes --verbosity diagnostic

clean:
	dotnet clean $(SOLUTION) -v q
	rm -rf src/OsuDroid.Android/bin src/OsuDroid.Android/obj
	rm -rf src/OsuDroid.iOS/bin src/OsuDroid.iOS/obj
	rm -rf src/OsuDroid.Game/bin src/OsuDroid.Game/obj
	rm -rf tests/OsuDroid.Game.Tests/bin tests/OsuDroid.Game.Tests/obj

build-android:
	dotnet build $(ANDROID_PROJECT) -c Debug $(DOTNET_BUILD_FLAGS) $(LOCAL_AUDIT_BYPASS_FLAGS)

verify-android-bass:
	./scripts/verify-android-bass.sh "$(ANDROID_APK)"

build-ios: require-ios-signing
	IOS_CODESIGN_KEY="$(IOS_CODESIGN_KEY)" IOS_TEAM_ID="$(IOS_TEAM_ID)" ./$(IOS_BUILD_SCRIPT)

install-android: build-android verify-android-bass
	test -f "$(ANDROID_APK)"
	$(ADB) install -r "$(ANDROID_APK)"

launch-android:
	$(ADB) shell monkey -p $(ANDROID_PACKAGE) -c android.intent.category.LAUNCHER 1

run-android: install-android launch-android

install-ios: build-ios
	@[ -n "$(IOS_DEVICE_ID)" ] || (echo "IOS_DEVICE_ID is required. Example: make install-ios IOS_DEVICE_ID=<device-id>"; exit 1)
	@IOS_APP_PATH=$$(IOS_CODESIGN_KEY="$(IOS_CODESIGN_KEY)" IOS_TEAM_ID="$(IOS_TEAM_ID)" $(IOS_APP_PATH_SCRIPT)); \
	test -d "$$IOS_APP_PATH"; \
	xcrun devicectl device install app --device "$(IOS_DEVICE_ID)" "$$IOS_APP_PATH"

launch-ios:
	@[ -n "$(IOS_DEVICE_ID)" ] || (echo "IOS_DEVICE_ID is required. Example: make launch-ios IOS_DEVICE_ID=<device-id>"; exit 1)
	@if IOS_DEVICE_ID="$(IOS_DEVICE_ID)" ./$(IOS_DOCTOR_SCRIPT); then \
		xcrun devicectl device process launch --device "$(IOS_DEVICE_ID)" --terminate-existing "$(IOS_BUNDLE_ID)"; \
	else \
		echo "CoreDevice is unhealthy on this machine. Falling back to the supported Xcode workflow."; \
		IOS_DEVICE_ID="$(IOS_DEVICE_ID)" IOS_BUNDLE_ID="$(IOS_BUNDLE_ID)" ./$(IOS_XCODE_SCRIPT); \
	fi

doctor-ios:
	@[ -n "$(IOS_DEVICE_ID)" ] || (echo "IOS_DEVICE_ID is required. Example: make doctor-ios IOS_DEVICE_ID=<device-id>"; exit 1)
	@IOS_DEVICE_ID="$(IOS_DEVICE_ID)" ./$(IOS_DOCTOR_SCRIPT)

xcode-ios:
	@IOS_DEVICE_ID="$(IOS_DEVICE_ID)" IOS_BUNDLE_ID="$(IOS_BUNDLE_ID)" ./$(IOS_XCODE_SCRIPT)

run-ios: install-ios launch-ios
