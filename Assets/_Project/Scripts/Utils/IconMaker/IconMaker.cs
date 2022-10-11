using System.IO;
using UnityEditor;
using UnityEngine;

#if (UNITY_EDITOR)
[ExecuteInEditMode]
public class IconMaker : MonoBehaviour {
    public int height = 128;
    public int width = 128;

    public bool create;
    public RenderTexture ren;
    public Camera bakeCam;
    public string spriteName;
    void Update()
    {
        if (create) {
            CreateIcon();
            create = false;
        }
    }
    public void CreateIcon() {
        if (string.IsNullOrEmpty(spriteName)) {
            spriteName = "icon";
        }
        string path = SaveLocation();
        path += spriteName;

        ren.height = height;
        ren.width = width;
        bakeCam.targetTexture = ren;
        
        RenderTexture currentRT = RenderTexture.active;
        bakeCam.targetTexture.Release();
        RenderTexture.active = bakeCam.targetTexture;
        bakeCam.Render();

        var targetTexture = bakeCam.targetTexture;
        Texture2D imgPng = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.ARGB32, false);
        imgPng.ReadPixels(new Rect(0, 0, height, width),0,0);
        imgPng.Apply();
        RenderTexture.active = currentRT;
        byte[] bytesPng = imgPng.EncodeToPNG();
        File.WriteAllBytes(path + ".png", bytesPng);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        Debug.Log(spriteName + " icon created");
    }
    string SaveLocation() {
        string saveLocation = Application.dataPath + "/_Project/Art/Textures/Sprites/Icons/";

        if (!Directory.Exists(saveLocation)) {
            Directory.CreateDirectory(saveLocation);
        }
        return saveLocation;
    }
}
#endif
