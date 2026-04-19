namespace OsuDroid.Game.Compatibility.Database;

public static class DroidDatabaseSchema
{
    public static IReadOnlyList<string> CreateStatements { get; } = new[]
    {
        """
        CREATE TABLE IF NOT EXISTS BeatmapInfo (
            filename TEXT NOT NULL,
            setDirectory TEXT NOT NULL,
            md5 TEXT NOT NULL,
            id INTEGER,
            audioFilename TEXT NOT NULL,
            backgroundFilename TEXT,
            status INTEGER,
            setId INTEGER,
            title TEXT NOT NULL,
            titleUnicode TEXT NOT NULL,
            artist TEXT NOT NULL,
            artistUnicode TEXT NOT NULL,
            creator TEXT NOT NULL,
            version TEXT NOT NULL,
            tags TEXT NOT NULL,
            source TEXT NOT NULL DEFAULT '',
            length INTEGER NOT NULL DEFAULT 0,
            bpmMin REAL NOT NULL DEFAULT 0,
            bpmMax REAL NOT NULL DEFAULT 0,
            circleSize REAL NOT NULL DEFAULT 0,
            approachRate REAL NOT NULL DEFAULT 0,
            overallDifficulty REAL NOT NULL DEFAULT 0,
            hpDrainRate REAL NOT NULL DEFAULT 0,
            sliderMultiplier REAL NOT NULL DEFAULT 0,
            sliderTickRate REAL NOT NULL DEFAULT 0,
            starRating REAL,
            localOffset INTEGER NOT NULL DEFAULT 0,
            dateAdded INTEGER NOT NULL DEFAULT 0,
            lastModified INTEGER NOT NULL DEFAULT 0,
            epilepsyWarning INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY(filename, setDirectory)
        )
        """,
        "CREATE INDEX IF NOT EXISTS filenameIdx ON BeatmapInfo(filename)",
        "CREATE INDEX IF NOT EXISTS setDirectoryIdx ON BeatmapInfo(setDirectory)",
        "CREATE INDEX IF NOT EXISTS setIdx ON BeatmapInfo(setDirectory, setId)",
        """
        CREATE TABLE IF NOT EXISTS BeatmapOptions (
            setDirectory TEXT NOT NULL PRIMARY KEY,
            isFavorite INTEGER NOT NULL DEFAULT 0,
            offset INTEGER NOT NULL DEFAULT 0
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS ScoreInfo (
            id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            beatmapMD5 TEXT NOT NULL,
            playerName TEXT NOT NULL,
            replayFilename TEXT NOT NULL,
            mods TEXT NOT NULL,
            score INTEGER NOT NULL,
            maxCombo INTEGER NOT NULL,
            mark TEXT NOT NULL,
            hit300k INTEGER NOT NULL,
            hit300 INTEGER NOT NULL,
            hit100k INTEGER NOT NULL,
            hit100 INTEGER NOT NULL,
            hit50 INTEGER NOT NULL,
            misses INTEGER NOT NULL,
            time INTEGER NOT NULL,
            sliderHeadHits INTEGER,
            sliderTickHits INTEGER,
            sliderRepeatHits INTEGER,
            sliderEndHits INTEGER
        )
        """,
        "CREATE INDEX IF NOT EXISTS beatmapIdx ON ScoreInfo(beatmapMD5)",
        "CREATE TABLE IF NOT EXISTS BeatmapSetCollection (name TEXT NOT NULL PRIMARY KEY)",
        """
        CREATE TABLE IF NOT EXISTS BeatmapSetCollection_BeatmapSetInfo (
            collectionName TEXT NOT NULL,
            setDirectory TEXT NOT NULL,
            PRIMARY KEY(collectionName, setDirectory)
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS BlockArea (
            id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            x REAL NOT NULL DEFAULT 0,
            y REAL NOT NULL DEFAULT 0,
            width REAL NOT NULL DEFAULT 0,
            height REAL NOT NULL DEFAULT 0
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS ModPreset (
            id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            serializedMods TEXT NOT NULL
        )
        """,
        $"PRAGMA user_version = {DroidDatabaseConstants.CurrentVersion}",
    };

    public static IReadOnlyList<string> RequiredTables { get; } = new[]
    {
        "BeatmapInfo",
        "BeatmapOptions",
        "ScoreInfo",
        "BeatmapSetCollection",
        "BeatmapSetCollection_BeatmapSetInfo",
        "BlockArea",
        "ModPreset",
    };
}
