
----------------------------------------------------------------------------------------------------

USE QPAccountsDB_HideSeek
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GP_ModifySpreader]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GP_ModifySpreader]
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GP_AddDelSpreader]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GP_AddDelSpreader]
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GP_QuerySpreadersInfo]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GP_QuerySpreadersInfo]
GO


SET QUOTED_IDENTIFIER ON 
GO

SET ANSI_NULLS ON 
GO

----------------------------------------------------------------------------------------------------

-- 查询资料
CREATE PROC GSP_GP_ModifySpreader
	@dwUserID INT,								-- 用户 I D
	@strPassword NCHAR(32),						-- 用户密码
	@dwSpreaderId INT,							-- mChen edit, @strSpreader NVARCHAR(32),					-- 推荐人
	@strClientIP NVARCHAR(15),					-- 连接地址
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

	-- 变量定义
	DECLARE @LogonPass AS NCHAR(32)
	DECLARE @OldSpreaderID AS NCHAR(32)

	SET @strErrorDescribe=N'Start！'
	
	-- 查询用户
	SELECT @OldSpreaderID=SpreaderID ,@LogonPass=LogonPass FROM AccountsInfo(NOLOCK) WHERE UserID=@dwUserID

	-- 密码判断
	IF @LogonPass<>@strPassword
	BEGIN
		SET @strErrorDescribe=N'您的用户密码不正确，个人信息查询失败！'
		RETURN 1
	END
	
	-- 推荐人
	IF @OldSpreaderID<>0
	BEGIN
		SET @strErrorDescribe=N'您已经设置了推荐人！'
		RETURN 2
	END
	
	-- 推荐人
	IF @dwSpreaderId=0	--mChen edit, IF @strSpreader=N''
	BEGIN
		SET @strErrorDescribe=N'推荐人为空！'
		RETURN 3
	END
	
	IF @dwSpreaderId = @dwUserID
	BEGIN
		SET @strErrorDescribe=N'推荐人不能是自己！'
		RETURN 4
	END
	

	-- 查询推荐人
	--mChen edit
	/*
	DECLARE @SpreaderUserID AS INT
	SELECT @SpreaderUserID=UserID FROM AccountsInfo WHERE UserID=@dwSpreaderId AND IsSpreader<>0
	--SELECT @SpreaderUserID=UserID FROM AccountsInfo WHERE Accounts=@strSpreader

	-- 结果处理
	IF @SpreaderUserID IS NULL
	BEGIN
		SET @strErrorDescribe=N'您输入的推荐码无效'
		--SET @strErrorDescribe=N'您输入的推荐人'+convert(varchar,@dwSpreaderId)+N'不存在或者不是有效的推荐人！'
		RETURN 6
	END
	*/
	DECLARE @ResultCout AS INT
	SELECT @ResultCout=count(*) FROM SpreadersInfo WHERE SpreaderID=@dwSpreaderId
	IF @ResultCout=0--IF @@ROWCOUNT=0
	BEGIN
		SET @strErrorDescribe=N'您输入的推荐码无效'
		--SET @strErrorDescribe=N'您输入的推荐人'+convert(varchar,@dwSpreaderId)+N'不存在或者不是有效的推荐人！'
		RETURN 6
	END

	-- 推广提成
	DECLARE @RegisterGrantScore INT
	DECLARE @Note NVARCHAR(512)
	SET @Note = N'绑定'--mChen edit,N'注册'
	SELECT @RegisterGrantScore = RegisterGrantScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GlobalSpreadInfo
	IF @RegisterGrantScore IS NULL
	BEGIN
		SET @RegisterGrantScore=8	--5000 --mChen edit
	END
	
	--mChen comment？To do
	INSERT INTO QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordSpreadInfo(
		UserID,InsureScore,TypeID,ChildrenID,CollectNote)
	VALUES(@dwSpreaderId,@RegisterGrantScore,1,@dwUserID,@Note)		
	
	UPDATE QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo SET InsureScore=InsureScore+@RegisterGrantScore	--mChen edit, SET Score=Score+@RegisterGrantScore
	WHERE UserID=@dwUserID
	
	--更新AccountsInfo
	UPDATE AccountsInfo SET SpreaderID=@dwSpreaderId WHERE UserID=@dwUserID
	
	--mChen comment
	/*
	UPDATE QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo SET Score=Score+@RegisterGrantScore
	WHERE UserID=@dwSpreaderId
	*/
	
	DECLARE @DestScore BIGINT
	--mChen edit
	SELECT @DestScore=InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID=@dwUserID
	--SELECT @DestScore=Score FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID=@dwUserID
	IF @DestScore IS NULL
	BEGIN
		SET @DestScore = 0
	END
	
	--mChen edit
	SET @strErrorDescribe=N'绑定邀请码成功，恭喜你获得 '+convert(varchar,@RegisterGrantScore)+N' 的钻石！'
	--SET @strErrorDescribe=N'设置推荐人成功，恭喜你获得 '+convert(varchar,@RegisterGrantScore)+N' 的金币！'
	
	SELECT @dwSpreaderId AS SpreaderID, @DestScore AS DestScore
		
