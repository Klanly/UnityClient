﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ArkCrossEngine;

public class UITreasureMap : UnityEngine.MonoBehaviour
{

    private const int c_LevelNum = 12;//關卡
    public UIButton[] buttonArr = new UIButton[c_LevelNum];
    public UISprite[] spArrowArr = new UISprite[c_LevelNum - 1];
    public UnityEngine.AudioClip m_AudioClipOpenChest;//打开宝箱时的音效
    public UIButton btnReset = null;
    public UnityEngine.GameObject goLevelInfo = null;
    public UnityEngine.GameObject goSlot = null;
    public UnityEngine.GameObject goTreasureButton;
    public UnityEngine.GameObject goMap = null;
    public UIGrid uiGrid = null;
    public UILabel lblCd = null;
    public UILabel lblReset = null;
    public UnityEngine.Vector3 originalPos = new UnityEngine.Vector3(-720, 68.5f, 0);
    public float UpPosY = 68.5f;
    public float DownPosY = -120f;
    public float DeltaX = 120;
    private bool m_IsInitializedMap = false;
    public float duration = 0.6f;
    public UnityEngine.AnimationCurve animationCureve;
    private int m_LevelIndex = -1;
    private int m_CurrentClickIndex = -1;
    private List<object> m_EventList = new List<object>();

