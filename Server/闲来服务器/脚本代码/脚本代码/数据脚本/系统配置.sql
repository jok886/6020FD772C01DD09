USE QPAccountsDB_HideSeek
GO

------------------------------------------------------------------------------------------------------------------------------------------------------------------------

INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'BankPrerequisite',2000,N'银行存取条件，存取金币数必须大于此数才可操作！',N'存取条件',N'键值：表示存取金币数必须大于此数才可存取')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'EnjoinInsure',0,N'由于系统维护，暂时停止游戏系统的保险柜服务，请留意网站公告信息！',N'银行服务',N'键值：0-开启，1-关闭')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'EnjoinLogon',0,N'由于系统维护，暂时停止游戏系统的登录服务，请留意网站公告信息！',N'登录服务',N'键值：0-开启，1-关闭')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'EnjoinRegister',0,N'由于系统维护，暂时停止游戏系统的注册服务，请留意网站公告信息！',N'注册服务',N'键值：0-开启，1-关闭')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'GrantIPCount',5,N'新用户注册IP每天赠送限制！',N'注册IP限制',N'键值：表示同一个IP最多赠送的次数，超过此数将不赠送金币')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'GrantScoreCount',1000,N'新用户注册系统送金币的数目！',N'注册赠送',N'键值：表示赠送的金币数量')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'GrantScoreInsure',100,N'新用户注册系统送元宝的数目！',N'注册赠送',N'键值：表示赠送的元宝数量')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'MedalRate',10,N'奖牌系统返还比率（千分比），奖牌根据玩家每局游戏税收返还给玩家！',N'奖牌返率',N'键值：表示奖牌系统返还比率值（千分比）！')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'RevenueRateTake',10,N'银行取款操作税收比率（千分比）！',N'取款税率',N'键值：表示银行取款操作税收比率值（千分比）！')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'RevenueRateTransfer',10,N'银行转账操作税收比率（千分比）！',N'转账税率',N'键值：表示银行转账操作税收比率值（千分比）！')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'TransferNullity',1,N'会员间转账是否收取手续费！',N'转账是否收税',N'')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'TransferPrerequisite',10000,N'银行转账条件，转账金币数必须大于此数才可转账！',N'转账条件',N'键值：表示转账金币数必须大于此数才可转账')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'SubsistenceAllowancesCondition', 1000, N'领取低保玩家游戏币不能低于', N'低保领取条件', N'键值：领取低保玩家金币不能低于该金币数')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'SubsistenceAllowancesGold', 1000, N'每次低保的游戏币数', N'低保每次金额', N'键值：每次低保的金币数')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'SubsistenceAllowancesNumber', 3, N'每天低保次数', N'每天低保次数', N'键值：每天低保次数')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'MaxSiginDay', 30, N'最大签到天数', N'', N'')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'SiginDayGold', 0, N'每次签到领取钻石数量', N'', N'')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'PublicNotice', 0, N'新安棋牌火爆公测中，欢迎各位玩家及代理反馈问题，祝游戏愉快！', N'', N'')
INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'HN_SC_NOTICE', 0, N'圈圈四川麻将杜绝外挂，任何说有外挂的都是骗子。|圈圈四川麻将仅是休闲游戏，请勿赌博。', N'', N'')

INSERT [SystemStatusInfo] ([StatusName],[StatusValue],[StatusString],[StatusTip],[StatusDescription]) VALUES ( N'PlayCountPerRaffle', 10, N'每打完多少场可以抽奖一次', N'', N'')

INSERT [SystemStatusInfo] ([StatusName], [StatusValue], [StatusString], [StatusTip], [StatusDescription]) VALUES (N'WinExperience', 10, N'赢局奖励的经验值', N'赢局经验', N'键值：赢局奖励的经验值')
INSERT [SystemStatusInfo] ([StatusName], [StatusValue], [StatusString], [StatusTip], [StatusDescription]) VALUES (N'MedalExchangeRate', 1000, N'元宝与游戏币兑换率', N'元宝兑换率', N'键值：1个元宝兑换多少游戏币')


