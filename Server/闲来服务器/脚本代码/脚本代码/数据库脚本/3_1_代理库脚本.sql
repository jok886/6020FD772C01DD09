USE [QPAccountsDB_HideSeek]
GO

/****** 对象:  Table [dbo].[SpreaderStatusInfo]    脚本日期: 09/12/2017 11:16:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SpreaderStatusInfo](
	[StatusName] [nvarchar](32) NOT NULL,
	[StatusValue] [int] NOT NULL CONSTRAINT [DF_SpreaderStatusInfo_StatusValue]  DEFAULT ((0)),
	[StatusString] [nvarchar](512) NOT NULL CONSTRAINT [DF_SpreaderStatusInfo_StatusString]  DEFAULT (''),
	[StatusTip] [nvarchar](50) NOT NULL CONSTRAINT [DF_SpreaderStatusInfo_StatusTip]  DEFAULT (''),
	[StatusDescription] [nvarchar](100) NOT NULL CONSTRAINT [DF_SpreaderStatusInfo_StatusDescription]  DEFAULT (''),
 CONSTRAINT [PK_SpreaderStatusInfo] PRIMARY KEY CLUSTERED 
(
	[StatusName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态名字' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreaderStatusInfo', @level2type=N'COLUMN',@level2name=N'StatusName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态数值' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreaderStatusInfo', @level2type=N'COLUMN',@level2name=N'StatusValue'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态字符' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreaderStatusInfo', @level2type=N'COLUMN',@level2name=N'StatusString'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态显示名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreaderStatusInfo', @level2type=N'COLUMN',@level2name=N'StatusTip'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'字符的描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreaderStatusInfo', @level2type=N'COLUMN',@level2name=N'StatusDescription'
GO

--mChen add, 增加代理请求记录
/****** 对象:  Table [dbo].[RecordAddSpreaderRequest]    脚本日期: 06/23/2017 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecordAddSpreaderRequest](
	[RecordID] [int] IDENTITY(1,1) NOT NULL,
	[RequestUserID] [int] NOT NULL,
	
	[SpreaderID] [int] NOT NULL,
	[RealName] [nvarchar](32) NOT NULL CONSTRAINT [DF_RecordAddSpreaderRequest_Nickname]  DEFAULT (''),
	[TelNum] [nvarchar](32) NOT NULL CONSTRAINT [DF_RecordAddSpreaderRequest_TellNum]  DEFAULT (''),
	[WeiXinAccount] [nvarchar](32) NOT NULL CONSTRAINT [DF_RecordAddSpreaderRequest_WeiXinAccount]  DEFAULT (''),
	[IDCardNo] [nchar](32) NOT NULL CONSTRAINT [DF_RecordAddSpreaderRequest_IDCardNo]  DEFAULT ('0'),
	[SpreaderLevel] [smallint] NOT NULL CONSTRAINT [DF_RecordAddSpreaderRequest_SpreadLevel]  DEFAULT ((3)),
	[ParentID] [int] NOT NULL,
	[RequestDate] [datetime] NOT NULL CONSTRAINT [DF_RecordAddSpreaderRequest_RequestDate]  DEFAULT (getdate()),
 CONSTRAINT [PK_RecordAddSpreaderRequest] PRIMARY KEY CLUSTERED 
(
	[SpreaderID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'发起请求的人的ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RecordAddSpreaderRequest', @level2type=N'COLUMN',@level2name=N'RequestUserID'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RecordAddSpreaderRequest', @level2type=N'COLUMN',@level2name=N'SpreaderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人姓名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RecordAddSpreaderRequest', @level2type=N'COLUMN',@level2name=N'RealName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人电话' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RecordAddSpreaderRequest', @level2type=N'COLUMN',@level2name=N'TelNum'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人微信号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RecordAddSpreaderRequest', @level2type=N'COLUMN',@level2name=N'WeiXinAccount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人身份证号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RecordAddSpreaderRequest', @level2type=N'COLUMN',@level2name=N'IDCardNo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人等级' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RecordAddSpreaderRequest', @level2type=N'COLUMN',@level2name=N'SpreaderLevel'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'上级代理ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RecordAddSpreaderRequest', @level2type=N'COLUMN',@level2name=N'ParentID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'请求日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RecordAddSpreaderRequest', @level2type=N'COLUMN',@level2name=N'RequestDate'
GO

--mChen add, for代理
/****** 对象:  Table [dbo].[SpreadersInfo]    脚本日期: 06/23/2017 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SpreadersInfo](
	[SpreaderID] [int] NOT NULL,
	[RealName] [nvarchar](32) NOT NULL CONSTRAINT [DF_SpreadersInfo_Nickname]  DEFAULT (''),
	[TelNum] [nvarchar](32) NOT NULL CONSTRAINT [DF_SpreadersInfo_TellNum]  DEFAULT (''),
	[WeiXinAccount] [nvarchar](32) NOT NULL CONSTRAINT [DF_SpreadersInfo_WeiXinAccount]  DEFAULT (''),
	[IDCardNo] [nchar](32) NOT NULL CONSTRAINT [DF_SpreadersInfo_IDCardNo]  DEFAULT ('0'),
	[SpreaderLevel] [smallint] NOT NULL CONSTRAINT [DF_SpreadersInfo_SpreadLevel]  DEFAULT ((3)),
	[ParentID] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL CONSTRAINT [DF_SpreadersInfo_CreateDate]  DEFAULT (getdate()),
	--[CashedOut] [decimal](16, 4) NOT NULL CONSTRAINT [DF_SpreadersInfo_CashedOut]  DEFAULT ((0)),
	[ExtraCash] [decimal](16, 4) NOT NULL CONSTRAINT [DF_SpreadersInfo_ExtraCash]  DEFAULT ((0)),
 CONSTRAINT [PK_SpreadersInfo] PRIMARY KEY CLUSTERED 
(
	[SpreaderID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreadersInfo', @level2type=N'COLUMN',@level2name=N'SpreaderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人姓名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreadersInfo', @level2type=N'COLUMN',@level2name=N'RealName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人电话' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreadersInfo', @level2type=N'COLUMN',@level2name=N'TelNum'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人微信号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreadersInfo', @level2type=N'COLUMN',@level2name=N'WeiXinAccount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人身份证号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreadersInfo', @level2type=N'COLUMN',@level2name=N'IDCardNo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代理人等级' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreadersInfo', @level2type=N'COLUMN',@level2name=N'SpreaderLevel'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'上级代理ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreadersInfo', @level2type=N'COLUMN',@level2name=N'ParentID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreadersInfo', @level2type=N'COLUMN',@level2name=N'CreateDate'
GO
--EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'已提现金额(元)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreadersInfo', @level2type=N'COLUMN',@level2name=N'CashedOut'
--GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'获得的额外金额(元)：比如删除直属下级代理获得对方的剩余金额' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SpreadersInfo', @level2type=N'COLUMN',@level2name=N'ExtraCash'
GO


