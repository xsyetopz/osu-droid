using System.Collections.Generic;
using System.IO;
using OsuDroid.Game;

namespace OsuDroid.iOS.Platform.Storage;

internal sealed class IOSPlatformStorage : IPlatformStorage
{
    public IReadOnlyList<string> GetSongRoots()
    {
        string songsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Songs");
        Directory.CreateDirectory(songsPath);
        return [songsPath];
    }
}
