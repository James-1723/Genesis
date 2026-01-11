using UnityEngine;
using System.Collections;

public class imagescaler : MonoBehaviour
{
    public Vector3 targetScale = new Vector3(1.5f, 1.5f, 1.5f);
    public float scaleDuration = 0.1f;
    public float particleDelay = 2f; // 粒子顯示時間
    [Tooltip("粒子效果距離相機的偏移距離(米)")]
    public float particleOffsetToCamera = 0.2f; // 粒子往相機方向偏移距離
    
    private Vector3 originalScale;
    private Coroutine current;
    public float timer;
    public GameObject scaleParticlePrefab;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void Update()
    {
        timer += Time.deltaTime;
        Vanish();
    }

    public void ScaleUp()
    {
        if (current != null) StopCoroutine(current);
        if (timer > 5f)
        {
            current = StartCoroutine(ScaleUpWithParticles());
            timer = 0f;
        }
    }

    public void ScaleDown()
    {
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(ScaleTo(originalScale));
    }

    private IEnumerator ScaleUpWithParticles()
    {
        // 生成粒子效果在更靠近相機的位置
        if (scaleParticlePrefab != null)
        {
            // 計算更靠近相機的位置
            Vector3 directionToCamera = (Camera.main.transform.position - transform.position).normalized;
            Vector3 particlePosition = transform.position + directionToCamera * particleOffsetToCamera;
            
            GameObject spawnedParticle = Instantiate(scaleParticlePrefab, particlePosition, Quaternion.identity);

            // 在particleDelay時間後銷毀粒子
            Destroy(spawnedParticle, 1f);
        }
        
        // 然後執行放大動畫
        yield return StartCoroutine(ScaleTo(targetScale));
    }

    private IEnumerator ScaleTo(Vector3 target)
    {
        float t = 0f;
        Vector3 start = transform.localScale;
        while (t < 1)
        {
            t += Time.deltaTime / scaleDuration;
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.localScale = target;
    }

    public void Vanish()
    {
        if (timer > 10f)
        {
            Destroy(gameObject);
        }
    }
}