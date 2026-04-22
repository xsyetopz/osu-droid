using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class MainMenuScene
{
    private void UpdateSpectrumState(double elapsedMilliseconds)
    {
        const float gradient = 20f;
        const float initialAlpha = 0.4f;

        if (!nowPlaying.IsPlaying)
        {
            for (var i = 0; i < SpectrumBarCount; i++)
            {
                spectrumPeakLevel[i] = 0f;
                spectrumPeakAlpha[i] = 0f;
            }

            return;
        }

        if (!hasRawSpectrum)
        {
            for (var i = 0; i < SpectrumBarCount; i++)
            {
                spectrumPeakLevel[i] = 0f;
                spectrumPeakAlpha[i] = 0f;
            }

            return;
        }

        const int windowSize = 240;
        var leftBound = 0;
        for (var i = 0; i < SpectrumBarCount; i++)
        {
            var rightBound = (int)Math.Pow(2d, i * 9d / (windowSize - 1));
            if (rightBound <= leftBound)
                rightBound = leftBound + 1;
            rightBound = Math.Clamp(rightBound, 0, rawSpectrum.Length - 2);

            var peak = 0f;
            while (leftBound < rightBound)
            {
                peak = MathF.Max(peak, rawSpectrum[1 + leftBound]);
                leftBound++;
            }

            leftBound = rightBound;

            var currentPeak = peak * 500f;

            if (currentPeak > spectrumPeakLevel[i])
            {
                spectrumPeakLevel[i] = currentPeak;
                spectrumPeakDownRate[i] = spectrumPeakLevel[i] / gradient;
                spectrumPeakAlpha[i] = initialAlpha;
            }
            else
            {
                spectrumPeakLevel[i] = MathF.Max(spectrumPeakLevel[i] - spectrumPeakDownRate[i], 0f);
                spectrumPeakAlpha[i] = MathF.Max(spectrumPeakAlpha[i] - initialAlpha / gradient, 0f);
            }
        }
    }
}
