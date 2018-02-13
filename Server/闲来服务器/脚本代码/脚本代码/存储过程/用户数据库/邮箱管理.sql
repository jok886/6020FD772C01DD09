
----------------------------------------------------------------------------------------------------

USE QPAccountsDB_HideSeek
GO


IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GP_AddMail]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[GSP_GP_AddMail]
GO


SET QUOTED_IDENTIFIER ON 
GO

SET ANSI_NULLS ON 
GO


----------------------------------------------------------------------------------------------------

-- 写邮件
CREATE PROC GSP_GP_AddMail
	@dwFromUserID INT,							-- 发件用户 I D，0表示系统
	@dwToUserID INT,							-- 收件用户 I D，0表示群发
	@strPassword NCHAR(32),						-- 发件用户密码
	@strMailTitle NVARCHAR(128),				-- 邮件标题
	@strMailContent NVARCHAR(512),				-- 邮件内容
	@strErrorDescribe NVARCHAR(127) OUTPUT		-- 输出信息
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

	-- 查询用户
	IF @dwFromUserID<>0
	BEGIN
		IF not exists(SELECT * FROM AccountsInfo WHERE UserID=@dwFromUserID AND LogonPass=@strPassword)
		BEGIN
			SET @strErrorDescribe = N'抱歉，发件用户信息不存在或者密码不正确！'
			return 1
		END
	END

	--插入数据
	INSERT INTO SystemMailbox VALUES(@dwFromUserID,@dwToUserID,@strMailTitle,@strMailContent,GetDate(),0)		
		
	SET @strErrorDescribe = N'写邮件成功!'

END

RETURN 0

GO

----------------------------------------------------------------------------------------------------