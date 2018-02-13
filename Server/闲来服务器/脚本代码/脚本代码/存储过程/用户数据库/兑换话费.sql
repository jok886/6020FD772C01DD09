
----------------------------------------------------------------------------------------------------

USE QPAccountsDB_HideSeek
GO

IF EXISTS (SELECT * FROM DBO.SYSOBJECTS WHERE ID = OBJECT_ID(N'[dbo].[GSP_GP_GetExchangeHuaFei]') and OBJECTPROPERTY(ID, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].GSP_GP_GetExchangeHuaFei
GO


SET QUOTED_IDENTIFIER ON 
GO

SET ANSI_NULLS ON 
GO

----------------------------------------------------------------------------------------------------

-- 查询资料
CREATE PROC GSP_GP_GetExchangeHuaFei
	
WITH ENCRYPTION AS

-- 属性设置
SET NOCOUNT ON

-- 执行逻辑
BEGIN

SELECT * FROM ExchangeHuaFeiList
		
END

RETURN 0

GO