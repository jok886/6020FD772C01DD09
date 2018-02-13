
----------------------------------------------------------------------------------------------------

USE QPAccountsDB_HideSeek
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GP_EfficacyAccounts]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GP_EfficacyAccounts]
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_MB_EfficacyAccounts]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_MB_EfficacyAccounts]
GO

SET QUOTED_IDENTIFIER ON 
GO

SET ANSI_NULLS ON 
GO

----------------------------------------------------------------------------------------------------

-- 帐号登录
CREATE PROC GSP_GP_EfficacyAccounts
	@strAccounts NVARCHAR(31),					-- 用户帐号
	@strUid NVARCHAR(49),
	@strOpenid NVARCHAR(31),
	@strPassword NCHAR(32),						-- 用户密码
	@strClientIP NVARCHAR(15),					-- 连接地址
	@strMachineID NVARCHAR(32),					-- 机器标识
	@nNeeValidateMBCard BIT,					-- 密保校验
	@wKindID SMALLINT,							-- 游戏 I D		mChen add, for Match Time
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 基本信息
DECLARE @UserID INT
DECLARE @CustomID INT
DECLARE @FaceID SMALLINT
DECLARE @Accounts NVARCHAR(31)
DECLARE @NickName NVARCHAR(31)
DECLARE @UnderWrite NVARCHAR(63)
DECLARE @SpreaderID INT
DECLARE @PlayTimeCount INT

-- 积分变量
DECLARE @Score BIGINT
DECLARE @Insure BIGINT

-- 扩展信息
DECLARE @GameID INT
DECLARE @Gender TINYINT
DECLARE @UserMedal INT
DECLARE @Experience INT
DECLARE @LoveLiness INT
DECLARE @InsureEnabled INT
DECLARE @MemberOrder SMALLINT
DECLARE @MemberOverDate DATETIME
DECLARE @MemberSwitchDate DATETIME
DECLARE @ProtectID INT
DECLARE @PasswordID INT

-- 辅助变量
DECLARE @EnjoinLogon AS INT

