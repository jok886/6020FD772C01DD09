using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using GameNet;
using System;

public class PlayerBase : MonoBehaviour
{
    public enum PlayerEventKind
    {
        Pick = 0,
        Boom,           //炸弹爆炸

        DeadByDecHp,    //自己扣完血而死
        DeadByPicked,   //被警察点死
        DeadByBoom,     //被炸弹炸死

        GetInventory,   //拾取道具
        DecHp,
        AddHp,

        MaxEventNum
    }
    public const float localHumanSpeed = 5.1f;
    public const float AISpeed = 4.0f;

    public static int MaxHp = 4;
    public static byte InvalidAIId = 255;

    public float MaxMoveSpeed = 4.0f;
    public float AIMoveSpeed = 4.0f;
    public int Hp = 4;
    public bool IsAI = false;
    public byte AIId = HNMJ_Defines.INVALID_AI_ID;

    public int PlayerIndexInTeam;
    public PlayerTeam.PlayerTeamType TeamType;

    protected Animator _avatarAnimator;
    protected CharacterController _controller;

    // for HideSeek WangHu
    public int ChairID = 0;
    public string ObjNamePicked = "";
    public float CurMoveSpeed = 0f;
    private Vector3 m_lastPlayerPos;

    //WQ
    public float m_JumpSpeed;
    public bool m_Jump;
    public bool m_Lock;
    public bool m_Jumping;
    public bool m_isGrounded;
    private bool m_PreviouslyGrounded;
    private Vector3 m_MoveDir = Vector3.zero;
    private CollisionFlags m_CollisionFlags;

    //玩家列表积分
    public int GameScore = 0;

    public byte m_isStealth;

    private HNGameManager m_hnGameManager = null;

    protected void Awake()
    {
        Init();
        // Animator
        //if (IsAI)
        //    _avatarAnimator = gameObject.transform.GetComponent<Animator>();
        //else
        _avatarAnimator = gameObject.transform.GetChild(0).transform.GetComponent<Animator>();

    }