END

RETURN 0

GO


--mChen add,增加/删除推荐人身份
----------------------------------------------------------------------------------------------------

-- 查询资料
CREATE PROC GSP_GP_AddDelSpreader
	@dwUserID INT,								-- 执行操作的用户 I D
	@strPassword NCHAR(32),						-- 用户密码
	
	@dwSpreaderId INT,							-- 推荐人ID：被操作者
	
	@strSpreaderRealName NVARCHAR(32),			-- 推荐人姓名
	--@strSpreaderIDCardNo NCHAR(32),			-- 推荐人身份证号
	@strSpreaderTelNum NVARCHAR(32),			
	@strSpreaderWeiXinAccount NVARCHAR(32),			
	
	@wSpreaderLevel SMALLINT,					-- 推荐人等级
	@dwParentSpreaderID INT,					-- 上级代理ID
		
	@bIsAddOperate BIT,							-- 增加/删除操作？
	@strClientIP NVARCHAR(15),					-- 连接地址
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

		-- 变量定义
	DECLARE @LogonPass AS NCHAR(32)

	-- 查询用户
	SELECT @LogonPass=LogonPass FROM AccountsInfo(NOLOCK) WHERE UserID=@dwUserID

	-- 密码判断
	IF @LogonPass<>@strPassword
	BEGIN
		SET @strErrorDescribe=N'您的用户密码不正确！'
		RETURN 1
	END
	
	-- 参数有效性检测
	IF @dwSpreaderId=0
	BEGIN
		SET @strErrorDescribe=N'代理人ID不能为空！'
		RETURN 2
	END
	IF @strSpreaderRealName=N''
	BEGIN
		SET @strErrorDescribe=N'代理人姓名不能为空！'
		RETURN 2
	END
	IF @strSpreaderTelNum=N''
	BEGIN
		SET @strErrorDescribe=N'代理人电话不能为空！'
		RETURN 2
	END
	IF @strSpreaderWeiXinAccount=N''
	BEGIN
		SET @strErrorDescribe=N'代理人微信号不能为空！'
		RETURN 2
	END
	/*
	IF @strSpreaderIDCardNo=N''
	BEGIN
		SET @strErrorDescribe=N'代理人身份证号不能为空！'
		RETURN 2
	END
	*/
	IF @dwParentSpreaderID=0
	BEGIN
		SET @strErrorDescribe=N'上级代理人ID不能为空！'
		RETURN 2
	END
	
	
	-- 查验操作人
	DECLARE @MySpreaderLevel AS INT
	SELECT @MySpreaderLevel=SpreaderLevel FROM SpreadersInfo WHERE SpreaderID=@dwUserID
	IF @MySpreaderLevel IS NULL
	BEGIN
		SET @strErrorDescribe=N'您不是代理人，没有权限执行此操作！strSpreaderRealName='+@strSpreaderRealName
		RETURN 3
	END
	IF @MySpreaderLevel>=@wSpreaderLevel
	BEGIN
		SET @strErrorDescribe=N'您的代理等级必须高于指定的代理等级，您不够权限执行此操作！'
		RETURN 3
	END
	
	
	-- 查验代理人
	DECLARE @ResultCout AS INT
	--DECLARE @IsSpreader AS BIT
	SELECT @ResultCout=count(*) FROM AccountsInfo WHERE UserID=@dwSpreaderId
	-- 结果处理
	IF @ResultCout=0 --IF @@ROWCOUNT=0
	BEGIN
		SET @strErrorDescribe=N'此代理人'+convert(varchar,@dwSpreaderId)+N'不存在！'
		RETURN 4
	END
	
	-- 查验上级推荐人
	DECLARE @ParentSpreaderLevel AS INT
	SELECT @ParentSpreaderLevel=SpreaderLevel FROM SpreadersInfo WHERE SpreaderID=@dwParentSpreaderID
	IF @ParentSpreaderLevel IS NULL
	BEGIN
		SET @strErrorDescribe=N'此上级代理不是代理人！'
		RETURN 5
	END
	IF @MySpreaderLevel>=@ParentSpreaderLevel AND @dwUserID<>@dwParentSpreaderID	--可以给自己加下面的代理
	BEGIN
		SET @strErrorDescribe=N'您的代理等级必须高于此上级代理等级，您不够权限执行此操作！'
		RETURN 5
	END
	IF @ParentSpreaderLevel>=@wSpreaderLevel
	BEGIN
		SET @strErrorDescribe=N'指定的代理等级必须低于上级代理等级！'
		RETURN 5
	END


	-- 增加/删除推荐人
	IF @bIsAddOperate<>0--bIsAddOperate
	BEGIN
		-- 增加推荐人
		
		--查询推荐人
		SELECT @ResultCout=count(*) FROM SpreadersInfo WHERE SpreaderID=@dwSpreaderId
		IF @ResultCout<>0--IF @@ROWCOUNT<>0
		BEGIN
			SET @strErrorDescribe=N'您要增加的代理已经是代理人！'
			RETURN 6
		END
		
		-- 查验已有代理人数是否达到上限
		DECLARE @ResultSpreaderCout AS INT
		--DECLARE @IsSpreader AS BIT
		SELECT @ResultSpreaderCout=count(*) FROM SpreadersInfo
		-- 结果处理
		IF @ResultSpreaderCout>=10000 
		BEGIN
			SET @strErrorDescribe=N'代理人数已达上限'+convert(varchar,@ResultSpreaderCout)+N'人，无法再增加！'
			RETURN 6
		END
		
		--执行增加操作
		INSERT INTO QPAccountsDB_HideSeekLink.QPAccountsDB_HideSeek.dbo.SpreadersInfo(
			SpreaderID,RealName,TelNum,WeiXinAccount,SpreaderLevel,ParentID)
		VALUES(@dwSpreaderId,@strSpreaderRealName,@strSpreaderTelNum,@strSpreaderWeiXinAccount,@wSpreaderLevel,@dwParentSpreaderID)
		
		--UPDATE AccountsInfo SET IsSpreader=1 WHERE UserID=@dwSpreaderId
			
		SET @strErrorDescribe=N'增加代理人成功!'
		
	END 
	ELSE
	BEGIN
		-- 删除推荐人
		
		--查询推荐人
		DECLARE @SpreaderLevelToDel AS INT
		DECLARE @ParentSpreaderId AS INT
		DECLARE @ExtraCash DECIMAL(16,4)	--获得的额外金额(元)：比如删除直属下级代理获得对方的剩余金额
		SELECT @SpreaderLevelToDel=SpreaderLevel,@ParentSpreaderId=ParentID,@ExtraCash=ExtraCash FROM SpreadersInfo 
		WHERE SpreaderID=@dwSpreaderId AND RealName=@strSpreaderRealName
		
		IF @SpreaderLevelToDel IS NULL
		BEGIN
			SET @strErrorDescribe=N'要删除的代理人已经不存在！'
			RETURN 7
		END	
		
		-- 查验操作人
		IF @MySpreaderLevel>=@SpreaderLevelToDel
		BEGIN
			SET @strErrorDescribe=N'您的代理等级不够删除该代理人！'
			RETURN 7
		END
		
		--计算该代理已提现金额(元)
		DECLARE @CashedOut DECIMAL(16,4)
		SELECT @CashedOut=SUM(Amount) FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordSpreaderCashout WHERE SpreaderID=@dwSpreaderId
		IF @CashedOut IS NULL
		BEGIN
			SET @CashedOut=0
		END	
	
		--将此代理的剩余提成余额转到父代理下面
		DECLARE @TotalGrant DECIMAL(16,4)					--总金额（元）
		DECLARE @TotalGrantOfChildrenBuy DECIMAL(16,4)		--名下用户充值获得的提成总额（元）
		DECLARE @TotalLeftCash DECIMAL(16,4)				--剩余金额（元）
		SELECT @TotalGrantOfChildrenBuy=SUM(Payment*PaymentGrantRate) FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordChildrenPayment WHERE RelatedSpreaderID=@dwSpreaderId 
		IF @TotalGrantOfChildrenBuy IS NULL
		BEGIN
			SET @TotalGrantOfChildrenBuy=0
		END
		SET @TotalGrant=@TotalGrantOfChildrenBuy+@ExtraCash
		SET @TotalLeftCash=@TotalGrant-@CashedOut
		-- 更新父代理的@ExtraCash
		UPDATE SpreadersInfo SET ExtraCash=ExtraCash+@TotalLeftCash
		WHERE SpreaderID=@ParentSpreaderId
		IF @@ROWCOUNT=0
		BEGIN
			SET @strErrorDescribe=N'剩余提成余额转到父代理 失败!'
			RETURN 8
		END
		
		
		--修改此代理的那些直属代理的ParentId为此代理的ParentId
		UPDATE SpreadersInfo SET ParentId=@ParentSpreaderId WHERE ParentId=@dwSpreaderId
		
		--修改所有绑定此代理的用户的绑定代理为此代理的ParentId
		UPDATE AccountsInfo SET SpreaderID=@ParentSpreaderId WHERE SpreaderID=@dwSpreaderId
		--UPDATE AccountsInfo SET IsSpreader=0 WHERE UserID=@dwSpreaderId
				
		--添加删除记录
		INSERT INTO QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordDelSpreader(OperatorUserID,SpreaderID,RealName,TelNum,WeiXinAccount,SpreaderLevel,ParentID,ExtraCash)
		VALUES(@dwUserID,@dwSpreaderId,@strSpreaderRealName,@strSpreaderTelNum,@strSpreaderWeiXinAccount,@SpreaderLevelToDel,@ParentSpreaderId,@ExtraCash)
		
		--执行删除操作
		--删除提现记录
		DELETE QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordSpreaderCashout WHERE SpreaderID=@dwSpreaderId
		--删除提成信息
		DELETE QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordChildrenPayment WHERE RelatedSpreaderID=@dwSpreaderId
		DELETE SpreadersInfo WHERE SpreaderID=@dwSpreaderId
		
		SET @strErrorDescribe=N'删除代理人成功!'
		
	END
	
	
	DECLARE @DestScore BIGINT
	SELECT @DestScore=InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID=@dwUserID
	IF @DestScore IS NULL
	BEGIN
		SET @DestScore = 0
	END
		
	SELECT @DestScore AS DestScore
		
