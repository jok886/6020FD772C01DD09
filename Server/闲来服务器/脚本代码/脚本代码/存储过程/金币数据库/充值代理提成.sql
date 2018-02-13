----------------------------------------------------------------------
-- 版权：2017
-- 时间：2017-08-18
-- 用途：用户充值
----------------------------------------------------------------------

USE [QPTreasureDB_HideSeek]
GO

-- 企业提现
IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].GSP_GP_AddEnterprisePayment') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].GSP_GP_AddEnterprisePayment
GO

-- 企业提现结果
IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].GSP_GP_AddEnterprisePaymentResult') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].GSP_GP_AddEnterprisePaymentResult
GO

-- 用户充值
IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].GSP_GP_AddPayment') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].GSP_GP_AddPayment
GO

-- 查询名下用户交易信息
IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].GSP_GP_QueryChildrenPaymentInfo') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].GSP_GP_QueryChildrenPaymentInfo
GO

-- 商品购买记录 WQ
IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].GSP_GP_ShopItemInfo') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].GSP_GP_ShopItemInfo
GO
-- 道具消费 WQ
IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].GSP_GP_InventoryConsumptionInfo') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].GSP_GP_InventoryConsumptionInfo
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

-------商品购买 WQ
CREATE PROCEDURE GSP_GP_ShopItemInfo
	@dwUserID		INT,					        -- 用户标识
	@szUID			NVARCHAR(49),				    -- 用户UID
	@szOrderID   	NVARCHAR(31),					-- 订单号
	@wItemID        smallint,                       ---商品ID
	@wAmount        smallint,                       ---总金额（元）
	@wCount         smallint,                       ---个数
	@strErrorDescribe		NVARCHAR(255) OUTPUT	-- 输出信息
WITH ENCRYPTION AS

SET NOCOUNT ON
---订单信息
--DECLARE @UserID INT
--DECLARE @_UID NVARCHAR(49)
DECLARE @OrderID NVARCHAR(31)
--DECIMAL @ItemID smallint
--DECIMAL @Amount smallint
--DECIMAL @_Count smallint

---执行逻辑
BEGIN

---验证订单
     SELECT @OrderID=OrderID
	 FROM RecordAddShopItemInfo
	 WHERE OrderID=@szOrderID
	 
	 IF @OrderID IS NULL
	 BEGIN
	    ---充值钻石
		UPDATE GameScoreInfo SET InsureScore=InsureScore+@wCount WHERE UserID=@dwUserID
		IF @@ROWCOUNT=0	
		BEGIN
		INSERT GameScoreInfo(UserID,InsureScore)
		VALUES (@dwUserID,@wCount)
		END
		---添加记录
		BEGIN
			DECLARE @RecordID INT
			INSERT INTO RecordAddShopItemInfo(UserID,_UID,OrderID,ItemID,Amount,_Count)
			VALUES (@dwUserID,@szUID,@szOrderID,@wItemID,@wAmount,@wCount)
			SET @RecordID=SCOPE_IDENTITY()
		END
		SET @strErrorDescribe =N'订单支付成功，请耐心等待！'
		
	END
	 
	ELSE
	BEGIN
		SET @strErrorDescribe =N'抱歉！该订单已存在，请勿重复提交！'
		RETURN 1
	END

    DECLARE @dwFinalInsureScore INT
	SELECT @dwFinalInsureScore=InsureScore FROM GameScoreInfo WHERE UserID=@dwUserID
	SELECT @dwFinalInsureScore AS FinalInsureScore
END 

RETURN 0
GO


---------------------------------------------------------------------------------------

-------道具消费 WQ
CREATE PROCEDURE GSP_GP_InventoryConsumptionInfo
	@dwUserID		INT,					        -- 用户标识
	@cbItemID       tinyint,                        ---商品ID
	@wAmount        smallint,                       ---消费金额
	@cbCostType         tinyint,                        ---消费类型
	@strErrorDescribe		NVARCHAR(255) OUTPUT	-- 输出信息
WITH ENCRYPTION AS

SET NOCOUNT ON

