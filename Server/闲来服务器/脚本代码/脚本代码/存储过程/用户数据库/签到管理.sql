
----------------------------------------------------------------------------------------------------

USE QPAccountsDB_HideSeek
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GR_CheckInQueryInfo]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GR_CheckInQueryInfo]
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GR_CheckAward]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GR_CheckAward]
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GR_CheckInDone]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GR_CheckInDone]
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GP_LoadCheckInReward]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GP_LoadCheckInReward]
GO


SET QUOTED_IDENTIFIER ON 
GO

SET ANSI_NULLS ON 
GO


----------------------------------------------------------------------------------------------------

-- 加载奖励
CREATE PROC GSP_GP_LoadCheckInReward
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

	-- 查询奖励
	SELECT * FROM QPPlatformDB_HideSeekLink.QPPlatformDB_HideSeek.dbo.SigninConfig	

END

RETURN 0

GO
----------------------------------------------------------------------------------------------------
-- 查询签到
CREATE PROC GSP_GR_CheckInQueryInfo
	@dwUserID INT,								-- 用户 I D
	@strPassword NCHAR(32),						-- 用户密码
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

	-- 查询用户
	IF not exists(SELECT * FROM AccountsInfo WHERE UserID=@dwUserID AND LogonPass=@strPassword)
	BEGIN
		SET @strErrorDescribe = N'抱歉，你的用户信息不存在或者密码不正确！'
		return 1
	END

	-- 签到记录
	DECLARE @wSeriesDate INT	
	DECLARE @wAwardDate INT	
	DECLARE @StartDateTime DateTime
	DECLARE @LastDateTime DateTime
	SELECT @StartDateTime=StartDateTime,@LastDateTime=LastDateTime,@wSeriesDate=SeriesDate,@wAwardDate=AwardDate FROM AccountsSignin 	
	WHERE UserID=@dwUserID
	IF @StartDateTime IS NULL OR @LastDateTime IS NULL OR @wSeriesDate IS NULL OR @wSeriesDate IS NULL OR @wAwardDate IS NULL
	BEGIN
		SELECT @StartDateTime=GetDate(),@LastDateTime=GetDate(),@wSeriesDate=0
		INSERT INTO AccountsSignin VALUES(@dwUserID,@StartDateTime,@LastDateTime,0,0)		
	END

	-- 日期判断
	DECLARE @TodayCheckIned TINYINT
	SET @TodayCheckIned = 0
	IF DateDiff(dd,@LastDateTime,GetDate()) = 0 	
	BEGIN
		IF @wSeriesDate > 0 SET @TodayCheckIned = 1
	END ELSE
	BEGIN	
		DECLARE @iMaxDay INT
		SELECT @iMaxDay =  MAX(RewardDay) FROM QPPlatformDB_HideSeekLink.QPPlatformDB_HideSeek.dbo.SigninConfig
		
		if @wAwardDate = @iMaxDay OR @wAwardDate > @iMaxDay
		BEGIN
			SELECT @StartDateTime = GetDate(),@LastDateTime = GetDate(),@wSeriesDate = 0,@wAwardDate = 0
			--mChen edit
			--INSERT INTO AccountsSignin VALUES(@dwUserID,@StartDateTime,@LastDateTime,0,0)
			UPDATE AccountsSignin SET StartDateTime=GetDate(),LastDateTime=GetDate(),SeriesDate=0,AwardDate=0 WHERE UserID=@dwUserID			
		END	
		--IF DateDiff(dd,@StartDateTime,GetDate()) <> @wSeriesDate
		--BEGIN
		--	SET @wSeriesDate = 0
		--	SET @wAwardDate = 0
		--	UPDATE AccountsSignin SET StartDateTime=GetDate(),LastDateTime=GetDate(),SeriesDate=0,AwardDate=0 WHERE UserID=@dwUserID									
		--END
	END

	
	
	-- 抛出数据
	SELECT @wSeriesDate AS SeriesDate,@wAwardDate AS AwardDate,@TodayCheckIned AS TodayCheckIned	