END

RETURN 0

GO

----------------------------------------------------------------------------------------------------


--mChen add,查询代理人信息
----------------------------------------------------------------------------------------------------

-- 查询资料
CREATE PROC GSP_GP_QuerySpreadersInfo
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
	SELECT @LogonPass=LogonPass FROM AccountsInfo(NOLOCK) WHERE UserID=@dwUserID

	-- 密码判断
	IF @LogonPass<>@strPassword
	BEGIN
		SET @strErrorDescribe=N'您的用户密码不正确！'
		RETURN 1
	END
	
	-- 查验操作人
	DECLARE @MySpreaderLevel AS INT
	SELECT @MySpreaderLevel=SpreaderLevel FROM SpreadersInfo WHERE SpreaderID=@dwUserID
	IF @MySpreaderLevel IS NULL
	BEGIN
		SET @strErrorDescribe=N'您不是代理人，没有权限执行此操作！'
		RETURN 2
	END
	
	SET @strErrorDescribe=N'查询代理人信息成功!'
	
	-- 查询用户
	IF @MySpreaderLevel=0
	BEGIN
		SELECT * FROM SpreadersInfo
	END ELSE 
	IF @MySpreaderLevel=1
	BEGIN	
		SELECT * FROM SpreadersInfo WHERE ParentID=@dwUserID 
		or ParentID in (SELECT SpreaderID FROM SpreadersInfo WHERE ParentID=@dwUserID)
	END ELSE
	IF @MySpreaderLevel=2
	BEGIN	
		SELECT * FROM SpreadersInfo WHERE ParentID=@dwUserID 
	END ELSE
	IF @MySpreaderLevel=3
	BEGIN	
		SET @strErrorDescribe=N'您是三级代理，名下无子代理！'
		RETURN 3
	END ELSE
	BEGIN	
		SET @strErrorDescribe=N'你的代理等级不对！'
		RETURN 4
	END
	--SELECT * FROM SpreadersInfo
		
END

RETURN 0

GO

----------------------------------------------------------------------------------------------------
