using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Textures;

namespace OsuDroid.Game.Resources;

public sealed class GameResources(TextureStore textures, ISampleStore samples) : IGameResources
{
    public Texture GetTexture(string name) => textures.Get(name);

    public Sample? GetSample(MenuSample sample) =>
        sample switch
        {
            MenuSample.ButtonHover => samples.Get("UI/button-hover"),
            MenuSample.ButtonConfirm => samples.Get("UI/button-select"),
            MenuSample.LogoSwoosh => samples.Get("Menu/osu-logo-swoosh"),
            MenuSample.SelectDifficulty => samples.Get("SongSelect/select-difficulty"),
            _ => null
        };
}