END

RETURN 0

GO

----------------------------------------------------------------------------------------------------

-- 领取物品
CREATE PROC GSP_GR_CheckAward
	@dwUserID INT,								-- 用户 I D
	@strPassword NCHAR(32),						-- 用户密码
	@strClientIP NVARCHAR(15),					-- 连接地址
	@strMachineID NVARCHAR(32),					-- 机器标识
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

	-- 查询用户
	IF not exists(SELECT * FROM AccountsInfo WHERE UserID=@dwUserID AND LogonPass=@strPassword)
	BEGIN
		SET @strErrorDescribe = N'抱歉，你的用户信息不存在或者密码不正确！'
		return 1
	END

	-- 签到记录
	DECLARE @wSeriesDate INT	
	DECLARE @wAwardDate INT	
	DECLARE @StartDateTime DateTime
	DECLARE @LastDateTime DateTime
	SELECT @StartDateTime=StartDateTime,@LastDateTime=LastDateTime,@wSeriesDate=SeriesDate,@wAwardDate=AwardDate  FROM AccountsSignin 
	WHERE UserID=@dwUserID
	IF @StartDateTime IS NULL OR @LastDateTime IS NULL OR @wSeriesDate IS NULL
	BEGIN
		SELECT @StartDateTime = GetDate(),@LastDateTime = GetDate(),@wSeriesDate = 0,@wAwardDate = 0
		INSERT INTO AccountsSignin VALUES(@dwUserID,@StartDateTime,@LastDateTime,0,0)		
	END

	-- 签到判断
	IF @wAwardDate = @wSeriesDate AND @wAwardDate > 0
	BEGIN
		SET @strErrorDescribe = N'抱歉，您今天已经领取签到奖励了！'
		return 3		
	END
	
	-- 查询奖励
	DECLARE @lRewardGold BIGINT
	DECLARE @lRewardType BIGINT
	DECLARE @lRewardDay BIGINT
	SELECT @lRewardGold=RewardGold,@lRewardType=RewardType,@lRewardDay=RewardDay FROM QPPlatformDB_HideSeekLink.QPPlatformDB_HideSeek.dbo.SigninConfig WHERE DayID=(@wAwardDate+1)
	IF @lRewardGold IS NULL 
	BEGIN
		SELECT @StartDateTime = GetDate(),@LastDateTime = GetDate(),@wSeriesDate = 0,@wAwardDate = 0

		--mChen edit
		--INSERT INTO AccountsSignin VALUES(@dwUserID,@StartDateTime,@LastDateTime,0,0)
		UPDATE AccountsSignin SET StartDateTime=GetDate(),LastDateTime=GetDate(),SeriesDate=0,AwardDate=0 WHERE UserID=@dwUserID	
			
		SET @strErrorDescribe = N'抱歉，您还不能领取这个奖励！'
		return 3		
	END	
	if @lRewardDay > @wSeriesDate
	BEGIN
		SET @strErrorDescribe = N'抱歉，您还不能领取这个奖励！'
		return 3		
	END	
	
	-- 更新记录
	SET @wAwardDate = @wAwardDate+1
	UPDATE AccountsSignin SET AwardDate = @wAwardDate WHERE UserID = @dwUserID
	
	IF @lRewardType = 1
	BEGIN
		-- 奖励钻石	
		UPDATE QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo SET InsureScore = InsureScore + @lRewardGold WHERE UserID = @dwUserID
		IF @@rowcount = 0
		BEGIN
			-- 插入资料
			INSERT INTO QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo (UserID,InsureScore, LastLogonIP, LastLogonMachine, RegisterIP, RegisterMachine)
			VALUES (@dwUserID, @lRewardGold, @strClientIP, @strMachineID, @strClientIP, @strMachineID)
		END
		
		SET @strErrorDescribe = N'签到成功！获得 '+convert(varchar,@lRewardGold)+N' 颗钻石！'
	END	


	-- 查询钻石	
	DECLARE @lScore BIGINT	
	SELECT @lScore=InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID = @dwUserID 	
	IF @lScore IS NULL SET @lScore = 0
	
		
	-- 抛出数据
	SELECT @lScore AS Score	
