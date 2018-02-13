----------------------------------------------------------------------
-- 版权：2017
-- 时间：2017-08-18
-- 用途：用户充值
----------------------------------------------------------------------

USE [QPTreasureDB_HideSeek]
GO


-- 用户充值
IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].GSP_GP_BoughtTaggerModel') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].GSP_GP_BoughtTaggerModel
GO

-- 钻石金币兑换
IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].GSP_GP_ExchangScoreInfo') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].GSP_GP_ExchangScoreInfo
GO

--  执行奖励 WQ
IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GR_AwardDone]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GR_AwardDone]
GO
	
---------------------------------------------------------------------------------------
-- 用户充值
CREATE PROCEDURE GSP_GP_BoughtTaggerModel
	@dwUserID			INT,					-- 用户标识
	@strPassword		NCHAR(32),				-- 用户密码
	@wBoughtModelIndex	SMALLINT,				-- 购买的警察模型
	@dwPayment			INT,					-- 付款金额
	@cbPaymentType		TINYINT,				-- 付款类型：0-金币，1-钻石，2-现金
	@lModelIndexToStore	BIGINT,					-- 编码后存储到AccountsTaggerModel的值
	@strErrorDescribe	NVARCHAR(255) OUTPUT	-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 订单信息
DECLARE @UserID INT
DECLARE @Nullity TINYINT
DECLARE @SpreaderID INT

-- 执行逻辑
BEGIN	
	
	-- 验证用户
	SELECT @UserID=UserID,@Nullity=Nullity
	FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsInfo
	WHERE UserID=@dwUserID

	IF @UserID IS NULL
	BEGIN
		SET @strErrorDescribe=N'抱歉！您的用户账号不存在。'
		RETURN 1
	END

	IF @Nullity=1
	BEGIN
		SET @strErrorDescribe=N'抱歉！您的用户账号暂时处于冻结状态，请联系客户服务中心了解详细情况。'
		RETURN 2
	END	
	
	
	-- 费用处理
	DECLARE @Score BIGINT
	DECLARE @Insure BIGINT
	SELECT @Score=Score,@Insure=InsureScore FROM GameScoreInfo(NOLOCK) WHERE UserID=@UserID
	IF @cbPaymentType=0
	BEGIN
		--金额判断

		IF @Score IS NULL
		BEGIN
			SET @strErrorDescribe=N'抱歉！您的资金账号不存在。'
			RETURN 3
		END
		ELSE IF @Score<@dwPayment
		BEGIN
			-- 错误信息
			SET @strErrorDescribe=N'您的金币不足！'
			RETURN 4
		END
		
		-- 扣除费用
		SET @Score = @Score-@dwPayment
		UPDATE GameScoreInfo SET Score=@Score WHERE UserID=@UserID
	END
	ELSE IF @cbPaymentType=1
	BEGIN
		--金额判断

		IF @Insure IS NULL
		BEGIN
			SET @strErrorDescribe=N'抱歉！您的资金账号不存在。'
			RETURN 5
		END
		ELSE IF @Insure<@dwPayment
		BEGIN
			-- 错误信息
			SET @strErrorDescribe=N'您的钻石不足！'
			RETURN 6
		END
		
		-- 扣除费用
		SET @Insure = @Insure-@dwPayment
		UPDATE GameScoreInfo SET InsureScore=@Insure WHERE UserID=@UserID
	END
	ELSE IF @cbPaymentType<>2
	BEGIN
			SET @strErrorDescribe=N'付款类型不对！'
			RETURN 7
	END
		

	-- 购买记录
	DECLARE @Note NVARCHAR(128)
	DECLARE @RecordID INT
	IF @cbPaymentType=0
	BEGIN
		SET @Note = N'花费'+LTRIM(STR(@dwPayment))+'金币，购买'+LTRIM(STR(@wBoughtModelIndex))+'号警察'
	END 
	ELSE IF @cbPaymentType=1
	BEGIN 
		SET @Note = N'花费'+LTRIM(STR(@dwPayment))+'钻石，购买'+LTRIM(STR(@wBoughtModelIndex))+'号警察'
	END 
	ELSE IF @cbPaymentType=2
	BEGIN
		SET @Note = N'花费'+LTRIM(STR(@dwPayment))+'元，购买'+LTRIM(STR(@wBoughtModelIndex))+'号警察'
	END
	INSERT INTO RecordBoughtTaggerModel(UserID,BoughtModelIndex,Payment,PaymentType,CollectNote)
	VALUES(@UserID,@wBoughtModelIndex,@dwPayment,@cbPaymentType,@Note)	
	SET @RecordID=SCOPE_IDENTITY()
	
	-- 更新用户模型表
	IF @wBoughtModelIndex < 64
	BEGIN
		DECLARE @ModelIndex0 BIGINT
		SELECT @ModelIndex0=ModelIndex0 FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsTaggerModel WHERE UserID=@UserID
		IF @ModelIndex0 IS NULL
		BEGIN
			INSERT INTO QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsTaggerModel(UserID,ModelIndex0)
			VALUES(@UserID,@lModelIndexToStore)	
		END
		ELSE
		BEGIN
			UPDATE QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsTaggerModel SET ModelIndex0=ModelIndex0 | @lModelIndexToStore WHERE UserID=@UserID 
		END
	END
	ELSE IF @wBoughtModelIndex < 128
	BEGIN
		DECLARE @ModelIndex1 BIGINT
		SELECT @ModelIndex1=ModelIndex1 FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsTaggerModel WHERE UserID=@UserID
		IF @ModelIndex1 IS NULL
		BEGIN
			INSERT INTO QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsTaggerModel(UserID,ModelIndex1)
			VALUES(@UserID,@lModelIndexToStore)	
		END
		ELSE
		BEGIN
			UPDATE QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsTaggerModel SET ModelIndex1=ModelIndex1 | @lModelIndexToStore WHERE UserID=@UserID 
		END
	END
	ELSE IF @wBoughtModelIndex < 192
	BEGIN
		DECLARE @ModelIndex2 BIGINT
		SELECT @ModelIndex2=ModelIndex2 FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsTaggerModel WHERE UserID=@UserID
		IF @ModelIndex2 IS NULL
		BEGIN
			INSERT INTO QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsTaggerModel(UserID,ModelIndex2)
			VALUES(@UserID,@lModelIndexToStore)	
		END
		ELSE
		BEGIN
			UPDATE QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsTaggerModel SET ModelIndex2=ModelIndex2 | @lModelIndexToStore WHERE UserID=@UserID 
		END
	END
	
	SET @strErrorDescribe=N'购买成功:'+@Note

	SELECT @UserID AS UserID, @Score AS Score, @Insure AS Insure 
	
