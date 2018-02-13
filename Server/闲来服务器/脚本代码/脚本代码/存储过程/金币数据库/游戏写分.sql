
----------------------------------------------------------------------------------------------------

USE QPTreasureDB_HideSeek
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GR_WriteGameScore]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GR_WriteGameScore]
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GR_MatchRecordScore]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GR_MatchRecordScore]
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GR_MatchTopScore]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GR_MatchTopScore]
GO

SET QUOTED_IDENTIFIER ON 
GO

SET ANSI_NULLS ON 
GO

----------------------------------------------------------------------------------------------------

-- 游戏写分
CREATE PROC GSP_GR_WriteGameScore

	-- 用户信息
	@dwUserID INT,								-- 用户 I D
	@dwDBQuestID INT,							-- 请求标识
	@dwInoutIndex INT,							-- 进出索引

	-- 变更成绩
	@lVariationScore BIGINT,					-- 用户分数
	@lVariationGrade BIGINT,					-- 用户成绩
	@lVariationInsure BIGINT,					-- 用户银行
	@lVariationRevenue BIGINT,					-- 游戏税收
	@lVariationWinCount INT,					-- 胜利盘数
	@lVariationLostCount INT,					-- 失败盘数
	@lVariationDrawCount INT,					-- 和局盘数
	@lVariationFleeCount INT,					-- 断线数目
	@lVariationUserMedal INT,					-- 用户奖牌
	@lVariationExperience INT,					-- 用户经验
	@lVariationLoveLiness INT,					-- 用户魅力
	@dwVariationPlayTimeCount INT,				-- 游戏时间

	-- 属性信息
	@wKindID INT,								-- 游戏 I D
	@wServerID INT,								-- 房间 I D
	@strClientIP NVARCHAR(15)					-- 连接地址

WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

	DECLARE @TreasureScore BIGINT
	SELECT @TreasureScore=Score+@lVariationScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID=@dwUserID
	
	if @TreasureScore < 0
	BEGIN							
		SET @TreasureScore=0
	END	
	
	-- 用户积分
	UPDATE QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo SET Score=@TreasureScore,
		WinCount=WinCount+@lVariationWinCount, LostCount=LostCount+@lVariationLostCount, DrawCount=DrawCount+@lVariationDrawCount,
		FleeCount=FleeCount+@lVariationFleeCount, PlayTimeCount=PlayTimeCount+@dwVariationPlayTimeCount
	WHERE UserID=@dwUserID

	-- 全局信息
	IF @lVariationExperience>0 OR @lVariationLoveLiness>0 OR @lVariationUserMedal>0
	BEGIN
		UPDATE QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsInfo SET Experience=Experience+@lVariationExperience, LoveLiness=LoveLiness+@lVariationLoveLiness,
			UserMedal=UserMedal+@lVariationUserMedal
		WHERE UserID=@dwUserID
	END

END

RETURN 0

GO

----------------------------------------------------------------------------------------------------

----------------------------------------------------------------------------------------------------

-- 比赛写分
CREATE PROC GSP_GR_MatchRecordScore

	-- 用户信息
	@dwUserID INT,								-- 用户ID
	@strNickName NVARCHAR(31),					-- 用户昵称
	@lScore BIGINT								-- 本次比赛积分

WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN
	
	DECLARE	@CurData datetime
	SET @CurData = GetDate()
	DECLARE @WinCount int 
	DECLARE @DrawCount int
	DECLARE @LoseCount int
	
	SET @WinCount = 0
	SET @DrawCount = 0
	SET @LoseCount = 0
	
	IF  @lScore > 0
	BEGIN
		SET @WinCount = 1
	END
	ELSE IF @lScore = 0
	BEGIN
		SET @DrawCount = 1
	END
	ELSE
	BEGIN
		SET @LoseCount = 1
	END

	--mysql写法
	/*INSERT INTO QPTreasureDB_HideSeek.dbo.RecordScoreInfo (UserID,MatchScore,MatchWinCount,MatchDrawCount,MatchLoseCount) VALUES (@dwUserID,@lScore,@WinCount, @DrawCount,@LoseCount)
  	ON DUPLICATE KEY UPDATE MatchScore=MatchScore + @lScore, MatchDrawCount=MatchDrawCount+@DrawCount,MatchWinCount=MatchWinCount+@WinCount,MatchLoseCount= MatchLoseCount+@LoseCount WHERE UserID = @dwUserID*/
  	--已有用户积分则更新socre，与相应的胜负平局数，不然插入新行（更新总分表）
  	IF EXISTS(SELECT * FROM QPTreasureDB_HideSeek.dbo.RecordScoreInfo WHERE UserID = @dwUserID)
  	BEGIN
  		UPDATE QPTreasureDB_HideSeek.dbo.RecordScoreInfo SET MatchScore=MatchScore + @lScore, MatchDrawCount=MatchDrawCount+@DrawCount,MatchWinCount=MatchWinCount+@WinCount,MatchLoseCount= MatchLoseCount+@LoseCount WHERE UserID = @dwUserID
  	END
  	ELSE
  	BEGIN
  		INSERT INTO QPTreasureDB_HideSeek.dbo.RecordScoreInfo (UserID,NickName,MatchScore,MatchWinCount,MatchDrawCount,MatchLoseCount) VALUES (@dwUserID,@strNickName,@lScore,@WinCount, @DrawCount,@LoseCount)
  	END
	
	--插入每局积分表
	INSERT INTO QPTreasureDB_HideSeek.dbo.RecordScore (UserID,MatchScore,MatchTime)	VALUES (@dwUserID,@lScore,@CurData)
END

RETURN 0

GO

CREATE PROCEDURE GSP_GR_MatchTopScore 
	@dwTopCount int								-- 排名个数
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--DECLARE @UserID INT
	--DECLARE @NickName NVARCHAR(31)
	--DECLARE @MatchScore int
	--DECLARE @MatchWinCount int
	--DECLARE @MatchDrawCount int
	--DECLARE @MatchLoseCount int

    -- Insert statements for procedure here
	-- 查询用户
	--SELECT TOP (@dwTopCount) @UserID=UserID,@NickName=NickName,@MatchScore=MatchScore,@MatchWinCount=MatchWinCount,@MatchDrawCount=MatchDrawCount,@MatchLoseCount=MatchLoseCount From QPTreasureDB_HideSeek.dbo.RecordScoreInfo Order by MatchScore desc
	--SELECT TOP (@dwTopCount) UserID,NickName,MatchScore,MatchWinCount,MatchDrawCount,MatchLoseCount From QPTreasureDB_HideSeek.dbo.RecordScoreInfo Order by MatchScore desc
	SELECT TOP (@dwTopCount) UserID,NickName,Experience From QPAccountsDB_HideSeek.dbo.AccountsInfo Order by Experience desc
	-- 输出信息
	--SELECT @UserID AS UserID, @NickName AS NickName, @MatchScore AS MatchScore, @MatchWinCount AS MatchWinCount, @MatchDrawCount AS MatchDrawCount, @MatchLoseCount AS MatchLoseCount
	
	--RETURN 0
END

RETURN 0

GO