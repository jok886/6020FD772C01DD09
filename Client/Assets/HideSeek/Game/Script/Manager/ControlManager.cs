using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using GameNet;

public class ControlManager
{
    public enum CameraControlMode
    {
        PlayerViewMode = 0,
        FreeCameraMode,
        DeadMode,
        LookOnMode
    }
    public enum InventoryItemID
    {
        ChangeModel,
        Stealth,
        Resurrection
    }
    public class DPadInfo
    {
        public Int64 FrameNum;    // FrameNum is used to match corresponding ButtonInfo
        public PlayerTeam.PlayerTeamType TeamType;
        public Int64 PlayerIndexInTeam;
        //public float H;         // Horizontal
        //public float V;         // Vertical
        public Vector3 WorldDirOfDpadL;
        public Vector3 WorldDirOfDpadR;
    }

    // 单例
    private static ControlManager _instance = null;
    public static ControlManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new ControlManager();
        }
        return _instance;
    }

    public static bool s_IsFirstPersonControl = true;
    public static bool s_UseETCBuildInSys = true;
    public static float s_MaxTouchCheckDistance = 6f;

    public static float s_speed;

    public ETCJoystick _etcJoystickL;
    public ETCJoystick _etcJoystickR;
    public ETCTouchPad _etcTouchPadR;
    public ETCDPad _etcDPadL;

    private GameObject CameraControl;
    private GameObject Canvas;
    private Button LockButton;//锁定
    private Button JumpButton;//跳
    public Button BoomButton;//炸弹
    private Button CrouchButton;//蹲
    private Button ChatButton;//聊天
    private Button PersonButton;//人称切换
    private Button ChangeModelButton;//变身
    public Button StealthButton;//隐身
    public Button ResurrectionButton;//复活
    private ETCButton _etcButtonUp;
    private ETCButton _etcButtonDown;

    public static bool m_Up = false;
    public static bool m_Down = false;
    public static bool isCrouch = false;
    public static bool isPerson_1st = true;

    private List<DPadInfo> _dPadInfoListFromServer;
    private List<Int64> _frameArrayFromServer;

    public int GameFrameNum { get; set; }

    // Pickup
    private float _timeSinceLastPickup = 0f;
    private long currentTime = 0;

    public ControlManager()
    {
        _dPadInfoListFromServer = new List<DPadInfo>();
        _frameArrayFromServer = new List<Int64>();
        Init();
    }

    private HNGameManager m_hnGameManager = null;

    public void Init()
    {
        s_speed = PlayerBase.localHumanSpeed;

        ButtonInit();

        JoystickInit();

        ShowButtonUI();

        m_hnGameManager = GameObject.FindObjectOfType<HNGameManager>();
    }

    public void JoystickInit()
    {
        _etcJoystickL = ETCInput.GetControlJoystick("JoystickL");
        _etcDPadL = ETCInput.GetControlDPad("DPadL");
        _etcJoystickR = ETCInput.GetControlJoystick("JoystickR");
        _etcTouchPadR = ETCInput.GetControlTouchPad("TouchPadR");

        if (_etcJoystickL == null || _etcDPadL == null || _etcJoystickR == null || _etcTouchPadR == null)
            return;

        if (HNGameManager.ControlCase == 2)
        {
            _etcJoystickL.axisX.speed = 80f;
            _etcJoystickL.axisY.speed = 80f;
            _etcJoystickL.axisY.invertedAxis = true;
        }
        else
        {
            _etcJoystickL.axisX.speed = s_speed;
            _etcJoystickL.axisY.speed = s_speed;
            _etcJoystickL.axisY.invertedAxis = false;
            _etcJoystickL.joystickType = ETCJoystick.JoystickType.Dynamic;  //左摇杆采用动态显示
            _etcJoystickL.joystickArea = ETCJoystick.JoystickArea.UserDefined;
        }
        _etcDPadL.axisX.speed = s_speed;
        _etcDPadL.axisY.speed = s_speed;
        _etcJoystickR.axisX.speed = 80f;
        _etcJoystickR.axisY.speed = 80f;
        _etcTouchPadR.axisX.speed = 10f;
        _etcTouchPadR.axisY.speed = 10f;
        _etcTouchPadR.axisY.invertedAxis = true;


        HNGameManager.SliderL.GetComponent<Slider>().value = _etcJoystickL.axisX.speed / 10;
        HNGameManager.TextL.GetComponent<Text>().text = _etcJoystickL.axisX.speed.ToString();
        HNGameManager.SliderR.GetComponent<Slider>().value = _etcJoystickR.axisX.speed / 200;
        HNGameManager.TextR.GetComponent<Text>().text = _etcJoystickR.axisX.speed.ToString();

        GameObject LocalHuman = GameObject.FindGameObjectWithTag("LocalHuman");
        if (ControlManager.s_UseETCBuildInSys)
        {
            if (HNGameManager.ControlCase == 0) //TouchPad   
            {
                _etcJoystickL.activated = true;
                _etcJoystickL.visible = true;
                _etcJoystickR.activated = false;
                _etcJoystickR.visible = false;
                _etcTouchPadR.activated = true;
                _etcTouchPadR.visible = false;
                _etcDPadL.activated = false;
                _etcDPadL.visible = false;
                if (UIManager.GetInstance() != null)
                    UIManager.GetInstance().ControlSpriteR.SetActive(true);

                //Left
                if (LocalHuman != null)
                {
                    ETCInput.SetAxisDirecTransform("HorizontalL", LocalHuman.transform);
                    ETCInput.SetAxisDirecTransform("VerticalL", LocalHuman.transform);
                }
                ETCInput.SetAxisDirectAction("HorizontalL", ETCAxis.DirectAction.TranslateLocal);
                ETCInput.SetAxisAffectedAxis("HorizontalL", ETCAxis.AxisInfluenced.X);
                ETCInput.SetAxisDirectAction("VerticalL", ETCAxis.DirectAction.TranslateLocal);
                ETCInput.SetAxisAffectedAxis("VerticalL", ETCAxis.AxisInfluenced.Z);
                //Right
                if (LocalHuman != null)
                {
                    ETCInput.SetAxisDirecTransform("HorizontalTR", LocalHuman.transform);
                    ETCInput.SetAxisDirecTransform("VerticalTR", Camera.main.transform);
                }
                ETCInput.SetAxisDirectAction("HorizontalTR", ETCAxis.DirectAction.RotateLocal);
                ETCInput.SetAxisAffectedAxis("HorizontalTR", ETCAxis.AxisInfluenced.Y);
                ETCInput.SetAxisDirectAction("VerticalTR", ETCAxis.DirectAction.RotateLocal);
                ETCInput.SetAxisAffectedAxis("VerticalTR", ETCAxis.AxisInfluenced.X);

            }
            else if (HNGameManager.ControlCase == 1)  //D-Pad
            {
                _etcJoystickL.activated = false;
                _etcJoystickL.visible = false;
                _etcJoystickR.activated = false;
                _etcJoystickR.visible = false;
                _etcTouchPadR.activated = true;
                _etcTouchPadR.visible = false;
                _etcDPadL.activated = true;
                _etcDPadL.visible = true;
                if (UIManager.GetInstance() != null)
                    UIManager.GetInstance().ControlSpriteR.SetActive(true);

                //Left
                if (LocalHuman != null)
                    ETCInput.SetAxisDirecTransform("HorizontalDL", LocalHuman.transform);
                ETCInput.SetAxisDirectAction("HorizontalDL", ETCAxis.DirectAction.TranslateLocal);
                ETCInput.SetAxisAffectedAxis("HorizontalDL", ETCAxis.AxisInfluenced.X);
                ETCInput.SetAxisDirecTransform("VerticalDL", LocalHuman.transform);
                ETCInput.SetAxisDirectAction("VerticalDL", ETCAxis.DirectAction.TranslateLocal);
                ETCInput.SetAxisAffectedAxis("VerticalDL", ETCAxis.AxisInfluenced.Z);
                //Right
                if (LocalHuman != null)
                    ETCInput.SetAxisDirecTransform("HorizontalTR", LocalHuman.transform);
                ETCInput.SetAxisDirectAction("HorizontalTR", ETCAxis.DirectAction.RotateLocal);
                ETCInput.SetAxisAffectedAxis("HorizontalTR", ETCAxis.AxisInfluenced.Y);
                ETCInput.SetAxisDirecTransform("VerticalTR", Camera.main.transform);
                ETCInput.SetAxisDirectAction("VerticalTR", ETCAxis.DirectAction.RotateLocal);
                ETCInput.SetAxisAffectedAxis("VerticalTR", ETCAxis.AxisInfluenced.X);
            }
            else if (HNGameManager.ControlCase == 2)  //Single
            {
                _etcJoystickL.activated = true;
                _etcJoystickL.visible = true;
                _etcJoystickR.activated = false;
                _etcJoystickR.visible = false;
                _etcTouchPadR.activated = false;
                _etcTouchPadR.visible = false;
                _etcDPadL.activated = true;
                _etcDPadL.visible = true;
                if (UIManager.GetInstance() != null)
                    UIManager.GetInstance().ControlSpriteR.SetActive(false);

                //Left
                if (LocalHuman != null)
                    ETCInput.SetAxisDirecTransform("HorizontalDL", LocalHuman.transform);
                ETCInput.SetAxisDirectAction("HorizontalDL", ETCAxis.DirectAction.TranslateLocal);
                ETCInput.SetAxisAffectedAxis("HorizontalDL", ETCAxis.AxisInfluenced.X);
                ETCInput.SetAxisDirecTransform("VerticalDL", LocalHuman.transform);
                ETCInput.SetAxisDirectAction("VerticalDL", ETCAxis.DirectAction.TranslateLocal);
                ETCInput.SetAxisAffectedAxis("VerticalDL", ETCAxis.AxisInfluenced.Z);
                //Right
                if (LocalHuman != null)
                    ETCInput.SetAxisDirecTransform("HorizontalL", LocalHuman.transform);
                ETCInput.SetAxisDirectAction("HorizontalL", ETCAxis.DirectAction.RotateLocal);
                ETCInput.SetAxisAffectedAxis("HorizontalL", ETCAxis.AxisInfluenced.Y);
                ETCInput.SetAxisDirecTransform("VerticalL", Camera.main.transform);
                ETCInput.SetAxisDirectAction("VerticalL", ETCAxis.DirectAction.RotateLocal);
                ETCInput.SetAxisAffectedAxis("VerticalL", ETCAxis.AxisInfluenced.X);
            }
            else if (HNGameManager.ControlCase == 3) //Joystick
            {
                _etcJoystickL.activated = true;
                _etcJoystickL.visible = true;
                _etcJoystickR.activated = true;
                _etcJoystickR.visible = true;
                _etcTouchPadR.activated = false;
                _etcTouchPadR.visible = false;
                _etcDPadL.activated = false;
                _etcDPadL.visible = false;
                if (UIManager.GetInstance() != null)
                    UIManager.GetInstance().ControlSpriteR.SetActive(false);

                //Left
                if (LocalHuman != null)
                    ETCInput.SetAxisDirecTransform("HorizontalL", LocalHuman.transform);
                ETCInput.SetAxisDirectAction("HorizontalL", ETCAxis.DirectAction.TranslateLocal);
                ETCInput.SetAxisAffectedAxis("HorizontalL", ETCAxis.AxisInfluenced.X);
                ETCInput.SetAxisDirecTransform("VerticalL", LocalHuman.transform);
                ETCInput.SetAxisDirectAction("VerticalL", ETCAxis.DirectAction.TranslateLocal);
                ETCInput.SetAxisAffectedAxis("VerticalL", ETCAxis.AxisInfluenced.Z);
                //Right
                if (LocalHuman != null)
                    ETCInput.SetAxisDirecTransform("HorizontalR", LocalHuman.transform);
                ETCInput.SetAxisDirectAction("HorizontalR", ETCAxis.DirectAction.RotateLocal);
                ETCInput.SetAxisAffectedAxis("HorizontalR", ETCAxis.AxisInfluenced.Y);
                ETCInput.SetAxisDirecTransform("VerticalR", Camera.main.transform);
                ETCInput.SetAxisDirectAction("VerticalR", ETCAxis.DirectAction.RotateLocal);
                ETCInput.SetAxisAffectedAxis("VerticalR", ETCAxis.AxisInfluenced.X);

                //_etcJoystickR.joystickType = ETCJoystick.JoystickType.Static;
                //_etcJoystickR.joystickArea = ETCJoystick.JoystickArea.UserDefined;
                //Sprite[] ctrName = Resources.LoadAll<Sprite>("UI/Joystick/ETCDefaultSprite");
                //Color color = new Color(255, 255, 255, 1);
                //ETCInput.SetControlSprite("JoystickR", ctrName[0], color);  //Background Sprite
                //ETCInput.SetJoystickThumbSprite("JoystickR", ctrName[7], color);  //Thumb
                //if (UIManager.GetInstance() != null)
                //    UIManager.GetInstance().JoyStick.SetActive(false);

                //_etcJoystickR.gameObject.transform.position = UIManager.GetInstance().JoyStick.transform.position;
                //_etcJoystickR.visible = true;
                //_etcJoystickR.axisX.speed = 80f;  //52.6f
                //_etcJoystickR.axisY.speed = 80f;
            }

        }
    }
    public void ButtonInit()
    {
        if (CameraControl == null)
            CameraControl = GameObject.Find("CameraControl");
        if (Canvas == null)
            Canvas = GameObject.Find("CanvasGamePaly_demo");

        if (Canvas == null)
            return;
        if (LockButton == null)
            LockButton = Canvas.transform.Find("Btn/LockButton").GetComponent<Button>();
        if (JumpButton == null)
            JumpButton = Canvas.transform.Find("Btn/JumpButton").GetComponent<Button>();
        if (CrouchButton == null)
            CrouchButton = Canvas.transform.Find("Btn/CrouchButton").GetComponent<Button>();
        if (_etcButtonUp == null)
            _etcButtonUp = ETCInput.GetControlButton("UpButton");
        if (_etcButtonDown == null)
            _etcButtonDown = ETCInput.GetControlButton("DownButton");
        if (BoomButton == null)
            BoomButton = Canvas.transform.Find("Btn/Boom").gameObject.GetComponent<Button>();
        if (BoomButton.gameObject != null)
            BoomButton.gameObject.SetActive(false);

        if (ChatButton == null)
            ChatButton = Canvas.transform.Find("Chat/ChatButton").gameObject.GetComponent<Button>();
        if (ChatButton.gameObject != null)
            ChatButton.gameObject.SetActive(true);
        if (PersonButton == null)
            PersonButton = Canvas.transform.Find("Btn/PersonButton").gameObject.GetComponent<Button>();
        if (PersonButton.gameObject != null)
            PersonButton.gameObject.SetActive(false);

        if (ChangeModelButton == null)
            ChangeModelButton = Canvas.transform.Find("Btn/ObjectSwitch").gameObject.GetComponent<Button>();
        if (ChangeModelButton.gameObject != null)
            ChangeModelButton.gameObject.SetActive(false);
        if (StealthButton == null)
            StealthButton = Canvas.transform.Find("Btn/StealthButton").gameObject.GetComponent<Button>();
        if (StealthButton.gameObject != null)
            StealthButton.gameObject.SetActive(false);
        if (ResurrectionButton == null)
            ResurrectionButton = Canvas.transform.Find("Btn/ResurrectionButton").gameObject.GetComponent<Button>();
        if (ResurrectionButton.gameObject != null)
            ResurrectionButton.gameObject.SetActive(false);
        RemoveListener();  //按键初始化移除所有监听器
        AddListenner();
    }
    public void DetectLocalControl()
    {
        //if (!_canBeControlled)
        //{
        //    return;
        //}

        if (!ControlManager.s_UseETCBuildInSys)
        {
            // Dpad
            DetectLocalDpad();
        }

        // Button
        // called DetectAndHandleLocalButton(...) in ButtonA, ButtonB, ...
    }

    private void DetectLocalDpad()
    {
        float hL = ETCInput.GetAxis("HorizontalL");
        float vL = ETCInput.GetAxis("VerticalL");
        Vector3 worldDirOfDpadL = GetWorldDirFromAxisValue(hL, vL);

        float hR = ETCInput.GetAxis("HorizontalR");
        float vR = ETCInput.GetAxis("VerticalR");
        Vector3 worldDirOfDpadR = GetWorldDirFromAxisValue(hR, vR);

        // Send sendData
        /////////////////////////////////////
        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            // SingleGame

            DPadInfo dPadInfo = new DPadInfo();
            dPadInfo.TeamType = PlayerTeam.PlayerTeamType.TaggerTeam;
            dPadInfo.PlayerIndexInTeam = 0;
            dPadInfo.WorldDirOfDpadL = worldDirOfDpadL;
            dPadInfo.WorldDirOfDpadR = worldDirOfDpadR;

            SendToLocalServer(dPadInfo); ///HandleDpadMsg(dPadInfo);
        }
        else
        {
            // MultiGame

            //SprotoType.clientControl.request req = new SprotoType.clientControl.request
            //{
            //    x = BitConverter.ToInt64(BitConverter.GetBytes((double)sendData.x), 0),
            //    y = BitConverter.ToInt64(BitConverter.GetBytes((double)sendData.z), 0)
            //};
            //NetSender.Send<Protocol.clientControl>(req);
        }
    }

    private Vector3 GetWorldDirFromAxisValue(float axisX, float axisY)
    {
        Vector3 worldDirection = new Vector3(axisX, 0, axisY);

        if (false/*ControlManager.s_IsFirstPersonControl*/)
        {
            return worldDirection;
        }
        else
        {
            if (axisX != 0f || axisY != 0f)
            {
                Vector3 newDirection = worldDirection;

                // Method x:
                newDirection = newDirection.normalized;
                //Camera.main.ScreenToWorldPoint
                //Create a rotation facing that point.
                ///Quaternion targetRotation = Quaternion.LookRotation(newDirection);


                // Method 0: OK 
                // calculate camera relative direction to move:
                Vector3 camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 camRight = Camera.main.transform.right;
                Vector3 moveDirection0 = axisY * camForward + axisX * camRight;


                // Method 1: OK
                // Forward vector relative to the camera along the x-z plane. 相对于相机的xz屏幕的前方
                Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
                forward.y = 0;
                forward = forward.normalized;

                // Right vector relative to the camera.相对于相机的右
                // Always orthogonal to the forward vector.总是垂直于前方向量
                Vector3 right = new Vector3(forward.z, 0, -forward.x);

                // Target direction relative to the camera.相对于相机的方向
                Vector3 targetDirection = axisX * right + axisY * forward;

                Vector3 moveDirection1 = targetDirection.normalized;
                //Create a rotation facing that point.
                ///Quaternion targetRotation = Quaternion.LookRotation(moveDirection1);



                // Method 2: OK
                Vector3 moveDirection2 = new Vector3(axisX, 0, axisY);
                moveDirection2 = Camera.main.transform.TransformDirection(moveDirection2);
                moveDirection2.y = 0f;
                moveDirection2 = moveDirection2.normalized;
                //Create a rotation facing that point.
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection2);



                // Method x:
                float angle = Mathf.Atan2(axisY, axisX) * Mathf.Rad2Deg;
                // Do something with the angle here.
                //this.transform.Rotate(0, angle, 0);
                //this.transform.rotation = Quaternion.Euler(0, angle, 0);



                // Set rotation to the move direction
                ///transform.rotation = targetRotation;

                worldDirection = moveDirection0;
            }
        }

        return worldDirection;
    }

    public void SendToLocalServer(object controlInfo)
    {
        // Send msg
        // ...

        // Assume LocalServer receive msg immediately
        LocalGameServer.GetInstance().ReceiveControlMsgFromClient(controlInfo);
    }

    public void ReceiveHeartbeatMsgFromServer(LocalGameServer.HeartbeatMsg heartbeatMsgFromServer)
    {
        if (heartbeatMsgFromServer.HasDpadMsg)
        {
            foreach (ControlManager.DPadInfo dPadMsg in heartbeatMsgFromServer.DPadMsgList)
            {
                AddServerHeartbeatMsgInfo(dPadMsg);
            }
        }

        //添加一般帧信息
        AddServerHeartbeatMsgInfo(heartbeatMsgFromServer.GameFrameNum);
    }

    private void AddServerHeartbeatMsgInfo(object controlInfo)
    {
        if (GameManager.LockObj == null)
        {
            GameManager.LockObj = new object();
        }

        DPadInfo dPadInfo = controlInfo as DPadInfo;
        if (dPadInfo != null)
        {
            lock (GameManager.LockObj)
                _dPadInfoListFromServer.Add(dPadInfo);

            return;
        }

        Int64 frameNum = (Int64)controlInfo;
        lock (GameManager.LockObj)
            _frameArrayFromServer.Add(frameNum);
    }



    public bool HaveServerMsg()
    {
        return _frameArrayFromServer.Count > 0;
    }



    private void CheckAndHandleReceivedHeartbeatMsgFromServer()
    {
        lock (GameManager.LockObj)
        {
            if (!ControlManager.s_UseETCBuildInSys)
            {
                if (_dPadInfoListFromServer.Count > 0)
                {
                    DPadInfo dPadInfo = _dPadInfoListFromServer[0];
                    if (dPadInfo == null)
                    {
                        Debug.Log("Error in dpad info array!!");
                    }
                    while (dPadInfo.FrameNum == GameFrameNum)
                    {
                        //Debug.Log("Handle Dpad msg Frame: " + _frameArrayFromServer[0]);
                        HandleDpadMsg(dPadInfo);
                        //Debug.Log("Dpad for " + dPadInfo.PlayerIndexInTeam + "on frame " + GameFrameNum);

                        _dPadInfoListFromServer.RemoveAt(0);
                        if (_dPadInfoListFromServer.Count > 0)
                        {
                            dPadInfo = _dPadInfoListFromServer[0];
                            if (dPadInfo == null)
                            {
                                Debug.Log("Error in dpad info array after remove!!");
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                }
            }

            //if (_buttonInfoArray.Count > 0)
            //{
            //}

            _frameArrayFromServer.RemoveAt(0);
        }

        GameFrameNum++;
    }


    private void HandleDpadMsg(DPadInfo dPadInfo)
    {
        PlayerBase player = GameObjectsManager.GetInstance().GetPlayer(dPadInfo.TeamType, (int)dPadInfo.PlayerIndexInTeam);
        player.MovePlayer(dPadInfo.WorldDirOfDpadL);
        player.RotatePlayer(dPadInfo.WorldDirOfDpadR);
    }

    public void PickupDetect()
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman == null)
        {
            return;
        }
        else if (localHuman.Hp <= 0)
        {
            // I'm dead
            return;
        }

        _timeSinceLastPickup += GameManager.s_deltaTime;
        if (_timeSinceLastPickup < 1.0f)
        {
            return;
        }

        Vector3 pickupPosOfScreen = new Vector3();
        //if (!Input.GetMouseButtonDown(0))
        //{
        //    return;
        //}
        pickupPosOfScreen = Input.mousePosition;
        //#if UNITY_IOS || UNITY_ANDROID
        //        if (Input.touchCount <= 0)
        //        {
        //            return;
        //        }
        //        else if (Input.touches[0].phase != TouchPhase.Began)
        //        {
        //            return;
        //        }
        //        Vector2 touchPos = Input.GetTouch(0).position;
        //        pickupPosOfScreen = new Vector3(touchPos.x, touchPos.y, 0f); //Input.GetTouch(0).position;
        //#else
        //                if (!Input.GetMouseButtonDown(0))
        //                {
        //                    return;
        //                }
        //                pickupPosOfScreen = Input.mousePosition;
        //#endif
#if UNITY_IOS || UNITY_ANDROID
        Vector2 touchPos = Vector2.zero;
        bool bHasValidTouch = false;
        if (Input.touchCount <= 0)
        {
            return;
        }
        else
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.touches[i].phase == TouchPhase.Began)
                {
                    bHasValidTouch = true;
                    touchPos = Input.GetTouch(i).position;
                    break;
                }
            }
        }
        if (!bHasValidTouch)
        {
            return;
        }
        pickupPosOfScreen = new Vector3(touchPos.x, touchPos.y, 0f); //Input.GetTouch(0).position;
        //Debug.Log("鼠标的屏幕坐标空间位置转射线：" + pickupPosOfScreen.x + " " + pickupPosOfScreen.y + " " + pickupPosOfScreen.z);
