///keyle: webgl�ļ����أ��ϴ�����
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ImageLoader : MonoBehaviour
{
    public string testUrl;
    [DllImport("__Internal")]
    private static extern void LoadImageFile();

    [DllImport("__Internal")]
    private static extern void DownloadTexture(IntPtr textureData, uint dataLength, string fileName);
    public void Start()
    {
        //��indexl.html�󼴽������ز���
        DownloadTexture("GeneratedTexture.png");
    }

    //�����ť�󼴽��м����ļ�����
    public void OnloadButtonClicked()
    {
#if UNITY_EDITOR
        OnImageLoaded(testUrl);
        return;
#endif
        LoadImageFile();
    }

    public async void OnImageLoadedAsync(string url)
    {
        // ʹ�� UnityWebRequest ������ͼƬ����
        UnityWebRequest request = UnityWebRequest.Get(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        request.downloadHandler = texDl;

        // ��������
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var imageObject = GameObject.Find("ImageObject");
            var sprite = Sprite.Create(texDl.texture, new Rect(0, 0, texDl.texture.width, texDl.texture.height), new Vector2(0.5f, 0.5f));
            imageObject.GetComponent<UnityEngine.UI.Image>().sprite = sprite;
        }
        else
        {
            Debug.LogError("Error downloading image: " + request.error);
        }

        request.Dispose();
    }

    public void OnImageLoaded(string url)
    {
        OnImageLoadedAsync(url);
    }


    public void DownloadTexture(string fileName)
    {
        byte[] textureBytes = GetTextureBytes(fileName);
        DownloadTexture(Marshal.UnsafeAddrOfPinnedArrayElement(textureBytes, 0), (uint)textureBytes.Length, fileName);
    }

    public byte[] GetTextureBytes(string textureName)
    {
        var texture = new Texture2D(100, 100);

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
            }
        }

        texture.Apply();
        byte[] pngData = texture.EncodeToPNG();
        return pngData;
    }


}