DECLARE @tempScore INT
DECLARE @finalScore INT
---执行逻辑
BEGIN

	-- 查询用户
	IF not exists(SELECT * FROM QPAccountsDB_HideSeek.dbo.AccountsInfo WHERE UserID=@dwUserID)
	BEGIN
		SET @strErrorDescribe = N'抱歉，你的用户信息不存在或者密码不正确！'
		return 1
	END
	IF @cbCostType=0
	BEGIN
		SELECT @tempScore=Score
		FROM QPTreasureDB_HideSeek.dbo.GameScoreInfo 
		WHERE UserID=@dwUserID
		IF @tempScore>@wAmount
		BEGIN
			UPDATE GameScoreInfo SET Score=Score-@wAmount WHERE UserID=@dwUserID
			SELECT @finalScore=Score
			FROM QPTreasureDB_HideSeek.dbo.GameScoreInfo 
			WHERE UserID=@dwUserID
		END
		ELSE
		BEGIN
			SET @strErrorDescribe = N'抱歉，你的金币不足！'
			return 2
		END
	END
	ELSE IF @cbCostType=1
	BEGIN
		SELECT @tempScore=InsureScore
		FROM QPTreasureDB_HideSeek.dbo.GameScoreInfo 
		WHERE UserID=@dwUserID
		IF @tempScore>@wAmount
		BEGIN
			UPDATE GameScoreInfo SET InsureScore=InsureScore-@wAmount WHERE UserID=@dwUserID
			SELECT @finalScore=InsureScore
			FROM QPTreasureDB_HideSeek.dbo.GameScoreInfo 
			WHERE UserID=@dwUserID
		END
		ELSE
		BEGIN
			SET @strErrorDescribe = N'抱歉，你的钻石不足！'
			return 2
		END
	END
	SELECT @cbItemID AS ItemID,@finalScore AS FinalScore,@cbCostType AS CostType
END 
	SET @strErrorDescribe = N'成功使用道具！'
RETURN 0
GO


---------------------------------------------------------------------------------------
-- 企业提现
CREATE PROCEDURE GSP_GP_AddEnterprisePayment
	@dwUserID				INT,					-- 用户标识
	@strPassword			NCHAR(32),				-- 用户密码
	@dwEnterprisePayment	INT,					-- 提现金额（分）
	@strErrorDescribe		NVARCHAR(255) OUTPUT	-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 订单信息
DECLARE @UserID INT
DECLARE @Accounts NVARCHAR(31)
DECLARE @GameID INT
DECLARE @Nullity TINYINT
DECLARE @SpreaderID INT

-- 金币信息
DECLARE @Score BIGINT

-- 执行逻辑
BEGIN	
	
	-- 验证用户
	DECLARE @Openid NVARCHAR(31)
	SELECT @UserID=UserID,@Openid=Openid,@GameID=GameID,@Accounts=Accounts,@Nullity=Nullity,@SpreaderID=SpreaderID
	FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsInfo
	WHERE UserID=@dwUserID

	IF @UserID IS NULL
	BEGIN
		SET @strErrorDescribe=N'抱歉！您要提现的用户账号不存在。'
		RETURN 1
	END

	IF @Nullity=1
	BEGIN
		SET @strErrorDescribe=N'抱歉！您要提现的用户账号暂时处于冻结状态，请联系客户服务中心了解详细情况。'
		RETURN 2
	END	
	
	-- 查验代理人身份
	DECLARE @RealName NVARCHAR(31)
	DECLARE @ExtraCash DECIMAL(16,4)	--获得的额外金额(元)：比如删除直属下级代理获得对方的剩余金额
	SELECT @RealName=RealName,@ExtraCash=ExtraCash FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.SpreadersInfo WHERE SpreaderID=@dwUserID
	IF @RealName IS NULL
	BEGIN
		SET @strErrorDescribe=N'您不是代理人，无法提现！'
		RETURN 3
	END
	
	--计算已提现金额(元)
	DECLARE @CashedOut DECIMAL(16,4)
	SELECT @CashedOut=SUM(Amount) FROM RecordSpreaderCashout WHERE SpreaderID=@dwUserID
	IF @CashedOut IS NULL
	BEGIN
		SET @CashedOut=0
	END	
		
	-- 验证提现金额
	DECLARE @TotalGrant DECIMAL(16,4)					--总金额（元）
	DECLARE @TotalGrantOfChildrenBuy DECIMAL(16,4)		--名下用户充值获得的提成总额（元）
	DECLARE @TotalLeftCash INT							--剩余金额（分）
	SELECT @TotalGrantOfChildrenBuy=SUM(Payment*PaymentGrantRate) FROM RecordChildrenPayment WHERE RelatedSpreaderID=@dwUserID 
	IF @TotalGrantOfChildrenBuy IS NULL
	BEGIN
		SET @TotalGrantOfChildrenBuy=0
	END
	SET @TotalGrant=@TotalGrantOfChildrenBuy+@ExtraCash
	SET @TotalLeftCash=(@TotalGrant-@CashedOut)*100
	IF @TotalLeftCash<@dwEnterprisePayment
	BEGIN
		SET @strErrorDescribe=N'超额提现！'
		RETURN 4
	END
	
	SET @strErrorDescribe=N'提现数据库验证成功!'
	
	SELECT @UserID AS UserID, @RealName AS RealName, @Openid AS Openid, @TotalLeftCash AS TotalLeftCash
