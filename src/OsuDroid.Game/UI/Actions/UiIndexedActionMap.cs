namespace OsuDroid.Game.UI;

internal sealed class UiIndexedActionMap
{
    private readonly UiAction firstAction;
    private readonly int count;
    private readonly (UiAction Action, int Index)[] aliases;

    public UiIndexedActionMap(UiAction firstAction, UiAction lastAction, params (UiAction Action, int Index)[] aliases)
    {
        var span = (int)lastAction - (int)firstAction;
        if (span < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lastAction), "Last action must be greater than or equal to first action.");
        }

        this.firstAction = firstAction;
        count = span + 1;
        this.aliases = aliases;

        foreach (var alias in aliases)
        {
            if ((uint)alias.Index >= (uint)count)
            {
                throw new ArgumentOutOfRangeException(nameof(aliases), $"Alias index {alias.Index} is out of range.");
            }
        }
    }

    public bool TryGetIndex(UiAction action, out int index)
    {
        var contiguousIndex = (int)action - (int)firstAction;
        if ((uint)contiguousIndex < (uint)count)
        {
            index = contiguousIndex;
            return true;
        }

        foreach (var alias in aliases)
        {
            if (alias.Action == action)
            {
                index = alias.Index;
                return true;
            }
        }

        index = -1;
        return false;
    }

    public bool TryGetAction(int index, out UiAction action)
    {
        if ((uint)index >= (uint)count)
        {
            action = UiAction.None;
            return false;
        }

        action = (UiAction)((int)firstAction + index);
        return true;
    }

    public bool Contains(UiAction action) => TryGetIndex(action, out _);
}
