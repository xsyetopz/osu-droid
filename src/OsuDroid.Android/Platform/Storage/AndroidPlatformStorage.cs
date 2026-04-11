using System.Collections.Generic;
using System.IO;
using Android.Content;
using OsuDroid.Game;

namespace OsuDroid.Android.Platform.Storage;

internal sealed class AndroidPlatformStorage(Context context) : IPlatformStorage
{
    public IReadOnlyList<string> GetSongRoots()
    {
        List<string> roots = [];

        addSongsRoot(roots, context.FilesDir?.AbsolutePath);
        addSongsRoot(roots, context.GetExternalFilesDir(null)?.AbsolutePath);

        return roots;
    }

    private static void addSongsRoot(List<string> roots, string? basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            return;
        }

        string songsPath = Path.Combine(basePath, "Songs");
        Directory.CreateDirectory(songsPath);
        roots.Add(songsPath);
    }
}