    public UITreasurePlayerInfo uiPlayerInfo = null;
    //
    public void UnSubscribe()
    {
        try
        {
            for (int i = 0; i < m_EventList.Count; i++)
            {
                if (m_EventList[i] != null)
                {
                    ArkCrossEngine.LogicSystem.EventChannelForGfx.Unsubscribe(m_EventList[i]);
                }
            }
            /*
	      foreach (object eo in m_EventList) {
	        if (eo != null) {
	          ArkCrossEngine.LogicSystem.EventChannelForGfx.Unsubscribe(eo);
	        }
	      }*/
            m_EventList.Clear();
        }
        catch (Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogFromGfx("[Error]:Exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    // Use this for initialization
    void Start()
    {
        try
        {
            //CYGTConnector.ShowCYGTSDK();
            object obj = null;
            obj = LogicSystem.EventChannelForGfx.Subscribe("ge_ui_unsubscribe", "ui", UnSubscribe);
            if (obj != null) m_EventList.Add(obj);
            obj = LogicSystem.EventChannelForGfx.Subscribe<ArkCrossEngine.Network.GeneralOperationResult>("ge_expedition_info", "expedition", HandlerResetResult);
            if (obj != null) m_EventList.Add(obj);
            obj = LogicSystem.EventChannelForGfx.Subscribe<int, int[], int[], ArkCrossEngine.Network.GeneralOperationResult>("ge_expedition_award", "expedition", HandlerAwardResult);
            if (goLevelInfo != null) NGUITools.SetActive(goLevelInfo, false);
            for (int index = 0; index < buttonArr.Length; ++index)
            {
                if (buttonArr[index] != null)
                {
                    UIEventListener.Get(buttonArr[index].gameObject).onClick = OnLevelButtonClick;
                }
            }
            InitTreasureMap();
            TransTreasureMapPosition();
        }
        catch (System.Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogErrorFromGfx("Exception {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    // Update is called once per frame
    void Update()
    {
        try
        {
            CalCountDown();
        }
        catch (System.Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogErrorFromGfx("Exception {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    void OnEnable()
    {
        try
        {
            InitTreasureMap();
        }
        catch (System.Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogErrorFromGfx("Exception {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    //初始化远征界面
    private void InitTreasureMap()
    {
        if (goTreasureButton != null && goMap != null && !m_IsInitializedMap)
        {
            for (int index = 1; index <= 12; ++index)
            {
                UnityEngine.GameObject go = NGUITools.AddChild(goMap, goTreasureButton);
                if (go != null)
                {
                    float xPos = (index - 1) * DeltaX + originalPos.x;
                    float yPos = index % 2 == 0 ? DownPosY : UpPosY;
                    go.transform.localPosition = new UnityEngine.Vector3(xPos, yPos, 0);
                    go.name = "gk" + index;
                    UIButton uiBtn = go.GetComponent<UIButton>();
                    buttonArr[index - 1] = uiBtn;
                    UITreasureButton tb = go.GetComponent<UITreasureButton>();
                    if (tb != null) tb.SetLabelNumber(index);

                }
            }
            m_IsInitializedMap = true;
        }
        RoleInfo role_info = LobbyClient.Instance.CurrentRole;
        if (role_info != null)
        {
            ExpeditionPlayerInfo ep_info = role_info.GetExpeditionInfo();
            if (ep_info != null)
            {
                int currentSchedule = ep_info.Schedule;
                for (int index = 0; index < buttonArr.Length; ++index)
                {
                    if (index < currentSchedule && buttonArr[index] != null)
                    {
                        //小于当前进度的按钮
                        if (index < ep_info.Tollgates.Length && ep_info.Tollgates[index] != null)
                        {
                            if (ep_info.Tollgates[index].IsAcceptedAward)
                            {
                                UITreasureButton tb = buttonArr[index].GetComponent<UITreasureButton>();
                                if (tb != null) tb.SetTreasureButtonState(ButtonState.Openned);
                            }
                            else
                            {
                                UITreasureButton tb = buttonArr[index].GetComponent<UITreasureButton>();
                                if (tb != null) tb.SetTreasureButtonState(ButtonState.Finished);
                            }
                            TweenPosition tweenPos = buttonArr[index].GetComponent<TweenPosition>();
                            if (tweenPos) Destroy(tweenPos);
                        }
                    }
                    //大于当前进度
                    if (index > currentSchedule && buttonArr[index] != null)
                    {
                        UITreasureButton tb = buttonArr[index].GetComponent<UITreasureButton>();
                        if (tb != null) tb.SetTreasureButtonState(ButtonState.Lock);
                        TweenPosition tweenPos = buttonArr[index].GetComponent<TweenPosition>();
                        if (tweenPos) Destroy(tweenPos);
                    }
                    //等于当前进度
                    if (index == currentSchedule && buttonArr[index] != null)
                    {
                        UITreasureButton tb = buttonArr[index].GetComponent<UITreasureButton>();
                        if (tb != null) tb.SetTreasureButtonState(ButtonState.UnLock);
                        //重置跳动的位置，以防按钮脱离原来位置
                        UnityEngine.Vector3 fromPos = buttonArr[index].transform.localPosition;
                        if (index % 2 == 0)
                            fromPos.y = originalPos.y;
                        else
                            fromPos.y = DownPosY;
                        buttonArr[index].transform.localPosition = fromPos;
                        UnityEngine.Vector3 targetPos = buttonArr[index].transform.localPosition + new UnityEngine.Vector3(0, 10f, 0);
                        TweenPosition tweenPos = TweenPosition.Begin(buttonArr[index].gameObject, duration, targetPos);
                        tweenPos.animationCurve = animationCureve;
                        tweenPos.style = UITweener.Style.PingPong;
                    }
                    if (index < currentSchedule)
                    {
                        if (index < spArrowArr.Length && spArrowArr[index] != null)
                            spArrowArr[index].spriteName = "guan-qian-lu-biao";
                    }
                    else
                    {
                        if (index < spArrowArr.Length && spArrowArr[index] != null)
                            spArrowArr[index].spriteName = "guan-qian-lu-biao2";
                    }
                }
            }
        }
    }
    //根据打到的当前关卡设置位置
    private void TransTreasureMapPosition()
    {
        RoleInfo role = LobbyClient.Instance.CurrentRole;
        if (role == null) return;
        ExpeditionPlayerInfo ep_info = role.GetExpeditionInfo();
        if (ep_info != null)
        {
            int shedule = ep_info.Schedule;
            //在前七关之前需要移动显示的位置
            if (shedule < 7 && shedule < buttonArr.Length && null != buttonArr[shedule])
            {
                float delta = buttonArr[shedule].transform.localPosition.x - originalPos.x;
                if (goMap != null)
                {
                    UnityEngine.Vector3 pos = goMap.transform.localPosition;
                    goMap.transform.localPosition = new UnityEngine.Vector3(pos.x - delta, pos.y, pos.z);
                }
            }
        }
    }
    //
    private void EnableBoxCollider(UnityEngine.GameObject go, bool enable)
    {
        if (go != null)
        {
            UnityEngine.BoxCollider bc = go.GetComponent<UnityEngine.BoxCollider>();
            if (bc != null) bc.enabled = enable;
        }
    }

    //关卡按钮点击
    private void OnLevelButtonClick(UnityEngine.GameObject go)
    {
        if (go != null)
        {
            switch (go.name)
            {
                case "gk1": ShowLevelInfo(0); break;
                case "gk2": ShowLevelInfo(1); break;
                case "gk3": ShowLevelInfo(2); break;
                case "gk4": ShowLevelInfo(3); break;
                case "gk5": ShowLevelInfo(4); break;
                case "gk6": ShowLevelInfo(5); break;
                case "gk7": ShowLevelInfo(6); break;
                case "gk8": ShowLevelInfo(7); break;
                case "gk9": ShowLevelInfo(8); break;
                case "gk10": ShowLevelInfo(9); break;
                case "gk11": ShowLevelInfo(10); break;
                case "gk12": ShowLevelInfo(11); break;
            }
        }
    }
    //計算倒計時并顯示
    public void CalCountDown()
    {
        RoleInfo role_info = LobbyClient.Instance.CurrentRole;
        if (role_info != null)
        {
            double currentTime = ArkCrossEngine.TimeUtility.CurTimestamp;
            ExpeditionPlayerInfo ep_info = role_info.GetExpeditionInfo();
            if (ep_info != null)
            {
                double startTime = ep_info.LastResetTimestamp;
                int totleTime = (int)ep_info.ExpeditionResetIntervalTime;
                int leftTime = totleTime - (int)(currentTime - startTime);
                leftTime = leftTime < 0 ? 0 : leftTime;
                int hour = leftTime / 3600;
                int munite = (leftTime % 3600) / 60;
                int second = leftTime % 60;
                string CHN = StrDictionaryProvider.Instance.GetDictString(451);
                string CHN_RESET = StrDictionaryProvider.Instance.GetDictString(452);
                string res = string.Format(CHN, hour, munite, second);
                if (lblCd != null) lblCd.text = res;
                if (leftTime <= 0)
                {
                    string str = string.Format(CHN_RESET, 1);
                    if (lblReset != null) lblReset.text = str;
                    if (btnReset != null) btnReset.isEnabled = true;
                }
                else
                {
                    string str = string.Format(CHN_RESET, 0);
                    if (lblReset != null) lblReset.text = str;
                    if (btnReset != null) btnReset.isEnabled = false;
                }
            }
        }
    }
    //点击按钮显示远征二级界面信息 或者领取奖励
    public void ShowLevelInfo(int index)
    {
        RoleInfo role_info = LobbyClient.Instance.CurrentRole;
        if (role_info != null)
        {
            m_CurrentClickIndex = index;
            ExpeditionPlayerInfo ep_info = role_info.GetExpeditionInfo();
            if (ep_info != null)
            {
                //角色已经死亡、且点击的不是宝箱则弹出提示对话框
                if (ep_info.Hp <= 0 && index == ep_info.Schedule)
                {
                    string CHN_DESC = StrDictionaryProvider.Instance.GetDictString(453);
                    string CHN_CON = StrDictionaryProvider.Instance.GetDictString(4);
                    ArkCrossEngine.LogicSystem.EventChannelForGfx.Publish("ge_show_dialog", "ui", CHN_DESC, CHN_CON, null, null, null, false);
                    return;
                }
                int currentSchedule = ep_info.Schedule;
                if (index < currentSchedule)
                {
                    LogicSystem.EventChannelForGfx.Publish("ge_ui_connect_hint", "ui", false, true);
                    GfxSystem.EventChannelForLogic.Publish("ge_expedition_award", "lobby", index);
                    return;
                }
            }
        }
        m_LevelIndex = index;
        DestroyChildrenInGrid();
        if (goLevelInfo != null) NGUITools.SetActive(goLevelInfo, true);
        if (role_info != null)
        {
            //获取远征信息
            ExpeditionPlayerInfo exPlayInfo = role_info.GetExpeditionInfo();
            if (exPlayInfo != null)
            {
                WorldSystem.Instance.ExpeditionErrorCheck();
                if (index < exPlayInfo.Tollgates.Length || exPlayInfo.Tollgates[index] != null)
                {
                    ExpeditionPlayerInfo.TollgateData levelData = exPlayInfo.Tollgates[index];
                    //NPC
                    if (levelData.Type == EnemyType.ET_Boss || levelData.Type == EnemyType.ET_Monster)
                    {
                        int numLimit = 0;
                        for (int i = 0; i < levelData.EnemyList.Count; ++i)
                        {
                            int monsterId = levelData.EnemyList[i];
                            int attrId = -1;
                            if (i < levelData.EnemyAttrList.Count)
                                attrId = levelData.EnemyAttrList[i];
                            if (goSlot == null && uiGrid == null) break;
                            UnityEngine.GameObject go = NGUITools.AddChild(uiGrid.gameObject, goSlot);
                            if (go == null) continue;
                            UITreasureSlot uiTsSlot = go.GetComponent<UITreasureSlot>();
                            if (uiTsSlot != null) uiTsSlot.SetLevelInfo(monsterId, attrId);
                            if (++numLimit >= 4) break;
                        }
                    }
                    else
                    {
                        //Player
                        foreach (ExpeditionImageInfo enemy_info in levelData.UserImageList)
                        {
                            if (goSlot == null && uiGrid == null) break;
                            UnityEngine.GameObject go = NGUITools.AddChild(uiGrid.gameObject, goSlot);
                            if (go == null) continue;
                            UITreasureSlot uiTsSlot = go.GetComponent<UITreasureSlot>();
                            if (uiTsSlot != null)
                            {
                                if (enemy_info.FightingScore <= 0)
                                {
                                    int min = (int)(role_info.FightingScore - role_info.FightingScore * 0.2f);
                                    int max = (int)(role_info.FightingScore + role_info.FightingScore * 0.2f);
                                    int rnd = CrossEngineHelper.Random.Next(min, max);
                                    enemy_info.FightingScore = rnd;
                                }
                                uiTsSlot.SetLevelInfo(enemy_info);
                            }
                        }
                    }
                }
                if (uiGrid != null) uiGrid.Reposition();
            }
        }
    }
    public void DestroyChildrenInGrid()
    {
        if (uiGrid != null)
        {
            while (uiGrid.transform.childCount > 0)
            {
                UnityEngine.Transform ts = uiGrid.transform.GetChild(0);
                if (ts != null) NGUITools.Destroy(ts.gameObject);
            }
        }
    }
    private void HandlerAwardResult(int money, int[] items, int[] nums, ArkCrossEngine.Network.GeneralOperationResult result)
    {
        try
        {
            LogicSystem.EventChannelForGfx.Publish("ge_ui_connect_hint", "ui", false, false);
            if (result == ArkCrossEngine.Network.GeneralOperationResult.LC_Succeed)
            {
                if (m_AudioClipOpenChest != null) NGUITools.PlaySound(m_AudioClipOpenChest);
                List<int> itemList = new List<int>();
                List<int> numList = new List<int>();
                if (itemList != null && numList != null && items != null && nums != null)
                {
                    for (int i = 0; i < items.Length; ++i)
                    {
                        itemList.Add(items[i]);
                    }
                    for (int i = 0; i < nums.Length; ++i)
                    {
                        numList.Add(nums[i]);
                    }
                    UnityEngine.GameObject goTaskAward = UIManager.Instance.GetWindowGoByName("TaskAward");
                    if (goTaskAward != null)
                    {
                        TaskAward taskAward = goTaskAward.GetComponent<TaskAward>();
                        taskAward.SetAwardForMermanKing(money, 0, 0, itemList, numList, m_CurrentClickIndex);
                    }
                }
                else
                {
                    UnityEngine.GameObject goTaskAward = UIManager.Instance.GetWindowGoByName("TaskAward");
                    if (goTaskAward != null)
                    {
                        TaskAward taskAward = goTaskAward.GetComponent<TaskAward>();
                        taskAward.SetAwardForMermanKing(money, 0, 0, null, null, m_CurrentClickIndex);
                    }
                }
                InitTreasureMap();
                UIManager.Instance.ShowWindowByName("TaskAward");
            }
            else
            {
                string CHN_DESC = StrDictionaryProvider.Instance.GetDictString(454);
                string CHN_CON = StrDictionaryProvider.Instance.GetDictString(4);
                ArkCrossEngine.LogicSystem.EventChannelForGfx.Publish("ge_show_dialog", "ui", CHN_DESC, CHN_CON, null, null, null, false);
            }
        }
        catch (Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogFromGfx("[Error]:{0},{1}", ex.Message, ex.StackTrace);
        }
    }
    private void HandlerResetResult(ArkCrossEngine.Network.GeneralOperationResult result)
    {
        try
        {
            if (ArkCrossEngine.Network.GeneralOperationResult.LC_Succeed == result)
            {
                InitTreasureMap();
                if (uiPlayerInfo != null)
                {
                    uiPlayerInfo.ResetPlayerInfo();
                }
            }
            else
            {
                if (ArkCrossEngine.Network.GeneralOperationResult.LC_Failure_Time == result)
                {
                    string chn_desc = StrDictionaryProvider.Instance.GetDictString(459);
                    LogicSystem.EventChannelForGfx.Publish("ge_screen_tip", "ui", chn_desc, UIScreenTipPosEnum.AlignTop, UnityEngine.Vector3.zero);
                }
            }
        }
        catch (Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogFromGfx("[Error]:Exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    private void HideLevelInfo()
    {
        if (goLevelInfo != null)
            NGUITools.SetActive(goLevelInfo, false);
    }
    public void OnCloseButton()
    {
        HideLevelInfo();
        UIManager.Instance.HideWindowByName("cangbaotu");
    }
    public void OnResetButton()
    {
        RoleInfo role_info = LobbyClient.Instance.CurrentRole;
        if (null != role_info)
        {
            UserInfo user = role_info.GetPlayerSelfInfo();
            if (null != user)
            {
                ExpeditionPlayerInfo e = role_info.GetExpeditionInfo();
                if (null != e)
                {
                    if (e.Schedule > 0)
                    {
                        CharacterProperty property = user.GetActualProperty();
                        GfxSystem.EventChannelForLogic.Publish("ge_expedition_reset", "lobby", 1000, 1000, 0, 0, true, false);
                    }
                    else
                    {
                        ArkCrossEngine.MyAction<int> Func = HandleDialog;
                        string CHN_CONFIRM = StrDictionaryProvider.Instance.GetDictString(4); //确定
                        string CHN_CANCEL = StrDictionaryProvider.Instance.GetDictString(9);  //取消
                        string CHN_DESC = StrDictionaryProvider.Instance.GetDictString(460);
                        LogicSystem.EventChannelForGfx.Publish("ge_show_dialog", "ui", CHN_DESC, null, CHN_CONFIRM, CHN_CANCEL, Func, false);
                    }
                }
            }
        }
    }

    private void HandleDialog(int action)
    {
        if (0 == action)
        {

        }
        else if (1 == action)
        {
            RoleInfo role_info = LobbyClient.Instance.CurrentRole;
            if (null != role_info)
            {
                UserInfo user = role_info.GetPlayerSelfInfo();
                if (null != user)
                {
                    CharacterProperty property = user.GetActualProperty();
                    GfxSystem.EventChannelForLogic.Publish("ge_expedition_reset", "lobby", 1000, 1000, 0, 0, true, false);
                }
            }
        }
    }
    public void OnStartButton()
    {
        HideLevelInfo();
        GfxSystem.EventChannelForLogic.Publish("ge_request_expedition", "lobby", 4001, m_LevelIndex);
    }
    public void OnCloseLevelInfo()
    {
        HideLevelInfo();
    }
}
