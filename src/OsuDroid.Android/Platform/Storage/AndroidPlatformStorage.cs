using System;
using System.Collections.Generic;
using OsuDroid.Game;

namespace OsuDroid.Android.Platform.Storage;

internal sealed class AndroidPlatformStorage : IPlatformStorage
{
    public IReadOnlyList<string> GetSongRoots() => Array.Empty<string>();
}
