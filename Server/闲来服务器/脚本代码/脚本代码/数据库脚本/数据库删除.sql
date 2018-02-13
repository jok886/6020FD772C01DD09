
USE master
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'QPAccountsDB_HideSeek')
DROP DATABASE [QPAccountsDB_HideSeek]
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'QPExerciseDB_HideSeek')
DROP DATABASE [QPExerciseDB_HideSeek]
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'QPGameMatchDB_HideSeek')
DROP DATABASE [QPGameMatchDB_HideSeek]
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'QPGameScoreDB_HideSeek')
DROP DATABASE [QPGameScoreDB_HideSeek]
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'QPPlatformDB_HideSeek')
DROP DATABASE [QPPlatformDB_HideSeek]
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'QPRecordDB_HideSeek')
DROP DATABASE [QPRecordDB_HideSeek]
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'QPTreasureDB_HideSeek')
DROP DATABASE [QPTreasureDB_HideSeek]
GO