END 

RETURN 0
GO

-------------------------------------------------------------------------------------------
-- 钻石金币兑换
CREATE PROCEDURE GSP_GP_ExchangScoreInfo
	@dwUserID			INT,					-- 用户标识
	@cbItemID           TINYINT,                -- 商品ID
	@wAmount            SMALLINT,               -- 金额
	@cbExchangeType     TINYINT,                -- 兑换类型
	@strErrorDescribe	NVARCHAR(255) OUTPUT	-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 订单信息
DECLARE @UserID INT
DECLARE @Nullity TINYINT

-- 执行逻辑
BEGIN	
	
	-- 验证用户
	SELECT @UserID=UserID,@Nullity=Nullity
	FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsInfo
	WHERE UserID=@dwUserID

	IF @UserID IS NULL
	BEGIN
		SET @strErrorDescribe=N'抱歉！您的用户账号不存在。'
		RETURN 1
	END

	IF @Nullity=1
	BEGIN
		SET @strErrorDescribe=N'抱歉！您的用户账号暂时处于冻结状态，请联系客户服务中心了解详细情况。'
		RETURN 2
	END	
	
	
	-- 费用处理
	DECLARE @Score BIGINT
	DECLARE @Insure BIGINT
	SELECT @Score=Score,@Insure=InsureScore FROM GameScoreInfo(NOLOCK) WHERE UserID=@UserID
	IF @cbExchangeType=0 --金币转钻石
	BEGIN
		--金额判断

		IF @Score IS NULL
		BEGIN
			SET @strErrorDescribe=N'抱歉！您的资金账号不存在。'
			RETURN 3
		END
		ELSE IF @Score<@wAmount
		BEGIN
			-- 错误信息
			SET @strErrorDescribe=N'您的金币不足！'
			RETURN 4
		END
		
		-- 扣除费用
		SET @Score = @Score-@wAmount
		UPDATE GameScoreInfo SET Score=@Score,InsureScore=InsureScore+@wAmount*100 WHERE UserID=@UserID
	END
	ELSE IF @cbExchangeType=1 --钻石转金币
	BEGIN
		--金额判断

		IF @Insure IS NULL
		BEGIN
			SET @strErrorDescribe=N'抱歉！您的资金账号不存在。'
			RETURN 5
		END
		ELSE IF @Insure<@wAmount
		BEGIN
			-- 错误信息
			SET @strErrorDescribe=N'您的钻石不足！'
			RETURN 6
		END
		
		-- 扣除费用
		SET @Insure = @Insure-@wAmount
		UPDATE GameScoreInfo SET InsureScore=@Insure,Score=Score+@wAmount*100 WHERE UserID=@UserID
	END
		

	-- 购买记录
	DECLARE @Note NVARCHAR(128)
	DECLARE @RecordID INT
	IF @cbExchangeType=0
	BEGIN
		SET @Note = N'花费'+LTRIM(STR(@wAmount))+'金币，购买钻石'
	END 
	ELSE IF @cbExchangeType=1
	BEGIN 
		SET @Note = N'花费'+LTRIM(STR(@wAmount))+'钻石，购买金币'
	END 
	INSERT INTO RecordExchangeScore(UserID,ItemID,ExchangeType,CollectNote)
	VALUES(@UserID,@cbItemID,@cbExchangeType,@Note)	
	SET @RecordID=SCOPE_IDENTITY()

	SET @strErrorDescribe=N'购买成功:'+@Note

	SELECT @Score=Score,@Insure=InsureScore
	FROM GameScoreInfo
	WHERE @UserID=UserID
	SELECT @cbItemID AS ItemID, @Score AS FinalScore, @Insure AS FinalInsure 
	
