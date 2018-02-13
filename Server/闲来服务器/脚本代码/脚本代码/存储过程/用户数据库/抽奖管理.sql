
----------------------------------------------------------------------------------------------------

USE QPAccountsDB_HideSeek
GO


IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GR_RaffleDone]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GR_RaffleDone]
GO


SET QUOTED_IDENTIFIER ON 
GO

SET ANSI_NULLS ON 
GO


----------------------------------------------------------------------------------------------------

-- 领取奖励
CREATE PROC GSP_GR_RaffleDone
	@dwUserID INT,								-- 用户 I D
	@dwRaffleGold INT,							-- 抽到的钻石
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

	-- 每打完多少场可以抽奖一次
	DECLARE @PlayCountPerRaffle AS INT
	SELECT @PlayCountPerRaffle=StatusValue FROM SystemStatusInfo WHERE StatusName=N'PlayCountPerRaffle'
	IF @PlayCountPerRaffle IS NULL 
	BEGIN
		SET @strErrorDescribe = N'抽奖未配置PlayCountPerRaffle！'
		return 2		
	END
	
	-- 抽奖记录查询
	DECLARE @RaffleCount INT	
	SELECT @RaffleCount=RaffleCount FROM AccountsRaffle WHERE UserID=@dwUserID
	IF @RaffleCount IS NULL
	BEGIN
		SET @RaffleCount = 0
		
		--插入数据
		INSERT INTO AccountsRaffle VALUES(@dwUserID,GetDate(),@RaffleCount,0,@PlayCountPerRaffle)		
	END

	SET @RaffleCount = @RaffleCount+1
		
	--查询用户已打场次
	DECLARE @PlayCount INT
	SELECT  @PlayCount=COUNT(*) FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.PrivateGameRecordUserRecordID WHERE UserID = @dwUserID
	
	--是否打满可抽奖场次
	DECLARE @MinPlayCountToRaffle INT
	SET @MinPlayCountToRaffle = @RaffleCount * @PlayCountPerRaffle
	IF @PlayCount < @MinPlayCountToRaffle
	BEGIN
		--SET @strErrorDescribe = N'未打满'+convert(varchar,@MinPlayCountToRaffle)+N'场不能抽奖！'
		SET @strErrorDescribe = N'未打满'+convert(varchar,@PlayCountPerRaffle)+N'场不能抽奖！'
		return 3		
	END
	
	-- 奖励钻石	
	UPDATE QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo SET InsureScore = InsureScore + @dwRaffleGold WHERE UserID = @dwUserID
	IF @@rowcount = 0
	BEGIN
		-- 插入数据
		INSERT INTO QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo (UserID,InsureScore, LastLogonIP, LastLogonMachine, RegisterIP, RegisterMachine)
		VALUES (@dwUserID, @dwRaffleGold, @strClientIP, @strMachineID, @strClientIP, @strMachineID)
	END

	-- 更新记录
	UPDATE AccountsRaffle SET LastDateTime = GetDate(),RaffleCount = @RaffleCount, RewardGold = RewardGold + @dwRaffleGold  WHERE UserID = @dwUserID
	
	SET @strErrorDescribe = N'抽奖成功!获得 '+convert(varchar,@dwRaffleGold)+N' 颗钻石！'

	-- 查询钻石
	DECLARE @lScore BIGINT	
	SELECT @lScore=InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID = @dwUserID 	
	IF @lScore IS NULL SET @lScore = 0
	
	-- 抛出数据
	SELECT @lScore AS Score, @RaffleCount As RaffleCount, @PlayCount As PlayCount
END

RETURN 0

GO

----------------------------------------------------------------------------------------------------