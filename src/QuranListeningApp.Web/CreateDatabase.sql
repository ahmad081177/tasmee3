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
GO

CREATE TABLE [SurahReferences] (
    [SurahNumber] int NOT NULL,
    [SurahNameArabic] nvarchar(100) NOT NULL,
    [SurahNameEnglish] nvarchar(100) NULL,
    [TotalAyahs] int NOT NULL,
    [IsMakki] bit NOT NULL,
    CONSTRAINT [PK_SurahReferences] PRIMARY KEY ([SurahNumber])
);
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [FullNameArabic] nvarchar(200) NOT NULL,
    [IdNumber] nvarchar(20) NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [Email] nvarchar(100) NULL,
    [Role] int NOT NULL,
    [GradeLevel] nvarchar(50) NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    [CreatedByUserId] uniqueidentifier NULL,
    [LastLoginDate] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [AuditLogs] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Action] int NOT NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [EntityId] uniqueidentifier NOT NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [Timestamp] datetime2 NOT NULL,
    [IpAddress] nvarchar(45) NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ListeningSessions] (
    [Id] uniqueidentifier NOT NULL,
    [StudentUserId] uniqueidentifier NOT NULL,
    [TeacherUserId] uniqueidentifier NOT NULL,
    [SessionDate] datetime2 NOT NULL,
    [FromSurahNumber] int NOT NULL,
    [FromAyahNumber] int NOT NULL,
    [ToSurahNumber] int NOT NULL,
    [ToAyahNumber] int NOT NULL,
    [MajorErrorsCount] int NOT NULL DEFAULT 0,
    [MinorErrorsCount] int NOT NULL DEFAULT 0,
    [IsCompleted] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Notes] nvarchar(max) NULL,
    [CreatedDate] datetime2 NOT NULL,
    [ModifiedDate] datetime2 NULL,
    CONSTRAINT [PK_ListeningSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ListeningSessions_Users_StudentUserId] FOREIGN KEY ([StudentUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ListeningSessions_Users_TeacherUserId] FOREIGN KEY ([TeacherUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AuditLogs_EntityType_EntityId] ON [AuditLogs] ([EntityType], [EntityId]);
GO

CREATE INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs] ([Timestamp]);
GO

CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
GO

CREATE INDEX [IX_ListeningSessions_IsCompleted] ON [ListeningSessions] ([IsCompleted]);
GO

CREATE INDEX [IX_ListeningSessions_SessionDate] ON [ListeningSessions] ([SessionDate]);
GO

CREATE INDEX [IX_ListeningSessions_StudentUserId] ON [ListeningSessions] ([StudentUserId]);
GO

CREATE INDEX [IX_ListeningSessions_TeacherUserId] ON [ListeningSessions] ([TeacherUserId]);
GO

CREATE INDEX [IX_Users_CreatedByUserId] ON [Users] ([CreatedByUserId]);
GO

CREATE INDEX [IX_Users_FullNameArabic] ON [Users] ([FullNameArabic]);
GO

CREATE UNIQUE INDEX [IX_Users_IdNumber] ON [Users] ([IdNumber]) WHERE [IdNumber] IS NOT NULL;
GO

CREATE INDEX [IX_Users_PhoneNumber] ON [Users] ([PhoneNumber]);
GO

CREATE INDEX [IX_Users_Role] ON [Users] ([Role]);
GO

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251020175929_InitialCreate', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ListeningSessions]') AND [c].[name] = N'FromSurahNumber');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ListeningSessions] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [ListeningSessions] DROP COLUMN [FromSurahNumber];
GO

EXEC sp_rename N'[ListeningSessions].[ToSurahNumber]', N'SurahNumber', N'COLUMN';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251020194313_ChangedToSingleSurahPerSession', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ListeningSessions] ADD [Grade] decimal(5,2) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251023100831_AddGradeToListeningSession', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AppSettings] (
    [Id] int NOT NULL,
    [SchoolNameArabic] nvarchar(200) NOT NULL,
    [SchoolLogoPath] nvarchar(500) NULL,
    [ModifiedDate] datetime2 NULL,
    [ModifiedByUserId] uniqueidentifier NULL,
    CONSTRAINT [PK_AppSettings] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251023102125_AddAppSettings', N'8.0.10');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [PledgeAcceptedDate] datetime2 NULL;
GO

ALTER TABLE [AppSettings] ADD [PledgeText] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251023102523_AddPledgeSupport', N'8.0.10');
GO

COMMIT;
GO

