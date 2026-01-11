using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class PromptComposer : MonoBehaviour
{
    [Header("Data Sources")]
    public PlayerMovementLogger movementLogger;
    public GetHandPosition handTracker;

    [Header("UI 顯示")]
    public TextMeshPro promptText3D;

    [Header("ComfyUI")]
    string pos;
    public ComfyPromptCtr ComfyPromptCtr;
    string neg = "Text, watermark";
    private void Start()
    {
        string initialText = "No prompt generated yet.";
        
        if (promptText3D != null)
            promptText3D.text = initialText;

        StartCoroutine(GeneratePromptRoutine()); // 啟動 coroutine
    }

    private IEnumerator GeneratePromptRoutine()
    {
        while (true)
        {
            pos = GeneratePrompt(); // 每 0.5 秒呼叫一次
            ComfyPromptCtr.QueuePrompt(pos,neg);
            yield return new WaitForSeconds(2f);
        }
    }

    public string GeneratePrompt()
    {
        Vector2 velocity = GetLast(movementLogger.GetVelocityLog());
        float handDistance, handAngle;
        ParseHandData(handTracker.ReturnHandPosition(), out handDistance, out handAngle);

        string direction = GetDirectionFromVelocity(velocity);
        string speedLabel = GetSpeedLabel(velocity.magnitude);
        string motionLabel = GetMotionLabel(handDistance);

        string prompt = $"The player is {speedLabel} {direction}. Their hand is making a {motionLabel} motion at an angle of {handAngle:F0}°. Please try to generate the landscape image that fit the player's emotion.";

        Debug.Log($"[Prompt Triggered] {prompt}");

        if (promptText3D != null)
            promptText3D.text = prompt;
        return prompt;
    }

    // 取得紀錄中最後一筆速度
    private Vector2 GetLast(List<Vector2> list)
    {
        return (list != null && list.Count > 0) ? list[^1] : Vector2.zero;
    }

    // 從 GetHandPosition 傳回的文字中解析手部距離與角度
    private void ParseHandData(string text, out float distance, out float angle)
    {
        distance = 0f;
        angle = 0f;
        if (text.StartsWith("Movement"))
        {
            var parts = text.Split('\n');
            float.TryParse(parts[0].Split(':')[1].Replace("m", "").Trim(), out distance);
            float.TryParse(parts[1].Split(':')[1].Replace("°", "").Trim(), out angle);
        }
    }

    // 將 XZ 向量轉為方向敘述
    private string GetDirectionFromVelocity(Vector2 v)
    {
        if (v == Vector2.zero) return "standing still";
        float angle = Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle < 45 || angle > 315) return "moving forward";
        if (angle < 135) return "moving right";
        if (angle < 225) return "moving backward";
        return "moving left";
    }

    // 將移動速度分類為語意標籤
    private string GetSpeedLabel(float speed)
    {
        if (speed < 0.1f) return "barely moving";
        if (speed < 0.5f) return "moving slowly";
        if (speed < 1.2f) return "walking";
        return "running";
    }

    // 將手部移動距離轉為語意標籤
    private string GetMotionLabel(float distance)
    {
        if (distance < 0.05f) return "subtle";
        if (distance < 0.15f) return "clear";
        return "vigorous";
    }
}