    // Use this for initialization
    protected void Start()
    {
        m_hnGameManager = GameObject.FindObjectOfType<HNGameManager>();

        if (gameObject.tag == "LocalHuman")
        {
            _controller = transform.GetComponent<CharacterController>();
            if (_controller == null)
            {
                _controller = gameObject.AddComponent<CharacterController>();
                Renderer render = null;
                if (TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                {
                    GameObject ModelObj = gameObject.transform.GetChild(0).gameObject;
                    if (ModelObj.transform.childCount == 0)   //整体模型
                        render = ModelObj.GetComponent<Renderer>();
                    else if (ModelObj.transform.childCount > 0)  //多段模型
                        render = ModelObj.transform.GetChild(0).GetComponent<Renderer>();
                }
                if (render != null)
                {
                    float min = render.bounds.size.x;
                    if (min > render.bounds.size.y)
                        min = render.bounds.size.y;
                    if (min > render.bounds.size.z)
                        min = render.bounds.size.z;
                    _controller.radius = min / 2f;
                    _controller.height = render.bounds.size.y;
                    _controller.center = new Vector3(0, _controller.height / 2f, 0);
                    _controller.skinWidth = _controller.radius * 0.1f;
                    _controller.minMoveDistance = 0;
                    _controller.stepOffset = _controller.height * 0.3f / 2;
                }
                else
                {
                    _controller.height = 1.7f;
                    _controller.center = new Vector3(0, _controller.height / 2f, 0);
                    _controller.skinWidth = _controller.radius * 0.1f;
                    _controller.minMoveDistance = 0;
                    _controller.stepOffset = _controller.height * 0.3f / 2;
                }
            }
            SetJumpSpeed(_controller);
        }
        // Disable local human rendering:
        if (IsLocalHuman() && TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
        {
            //DisableModelDisplay();//mChen
        }

        if (TeamType == PlayerTeam.PlayerTeamType.HideTeam)
        {
            if (_controller)
            {
                //_controller.height = 1f;
            }
        }

        if (_controller)
        {
            //_controller.stepOffset = 0.1f;
        }

        m_lastPlayerPos = transform.position;
    }
    public void SetJumpSpeed(CharacterController _controller)
    {
        //根据模型高度设置速度
        if (_controller.height < 1.7)
            m_JumpSpeed = 6;
        else
            m_JumpSpeed = _controller.height * 3.5f;
        if (m_JumpSpeed > 6)
            m_JumpSpeed = 6;
    }
    // Update is called once per frame
    protected void Update()
    {
        //if (IsLocalHuman())
        //    JumpUpdate();
    }
    protected void FixedUpdate()
    {
        if (IsLocalHuman())
            JumpFixedUpdate();
    }
    //public void JumpUpdate()
    //{
    //    if (gameObject.tag == "LocalHuman")
    //    {
    //        if (_controller == null)
    //            _controller = gameObject.GetComponent<CharacterController>();
    //        if (!m_PreviouslyGrounded && m_isGrounded) //落地
    //        {
    //            m_MoveDir = Vector3.zero;
    //            m_Jumping = false;
    //        }
    //        m_PreviouslyGrounded = m_isGrounded;
    //    }
    //}
    public void JumpFixedUpdate()
    {
        if (gameObject.tag == "LocalHuman")
        {
            if (_controller == null)
                _controller = gameObject.AddComponent<CharacterController>();
            if (m_isGrounded)
            {
                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    m_Jump = false;
                    m_Jumping = true;
                    m_isGrounded = false;
                }
            }
            //else
            {
                if (!m_Lock)
                {
                    if (m_MoveDir.y > 0)
                        m_MoveDir += Physics.gravity * Time.fixedDeltaTime;
                    else
                    {
                        if (TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                        {
                            m_MoveDir += Physics.gravity * 0.25f * Time.fixedDeltaTime;   //警察下降速度减缓
                        }
                        else if (TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                        {
                            m_MoveDir += Physics.gravity * Time.fixedDeltaTime;
                        }
                    }


                }
                else
                    m_MoveDir = Vector3.zero;
            }
            if (_controller != null)
                if (_controller.enabled)
                    m_CollisionFlags = _controller.Move(m_MoveDir * Time.fixedDeltaTime);
        }
    }
    public void Init()
    {
        Hp = 4;
        m_JumpSpeed = 5;
        m_Jumping = false;
        m_Jump = false;
        m_PreviouslyGrounded = false;
        m_Lock = false;
        m_isGrounded = false;

        m_isStealth = 0;
    }
    public bool IsLocalHuman()
    {
        bool isLocalPlayer = (TeamType == GameObjectsManager.s_LocalHumanTeamType && ChairID == HNGameManager.m_iLocalChairID);

        bool isLocalHuman = isLocalPlayer && !IsAI;
        return isLocalHuman;
    }

    public void MovePlayer(Vector3 moveDirection)
    {
        float deltaTime = 0.03f;
        if (moveDirection.magnitude > 0f)
        {
            Vector3 deltaPos = moveDirection * MaxMoveSpeed * deltaTime;
            this.transform.position += deltaPos;

            //Debug.Log("HandleDpadMove:"+ deltaPos);
        }
    }

    public void RotatePlayer(Vector3 rotateDirection)
    {
        if (true)//if (UseWorldTurnAround)
        {
            this.transform.Rotate(Vector3.up * rotateDirection.x, Space.Self);
            Camera.main.transform.Rotate(Vector3.right * rotateDirection.z, Space.Self);
            ///this.transform.LookAt(transform.position + rotateDirection);
        }
        else
        {
            this.transform.LookAt(rotateDirection);
        }
    }

    public virtual void CustomUpdate()
    {
        // Animation
        CustomUpdateAnimation();

        if (this != null && !IsDead())
        {
            Vector3 vec3Delta = m_lastPlayerPos - transform.position;
            CurMoveSpeed = vec3Delta.magnitude / 0.03f;
            if (CurMoveSpeed > MaxMoveSpeed)
            {
                CurMoveSpeed = MaxMoveSpeed;
            }

            //if (!IsAI && TeamType == PlayerTeam.PlayerTeamType.TaggerTeam && !IsLocalHuman())
            //{
            //    Debug.Log("CurMoveSpeed=" + CurMoveSpeed);
            //}
        }
        else
        {
            CurMoveSpeed = 0f;
        }
        if (this != null)
            m_lastPlayerPos = transform.position;
    }

    public float GetCurSpeed()
    {
        return CurMoveSpeed;

        float curSpeed = 0f;
        if (IsAI)
        {
            curSpeed = AIMoveSpeed;
        }
        else
        {
            if (_controller != null)
            {
                curSpeed = _controller.velocity.magnitude;
            }
            else
            {
                curSpeed = MaxMoveSpeed;

                ///Debug.Log("Error:GetCurSpeed: no CharacterController");
            }

            // for HideSeek WangHu
            if (!IsLocalHuman())
            {
                //强制播放走路动画
                curSpeed = 1.0f;

                curSpeed = CurMoveSpeed;
            }
        }

        return curSpeed;
    }

    // Animation
    private void CustomUpdateAnimation()
    {
        if (_avatarAnimator)
        {
            float curSpeed = GetCurSpeed();

            _avatarAnimator.SetFloat("Speed_f", curSpeed);
            if (curSpeed > 0.001f)
            {
                ///Debug.Log("curSpeed="+ curSpeed);
            }

            _avatarAnimator.Update(GameManager.s_deltaTime);
        }
    }

    private void EnableModelDisplay()
    {
        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer component in rendererComponents)
        {
            component.enabled = true;
        }
    }

    private void DisableModelDisplay()
    {
        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer component in rendererComponents)
        {
            component.enabled = false;
        }
    }

    public bool IsDead()
    {
        bool bIsDead = false;
        if (Hp <= 0)
            bIsDead = true;
        return bIsDead;
    }

    public void AddHP()
    {
        // Heal self

        this.Hp++;
        if (this.Hp > PlayerBase.MaxHp)
        {
            this.Hp = PlayerBase.MaxHp;
        }
    }

    public void DecHP()
    {
        // Hurt self

        this.Hp--;
        if (this.Hp <= 0)
        {
            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
            {
                // make self dead and destroy later
                MakeDead();
            }
            else
            {
                this.Hp = 1;

                // Dead event sync
                PlayerEventItem deadEvent = new PlayerEventItem();
                deadEvent.cbTeamType = (byte)TeamType;
                deadEvent.wChairId = (ushort)ChairID;
                deadEvent.cbAIId = AIId;
                deadEvent.cbEventKind = (byte)PlayerEventKind.DeadByDecHp;
                //killer
                deadEvent.nCustomData0 = (Int32)TeamType;
                deadEvent.nCustomData1 = (Int32)ChairID;
                deadEvent.nCustomData2 = (Int32)AIId;
                GameObjectsManager.GetInstance().PlayerEventList.Add(deadEvent);
            }
        }
    }

    public void SyncDead(PlayerBase.PlayerEventKind deadEventKind, PlayerBase killer)
    {
        //deadEventKind: DeadByDecHp-自己扣完血而死, DeadByPicked-被警察点死, DeadByBoom-被炸弹炸死

        Debug.LogWarning("SyncDead:" + gameObject.name + " is dead!");

        byte Gamestate = SocketDefines.GAME_STATUS_FREE;
        if (CServerItem.get() != null)
        {
            Gamestate = CServerItem.get().GetGameStatus();
        }
        if (Gamestate != SocketDefines.GAME_STATUS_PLAY)
        {
            Debug.LogError("SyncDead when Gamestate=" + Gamestate);
            return;
        }

        if (deadEventKind != PlayerBase.PlayerEventKind.DeadByDecHp && deadEventKind != PlayerBase.PlayerEventKind.DeadByPicked && deadEventKind != PlayerBase.PlayerEventKind.DeadByBoom)
        {
            Debug.LogError("SyncDead:incorrect deadEventKind=%d" + deadEventKind);
            return;
        }

        //击杀信息
        //if (gameObject.GetComponent<PlayerBase>().IsAI)
        //{
        //    UIManager.GetInstance().ShowMiddleTips(gameObject.name + "找到了: " + pickedPlayer.gameObject.name);
        //}
        //else
        //{
        //    String[] str = GlobalUserInfo.getNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        //    UIManager.GetInstance().ShowMiddleTips(str[0] + "找到了: " + pickedPlayer.gameObject.name);
        //}

        if (deadEventKind == PlayerBase.PlayerEventKind.DeadByPicked || deadEventKind == PlayerBase.PlayerEventKind.DeadByBoom)
        {
            //由服务器的HP同步
            //// Heal killer
            //if (killer != null)
            //{
            //    killer.AddHP();
            //}

            if (deadEventKind == PlayerBase.PlayerEventKind.DeadByPicked)
            {
                if (m_hnGameManager != null && killer.IsLocalHuman())  //Killer是本地玩家，播放找到躲藏者提示
                {
                    m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_HIDE_BEFOUNDED);
                }
            }
        }

        MakeDead();
    }

