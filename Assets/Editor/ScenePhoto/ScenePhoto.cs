using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class ScenePhoto : EditorWindow
{

    private static UnityEngine.Vector2 m_WinMinSize = new UnityEngine.Vector2(315.0f, 400.0f);
    private static Rect m_WinPosition = new Rect(100.0f, 100.0f, 315.0f, 400.0f);
    private static string DEFAULT_PHOTO_SAVE_PATH = Path.Combine(UnityEngine.Application.dataPath, "../../../Public/Resource/Public/Scenes");
    private static string DEFAULT_PHOTO_CAMERA_ASSET_PATH = "Assets/Editor/ScenePhoto/PhotoCamera.prefab";
    public string DebugInfo;

    private UnityEngine.Vector3 SceneTopLeft;
    private UnityEngine.Vector3 SceneBottomRight;
    private UnityEngine.Vector2 PhotoSize;
    private Texture2D Photo;
    private string PhotoSavePath;

    private UnityEngine.GameObject PhotoCameraAsset;
    private UnityEngine.GameObject PhotoCameraObj;
    private UnityEngine.Camera PhotoCamera;

    private bool IsAdvancedConfig = false;
    private bool IsHDR = true;

    [MenuItem("Custom/ScenePhoto")]
    private static void Init()
    {
        ScenePhoto window = EditorWindow.GetWindow<ScenePhoto>("ScenePhoto", true, typeof(EditorWindow));
        window.position = m_WinPosition;
        window.minSize = m_WinMinSize;
        window.wantsMouseMove = true;
        window.Show();

        window.Initialize();
    }
    private void Initialize()
    {
        SceneTopLeft = UnityEngine.Vector3.zero;
        SceneBottomRight = new UnityEngine.Vector3(96, 0.0f, 96);
        PhotoSize = new UnityEngine.Vector2(1536.0f, 1536.0f);
        PhotoSavePath = DEFAULT_PHOTO_SAVE_PATH + "/Scene.jpg";
        PhotoCamera = null;
        if (PhotoCameraObj != null)
        {
            UnityEngine.GameObject.DestroyImmediate(PhotoCameraObj);
        }
        if (PhotoCameraAsset != null)
        {
            PhotoCameraAsset = null;
        }
        if (Photo != null)
        {
            UnityEngine.GameObject.DestroyImmediate(Photo);
            Photo = null;
        }
        while (true)
        {
            UnityEngine.GameObject remainObj = UnityEngine.GameObject.Find("PhotoCamera");
            if (remainObj != null)
            {
                UnityEngine.GameObject.DestroyImmediate(remainObj);
            }
            else
            {
                break;
            }
        }
    }
    private void OnDestroy()
    {
        Initialize();
    }
    private void OnGUI()
    {
        SceneTopLeft = EditorGUILayout.Vector3Field("SceneTopLeft", SceneTopLeft);
        SceneBottomRight = EditorGUILayout.Vector3Field("SceneBottomRight", SceneBottomRight);
        PhotoSize = EditorGUILayout.Vector2Field("PhotoSize", PhotoSize);

        EditorGUILayout.BeginHorizontal();
        PhotoSavePath = EditorGUILayout.TextField("SerizlizeFile:", PhotoSavePath);
        if (GUILayout.Button("Select", GUILayout.MaxWidth(50)))
        {
            PhotoSavePath = EditorUtility.SaveFilePanel(
              "Select Path to save photo",
              DEFAULT_PHOTO_SAVE_PATH,
              "Scene",
              "png");
        }
        EditorGUILayout.EndHorizontal();
        IsAdvancedConfig = EditorGUILayout.Foldout(IsAdvancedConfig, "AdvancedConfig");
        if (IsAdvancedConfig)
        {
            IsHDR = EditorGUILayout.Toggle("IsHDR", IsHDR);
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Take Photo", GUILayout.MaxWidth(80)))
        {
            TakePhoto();
        }
        EditorGUILayout.EndHorizontal();
        Rect winRect = this.position;
        float textureFildSize = UnityEngine.Mathf.Min(winRect.width, winRect.height);
        Photo = EditorGUILayout.ObjectField(
          Photo,
          typeof(UnityEngine.Texture),
          false,
          GUILayout.MaxWidth(textureFildSize),
          GUILayout.MaxHeight(textureFildSize)
          ) as Texture2D;
        this.Repaint();
    }
    private void TakePhoto()
    {
        List<UnityEngine.GameObject> airWallMeshes = new List<UnityEngine.GameObject>();
        try
        {
            UnityEngine.GameObject airWallContainer = UnityEngine.GameObject.Find("EventObj/StaticAirWall");
            if (null != airWallContainer)
            {
                int count = airWallContainer.transform.childCount;
                for (int airIndex = 0; airIndex < count; ++airIndex)
                {
                    UnityEngine.Transform airWall = airWallContainer.transform.GetChild(airIndex);
                    if (null != airWall)
                    {
                        UnityEngine.GameObject child = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
                        if (null != child)
                        {
                            child.transform.parent = airWall;
                            child.transform.localPosition = UnityEngine.Vector3.zero;
                            child.transform.localRotation = UnityEngine.Quaternion.identity;
                            child.transform.localScale = UnityEngine.Vector3.one;
                            child.SetActive(true);
                            child.GetComponent<Renderer>().sharedMaterial.shader = UnityEngine.Shader.Find("Self-Illumin/Specular");
                            child.GetComponent<Renderer>().sharedMaterial.color = UnityEngine.Color.yellow;
                            airWallMeshes.Add(child);
                        }
                    }
                }
            }
            Debug.Log("AirWall Count:" + airWallMeshes.Count);
            if (PhotoCameraAsset == null)
            {
                PhotoCameraAsset = AssetDatabase.LoadAssetAtPath(DEFAULT_PHOTO_CAMERA_ASSET_PATH, typeof(UnityEngine.GameObject)) as UnityEngine.GameObject;
            }
            if (PhotoCameraObj == null && PhotoCameraAsset != null)
            {
                PhotoCameraObj = UnityEngine.GameObject.Instantiate(PhotoCameraAsset) as UnityEngine.GameObject;
                PhotoCamera = PhotoCameraObj.GetComponent<UnityEngine.Camera>();
            }

            if (Photo != null)
            {
                UnityEngine.GameObject.DestroyImmediate(Photo);
                Photo = null;
            }

            if (PhotoCamera == null)
            {
                EditorUtility.DisplayDialog(
                  "Error",
                  "Photo UnityEngine.Camera Miss! Are you miss PhotoCamera.prefab?",
                  "OK");
                return;
            }
            UnityEngine.Vector3 tSceneSize = SceneBottomRight - SceneTopLeft;
            UnityEngine.Vector3 tSceneCenter = (SceneTopLeft + SceneBottomRight) / 2;

            PhotoCamera.orthographic = true;
            PhotoCamera.aspect = 1.0f;
            PhotoCamera.allowHDR = IsHDR;
            PhotoCamera.orthographicSize = UnityEngine.Mathf.Max(tSceneSize.x, tSceneSize.y) / 2;
            //PhotoCamera.rect = new Rect(0, 0, tSceneSize.x, tSceneSize.z);
            PhotoCamera.transform.position = tSceneCenter + new UnityEngine.Vector3(0, 1000.0f, 0);
            PhotoCamera.transform.LookAt(tSceneCenter);

            RenderTexture currentActiveRT = RenderTexture.active;

            RenderTexture tCameraRT = new RenderTexture((int)PhotoSize.x, (int)PhotoSize.y, 24);
            PhotoCamera.targetTexture = tCameraRT;
            RenderTexture.active = tCameraRT;
            PhotoCamera.Render();

            Photo = new Texture2D((int)PhotoSize.x, (int)PhotoSize.y, TextureFormat.RGB24, false);
            Photo.ReadPixels(new Rect(0, 0, (int)PhotoSize.x, (int)PhotoSize.y), 0, 0);
            Photo.Apply();

            RenderTexture.active = null;
            PhotoCamera.targetTexture = null;
            RenderTexture.active = currentActiveRT;

            byte[] bytes;
            bytes = Photo.EncodeToPNG();
            System.IO.File.WriteAllBytes(GetPhotoName(), bytes);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log("ScenePhoto.TackPhoto failed.ex:" + ex.Message);
        }
        finally
        {
            foreach (UnityEngine.GameObject child in airWallMeshes)
            {
                child.transform.parent = null;
                child.SetActive(false);
                UnityEngine.GameObject.DestroyImmediate(child);
            }
        }
    }
    private string GetPhotoName()
    {
        return PhotoSavePath;
        /*
        string tPhotoTimeFormat = "yyyyMMddHHmmss";
        return string.Format("{0}/{1}_{2}.png",
          PhotoSavePath,
          "Photo",
          DateTime.Now.ToString(tPhotoTimeFormat));
        */
    }
}