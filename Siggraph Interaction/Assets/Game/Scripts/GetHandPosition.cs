using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GetHandPosition : MonoBehaviour
{
    [Header("Testing UI Components")]
    public string handPositionDistanceText;

    [Header("OVR Hand")]
    public OVRHand ovrHand;
    private Vector3 currentPosition;
    private Vector3 lastPosition;
    private float timeSinceLastCalculation = 0f;
    private const float CALCULATION_INTERVAL = 0.5f; // 半秒

    private void Start()
    {
        currentPosition = Vector3.zero;
        lastPosition = Vector3.zero;
    }

    private void Update()
    {
        if (ovrHand.IsTracked)
        {
            // 每幀更新當前位置
            currentPosition = ovrHand.PointerPose.position;
            transform.position = currentPosition;
            transform.rotation = ovrHand.PointerPose.rotation;

            // 計時器
            timeSinceLastCalculation += Time.deltaTime;

            // 每半秒計算一次
            if (timeSinceLastCalculation >= CALCULATION_INTERVAL)
            {
                // 計算兩點之間的距離
                float distance = Vector3.Distance(currentPosition, lastPosition);
                
                // 計算移動方向與前方向量的夾角
                Vector3 movementDirection = (currentPosition - lastPosition).normalized;
                float angle = Vector3.Angle(Vector3.forward, movementDirection);

                // 更新顯示文字
                handPositionDistanceText = $"Movement Distance: {distance:F2}m\nAngle: {angle:F2}°";

                // 重置計時器和更新上一次位置
                timeSinceLastCalculation = 0f;
                lastPosition = currentPosition;
            }
        }
        else
        {
            handPositionDistanceText = "No Hand Position";
            currentPosition = Vector3.zero;
            lastPosition = Vector3.zero;
        }
    }

    public string ReturnHandPosition()
    {
        return handPositionDistanceText;
    }
}