END 

RETURN 0
GO



---------------------------------------------------------------------------------------
-- 企业提现结果
CREATE PROCEDURE GSP_GP_AddEnterprisePaymentResult
	@dwUserID				INT,					-- 用户标识
	@dwEnterprisePayment	INT,					-- 提现金额（分）
	@strErrorDescribe		NVARCHAR(255) OUTPUT	-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN	
	
	--提现记录
	INSERT INTO RecordSpreaderCashout(SpreaderID,Amount) VALUES(@dwUserID,@dwEnterprisePayment/100.0)		
	
	/*
	-- 更新已提现金额（元）
	UPDATE QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.SpreadersInfo SET CashedOut=CashedOut+@dwEnterprisePayment/100.0 
	WHERE SpreaderID=@dwUserID
	IF @@ROWCOUNT=0
	BEGIN
		SET @strErrorDescribe=N'更新已提现金额 失败!'
		RETURN 1
	END
	*/
	
	SET @strErrorDescribe=N'更新提现记录 成功!'
	
END 

RETURN 0
GO

	
---------------------------------------------------------------------------------------
-- 用户充值
CREATE PROCEDURE GSP_GP_AddPayment
	@dwUserID			INT,					-- 用户标识
	@strPassword		NCHAR(32),				-- 用户密码
	@dwPayAmount		INT,					-- 付款金额
	@dwBoughtDiamond	INT,					-- 所购钻石
	--@dwFinalInsureScore INT output	,			-- 最终钻石数目
	@strErrorDescribe	NVARCHAR(255) OUTPUT	-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 订单信息
DECLARE @UserID INT
DECLARE @Accounts NVARCHAR(31)
DECLARE @GameID INT
DECLARE @Nullity TINYINT
DECLARE @SpreaderID INT

-- 金币信息
DECLARE @Score BIGINT

