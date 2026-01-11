using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Text;
using UnityEngine.Networking;

public class LLMGenerator : MonoBehaviour
{
    public string[] fortune;
    public string[] emotion = new string[] { "anger", "fear", "disgust", "sadness", "happiness", "shy", "envy", "anxiety", "passive" };
    private string apiKey = "";

    void Start()
    {
        StartCoroutine(GenerateFortune());
    }

    public IEnumerator GenerateFortuneWithEmotion(string emotion)
    {
        string url = "https://api.openai.com/v1/chat/completions";
        string prompt = $"請以{emotion}的情緒為主題，生成一段對應該情緒的名言佳句、詩詞、句子，長度約20-30字";

        string jsonData = @"{
            ""model"": ""gpt-3.5-turbo"",
            ""messages"": [
                { ""role"": ""user"", ""content"": """ + prompt + @""" }
            ]
        }";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                
                try
                {
                    OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>(responseText);
                    
                    if (response.choices != null && response.choices.Length > 0)
                    {
                        string generatedText = response.choices[0].message.content;
                        fortune = new string[] { emotion, generatedText };
                    }
                    else
                    {
                        Debug.LogError("API 回應中沒有找到生成的文字");
                        fortune = new string[] { emotion, "無法生成籤詩，請稍後再試" };
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"解析 JSON 時發生錯誤: {e.Message}");
                    fortune = new string[] { emotion, "無法解析回應，請稍後再試" };
                }
            }
            else
            {
                Debug.LogError("錯誤：" + request.error);
                fortune = new string[] { emotion, "無法生成籤詩，請稍後再試" };
            }
        }
    }

    public IEnumerator GenerateFortune()
    {
        var randomEmotion = emotion[UnityEngine.Random.Range(0, emotion.Length)];
        yield return StartCoroutine(GenerateFortuneWithEmotion(randomEmotion));
    }
}