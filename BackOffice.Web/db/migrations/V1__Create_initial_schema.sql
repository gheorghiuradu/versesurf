CREATE TABLE IF NOT EXISTS playlists (
    "Id" TEXT PRIMARY KEY,
    "SpotifyId" TEXT,
    "Name" TEXT NOT NULL,
    "Enabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "Featured" BOOLEAN NOT NULL DEFAULT FALSE,
    "PictureUrl" TEXT,
    "Votes" INTEGER NOT NULL DEFAULT 0,
    "Language" TEXT,
    "Plays" INTEGER NOT NULL DEFAULT 0,
    "AddedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS songs (
    "Id" TEXT PRIMARY KEY,
    "PlaylistId" TEXT REFERENCES playlists("Id"),
    "SpotifyId" TEXT,
    "ISRC" TEXT,
    "Artist" TEXT,
    "Title" TEXT,
    "IsExplicit" BOOLEAN NOT NULL DEFAULT FALSE,
    "Snippet" TEXT,
    "Plays" INTEGER NOT NULL DEFAULT 0,
    "Enabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "FullAudioUrl" TEXT,
    "PreviewUrl" TEXT,
    "BmiLicenseId" TEXT,
    "ASCAPLicenseId" TEXT,
    "SesacLicenseId" TEXT,
    "StartSecond" REAL,
    "EndSecond" REAL,
    "AddedAt" TIMESTAMP NOT NULL,
    "ModifiedAt" TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS events (
    "Id" SERIAL PRIMARY KEY,
    "Sender" TEXT,
    "TimeStamp" TIMESTAMP NOT NULL,
    "EventType" TEXT,
    "PayloadJson" TEXT,
    "PayloadName" TEXT,
    "PayloadType" TEXT
);