-- 执行逻辑
BEGIN	
	
	-- 验证用户
	SELECT @UserID=UserID,@GameID=GameID,@Accounts=Accounts,@Nullity=Nullity,@SpreaderID=SpreaderID
	FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsInfo
	WHERE UserID=@dwUserID

	IF @UserID IS NULL
	BEGIN
		SET @strErrorDescribe=N'抱歉！您要充值的用户账号不存在。'
		RETURN 2
	END

	IF @Nullity=1
	BEGIN
		SET @strErrorDescribe=N'抱歉！您要充值的用户账号暂时处于冻结状态，请联系客户服务中心了解详细情况。'
		RETURN 3
	END	
	
	--推广系统
	--普通用户绑定代理后，充值赠送比率
	DECLARE @dwGrantDiamond INT
	IF @SpreaderID<>0
	BEGIN
		DECLARE @PaymentGrantRate DECIMAL(16,4)
		SELECT @PaymentGrantRate=PaymentGrantRate FROM GlobalSpreadInfo
		IF @PaymentGrantRate IS NULL
		BEGIN
			SET @PaymentGrantRate=0.20
		END
		
		SET @dwGrantDiamond = cast(round(@PaymentGrantRate*@dwBoughtDiamond,0) as INT)
	END 
	ELSE
	BEGIN
		SET @dwGrantDiamond=0
	END

	-- 充值钻石
	UPDATE GameScoreInfo SET InsureScore=InsureScore+@dwBoughtDiamond+@dwGrantDiamond WHERE UserID=@UserID
	IF @@ROWCOUNT=0	
	BEGIN
		INSERT GameScoreInfo(UserID,InsureScore,RegisterIP,LastLogonIP)
		VALUES (@UserID,@dwBoughtDiamond+@dwGrantDiamond,'0.0.0.0','0.0.0.0')
	END

	-- 充值记录
	DECLARE @Note NVARCHAR(128)
	DECLARE @RecordID INT
	SET @Note = N'充值'+LTRIM(STR(@dwPayAmount))+'元，购买'+LTRIM(STR(@dwBoughtDiamond))+'钻石，赠送'+LTRIM(STR(@dwGrantDiamond))+'钻石'
	INSERT INTO RecordUserPayment(UserID,BindSpreaderID,Payment,BoughtInsureScore,GrantInsureScore,TypeID,CollectNote)
	VALUES(@UserID,@SpreaderID,@dwPayAmount,@dwBoughtDiamond,@dwGrantDiamond,1,@Note)	
	SET @RecordID=SCOPE_IDENTITY()

	-- 记录 for代理提现
	IF @SpreaderID<>0
	BEGIN
	
		--获取提成比率配置
		DECLARE @FillGrantRateOfL1 DECIMAL(16,4)
		DECLARE @FillGrantRateOfL2 DECIMAL(16,4)
		DECLARE @FillGrantRateOfL3 DECIMAL(16,4)
		DECLARE @FillGrantRateOfSecondhand DECIMAL(16,4)
		DECLARE @FillGrantRateOfThirdhand DECIMAL(16,4)
		SELECT @FillGrantRateOfL1=FillGrantRateOfL1,@FillGrantRateOfL2=FillGrantRateOfL2,@FillGrantRateOfL3=FillGrantRateOfL3, 
		@FillGrantRateOfSecondhand=FillGrantRateOfSecondhand,@FillGrantRateOfThirdhand=FillGrantRateOfThirdhand 
		FROM GlobalSpreadInfo
		IF @FillGrantRateOfL1 IS NULL
		BEGIN
			SET @strErrorDescribe=N'没有配置提成比率GlobalSpreadInfo'
			RETURN 4
		END
		
		DECLARE @SpreaderIDOfSecondhand INT 	--隔一层代理
		DECLARE @SpreaderIDOfThirdhand INT		--隔二层代理
		
		--直代
		---------------------------------------------------------------------
		DECLARE @dwSpreaderLevel INT			--直代等级
		SELECT @dwSpreaderLevel=SpreaderLevel,@SpreaderIDOfSecondhand=ParentID FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.SpreadersInfo WHERE SpreaderID=@SpreaderID
		IF @dwSpreaderLevel IS NOT NULL AND @dwSpreaderLevel<>0
		BEGIN
		
			--获取提成比率
			DECLARE @GrantRate DECIMAL(16,4)
			IF @dwSpreaderLevel=1
			BEGIN
				SET @GrantRate = @FillGrantRateOfL1
			END ELSE
			IF @dwSpreaderLevel=2
			BEGIN
				SET @GrantRate = @FillGrantRateOfL2
			END ELSE
			IF @dwSpreaderLevel=3
			BEGIN
				SET @GrantRate = @FillGrantRateOfL3
			END 
			
			--写记录
			INSERT INTO RecordChildrenPayment(RecordIDOfUserPayment,UserID,Payment,RelatedSpreaderID,RelatedSpreaderType,PaymentGrantRate,PaymentGrantState)
			VALUES(@RecordID,@UserID,@dwPayAmount,@SpreaderID,0,@GrantRate,0)	
		
		
			--隔一层代理
			----------------------------------------------------------------------------------	
			DECLARE @dwSpreaderLevelOfSecondhand INT	--隔一层代理等级
			SELECT @dwSpreaderLevelOfSecondhand=SpreaderLevel,@SpreaderIDOfThirdhand=ParentID FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.SpreadersInfo WHERE SpreaderID=@SpreaderIDOfSecondhand
			IF @dwSpreaderLevelOfSecondhand IS NOT NULL AND @dwSpreaderLevelOfSecondhand<>0
			BEGIN
				--获取提成比率
				SET @GrantRate = @FillGrantRateOfSecondhand
				
				--写记录
				INSERT INTO RecordChildrenPayment(RecordIDOfUserPayment,UserID,Payment,RelatedSpreaderID,RelatedSpreaderType,PaymentGrantRate,PaymentGrantState)
				VALUES(@RecordID,@UserID,@dwPayAmount,@SpreaderIDOfSecondhand,1,@GrantRate,0)	
				
				
				--隔二层代理
				-----------------------------------------------------------------------------------------
				DECLARE @dwSpreaderLevelOfThirdhand INT	--隔二层代理等级
				SELECT @dwSpreaderLevelOfThirdhand=SpreaderLevel FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.SpreadersInfo WHERE SpreaderID=@SpreaderIDOfThirdhand
				IF @dwSpreaderLevelOfThirdhand IS NOT NULL AND @dwSpreaderLevelOfThirdhand<>0
				BEGIN
					--获取提成比率
					SET @GrantRate = @FillGrantRateOfThirdhand
					
					--写记录
					INSERT INTO RecordChildrenPayment(RecordIDOfUserPayment,UserID,Payment,RelatedSpreaderID,RelatedSpreaderType,PaymentGrantRate,PaymentGrantState)
					VALUES(@RecordID,@UserID,@dwPayAmount,@SpreaderIDOfThirdhand,2,@GrantRate,0)	
				END
			END		
		END
		
	END
	
	SET @strErrorDescribe=N'充值成功:'+@Note
	
	DECLARE @dwFinalInsureScore INT
	SELECT @dwFinalInsureScore=InsureScore FROM GameScoreInfo WHERE UserID=@UserID
	
	SELECT @UserID AS UserID, @dwFinalInsureScore AS FinalInsureScore
	
