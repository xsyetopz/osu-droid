using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Textures;

namespace OsuDroid.Game.Resources;

public interface IGameResources
{
    Texture GetTexture(string name);

    Sample? GetSample(MenuSample sample);
}
