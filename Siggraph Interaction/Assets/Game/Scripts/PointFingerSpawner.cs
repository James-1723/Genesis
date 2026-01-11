using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using System.Linq;
using Oculus.Interaction.PoseDetection;
using TMPro;
using System.Collections;
using System;
using System.IO;
using UnityEngine.UI;

public class PointFingerSpawner : MonoBehaviour
{
    public OVRSkeleton skeleton;
    public ShapeRecognizerActiveState pointGestureActiveState;
    public GameObject prefabToSpawn;
    public GameObject pointParticlePrefab;
    public float minDistanceFromHead = 0.4f;
    public float spawnCooldownTime = 1f;
    public float spawnInterval = 2f;
    public TextMeshPro promptText3D;
    
    [Header("Particle Settings")]
    [Tooltip("粒子效果距離相機的偏移距離(米)")]
    public float particleOffsetToCamera = 0.15f; // 粒子往相機方向偏移距離
    

    private float lastSpawnTime = -999f;
    private float elapsedTime = 0f;
    private bool hasSpawned = false;
    
    [SerializeField] 
    private GameManager gameManager;

    private void Start()
    {
        if (promptText3D != null)
        {
            promptText3D.text = "Waiting for skeleton data...";
        }
        StartCoroutine(WaitForSkeletonReady());
    }

    private IEnumerator WaitForSkeletonReady()
    {
        while (skeleton == null || !skeleton.IsDataValid || skeleton.Bones == null || skeleton.Bones.Count == 0)
        {
            Debug.Log("Waiting for skeleton data...");
            if (promptText3D != null)
                promptText3D.text = "Waiting for skeleton data...";
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Skeleton ready. Starting gesture detection!");
        if (promptText3D != null)
            promptText3D.text = "Skeleton ready. Starting gesture detection!";
    }
    
    private void Update()
    {
        // Debug.LogWarning("timer: " + gameManager.timer);
        if (skeleton == null || !skeleton.IsDataValid || skeleton.Bones == null || skeleton.Bones.Count == 0)
            return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime < spawnCooldownTime) return;
        if (Time.time - lastSpawnTime < spawnInterval) return;

        Transform tip = skeleton.Bones
            .FirstOrDefault(b => b.Id == OVRSkeleton.BoneId.Hand_IndexTip)?.Transform;

        if (tip == null) return;

        float distance = Vector3.Distance(tip.position, Camera.main.transform.position);
        bool gestureActive = pointGestureActiveState != null && pointGestureActiveState.Active;
        bool distanceOK = distance > minDistanceFromHead;
        bool cooldownOK = Time.time - lastSpawnTime >= spawnInterval;

        string debugStatus =
            $"[DEBUG CHECK]\n" +
            $"- Gesture Active: {(gestureActive ? "True" : "False")}\n" +
            $"- Distance OK: {(distanceOK ? $"{distance:F2}m" : $"{distance:F2}m")}\n" +
            $"- Cooldown OK: {(cooldownOK ? "ok" : $"{Time.time - lastSpawnTime:F2}s")}\n" +
            $"- hasSpawned: {(hasSpawned ? "True" : "False")}";

        Debug.Log(debugStatus);
        if (promptText3D != null) promptText3D.text = debugStatus;

        if (gestureActive && distanceOK && !hasSpawned && gameManager.timer > 10f)
        {
            // 生成主要物件
            GameObject spawned = Instantiate(prefabToSpawn, tip.position, Quaternion.identity);
            
            // 生成粒子效果在更靠近相機的位置並設定1秒後銷毀
            if (pointParticlePrefab != null)
            {
                // 計算粒子位置：從手指位置往相機方向偏移
                Vector3 directionToCamera = (Camera.main.transform.position - tip.position).normalized;
                Vector3 particlePosition = tip.position + directionToCamera * particleOffsetToCamera;
                
                GameObject spawnedParticle = Instantiate(pointParticlePrefab, particlePosition, Quaternion.identity);
                
                // 讓粒子朝向相機
                Vector3 lookDirection = Camera.main.transform.position - spawnedParticle.transform.position;
                if (lookDirection != Vector3.zero)
                {
                    spawnedParticle.transform.rotation = Quaternion.LookRotation(lookDirection);
                    
                    // 如果粒子prefab需要特定旋轉調整，可以取消註解並調整角度
                    // spawnedParticle.transform.Rotate(0, 180, 0);
                }
                
                Destroy(spawnedParticle, 1f); // 1秒後自動銷毀
            }
            
            // 載入圖片邏輯
            string folderPath = Application.persistentDataPath + "/SavedImages";
            if (Directory.Exists(folderPath))
            {
                string newestFile = Directory.GetFiles(folderPath, "*.png")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(newestFile))
                {
                    try
                    {
                        byte[] imgBytes = File.ReadAllBytes(newestFile);
                        Texture2D tex = new Texture2D(2, 2);
                        tex.LoadImage(imgBytes);

                        RawImage img = spawned.GetComponentInChildren<RawImage>();
                        if (img != null)
                        {
                            img.texture = tex;
                        }
                        else
                        {
                            Debug.LogWarning("RawImage component not found in prefab.");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error loading image: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogWarning("No image files found in SavedImages.");
                }
            }
            else
            {
                Debug.LogWarning("SavedImages folder not found: " + folderPath);
            }
            
            spawned.transform.LookAt(Camera.main.transform);
            spawned.transform.Rotate(0, 180, 0);
            lastSpawnTime = Time.time;
            hasSpawned = true;

            string successText =
                $"[SPAWN TRIGGERED]\n" +
                $"- Time: {Time.time:F1}s\n" +
                $"- Tip Pos: {tip.position}";

            Debug.Log(successText);
            if (promptText3D != null)
                promptText3D.text = successText;
        }

        if (distance < minDistanceFromHead * 0.8f)
        {
            hasSpawned = false;
        }
    }
}