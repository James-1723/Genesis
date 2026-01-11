using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using JetBrains.Annotations;
using TMPro;

[System.Serializable]
public class ImageData
{
    public string filename;
    public string subfolder;
    public string type;
}

[System.Serializable]
public class OutputData
{
    public ImageData[] images;
}

[System.Serializable]
public class PromptData
{
    public OutputData outputs;
}


public class ComfyImageCtr: MonoBehaviour
{
    public TextMeshPro comfyText3D;
    public ComfyPromptCtr comfyPromptCtr;
    public void RequestFileName(string id){
    StartCoroutine(RequestFileNameRoutine(id));
    
}

 IEnumerator RequestFileNameRoutine(string promptID)
    {
        string url = "http://140.119.110.218:8188/history/" + promptID;
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    string imageURL = "http://140.119.110.218:8188/view?filename=" + ExtractFilename(webRequest.downloadHandler.text);
                    StartCoroutine(DownloadImage(imageURL));
                    break;
            }
            
        }
    }
    
    string ExtractFilename(string jsonString)
    {
        // Step 1: Identify the part of the string that contains the "filename" key
        string keyToLookFor = "\"filename\":";
        int startIndex = jsonString.IndexOf(keyToLookFor);

        if (startIndex == -1)
        {
            return "filename key not found";
        }

        // Adjusting startIndex to get the position right after the keyToLookFor
        startIndex += keyToLookFor.Length;

        // Step 2: Extract the substring starting from the "filename" key
        string fromFileName = jsonString.Substring(startIndex);

        // Assuming that filename value is followed by a comma (,)
        int endIndex = fromFileName.IndexOf(',');

        // Extracting the filename value (assuming it's wrapped in quotes)
        string filenameWithQuotes = fromFileName.Substring(0, endIndex).Trim();

        // Removing leading and trailing quotes from the extracted value
        string filename = filenameWithQuotes.Trim('"');
        Debug.Log(filename);
        return filename;
    }

    
     IEnumerator DownloadImage(string imageUrl)
    {
        yield return new WaitForSeconds(0.5f);
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                
                Texture2D texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;

                byte[] pngData = texture.EncodeToPNG();

                string folderPath = Application.persistentDataPath + "/SavedImages";
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }
                string fileName = "comfy_image_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                string fullPath = System.IO.Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(fullPath, pngData);
                Debug.Log("Saved image to: " + fullPath);
                comfyText3D.text = "Saved image to: "+ fullPath;
                #if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh(); // 讓 Unity 自動刷新顯示新圖片
                #endif
                comfyText3D.text = "Avaliable for next prompt";
                comfyPromptCtr.generating = false;
                
            }
            else
            {
                Debug.LogError("Image download failed: " + webRequest.error);
                comfyText3D.text = "Image download failed: " + webRequest.error;
                comfyPromptCtr.generating = false;
            }
        }
    }
}
