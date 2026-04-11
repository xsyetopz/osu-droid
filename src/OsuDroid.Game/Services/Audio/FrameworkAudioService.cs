using OsuDroid.Game.Resources;

namespace OsuDroid.Game.Services.Audio;

public sealed class FrameworkAudioService(IGameResources resources) : IAudioService
{
    public void PlayMenuSample(MenuSample sample)
    {
        resources.GetSample(sample)?.Play();
    }
}
