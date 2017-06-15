﻿using ArkCrossEngine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public enum LabelType
{
    SkillName = 0,
    Damage,
    Cd,
    EnergyCost,
    HitCount,
    CurrentLevel,
    NextLevel, L
}
public enum UISkillState
{
    Unlock,//未解锁
    CanLock,//可以解锁
    CanLevelUp,//可升级
}
public class UISkillInfo : UnityEngine.MonoBehaviour
{
    //Introduce控件下的lable
    public UILabel skillName = null;
    //升级下的label
    public UILabel lblLv0Level = null;
    public UILabel lblLv0Cd = null;
    public UILabel lblLv0Cost = null;
    public UILabel lblLv0Damage = null;
    public UILabel lblLv0HitNum = null;
    public UILabel lblLvDesc = null;

    //升级下的label
    public UILabel lblLv1Level = null;
    public UILabel lblLv1Cd = null;
    public UILabel lblLv1Cost = null;
    public UILabel lblLv1Damage = null;
    public UILabel lblLv1HitNum = null;

    public UILabel lblCurrentVigor;
    public UILabel lblLvMoney = null;
    public UILabel lblPlayerVigor;
    public UIProgressBar progressBarVigor;
    //升阶下的lable
    public UILabel lblLift1 = null;
    public UILabel lblLift2 = null;
    public UILabel lblLift3 = null;
    public UILabel lblLift4 = null;
    public UILabel lblLiftDes = null;
    public UILabel lblLiftGoods = null;

    public UISprite[] spLift = new UISprite[4];
    public UISprite[] spArrow = new UISprite[3];

    public UITexture textureLiftGoods = null;
    public UIButton btnLiftUp = null;

    //技能信息
    public UILabel lblSkillName = null;
    public UILabel lblSkillLevel = null;
    public UILabel lblSkillSection = null;
    public UILabel lblSkillDes = null;
    public UILabel lblSkillCd = null;
    public UILabel lblSkillCost = null;
    public UILabel lblSkillDamage = null;
    public UILabel lblSkillHitNum = null;

    public UILabel lblSkillDes2 = null;
    public UILabel lblSkillDes3 = null;
    public UILabel lblSkillDes4 = null;


    public UnityEngine.Color SectionColor = new UnityEngine.Color();
    public UnityEngine.Color AshColor = new UnityEngine.Color();
    public UnityEngine.Color LightColor = new UnityEngine.Color();
    //升级|解锁
    public UIButton btnLevelUp;
    public UILabel LevelUpActionText0 = null;
    public UILabel LevelUpActionText1 = null;
    public UISkillIntroduce introduce = null;

    public UnityEngine.GameObject goEffectPos1 = null;
    public UnityEngine.GameObject goEffectPos2 = null;
    public UnityEngine.GameObject goEffectPos3 = null;
    public UnityEngine.GameObject goEffectPos4 = null;

    public OnVigorChange onVigorChange;
    public delegate void OnVigorChange(int vigor);
    // Use this for initialization
    private SkillInfo m_SkillInfo = null;

    private UnityEngine.GameObject effect = null;
    private float duration = 1.0f;
    private int m_PlayerVigor = 0;
    private float m_PlayerVigorPlus = 0f;
    private const float c_SecondPerVigorPlus = 6f;//每20秒增加一点

