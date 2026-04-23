namespace OsuDroid.Game.Scenes.MainMenu;

public sealed partial class MainMenuScene
{
    private void UpdateSpectrumState()
    {
        const float gradient = 20f;
        const float initialAlpha = 0.4f;

        if (!_nowPlaying.IsPlaying)
        {
            for (int i = 0; i < SpectrumBarCount; i++)
            {
                _spectrumPeakLevel[i] = 0f;
                _spectrumPeakAlpha[i] = 0f;
            }

            return;
        }

        if (!_hasRawSpectrum)
        {
            for (int i = 0; i < SpectrumBarCount; i++)
            {
                _spectrumPeakLevel[i] = 0f;
                _spectrumPeakAlpha[i] = 0f;
            }

            return;
        }

        const int windowSize = 240;
        int leftBound = 0;
        for (int i = 0; i < SpectrumBarCount; i++)
        {
            int rightBound = (int)Math.Pow(2d, i * 9d / (windowSize - 1));
            if (rightBound <= leftBound)
            {
                rightBound = leftBound + 1;
            }

            rightBound = Math.Clamp(rightBound, 0, _rawSpectrum.Length - 2);

            float peak = 0f;
            while (leftBound < rightBound)
            {
                peak = MathF.Max(peak, _rawSpectrum[1 + leftBound]);
                leftBound++;
            }

            leftBound = rightBound;

            float currentPeak = peak * 500f;

            if (currentPeak > _spectrumPeakLevel[i])
            {
                _spectrumPeakLevel[i] = currentPeak;
                _spectrumPeakDownRate[i] = _spectrumPeakLevel[i] / gradient;
                _spectrumPeakAlpha[i] = initialAlpha;
            }
            else
            {
                _spectrumPeakLevel[i] = MathF.Max(_spectrumPeakLevel[i] - _spectrumPeakDownRate[i], 0f);
                _spectrumPeakAlpha[i] = MathF.Max(_spectrumPeakAlpha[i] - initialAlpha / gradient, 0f);
            }
        }
    }
}