-- 执行逻辑
BEGIN
	-- 系统暂停
	SELECT @EnjoinLogon=StatusValue FROM SystemStatusInfo(NOLOCK) WHERE StatusName=N'EnjoinLogon'
	IF @EnjoinLogon IS NOT NULL AND @EnjoinLogon<>0
	BEGIN
		SELECT @strErrorDescribe=StatusString FROM SystemStatusInfo(NOLOCK) WHERE StatusName=N'EnjoinLogon'
		RETURN 2
	END

	-- 效验地址
	SELECT @EnjoinLogon=EnjoinLogon FROM ConfineAddress(NOLOCK) WHERE AddrString=@strClientIP AND (EnjoinOverDate>GETDATE() OR EnjoinOverDate IS NULL)
	IF @EnjoinLogon IS NOT NULL AND @EnjoinLogon<>0
	BEGIN
		SET @strErrorDescribe=N'抱歉地通知您，系统禁止了您所在的 IP 地址的登录功能，请联系客户服务中心了解详细情况！'
		RETURN 4
	END
	
	-- 效验机器
	SELECT @EnjoinLogon=EnjoinLogon FROM ConfineMachine(NOLOCK) WHERE MachineSerial=@strMachineID AND (EnjoinOverDate>GETDATE() OR EnjoinOverDate IS NULL)
	IF @EnjoinLogon IS NOT NULL AND @EnjoinLogon<>0
	BEGIN
		SET @strErrorDescribe=N'抱歉地通知您，系统禁止了您的机器的登录功能，请联系客户服务中心了解详细情况！'
		RETURN 7
	END
	
	-- mChen add
	-- 统一使用Uid------------------------
	DECLARE @UserIDOfUid INT
	DECLARE @UserIDOfOpenid INT
	DECLARE @strDescribe NVARCHAR(128)
	SELECT @UserIDOfUid=UserID FROM AccountsInfo(NOLOCK) WHERE Accounts=@strUid
	SELECT @UserIDOfOpenid=UserID FROM AccountsInfo(NOLOCK) WHERE Accounts=@strOpenid
	
	-- 更新Unionid和Openid列
	IF @UserIDOfUid IS NOT NULL
	BEGIN
		UPDATE AccountsInfo SET Unionid=@strUid,Openid=@strOpenid WHERE UserID=@UserIDOfUid
	END
	IF @UserIDOfOpenid IS NOT NULL
	BEGIN
		UPDATE AccountsInfo SET Unionid=@strUid,Openid=@strOpenid WHERE UserID=@UserIDOfOpenid
	END
	
	IF @UserIDOfUid IS NOT NULL AND @UserIDOfOpenid IS NOT NULL
	BEGIN
		-- 2个Accounts账号（iOS, Android）
		
		DECLARE @SpreaderLevelOfUid INT
		DECLARE @SpreaderLevelOfOpenid INT
		SELECT @SpreaderLevelOfUid=SpreaderLevel FROM SpreadersInfo WHERE SpreaderID=@UserIDOfUid
		SELECT @SpreaderLevelOfOpenid=SpreaderLevel FROM SpreadersInfo WHERE SpreaderID=@UserIDOfOpenid
		IF @SpreaderLevelOfUid IS NOT NULL AND @SpreaderLevelOfOpenid IS NOT NULL
		BEGIN
			-- 2个都是代理人
			-- 待处理
			SET @strDescribe=N'2个都是代理人'
		END ELSE
		IF @SpreaderLevelOfUid IS NOT NULL 
		BEGIN
			-- Uid是代理人，Openid不是代理人
			-- 直接删除Openid那人就行
			DELETE AccountsInfo WHERE UserID=@UserIDOfOpenid
		END ELSE
		IF @SpreaderLevelOfOpenid IS NOT NULL 
		BEGIN
			-- Openid是代理人，Uid不是代理人
			-- 更新Openid那人的Accounts为Uid，删除Uid那人
			UPDATE AccountsInfo SET Accounts=@strUid,RegAccounts=@strUid,Unionid=@strUid,Openid=@strOpenid WHERE UserID=@UserIDOfOpenid
			DELETE AccountsInfo WHERE UserID=@UserIDOfUid
		END ELSE
		BEGIN
			-- 两个都不是代理人
			-- 直接删除Openid那人就行
			DELETE AccountsInfo WHERE UserID=@UserIDOfOpenid
		END
		
	END ELSE
	IF @UserIDOfOpenid IS NOT NULL
	BEGIN
		-- 只有1个Openid的Accounts账号（Android）
		-- 不管有没有代理账号，只要更新Accounts为Uid就行了
		UPDATE AccountsInfo SET Accounts=@strUid,RegAccounts=@strUid,Unionid=@strUid,Openid=@strOpenid WHERE UserID=@UserIDOfOpenid
	END
	/*
	ELSE
	IF @UserIDOfUid IS NOT NULL
	BEGIN
		-- 只有1个Uid的Accounts账号（iOS）
		-- 不用管
	END ELSE
	BEGIN
		-- 没有账号（新用户）
		-- 不用管
	END
	*/
	---------------------------
 
	-- 查询用户
	DECLARE @Nullity TINYINT
	DECLARE @StunDown TINYINT
	DECLARE @LogonPass AS NCHAR(32)
	DECLARE	@MachineSerial NCHAR(32)
	DECLARE	@strInsurePass NCHAR(32)
	DECLARE @MoorMachine AS TINYINT
	SELECT @UserID=UserID, @GameID=GameID, @Accounts=Accounts, @NickName=NickName, @UnderWrite=UnderWrite, @LogonPass=LogonPass,@strInsurePass=InsurePass,
		@FaceID=FaceID, @CustomID=CustomID, @Gender=Gender, @Nullity=Nullity, @StunDown=StunDown, @UserMedal=UserMedal, @Experience=Experience,
		@LoveLiness=LoveLiness, @MemberOrder=MemberOrder, @MemberOverDate=MemberOverDate, @MemberSwitchDate=MemberSwitchDate,
		@MoorMachine=MoorMachine, @MachineSerial=LastLogonMachine,@SpreaderID=SpreaderID,@PlayTimeCount=PlayTimeCount,@PasswordID=PasswordID,@ProtectID=ProtectID
	FROM AccountsInfo(NOLOCK) WHERE Accounts=@strAccounts

	-- 查询用户
	IF @UserID IS NULL
	BEGIN
		SET @strErrorDescribe=N'您的帐号不存在或者密码输入有误，请查证后再次尝试登录！'
		RETURN 1
	END	

	-- 帐号禁止
	IF @Nullity<>0
	BEGIN
		SET @strErrorDescribe=N'您的帐号暂时处于冻结状态，请联系客户服务中心了解详细情况！'
		RETURN 2
	END	

	-- 帐号关闭
	IF @StunDown<>0
	BEGIN
		SET @strErrorDescribe=N'您的帐号使用了安全关闭功能，必须重新开通后才能继续使用！'
		RETURN 2
	END	
	
	-- 固定机器
	IF @MoorMachine=1
	BEGIN
		IF @MachineSerial<>@strMachineID
		BEGIN
			SET @strErrorDescribe=N'您的帐号使用固定机器登录功能，您现所使用的机器不是所指定的机器！'
			RETURN 1
		END
	END

	-- 密码判断
	IF @LogonPass<>@strPassword
	BEGIN
		SET @strErrorDescribe=N'您的帐号不存在或者密码输入有误，请查证后再次尝试登录！'
		RETURN 3
	END

	-- 密保校验
	IF @nNeeValidateMBCard=1 AND @PasswordID<>0
	BEGIN
		SELECT @PasswordID AS PasswordID
		RETURN 18
	END

	-- 固定机器
	IF @MoorMachine=2
	BEGIN
		SET @MoorMachine=1
		SET @strErrorDescribe=N'您的帐号成功使用了固定机器登录功能！'
		UPDATE AccountsInfo SET MoorMachine=@MoorMachine, LastLogonMachine=@strMachineID WHERE UserID=@UserID
	END

	-- 推广员提成
	IF @SpreaderID<>0 
	BEGIN
		DECLARE @GrantTime	INT
		DECLARE @GrantScore	BIGINT
		DECLARE @Note NVARCHAR(512)
		SET @Note = N'游戏时长达标一次性奖励'

		SELECT @GrantTime=PlayTimeCount,@GrantScore=PlayTimeGrantScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GlobalSpreadInfo
		WHERE ID=1
		IF @GrantTime IS NULL OR @GrantTime=0
		BEGIN
			SET @GrantTime = 108000 -- 30小时
			SET @GrantScore = 200000
		END			
		IF @PlayTimeCount>=@GrantTime
		BEGIN
			-- 获取提成信息
			DECLARE @RecordID INT
			SELECT @RecordID=RecordID FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordSpreadInfo
			WHERE UserID = @SpreaderID AND ChildrenID = @UserID AND TypeID = 2
			
			IF @RecordID IS NULL
			BEGIN
				INSERT INTO QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordSpreadInfo(
					UserID,Score,TypeID,ChildrenID,CollectNote)
				VALUES(@SpreaderID,@GrantScore,2,@UserID,@Note)	
			END		
		END
	END

	-- 查询金币
	SELECT @Score=Score, @Insure=InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID=@UserID

	-- 数据调整
	IF @Score IS NULL SET @Score=0
	IF @Insure IS NULL SET @Insure=0

	-- 会员等级
	IF @MemberOrder<>0 AND GETDATE()>@MemberSwitchDate
	BEGIN
		DECLARE @UserRight INT	
		SET @UserRight=0
		
		-- 删除会员
		DELETE AccountsMember WHERE UserID=@UserID AND MemberOverDate<=GETDATE()

		-- 搜索会员
		SELECT @MemberOverDate=MAX(MemberOverDate), @MemberOrder=MAX(MemberOrder), @MemberSwitchDate=MIN(MemberOverDate)
			FROM AccountsMember(NOLOCK) WHERE UserID=@UserID

		-- 数据调整
		IF @MemberOrder IS NULL 
		BEGIN
			SET @MemberOrder=0
			SET @UserRight=512
		END
		IF @MemberOverDate IS NULL SET @MemberOverDate='1980-1-1'
		IF @MemberSwitchDate IS NULL SET @MemberSwitchDate='1980-1-1'

		-- 更新数据
		UPDATE AccountsInfo SET MemberOrder=@MemberOrder, MemberOverDate=@MemberOverDate, MemberSwitchDate=@MemberSwitchDate,
			UserRight=UserRight&~@UserRight WHERE UserID=@UserID
	END

	-- 更新信息
	UPDATE AccountsInfo SET GameLogonTimes=GameLogonTimes+1,LastLogonDate=GETDATE(), LastLogonIP=@strClientIP,
		LastLogonMachine=@strMachineID WHERE UserID=@UserID
		
	SET @InsureEnabled=1
	IF @strInsurePass = N'' SET @InsureEnabled=0

	-- 记录日志
	DECLARE @DateID INT
	SET @DateID=CAST(CAST(GETDATE() AS FLOAT) AS INT)
	UPDATE SystemStreamInfo SET GameLogonSuccess=GameLogonSuccess+1 WHERE DateID=@DateID
	IF @@ROWCOUNT=0 INSERT SystemStreamInfo (DateID, GameLogonSuccess) VALUES (@DateID, 1)

	--mChen add, for Match Time
	DECLARE @MatchStartTime datetime
	DECLARE	@MatchEndTime datetime
	SELECT @MatchStartTime = MatchStartTime, @MatchEndTime = MatchEndTime FROM QPPlatformDB_HideSeekLink.QPPlatformDB_HideSeek.dbo.PrivateInfo WHERE KindID=@wKindID
	
	--mChen add, for签到记录
	DECLARE @wSeriesDate INT	
	SELECT @wSeriesDate=SeriesDate FROM AccountsSignin WHERE UserID=@UserID
	IF @wSeriesDate IS NULL
	BEGIN
		SET @wSeriesDate = 0	
	END
	
	--mChen add:查询用户已打场次,for抽奖
	DECLARE @PlayCount INT
	SELECT  @PlayCount=COUNT(*) FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.PrivateGameRecordUserRecordID WHERE UserID = @UserID
	
	--mChen add:查询抽奖记录
	DECLARE @RaffleCount INT	
	SELECT @RaffleCount=RaffleCount FROM AccountsRaffle WHERE UserID=@UserID
	IF @RaffleCount IS NULL
	BEGIN
		SET @RaffleCount = 0
	END
	-- 每打完多少场可以抽奖一次
	DECLARE @PlayCountPerRaffle INT
	SELECT @PlayCountPerRaffle=StatusValue FROM SystemStatusInfo WHERE StatusName=N'PlayCountPerRaffle'
	IF @PlayCountPerRaffle IS NULL
	BEGIN
		SET @PlayCountPerRaffle = 20
	END
	
	--mChen add:查询代理等级
	DECLARE @SpreaderLevel AS INT
	SELECT @SpreaderLevel=SpreaderLevel FROM SpreadersInfo WHERE SpreaderID=@UserID
	IF @SpreaderLevel IS NULL
	BEGIN
		SET @SpreaderLevel=-1
	END
	
	--mChen add:查询公告信息
	DECLARE @strPublicNotice NVARCHAR(256)
	SELECT @strPublicNotice=StatusString FROM SystemStatusInfo(NOLOCK) WHERE StatusName=N'PublicNotice'
	IF @strPublicNotice IS NULL
	BEGIN
		SET @strPublicNotice=N''
	END
	
	--mChen add, for HideSeek:查询警察模型库
	DECLARE @lModelIndex0 BIGINT
	SELECT @lModelIndex0=ModelIndex0 FROM AccountsTaggerModel(NOLOCK) WHERE UserID=@UserID
	IF @lModelIndex0 IS NULL
	BEGIN
		SET @lModelIndex0=1
		INSERT INTO AccountsTaggerModel(UserID,ModelIndex0)
		VALUES(@UserID,@lModelIndex0)	
	END
	
	--WQ 查询用户头像
	DECLARE @strHeadHttp NVARCHAR(256)
	SELECT @strHeadHttp=HeadHttp FROM IndividualDatum WHERE UserID=@UserID
	IF @strHeadHttp IS NULL
	BEGIN
		SET @strHeadHttp=N'0'
	END
	
	-- 输出变量
	SELECT @UserID AS UserID, @MatchStartTime AS MatchStartTime, @MatchEndTime AS MatchEndTime, 
		@wSeriesDate AS SeriesDate, @PlayCount AS PlayCount, 
		@RaffleCount AS RaffleCount, @PlayCountPerRaffle AS PlayCountPerRaffle,
		@SpreaderLevel AS SpreaderLevel,
		@strPublicNotice AS PublicNotice,
		@lModelIndex0 AS ModelIndex0,
		@GameID AS GameID, @Accounts AS Accounts, @NickName AS NickName, @UnderWrite AS UnderWrite,
		@FaceID AS FaceID, @CustomID AS CustomID, @Gender AS Gender, @UserMedal AS UserMedal, @Experience AS Experience,
		@Score AS Score, @Insure AS Insure, @LoveLiness AS LoveLiness, @MemberOrder AS MemberOrder, @MemberOverDate AS MemberOverDate,
		@MoorMachine AS MoorMachine,@PasswordID as PasswordID,@SpreaderID as SpreaderID,@InsureEnabled as InsureEnabled,
		@strHeadHttp AS HeadHttp

