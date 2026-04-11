using System;
using System.Collections.Generic;
using OsuDroid.Game;

namespace OsuDroid.iOS.Platform.Storage;

internal sealed class IOSPlatformStorage : IPlatformStorage
{
    public IReadOnlyList<string> GetSongRoots() => Array.Empty<string>();
}
