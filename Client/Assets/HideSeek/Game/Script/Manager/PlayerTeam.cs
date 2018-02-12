using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using GameNet;
using UnityEngine.UI;
using System.Linq;

public class PlayerTeam
{
    public enum PlayerTeamType
    {
        TaggerTeam = 0,
        HideTeam,
        MaxTeamNum
    }

    private ArrayList _playerArray;  //List<PlayerBase> _playerArray;
    private PlayerTeamType _teamType;

    private const int TaggerNumLimit = 3;

    public PlayerTeam(PlayerTeamType type)
    {
        _playerArray = new ArrayList(); //_playerArray = new List<PlayerBase>();
        _teamType = type;

        //if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        //{
        //    if(_teamType == GameObjectsManager.s_LocalHumanTeamType)
        //    {
        //        AddAPlayer(false);
        //    }

        //    CreateAIPlayers();
        //}
    }

    public void CreateAIPlayers()
    {
        Debug.Log("CreateAIPlayers!!!!!!!!!!!!!!!!!!!");
        if (HNGameManager.m_iLocalChairID == -1)
        {
            return;
        }

        int[] playerNumInTeam = new int[(int)PlayerTeamType.MaxTeamNum] { 1, 1 };
        for (int i = 0; i < playerNumInTeam[(int)_teamType]; i++)
        {
            if (_teamType == PlayerTeamType.TaggerTeam)
            {
                // TaggerTeam
                AddATagger(true, 0, 0, 0);
                AddATagger(true, 1, 1, 1);
            }
            else
            {
                // HideTeam
                AddAHide(true, 0, 0, 2);
                AddAHide(true, 1, 1, 3);
            }
        }
    }

    public int AddAPlayer(bool isAI, int nChairID = 0, byte cbModelIndex = 0, byte cbAIId = HNMJ_Defines.INVALID_AI_ID)
    {
        Debug.Log("AddAPlayer!!!");

        GameManager.ListTaggerPosition.Clear();
        GameManager.ListHiderPosition.Clear();
        if (GameManager.ListTaggerPosition.Count == 0)
        {
            GameObject[] tempT = GameObject.FindGameObjectsWithTag("TaggerBornPoint").OrderBy(g => g.transform.GetSiblingIndex()).ToArray();
            for (int i = 0; i < tempT.Length; i++)
            {
                GameManager.ListTaggerPosition.Add(tempT[i]/*.transform.position*/);
            }
            GameObject[] tempH = GameObject.FindGameObjectsWithTag("HiderBornPoint").OrderBy(g => g.transform.GetSiblingIndex()).ToArray();
            for (int i = 0; i < tempH.Length; i++)
            {
                GameManager.ListHiderPosition.Add(tempH[i]/*.transform.position*/);
            }
        }

        lock (GameManager.LockObj)
        {
            if (_teamType == PlayerTeamType.TaggerTeam)
            {
                // TaggerTeam
                AddATagger(isAI, nChairID, cbModelIndex, cbAIId);
            }
            else
            {
                // HideTeam
                AddAHide(isAI, nChairID, cbModelIndex, cbAIId);
            }

            int playerIndexInTeam = _playerArray.Count - 1;
            return playerIndexInTeam;
        }
    }