    public void MakeDead(bool bOffLine = false)
    {
        Debug.LogWarning("MakeDead:" + gameObject.name + " is dead!");

        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)  //联机模式判断状态
        {
            byte Gamestate = SocketDefines.GAME_STATUS_FREE;
            if (CServerItem.get() != null)
            {
                Gamestate = CServerItem.get().GetGameStatus();
            }
            if (Gamestate != SocketDefines.GAME_STATUS_PLAY)
            {
                Debug.LogError("MakeDead when Gamestate=" + Gamestate);
                return;
            }
        }

        Hp = 0;

        if (IsLocalHuman())
        {
            //切换死亡视角
            ControlManager.GetInstance().ControlModelSwitch(ControlManager.CameraControlMode.DeadMode);

            if (TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                UIManager.GetInstance().ShowTopTips("你已经被发现了！", true);
            else if (TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                UIManager.GetInstance().ShowTopTips("你已经死亡！", true);

            if (TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
            {
                //byte ModelIndex = (byte)PlayerPrefs.GetInt("ChoosedModelIndex");
                //GameObject temp = Resources.Load<GameObject>(HNGameManager.taggerPrefabPath[ModelIndex]);
                //GameObject localHuman = Instantiate(temp);
                //localHuman.transform.position = gameObject.transform.position;
                //localHuman.transform.localEulerAngles = gameObject.transform.localEulerAngles;

                //炸弹处理
                if (ControlManager.GetInstance().BoomButton != null)
                {
                    ControlManager.GetInstance().BoomButton.gameObject.SetActive(false);
                }
                InventoryManager.HaveBoom = false;

                if (!IsAI)  //死亡提示音
                    m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_SEEK_DEAD);

                if (IsAI)
                    _avatarAnimator = gameObject.transform.GetComponent<Animator>();
                else
                    _avatarAnimator = gameObject.transform.GetChild(0).transform.GetComponent<Animator>();
                if (_avatarAnimator != null)
                {
                    EnableModelDisplay();
                    _avatarAnimator.SetBool("Death_b", true);
                }

                if (UIManager.GetInstance() != null)   //警察死亡隐藏人称切换
                    UIManager.GetInstance().m_Canvas.transform.Find("Btn/PersonButton").gameObject.SetActive(false);

                //GameObject.Destroy(gameObject, 3.0f);
            }
            else
            {
                if (!IsAI)  //死亡提示音
                {
                    m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_SEEK_DEAD);
                    if (UIManager.TimeLeft > GameManager.HIDESEEK_GAME_PLAY_TIME - 60 && !bOffLine)  //复活道具使用  bOffLine仅供断线重连时判断
                    {
                        Loom.QueueOnMainThread(() =>
                        {
                            ControlManager.GetInstance().ResurrectionButton.gameObject.SetActive(true);
                            Loom.QueueOnMainThread(() =>
                            {
                                ControlManager.GetInstance().ResurrectionButton.gameObject.SetActive(false);
                            }, 10);
                        });
                    }
                    if (UIManager.GetInstance() != null)
                    {
                        UIManager.GetInstance().m_Canvas.transform.Find("Btn/ObjectSwitch").gameObject.SetActive(false);  //模型切换
                        UIManager.GetInstance().m_Canvas.transform.Find("Btn/StealthButton").gameObject.SetActive(false);
                    }
                }
                //GameObject.Destroy(gameObject);
            }
        }
        else
        {
            if (TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
            {
                if (_avatarAnimator != null)
                {
                    _avatarAnimator.SetBool("Death_b", true);
                }

                //GameObject.Destroy(gameObject, 3.0f);
            }
            else
            {
                //GameObject.Destroy(gameObject);
            }
        }

        //GameObject.Destroy(gameObject)改为隐藏，保证SendClientPlayersInfo_WangHu还能发送他的信息
        float fDelayTime = 0f;
        if (TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
        {
            fDelayTime = 3.0f;
        }
        Loom.QueueOnMainThread(() =>
        {
            if (this == null)
                return;

            if (gameObject.GetComponent<CharacterController>() != null)
                Destroy(gameObject.GetComponent<CharacterController>());
            GameObject InfoPanel = gameObject.transform.Find("InfoPanel").gameObject;
            if (InfoPanel != null) InfoPanel.SetActive(false);
            GameObject ModelObj = gameObject.transform.GetChild(0).gameObject;
            if (ModelObj.transform.childCount == 0)
            {
                if (ModelObj.transform.GetComponent<Renderer>() != null)
                    ModelObj.transform.GetComponent<Renderer>().enabled = false;
            }
            else if (ModelObj.transform.childCount > 0)
            {
                for (int j = 0; j < ModelObj.transform.childCount; j++)
                {
                    if (ModelObj.transform.GetChild(j).GetComponent<Renderer>() != null)
                        ModelObj.transform.GetChild(j).GetComponent<Renderer>().enabled = false;
                }
            }
            //MeshCollider mCollider = gameObject.GetComponent<MeshCollider>();
            //if (mCollider != null)
            //    mCollider.enabled = false;
            //mCollider = ModelObj.GetComponent<MeshCollider>();
            //if (mCollider != null)
            //    mCollider.enabled = false;
            //BoxCollider bCollider = gameObject.GetComponent<BoxCollider>();
            //if (bCollider != null)
            //    bCollider.enabled = false;
            //bCollider = ModelObj.GetComponent<BoxCollider>();
            //if (bCollider != null)
            //    bCollider.enabled = false;
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
            collider = null;
            collider = ModelObj.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            if (_controller != null)
                _controller.enabled = false;
        }, fDelayTime);

        //Stop obj behavior tree
        if (IsAI)
        {
            BehaviorDesigner.Runtime.Behavior behavior = gameObject.GetComponent<BehaviorDesigner.Runtime.Behavior>();
            if (behavior != null)
            {
                behavior.DisableBehavior();
            }
        }

        // for dead sync
    }

    public void PlayPickupAnim()
    {
        if (_avatarAnimator != null)
        {
            _avatarAnimator.SetBool("Pickup_b", true);
        }
    }

    public void PickupObject(GameObject objToPick, Vector3 pickupPos)
    {
        if (gameObject.GetComponent<PlayerBase>().TeamType == PlayerTeam.PlayerTeamType.HideTeam)  //躲藏的玩家无法执行找操作
            return;
        else
        {
            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
            {
                byte Gamestate = CServerItem.get().GetGameStatus();
                if (Gamestate != SocketDefines.GAME_STATUS_PLAY)
                    return;
            }
            else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
            {
                if (GameManager.s_singleGameStatus != SocketDefines.GAME_STATUS_PLAY)
                    return;
            }
        }
        if (objToPick == null || this.Hp <= 0)
        {
            return;
        }

        if (this.IsAI)
        {
            //AI没有用射线检测，也就没有距离判断，所以这里要加入

            Vector3 distToPick = objToPick.transform.position - this.transform.position;
            if (distToPick.magnitude > ControlManager.s_MaxTouchCheckDistance)
            {
                Debug.LogError("-----------距离过远！！！！！" + distToPick.magnitude + " " + objToPick.transform.position + " " + this.transform.position);
                //if (objToPick.tag == "Hide" || objToPick.tag == "NormalFurniture") //objToPick.name
                {
                    UIManager.GetInstance().ShowPickupTooFarText();

                    return;
                }
            }
        }

        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
        {
            // Pick event sync
            PlayerEventItem pickEvent = new PlayerEventItem();
            pickEvent.cbTeamType = (byte)TeamType;
            pickEvent.wChairId = (ushort)ChairID;
            pickEvent.cbAIId = (byte)AIId;
            pickEvent.cbEventKind = (byte)PlayerBase.PlayerEventKind.Pick;
            GameObjectsManager.GetInstance().PlayerEventList.Add(pickEvent);
        }
        else
        {
            if (IsAI || !ControlManager.isPerson_1st)  //只有第三人称才有拾取动画
            {
                if (IsLocalHuman())
                {
                    EnableModelDisplay();
                }

                PlayPickupAnim();
                //if (_avatarAnimator != null)
                //{
                //    _avatarAnimator.SetBool("Pickup_b", true);
                //}
            }
        }

        PlayerBase pickedPlayer = null;
        string strObjToPickTag = null;
        if (this.IsAI)
        {
            //this是AI时，传的objToPick是根节点（PlayerBase都在根节点）

            pickedPlayer = objToPick.GetComponent<PlayerBase>();

            if (pickedPlayer == null)
            {
                //objToPick是场景中的物体，不是躲藏者

                strObjToPickTag = objToPick.tag;
            }
            else
            {
                Transform model = pickedPlayer.transform.FindChild("Model");
                if (model != null)
                {
                    strObjToPickTag = model.tag;    //tag在model节点
                }
                else
                {
                    Debug.LogError("PickupObject IsAI: model==null");
                }
            }
        }
        else
        {
            //this不是AI时，objToPick如果是躲藏者，传的是Model子节点，否则传的也是根节点

            pickedPlayer = objToPick.transform.parent.GetComponent<PlayerBase>();
            if (pickedPlayer == null)
            {
                //objToPick是场景中的物体，不是躲藏者

                strObjToPickTag = objToPick.tag;
            }
            else
            {
                strObjToPickTag = objToPick.tag;
            }
        }
            
        if (strObjToPickTag == "Hide" || (strObjToPickTag == "LocalHuman" && pickedPlayer != null && pickedPlayer.TeamType == PlayerTeam.PlayerTeamType.HideTeam)) //objToPick.name
        {
            ObjNamePicked = pickedPlayer.gameObject.name;
            Debug.Log(gameObject.name + " touched Hide: " + ObjNamePicked);

            if (IsLocalHuman())
            {
                Debug.Log("I'm touched Hide: " + ObjNamePicked);
            }

            // Check if objToPick is alive
            if (pickedPlayer != null)
            {
                if (pickedPlayer.IsDead())
                {
                    Debug.Log("It was already dead!");
                }
                else
                {
                    if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
                    {
                        if (IsAI && pickedPlayer.m_isStealth == 1) //AI警察不点隐身的躲藏者
                            return;
                        //击杀信息
                        if (gameObject.GetComponent<PlayerBase>().IsAI)
                        {
                            //String[] str = gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            //String[] pickedStr = pickedPlayer.gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            UIManager.GetInstance().ShowMiddleTips(gameObject.name + " 找到了: " + pickedPlayer.gameObject.name);
                        }
                        else
                        {
                            String[] str = GlobalUserInfo.getNickName().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                            //String[] pickedStr = pickedPlayer.gameObject.name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                            UIManager.GetInstance().ShowMiddleTips(str[0] + " 找到了: " + pickedPlayer.gameObject.name);
                        }

                        // Heal self
                        AddHP();

                        if (m_hnGameManager != null && IsLocalHuman())  //本地玩家找到躲藏者提示
                        {
                            m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_HIDE_BEFOUNDED);
                        }

                        // make pickedPlayer dead and destroy later
                        pickedPlayer.MakeDead();
                        //if (!pickedPlayer.IsAI)
                        //{
                        //    PlayerChairIDOfPickedDead = pickedPlayer.ChairID;
                        //}
                    }
                    else
                    {
                        //Debug.LogWarning("+++++++++++++++++ " + pickedPlayer.m_isStealth + " " + IsAI);
                        if (pickedPlayer.m_isStealth == 1 && IsAI)  //AI警察不点隐身的躲藏者
                        {
                            //Debug.LogWarning("----------玩家隐身中");
                            return;
                        }
                        // Dead event sync
                        PlayerEventItem deadEvent = new PlayerEventItem();
                        deadEvent.cbTeamType = (byte)pickedPlayer.TeamType;
                        deadEvent.wChairId = (ushort)pickedPlayer.ChairID;
                        deadEvent.cbAIId = pickedPlayer.AIId;
                        deadEvent.cbEventKind = (byte)PlayerEventKind.DeadByPicked;
                        //killer
                        deadEvent.nCustomData0 = (Int32)this.TeamType;
                        deadEvent.nCustomData1 = (Int32)this.ChairID;
                        deadEvent.nCustomData2 = (Int32)this.AIId;
                        GameObjectsManager.GetInstance().PlayerEventList.Add(deadEvent);
                    }

                    //警察找到躲藏者加分
                    GameScore += 50;
                }
            }
        }
        else if (strObjToPickTag == "NormalFurniture")
        {
            ObjNamePicked = objToPick.transform.parent.name;
            Debug.Log("touched normal furniture: " + ObjNamePicked);

            UIManager.GetInstance().ShowPickupWrongUI(pickupPos);//hit.point

            if (m_hnGameManager != null && IsLocalHuman())  //本地玩家点击错误物体提示
            {
                m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_PickObj);
            }
            if (IsLocalHuman())
            {
                UIManager.GetInstance().ShowMiddleTips("该物品不是玩家！");
            }
            if (UIManager.TimeLeft <= 45)   //剩余时间小于45秒时，警察无敌
                return;

            // Hurt self
            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
            {
                DecHP();
            }
            else
            {
                PlayerEventItem decHpEvent = new PlayerEventItem();
                decHpEvent.cbTeamType = (byte)this.TeamType;
                decHpEvent.wChairId = (ushort)this.ChairID;
                decHpEvent.cbAIId = (byte)this.AIId;
                decHpEvent.cbEventKind = (byte)PlayerBase.PlayerEventKind.DecHp;
                GameObjectsManager.GetInstance().PlayerEventList.Add(decHpEvent);
            }
        }
        else if (strObjToPickTag == "Tagger")
        {
            ObjNamePicked = pickedPlayer.gameObject.name;
            Debug.Log("touched Tagger: " + ObjNamePicked);
        }
        else
        {
            Debug.LogError("PickupObject: incorrect objToPickTag="+ strObjToPickTag);
        }


        //判断是否团灭
        int taggerCount = 0;
        int hideCount = 0;
        for (PlayerTeam.PlayerTeamType teamType = PlayerTeam.PlayerTeamType.TaggerTeam; teamType < PlayerTeam.PlayerTeamType.MaxTeamNum; teamType++)
        {
            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(teamType);
            if (team == null)
                continue;
            for (int i = 0; i < team.GetPlayerNum(); i++)
            {
                PlayerBase playerBase = GameObjectsManager.GetInstance().GetPlayer(teamType, i);
                if (playerBase != null && playerBase.Hp != 0)
                {
                    if (teamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                        taggerCount++;
                    else if (teamType == PlayerTeam.PlayerTeamType.HideTeam)
                        hideCount++;
                }
            }
        }
        if (taggerCount == 0 || hideCount == 0)
        {
            if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
            {
                if (UIManager.GetInstance() != null)
                    UIManager.GetInstance().ShowWinOrLose();
                m_hnGameManager.StopSingleGame();
                m_hnGameManager.PlayAgainSingleGame();
            }
        }
    }

    public byte GetKilledCode()
    {
        //HasKilledPlayer:0x80, KilledPlayerIsAI:0x40, KilledPlayerTeamType:0x20, KilledPlayerChairID:0x1f, 
        byte cbKilledPlayer = 0;
        cbKilledPlayer |= 0x80;

        if (IsAI)
        {
            cbKilledPlayer |= 0x40;
        }

        if (TeamType == PlayerTeam.PlayerTeamType.HideTeam)
        {
            cbKilledPlayer |= 0x20;
        }

        cbKilledPlayer |= (byte)ChairID;

        return cbKilledPlayer;
    }

    public void AnimEvent_PickupEnd()
    {
        if (IsLocalHuman())
        {
            DisableModelDisplay();
        }

        if (_avatarAnimator)
        {
            _avatarAnimator.SetBool("Pickup_b", false);
        }
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            m_isGrounded = true;
            m_Jumping = false;
            m_Jump = false;
        }
        //Rigidbody body = hit.collider.attachedRigidbody;
        //if (this.transform.gameObject.name == "LocalHuman")
        //{
        //    Debug.Log("LocalHuman OnControllerColliderHit");

        //    if(!hit.collider.gameObject.name.Contains("Floor"))
        //    {
        //        Debug.Log("LocalHuman collide： " + hit.collider.gameObject.name);
        //    }
        //}

        //if (body == null || body.isKinematic)
        //{
        //    return;
        //}
        //else
        //{
        //    Debug.Log("touch gameObject： " + hit.collider.gameObject.name);

        //    //摧毁物体
        //    //Destroy(hit.collider.gameObject);

        //    //给物体一个移动的力
        //    //body.velocity = new Vector3(hit.moveDirection.x,0,hit.moveDirection.z) * 30.0f;
        //}
    }
    void OnGUI()
    {

    }
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (IsLocalHuman())
    //    {
    //        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
    //    }
    //    if (collision.collider.name == "MeshCollider")
    //    {
    //        m_isGrounded = true;
    //        m_MoveDir = Vector3.zero;
    //    }
    //}
    public void SetGameObjRenderVisible(bool bVisible)
    {
        if (Hp != 0)
        {
            Transform infoPanel = this.transform.FindChild("InfoPanel");
            if (infoPanel != null) infoPanel.gameObject.SetActive(bVisible);
            GameObject modelObj = this.transform.FindChild("Model").gameObject;
            if (modelObj != null)
            {
                if (modelObj.transform.childCount == 0)
                {
                    modelObj.transform.GetComponent<Renderer>().enabled = bVisible;
                }
                else if (modelObj.transform.childCount > 0)
                {
                    for (int j = 0; j < modelObj.transform.childCount; j++)
                        if (modelObj.transform.GetChild(j).GetComponent<Renderer>() != null)
                            modelObj.transform.GetChild(j).GetComponent<Renderer>().enabled = bVisible;
                }
            }
        }
    }
    public void SetTopInfoVisible(bool bVisible)
    {
        Transform infoPanel = this.transform.FindChild("InfoPanel");
        if (infoPanel != null)
            infoPanel.gameObject.SetActive(bVisible);
    }
    public void SetGameObjVisible(bool bVisible)
    {
        if (Hp != 0)
        {
            if(bVisible)
            {
                Transform infoPanel = this.transform.FindChild("InfoPanel");
                if (infoPanel != null)
                    infoPanel.gameObject.SetActive(bVisible);
            }
            GameObject modelObj = this.transform.FindChild("Model").gameObject;
            if (modelObj != null)
            {
                if (modelObj.transform.childCount == 0)
                {
                    modelObj.transform.GetComponent<Renderer>().enabled = bVisible;
                }
                else if (modelObj.transform.childCount > 0)
                {
                    for (int j = 0; j < modelObj.transform.childCount; j++)
                        if (modelObj.transform.GetChild(j).GetComponent<Renderer>() != null)
                            modelObj.transform.GetChild(j).GetComponent<Renderer>().enabled = bVisible;
                }
            }
            //MeshCollider mCollider = GetComponent<MeshCollider>();
            //if (mCollider != null)
            //    mCollider.enabled = bVisible;
            //BoxCollider bCollider = GetComponent<BoxCollider>();
            //if (bCollider != null)
            //    bCollider.enabled = bVisible;
            Collider collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = bVisible;
            collider = null;
            collider = modelObj.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = bVisible;

        }
    }
    public void SetGameObjAplha(float fAplha)
    {
        GameObject modelObj = this.transform.FindChild("Model").gameObject;
        Renderer _renderer = null;
        int index = 0;
        if (modelObj != null)
        {
            if (modelObj.transform.childCount == 0)
            {
                _renderer = modelObj.transform.GetComponent<Renderer>();
                if (_renderer != null)
                {
#if true//UNITY_EDITOR
                    index = _renderer.materials.Length;
                    for (int j = 0; j < index; j++)
                    {
                        if (fAplha == 1)
                            _renderer.materials[j].shader = Shader.Find("Legacy Shaders/Diffuse");
                        else
                            _renderer.materials[j].shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                        Color color = _renderer.materials[j].color;
                        color.a = fAplha;
                        _renderer.materials[j].SetColor("_Color", color);
                    }
#else
                    index = _renderer.sharedMaterials.Length;
                    for (int j = 0; j < index; j++)
                    {
                        if (fAplha == 1)
                            _renderer.sharedMaterials[j].shader = Shader.Find("Legacy Shaders/Diffuse");
                        else
                            _renderer.sharedMaterials[j].shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                        Color color = _renderer.sharedMaterials[j].color;
                        color.a = fAplha;
                        _renderer.sharedMaterials[j].SetColor("_Color", color);
                    }
#endif
                }

            }
            else if (modelObj.transform.childCount > 0)
            {
                for (int i = 0; i < modelObj.transform.childCount; i++)
                {
                    _renderer = modelObj.transform.GetChild(i).GetComponent<Renderer>();
                    if (_renderer != null)
                    {
#if true//UNITY_EDITOR
                        index = _renderer.materials.Length;
                        for (int j = 0; j < index; j++)
                        {
                            if (fAplha == 1)
                                _renderer.materials[j].shader = Shader.Find("Legacy Shaders/Diffuse");
                            else
                                _renderer.materials[j].shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                            Color color = _renderer.materials[j].color;
                            color.a = fAplha;
                            _renderer.materials[j].SetColor("_Color", color);
                        }
#else
                        index = _renderer.sharedMaterials.Length;
                        for (int j = 0; j < index; j++)
                        {
                            if (fAplha == 1)
                                _renderer.sharedMaterials[j].shader = Shader.Find("Legacy Shaders/Diffuse");
                            else
                                _renderer.sharedMaterials[j].shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                            Color color = _renderer.sharedMaterials[j].color;
                            color.a = fAplha;
                            _renderer.sharedMaterials[j].SetColor("_Color", color);
                        }
#endif
                    }
                }
            }
        }
    }
}
