namespace OsuDroid.Game.Runtime;

public interface IMenuSfxPlayer
{
    void Play(string key);

    void SetVolume(float normalizedVolume);
}

public sealed class NoOpMenuSfxPlayer : IMenuSfxPlayer
{
    public void Play(string key)
    {
    }

    public void SetVolume(float normalizedVolume)
    {
    }
}
