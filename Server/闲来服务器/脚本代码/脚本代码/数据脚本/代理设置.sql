use QPAccountsDB_HideSeek

Truncate table [dbo].[SpreaderStatusInfo]

INSERT [dbo].[SpreaderStatusInfo] ([StatusName], [StatusValue], [StatusString], [StatusTip], [StatusDescription]) VALUES (N'MinCashoutValue', 1, N'最小提现金额，单位元', N'代理提现', N'')
INSERT [dbo].[SpreaderStatusInfo] ([StatusName], [StatusValue], [StatusString], [StatusTip], [StatusDescription]) VALUES (N'MaxCashoutValue', 500, N'最大提现金额，单位元', N'代理提现', N'')
