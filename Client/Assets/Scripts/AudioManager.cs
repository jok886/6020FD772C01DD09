using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance = null;
    public static AudioManager GetInstance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<AudioManager>();
            }
            return _instance;
        }
    }
    public enum Sound_Place
    {
        Place_Start = 0,

        DaTong = Place_Start,
        MeiCheng,
        NanFeng,
        ShouCang,

        PlaceNum
    }
    private string[] m_soundPlaceName =
    {
        "DaTong",
        "MeiCheng",
        "NanFeng",
        "ShouCang"
    };

    private string[] m_cardClipName =
    {
        "1筒",
        "2筒",
        "3筒",
        "4筒",
        "5筒",
        "6筒",
        "7筒",
        "8筒",
        "9筒",

        "1万",
        "2万",
        "3万",
        "4万",
        "5万",
        "6万",
        "7万",
        "8万",
        "9万",

        "1条",
        "2条",
        "3条",
        "4条",
        "5条",
        "6条",
        "7条",
        "8条",
        "9条",

        "东风",
        "南风",
        "西风",
        "北风",
        "红中",
        "发财",
        "白板",

        "buhua"
    };


    public enum Sound_Defines
    {
        Sound_Start = 0,

        SOUND_PENG = Sound_Start,
        SOUND_CHI = 1,
        SOUND_GANG = 2,
        SOUND_HU = 3,
        SOUND_ZIMO = 4,

        //聊天配音
        SOUND_ChatSound_Start = 5,
        SOUND_KuaiDian = SOUND_ChatSound_Start,
        SOUND_ZaiBuGeiWoPeng = 6,
        SOUND_JiaoSao = 7,
        SOUND_HaiChi = 8,
        SOUND_XiangChi = 9,
        SOUND_CaiShenNe = 10,
        SOUND_YaoChiDe = 11,
        SOUND_BuZuoNiXiaJiaLe = 12,
        SOUND_BuLaiLe = 13,
        SOUND_ChiBuXiaoDa = 14,
        SOUND_MeiHu = 15,

        //胡牌配音
        SOUND_GameEndSound_Start = 16,
        SOUND_GameEndSound_BaoTou = SOUND_GameEndSound_Start,
        SOUND_GameEndSound_13BuDa = 17,
        SOUND_GameEndSound_8DuiKaoTou = 18,
        SOUND_GameEndSound_8DuiPiaoCai = 19,
        SOUND_GameEndSound_8DuiZiMo = 20,
        SOUND_GameEndSound_GangKai = 21,
        SOUND_GameEndSound_GangPiao = 22,
        SOUND_GameEndSound_PiaoCai = 23,
        SOUND_GameEndSound_PiaoGang = 24,
        SOUND_GameEndSound_Qing8DuiZiMo = 25,
        SOUND_GameEndSound_QingFengZiZiMo = 26,
        SOUND_GameEndSound_QingYiSeGangKai = 27,
        SOUND_GameEndSound_QingYiSeKaoTou = 28,
        SOUND_GameEndSound_QingYiSeQiangGang = 29,
        SOUND_GameEndSound_QingYiSeZiMo = 30,
        SOUND_GameEndSound_GangBao = 31,

        SOUND_Num
    }
    private string[] m_soundClipName =
    {
        "碰",
        "吃",
        "杠",
        "老天开眼了，总算糊了一把",
        "自摸",

        "动作快点啦",//"KuaiDian",
        "再不给我碰，我就打八对子了",//"ZaiBuGeiWoPeng",
        "叫扫，这么没风头的",//"JiaoSao",
        "还吃？抓到财神不告诉你",//"HaiChi",
        "想吃？没有的",//"XiangChi",
        "财神呢？财神死哪里去了？",//"CaiShenNe",
        "下面是个人，要吃的",//"YaoChiDe",
        "下次再也不坐你下家",//"BuZuoNiXiaJiaLe",
        "不来了，不来了，你什么风头啊",//"Bulaile",
        "这个麻将吃不消打，下次再来过",//"ChiBuXiaoDa",
        "这么好的牌都没得糊",//"MeiHu",

        "拷头",//"KaoTou",
        "13幺",//"13BuDa",
        "八对拷头",//"8DuiKaoTou",
        "八对飘财",//"8DuiPiaoCai",
        "八对自摸",//"8DuiZiMo",
        "杠开",//"GangKai",
        "杠飘",//"GangPiao",
        "飘财",//"PiaoCai",
        "飘杠",//"PiaoGang",
        "清八对自摸",//"Qing8DuiZiMo",
        "清风子自摸",//"QingFengZiZiMo",
        "清一色杠开",//"QingYiSeGangKai",
        "清一色拷头",//"QingYiSeKaoTou",
        "清一色抢杠",//"QingYiSeQiangGang",
        "清一色自摸",//"QingYiSeZiMo"
        "财杠",
    };

    public enum Sound_Effect_Defines
    {
        Sound_EFFECT_Start = 0,

        SOUND_EFFECT_Jump               = Sound_EFFECT_Start,
        SOUND_EFFECT_Crouch,
        SOUND_EFFECT_PickObj,
        SOUND_EFFECT_PickObjOutRange,
        SOUND_EFFECT_Boom,
        SOUND_HIDE_BEFOUNDED,
        SOUND_HIDE_LOCK,
        SOUND_HIDE_UNLOCK,
        SOUND_SEEK_DEAD,
        SOUND_SEEK_STEP,
        SOUND_TIPS_STARTOREND,
        SOUND_TIPS_START_SEEK,
        SOUND_TIME_SEC,
        SOUND_BUTTON_CLICK,
        SOUND_BUTTON_CANCEL,
        SOUND_SIGNGIN_AWARED,
        SOUND_LOGO,
        SPIND_TIPS_EMAIL,

        SOUND_EFFECT_Num
    }
    public enum Sound_BGM_Difines
    {
        BGM1,
        BGM2,
        BGM3,
        BGM4,
        BGM5,

        BGM_Num
    }
    private string[] m_soundEffectClipName =
    {
        "跳",
        "蹲",
        "击打物品",
        "击打物品超出范围",
        "爆炸",
        "物品被抓到",
        "物品锁定",
        "物品锁解",
        "警察死亡",
        "警察脚步",
        "提示音-开头(8-45-100)",
        "提示音-开头-45秒倒计时结束-警察被放出来的声音",
        "倒计时声音(sec)",
        "按钮UI-按下",
        "按钮UI-取消+后退+关闭",
        "签到+奖励窗口-弹出的声音",
        "闪屏Logo",
        "提示音-有人关注或者邮件"
    };

    //public AudioClip[] MaleCardClip;
    //public AudioClip[] FemaleCardClip;

    //public AudioClip[] MaleEffectClip;
    //public AudioClip[] FemaleEffectClip;

    private Dictionary<int, int> cardValueToClipMap = new Dictionary<int, int>(34);//9 * 3 + 4(风) + 3（中发白）// + 1 （补花）找不到的都是补花

    private struct AudioClipInfo
    {
        public string clipFilePlath;
        public AudioClip clip;
    }
    private AudioClipInfo[,,] m_cardClipInfos = new AudioClipInfo[(int)Sound_Place.PlaceNum, 2, 9 + 9 + 9 + 7 + 1];
    private AudioClipInfo[,,] m_soundClipInfos = new AudioClipInfo[(int)Sound_Place.PlaceNum, 2, (int)Sound_Defines.SOUND_Num];
    private AudioClipInfo[] m_soundEffect = new AudioClipInfo[(int)Sound_Effect_Defines.SOUND_EFFECT_Num];
    private AudioClipInfo[] m_BGM = new AudioClipInfo[(int)Sound_BGM_Difines.BGM_Num];
    ///private Dictionary<Sound_Place, Dictionary> audioClipInfos = new Dictionary<Sound_Place, Dictionary>();

    private HNGameManager hnGameManager;

    void Start()
    {
        hnGameManager = GameObject.FindObjectOfType<HNGameManager>();

        for (int i = 0; i < 9; i++)
        {
            cardValueToClipMap.Add(i + 11, i);
            cardValueToClipMap.Add(i + 21, i + 9);
            cardValueToClipMap.Add(i + 31, i + 18);
        }
        for (int i = 0; i < 4; i++)
        {
            cardValueToClipMap.Add(i + 41, i + 27);
        }
        for (int i = 0; i < 3; i++)
        {
            cardValueToClipMap.Add(i + 45, i + 31);
        }

        //init m_soundClipInfos m_cardClipInfos
        for (int place = (int)Sound_Place.Place_Start; place < (int)Sound_Place.PlaceNum; place++)
        {
            for (int iGender = 0; iGender < 2; iGender++)
            {
                string clipFilePlath = "Sounds/" + m_soundPlaceName[place] + "/";
                if (iGender == 1)
                {
                    clipFilePlath = clipFilePlath + "Male/";
                }
                else
                {
                    clipFilePlath = clipFilePlath + "Female/";
                }

                for (int soundId = (int)Sound_Defines.Sound_Start; soundId < (int)Sound_Defines.SOUND_Num; soundId++)
                {
                    string soundClipFilePlath = clipFilePlath + m_soundClipName[soundId];
                    m_soundClipInfos[place, iGender, soundId].clipFilePlath = soundClipFilePlath;
                }

                for (int cardClipId = 0; cardClipId < 9 + 9 + 9 + 7 + 1; cardClipId++)
                {
                    string cardClipFilePlath = clipFilePlath + m_cardClipName[cardClipId];
                    m_cardClipInfos[place, iGender, cardClipId].clipFilePlath = cardClipFilePlath;
                }
            }
        }

        for (int effectId = (int)Sound_Effect_Defines.Sound_EFFECT_Start; effectId < (int)Sound_Effect_Defines.SOUND_EFFECT_Num; effectId++)
        {
            m_soundEffect[effectId].clipFilePlath = "Sounds/Effects/" + m_soundEffectClipName[effectId];
        }
        for (int bgmId = (int)Sound_BGM_Difines.BGM1; bgmId < (int)Sound_BGM_Difines.BGM_Num; bgmId++)
        {
            m_BGM[bgmId].clipFilePlath = "Sounds/BGM/" + (bgmId + 1);
        }
    }

    public AudioClip getCardClipByGender(int iGender, int nCardIntValue)
    {
        //iGender = 1;
        int cardClipNameIdx = 34;
        if (cardValueToClipMap.ContainsKey(nCardIntValue))
        {
            cardClipNameIdx = cardValueToClipMap[nCardIntValue];
        }

        int iPlace = (int)hnGameManager.m_soundPlace;
        if (m_cardClipInfos[iPlace, iGender, cardClipNameIdx].clip == null)
        {
            string clipFilePlath = m_cardClipInfos[iPlace, iGender, cardClipNameIdx].clipFilePlath;
            m_cardClipInfos[iPlace, iGender, cardClipNameIdx].clip = Resources.Load(clipFilePlath) as AudioClip;
        }
        return m_cardClipInfos[(int)hnGameManager.m_soundPlace, iGender, cardClipNameIdx].clip;

        /*
        if (cardValueToClipMap.ContainsKey(nCardIntValue))
        {
            if (iGender == 0)
            {
                return MaleCardClip[cardValueToClipMap[nCardIntValue]];
            }
            else
            {
                return FemaleCardClip[cardValueToClipMap[nCardIntValue]];
            }
        }

        return iGender == 0 ? MaleCardClip[34] : FemaleCardClip[34];
        */
    }

    public AudioClip getSoundByGender(int iGender, int iSoundId)
    {
        //iGender = 1;
        int iPlace = (int)hnGameManager.m_soundPlace;
        if (m_soundClipInfos[iPlace, iGender, iSoundId].clip == null)
        {
            string clipFilePlath = m_soundClipInfos[iPlace, iGender, iSoundId].clipFilePlath;
            m_soundClipInfos[iPlace, iGender, iSoundId].clip = Resources.Load(clipFilePlath) as AudioClip;
        }
        return m_soundClipInfos[iPlace, iGender, iSoundId].clip;

        ///return iGender == 0 ? MaleEffectClip[iSoundId] : FemaleEffectClip[iSoundId];
    }

    public AudioClip getEffectClip(int iEffectId)
    {
        if(m_soundEffect[iEffectId].clip == null)
        {
            string clipFilePlath = m_soundEffect[iEffectId].clipFilePlath;
            m_soundEffect[iEffectId].clip = Resources.Load(clipFilePlath) as AudioClip;
        }
        return m_soundEffect[iEffectId].clip;
    }
    public AudioClip getBGMClip(int iBGMId)
    {
        if (m_BGM[iBGMId].clip == null)
        {
            string clipFilePlath = m_BGM[iBGMId].clipFilePlath;
            m_BGM[iBGMId].clip = Resources.Load(clipFilePlath) as AudioClip;
        }
        return m_BGM[iBGMId].clip;
    }
    public void PlaySound(int soundIndex)
    {
        hnGameManager.PlaySoundEffect(-1, soundIndex);
    }
}