    void Start()
    {
        try
        {
            effect = ArkCrossEngine.ResourceSystem.GetSharedResource("UI_Fx/7_FX_UI_ShengJi_01") as UnityEngine.GameObject;
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
            //精力默认每秒+1
            if (m_PlayerVigorPlus >= c_SecondPerVigorPlus)
            {
                m_PlayerVigorPlus = 0;
                SetPlayerVigor(++m_PlayerVigor);
                if (onVigorChange != null)
                {
                    onVigorChange(m_PlayerVigor);
                }
            }
            else
            {
                m_PlayerVigorPlus += UnityEngine.Time.deltaTime;
            }
        }
        catch (System.Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogicErrorLog("Exception {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    //设置技能信息界面
    public void SetSkillPanelInfo(int skillId)
    {
        SetSkillInfo(skillId);
        int skillLevel = 0;
        foreach (SkillInfo info in LobbyClient.Instance.CurrentRole.SkillInfos)
        {
            if (info.SkillId == m_SkillId)
            {
                skillLevel = info.SkillLevel;
                break;
            }
        }
        if (lblSkillLevel != null) lblSkillLevel.text = "Lv." + skillLevel;
        SkillLogicData skillCfg = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, m_SkillId) as SkillLogicData;
        if (skillCfg != null)
        {
            if (lblSkillName != null) lblSkillName.text = skillCfg.ShowName;
            if (lblSkillDes != null) lblSkillDes.text = skillCfg.ShowDescription;
            string str = "";
            StrDictionary strDic = StrDictionaryProvider.Instance.GetDataById(351);
            if (strDic != null) str = strDic.m_String;
            if (lblSkillSection != null) lblSkillSection.text = skillCfg.ShowSteps + str;
            if (lblSkillCd != null) lblSkillCd.text = FormatString(skillCfg.ShowCd + "S", LabelType.Cd);
            if (lblSkillCost != null) lblSkillCost.text = FormatString(skillCfg.ShowCostEnergy.ToString(), LabelType.EnergyCost);
            if (lblSkillHitNum != null) lblSkillHitNum.text = FormatString("3", LabelType.HitCount);
            float totalDamage = (skillCfg.ShowBaseDamage + skillCfg.DamagePerLevel * skillLevel) * 100;
            if (lblSkillDamage != null) lblSkillDamage.text = FormatString(((int)totalDamage).ToString() + "%", LabelType.Damage);
            str = str + ":";
            if (lblSkillDes2 != null)
            {
                lblSkillDes2.text = 2 + str + skillCfg.ShowSteps2Des;
                if (skillCfg.ShowSteps >= 2) lblSkillDes2.color = SectionColor;
                else
                {
                    lblSkillDes2.color = AshColor;
                }
            }
            if (lblSkillDes3 != null)
            {
                lblSkillDes3.text = 3 + str + skillCfg.ShowSteps3Des;
                if (skillCfg.ShowSteps >= 3) lblSkillDes3.color = SectionColor;
                else
                {
                    lblSkillDes3.color = AshColor;
                }
            }
            if (lblSkillDes4 != null)
            {
                lblSkillDes4.text = 4 + str + skillCfg.ShowSteps4Des;
                if (skillCfg.ShowSteps >= 4) lblSkillDes4.color = SectionColor;
                else
                {
                    lblSkillDes4.color = AshColor;
                }
            }
        }
        if (skillLevel == 0)
        {
            if (lblSkillCd != null) lblSkillCd.text = FormatString(0 + "S", LabelType.Cd);
            if (lblSkillCost != null) lblSkillCost.text = FormatString("0", LabelType.EnergyCost);
            if (lblSkillHitNum != null) lblSkillHitNum.text = FormatString("0", LabelType.HitCount);
            if (lblSkillDamage != null) lblSkillDamage.text = FormatString("0", LabelType.Damage);
        }

    }
    //设置精力值
    public void SetPlayerVigor(int currentVigor)
    {
        RoleInfo role_info = LobbyClient.Instance.CurrentRole;
        if (role_info != null)
        {
            if (role_info != null && role_info.VigorMax != 0 && progressBarVigor != null)
            {
                if (currentVigor > role_info.VigorMax)
                    currentVigor = role_info.VigorMax;
                progressBarVigor.value = currentVigor / (float)role_info.VigorMax;
                if (lblPlayerVigor != null) lblPlayerVigor.text = (int)currentVigor + "/" + role_info.VigorMax;
            }
        }
        m_PlayerVigor = currentVigor;
    }
    //设置升级界面信息
    public void SetSkillLevelUpInfo(int skillId, bool isUpgrade = false)
    {
        SetSkillInfo(skillId);
        int skillLevel = 0;
        foreach (SkillInfo info in LobbyClient.Instance.CurrentRole.SkillInfos)
        {
            if (info.SkillId == m_SkillId)
            {
                skillLevel = info.SkillLevel;
                break;
            }
        }
        string str = StrDictionaryProvider.Instance.GetDictString(352);
        str = StrDictionaryProvider.Instance.GetDictString(353);
        if (lblLv0Level != null)
            lblLv0Level.text = str + skillLevel.ToString();
        if (lblLv1Level != null) lblLv1Level.text = str + (skillLevel + 1).ToString();
        SkillLogicData skillCfg = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, m_SkillId) as SkillLogicData;
        if (skillCfg != null)
        {
            if (lblLvDesc != null) lblLvDesc.text = skillCfg.ShowDescription;
            str = StrDictionaryProvider.Instance.GetDictString(354);
            if (lblLv0Cd != null)
            {
                if (isUpgrade == true && lblLv0Cd.text != str + skillCfg.ShowCd + "S")
                {
                    PlayParticle(goEffectPos1);
                }
                lblLv0Cd.text = str + skillCfg.ShowCd + "S";
            }
            if (lblLv1Cd != null)
            {
                lblLv1Cd.text = str + skillCfg.ShowCd + "S";
            }
            str = StrDictionaryProvider.Instance.GetDictString(355);
            if (lblLv0Cost != null)
            {
                if (isUpgrade == true && lblLv0Cost.text != str + skillCfg.ShowCostEnergy.ToString())
                {
                    PlayParticle(goEffectPos3);
                }
                lblLv0Cost.text = str + skillCfg.ShowCostEnergy.ToString();
            }
            if (lblLv1Cost != null)
            {
                lblLv1Cost.text = str + skillCfg.ShowCostEnergy.ToString();
            }
            str = StrDictionaryProvider.Instance.GetDictString(356);
            if (lblLv0HitNum != null)
            {
                if (isUpgrade == true && lblLv0HitNum.text != str + skillCfg.ShowHitNum)
                {
                    PlayParticle(goEffectPos4);
                }
                lblLv0HitNum.text = str + skillCfg.ShowHitNum;
            }
            if (lblLv1HitNum != null)
            {
                lblLv1HitNum.text = str + skillCfg.ShowHitNum;
            }
            str = StrDictionaryProvider.Instance.GetDictString(357);
            float totalDamage = (skillCfg.ShowBaseDamage + skillCfg.DamagePerLevel * skillLevel) * 100;
            if (lblLv0Damage != null)
            {
                if (isUpgrade == true && lblLv0Damage.text != str + ((int)totalDamage).ToString() + "%")
                {
                    PlayParticle(goEffectPos2);
                }
                lblLv0Damage.text = str + ((int)totalDamage).ToString() + "%";
            }
            totalDamage = (skillCfg.ShowBaseDamage + skillCfg.DamagePerLevel * (skillLevel + 1)) * 100;
            if (lblLv1Damage != null)
            {
                lblLv1Damage.text = str + ((int)totalDamage).ToString() + "%";
            }

            SkillLevelupConfig levelUpCfg = SkillLevelupConfigProvider.Instance.GetDataById(skillLevel);
            if (levelUpCfg != null)
            {
                if (skillCfg.LevelUpCostType >= 1 && skillCfg.LevelUpCostType <= levelUpCfg.m_TypeList.Count && lblLvMoney != null)
                    lblLvMoney.text = levelUpCfg.m_TypeList[skillCfg.LevelUpCostType - 1].ToString();
                if (skillCfg.LevelUpVigorType >= 1 && skillCfg.LevelUpVigorType <= levelUpCfg.m_VigorList.Count && lblCurrentVigor != null)
                    lblCurrentVigor.text = levelUpCfg.m_VigorList[skillCfg.LevelUpVigorType - 1].ToString();
            }
            else
            {
                if (lblLvMoney != null) lblLvMoney.text = "0";
            }
        }
        if (skillLevel == 0)
        {
            string CHN = StrDictionaryProvider.Instance.GetDictString(354);
            if (lblLv0Cd != null) lblLv0Cd.text = CHN + 0 + "S";
            CHN = StrDictionaryProvider.Instance.GetDictString(355);
            if (lblLv0Cost != null) lblLv0Cost.text = CHN + "0";
            CHN = StrDictionaryProvider.Instance.GetDictString(356);
            if (lblLv0HitNum != null) lblLv0HitNum.text = CHN + "0";
            CHN = StrDictionaryProvider.Instance.GetDictString(357);
            if (lblLv0Damage != null) lblLv0Damage.text = CHN + 0 + "%";
        }
    }
    private void PlayParticle(UnityEngine.GameObject posGo)
    {
        if (effect != null && posGo != null)
        {
            UnityEngine.GameObject ef = CrossObjectHelper.TryCastObject<UnityEngine.GameObject>(ResourceSystem.NewObject(effect));
            if (ef != null)
            {
                ef.transform.position = posGo.transform.position;
                Destroy(ef, duration);
            }
        }
    }

    //设置升阶信息
    public void SetLiftSkillInfo(int skillId)
    {
        /*
        SetSkillInfo(skillId);
        SkillLogicData skillCfg = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, m_SkillId) as SkillLogicData;
        if (skillCfg != null) {
          UnityEngine.GameObject goAtlas = ResourceSystem.GetSharedResource(skillCfg.ShowAtlasPath) as UnityEngine.GameObject;
          if (goAtlas != null) {
            UIAtlas atlas = goAtlas.GetComponent<UIAtlas>();
            if (atlas != null) {
              for (int index = 0; index < spLift.Length; ++index) {
                if(spLift[index]!=null){
                  spLift[index].atlas = atlas; spLift[index].spriteName = skillCfg.ShowIconName;
                  if (skillCfg.ShowSteps >= index + 1) {
                    spLift[index].color = LightColor;
                  } else {
                    spLift[index].color = AshColor;
                  }
                }
                if (index < spArrow.Length && spArrow[index] != null) {
                  if (skillCfg.ShowSteps > index + 1) {
                    spArrow[index].spriteName = "sheng-ji-jian-tou1";
                  } else {
                    spArrow[index].spriteName = "sheng-ji-jian-tou2";
                  }
                }
              }
            }
          }
          if (lblLift1 != null && lblLift2 != null && lblLift3 != null && lblLift4 != null) {
            string CHN = StrDictionaryProvider.Instance.GetDictString(351);
            CHN = CHN + ":";
            lblLift1.text = 1+CHN + skillCfg.ShowDescription;
            if (skillCfg.ShowSteps >= 1) lblLift1.color = SectionColor;
            else {
              lblLift1.color = AshColor;
            }
            lblLift2.text = 2 + CHN + skillCfg.ShowSteps2Des;
            if (skillCfg.ShowSteps >= 2) lblLift2.color = SectionColor;
            else {
              lblLift2.color = AshColor;
            }
            lblLift3.text = 3 + CHN + skillCfg.ShowSteps3Des;
            if (skillCfg.ShowSteps >= 3) lblLift3.color = SectionColor;
            else {
              lblLift3.color = AshColor;
            }
            lblLift4.text = 4 + CHN + skillCfg.ShowSteps4Des;
            if (skillCfg.ShowSteps >= 4) lblLift4.color = SectionColor;
            else {
              lblLift4.color = AshColor;
            }
          }
          if (skillCfg.ShowSteps < 4) {
            if (textureLiftGoods != null) NGUITools.SetActive(textureLiftGoods.gameObject, true);
            if (lblLiftGoods != null) NGUITools.SetActive(lblLiftGoods.gameObject, true);
            if (btnLiftUp != null) NGUITools.SetActive(btnLiftUp.gameObject,false);
            string CHN = StrDictionaryProvider.Instance.GetDictString(358);
            if(lblLiftDes!=null) lblLiftDes.text = (skillCfg.ShowSteps + 1).ToString() + CHN;
            if (skillCfg.LiftCostItemList.Count > 0 && skillCfg.LiftCostItemNumList.Count > 0) {
              int itemId = skillCfg.LiftCostItemList[0];
              int itemNeedNum = skillCfg.LiftCostItemNumList[0];
              ItemConfig itemCfg = ItemConfigProvider.Instance.GetDataById(itemId);
              if (itemCfg != null) {
                RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                if (role_info != null) {
                  int item_num = 0;
                  foreach (ItemDataInfo item_info in role_info.Items) {
                    if (itemId == item_info.ItemId) {
                      item_num = item_info.ItemNum; break;
                    }
                  }
                  if (lblLiftGoods != null) lblLiftGoods.text = "x" + item_num + "/" + itemNeedNum;
                }
                string path = itemCfg.m_ItemTrueName;
                UnityEngine.Texture tex = ResourceSystem.GetSharedResource(path) as UnityEngine.Texture;
                if (textureLiftGoods != null) {
                  if (tex != null) {
                    textureLiftGoods.mainTexture = tex;
                  }
                }
              }
            }
          } else {
            string CHN = StrDictionaryProvider.Instance.GetDictString(359);
            if (lblLiftDes != null) lblLiftDes.text = CHN;
            if (btnLiftUp != null) NGUITools.SetActive(btnLiftUp.gameObject, false);
            if (textureLiftGoods != null) NGUITools.SetActive(textureLiftGoods.gameObject, false);
            if (lblLiftGoods != null) NGUITools.SetActive(lblLiftGoods.gameObject, false);
          }
        }
         */
    }

    //根据技能Id设置技能信息
    public void SetSkillInfo(int skillId)
    {
        SkillId = skillId;
        if (introduce != null) introduce.SetSkillIntroduce(skillId);
        foreach (SkillInfo info in LobbyClient.Instance.CurrentRole.SkillInfos)
        {
            if (null != info && info.SkillId != -1 && info.SkillId == SkillId)
            {
                m_SkillInfo = info;
                return;
            }
        }
        m_SkillInfo = null;
    }

    //升级、解锁按钮
    public void OnLevelUpButtonClick()
    {
        if (m_SkillInfo == null) return;
        if (!(m_SkillInfo.SkillLevel > 0))
        {
            //Debug.Log("解锁！");
            LogicSystem.PublishLogicEvent("ge_unlock_skill", "lobby", UISkillSetting.presetIndex, SkillId);
        }
        else
        {
            //Debug.Log("升级！");
            LogicSystem.PublishLogicEvent("ge_upgrade_skill", "lobby", UISkillSetting.presetIndex, SkillId, false);
        }
    }
    //设置按钮状态：解锁、未解锁
    public void SetActionState(bool isUnlock)
    {
        string chn_levelUp = StrDictionaryProvider.Instance.GetDictString(404);
        if (LevelUpActionText0 != null && LevelUpActionText1 != null)
        {
            LevelUpActionText0.text = chn_levelUp;
            LevelUpActionText1.text = chn_levelUp;
        }
        if (isUnlock && btnLevelUp != null)
        {
            //
            UISprite sp = btnLevelUp.GetComponent<UISprite>();
            if (sp != null) sp.color = LightColor;
            btnLevelUp.enabled = true;
            btnLevelUp.isEnabled = true;
        }
        else if (btnLevelUp != null)
        {
            UISprite sp = btnLevelUp.GetComponent<UISprite>();
            if (sp != null) sp.color = AshColor;
            btnLevelUp.enabled = false;
            btnLevelUp.isEnabled = false;
        }
    }
    //升阶按钮
    public void OnLiftSkillButtonClick()
    {
        //Debug.Log("升阶！");
        GfxSystem.EventChannelForLogic.Publish("ge_lift_skill", "lobby", SkillId);
    }
    //
    private string FormatString(string str, LabelType lableType)
    {
        string ret = str;
        string chn_des = "";
        switch (lableType)
        {
            case LabelType.Cd:
                chn_des = StrDictionaryProvider.Instance.GetDictString(354);
                ret = chn_des + str; break;
            case LabelType.EnergyCost:
                chn_des = StrDictionaryProvider.Instance.GetDictString(355);
                ret = chn_des + str; break;
            case LabelType.Damage:
                chn_des = StrDictionaryProvider.Instance.GetDictString(357);
                ret = chn_des + str; break;
            case LabelType.HitCount:
                chn_des = StrDictionaryProvider.Instance.GetDictString(356);
                ret = chn_des + str; break;
            case LabelType.CurrentLevel:
                chn_des = StrDictionaryProvider.Instance.GetDictString(352);
                ret = chn_des + str; break;
        }
        return ret;
    }
    public int SkillId
    {
        get { return m_SkillId; }
        set { m_SkillId = value; }
    }
    public UISkillState SkillState
    {
        get { return m_SkillState; }
        set
        {
            m_SkillState = value;
        }
    }
    private int m_SkillId = -1;
    public UISkillState m_SkillState = UISkillState.Unlock;
}