END

RETURN 0

GO

----------------------------------------------------------------------------------------------------

-- 帐号登录
CREATE PROC GSP_MB_EfficacyAccounts
	@strAccounts NVARCHAR(31),					-- 用户帐号
	@strPassword NCHAR(32),						-- 用户密码
	@strClientIP NVARCHAR(15),					-- 连接地址
	@strMachineID NVARCHAR(32),					-- 机器标识
	@strMobilePhone NVARCHAR(11),				-- 手机号码
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 基本信息
DECLARE @UserID INT
DECLARE @CustomID INT
DECLARE @FaceID SMALLINT
DECLARE @Accounts NVARCHAR(31)
DECLARE @NickName NVARCHAR(31)
DECLARE @UnderWrite NVARCHAR(63)
DECLARE @SpreaderID INT
DECLARE @PlayTimeCount INT

-- 积分变量
DECLARE @Score BIGINT
DECLARE @Insure BIGINT

-- 扩展信息
DECLARE @GameID INT
DECLARE @Gender TINYINT
DECLARE @UserMedal INT
DECLARE @Experience INT
DECLARE @LoveLiness INT
DECLARE @InsureEnabled INT
DECLARE @MemberOrder SMALLINT
DECLARE @MemberOverDate DATETIME
DECLARE @MemberSwitchDate DATETIME

