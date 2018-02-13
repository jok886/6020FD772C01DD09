
USE master
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'QPNativeWebDB_HideSeek')
DROP DATABASE [QPNativeWebDB_HideSeek]
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'QPPlatformManagerDB_HideSeek')
DROP DATABASE [QPPlatformManagerDB_HideSeek]
GO



GO
