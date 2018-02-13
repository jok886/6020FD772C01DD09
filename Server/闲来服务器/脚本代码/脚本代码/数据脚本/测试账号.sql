USE QPAccountsDB_HideSeek
GO

INSERT AccountsInfo (Accounts,NickName,LogonPass,InsurePass,RegAccounts,SpreaderID,PassPortID,Compellation,Gender,FaceID,
	GameLogonTimes,LastLogonIP,LastLogonMachine,RegisterIP,RegisterMachine)
VALUES (N'111111',N'111111',N'111111',N'111111',N'0',0,0,N'',0,0,1,N'',N'',N'',N'')

INSERT AccountsInfo (Accounts,NickName,LogonPass,InsurePass,RegAccounts,SpreaderID,PassPortID,Compellation,Gender,FaceID,
	GameLogonTimes,LastLogonIP,LastLogonMachine,RegisterIP,RegisterMachine)
VALUES (N'222222',N'222222',N'111111',N'111111',N'0',0,0,N'',0,0,1,N'',N'',N'',N'')

INSERT AccountsInfo (Accounts,NickName,LogonPass,InsurePass,RegAccounts,SpreaderID,PassPortID,Compellation,Gender,FaceID,
	GameLogonTimes,LastLogonIP,LastLogonMachine,RegisterIP,RegisterMachine)
VALUES (N'333333',N'333333',N'111111',N'111111',N'0',0,0,N'',0,0,1,N'',N'',N'',N'')

INSERT AccountsInfo (Accounts,NickName,LogonPass,InsurePass,RegAccounts,SpreaderID,PassPortID,Compellation,Gender,FaceID,
	GameLogonTimes,LastLogonIP,LastLogonMachine,RegisterIP,RegisterMachine)
VALUES (N'444444',N'444444',N'111111',N'111111',N'0',0,0,N'',0,0,1,N'',N'',N'',N'')


INSERT AccountsWeakAddRank (UserID,LastLogonTime,UserPoint) VALUES (1,0,1)
INSERT AccountsWeakAddRank (UserID,LastLogonTime,UserPoint) VALUES (2,0,2)
INSERT AccountsWeakAddRank (UserID,LastLogonTime,UserPoint) VALUES (3,0,3)
INSERT AccountsWeakAddRank (UserID,LastLogonTime,UserPoint) VALUES (4,0,4)
INSERT AccountsWeakAddRank (UserID,LastLogonTime,UserPoint) VALUES (5,0,5)

INSERT AccountsQuarterAddRank (UserID,LastLogonTime,UserPoint) VALUES (1,0,100)
INSERT AccountsQuarterAddRank (UserID,LastLogonTime,UserPoint) VALUES (2,0,200)
INSERT AccountsQuarterAddRank (UserID,LastLogonTime,UserPoint) VALUES (3,0,3)
INSERT AccountsQuarterAddRank (UserID,LastLogonTime,UserPoint) VALUES (4,0,400)
INSERT AccountsQuarterAddRank (UserID,LastLogonTime,UserPoint) VALUES (5,0,5)
INSERT AccountsQuarterAddRank (UserID,LastLogonTime,UserPoint) VALUES (6,0,10000)

INSERT AccountsYearAddRank (UserID,LastLogonTime,UserPoint) VALUES (1,0,1121)
INSERT AccountsYearAddRank (UserID,LastLogonTime,UserPoint) VALUES (2,0,4452)
INSERT AccountsYearAddRank (UserID,LastLogonTime,UserPoint) VALUES (3,0,676573)
INSERT AccountsYearAddRank (UserID,LastLogonTime,UserPoint) VALUES (4,0,76674)
INSERT AccountsYearAddRank (UserID,LastLogonTime,UserPoint) VALUES (5,0,887775)
INSERT AccountsYearAddRank (UserID,LastLogonTime,UserPoint) VALUES (6,0,587775)

INSERT ExchangeHuaFeiList (ExchangeID,UseType,UseNum,GetType,GetNum,GoodsName,ExchangeDesc,ImgIcon,Flag) VALUES (1,0,100,1,10,N'10元话费',N'',N'',0)
INSERT ExchangeHuaFeiList (ExchangeID,UseType,UseNum,GetType,GetNum,GoodsName,ExchangeDesc,ImgIcon,Flag) VALUES (2,0,200,1,20,N'20元话费',N'',N'',0)
INSERT ExchangeHuaFeiList (ExchangeID,UseType,UseNum,GetType,GetNum,GoodsName,ExchangeDesc,ImgIcon,Flag) VALUES (3,0,300,1,30,N'30元话费',N'',N'',0)
INSERT ExchangeHuaFeiList (ExchangeID,UseType,UseNum,GetType,GetNum,GoodsName,ExchangeDesc,ImgIcon,Flag) VALUES (4,0,500,1,50,N'50元话费',N'',N'',0)
INSERT ExchangeHuaFeiList (ExchangeID,UseType,UseNum,GetType,GetNum,GoodsName,ExchangeDesc,ImgIcon,Flag) VALUES (5,0,1000,1,100,N'100元话费',N'',N'',0)

INSERT ShopInfoList (ItemID,ItemType,OrderID_IOS,OrderID_Android,Price,GoodsNum,ItemTitle,ItemDesc,ItemIcon,ItemName) VALUES (1,1,1001,2001,68,1100000,N'68元',N'',N'',N'1100000金币')
INSERT ShopInfoList (ItemID,ItemType,OrderID_IOS,OrderID_Android,Price,GoodsNum,ItemTitle,ItemDesc,ItemIcon,ItemName) VALUES (2,1,1002,2002,6,68000,N'6元',N'',N'',N'68000金币')
INSERT ShopInfoList (ItemID,ItemType,OrderID_IOS,OrderID_Android,Price,GoodsNum,ItemTitle,ItemDesc,ItemIcon,ItemName) VALUES (3,1,1003,2003,12,150000,N'12元',N'',N'',N'150000金币')
INSERT ShopInfoList (ItemID,ItemType,OrderID_IOS,OrderID_Android,Price,GoodsNum,ItemTitle,ItemDesc,ItemIcon,ItemName) VALUES (4,1,1004,2004,18,250000,N'18元',N'',N'',N'250000金币')


USE QPTreasureDB_HideSeek
GO

INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (10,3,0,0,0,0,0,0,16777215,1)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (11,3,0,0,0,0,0,0,16777215,2)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (12,3,0,0,0,0,0,0,16777215,4)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (13,3,0,0,0,0,0,0,16777215,1)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (14,3,0,0,0,0,0,0,16777215,2)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (15,3,0,0,0,0,0,0,16777215,4)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (16,3,0,0,0,0,0,0,16777215,1)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (17,3,0,0,0,0,0,0,16777215,2)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (18,3,0,0,0,0,0,0,16777215,4)

INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (20,3,0,0,0,0,0,0,16777215,1)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (21,3,0,0,0,0,0,0,16777215,2)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (22,3,0,0,0,0,0,0,16777215,4)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (23,3,0,0,0,0,0,0,16777215,1)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (24,3,0,0,0,0,0,0,16777215,2)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (25,3,0,0,0,0,0,0,16777215,4)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (26,3,0,0,0,0,0,0,16777215,1)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (27,3,0,0,0,0,0,0,16777215,2)
INSERT AndroidManager (UserID,ServerID,MinPlayDraw,MaxPlayDraw,MinTakeScore,MaxTakeScore,MinReposeTime,MaxReposeTime,ServiceTime,ServiceGender)
VALUES (28,3,0,0,0,0,0,0,16777215,4)