END 

RETURN 0
GO



--查询名下用户交易信息
----------------------------------------------------------------------------------------------------

-- 查询资料
CREATE PROC GSP_GP_QueryChildrenPaymentInfo
	@dwUserID INT,								-- 用户ID
	@strPassword NCHAR(32),						-- 用户密码
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

	-- 变量定义
	DECLARE @LogonPass AS NCHAR(32)

	-- 查询用户
	SELECT @LogonPass=LogonPass FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.AccountsInfo WHERE UserID=@dwUserID

	-- 密码判断
	IF @LogonPass<>@strPassword
	BEGIN
		SET @strErrorDescribe=N'您的用户密码不正确！'
		RETURN 1
	END
	
	
	-- 查验操作人
	DECLARE @dwMySpreaderLevel INT
	DECLARE @ExtraCash DECIMAL(16,4)	--获得的额外金额(元)：比如删除直属下级代理获得对方的剩余金额
	SELECT @dwMySpreaderLevel=SpreaderLevel,@ExtraCash=ExtraCash FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.SpreadersInfo WHERE SpreaderID=@dwUserID
	IF @dwMySpreaderLevel IS NULL
	BEGIN
		SET @strErrorDescribe=N'您不是代理人，没有权限执行此操作！'
		RETURN 2
	END
		
	--计算已提现金额(元)
	DECLARE @CashedOut DECIMAL(16,4)
	SELECT @CashedOut=SUM(Amount) FROM RecordSpreaderCashout WHERE SpreaderID=@dwUserID
	IF @CashedOut IS NULL
	BEGIN
		SET @CashedOut=0
	END	
		
	-- 查询名下用户交易信息
	DECLARE @HasChildrenBuyCount INT
	DECLARE @TotalGrant DECIMAL(16,4)					--总金额（元）
	DECLARE @TotalGrantOfChildrenBuy DECIMAL(16,4)		--名下用户充值获得的提成总额（元）
	DECLARE @TotalLeftCash DECIMAL(16,4)				--剩余金额（元）
	SELECT @TotalGrantOfChildrenBuy=SUM(Payment*PaymentGrantRate) FROM RecordChildrenPayment WHERE RelatedSpreaderID=@dwUserID 
	IF @TotalGrantOfChildrenBuy IS NULL
	BEGIN
		SET @TotalGrantOfChildrenBuy=0
		SET @TotalGrant=@TotalGrantOfChildrenBuy+@ExtraCash
		SET @TotalLeftCash=@TotalGrant-@CashedOut
		
		SET @strErrorDescribe=N'查询名下用户交易信息成功!'
		SET @HasChildrenBuyCount=0
		SELECT @HasChildrenBuyCount as HasChildrenBuyCount, @TotalGrantOfChildrenBuy as TotalGrantOfChildrenBuy, @ExtraCash as ExtraCash, @CashedOut as CashedOut, @TotalLeftCash as TotalLeftCash 
	END ELSE
	BEGIN
		SET @TotalGrant=@TotalGrantOfChildrenBuy+@ExtraCash
		SET @TotalLeftCash=@TotalGrant-@CashedOut
		
		SET @strErrorDescribe=N'查询名下用户交易信息成功!'
		SET @HasChildrenBuyCount=1
		SELECT *, @HasChildrenBuyCount as HasChildrenBuyCount, @TotalGrantOfChildrenBuy as TotalGrantOfChildrenBuy, @ExtraCash as ExtraCash, @CashedOut as CashedOut, @TotalLeftCash as TotalLeftCash FROM RecordChildrenPayment WHERE RelatedSpreaderID=@dwUserID 
	END
	
	/*
	DECLARE @FillGrantRateOfL1 DECIMAL(16,4)
	DECLARE @FillGrantRateOfL2 DECIMAL(16,4)
	DECLARE @FillGrantRateOfL3 DECIMAL(16,4)
	DECLARE @FillGrantRateOfSecondhand DECIMAL(16,4)
	DECLARE @FillGrantRateOfThirdhand DECIMAL(16,4)
	SELECT @FillGrantRateOfL1=FillGrantRateOfL1,@FillGrantRateOfL2=FillGrantRateOfL2,@FillGrantRateOfL3=FillGrantRateOfL3, 
	@FillGrantRateOfSecondhand=FillGrantRateOfSecondhand,@FillGrantRateOfThirdhand=FillGrantRateOfThirdhand
	FROM GlobalSpreadInfo
	IF @FillGrantRateOfL1 IS NULL
	BEGIN
		SET @strErrorDescribe=N'没有配置提成比率GlobalSpreadInfo'
		RETURN 2
	END
		
	DECLARE @Gain DECIMAL(16,4)
	DECLARE @dwPayAmount INT

	--dwUserID是直代
	
	--获取提成比率
	DECLARE @GrantRate DECIMAL(16,4)
	IF @dwMySpreaderLevel=1
	BEGIN
		SET @GrantRate = @FillGrantRateOfL1
	END ELSE
	IF @dwMySpreaderLevel=2
	BEGIN
		SET @GrantRate = @FillGrantRateOfL2
	END ELSE
	IF @dwMySpreaderLevel=3
	BEGIN
		SET @GrantRate = @FillGrantRateOfL3
	END
	
	SELECT *,Payment*@GrantRate AS Gain FROM RecordUserPayment WHERE BindSpreaderID=@dwUserID
	

	--dwUserID是隔层代理
	SET @GrantRate = @FillGrantRateOfSecondhand
	
	--dwUserID是隔二层代理
	SET @GrantRate = @FillGrantRateOfThirdhand
	
	SELECT * FROM RecordUserPayment WHERE BindSpreaderID<>0 AND 
		(BindSpreaderID=@dwUserID or BindSpreaderID in
			(
				--dwUserID的子代理
				SELECT SpreaderID FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.SpreadersInfo WHERE ParentID=@dwUserID 
				or ParentID in (SELECT SpreaderID FROM QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.SpreadersInfo WHERE ParentID=@dwUserID)
			)
		)
	*/
		
END

RETURN 0

GO


