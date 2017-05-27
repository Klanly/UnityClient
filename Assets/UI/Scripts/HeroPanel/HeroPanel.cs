﻿using System;
using System.Collections.Generic;
using ArkCrossEngine;
using System.Text;
public enum PanelType
{
    MySelf,
    Enemy
}
public class HeroPanel : UnityEngine.MonoBehaviour
{

    // Use this for initialization
    public UnityEngine.GameObject HeroView = null;
    public UISprite spHeroPortrait = null;
    public UIProgressBar hpProgressBar = null;
    public UIProgressBar mpProgressBar = null;
    public UILabel lblLevel = null;
    public UILabel lblHp = null;
    public UILabel lblMp = null;
    public PanelType panelType = PanelType.MySelf;
    public UnityEngine.GameObject btnQuit;
    private int preLevel = -1;
    private bool m_IsInitialized = false;
    private List<object> m_EventList = new List<object>();
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
            m_EventList.Clear();
        }
        catch (Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogicLog("[Error]:Exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    void Start()
    {
        try
        {
            object obj = null;
            obj = LogicSystem.EventChannelForGfx.Subscribe("ge_ui_unsubscribe", "ui", UnSubscribe);
            if (obj != null) m_EventList.Add(obj);
            obj = LogicSystem.EventChannelForGfx.Subscribe<GfxUserInfo>("ge_update_gfxuserinfo", "ui", UpdateGfxUserinfo);
            if (obj != null) m_EventList.Add(obj);
        }
        catch (System.Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogicErrorLog("Exception {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    // Update is called once per frame
    void Update()
    {
        try
        {
            if (panelType == PanelType.MySelf)
            {
                UpdateSelfPanel();
            }
            else
            {
                UpdateEnemyPanel();
            }
        }
        catch (System.Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogicErrorLog("Exception {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    //重新布局退出按钮
    public void LayoutQuitBtn()
    {
        if (btnQuit != null)
        {
            btnQuit.transform.localPosition = new UnityEngine.Vector3(433f, -26f, 0f);
        }
    }
    //自己的血条
    public void UpdateSelfPanel()
    {
        if (m_GfxUserInfo != null)
        {
            SharedGameObjectInfo share_info = LogicSystem.GetSharedGameObjectInfo(m_GfxUserInfo.m_ActorId);
            if (null != share_info)
            {
                UpdateHealthBar((int)share_info.Blood, (int)share_info.MaxBlood);
                UpdateMp((int)share_info.Energy, (int)share_info.MaxEnergy);
                SetHeroLevel(m_GfxUserInfo.m_Level);
                if (!m_IsInitialized)
                {
                    Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(m_GfxUserInfo.m_HeroId);
                    if (playerData != null)
                    {
                        m_IsInitialized = true;
                        SetHeroPortrait(playerData.m_Portrait);
                    }
                }
            }
        }
    }
    //敌人的血条
    public void UpdateEnemyPanel()
    {
        if (m_GfxUserInfo == null) return;
        SharedGameObjectInfo enemy_info = LogicSystem.GetSharedGameObjectInfo(m_GfxUserInfo.m_ActorId);
        if (enemy_info != null)
        {
            UpdateHealthBar((int)enemy_info.Blood, (int)enemy_info.MaxBlood);
            UpdateMp((int)enemy_info.Energy, (int)enemy_info.MaxEnergy);
            SetHeroLevel(m_GfxUserInfo.m_Level);
            if (!m_IsInitialized)
            {
                Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(m_GfxUserInfo.m_HeroId);
                if (playerData != null)
                {
                    m_IsInitialized = true;
                    SetHeroPortrait(playerData.m_Portrait);
                }
            }
        }
    }
    //设置玩家头像
    void SetHeroPortrait(string portrait)
    {
        if (null != spHeroPortrait)
        {
            spHeroPortrait.spriteName = portrait;
            UIButton btn = spHeroPortrait.GetComponent<UIButton>();
            if (btn != null)
                btn.normalSprite = portrait;
        }
    }
    //设置玩家等级
    void SetHeroLevel(int level)
    {
        if (preLevel != level)
        {
            preLevel = level;
            if (lblLevel != null)
                lblLevel.text = level.ToString();
        }
    }
    //设置玩家昵称
    void SetHeroNickName(string nickName)
    {
        UnityEngine.Transform trans = this.transform.Find("NickName");
        if (trans == null)
            return;
        UnityEngine.GameObject go = trans.gameObject;
        if (null != go)
        {
            UILabel label = go.GetComponent<UILabel>();
            if (null != label)
                label.text = nickName;
        }
    }
    //更新血条
    void UpdateHealthBar(int curValue, int maxValue)
    {
        if (maxValue <= 0 || curValue < 0)
            return;
        float value = curValue / (float)maxValue;
        if (null != hpProgressBar)
        {
            hpProgressBar.value = value;
        }
        if (null != lblHp)
        {
            value *= 100;
            if (value > 0f && value < 1f)
                value = 1f;
            StringBuilder sb = new StringBuilder();
            sb.Append((int)(curValue) + "/" + maxValue);
            lblHp.text = sb.ToString();
        }
    }
    //更新魔法值
    void UpdateMp(int curValue, int maxValue)
    {
        if (maxValue <= 0 || curValue < 0)
            return;
        float value = curValue / (float)maxValue;
        if (null != mpProgressBar)
        {
            mpProgressBar.value = value;
        }
        if (null != lblMp)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((int)(curValue) + "/" + maxValue);
            lblMp.text = sb.ToString();
        }
    }
    void CastAnimation(UnityEngine.GameObject father)
    {
        if (null == father)
            return;
        UnityEngine.GameObject goBack = null;
        UIProgressBar progressBar = null;
        UnityEngine.Transform trans = father.transform.Find("Sprite(red)");
        if (trans != null)
            goBack = trans.gameObject;
        progressBar = father.GetComponent<UIProgressBar>();

        UISprite spBack = null;
        if (null != goBack)
        {
            spBack = goBack.GetComponent<UISprite>();
        }

        if (null != spBack && null != progressBar)
        {
            if (spBack.fillAmount <= progressBar.value)
            {
                spBack.fillAmount = progressBar.value;
            }
            else
            {
                spBack.fillAmount -= RealTime.deltaTime * 0.5f;
            }
        }
    }
    public void SetUserInfo(GfxUserInfo userinfo)
    {
        m_GfxUserInfo = userinfo;
    }
    public void OnPortraitClick()
    {
        //LogicSystem.EventChannelForGfx.Publish("ge_cast_skill", "game", "SkillEx");
    }
    public void SetActive(bool active)
    {
        if (HeroView != null)
        {
            NGUITools.SetActive(HeroView, active);
        }
        if (hpProgressBar != null)
        {
            NGUITools.SetActive(hpProgressBar.gameObject, active);
        }
        if (mpProgressBar != null)
        {
            NGUITools.SetActive(mpProgressBar.gameObject, active);
        }
    }
    public void SetExitActive(bool vis)
    {
        if (btnQuit != null)
        {
            NGUITools.SetActive(btnQuit, vis);
        }
    }
    private void UpdateGfxUserinfo(GfxUserInfo gfxInfo)
    {
        if (gfxInfo != null && m_GfxUserInfo != null && gfxInfo.m_Nick == m_GfxUserInfo.m_Nick)
        {
            m_GfxUserInfo = gfxInfo;
        }
    }
    //记录怒气条的6个怒气点
    private UnityEngine.Vector3 m_OriginalPos;
    private GfxUserInfo m_GfxUserInfo = null;

}