END 

RETURN 0
GO

----------------------------------------------------------------------------------------------------

-- 领取奖励
CREATE PROC GSP_GR_AwardDone
	@dwUserID INT,								-- 用户 I D
	@dwAwardGold INT,							-- 奖励金额
	@cbCostType TINYINT,                        -- 奖励类型
	@strPassword NCHAR(32),						-- 用户密码
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

DECLARE @lScore BIGINT
	
-- 执行逻辑
BEGIN

	-- 查询用户
	IF not exists(SELECT * FROM QPAccountsDB_HideSeek.dbo.AccountsInfo WHERE UserID=@dwUserID AND LogonPass=@strPassword)
	BEGIN
		SET @strErrorDescribe = N'抱歉，你的用户信息不存在或者密码不正确！'
		return 1
	END
	
	IF @cbCostType=0
	BEGIN
		-- 奖励金币	
		UPDATE GameScoreInfo SET Score = Score + @dwAwardGold WHERE UserID = @dwUserID
		IF @@rowcount = 0
		BEGIN
			-- 插入数据
			INSERT INTO GameScoreInfo (UserID,Score)
			VALUES (@dwUserID, @dwAwardGold)
		END
		SET @strErrorDescribe = N'奖励获得 '+convert(varchar,@dwAwardGold)+N' 枚金币！'

		-- 查询钻石
		SELECT @lScore=Score FROM GameScoreInfo WHERE UserID = @dwUserID 	

	END
	ELSE IF @cbCostType=1
	BEGIN
		-- 奖励钻石	
		UPDATE GameScoreInfo SET InsureScore = InsureScore + @dwAwardGold WHERE UserID = @dwUserID
		IF @@rowcount = 0
		BEGIN
			-- 插入数据
			INSERT INTO GameScoreInfo (UserID,InsureScore)
			VALUES (@dwUserID, @dwAwardGold)
		END
		
		SET @strErrorDescribe = N'奖励获得 '+convert(varchar,@dwAwardGold)+N' 颗钻石！'

		-- 查询钻石
		SELECT @lScore=InsureScore FROM GameScoreInfo WHERE UserID = @dwUserID 	

	END
	IF @lScore IS NULL SET @lScore = 0
	
	-- 抛出数据
	SELECT @lScore AS Score, @cbCostType As CostType	
END

RETURN 0

GO