#else
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }
        pickupPosOfScreen = Input.mousePosition;
#endif

        // Check if is Dpad-touch
        //If Dpad pressed, return
        //if (ETCInput.GetButtonDown("UpButton") || ETCInput.GetButtonDown("DownButton"))
        //{
        //    return;
        //}
        if (_etcJoystickL != null)
        {
            Vector3 distL = _etcJoystickL.transform.position - pickupPosOfScreen;
            Vector3 distR = _etcJoystickR.transform.position - pickupPosOfScreen;
            RectTransform rectTransL = _etcJoystickL.GetComponent<RectTransform>();
            RectTransform rectTransR = _etcJoystickR.GetComponent<RectTransform>();
            if (distL.magnitude < rectTransL.sizeDelta.x * 1.2f || distR.magnitude < rectTransR.sizeDelta.x * 1.2f)
            {
                //Debug.Log("点击处有按键");
                return;
            }
        }
        #region 剔除UI处检测
        //跳
        Vector3 jumpDist = JumpButton.gameObject.transform.position - pickupPosOfScreen;
        jumpDist = new Vector3(jumpDist.x, jumpDist.y, 0);
        if (jumpDist.magnitude < JumpButton.gameObject.GetComponent<RectTransform>().sizeDelta.magnitude * 0.5f)
            return;
        //锁
        Vector3 lockDist = LockButton.gameObject.transform.position - pickupPosOfScreen;
        lockDist = new Vector3(lockDist.x, lockDist.y, 0);
        if (lockDist.magnitude < LockButton.gameObject.GetComponent<RectTransform>().sizeDelta.magnitude * 0.5f)
            return;
        //蹲
        Vector3 crouchDist = CrouchButton.gameObject.transform.position - pickupPosOfScreen;
        crouchDist = new Vector3(crouchDist.x, crouchDist.y, 0);
        if (crouchDist.magnitude < CrouchButton.gameObject.GetComponent<RectTransform>().sizeDelta.magnitude * 0.5f)
            return;
        //炸弹
        Vector3 boomDist = BoomButton.gameObject.transform.position - pickupPosOfScreen;
        boomDist = new Vector3(boomDist.x, boomDist.y, 0);
        if (boomDist.magnitude < BoomButton.gameObject.GetComponent<RectTransform>().sizeDelta.magnitude * 0.5f)
            return;
        #endregion
        #region 击杀点击范围判断
        //获取设备宽高    
        int device_width = Screen.width;
        int device_height = Screen.height;
        if (pickupPosOfScreen.x < device_width * 0.25 || pickupPosOfScreen.x > device_width * 0.75 ||
            pickupPosOfScreen.y < device_height * 0.2 || pickupPosOfScreen.y > device_height * 0.8)
        {
            //Debug.LogError("----------: " + device_width + " " + device_height);
            //Debug.LogError("++++++++++: " + pickupPosOfScreen.x + " " + pickupPosOfScreen.y);
            Debug.Log("点击超出判断范围");
            return;
        }
        #endregion

        //鼠标的屏幕坐标空间位置转射线: 创建一条射线，产生的射线是在世界空间中，从相机的近裁剪面开始并穿过屏幕position(x,y)像素坐标（position.z被忽略）  
        Ray ray = Camera.main.ScreenPointToRay(pickupPosOfScreen);
        //RaycastHit是一个结构体对象，用来储存射线返回的信息  
        RaycastHit hitWithNoDistLimit;
        RaycastHit hit;
        LayerMask mask = 1 << 0 | ~(0 << 8);   //只判断Default层级
        //如果射线碰撞到对象，把返回信息储存到hit中
        bool isHit = Physics.Raycast(ray, out hit, s_MaxTouchCheckDistance, mask);
        //Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red);
        //if (hit.transform.gameObject != null)
        //    Debug.LogError("+++++++++++++++++++++++++++" + hit.transform.gameObject);
        //if (hit.transform.gameObject.transform.parent != null)
        //    Debug.LogError("+++++++++++++++++++++++++++" + hit.transform.gameObject.transform.parent);
        if (isHit &&
            (hit.transform.gameObject.tag == "Hide" || hit.transform.gameObject.tag == "NormalFurniture")    // hit.transform.gameObject.tag!="Untagged"
            )
        {
            long temp = DateTime.Now.Ticks;  //此属性的值为自 0001 年 1 月 1 日午夜 12:00 以来所经过时间以 100 毫微秒为间隔表示时的数字。
            if (temp - currentTime > 2 * Math.Pow(10, 7))   //限制点击频率-2s
            {
                //Debug.LogWarning("-------------------------------" + hit.transform.gameObject);
                localHuman.PickupObject(hit.transform.gameObject, hit.point);
                currentTime = temp;
            }
        }
        /*
        bool isNoDistLimitHit = Physics.Raycast(ray, out hitWithNoDistLimit);
        if (isNoDistLimitHit && !isHit)
        {
            if (hitWithNoDistLimit.transform.gameObject.tag == "Hide" || hitWithNoDistLimit.transform.gameObject.tag == "NormalFurniture") //hit.transform.gameObject.name
            {
                UIManager.GetInstance().ShowPickupTooFarText();

                return;
            }
        }
        //*/
    }


    public void CustomUpdate()
    {
        PickupDetect();

        CheckAndHandleReceivedHeartbeatMsgFromServer();
    }
    public void ClickChangeModelButton()
    {
        if(GameManager.s_gameSingleMultiType==GameSingleMultiType.MultiGame_WangHu)
        {
            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
            if (kernel != null)
                kernel.SendInventoryConsumption((byte)InventoryItemID.ChangeModel);
        }
        else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            int ModelIndex = UnityEngine.Random.Range(0, 256);
            ChangeModel(ModelIndex);
        }
    }
    public void ClickStealthButton()
    {
        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
        {
            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
            if (kernel != null)
                kernel.SendInventoryConsumption((byte)InventoryItemID.Stealth);
        }
        else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            Stealth(false);
            UIManager.GetInstance().StartSingleStealthTime(15);
            UIManager.GetInstance().StartColdTime(StealthButton, 60);
        }
    }
    public void ClickResurrectionButton()
    {
        if (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
        {
            var kernel = (GameScene)CServerItem.get().GetClientKernelSink();
            if (kernel != null)
                kernel.SendInventoryConsumption((byte)InventoryItemID.Resurrection);
        }
        else if (GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
        {
            Resurrection();
            Stealth(false);
            UIManager.GetInstance().StartSingleStealthTime(5);
        }

    }
    public void ClickPersonButton()  //切换人称
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman != null)
        {
            GameObject ModelObj = localHuman.transform.FindChild("Model").transform.gameObject;
            if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
            {
                if (isPerson_1st)
                {
                    if (Camera.main.transform.parent != null)
                    {
                        Camera.main.transform.localPosition = new Vector3(0, 2.4f, -3);  //切换为第三人称
                    }
                    isPerson_1st = false;
                    Camera.main.transform.gameObject.GetComponent<ObstructTransparent>().enabled = true;  //开启射线透明效果
                    localHuman.SetGameObjRenderVisible(true);
                }
                else
                {
                    if (Camera.main.transform.parent != null)
                    {
                        Camera.main.transform.localPosition = new Vector3(0, 1.7f, 0);  //切换为第一人称
                        ModelObj.transform.localEulerAngles = new Vector3(0, Camera.main.transform.localEulerAngles.y, 0);
                    }
                    isPerson_1st = true;
                    Camera.main.transform.gameObject.GetComponent<ObstructTransparent>().enabled = false;  //关闭射线透明效果
                    localHuman.SetGameObjRenderVisible(false);
                    ObstructTransparent OT = GameObject.FindObjectOfType<ObstructTransparent>();
                    if (OT != null)
                        OT.ClearAllMaterialsColor();
                }
                //HNGameManager.CameraLocalPos = Camera.main.transform.localPosition;
            }
        }
    }
    public void ClickJumpButton()  //跳
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman == null)
            return;
        if (!localHuman.m_Jump && !localHuman.m_Lock && !localHuman.m_Jumping && localHuman.m_isGrounded)
        {
            localHuman.m_Jump = true;
            //localHuman.gameObject.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePositionY;

            if (m_hnGameManager != null)
            {
                m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_Jump);
            }
        }
    }
    public void ClickCrouchButton()  //蹲
    {
        if (Camera.main.transform.parent != null)
        {
            if (isCrouch)
            {
                Camera.main.transform.parent.localPosition = new Vector3(0, 0, 0);
                isCrouch = false;
            }
            else
            {
                Camera.main.transform.parent.localPosition = new Vector3(0, -1.5f, 0);
                isCrouch = true;
            }
            if (m_hnGameManager != null)
            {
                m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_EFFECT_Crouch);
            }
        }
    }
    public void ClickLockButton()
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman == null)
            return;

        if (!localHuman.m_Lock)
        {
            localHuman.m_Lock = true;
            m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_HIDE_LOCK);
        }
        else
        {
            localHuman.m_Lock = false;
            m_hnGameManager.PlaySoundEffect(-1, (int)AudioManager.Sound_Effect_Defines.SOUND_HIDE_UNLOCK);
        }

        if (localHuman.m_Lock)
        {
            //_etcJoystickL.axisX.autoLinkTagPlayer = false;
            //_etcJoystickL.axisY.autoLinkTagPlayer = false;
            JumpButton.gameObject.SetActive(false);
            //_etcJoystickL.axisX.directTransform = null;
            //_etcJoystickL.axisY.directTransform = null;
            //_etcJoystickR.axisX.directTransform = CameraControl.transform;
            //ETCInput.SetAxisDirecTransform("VerticalR", Camera.main.transform);
            ControlSwitchL(null);
            ControlSwitchR(CameraControl.transform, Camera.main.transform);
            localHuman.gameObject.GetComponent<CharacterController>().enabled = false;
            CameraControl.GetComponent<CameraControlManager>().enabled = true;
            Vector3 CCPos = Camera.main.transform.position;
            Vector3 CCEul = Vector3.zero;
            //CCEul.x = Camera.main.transform.localEulerAngles.x;
            CCEul.y = localHuman.transform.localEulerAngles.y;
            CameraControl.transform.SetParent(null, false);
            CameraControl.transform.position = CCPos;
            CameraControl.transform.localEulerAngles = CCEul;
            Camera.main.transform.localPosition = Vector3.zero;
            //Camera.main.transform.localEulerAngles = Vector3.zero;
            //LockButton.gameObject.GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/Grey", typeof(Sprite)) as Sprite;
            //LockButton.gameObject.transform.Find("Image").GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/Sliver", typeof(Sprite)) as Sprite;
            LockButton.gameObject.GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/Lock", typeof(Sprite)) as Sprite;
            //锁定状态清除透明效果
            ObstructTransparent OT = GameObject.FindObjectOfType<ObstructTransparent>();
            if (OT != null)
                OT.ClearAllMaterialsColor();
        }
        else
        {
            //_etcJoystickL.axisX.autoLinkTagPlayer = true;
            //_etcJoystickL.axisY.autoLinkTagPlayer = true;
            JumpButton.gameObject.SetActive(true);
            //_etcJoystickL.axisX.directTransform = localHuman.gameObject.transform;
            //_etcJoystickL.axisY.directTransform = localHuman.gameObject.transform;
            //_etcJoystickR.axisX.directTransform = localHuman.gameObject.transform;
            //ETCInput.SetAxisDirecTransform("VerticalR", Camera.main.transform);
            ControlSwitchL(localHuman.transform);
            ControlSwitchR(localHuman.transform, Camera.main.transform);
            localHuman.gameObject.GetComponent<CharacterController>().enabled = true;
            CameraControl.GetComponent<CameraControlManager>().enabled = false;
            CameraControl.transform.position = Vector3.zero;
            CameraControl.transform.localEulerAngles = Vector3.zero;
            CameraControl.transform.SetParent(localHuman.gameObject.transform, false);
            SetHideCameraLocalPos(localHuman);
            Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
            //LockButton.gameObject.GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/Red", typeof(Sprite)) as Sprite;
            //LockButton.gameObject.transform.Find("Image").GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/Gold", typeof(Sprite)) as Sprite;
            LockButton.gameObject.GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/UnLock", typeof(Sprite)) as Sprite;
        }
    }
    public void ShowButtonUI()
    {
        Debug.Log("ShowButtonUI!!!!!");
        if (LockButton == null)
        {
            return;
        }

        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman != null)
        {
            if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
            {
                LockButton.gameObject.SetActive(false);
                CrouchButton.gameObject.SetActive(true);
                JumpButton.gameObject.SetActive(true);
                _etcButtonUp.gameObject.SetActive(false);
                _etcButtonDown.gameObject.SetActive(false);
            }
            else if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
            {
                LockButton.gameObject.SetActive(true);
                CrouchButton.gameObject.SetActive(false);
                JumpButton.gameObject.SetActive(true);
                _etcButtonUp.gameObject.SetActive(true);
                _etcButtonDown.gameObject.SetActive(true);
            }
        }
    }
    //获取摇杆偏移量给摄像机移动使用
    public void GetAxisFromETC()
    {
        float hL = 0;
        float vL = 0;
        switch (HNGameManager.ControlCase)
        {
            case 0:
                hL = ETCInput.GetAxis("HorizontalL");
                vL = ETCInput.GetAxis("VerticalL");
                break;
            case 1:
                hL = ETCInput.GetAxis("HorizontalDL");
                vL = ETCInput.GetAxis("VerticalDL");
                break;
            case 2:
                hL = ETCInput.GetAxis("HorizontalDL");
                vL = ETCInput.GetAxis("VerticalDL");
                break;
            case 3:
                hL = ETCInput.GetAxis("HorizontalL");
                vL = ETCInput.GetAxis("VerticalL");
                break;
        }
        //将摇杆的偏移量转换为当前玩家朝向的偏移量
        Vector3 moveDirection = GetWorldDirFromAxisValue(hL, vL);
        //Debug.Log("---------偏移量：" + moveDirection);
        CameraControlManager.moveDirection = moveDirection;
        GameObjectsManager.GetInstance().SetLocalHumanForWard(moveDirection);
    }
    public void ControlModelSwitch(CameraControlMode eCtrlMode)
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        GameObject PosPoint = GameObject.Find("PosPoint");
        GameObject LookAtPoint = GameObject.Find("LookAtPoint");
        switch (eCtrlMode)
        {
            case CameraControlMode.FreeCameraMode:  //摄像机镜头模式
                if (CameraControl == null || Camera.main == null)
                    return;

                _etcButtonUp.gameObject.SetActive(true);
                _etcButtonDown.gameObject.SetActive(true);
                JumpButton.gameObject.SetActive(false);
                CrouchButton.gameObject.SetActive(false);
                LockButton.gameObject.SetActive(false);

                ControlSwitchL(null);
                ControlSwitchR(CameraControl.transform, Camera.main.transform);
                CameraControl.GetComponent<CameraControlManager>().enabled = true;
                CameraControl.transform.SetParent(null, false);
                if (localHuman != null)
                {
                    //CameraControl.transform.position = new Vector3(localHuman.gameObject.transform.position.x, 20, localHuman.gameObject.transform.position.z);
                    //CameraControl.transform.localEulerAngles = localHuman.gameObject.transform.localEulerAngles;
                    //Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
                    if (PosPoint != null && LookAtPoint != null)
                    {
                        CameraControl.transform.position = PosPoint.transform.position;
                        CameraControl.transform.localEulerAngles = PosPoint.transform.localEulerAngles;
                        Camera.main.transform.LookAt(LookAtPoint.transform);
                    }
                    Camera.main.transform.localPosition = Vector3.zero;
                    Camera.main.transform.gameObject.GetComponent<ObstructTransparent>().enabled = false;  //关闭射线透明效果
                }
                break;
            case CameraControlMode.PlayerViewMode:  //玩家镜头模式
                if (localHuman == null)
                {
                    return;
                }
                if (LockButton != null)
                {
                    if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                    {
                        LockButton.gameObject.SetActive(false);
                        CrouchButton.gameObject.SetActive(true);
                        _etcButtonUp.gameObject.SetActive(false);
                        _etcButtonDown.gameObject.SetActive(false);
                        Camera.main.transform.localEulerAngles = Vector3.zero;
                        if (isPerson_1st)
                        {
                            Camera.main.transform.localPosition = new Vector3(0, 1.7f, 0);
                            localHuman.SetGameObjRenderVisible(false);
                            Camera.main.transform.gameObject.GetComponent<ObstructTransparent>().enabled = false;  //关闭射线透明效果
                        }
                        else
                        {
                            Camera.main.transform.localPosition = new Vector3(0, 2.4f, -3);
                            Camera.main.transform.gameObject.GetComponent<ObstructTransparent>().enabled = true;  //开启射线透明效果
                        }
                    }
                    else if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                    {
                        LockButton.gameObject.SetActive(true);
                        CrouchButton.gameObject.SetActive(false);
                        _etcButtonUp.gameObject.SetActive(true);
                        _etcButtonDown.gameObject.SetActive(true);
                        Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
                        SetHideCameraLocalPos(localHuman);
                        Camera.main.transform.gameObject.GetComponent<ObstructTransparent>().enabled = true;  //开启射线透明效果
                    }
                }
                JumpButton.gameObject.SetActive(true);

                ControlSwitchL(localHuman.gameObject.transform);
                ControlSwitchR(localHuman.gameObject.transform, Camera.main.transform);
                if (CameraControl != null)
                {
                    CameraControl.GetComponent<CameraControlManager>().enabled = false;
                    CameraControl.transform.SetParent(localHuman.gameObject.transform, false);
                    CameraControl.transform.localPosition = Vector3.zero;
                    CameraControl.transform.localEulerAngles = Vector3.zero;
                }
                break;
            case CameraControlMode.DeadMode:   //死亡模式
                if (CameraControl == null || Camera.main == null)
                    return;

                JumpButton.gameObject.SetActive(false);
                CrouchButton.gameObject.SetActive(false);
                LockButton.gameObject.SetActive(false);
                _etcButtonUp.gameObject.SetActive(true);
                _etcButtonDown.gameObject.SetActive(true);

                ControlSwitchL(null);
                ControlSwitchR(CameraControl.transform, Camera.main.transform);
                CameraControl.GetComponent<CameraControlManager>().enabled = true;
                CameraControl.transform.SetParent(null, false);
                //CameraControl.transform.position = new Vector3(0, 20, 0);
                //Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
                if (localHuman != null)
                {
                    if (localHuman.GetComponent<PlayerBase>().TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                    {
                        CameraControl.transform.position = localHuman.transform.position + new Vector3(0, 5, -8);
                        Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
                    }
                    else if (localHuman.GetComponent<PlayerBase>().TeamType == PlayerTeam.PlayerTeamType.HideTeam)
                    {
                        if (PosPoint != null && LookAtPoint != null)
                        {
                            CameraControl.transform.position = PosPoint.transform.position;
                            CameraControl.transform.localEulerAngles = PosPoint.transform.localEulerAngles;
                            Camera.main.transform.LookAt(LookAtPoint.transform);
                        }
                    }
                    Camera.main.transform.localPosition = Vector3.zero;
                }
                break;
            case CameraControlMode.LookOnMode:   //浏览模式
                if (CameraControl == null || Camera.main == null)
                    return;

                _etcButtonUp.gameObject.SetActive(true);
                _etcButtonDown.gameObject.SetActive(true);
                JumpButton.gameObject.SetActive(false);
                CrouchButton.gameObject.SetActive(false);
                LockButton.gameObject.SetActive(false);

                ControlSwitchL(null);
                ControlSwitchR(CameraControl.transform, Camera.main.transform);
                _etcJoystickL.axisX.directTransform = null;
                _etcJoystickL.axisY.directTransform = null;
                CameraControl.GetComponent<CameraControlManager>().enabled = true;
                CameraControl.transform.SetParent(null, false);
                //CameraControl.transform.position = new Vector3(0, 20, 0);
                //Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
                if (PosPoint != null && LookAtPoint != null)
                {
                    CameraControl.transform.position = PosPoint.transform.position;
                    CameraControl.transform.localEulerAngles = PosPoint.transform.localEulerAngles;
                    Camera.main.transform.LookAt(LookAtPoint.transform);
                }
                Camera.main.transform.localPosition = Vector3.zero;
                break;
        }
    }
    public void ControlSwitchL(Transform Obj = null)
    {
        switch (HNGameManager.ControlCase)
        {
            case 0:
                _etcJoystickL.axisX.directTransform = Obj;
                _etcJoystickL.axisY.directTransform = Obj;
                break;
            case 1:
                _etcDPadL.axisX.directTransform = Obj;
                _etcDPadL.axisY.directTransform = Obj;
                break;
            case 2:
                _etcDPadL.axisX.directTransform = Obj;
                _etcDPadL.axisY.directTransform = Obj;
                break;
            case 3:
                _etcJoystickL.axisX.directTransform = Obj;
                _etcJoystickL.axisY.directTransform = Obj;
                break;
        }
    }
    public void ControlSwitchR(Transform Obj1, Transform Obj2)
    {
        switch (HNGameManager.ControlCase)
        {
            case 0:
                ETCInput.SetAxisDirecTransform("HorizontalTR", Obj1);
                ETCInput.SetAxisDirecTransform("VerticalTR", Obj2);
                break;
            case 1:
                ETCInput.SetAxisDirecTransform("HorizontalTR", Obj1);
                ETCInput.SetAxisDirecTransform("VerticalTR", Obj2);
                break;
            case 2:
                ETCInput.SetAxisDirecTransform("HorizontalL", Obj1);
                ETCInput.SetAxisDirecTransform("VerticalL", Obj2);
                break;
            case 3:
                ETCInput.SetAxisDirecTransform("HorizontalR", Obj1);
                ETCInput.SetAxisDirecTransform("VerticalR", Obj2);
                break;
        }
    }
    public void SetETCUIVisible(bool bVisible)
    {
        //if (!bVisible)
        //{
        //    _etcJoystickL.gameObject.GetComponent<Image>().enabled = false;
        //    _etcJoystickL.gameObject.transform.Find("Thumb").GetComponent<Image>().enabled = false;
        //    _etcJoystickR.gameObject.GetComponent<Image>().enabled = false;
        //    _etcJoystickR.gameObject.transform.Find("Thumb").GetComponent<Image>().enabled = false;
        //    _etcButtonUp.gameObject.SetActive(false);
        //    _etcButtonDown.gameObject.SetActive(false);
        //    LockButton.gameObject.SetActive(false);
        //    CrouchButton.gameObject.SetActive(false);
        //    JumpButton.gameObject.SetActive(false);
        //}
        //else
        //{
        //    _etcJoystickL.gameObject.GetComponent<Image>().enabled = true;
        //    _etcJoystickL.gameObject.transform.Find("Thumb").GetComponent<Image>().enabled = true;
        //    _etcJoystickR.gameObject.GetComponent<Image>().enabled = true;
        //    _etcJoystickR.gameObject.transform.Find("Thumb").GetComponent<Image>().enabled = true;
        //    _etcButtonUp.gameObject.SetActive(true);
        //    _etcButtonDown.gameObject.SetActive(true);
        //    JumpButton.gameObject.SetActive(true);
        //    Human localHuman = null;
        //    if (GameObjectsManager.GetInstance() != null)
        //    {
        //        localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        //        if (localHuman != null)
        //        {
        //            if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
        //            {
        //                CrouchButton.gameObject.SetActive(true);
        //            }
        //            else if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
        //            {
        //                LockButton.gameObject.SetActive(true);
        //            }
        //        }
        //    }
        //    else
        //        Debug.Log("无法获取单例");
        //}
    }
    public void SpeedChange(float value)
    {
        switch (HNGameManager.ControlCase)
        {
            case 0:
                _etcJoystickL.axisX.speed = value;
                _etcJoystickL.axisY.speed = value;
                break;
            case 1:
                _etcDPadL.axisX.speed = value;
                _etcDPadL.axisY.speed = value;

                break;
            case 2:
                _etcDPadL.axisX.speed = value;
                _etcDPadL.axisY.speed = value;
                break;
            case 3:
                _etcJoystickL.axisX.speed = value;
                _etcJoystickL.axisY.speed = value;
                break;
        }
    }
    public void RemoveListener()
    {
        if (JumpButton != null)
            JumpButton.onClick.RemoveAllListeners();
        if (CrouchButton != null)
            CrouchButton.onClick.RemoveAllListeners();
        if (LockButton != null)
            LockButton.onClick.RemoveAllListeners();
        if (BoomButton != null)
            BoomButton.onClick.RemoveAllListeners();
        if (_etcButtonUp != null)
        {
            _etcButtonUp.onDown.RemoveAllListeners();
            _etcButtonUp.onUp.RemoveAllListeners();
        }
        if (_etcButtonDown != null)
        {
            _etcButtonDown.onDown.RemoveAllListeners();
            _etcButtonDown.onUp.RemoveAllListeners();
        }
        if (ChatButton != null)
            ChatButton.onClick.RemoveAllListeners();
        if (PersonButton != null)
            PersonButton.onClick.RemoveAllListeners();
        if (ChangeModelButton != null)
            ChangeModelButton.onClick.RemoveAllListeners();
        if (StealthButton != null)
            StealthButton.onClick.RemoveAllListeners();
        if (ResurrectionButton != null)
            ResurrectionButton.onClick.RemoveAllListeners();
    }
    public void AddListenner()
    {
        if (JumpButton != null)
            JumpButton.onClick.AddListener(() => { ClickJumpButton(); });
        if (CrouchButton != null)
            CrouchButton.onClick.AddListener(() => { ClickCrouchButton(); });
        if (LockButton != null)
            LockButton.onClick.AddListener(() => { ClickLockButton(); });
        if (BoomButton != null)
        {
            BoomButton.onClick.AddListener(() => { InventoryManager.GetInstane.BoomUse(); });
        }
        if (ChatButton != null)
        {
            ChatButton.onClick.AddListener(() =>
            {
                GameObject ChatInputField = ChatButton.gameObject.transform.parent.Find("ChatInputField").gameObject;
                ChatInputField.SetActive(!ChatInputField.activeSelf);
                GameObject Texts = ChatButton.gameObject.transform.parent.Find("Texts").gameObject;
                Texts.SetActive(ChatInputField.activeSelf);
            });
        }
        if (PersonButton != null)
            PersonButton.onClick.AddListener(() => { ClickPersonButton(); });
        if (ChangeModelButton != null)
            ChangeModelButton.onClick.AddListener(() => { ClickChangeModelButton(); });
        if (StealthButton != null)
            StealthButton.onClick.AddListener(() => { ClickStealthButton(); });
        if (ResurrectionButton != null)
            ResurrectionButton.onClick.AddListener(() => { ClickResurrectionButton(); });

        if (_etcButtonUp != null)
        {
            _etcButtonUp.onDown.AddListener(() =>
            {
                if (!m_Up)
                    m_Up = true;
            });
            _etcButtonUp.onUp.AddListener(() =>
            {
                if (m_Up)
                    m_Up = false;
            });
        }
        if (_etcButtonDown != null)
        {
            _etcButtonDown.onDown.AddListener(() =>
            {
                if (!m_Down)
                    m_Down = true;
            });
            _etcButtonDown.onUp.AddListener(() =>
            {
                if (m_Down)
                    m_Down = false;
            });
        }
    }
    public void SetHideCameraLocalPos(Human localHuman)
    {
        if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
        {
            float y = 0;
            float z = 0;
            GameObject ModelObj = localHuman.gameObject.transform.GetChild(0).gameObject;
            if (ModelObj.transform.childCount == 0)
            {
                y = ModelObj.gameObject.GetComponent<Renderer>().bounds.size.y;
                z = ModelObj.gameObject.GetComponent<Renderer>().bounds.size.z;
            }
            else if (ModelObj.transform.childCount > 0)
            {
                if (ModelObj.transform.GetChild(0).name == "CameraControl")
                {
                    y = ModelObj.gameObject.GetComponent<Renderer>().bounds.size.y;
                    z = ModelObj.gameObject.GetComponent<Renderer>().bounds.size.z;
                }
                else
                {
                    y = ModelObj.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y;
                    z = ModelObj.transform.GetChild(0).GetComponent<Renderer>().bounds.size.z;
                }
            }
            Camera.main.transform.localPosition = new Vector3(0, y * 1.5f + 1, -(z * 1.5f + 2));
        }
    }
    public void SetETCUIControlEnable(bool enable)
    {
        if (enable)
        {
            switch (HNGameManager.ControlCase)
            {
                case 0:
                    if (_etcJoystickL != null)
                    {
                        _etcJoystickL.visible = true;
                        _etcJoystickL.activated = true;
                    }

                    if (_etcTouchPadR != null)
                    {
                        _etcTouchPadR.visible = false;
                        _etcTouchPadR.activated = true;
                    }
                    break;
                case 1:
                    if (_etcTouchPadR != null)
                    {
                        _etcTouchPadR.visible = false;
                        _etcTouchPadR.activated = true;
                    }
                    if (_etcDPadL != null)
                    {
                        _etcDPadL.visible = true;
                        _etcDPadL.activated = true;
                    }
                    break;
                case 2:
                    if (_etcJoystickL != null)
                    {
                        _etcJoystickL.visible = true;
                        _etcJoystickL.activated = true;
                    }
                    if (_etcDPadL != null)
                    {
                        _etcDPadL.visible = true;
                        _etcDPadL.activated = true;
                    }

                    break;
                case 3:
                    if (_etcJoystickL != null)
                    {
                        _etcJoystickL.visible = true;
                        _etcJoystickL.activated = true;
                    }
                    if (_etcJoystickR != null)
                    {
                        _etcJoystickR.visible = true;
                        _etcJoystickR.activated = true;
                    }
                    break;
            }
        }
        else
        {
            if (_etcJoystickL != null)
            {
                _etcJoystickL.visible = false;
                _etcJoystickL.activated = false;
            }
            if (_etcJoystickR != null)
            {
                _etcJoystickR.visible = false;
                _etcJoystickR.activated = false;
            }
            if (_etcJoystickR != null)
            {
                _etcTouchPadR.visible = false;
                _etcTouchPadR.activated = false;
            }
            if (_etcDPadL != null)
            {
                _etcDPadL.visible = false;
                _etcDPadL.activated = false;
            }
        }
    }
    public void SetChatSystemEnable(bool bVisible)
    {
        GameObject ChatInputField = ChatButton.gameObject.transform.parent.Find("ChatInputField").gameObject;
        ChatInputField.SetActive(bVisible);
        GameObject Texts = ChatButton.gameObject.transform.parent.Find("Texts").gameObject;
        Texts.SetActive(bVisible);
    }
    public void ChangeModel(Int32 randomNum)   //变身
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
        {
            int modelIndex = 0;
            Int32 temp = randomNum;
            modelIndex = (int)(temp % HNGameManager.hidePrefabNames.Count);

            GameObject newObj = Resources.Load(HNGameManager.hidePrefabPath[modelIndex]) as GameObject;
            GameObject model = UnityEngine.Object.Instantiate(newObj.transform.FindChild("Model").gameObject);
            model.name = "Model";
            model.tag = "Hide";
            GameObject oldModel = localHuman.transform.FindChild("Model").gameObject;
            model.transform.SetParent(localHuman.gameObject.transform);
            model.transform.SetSiblingIndex(0);
            model.transform.localPosition = oldModel.transform.localPosition;
            model.transform.localEulerAngles = oldModel.transform.localEulerAngles;
            UnityEngine.Object.Destroy(oldModel);

            if (localHuman.m_isStealth == 1)
                localHuman.SetGameObjAplha(0.4f);

            Renderer render = null;
            CharacterController _controller = localHuman.gameObject.transform.GetComponent<CharacterController>();
            if (_controller == null)
                _controller = localHuman.gameObject.AddComponent<CharacterController>();
            GameObject ModelObj = localHuman.gameObject.transform.FindChild("Model").gameObject;
            if (ModelObj.transform.childCount == 0)   //整体模型
                render = ModelObj.GetComponent<Renderer>();
            else if (ModelObj.transform.childCount > 0)  //多段模型
            {
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

                GameObject infoObj = localHuman.transform.Find("InfoPanel").gameObject;
                infoObj.transform.localPosition = new Vector3(infoObj.transform.localPosition.x, render.bounds.size.y + 0.2f, infoObj.transform.localPosition.z);
                infoObj.transform.localScale = infoObj.transform.localScale;
            }
            //根据模型高度设置速度
            localHuman.SetJumpSpeed(_controller);

            if (Camera.main.transform.parent.transform.parent != null) //变身时判断此刻是否为玩家视角模式，若是则应设置相机与其父物体的相对位置
            {
                Camera.main.transform.localPosition = new Vector3(0, render.bounds.size.y * 1.5f + 1, -(render.bounds.size.z * 1.5f + 2));
                Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
            }
            else
            {
                Camera.main.transform.localPosition = Vector3.zero;
                Camera.main.transform.localEulerAngles = new Vector3(30, 0, 0);
            }

            localHuman.gameObject.name = HNGameManager.hidePrefabNames[modelIndex];
        }
    }
    public void ChangeModel(GameObject Obj, Int32 randomNum)   //变身
    {
        PlayerBase Human = Obj.GetComponent<PlayerBase>();
        if (Human != null && Human.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
        {
            int modelIndex = 0;
            Int32 temp = randomNum;
            modelIndex = (int)(temp % HNGameManager.hidePrefabNames.Count);

            GameObject newObj = Resources.Load(HNGameManager.hidePrefabPath[modelIndex]) as GameObject;
            GameObject model = UnityEngine.Object.Instantiate(newObj.transform.FindChild("Model").gameObject);
            model.name = "Model";
            model.tag = "Hide";
            GameObject oldModel = Human.gameObject.transform.FindChild("Model").gameObject;
            model.transform.SetParent(Human.gameObject.transform);
            model.transform.SetSiblingIndex(0);
            model.transform.localPosition = oldModel.transform.localPosition;
            model.transform.localEulerAngles = oldModel.transform.localEulerAngles;
            UnityEngine.Object.Destroy(oldModel);


            Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
            if (Human.m_isStealth == 1)   //检查变身时玩家是否在隐身状态
            {
                if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                    Human.SetGameObjVisible(false);
                else
                    Human.SetGameObjAplha(0.4f);
            }
            //判断当前状态
            byte Gamestate = CServerItem.get().GetGameStatus();
            if (Gamestate == SocketDefines.GAME_STATUS_HIDE)  //躲藏者在躲藏阶段变身继续保持隐身状态
                if (localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam)
                    Human.SetGameObjVisible(false);

            Renderer render = null;
            CharacterController _controller = Human.gameObject.transform.GetComponent<CharacterController>();
            if (_controller == null)
                _controller = Human.gameObject.AddComponent<CharacterController>();
            GameObject ModelObj = Human.gameObject.transform.FindChild("Model").gameObject;
            if (ModelObj.transform.childCount == 0)   //整体模型
                render = ModelObj.GetComponent<Renderer>();
            else if (ModelObj.transform.childCount > 0)  //多段模型
            {
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

                GameObject infoObj = Obj.transform.Find("InfoPanel").gameObject;
                infoObj.transform.localPosition = new Vector3(infoObj.transform.localPosition.x, render.bounds.size.y + 0.2f, infoObj.transform.localPosition.z);
                infoObj.transform.localScale = infoObj.transform.localScale;
            }
            //根据模型高度设置速度
            Human.SetJumpSpeed(_controller);

            Human.gameObject.name = HNGameManager.hidePrefabNames[modelIndex];
        }
    }
    public void StealthTime(byte time)
    {
        Text timeText = StealthButton.gameObject.transform.Find("TimeLeft").GetComponent<Text>();
        if (timeText != null)
        {
            if (time == 255)
                timeText.text = "";
            else
                timeText.text = time.ToString();
        }

    }
    public void Stealth(bool bVisible)  //隐身
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
        {
            //因为每一秒都会收到信息，如果当前状态和想要变化的状态一致，则不操作
            if (localHuman.m_isStealth == ((byte)(bVisible == true ? 0 : 1)))
                return;
            if (bVisible)
                localHuman.SetGameObjAplha(1);
            else
                localHuman.SetGameObjAplha(0.4f);
            StealthButton.interactable = bVisible;
            localHuman.m_isStealth = (byte)(bVisible == true ? 0 : 1);

        }
    }
    public void Stealth(GameObject Obj, bool bVisible)  //隐身
    {
        PlayerBase Human = Obj.GetComponent<PlayerBase>();
        PlayerBase localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (Human != null && localHuman != null)
        {
            //因为每一秒都会收到信息，如果当前状态和想要变化的状态一致，则不操作
            if (Human.m_isStealth == ((byte)(bVisible == true ? 0 : 1)))
                return;
            if (localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam && !Human.IsAI)
            {
                if (bVisible)
                    Human.SetGameObjAplha(1);
                else
                    Human.SetGameObjAplha(0.4f);
                Human.m_isStealth = (byte)(bVisible == true ? 0 : 1);

            }
            else if(localHuman.TeamType == PlayerTeam.PlayerTeamType.TaggerTeam && !Human.IsAI)
            {
                Human.SetGameObjVisible(bVisible);
                Human.m_isStealth = (byte)(bVisible == true ? 0 : 1);
            }
        }
    }
    public void Resurrection()  //复活
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman != null && localHuman.TeamType == PlayerTeam.PlayerTeamType.HideTeam)
        {
            if (UIManager.TimeLeft > GameManager.HIDESEEK_GAME_PLAY_TIME - 60)
            {
                localHuman.GetComponent<PlayerBase>().Hp = 1;
                Loom.QueueOnMainThread(() =>
                {
                    int Index = (int)(MersenneTwister.MT19937.Int63() % GameManager.ListHiderPosition.Count);
                    localHuman.transform.position = GameManager.ListHiderPosition[Index].transform.position;
                    localHuman.transform.localEulerAngles = GameManager.ListHiderPosition[Index].transform.localEulerAngles;
                    localHuman.SetGameObjVisible(true);
                    //使用复活之后锁定状态去除
                    localHuman.m_Lock = false;
                    LockButton.gameObject.GetComponent<Image>().overrideSprite = Resources.Load("Textures/Sprites/Sprites/UnLock", typeof(Sprite)) as Sprite;
                    if (UIManager.GetInstance() != null)
                        UIManager.GetInstance().ShowTopTips("", false);
                    ControlModelSwitch(CameraControlMode.PlayerViewMode);
                    ResurrectionButton.gameObject.SetActive(false);
                    StealthButton.gameObject.SetActive(true);
                    ChangeModelButton.gameObject.SetActive(true);
                });
            }

            Renderer render = null;
            CharacterController _controller = localHuman.gameObject.transform.GetComponent<CharacterController>();
            if (_controller == null)
                _controller = localHuman.gameObject.AddComponent<CharacterController>();
            GameObject ModelObj = localHuman.gameObject.transform.GetChild(0).gameObject;
            if (ModelObj.transform.childCount == 0)   //整体模型
                render = ModelObj.GetComponent<Renderer>();
            else if (ModelObj.transform.childCount > 0)  //多段模型
            {
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
        }
    }
    public void Resurrection(GameObject Obj)  //复活
    {
        PlayerBase Human = Obj.GetComponent<PlayerBase>();
        if (Human != null && Human.TeamType == PlayerTeam.PlayerTeamType.HideTeam && !Human.IsAI)
        {
            if (UIManager.TimeLeft > GameManager.HIDESEEK_GAME_PLAY_TIME - 60)
            {
                Human.Hp = 1;
                Loom.QueueOnMainThread(() =>
                {
                    int Index = (int)(MersenneTwister.MT19937.Int63() % GameManager.ListHiderPosition.Count);
                    Human.gameObject.transform.position = GameManager.ListHiderPosition[Index].transform.position;
                    Human.gameObject.transform.localEulerAngles = GameManager.ListHiderPosition[Index].transform.localEulerAngles;
                    //if (GameObjectsManager.GetInstance() != null)
                    //    GameObjectsManager.GetInstance().SetGameObjVisible(localHuman.gameObject, true);
                    Human.SetGameObjVisible(true);
                    if (UIManager.GetInstance() != null)
                        UIManager.GetInstance().ShowTopTips("", false);
                    ControlModelSwitch(CameraControlMode.PlayerViewMode);
                    ResurrectionButton.gameObject.SetActive(false);
                });
            }
        }
    }
}
