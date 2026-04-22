namespace OsuDroid.Game.Runtime;

public interface IMenuSfxPlayer
{
    void Play(string key);
}

public sealed class NoOpMenuSfxPlayer : IMenuSfxPlayer
{
    public void Play(string key)
    {
    }
}
