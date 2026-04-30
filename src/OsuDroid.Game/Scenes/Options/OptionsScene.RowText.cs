using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private static IEnumerable<string> WrapText(string text, float size, float width)
    {
        int maxCharacters = Math.Max(8, (int)MathF.Floor(width / (size * 0.48f)));
        foreach (
            string paragraph in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n')
        )
        {
            if (paragraph.Length == 0)
            {
                yield return string.Empty;
                continue;
            }

            var line = new System.Text.StringBuilder();
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                int nextLength = line.Length == 0 ? word.Length : line.Length + 1 + word.Length;
                if (line.Length > 0 && nextLength > maxCharacters)
                {
                    yield return line.ToString();
                    line.Clear();
                }

                if (line.Length > 0)
                {
                    line.Append(' ');
                }

                line.Append(word);
            }

            if (line.Length > 0)
            {
                yield return line.ToString();
            }
        }
    }

    private static void AddWrappedSummary(
        List<UiElementSnapshot> elements,
        string id,
        string summary,
        float x,
        float y,
        float width,
        float alpha,
        UiAction action,
        bool enabled
    )
    {
        int elementIndex = 0;
        float lineY = y;
        foreach (string line in WrapText(summary, RowSummarySize, width))
        {
            if (line.Length == 0)
            {
                lineY += SummaryLineHeight;
                continue;
            }

            string lineId = elementIndex == 0 ? id : $"{id}-{elementIndex}";
            elements.Add(
                Text(
                    lineId,
                    line,
                    x,
                    lineY,
                    width,
                    SummaryLineHeight,
                    RowSummarySize,
                    s_secondaryText,
                    alpha,
                    false,
                    action,
                    enabled,
                    clipToBounds: true
                )
            );
            lineY += SummaryLineHeight;
            elementIndex++;
        }
    }
}
