using System;

namespace OsuDroid.Game;

public interface IExternalUriLauncher
{
    void Open(Uri uri);
}
