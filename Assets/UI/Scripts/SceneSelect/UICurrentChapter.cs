﻿using UnityEngine;
using ArkCrossEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UICurrentChapter : UnityEngine.MonoBehaviour
{

    public int m_ChapterId = -1;
    private const int C_SceneNumPerChapter = 9;//每一章的场景数
    public UIScene[] sceneArr = new UIScene[9];
    private const int C_Cols = 3;
    public UnityEngine.GameObject goScene = null;
    public const int C_width = 275;
    public const int C_height = 160;
    public const int C_offsetH = 20;
    public const int C_offsetV = 2;
    public UnityEngine.Vector3 C_OriginalPos = new UnityEngine.Vector3(-300f, 150f, 0f);
    public static bool m_UnLockNextScene = false;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            //NGUIDebug.Log(centerOnChild.centeredObject.name);
        }
        catch (System.Exception ex)
        {
            ArkCrossEngine.LogicSystem.LogicErrorLog("[Error]:Exception:{0}\n{1}", ex.Message, ex.StackTrace);
        }
    }
    public bool InitChapterById(int chapterId, int[] chapterInfo, SubSceneType subType, ref bool isNextChapterUnlock)
    {
        isNextChapterUnlock = false;
        if (chapterInfo == null) return true;
        m_ChapterId = chapterId;
        int index = 0;
        bool isLockChapter = true;//判断当前关卡是否解锁（）
        for (index = 0; index < chapterInfo.Length; ++index)
        {
            if (index < sceneArr.Length && sceneArr[index] != null)
            {
                UIScene sceneScript = sceneArr[index];
                if (sceneScript != null)
                {
                    bool isLock = true;
                    //星级
                    int grade = 0;
                    RoleInfo role_info = LobbyClient.Instance.CurrentRole;
                    if (role_info != null)
                    {
                        if (!role_info.SceneInfo.ContainsKey(chapterInfo[index]) && chapterInfo[index] != -1)
                            //判断这一章是否已经全部打完
                            isNextChapterUnlock = false;
                        //判断是否已经打过
                        if (role_info.SceneInfo.ContainsKey(chapterInfo[index]))
                        {
                            isLock = false;
                            int id_key = chapterInfo[index];
                            grade = role_info.SceneInfo[id_key];
                        }
                        else if (UICurrentChapter.m_UnLockNextScene == false)
                        {
                            //每次初始化时、应该解锁下一关
                            UICurrentChapter.m_UnLockNextScene = true;
                            isLock = false;
                        }
                    }
                    if (isLock == false)
                    {
                        isLockChapter = false;
                    }
                    if (index == chapterInfo.Length - 1 && isNextChapterUnlock)
                    {
                        if (chapterInfo[index] == -1) UICurrentChapter.m_UnLockNextScene = false;
                    }
                    sceneScript.InitScene(chapterInfo[index], isLock, subType, grade);
                }
            }
        }
        return isLockChapter;
    }
    //设置开始的场景
    public void SetStartScene(int sceneId)
    {
        foreach (UIScene scene in sceneArr)
        {
            if (scene != null && scene.GetSceneId() == sceneId)
            {
                scene.HightLight();
            }
        }
    }
    //
    public void DelTweenOnScene(int sceneId)
    {
        for (int index = 0; index < sceneArr.Length; ++index)
        {
            if (sceneArr[index] != null && sceneArr[index].GetSceneId() == sceneId)
            {
                sceneArr[index].DelTweenOnScene();
            }
        }
        /*
    foreach (UIScene scene in sceneArr) {
      if (scene != null && scene.GetSceneId() == sceneId) {
        scene.DelTweenOnScene();
      }
    }*/
    }
    //设置与关闭精英模式
    public void ShowSceneMode(SubSceneType subType)
    {
        for (int index = 0; index < sceneArr.Length; ++index)
        {
            if (sceneArr[index] != null)
            {
                //sceneArr[index].ShowSceneMode(subType);
            }
        }
    }

}