    private PlayerBase AddATagger(bool isAI, int nChairID = 0, byte cbModelIndex = 0, byte cbAIId = HNMJ_Defines.INVALID_AI_ID)
    {
        int playerIndexInTeam = _playerArray.Count;
        GameObject role = null;

        GameObject taggerTeam = GameObject.Find("Player/TaggerTeam");

        //mChen add, for HideSeek WangHu
        bool bIsLocalHuman = (_teamType == GameObjectsManager.s_LocalHumanTeamType && nChairID == HNGameManager.m_iLocalChairID);
        GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
        tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
        if (bIsLocalHuman && !isAI)
            HNGameManager.MapIndex.GetComponent<Text>().text = pGlobalUserData.cbMapIndexRand.ToString() + "   " + cbModelIndex;
        if (isAI)
        {
            bIsLocalHuman = false;
        }

        cbModelIndex = (byte)(cbModelIndex % HNGameManager.taggerPrefabNames.Count);

        if (isAI)//if (playerIndexInTeam == 1)
        {
            // AI Tagger

            GameObject loadObj = Resources.Load("Player/Prefabs/Taggers/AIs/AI_" + HNGameManager.taggerPrefabFileNames[cbModelIndex]) as GameObject;
            //GameObject loadObj = Resources.Load(HNGameManager.taggerPrefabPath[cbModelIndex]) as GameObject;
            role = UnityEngine.Object.Instantiate(loadObj);
            role.tag = "Tagger";
            role.name = HNGameManager.taggerPrefabNames[cbModelIndex];
            role.transform.SetParent(taggerTeam.transform, false);
            role.transform.localPosition = Vector3.zero;
            role.SetActive(true);

            AI ai = role.GetComponent<AI>();
            if (ai == null)
            {
                ai = role.AddComponent<AI>();
            }

            // Handle BehaviorTree and NavMeshAgent
            BehaviorDesigner.Runtime.BehaviorTree behaviorTree = role.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            if (nChairID == HNGameManager.m_iLocalChairID)
            {
                // 本地AI

                if (behaviorTree == null)
                {
                    behaviorTree = role.AddComponent<BehaviorDesigner.Runtime.BehaviorTree>();
                    behaviorTree.ExternalBehavior = Resources.Load("BehaviorDesigner/BehaviorTree/TaggerAI") as BehaviorDesigner.Runtime.ExternalBehavior;//HideAI TaggerAI
                }
                behaviorTree.enabled = false;

                UnityEngine.AI.NavMeshAgent navMeshAgent = role.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (navMeshAgent != null)
                {
                    navMeshAgent.enabled = false;
                    navMeshAgent.height *= 0.5f;
                    navMeshAgent.baseOffset = 0f;
                }
            }
            else
            {
                // 其它玩家的AI

                if (behaviorTree != null)
                {
                    GameObject.Destroy(behaviorTree);
                }

                UnityEngine.AI.NavMeshAgent navMeshAgent = role.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (navMeshAgent != null)
                {
                    navMeshAgent.enabled = false;
                }
            }
        }
        else
        {
            // Human Tagger

            if (bIsLocalHuman)
            {
                // Local Human Tagger
                Debug.Log("---------------------AddATagger: Create Local Human Tagger");

                //GameObject loadObj = Resources.Load("Player/Prefabs/Taggers/" + "LocalTagger") as GameObject;//Police_Brown
                GameObject loadObj = Resources.Load(HNGameManager.taggerPrefabPath[cbModelIndex]) as GameObject;
                role = UnityEngine.Object.Instantiate(loadObj);
                ///role = GameObject.Find("LocalHuman");
                //role.name = "LocalTagger";
                role.tag = "LocalHuman";
                role.name = HNGameManager.taggerPrefabNames[cbModelIndex];
                role.transform.SetParent(taggerTeam.transform, false);
                role.transform.localPosition = Vector3.zero;
                role.SetActive(true);

                Human human = role.GetComponent<Human>();
                if (human == null)
                {
                    human = role.AddComponent<Human>();  
                }
                //Rigidbody rigi = role.AddComponent<Rigidbody>();
                //rigi.mass = 0.1f;
                //rigi.drag = 10;
                //rigi.useGravity = false;
                //rigi.collisionDetectionMode = CollisionDetectionMode.Continuous;
                //rigi.constraints = RigidbodyConstraints.FreezeRotation;
                //CharacterController cc = role.AddComponent<CharacterController>();
                //rigi.isKinematic = true;

                ////Material
                ////fix玩家第三人称视角被遮挡:ZMask处理
                //Transform meshBaseTrans = role.transform.FindChild("Model").gameObject.transform.FindChild("mesh_Base");
                //if (meshBaseTrans != null)
                //{
                //    SkinnedMeshRenderer mesh = meshBaseTrans.GetComponent<SkinnedMeshRenderer>();
                //    if (mesh != null)
                //    {
                //        if (!mesh.material.shader.name.Equals("Custom/ZMask"))
                //        {
                //            mesh.material.shader = Shader.Find("Custom/ZMask");

                //            Color col = new Color(0, 0.6f, 1, 0.3f);
                //            mesh.material.SetColor("_ZColor", col);
                //        }
                //    }
                //}

                //Main Camera
                if(ControlManager.isPerson_1st)
                    Camera.main.transform.localPosition = new Vector3(0, 1.7f, 0);
                else
                    Camera.main.transform.localPosition = new Vector3(0, 2.4f, -3);
                //HNGameManager.CameraLocalPos = Camera.main.transform.localPosition;
                Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
                if (Camera.main.GetComponent<DirectionKey>() == null)
                    Camera.main.gameObject.AddComponent<DirectionKey>();
                //if (ControlManager.s_IsFirstPersonControl)
                {
                    //if (Camera.main.transform.parent != null)
                    //{
                    //    Camera.main.transform.parent.SetParent(role.transform, false);
                    //    Camera.main.transform.parent.transform.localPosition = Vector3.zero;
                    //}
                    GameObject PosPoint = GameObject.Find("PosPoint");
                    GameObject LookAtPoint = GameObject.Find("LookAtPoint");
                    if (PosPoint != null && LookAtPoint != null)
                    {
                        if (Camera.main.transform.parent != null)
                        {
                            Camera.main.transform.parent.transform.position = PosPoint.transform.position;
                            Camera.main.transform.parent.transform.localEulerAngles = PosPoint.transform.localEulerAngles;
                            Camera.main.transform.LookAt(LookAtPoint.transform);
                            Camera.main.transform.localPosition = Vector3.zero;
                        }
                    }
                }
                //ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.PlayerViewMode);
                if (UIManager.GetInstance().m_Canvas != null)
                {
                    UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart0").GetComponent<Image>().sprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
                    UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart1").GetComponent<Image>().sprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
                    UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart2").GetComponent<Image>().sprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
                    UIManager.GetInstance().m_Canvas.transform.Find("Hearts/Heart3").GetComponent<Image>().sprite = Resources.Load("UI/Hearts/Pink", typeof(Sprite)) as Sprite;
                }
            }
            else
            {
                // Other Human Tagger

                GameObject loadObj = Resources.Load(HNGameManager.taggerPrefabPath[cbModelIndex]) as GameObject;
                role = UnityEngine.Object.Instantiate(loadObj);
                role.name = HNGameManager.taggerPrefabNames[cbModelIndex];
                role.tag = "Tagger";
                role.transform.SetParent(taggerTeam.transform, false);
                role.SetActive(true);

                Human human = role.GetComponent<Human>();
                if (human == null)
                {
                    human = role.AddComponent<Human>();
                }

                //Rigidbody rigi = role.AddComponent<Rigidbody>();
                //rigi.mass = 0.1f;
                //rigi.drag = 10;
                //rigi.useGravity = false;
                //rigi.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
        // Pos
        if (GameManager.ListTaggerPosition.Count != 0)
        {
            while (GameManager.RandomNumT.Count != GameManager.ListTaggerPosition.Count)
            {
                int index = (int)(MersenneTwister.MT19937.Int63() % GameManager.ListTaggerPosition.Count);
                if (!GameManager.ListRandomNumT.Contains((index)))
                {
                    GameManager.ListRandomNumT.Add(index);
                    GameManager.RandomNumT.Add(index, 0);
                }
            }
            if (role.tag == "LocalHuman")
            {
                if (GameManager.RandomNumT.Count > 0)
                {
                    role.transform.position = GameManager.ListTaggerPosition[nChairID].transform.position;
                    role.transform.position += new Vector3(0, 0.5f, 0);
                    role.transform.localEulerAngles = GameManager.ListTaggerPosition[nChairID].transform.localEulerAngles;
                    GameManager.RandomNumT[nChairID] = 1;
                }
                HNGameManager.playerTeamPos = true;  //标记localHuman位置已设置，供断线重连使用
            }
            else
            {
                if (GameManager.RandomNumT.Count > 0)
                {
                    if (isAI)
                    {
                        //int RealPlayerNumT = GameObjectsManager.GetInstance().GetRealPlayerNum(PlayerTeamType.TaggerTeam);
                        //Debug.Log("+++++++++++++++++++ " + (RealPlayerNumT + cbAIId));
                        //role.transform.position = GameManager.ListTaggerPosition[GameManager.RandomNumT[(RealPlayerNumT + cbAIId) % GameManager.RandomNumT.Count]].transform.position;
                        //role.transform.localEulerAngles = GameManager.ListTaggerPosition[GameManager.RandomNumT[(RealPlayerNumT + cbAIId) % GameManager.RandomNumT.Count]].transform.localEulerAngles;
                        for (int i = 0; i < GameManager.RandomNumT.Count; i++)
                        {
                            if (GameManager.RandomNumT[GameManager.ListRandomNumT[i]] == 0)
                            {
                                role.transform.position = GameManager.ListTaggerPosition[GameManager.ListRandomNumT[i]].transform.position;
                                role.transform.localEulerAngles = GameManager.ListTaggerPosition[GameManager.ListRandomNumT[i]].transform.localEulerAngles;
                                GameManager.RandomNumT[GameManager.ListRandomNumT[i]] = 1;
                                break;
                            }
                            else
                                continue;
                        }
                    }
                    else
                    {
                        role.transform.position = GameManager.ListTaggerPosition[nChairID].transform.position;
                        role.transform.localEulerAngles = GameManager.ListTaggerPosition[nChairID].transform.localEulerAngles;
                        GameManager.RandomNumT[nChairID] = 1;
                    }
                }
            }
        }

        //Add InfoPanelPref
        GameObject tempObj = Resources.Load("Player/Prefabs/InfoPanel") as GameObject;
        GameObject infoObj = UnityEngine.Object.Instantiate(tempObj);
        infoObj.name = "InfoPanel";
        infoObj.transform.SetParent(role.transform);
        infoObj.transform.localPosition = new Vector3(tempObj.transform.localPosition.x, 1.9f, tempObj.transform.localPosition.z);
        infoObj.transform.localScale = tempObj.transform.localScale;

        // Add into Team
        PlayerBase player = role.GetComponent<PlayerBase>();
        player.PlayerIndexInTeam = playerIndexInTeam;
        player.TeamType = PlayerTeamType.TaggerTeam;

        //mChen add, for HideSeek WangHu
        player.ChairID = nChairID;
        player.IsAI = isAI;
        player.AIId = cbAIId;

        //if (/*nChairID > HNGameManager.m_iLocalChairID && */!isAI)
        //{
        //    var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        //    if (kernel != null)
        //    {
        //        String[] str = kernel.getPlayerByChairID(nChairID).GetNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        //        //UIManager.GetInstance().ShowMiddleTips(str[0] + "加入了房间！");
        //        Debug.Log("------------" + str[0]);
        //        ChatSystem.GetInstance.ShowChatText("通知", str[0] + " 加入了房间！");
        //    }
        //}

        _playerArray.Add(player);
        if (player.IsLocalHuman())
        {
            //根据所属队伍显示相应UI
            ControlManager.GetInstance().Init();
            ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.FreeCameraMode);
            ControlManager.s_speed = PlayerBase.localHumanSpeed;
            ControlManager.GetInstance().SpeedChange(ControlManager.s_speed);
        }
        UIManager.GetInstance().StartPlayerJionDealInGaming((byte)player.ChairID);
        // Set AI name
        if (isAI)
        {
            //role.name = String.Format("机器人_{0}_{1}_{2}", nChairID, cbAIId, role.name);
            role.name = String.Format("{0}(机器人)", role.name);
        }
        return player;
    }

    private PlayerBase AddAHide(bool isAI, int nChairID = 0, byte cbModelIndex = 0, byte cbAIId = HNMJ_Defines.INVALID_AI_ID)
    {
        HNGameManager.hidePrefabFileNames.Clear();
        HNGameManager.hidePrefabNames.Clear();
        HNGameManager.hidePrefabPath.Clear();
        GlobalUserInfo pGlobalUserInfo = GlobalUserInfo.GetInstance();
        tagGlobalUserData pGlobalUserData = pGlobalUserInfo.GetGlobalUserData();
        string name = "";
        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
            name = ItemListManager.GetInstance.GetMapName(pGlobalUserData.cbMapIndexRandForSingleGame);
        else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
            name = ItemListManager.GetInstance.GetMapName(pGlobalUserData.cbMapIndexRand);


        for (int i = 0; i < HNGameManager.listItem.Count; i++)
        {
            if (HNGameManager.listItem[i].Map == name)
            {
                for (int j = 0; j < HNGameManager.listItem[i].SchemeItem.ModelNameList.Count; j++)
                {
                    HNGameManager.hidePrefabFileNames.Add(HNGameManager.listItem[i].SchemeItem.ModelNameList[j].FileName);
                    HNGameManager.hidePrefabNames.Add(HNGameManager.listItem[i].SchemeItem.ModelNameList[j].Name);
                    HNGameManager.hidePrefabPath.Add(HNGameManager.listItem[i].SchemeItem.ModelNameList[j].Path);
                }
                break;
            }
        }
        int playerIndexInTeam = _playerArray.Count;

        GameObject role = null;

        GameObject hideTeam = GameObject.Find("Player/HideTeam");

        //mChen add, for HideSeek WangHu
        bool bIsLocalHuman = (_teamType == GameObjectsManager.s_LocalHumanTeamType && nChairID == HNGameManager.m_iLocalChairID);
        if (bIsLocalHuman && !isAI)
            HNGameManager.MapIndex.GetComponent<Text>().text = pGlobalUserData.cbMapIndexRand.ToString() + "   " + cbModelIndex;
        if (isAI)
        {
            bIsLocalHuman = false;
        }

        ///int cbModelIndex = nChairID % hidePrefabNames.Length; ;//(int)(MersenneTwister.MT19937.Real3() * 8f);//[0,7]
        cbModelIndex = (byte)(cbModelIndex % HNGameManager.hidePrefabFileNames.Count);
        if (isAI)
        {

            // AI Hide
            GameObject loadObj = Resources.Load(HNGameManager.hidePrefabPath[cbModelIndex]) as GameObject;
            role = UnityEngine.Object.Instantiate(loadObj);
            GameObject ModelObj = role.transform.GetChild(0).gameObject;
            ModelObj.tag = "Hide";
            //if (ModelObj.transform.childCount == 0)
            //{
            //    ModelObj.transform.GetComponent<Renderer>().enabled = false;
            //}
            //else if (ModelObj.transform.childCount > 0)
            //{
            //    for (int i = 0; i < ModelObj.transform.childCount; i++)  //隐藏模型Renderer
            //    {
            //        if (ModelObj.transform.GetChild(i).GetComponent<Renderer>() != null)
            //            ModelObj.transform.GetChild(i).GetComponent<Renderer>().enabled = false;
            //    }
            //}
            role.name = HNGameManager.hidePrefabNames[cbModelIndex];
            //role.tag = "Hide";
            role.transform.SetParent(hideTeam.transform, false);
            role.transform.localPosition = Vector3.zero;
            role.SetActive(true);

            //MeshCollider mCollider = role.GetComponent<MeshCollider>();
            //if (mCollider != null)
            //    mCollider.enabled = false;
            //BoxCollider bCollider = role.GetComponent<BoxCollider>();
            //if (bCollider != null)
            //    bCollider.enabled = false;
            Collider collider = role.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
            collider = null;
            collider = ModelObj.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            AI ai = role.GetComponent<AI>();
            if (ai == null)
            {
                ai = role.AddComponent<AI>();
            }
            ai.MaxMoveSpeed = PlayerBase.AISpeed * 0.7f;

            // Handle BehaviorTree and NavMeshAgent
            BehaviorDesigner.Runtime.BehaviorTree behaviorTree = role.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            UnityEngine.AI.NavMeshAgent navMeshAgent = role.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nChairID == HNGameManager.m_iLocalChairID)
            {
                // 本地AI

                if (behaviorTree == null)
                {
                    behaviorTree = role.AddComponent<BehaviorDesigner.Runtime.BehaviorTree>();
                    behaviorTree.ExternalBehavior = Resources.Load("BehaviorDesigner/BehaviorTree/HideAI") as BehaviorDesigner.Runtime.ExternalBehavior;//HideAI TaggerAI
                }
                //behaviorTree.enabled = false;

                // Handle NavMeshAgent
                if (navMeshAgent == null)
                {
                    navMeshAgent = role.AddComponent<UnityEngine.AI.NavMeshAgent>();
                }
                navMeshAgent.height *= 0.5f;
                navMeshAgent.baseOffset = 0f;
            }
            else
            {
                // 其它玩家的AI

                if (behaviorTree != null)
                {
                    GameObject.Destroy(behaviorTree);
                }

                if (navMeshAgent != null)
                {
                    navMeshAgent.enabled = false;
                }
            }

        }
        else
        {
            if (bIsLocalHuman)
            {
                Debug.Log("----------------------Hider  bIsLocalHuman");
                // Local Human Hide
                Debug.Log("---------------------AddAHide: Create Local Human Hide");

                GameObject loadObj = Resources.Load(HNGameManager.hidePrefabPath[cbModelIndex]) as GameObject;
                role = UnityEngine.Object.Instantiate(loadObj);
                //role.name = "LocalHide";
                role.name = HNGameManager.hidePrefabNames[cbModelIndex];
                role.tag = "LocalHuman";
                role.transform.SetParent(hideTeam.transform, false);
                role.transform.localPosition = Vector3.zero;
                role.SetActive(true);

                Human human = role.GetComponent<Human>();
                if (human == null)
                {
                    human = role.AddComponent<Human>();
                }

                //Rigidbody rigi = role.AddComponent<Rigidbody>();
                //rigi.mass = 0.1f;
                //rigi.drag = 10;
                //rigi.useGravity = false;
                //rigi.constraints = RigidbodyConstraints.FreezeRotation;
                //CharacterController cc = role.AddComponent<CharacterController>();

                //Main Camera
                //if (ControlManager.s_IsFirstPersonControl)
                {
                    //if (Camera.main.transform.parent != null)
                    //{
                    //    Camera.main.transform.parent.SetParent(role.transform, false);
                    //    Camera.main.transform.parent.transform.localPosition = Vector3.zero;
                    //}
                    GameObject PosPoint = GameObject.Find("PosPoint");
                    GameObject LookAtPoint = GameObject.Find("LookAtPoint");
                    if (PosPoint != null && LookAtPoint != null)
                    {
                        if (Camera.main.transform.parent != null)
                        {
                            Camera.main.transform.parent.transform.position = PosPoint.transform.position;
                            Camera.main.transform.parent.transform.localEulerAngles = PosPoint.transform.localEulerAngles;
                            Camera.main.transform.LookAt(LookAtPoint.transform);
                            Camera.main.transform.localPosition = Vector3.zero;
                        }
                    }
                }
                float y = 0;
                float z = 0;
                GameObject ModelObj = role.transform.GetChild(0).gameObject;
                ModelObj.tag = "Hide";
                if (ModelObj.transform.childCount == 0)
                {
                    y = ModelObj.gameObject.GetComponent<Renderer>().bounds.size.y;
                    z = ModelObj.gameObject.GetComponent<Renderer>().bounds.size.z;
                }
                else if (ModelObj.transform.childCount > 0)
                {
                    y = ModelObj.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y;
                    z = ModelObj.transform.GetChild(0).GetComponent<Renderer>().bounds.size.z;
                }
                Camera.main.transform.localPosition = new Vector3(0, y * 1.5f + 1, -(z * 1.5f + 2));
                //HNGameManager.CameraLocalPos = Camera.main.transform.localPosition;
                Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
                //if (Camera.main.GetComponent<DirectionKey>() == null)
                //    Camera.main.gameObject.AddComponent<DirectionKey>();

                //ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.PlayerViewMode);
            }
            else
            {
                // Other Human Hide

                GameObject loadObj = Resources.Load(HNGameManager.hidePrefabPath[cbModelIndex]) as GameObject;
                role = UnityEngine.Object.Instantiate(loadObj);
                role.name = HNGameManager.hidePrefabNames[cbModelIndex];
                //role.tag = "Hide";
                role.transform.SetParent(hideTeam.transform, false);
                role.SetActive(true);

                GameObject ModelObj = role.transform.GetChild(0).gameObject;
                ModelObj.tag = "Hide";

                Human human = role.GetComponent<Human>();
                if (human == null)
                {
                    human = role.AddComponent<Human>();
                }

                //Rigidbody rigi = role.AddComponent<Rigidbody>();
                //rigi.mass = 0.1f;
                //rigi.drag = 10;
                //rigi.useGravity = false;
                //rigi.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        // Pos
        if (GameManager.ListHiderPosition.Count != 0)
        {
            while (GameManager.RandomNumH.Count != GameManager.ListHiderPosition.Count)
            {
                int index = (int)(MersenneTwister.MT19937.Int63() % GameManager.ListHiderPosition.Count);
                if (!GameManager.ListRandomNumH.Contains((index)))
                {
                    GameManager.ListRandomNumH.Add(index);
                    GameManager.RandomNumH.Add(index, 0);
                }
            }
            if (role.tag == "LocalHuman")
            {
                if (GameManager.RandomNumH.Count > 0)
                {
                    role.transform.position = GameManager.ListHiderPosition[nChairID].transform.position;
                    role.transform.position += new Vector3(0, 0.5f, 0);
                    role.transform.localEulerAngles = GameManager.ListHiderPosition[nChairID].transform.localEulerAngles;
                    GameManager.RandomNumH[nChairID] = 1;
                }
                HNGameManager.playerTeamPos = true;   //标记localHuman位置已设置，供断线重连使用
            }
            else
            {
                if (GameManager.RandomNumH.Count > 0)
                {
                    if (isAI)
                    {
                        //int RealPlayerNumH = GameObjectsManager.GetInstance().GetRealPlayerNum(PlayerTeamType.HideTeam);
                        //int RealPlayerNumT = GameObjectsManager.GetInstance().GetRealPlayerNum(PlayerTeamType.TaggerTeam);
                        //int indexAI = TaggerNumLimit - RealPlayerNumT > 0 ? TaggerNumLimit - RealPlayerNumT : 0;      //TaggerNumLimit--警察玩家未满 TaggerNumLimit 个人填充AI至 TaggerNumLimit 人，因为AIId不分警察躲藏者，需了解在警察方有几个AI,才能设置躲藏者AI的位置
                        //indexAI = cbAIId - indexAI;
                        //Debug.LogWarning("----------------------- " + RealPlayerNumH + " " + indexAI + " " + (RealPlayerNumH + indexAI));
                        //role.transform.position = GameManager.ListHiderPosition[GameManager.RandomNumH[(RealPlayerNumH + indexAI) % GameManager.RandomNumH.Count]].transform.position;
                        //role.transform.localEulerAngles = GameManager.ListHiderPosition[GameManager.RandomNumH[(RealPlayerNumH + indexAI) % GameManager.RandomNumH.Count]].transform.localEulerAngles;
                        for (int i = 0; i < GameManager.RandomNumH.Count; i++)
                        {
                            if (GameManager.RandomNumH[GameManager.ListRandomNumH[i]] == 0)
                            {
                                role.transform.position = GameManager.ListHiderPosition[GameManager.ListRandomNumH[i]].transform.position;
                                role.transform.localEulerAngles = GameManager.ListHiderPosition[GameManager.ListRandomNumH[i]].transform.localEulerAngles;
                                GameManager.RandomNumH[GameManager.ListRandomNumH[i]] = 1;
                                break;
                            }
                            else
                                continue;
                        }
                    }
                    else
                    {
                        role.transform.position = GameManager.ListHiderPosition[nChairID].transform.position;
                        role.transform.localEulerAngles = GameManager.ListHiderPosition[nChairID].transform.localEulerAngles;
                        GameManager.RandomNumH[nChairID] = 1;
                    }
                }
            }
        }

        //Add InfoPanelPref
        GameObject tempObj = Resources.Load("Player/Prefabs/InfoPanel") as GameObject;
        GameObject infoObj = UnityEngine.Object.Instantiate(tempObj);
        infoObj.name = "InfoPanel";
        infoObj.transform.SetParent(role.transform);
        float yy = 0;
        GameObject modelObj = role.transform.GetChild(0).gameObject;
        if (modelObj.transform.childCount == 0)
            yy = modelObj.gameObject.GetComponent<Renderer>().bounds.size.y;
        else if (modelObj.transform.childCount > 0)
            yy = modelObj.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y;
        infoObj.transform.localPosition = new Vector3(tempObj.transform.localPosition.x, yy + 0.2f, tempObj.transform.localPosition.z);
        infoObj.transform.localScale = tempObj.transform.localScale;

        // Add into Team
        PlayerBase player = role.GetComponent<PlayerBase>();
        player.PlayerIndexInTeam = playerIndexInTeam;
        player.TeamType = PlayerTeamType.HideTeam;
        //隐藏躲藏者Renderer
        player.SetGameObjVisible(false);

        //mChen add, for HideSeek WangHu
        player.ChairID = nChairID;
        player.IsAI = isAI;
        player.AIId = cbAIId;

        //if (/*nChairID > HNGameManager.m_iLocalChairID && */!isAI)
        //{
        //    var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
        //    if (kernel != null)
        //    {
        //        String[] str = kernel.getPlayerByChairID(nChairID).GetNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        //        //UIManager.GetInstance().ShowMiddleTips(str[0] + "加入了房间！");
        //        ChatSystem.GetInstance.ShowChatText("通知", str[0] + " 加入了房间！");
        //    }
        //}

        _playerArray.Add(player);
        if (player.IsLocalHuman())
        {
            //根据所属队伍显示相应UI
            ControlManager.GetInstance().Init();
            //ControlManager.GetInstance().ShowButtonUI();
            ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.FreeCameraMode);
            ControlManager.s_speed = PlayerBase.localHumanSpeed * 0.7f;   //躲藏者速度降低30%
            ControlManager.GetInstance().SpeedChange(ControlManager.s_speed);
        }
        UIManager.GetInstance().StartPlayerJionDealInGaming((byte)player.ChairID);
        // Set AI name
        if (isAI)
        {
            //role.name = String.Format("机器人_{0}_{1}_{2}", nChairID, cbAIId, role.name);
            role.name = String.Format("{0}(机器人)", role.name);
        }
        return player;
    }

    
    public int GetPlayerNum()
    {
        int playerCount = _playerArray.Count;
        return playerCount;
    }

    public ArrayList GetPlayers()
    {
        return _playerArray;
    }

    public PlayerBase GetPlayer(int playerIndex)
    {
        int playerCount = _playerArray.Count;
        if (playerCount > 0)
        {
            if (playerIndex >= 0 && playerIndex < playerCount)
            {
                PlayerBase player = _playerArray[playerIndex] as PlayerBase;
                return player;
            }
            else
            {
                throw new Exception();
            }
        }

        return null;
    }

    //mChen add, for HideSeek WangHu
    public PlayerBase GetPlayerByChairID(int nChaidID, byte cbAIId)
    {
        foreach (PlayerBase player in _playerArray)
        {
            if (player && player.ChairID == nChaidID && player.AIId == cbAIId)
            {
                return player;
            }
        }

        return null;
    }
    public int GetAINum(int nChaidID)
    {
        int nAINum = 0;
        if (nChaidID >= 0 && nChaidID < HNMJ_Defines.GAME_PLAYER)
        {
            foreach (PlayerBase player in _playerArray)
            {
                if (player && player.IsAI && player.ChairID == nChaidID)
                {
                    nAINum++;
                }
            }
        }
        return nAINum;
    }
    
    public void RemovePlayers(bool bRemoveAI = true)
    {
        for (int i = 0; i < _playerArray.Count; i++)
        {
            PlayerBase player = GetPlayer(i);
            if (player)
            {
                if (player.IsLocalHuman())
                {
                    // Local
                    if (Camera.main.transform.parent != null)
                    {
                        Camera.main.transform.parent.SetParent(null, false);///Camera.main.transform.SetParent(null, false);
                        Camera.main.transform.parent.transform.position = Vector3.zero;
                    }
                }

                if (!player.IsAI || (player.IsAI && bRemoveAI))
                {
                    GameObject.Destroy(player.gameObject);
                    _playerArray.RemoveAt(i);
                    i--;    //add to fix两个以上的人联机，有人断线重连回来后必会多一个人
                    //player.SyncDead();//comment to 防止断线引起的自己死亡
                }
            }
        }
    }
    public void RemovePlayerByChairID(byte cbChairID)
    {
        for (int i = 0; i < _playerArray.Count; i++)
        {
            PlayerBase player = GetPlayer(i);
            if (player != null)
            {
                if (!player.IsAI && player.ChairID == cbChairID)
                {
                    GameObject.Destroy(player.gameObject);
                    _playerArray.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void CustomUpdate()
    {
        foreach (PlayerBase player in _playerArray)
        {
            player.CustomUpdate();
        }
    }

}