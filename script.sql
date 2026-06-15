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
CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Icon] nvarchar(25) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);

CREATE TABLE [DepenseMois] (
    [Id] int NOT NULL IDENTITY,
    [Montant] int NOT NULL,
    [Mois] int NOT NULL,
    [Annee] int NOT NULL,
    [TransactionType] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_DepenseMois] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(30) NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [LastLoginAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Transactions] (
    [Id] int NOT NULL IDENTITY,
    [Intitule] nvarchar(150) NOT NULL,
    [Montant] decimal(18,2) NOT NULL,
    [CategorieId] int NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [TransactionTable] nvarchar(13) NOT NULL,
    [Frequence] int NULL,
    [EstDomiciliee] bit NULL,
    [ReminderDaysBefore] int NULL,
    [DateFin] datetime2 NULL,
    [Date] datetime2 NULL,
    [TransactionType] int NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transactions_Categories_CategorieId] FOREIGN KEY ([CategorieId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RefreshTokens] (
    [Id] int NOT NULL IDENTITY,
    [Token] nvarchar(max) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [DepenseDueDates] (
    [Id] int NOT NULL IDENTITY,
    [Date] datetime2 NOT NULL,
    [MontantEffectif] decimal(18,2) NULL,
    [DepenseId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_DepenseDueDates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DepenseDueDates_Transactions_DepenseId] FOREIGN KEY ([DepenseId]) REFERENCES [Transactions] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Rappels] (
    [Id] int NOT NULL IDENTITY,
    [DepenseFixeId] int NOT NULL,
    [RappelDate] datetime2 NOT NULL,
    [Vu] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Rappels] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Rappels_Transactions_DepenseFixeId] FOREIGN KEY ([DepenseFixeId]) REFERENCES [Transactions] ([Id]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Icon', N'Name') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([Id], [Icon], [Name])
VALUES (1, N'', N'NoCategory');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Icon', N'Name') AND [object_id] = OBJECT_ID(N'[Categories]'))
    SET IDENTITY_INSERT [Categories] OFF;

CREATE INDEX [IX_DepenseDueDates_DepenseId] ON [DepenseDueDates] ([DepenseId]);

CREATE INDEX [IX_Rappels_DepenseFixeId] ON [Rappels] ([DepenseFixeId]);

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);

CREATE INDEX [IX_Transactions_CategorieId] ON [Transactions] ([CategorieId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260122081013_InitialAfterAddUser', N'10.0.1');

COMMIT;
GO

