using UnityEngine;
using System.Collections;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using TMPro;

public class HandSwingDetector : MonoBehaviour
{
    [Header("Hand References")]
    public OVRSkeleton leftSkeleton;
    public OVRSkeleton rightSkeleton;

    [Header("Parameters")]
    [Tooltip("判定揮動必須達到的水平位移（公尺）")]
    public float swingThreshold = 0.25f;
    [Tooltip("偵測完成後的冷卻時間（秒）")]
    public float cooldown = 0.6f;

    [Tooltip("最小揮動速度（公尺/秒")]
    public float minSwingSpeed = 0.8f;

    [Header("UI 顯示")]
    public TextMeshPro swingText3D;

    private HandState _left, _right;

    private void Start()
    {
        _left = new HandState();
        _right = new HandState();

        if (swingText3D != null)
            swingText3D.text = "No Swing Detected";
    }

    private void Update()
    {
        ProcessHand(leftSkeleton, ref _left, "Left");
        ProcessHand(rightSkeleton, ref _right, "Right");
    }

    private void ProcessHand(OVRSkeleton skel, ref HandState state, string label)
    {
        if (skel == null || !skel.IsDataValid || !skel.IsDataHighConfidence) return;

        Transform wrist = skel.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform;
        Vector3 nowPos = wrist.position;
        float nowTime = Time.time;

        if (!state.initialized)
        {
            state.prevPos = nowPos;
            state.prevTime = nowTime;
            state.startPos = nowPos;
            state.initialized = true;
            return;
        }

        float deltaX = nowPos.x - state.prevPos.x;
        float deltaTime = nowTime - state.prevTime;
        float speed = Mathf.Abs(deltaX) / Mathf.Max(deltaTime, 0.0001f);

        // 若方向改變，重置偵測
        if (Mathf.Sign(deltaX) != Mathf.Sign(state.accumulatedX) && Mathf.Abs(state.accumulatedX) > 0.01f)
        {
            state.startPos = state.prevPos;
            state.accumulatedX = 0f;
        }

        state.accumulatedX += deltaX;
        state.prevPos = nowPos;
        state.prevTime = nowTime;

        if (Mathf.Abs(state.accumulatedX) >= swingThreshold &&
            speed >= minSwingSpeed &&
            nowTime - state.lastTime > cooldown)
        {
            string dir = state.accumulatedX > 0 ? "LeftToRight" : "RightToLeft";
            RaiseSwing(label, dir, state.startPos, nowPos);
            state.lastTime = nowTime;

            // 重置狀態
            state.startPos = nowPos;
            state.accumulatedX = 0f;
        }
    }

    private void RaiseSwing(string handLabel, string direction, Vector3 start, Vector3 end)
    {
        string msg = $"[{handLabel}] Swing {direction}\nStart: {start:F2}\nEnd: {end:F2}";
        Debug.Log(msg);

        if (swingText3D != null)
            swingText3D.text = msg;
    }

    private struct HandState
    {
        public bool initialized;
        public Vector3 prevPos;
        public float prevTime;
        public Vector3 startPos;
        public float accumulatedX;
        public float lastTime;
    }
}