-- 辅助变量
DECLARE @EnjoinLogon AS INT

-- 执行逻辑
BEGIN
	-- 系统暂停
	SELECT @EnjoinLogon=StatusValue FROM SystemStatusInfo(NOLOCK) WHERE StatusName=N'EnjoinLogon'
	IF @EnjoinLogon IS NOT NULL AND @EnjoinLogon<>0
	BEGIN
		SELECT @strErrorDescribe=StatusString FROM SystemStatusInfo(NOLOCK) WHERE StatusName=N'EnjoinLogon'
		RETURN 2
	END

	-- 效验地址
	SELECT @EnjoinLogon=EnjoinLogon FROM ConfineAddress(NOLOCK) WHERE AddrString=@strClientIP AND GETDATE()<EnjoinOverDate
	IF @EnjoinLogon IS NOT NULL AND @EnjoinLogon<>0
	BEGIN
		SET @strErrorDescribe=N'抱歉地通知您，系统禁止了您所在的 IP 地址的登录功能，请联系客户服务中心了解详细情况！'
		RETURN 4
	END
	
	-- 效验机器
	SELECT @EnjoinLogon=EnjoinLogon FROM ConfineMachine(NOLOCK) WHERE MachineSerial=@strMachineID AND GETDATE()<EnjoinOverDate
	IF @EnjoinLogon IS NOT NULL AND @EnjoinLogon<>0
	BEGIN
		SET @strErrorDescribe=N'抱歉地通知您，系统禁止了您的机器的登录功能，请联系客户服务中心了解详细情况！'
		RETURN 7
	END

	-- 查询用户
	DECLARE @Nullity TINYINT
	DECLARE @StunDown TINYINT
	DECLARE @LogonPass AS NCHAR(32)
	DECLARE @strInsurePass AS NCHAR(32)
	DECLARE @MoorMachine AS TINYINT
	SELECT @UserID=UserID, @GameID=GameID, @Accounts=Accounts, @NickName=NickName, @UnderWrite=UnderWrite, @LogonPass=LogonPass,@strInsurePass=InsurePass,
		@FaceID=FaceID, @CustomID=CustomID, @Gender=Gender, @Nullity=Nullity, @StunDown=StunDown, @UserMedal=UserMedal, @Experience=Experience,
		@LoveLiness=LoveLiness, @MemberOrder=MemberOrder, @MemberOverDate=MemberOverDate, @MemberSwitchDate=MemberSwitchDate,
		@MoorMachine=MoorMachine,@SpreaderID=SpreaderID,@PlayTimeCount=PlayTimeCount
	FROM AccountsInfo(NOLOCK) WHERE Accounts=@strAccounts

	-- 查询用户
	IF @UserID IS NULL
	BEGIN
		SET @strErrorDescribe=N'您的帐号不存在或者密码输入有误，请查证后再次尝试登录！'
		RETURN 1
	END	

	-- 帐号禁止
	IF @Nullity<>0
	BEGIN
		SET @strErrorDescribe=N'您的帐号暂时处于冻结状态，请联系客户服务中心了解详细情况！'
		RETURN 2
	END	

	-- 帐号关闭
	IF @StunDown<>0
	BEGIN
		SET @strErrorDescribe=N'您的帐号使用了安全关闭功能，必须重新开通后才能继续使用！'
		RETURN 2
	END	
	
	-- 固定机器
	IF @MoorMachine <> 0
	BEGIN
		SET @strErrorDescribe=N'您的帐号使用固定机器登录功能，您不能使用手机终端进行登录！'
		RETURN 1
	END

	-- 密码判断
	IF @LogonPass<>@strPassword
	BEGIN
		SET @strErrorDescribe=N'您的帐号不存在或者密码输入有误，请查证后再次尝试登录！'
		RETURN 3
	END

	-- 推广员提成
	IF @SpreaderID<>0 
	BEGIN
		DECLARE @GrantTime	INT
		DECLARE @GrantScore	BIGINT
		DECLARE @Note NVARCHAR(512)
		SET @Note = N'游戏时长达标一次性奖励'

		SELECT @GrantTime=PlayTimeCount,@GrantScore=PlayTimeGrantScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GlobalSpreadInfo
		WHERE ID=1
		IF @GrantTime IS NULL OR @GrantTime=0
		BEGIN
			SET @GrantTime = 108000 -- 30小时
			SET @GrantScore = 200000
		END			
		IF @PlayTimeCount>=@GrantTime
		BEGIN
			-- 获取提成信息
			DECLARE @RecordID INT
			SELECT @RecordID=RecordID FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordSpreadInfo
			WHERE UserID = @SpreaderID AND ChildrenID = @UserID AND TypeID = 2
			IF @RecordID IS NULL
			BEGIN
				INSERT INTO QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.RecordSpreadInfo(
					UserID,Score,TypeID,ChildrenID,CollectNote)
				VALUES(@SpreaderID,@GrantScore,2,@UserID,@Note)	
			END		
		END
	END

	-- 查询金币
	SELECT @Score=Score, @Insure=InsureScore FROM QPTreasureDB_HideSeekLink.QPTreasureDB_HideSeek.dbo.GameScoreInfo WHERE UserID=@UserID

	-- 数据调整
	IF @Score IS NULL SET @Score=0
	IF @Insure IS NULL SET @Insure=0

	-- 会员等级
	IF @MemberOrder<>0 AND GETDATE()>@MemberSwitchDate
	BEGIN
		DECLARE @UserRight INT	
		SET @UserRight=0
		
		-- 删除会员
		DELETE AccountsMember WHERE UserID=@UserID AND MemberOverDate<=GETDATE()

		-- 搜索会员
		SELECT @MemberOverDate=MAX(MemberOverDate), @MemberOrder=MAX(MemberOrder), @MemberSwitchDate=MIN(MemberOverDate)
			FROM AccountsMember(NOLOCK) WHERE UserID=@UserID

		-- 数据调整
		IF @MemberOrder IS NULL 
		BEGIN
			SET @MemberOrder=0
			SET @UserRight=512
		END
		IF @MemberOverDate IS NULL SET @MemberOverDate='1980-1-1'
		IF @MemberSwitchDate IS NULL SET @MemberSwitchDate='1980-1-1'

		-- 更新数据
		UPDATE AccountsInfo SET MemberOrder=@MemberOrder, MemberOverDate=@MemberOverDate, MemberSwitchDate=@MemberSwitchDate,
			UserRight=UserRight&~@UserRight WHERE UserID=@UserID
	END
	
	SET @InsureEnabled=1
	IF @strInsurePass = N'' SET @InsureEnabled=0

	-- 更新信息
	UPDATE AccountsInfo SET GameLogonTimes=GameLogonTimes+1, LastLogonDate=GETDATE(), LastLogonIP=@strClientIP,
		LastLogonMachine=@strMachineID, LastLogonMobile=@strMobilePhone WHERE UserID=@UserID

	-- 记录日志
	DECLARE @DateID INT
	SET @DateID=CAST(CAST(GETDATE() AS FLOAT) AS INT)
	UPDATE SystemStreamInfo SET GameLogonSuccess=GameLogonSuccess+1 WHERE DateID=@DateID
	IF @@ROWCOUNT=0 INSERT SystemStreamInfo (DateID, GameLogonSuccess) VALUES (@DateID, 1)

	-- 输出变量
	SELECT @UserID AS UserID, @GameID AS GameID, @Accounts AS Accounts, @NickName AS NickName, @UnderWrite AS UnderWrite,
		@FaceID AS FaceID, @CustomID AS CustomID, @Gender AS Gender, @UserMedal AS UserMedal, @Experience AS Experience,
		@Score AS Score, @Insure AS Insure, @LoveLiness AS LoveLiness, @MemberOrder AS MemberOrder, @MemberOverDate AS MemberOverDate,
		@MoorMachine AS MoorMachine,@SpreaderID AS SpreaderID,@InsureEnabled AS InsureEnabled

END

RETURN 0

GO

----------------------------------------------------------------------------------------------------
