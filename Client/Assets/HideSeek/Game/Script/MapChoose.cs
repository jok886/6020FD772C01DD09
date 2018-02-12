using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MapChoose : MonoBehaviour
{
    public static int Mapindex;
    public Button Left;
    public Button Right;
    private bool isMove;
    private bool isTouch;
    private List<Vector3> listGameObjPos;
    private List<GameObject> listGameObj;
    private List<string> listMapName;
    public enum MoveDirection
    {
        Left,
        Right
    }
    void Start()
    {
        Mapindex = 0;
        Left = GameObject.Find("Canvas/Window/CreateWindow/Left").GetComponent<Button>();
        Right = GameObject.Find("Canvas/Window/CreateWindow/Right").GetComponent<Button>();

        //Get MapName
        ItemListManager.GetInstance.LoadAndDeserialize();
        List<Item> listItem = ItemListManager.GetInstance.items.ItemList;
        listMapName = new List<string>();
        for (int i = 0; i < listItem.Count; i++)
            listMapName.Add(listItem[i].Map);

        isMove = false;
        isTouch = false;
        listGameObjInit();
        Left.onClick.AddListener(() =>
        {
            if (!isMove)
            {
                isMove = true;
                Move(MoveDirection.Left);
            }
        });
        Right.onClick.AddListener(() =>
        {
            if (!isMove)
            {
                isMove = true;
                Move(MoveDirection.Right);
            }
        });
    }

    void Update()
    {
        if(isTouch)
        {
#if UNITY_ANDROID || UNITY_IOS
            judueFinger();
#elif UNITY_STANDALONE||UNITY_EDITOR
            judueFingerPC();
#endif
        }
    }
    private void listGameObjInit()
    {
        listGameObj = new List<GameObject>();
        listGameObjPos = new List<Vector3>();
        listGameObjPos.Add(new Vector3(-750, 0, 0));
        listGameObjPos.Add(new Vector3(-500, 0, 0));
        listGameObjPos.Add(new Vector3(-250, 0, 0));
        listGameObjPos.Add(Vector3.zero);
        listGameObjPos.Add(new Vector3(250, 0, 0));
        listGameObjPos.Add(new Vector3(500, 0, 0));
        listGameObjPos.Add(new Vector3(750, 0, 0));
        for (int i = 0; i < listGameObjPos.Count; i++)
        {
            GameObject loadObj = Resources.Load("UI/MapPrefabs/Map0") as GameObject;
            GameObject temp = Instantiate(loadObj);

            EventTrigger ET = temp.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => { if (!isTouch) isTouch = true; });
            ET.triggers.Add(entry);

            temp.transform.SetParent(GameObject.Find("Canvas/Window/CreateWindow/Maps").transform, false);
            if (i == 3)
            {
                temp.GetComponent<Transform>().SetSiblingIndex(3);
                temp.transform.localScale = Vector3.one;
                temp.transform.Find("SceneImage").GetComponent<Image>().overrideSprite = Resources.Load(GetMapNamePath(listMapName[0]), typeof(Sprite)) as Sprite;
                GameObject.Find("Canvas/Window/CreateWindow/Maps/MapName").GetComponent<Text>().text = GetMapName(temp.transform.Find("SceneImage").GetComponent<Image>().overrideSprite.name);
                Mapindex = GetMapIndex(temp.transform.Find("SceneImage").GetComponent<Image>().overrideSprite.name);
            }
            else if (i == 2 || i == 4)
            {
                temp.GetComponent<Transform>().SetSiblingIndex(2);
                temp.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                if (i == 2)
                    temp.transform.Find("SceneImage").GetComponent<Image>().overrideSprite = Resources.Load(GetMapNamePath(listMapName[listMapName.Count - 1]), typeof(Sprite)) as Sprite;
                else if (i == 4)
                    temp.transform.Find("SceneImage").GetComponent<Image>().overrideSprite = Resources.Load(GetMapNamePath(listMapName[1]), typeof(Sprite)) as Sprite;
            }
            else if (i == 1 || i == 5)
            {
                temp.GetComponent<Transform>().SetSiblingIndex(1);
                temp.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                if (i == 1)
                    temp.transform.Find("SceneImage").GetComponent<Image>().overrideSprite = Resources.Load(GetMapNamePath(listMapName[listMapName.Count - 1 - 1]), typeof(Sprite)) as Sprite;
                else if (i == 5)
                    temp.transform.Find("SceneImage").GetComponent<Image>().overrideSprite = Resources.Load(GetMapNamePath(listMapName[2]), typeof(Sprite)) as Sprite;
            }
            else if (i == 0 || i == 6)
            {
                temp.GetComponent<Transform>().SetSiblingIndex(0);
                temp.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                if (i == 0)
                    temp.transform.Find("SceneImage").GetComponent<Image>().overrideSprite = Resources.Load(GetMapNamePath(listMapName[listMapName.Count - 1 - 2]), typeof(Sprite)) as Sprite;
                else if (i == 6)
                    temp.transform.Find("SceneImage").GetComponent<Image>().overrideSprite = Resources.Load(GetMapNamePath(listMapName[3]), typeof(Sprite)) as Sprite;
            }
            temp.transform.localPosition = listGameObjPos[i];
            listGameObj.Add(temp);
        }
    }
    private void Move(MoveDirection dir)
    {
        int currentIndex = GetMapIndex(listGameObj[3].transform.Find("SceneImage").GetComponent<Image>().overrideSprite.name);
        switch (dir)
        {
            case MoveDirection.Right:
                GameObject loadObjLeft = Resources.Load("UI/MapPrefabs/Map0") as GameObject;
                GameObject newObjLeft = Instantiate(loadObjLeft);

                EventTrigger ETL = newObjLeft.AddComponent<EventTrigger>();
                EventTrigger.Entry entryL = new EventTrigger.Entry();
                entryL.eventID = EventTriggerType.PointerDown;
                entryL.callback.AddListener((data) => { if (!isTouch) isTouch = true; });
                ETL.triggers.Add(entryL);

                newObjLeft.transform.SetParent(GameObject.Find("Canvas/Window/CreateWindow/Maps").transform, false);
                newObjLeft.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                newObjLeft.transform.localPosition = listGameObjPos[listGameObjPos.Count - 1] + new Vector3(250, 0, 0);
                newObjLeft.GetComponent<Transform>().SetSiblingIndex(0);
                //string nameLeft = GetMapNamePath(listGameObj[listGameObj.Count - 1 - 3].transform.Find("SceneImage").GetComponent<Image>().overrideSprite.name);
                string nameLeft = GetMapNamePath(listMapName[currentIndex + 4 > listMapName.Count - 1 ? (currentIndex + 4) % listMapName.Count : currentIndex + 4]);
                newObjLeft.transform.Find("SceneImage").GetComponent<Image>().overrideSprite = Resources.Load(nameLeft, typeof(Sprite)) as Sprite;
                listGameObj.Add(newObjLeft);

                Sequence moveLeft = DOTween.Sequence();
                for (int i = 1; i < listGameObj.Count; i++)
                {
                    int x = (int)listGameObj[i].transform.localPosition.x;
                    moveLeft.Insert(0, listGameObj[i].transform.DOLocalMoveX(x - 250, 0.5f)).SetRelative().Pause();
                    if (i == 1 || i == 7)
                    {
                        moveLeft.Insert(0, listGameObj[i].transform.DOScale(new Vector3(0.7f, 0.7f, 0.7f), 0.25f)).Pause();
                        ChangeSiblingIndex(i, 0);
                    }
                    else if (i == 2 || i == 6)
                    {
                        moveLeft.Insert(0, listGameObj[i].transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.25f)).Pause();
                        ChangeSiblingIndex(i, 1);
                    }
                    else if (i == 3 || i == 5)
                    {
                        moveLeft.Insert(0, listGameObj[i].transform.DOScale(new Vector3(0.9f, 0.9f, 0.9f), 0.25f)).Pause();
                        ChangeSiblingIndex(i, 2);
                    }
                    else if (i == 4)
                    {
                        moveLeft.Insert(0, listGameObj[i].transform.DOScale(new Vector3(1f, 1f, 1f), 0.25f)).Pause();
                        ChangeSiblingIndex(i, 3);
                        GameObject.Find("Canvas/Window/CreateWindow/Maps/MapName").GetComponent<Text>().text = GetMapName(listGameObj[i].transform.Find("SceneImage").GetComponent<Image>().overrideSprite.name);
                        Mapindex = GetMapIndex(listGameObj[i].transform.Find("SceneImage").GetComponent<Image>().overrideSprite.name);
                    }
                }
                moveLeft.Play().OnComplete(() => { isMove = false; });
                Destroy(listGameObj[0]);
                listGameObj.RemoveAt(0);
                break;
            case MoveDirection.Left:
                GameObject loadObjRight = Resources.Load("UI/MapPrefabs/Map0") as GameObject;
                GameObject newObjRight = Instantiate(loadObjRight);

                EventTrigger ETR = newObjRight.AddComponent<EventTrigger>();
                EventTrigger.Entry entryR = new EventTrigger.Entry();
                entryR.eventID = EventTriggerType.PointerDown;
                entryR.callback.AddListener((data) => { if (!isTouch) isTouch = true; });
                ETR.triggers.Add(entryR);

                newObjRight.transform.SetParent(GameObject.Find("Canvas/Window/CreateWindow/Maps").transform, false);
                newObjRight.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                newObjRight.transform.localPosition = listGameObjPos[0] + new Vector3(-250, 0, 0);
                newObjRight.GetComponent<Transform>().SetSiblingIndex(0);
                //string nameRight = GetMapNamePath(listGameObj[0 + 3].transform.Find("SceneImage").GetComponent<Image>().overrideSprite.name);
                string nameRight = GetMapNamePath(listMapName[currentIndex - 4 < 0 ? listMapName.Count + (currentIndex - 4) : currentIndex - 4]);
                newObjRight.transform.Find("SceneImage").GetComponent<Image>().overrideSprite = Resources.Load(nameRight, typeof(Sprite)) as Sprite;
                listGameObj.Insert(0, newObjRight);

                Sequence moveRight = DOTween.Sequence();
                for (int i = 0; i < listGameObj.Count - 1; i++)
                {
                    int x = (int)listGameObj[i].transform.localPosition.x;
                    moveRight.Insert(0, listGameObj[i].transform.DOLocalMoveX(x + 250, 0.5f)).Pause();
                    if (i == 0 || i == 6)
                    {
                        moveRight.Insert(0, listGameObj[i].transform.DOScale(new Vector3(0.7f, 0.7f, 0.7f), 0.25f)).Pause();
                        ChangeSiblingIndex(i, 0);
                    }
                    else if (i == 1 || i == 5)
                    {
                        moveRight.Insert(0, listGameObj[i].transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.25f)).Pause();
                        ChangeSiblingIndex(i, 1);
                    }
                    else if (i == 2 || i == 4)
                    {
                        moveRight.Insert(0, listGameObj[i].transform.DOScale(new Vector3(0.9f, 0.9f, 0.9f), 0.25f)).Pause();
                        ChangeSiblingIndex(i, 2);
                    }
                    else if (i == 3)
                    {
                        moveRight.Insert(0, listGameObj[i].transform.DOScale(new Vector3(1f, 1f, 1f), 0.25f)).Pause();
                        ChangeSiblingIndex(i, 3);
                        GameObject.Find("Canvas/Window/CreateWindow/Maps/MapName").GetComponent<Text>().text = GetMapName(listGameObj[i].transform.Find("SceneImage").GetComponent<Image>().overrideSprite.name);
                        Mapindex = GetMapIndex(listGameObj[i].transform.Find("SceneImage").GetComponent<Image>().overrideSprite.name);
                    }
                }
                moveRight.Play().OnComplete(() => { isMove = false; });
                Destroy(listGameObj[listGameObj.Count - 1]);
                listGameObj.RemoveAt(listGameObj.Count - 1);
                break;
        }
    }
    private void ChangeSiblingIndex(int i, int index)
    {
        listGameObj[i].GetComponent<Transform>().SetSiblingIndex(index);
    }
    private string GetMapNamePath(string str)
    {
        string path = "";
        switch (str)
        {
            case "Military":
                path = "UI/Maps/JSJD";
                break;
            case "Office":
                path = "UI/Maps/BGS";
                break;
            case "Port":
                path = "UI/Maps/GK";
                break;
            case "ClassRoom":
                path = "UI/Maps/CR";
                break;
            case "Town":
                path = "UI/Maps/CZ";
                break;
            default:
                Debug.LogError("无法获取路径！");
                break;
        }
        return path;
    }
    private string GetMapName(string str)
    {
        string name = "";
        switch (str)
        {
            case "JSJD":
                name = "军事基地";
                break;
            case "BGS":
                name = "办公室";
                break;
            case "GK":
                name = "港口";
                break;
            case "CR":
                name = "教室";
                break;
            case "CZ":
                name = "城镇";
                break;
            default:
                Debug.LogError("无法获取名字！");
                break;
        }
        return name;
    }
    private int GetMapIndex(string str)
    {
        int index = 0;
        switch (str)
        {
            case "JSJD":
                index = 0;
                break;
            case "BGS":
                index = 1;
                break;
            case "GK":
                index = 2;
                break;
            case "CR":
                index = 3;
                break;
            case "CZ":
                index = 4;
                break;
            default:
                Debug.LogError("无法获取地图索引");
                break;
        }
        return index;
    }
    /**
     * 
     * 新建一个公共方法用于判断手指的移动方向 
     * 假如是往左或者往上 则模型往各个轴的正方向位置移动 函数返回1
     * 加入是往右或者往下 则模型往各个轴的负方向位置移动 函数返回-1
     * 
     * **/
    private bool startPosFlag = true;
    private int backValue = 0;
    private Vector3 startFingerPos = Vector3.zero;
    private Vector3 nowFingerPos = Vector3.zero;
    private float xMoveDistance = 0;
    private float yMoveDistance = 0;
    private void judueFinger()
    {
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began && startPosFlag == true)
            {
                //Debug.Log("======开始触摸=====");
                startFingerPos = Input.GetTouch(0).position;
                startPosFlag = false;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                //Debug.Log("======释放触摸=====");
                startPosFlag = true;
            }
            nowFingerPos = Input.GetTouch(0).position;
            xMoveDistance = Mathf.Abs(nowFingerPos.x - startFingerPos.x);
            yMoveDistance = Mathf.Abs(nowFingerPos.y - startFingerPos.y);
            //if (xMoveDistance > yMoveDistance)
            {
                if (nowFingerPos.x - startFingerPos.x > 0 && Mathf.Abs(nowFingerPos.x - startFingerPos.x) > 25)
                {
                    //Debug.Log("=======沿着X轴负方向移动=====");
                    backValue = -1;         //沿着X轴负方向移动
                }
                else if (nowFingerPos.x - startFingerPos.x < 0 && Mathf.Abs(nowFingerPos.x - startFingerPos.x) > 25)
                {
                    //Debug.Log("=======沿着X轴正方向移动=====");
                    backValue = 1;          //沿着X轴正方向移动
                }
            }
            //else
            //{
            //    if (nowFingerPos.y - startFingerPos.y > 0)
            //    {
            //        //Debug.Log("=======沿着Y轴正方向移动=====");
            //        backValue = 1;         //沿着Y轴正方向移动
            //    }
            //    else
            //    {
            //        //Debug.Log("=======沿着Y轴负方向移动=====");
            //        backValue = -1;         //沿着Y轴负方向移动
            //    }
            //}
        }
        if (backValue == 1 && !isMove && startPosFlag)
        {
            isMove = true;
            isTouch = false;
            backValue = 0;
            Move(MoveDirection.Right);
        }
        else if (backValue == -1 && !isMove && startPosFlag)
        {
            isMove = true;
            isTouch = false;
            backValue = 0;
            Move(MoveDirection.Left);
        }
    }
    private void judueFingerPC()
    {
        //if (Input.touchCount > 0)
        {
            if (Input.GetMouseButtonDown(0)   && startPosFlag == true)
            {
                //Debug.Log("======开始触摸=====");
                startFingerPos = Input.mousePosition;
                startPosFlag = false;
            }
            if (Input.GetMouseButtonUp(0))
            {
                //Debug.Log("======释放触摸=====");
                startPosFlag = true;
            }
            nowFingerPos = Input.mousePosition;
            xMoveDistance = Mathf.Abs(nowFingerPos.x - startFingerPos.x);
            yMoveDistance = Mathf.Abs(nowFingerPos.y - startFingerPos.y);
            //if (xMoveDistance > yMoveDistance)
            {
                if (nowFingerPos.x - startFingerPos.x > 0 && Mathf.Abs(nowFingerPos.x - startFingerPos.x) > 25)
                {
                    //Debug.Log("=======沿着X轴负方向移动=====");
                    backValue = -1;         //沿着X轴负方向移动
                }
                else if (nowFingerPos.x - startFingerPos.x < 0 && Mathf.Abs(nowFingerPos.x - startFingerPos.x) > 25)
                {
                    //Debug.Log("=======沿着X轴正方向移动=====");
                    backValue = 1;          //沿着X轴正方向移动
                }
            }

        }
        Debug.Log("backValue：" + backValue);
        if (backValue == 1 && !isMove && startPosFlag)
        {
            isMove = true;
            isTouch = false;
            backValue = 0;
            Move(MoveDirection.Right);
        }
        else if (backValue == -1 && !isMove && startPosFlag)
        {
            isMove = true;
            isTouch = false;
            backValue = 0;
            Move(MoveDirection.Left);
        }
    }
}
