IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Podcasts] (
    [PodcastID] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatorID] nvarchar(max) NULL,
    [CreatedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Podcasts] PRIMARY KEY ([PodcastID])
);

CREATE TABLE [Users] (
    [UserID] nvarchar(450) NOT NULL,
    [UserName] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [JoinDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserID])
);

CREATE TABLE [Episodes] (
    [EpisodeID] int NOT NULL IDENTITY,
    [PodcastID] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [ReleaseDate] datetime2 NOT NULL,
    [Duration] int NOT NULL,
    [PlayCount] int NOT NULL,
    [AudioFileUrl] nvarchar(max) NULL,
    [Host] nvarchar(max) NULL,
    [Topic] nvarchar(max) NULL,
    CONSTRAINT [PK_Episodes] PRIMARY KEY ([EpisodeID]),
    CONSTRAINT [FK_Episodes_Podcasts_PodcastID] FOREIGN KEY ([PodcastID]) REFERENCES [Podcasts] ([PodcastID]) ON DELETE CASCADE
);

CREATE TABLE [Subscriptions] (
    [SubscriptionID] int NOT NULL IDENTITY,
    [UserID] nvarchar(450) NOT NULL,
    [PodcastID] int NOT NULL,
    [SubscribedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Subscriptions] PRIMARY KEY ([SubscriptionID]),
    CONSTRAINT [FK_Subscriptions_Podcasts_PodcastID] FOREIGN KEY ([PodcastID]) REFERENCES [Podcasts] ([PodcastID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Subscriptions_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);

CREATE INDEX [IX_Episodes_PodcastID] ON [Episodes] ([PodcastID]);

CREATE INDEX [IX_Subscriptions_PodcastID] ON [Subscriptions] ([PodcastID]);

CREATE INDEX [IX_Subscriptions_UserID] ON [Subscriptions] ([UserID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251024235802_InitAppDb', N'9.0.10');

COMMIT;
GO