END
RETURN 0

GO
----------------------------------------------------------------------------------------------------

-- 领取奖励
CREATE PROC GSP_GR_CheckInDone
	@dwUserID INT,								-- 用户 I D
	@strPassword NCHAR(32),						-- 用户密码
	@strClientIP NVARCHAR(15),					-- 连接地址
	@strMachineID NVARCHAR(32),					-- 机器标识
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

	-- 查询用户
	IF not exists(SELECT * FROM AccountsInfo WHERE UserID=@dwUserID AND LogonPass=@strPassword)
	BEGIN
		SET @strErrorDescribe = N'抱歉，你的用户信息不存在或者密码不正确！'
		return 1
	END

	-- 签到记录
	DECLARE @wSeriesDate INT	
	DECLARE @StartDateTime DateTime
	DECLARE @LastDateTime DateTime
	SELECT @StartDateTime=StartDateTime,@LastDateTime=LastDateTime,@wSeriesDate=SeriesDate FROM AccountsSignin 
	WHERE UserID=@dwUserID
	IF @StartDateTime IS NULL OR @LastDateTime IS NULL OR @wSeriesDate IS NULL
	BEGIN
		SELECT @StartDateTime = GetDate(),@LastDateTime = GetDate(),@wSeriesDate = 0
		INSERT INTO AccountsSignin VALUES(@dwUserID,@StartDateTime,@LastDateTime,0,0)		
	END

	-- 签到判断
	IF DateDiff(dd,@LastDateTime,GetDate()) = 0 AND @wSeriesDate > 0
	BEGIN
		SET @strErrorDescribe = N'抱歉，您今天已经签到了！'
		return 3		
	END
	
	
	-- 每天奖励钻石数
	DECLARE @iSiginDayGold AS INT
	SELECT @iSiginDayGold=StatusValue FROM SystemStatusInfo WHERE StatusName=N'SiginDayGold'
	IF @iSiginDayGold IS NULL 
	BEGIN
		SET @strErrorDescribe = N'签到未配置SiginDayGold！'
		return 4		
	END

	-- 更新记录
	SET @wSeriesDate = @wSeriesDate+1
	UPDATE AccountsSignin SET LastDateTime = GetDate(),SeriesDate = @wSeriesDate WHERE UserID = @dwUserID
	
	-- 奖励金币	
	/*
	UPDATE QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo SET Score = Score + @iSiginDayGold WHERE UserID = @dwUserID
	IF @@rowcount = 0
	BEGIN
		-- 插入资料
		INSERT INTO QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo (UserID,Score, LastLogonIP, LastLogonMachine, RegisterIP, RegisterMachine)
		VALUES (@dwUserID, @iSiginDayGold, @strClientIP, @strMachineID, @strClientIP, @strMachineID)
	END
	*/
	-- mChen add,奖励钻石	
	UPDATE QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo SET InsureScore = InsureScore + @iSiginDayGold WHERE UserID = @dwUserID
	IF @@rowcount = 0
	BEGIN
		-- 插入资料
		INSERT INTO QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo (UserID,InsureScore, LastLogonIP, LastLogonMachine, RegisterIP, RegisterMachine)
		VALUES (@dwUserID, @iSiginDayGold, @strClientIP, @strMachineID, @strClientIP, @strMachineID)
	END

	--SET @strErrorDescribe = N'签到成功!获得 '+convert(varchar,@iSiginDayGold)+N' 颗钻石！'
	SET @strErrorDescribe = N'签到成功!'

	-- 查询钻石
	DECLARE @lScore BIGINT	
	SELECT @lScore=InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID = @dwUserID 	
	IF @lScore IS NULL SET @lScore = 0
	
	-- 抛出数据
	SELECT @lScore AS Score, @wSeriesDate As SeriesDate	
END

RETURN 0

GO

----------------------------------------------------------------------------------------------------