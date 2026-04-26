namespace OsuDroid.Game.UI.Actions;

internal sealed class UiIndexedActionMap
{
    private readonly UiAction _firstAction;
    private readonly int _count;
    private readonly (UiAction Action, int Index)[] _aliases;

    public UiIndexedActionMap(
        UiAction _firstAction,
        UiAction lastAction,
        params (UiAction Action, int Index)[] _aliases
    )
    {
        int span = (int)lastAction - (int)_firstAction;
        if (span < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lastAction),
                "Last action must be greater than or equal to first action."
            );
        }

        this._firstAction = _firstAction;
        _count = span + 1;
        this._aliases = _aliases;

        foreach ((UiAction Action, int Index) alias in _aliases)
        {
            if ((uint)alias.Index >= (uint)_count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(_aliases),
                    $"Alias index {alias.Index} is out of range."
                );
            }
        }
    }

    public bool TryGetIndex(UiAction action, out int index)
    {
        int contiguousIndex = (int)action - (int)_firstAction;
        if ((uint)contiguousIndex < (uint)_count)
        {
            index = contiguousIndex;
            return true;
        }

        foreach ((UiAction Action, int Index) alias in _aliases)
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
        if ((uint)index >= (uint)_count)
        {
            action = UiAction.None;
            return false;
        }

        action = (UiAction)((int)_firstAction + index);
        return true;
    }

    public bool Contains(UiAction action) => TryGetIndex(action, out _);
}
