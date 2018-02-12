using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HpUI : MonoBehaviour
{

    public Sprite HeartPinkSprite;
    public Sprite HeartGreySprite;
    public UnityEngine.AnimatorOverrideController HeartGrowAnimatorController;

    private int _lastLocalHumanHp;

    private struct HeartItem
    {
        public UnityEngine.UI.Image HeartImage;
        public Animator HeartAnimator;
    }
    private HeartItem[] _heartItems;

    void Awake()
    {
        _heartItems = new HeartItem[PlayerBase.MaxHp];
        for (int i = 0; i < PlayerBase.MaxHp; i++)
        {
            GameObject heart = GameObject.Find("Heart" + i);
            if (heart != null)
            {
                _heartItems[i] = new HeartItem();
                _heartItems[i].HeartImage = heart.GetComponent<Image>();
                _heartItems[i].HeartAnimator = heart.GetComponent<Animator>();
            }
        }
    }

    // Use this for initialization
    void Start()
    {

        _lastLocalHumanHp = -1;
    }

    private void UpdateHpItem()
    {
        Human localHuman = GameObjectsManager.GetInstance().GetLocalHuman();
        if (localHuman != null && _lastLocalHumanHp != localHuman.Hp)
        {
            _lastLocalHumanHp = localHuman.Hp;

            for (int i = 0; i < PlayerBase.MaxHp; i++)
            {
                Image heartImage = _heartItems[i].HeartImage;
                Animator HeartAnimator = _heartItems[i].HeartAnimator;
                if (i < _lastLocalHumanHp)
                {
                    // Sprite
                    heartImage.sprite = HeartPinkSprite;
                    //var heartGreyImage = Resources.Load("UI/HUD/Texture/" + "Grey") as Texture2D;
                    //Sprite heartGreySprite = Sprite.Create(heartGreyImage, new Rect(0, 0, heartGreyImage.width, heartGreyImage.height), new Vector2(0.5f, 0.5f));
                    //heartImage.sprite = heartGreySprite;


                    // AnimatorController
                    if (i == _lastLocalHumanHp - 1)
                    {
                        HeartAnimator.runtimeAnimatorController = HeartGrowAnimatorController;
                        //AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(AnimationControllerPath + "/" + name + ".controller");
                        //AnimatorOverrideController overrideController = new AnimatorOverrideController();
                        //overrideController.runtimeAnimatorController = GetComponent<Animator>().runtimeAnimatorController;
                        //AnimationClip clip = Resources.Load<AnimationClip>("Player/Animations/test");
                        //overrideController["Nova@yunqiu"] = clip;
                        //GetComponent<Animator>().avatar = Resources.Load<Avatar>("Player/Models/Nova");
                        //GetComponent<Animator>().runtimeAnimatorController = overrideController;
                    }
                    else
                    {
                        HeartAnimator.runtimeAnimatorController = null;
                    }

                }
                else
                {
                    // Sprite
                    heartImage.sprite = HeartGreySprite;

                    // AnimatorController
                    HeartAnimator.runtimeAnimatorController = null;
                }
            }
        }
        else if (localHuman == null)
        {
            for (int i = 0; i < PlayerBase.MaxHp; i++)
            {
                if (_heartItems != null)
                {
                    Image heartImage = _heartItems[i].HeartImage;
                    heartImage.sprite = HeartGreySprite;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ((GameManager.s_gameSingleMultiType == GameSingleMultiType.SingleGame)
            || (GameManager.s_gameSingleMultiType == GameSingleMultiType.MultiGame_WangHu)
            || (GameManager.s_NetWorkClient.MultiGameState == NetWorkClient.GameState.Game_Running))
        {
            UpdateHpItem();
        }
    }

    public void SetlastLocalHumanHp(int hp)
    {
        _lastLocalHumanHp = hp;
    }
}
