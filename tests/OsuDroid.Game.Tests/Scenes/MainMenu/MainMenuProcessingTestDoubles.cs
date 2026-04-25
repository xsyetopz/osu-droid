using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Import;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    private sealed class FakeBeatmapProcessingService : IBeatmapProcessingService
    {
        public BeatmapProcessingState State { get; private set; } = new(true, 35, "Processing beatmaps...");

        public int StartCalls { get; private set; }

        private BeatmapLibrarySnapshot? _completedSnapshot;

        public bool HasPendingWork() => _completedSnapshot is null;

        public void EnqueueArchive(string archivePath, BeatmapOnlineMetadata? metadata = null)
        {
        }

        public void Start() => StartCalls++;

        public bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot _snapshot)
        {
            if (_completedSnapshot is null)
            {
                _snapshot = BeatmapLibrarySnapshot.Empty;
                return false;
            }

            _snapshot = _completedSnapshot;
            _completedSnapshot = null;
            State = new BeatmapProcessingState();
            return true;
        }

        public void Complete()
        {
            _completedSnapshot = BeatmapLibrarySnapshot.Empty;
            State = new BeatmapProcessingState(false, 100, "Processing beatmaps...");
        }
    }

    private sealed class NoPendingBeatmapProcessingService : IBeatmapProcessingService
    {
        public BeatmapProcessingState State { get; } = new();

        public bool HasPendingWork() => false;

        public void EnqueueArchive(string archivePath, BeatmapOnlineMetadata? metadata = null)
        {
        }

        public void Start()
        {
        }

        public bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot snapshot)
        {
            snapshot = BeatmapLibrarySnapshot.Empty;
            return false;
        }
    }
}
