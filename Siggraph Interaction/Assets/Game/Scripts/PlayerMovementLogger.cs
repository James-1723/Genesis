using UnityEngine;
using System.Collections.Generic;

public class PlayerMovementLogger : MonoBehaviour
{
    public Transform playerTransform;

    [Header("Custom Sampling Settings")]
    public float customDeltaTime = 0.5f;    //設為public方便自訂
    public bool recordMovement = true;      //boolean控制是否紀錄位移

    private float timer = 0f;
    private Vector3 lastPosition;

    // 平均速度Log List
    private List<Vector2> velocityLog = new List<Vector2>();
    // 時間內總移動量Log List
    private List<Vector2> deltaXZLog = new List<Vector2>();

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = this.transform;
        }

        lastPosition = playerTransform.position;
        timer = 0f;
    }

    void Update()
    {
        if (!recordMovement) return;

        timer += Time.deltaTime;

        if (timer >= customDeltaTime)
        {
            Vector3 currentPosition = playerTransform.position;

            // 計算自訂時間內的位移向量
            Vector3 delta = currentPosition - lastPosition;

            // 平均速度 = 位移 / customDeltaTime
            Vector2 velocityXZ = new Vector2(delta.x, delta.z) / timer;

            // 取delta中的 x, z 座標量
            Vector2 deltaXZ = new Vector2(delta.x, delta.z);

            // add data to the log lists
            velocityLog.Add(velocityXZ);
            deltaXZLog.Add(deltaXZ);

            //Console Log
            Debug.Log($"[Recorded] ΔTime: {timer:F3}s | Velocity XZ: {velocityXZ} | XZ Delta: {deltaXZ}");

            //Reset
            lastPosition = currentPosition;
            timer = 0f;
        }
    }

    //Methods for Accessing and Clearing the logs
    public List<Vector2> GetVelocityLog() => new List<Vector2>(velocityLog);
    public List<Vector2> GetDeltaXZ() => new List<Vector2>(deltaXZLog);
    public void ClearLog()
    {
        velocityLog.Clear();
        deltaXZLog.Clear();
    }
}