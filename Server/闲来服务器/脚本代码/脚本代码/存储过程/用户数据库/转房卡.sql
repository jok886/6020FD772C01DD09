
----------------------------------------------------------------------------------------------------

USE QPAccountsDB_HideSeek
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GR_QueryNickName]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GR_QueryNickName]
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GR_TransferDiamonds]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GR_TransferDiamonds]
GO

SET QUOTED_IDENTIFIER ON 
GO

SET ANSI_NULLS ON 
GO



----------------------------------------------------------------------------------------------------
-- 查询昵称
CREATE PROC GSP_GR_QueryNickName
	@dwUserID INT,								-- 用户 I D
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

	DECLARE @NickNameToQuery NVARCHAR(31)
	set @NickNameToQuery = N'未知用户'
	-- 查询用户
	SET @NickNameToQuery = (SELECT NickName  FROM AccountsInfo WHERE UserID=@dwUserID)
	-- @NickNameToQuery=NickName  FROM AccountsInfo WHERE UserID=@dwUserID
	IF @NickNameToQuery IS NULL
	BEGIN
		SET @strErrorDescribe = N'抱歉，您输入的用户ID不存在！请重新输入正确的ID!'
		return 1
	END
	
	
	SET @strErrorDescribe = N'查询昵称成功!'
	-- 抛出数据
	SELECT @NickNameToQuery AS NickName		
END

RETURN 0

GO

-----------------------------------------------------------------
--转房卡----
CREATE PROC GSP_GR_TransferDiamonds
	@dwLocalID INT,								-- 转出房卡用户ID
	@dwUserID INT,								-- 转入房卡用户ID
	@dwDiamondNum INT,							-- 转移房卡数量（钻石）
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

	DECLARE @InsureScoreOut INT 
	DECLARE @InsureScoreIn INT 
	Set @InsureScoreOut = 0
	set @InsureScoreIn = 0
	-- 查询用户
	SET @InsureScoreOut = (SELECT InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID=@dwLocalID)
	--SELECT @InsureScoreOut=InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID=@dwLocalID
	IF @InsureScoreOut IS NULL
	BEGIN
		SET @strErrorDescribe = N'抱歉，转出房卡的用户ID不存在！转房卡失败！'
		return 1
	END

	-- 查询用户
	SET @InsureScoreIn = (SELECT InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID=@dwUserID)
	--SELECT @InsureScoreIn=InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID=@dwUserID
	IF @InsureScoreIn IS NULL
	BEGIN
		SET @strErrorDescribe = N'抱歉，转入房卡的用户ID不存在！转房卡失败！'
		return 1
	END
	
	IF @dwDiamondNum > @InsureScoreOut
	BEGIN
		SET @strErrorDescribe = N'没有足够的房卡可以转给他人！转房卡失败！'
		RETURN 1
	END

	--更新两人的数据库
	UPDATE 	QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo SET InsureScore=InsureScore-@dwDiamondNum WHERE UserID=@dwLocalID
	UPDATE 	QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo SET InsureScore=InsureScore+@dwDiamondNum WHERE UserID=@dwUserID
	
	--记录转房卡信息
	DECLARE @NickNameFrom NVARCHAR(31)
	DECLARE @NickNameTo NVARCHAR(31)
	set @NickNameFrom = N'未知用户'
	set @NickNameTo = N'未知用户'
	-- 查询用户
	SET @NickNameFrom = (SELECT NickName  FROM AccountsInfo WHERE UserID=@dwLocalID)
	SET @NickNameTo = (SELECT NickName  FROM AccountsInfo WHERE UserID=@dwUserID)
	INSERT INTO QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordTransferDiamond VALUES(@dwLocalID, @NickNameFrom , @dwUserID, @NickNameTo, @dwDiamondNum, getdate())
	SET @strErrorDescribe = N'转房卡操作成功！'

END


RETURN 0

GO