
----------------------------------------------------------------------------------------------------

USE QPPlatformDB_HideSeek
GO

-- 删除数据
DELETE GameTypeItem
DELETE GameKindItem
DELETE GameNodeItem
DELETE GameGameItem
DELETE DataBaseInfo
DELETE PrivateInfo
GO

----------------------------------------------------------------------------------------------------

INSERT DataBaseInfo (DBAddr, DBPort, DBUser, DBPassword,MachineID,Information) VALUES ('127.0.0.1',1433,'sa','Cm123.','','')--mChen fangyuan1793

----------------------------------------------------------------------------------------------------

-- 类型数据
INSERT GameTypeItem (TypeID, TypeName, SortID, Nullity) VALUES ( 1, '财富游戏',100, 0)
INSERT GameTypeItem (TypeID, TypeName, SortID, Nullity) VALUES ( 2, '牌类游戏',200, 0)
INSERT GameTypeItem (TypeID, TypeName, SortID, Nullity) VALUES ( 3, '棋类游戏',300, 0)
INSERT GameTypeItem (TypeID, TypeName, SortID, Nullity) VALUES ( 4, '麻将游戏',400, 0)
INSERT GameTypeItem (TypeID, TypeName, SortID, Nullity) VALUES ( 5, '休闲游戏',500, 0)
INSERT GameTypeItem (TypeID, TypeName, SortID, Nullity) VALUES ( 6, '视频游戏',600, 0)

----------------------------------------------------------------------------------------------------
			
--mChen			
INSERT PrivateInfo 	(KindID, PlayCout1, PlayCost1,PlayCostAvg1, PlayCout2, PlayCost2,PlayCostAvg2,PlayCout3, PlayCost3,PlayCostAvg3,PlayCout4, PlayCost4,PlayCostAvg4,CostGold) 	
			VALUES ( 311,      1,         0, 		0,				16,			16,			4,		0,				0,		0,			0,			0,		0,			0)


INSERT GameKindItem (KindID, GameID, TypeID, JoinID,SortID,KindName,ProcessName,GameRuleUrl,DownLoadUrl,Nullity) VALUES ( 311,311,2,0,100, '开心躲猫猫','HNMJ.exe',0,0,0)
INSERT GameGameItem (GameID, GameName, SuporType, DataBaseAddr,DataBaseName,ServerVersion,ClientVersion,ServerDLLName,ClientExeName) VALUES ( 311, '开心躲猫猫',1,'127.0.0.1','QPTreasureDB_HideSeek',101056771,101056771,'HNMJServer-zz','HNMJ.exe')	--mChen

INSERT GameKindItem (KindID, GameID, TypeID, JoinID,SortID,KindName,ProcessName,GameRuleUrl,DownLoadUrl,Nullity) VALUES ( 312,312,2,0,100, '13水','HNMJ.exe',0,0,0)
INSERT GameGameItem (GameID, GameName, SuporType, DataBaseAddr,DataBaseName,ServerVersion,ClientVersion,ServerDLLName,ClientExeName) VALUES ( 312, '13水',1,'127.0.0.1','QPTreasureDB_HideSeek',101056771,101056771,'13SServer','HNMJ.exe')	--mChen


GO


USE QPGameMatchDB_HideSeek
GO
DELETE MatchPublic
DELETE MatchImmediate
DELETE MatchLockTime
DELETE MatchReward

INSERT MatchPublic (MatchID,MatchNo,KindID,MatchName,MatchType,MatchFeeType,MatchFee,MatchEnterScore,MemberOrder,CollectDate) VALUES (1,1,302,N'血战到底比赛',1,0,100,1000,0,0)
INSERT MatchImmediate (MatchID,MatchNo,StartUserCount,AndroidUserCount,InitialBase,InitialScore,MinEnterGold,PlayCount,SwitchTableCount,PrecedeTimer) 
VALUES (1,1,4,0,100,1000,100,2,0,0)

INSERT MatchPublic (MatchID,MatchNo,KindID,MatchName,MatchType,MatchFeeType,MatchFee,MatchEnterScore,MemberOrder,CollectDate) VALUES (3,3,302,N'血战到底比赛',0,0,100,1000,0,0)
INSERT MatchLockTime (MatchID,MatchNo,StartTime,EndTime,InitScore,CullScore,MinPlayCount) 
VALUES (3,3,'2016-04-21 0:0:0','2017-04-21 0:0:0',1000,100,2)

--mChen
INSERT MatchPublic (MatchID,MatchNo,KindID,MatchName,MatchType,MatchFeeType,MatchFee,MatchEnterScore,MemberOrder,CollectDate) VALUES (5,5,311,N'开心躲猫猫定时赛',0,0,100,1000,0,0)
INSERT MatchLockTime (MatchID,MatchNo,StartTime,EndTime,InitScore,CullScore,MinPlayCount) 
VALUES (5,5,'2016-04-21 0:0:0','2020-04-21 23:0:0',1000,100,2)


INSERT MatchReward (MatchID,MatchNo,MatchRank,RewardGold,RewardMedal,RewardExperience,RewardDescibe) VALUES (1,1,1,4000,0,0,N'金币')
INSERT MatchReward (MatchID,MatchNo,MatchRank,RewardGold,RewardMedal,RewardExperience,RewardDescibe) VALUES (1,1,2,3000,0,0,N'金币')
INSERT MatchReward (MatchID,MatchNo,MatchRank,RewardGold,RewardMedal,RewardExperience,RewardDescibe) VALUES (1,1,3,2000,0,0,N'金币')
INSERT MatchReward (MatchID,MatchNo,MatchRank,RewardGold,RewardMedal,RewardExperience,RewardDescibe) VALUES (1,1,4,1000,0,0,N'金币')

--mChen add
INSERT MatchReward (MatchID,MatchNo,MatchRank,RewardGold,RewardMedal,RewardExperience,RewardDescibe) VALUES (5,5,1,4000,0,0,N'金币')
INSERT MatchReward (MatchID,MatchNo,MatchRank,RewardGold,RewardMedal,RewardExperience,RewardDescibe) VALUES (5,5,2,3000,0,0,N'金币')
INSERT MatchReward (MatchID,MatchNo,MatchRank,RewardGold,RewardMedal,RewardExperience,RewardDescibe) VALUES (5,5,3,2000,0,0,N'金币')
INSERT MatchReward (MatchID,MatchNo,MatchRank,RewardGold,RewardMedal,RewardExperience,RewardDescibe) VALUES (5,5,4,1000,0,0,N'金币')

GO