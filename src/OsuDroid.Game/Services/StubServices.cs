using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OsuDroid.Game.Localisation;

namespace OsuDroid.Game;

public sealed class StubAuthState
{
    public SessionSnapshot Session { get; set; } = SessionSnapshot.Guest();
    public AccountProfile Account { get; set; } = AccountProfile.Guest();
}

public sealed class StubSessionService(StubAuthState state) : ISessionService
{
    public SessionSnapshot Current => state.Session;

    public Task<SessionSnapshot> RestoreAsync(CancellationToken cancellationToken = default) => Task.FromResult(state.Session);

    public Task<SessionSnapshot> SignInAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var trimmed = string.IsNullOrWhiteSpace(username) ? "Guest" : username.Trim();

        state.Session = new SessionSnapshot(false, true, trimmed, trimmed);
        state.Account = new AccountProfile(trimmed, null, LoginPanelStrings.SignedIn.ToString());
        return Task.FromResult(state.Session);
    }

    public void SignOut()
    {
        state.Session = SessionSnapshot.Guest();
        state.Account = AccountProfile.Guest();
    }
}

public sealed class StubAccountService(StubAuthState state) : IAccountService
{
    public AccountProfile Current => state.Account;

    public Task<AccountProfile> RefreshAsync(CancellationToken cancellationToken = default) => Task.FromResult(state.Account);
}

public sealed class StubBeatmapLibraryService : IBeatmapLibraryService
{
    private static readonly IReadOnlyList<BeatmapCard> localBeatmaps =
    [
        new("31589", "Demetori", "Desire Drive", "Extra Stage", "Sotarks"),
        new("1972514", "Camellia", "Exit This Earth's Atomosphere", "Event Horizon", "Lasse"),
        new("1627144", "xi", "Blue Zenith", "Blue Another", "Asphyxia"),
        new("1644215", "UNDEAD CORPORATION", "Everything will freeze", "Time Freeze", "Ekoro"),
        new("906279", "DJ TOTTO", "Crystalia", "Insane", "Monstrata"),
        new("819257", "Halozy", "Genryuu Kaiko", "Lunatic", "Mismagius")
    ];

    private static readonly IReadOnlyList<BeatmapCard> onlineBeatmaps =
    [
        new("3438442", "Feryquitous", "Paved Garden", "Afar", "Ryuusei Aika"),
        new("3289120", "seatrus", "ILLEGAL LEGACY", "Anathema", "Sytho"),
        new("2672446", "Diao ye zong", "Fragments", "Amethyst", "Miraie"),
        new("2914287", "Aitsuki Nakuru", "Monochrome Butterfly", "Promise", "Luscent"),
        new("1984218", "Camellia", "Tojita Sekai", "Collapse", "Mirash"),
        new("2460213", "Memme", "Chinese Restaurant", "Overdose", "Nevo"),
        new("3350617", "Silentroom", "Nhelv", "Hyperspace", "Sotarks"),
        new("3455027", "Kobaryo", "Villain Virus", "Terminal", "Ryuusei Aika")
    ];

    public IReadOnlyList<BeatmapCard> LocalBeatmaps => localBeatmaps;

    public Task<IReadOnlyList<BeatmapCard>> RefreshLocalAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(localBeatmaps);

    public Task<IReadOnlyList<BeatmapCard>> SearchOnlineAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(onlineBeatmaps);

        var trimmed = query.Trim();
        var filtered = onlineBeatmaps.Where(card =>
                card.Artist.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
                || card.Title.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
                || card.DifficultyName.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
                || card.Mapper.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return Task.FromResult<IReadOnlyList<BeatmapCard>>(filtered);
    }
}
