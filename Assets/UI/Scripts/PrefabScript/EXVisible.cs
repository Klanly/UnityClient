﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ArkCrossEngine;

public class EXVisible : UnityEngine.MonoBehaviour
{
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
            ArkCrossEngine.LogicSystem.LogicLog("[Error]:Exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    // Use this for initialization
    void Start()
    {
        try
        {
            time = 0.0f;
            object obj = ArkCrossEngine.LogicSystem.EventChannelForGfx.Subscribe<string, bool>("ge_ex_skill", "ui", Ex);
            if (obj != null) m_EventList.Add(obj);
            obj = LogicSystem.EventChannelForGfx.Subscribe("ge_ui_unsubscribe", "ui", UnSubscribe);
            if (obj != null) m_EventList.Add(obj);
            SetPosition();
            Come(0.0f);
            NGUITools.SetActive(gameObject, false);
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
            time += RealTime.deltaTime;
            if (time < cometime)
            {
                Come(time);
            }
            if (time >= (staytime - gotime))
            {
                Go(time - (staytime - gotime));
            }
            if (time > staytime)
            {
                Come(0.0f);
                time = 0.0f;
                NGUITools.SetActive(gameObject, false);
            }
        }
        catch (System.Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogicErrorLog("Exception {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    void Ex(string hero, bool isstart)
    {
        try
        {
            if (isstart)
            {
                UnityEngine.Transform tf = gameObject.transform.Find("SpriteHero");
                if (tf != null)
                {
                    UISprite us = tf.gameObject.GetComponent<UISprite>();
                    if (us != null)
                    {
                        us.spriteName = hero;
                    }
                }
                time = 0.0f;
                NGUITools.SetActive(gameObject, true);
            }
        }
        catch (Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogicLog("[Error]:Exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    void SetPosition()
    {
        float h = Screen.height;
        //float w = Screen.width;
        UnityEngine.Transform tf = gameObject.transform.Find("SpriteBack");
        if (tf != null)
        {
            tf.position = UICamera.mainCamera.ScreenToWorldPoint(new UnityEngine.Vector3(0, h, 0));
        }
        tf = gameObject.transform.Find("SpriteHero");
        if (tf != null)
        {
            tf.position = UICamera.mainCamera.ScreenToWorldPoint(new UnityEngine.Vector3(0, h, 0));
        }
        tf = gameObject.transform.Find("SpriteLight");
        if (tf != null)
        {
            tf.position = UICamera.mainCamera.ScreenToWorldPoint(new UnityEngine.Vector3(0, h, 0));
        }
    }
    void Come(float come)
    {
        float scale = 1.0f - come / cometime;
        this.transform.localPosition = new UnityEngine.Vector3(-1688 * scale, 0, 0);
    }
    void Go(float go)
    {
        float scale = go / gotime;
        this.transform.localPosition = new UnityEngine.Vector3(-1688 * scale, 0, 0);
    }
    private float time = 0.0f;
    private float cometime = 0.1f;
    private float gotime = 0.1f;
    private float staytime = 1.2f;
}
