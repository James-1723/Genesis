using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class ResponseData
{
    public string prompt_id;
}
public class ComfyPromptCtr : MonoBehaviour
{
    public bool generating = false;
    public TextMeshPro comfyText3D;
    private void Start()
    {
       //QueuePrompt("pretty man","watermark");
    }

    public void QueuePrompt(string positivePrompt, string negativePrompt)
    {
        if (!generating)
        {
            generating = true;
            StartCoroutine(QueuePromptCoroutine(positivePrompt, negativePrompt));

        }
        else
        {
            Debug.Log("Generating!");
        }
    }
    private IEnumerator QueuePromptCoroutine(string positivePrompt,string negativePrompt)
    {
        string url = "http://140.119.110.218:8188/prompt";
        Debug.Log("Request URL: " + url);

        int randomSeed = UnityEngine.Random.Range(0, int.MaxValue);
        string promptText = GeneratePromptJson();
        promptText = promptText.Replace("Pprompt", positivePrompt);
        promptText = promptText.Replace("Nprompt", negativePrompt);
        promptText = promptText.Replace("\"seed\": 115018773819333", $"\"seed\": {randomSeed}");
        Debug.Log(promptText);
        //UnityWebRequest request = UnityWebRequest.Get(url);
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(promptText);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request failed!");
            Debug.LogError("Error Type: " + request.result);
            Debug.LogError("HTTP Code: " + request.responseCode);
            Debug.LogError("Error Message: " + request.error);
            Debug.LogError("Server Response: " + request.downloadHandler.text);  // <<< 這個最重要
            comfyText3D.text = "request failed";
        }
        else
        {
            Debug.Log("Prompt queued successfully." + request.downloadHandler.text);
            comfyText3D.text = "Prompt queued successfully.";
            ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
            Debug.Log("Prompt ID: " + data.prompt_id);
            comfyText3D.text = "Prompt queued successfully.";


            // 建立連線並開始監聽
            GetComponent<ComfyWebsocket>().ConnectAndListen(data.prompt_id);
            // GetComponent<ComfyImageCtr>().RequestFileName(data.prompt_id);
        }
    }
    public string promptJson;

private string GeneratePromptJson()
    {
 string guid = Guid.NewGuid().ToString();

    string promptJsonWithGuid = $@"
{{
    ""id"": ""{guid}"",
    ""prompt"": {promptJson}
}}";

    return promptJsonWithGuid;
    }
}
