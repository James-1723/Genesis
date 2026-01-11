using UnityEngine;
using System.Collections;

public class AudioFadeController : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    
    [Header("Fade Settings")]
    [Tooltip("When to start fading (in seconds from start)")]
    public float fadeStartTime = 10f;
    
    [Tooltip("How long the fade should take (in seconds)")]
    public float fadeDuration = 2f;
    
    [Tooltip("Should the fade start automatically based on timer?")]
    public bool autoFade = true;
    
    [Tooltip("Should the audio stop playing after fade completes?")]
    public bool stopAfterFade = true;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private float timer = 0f;
    private bool hasFaded = false;
    private float originalVolume;
    private Coroutine fadeCoroutine;
    
    void Start()
    {
        // Get AudioSource if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on " + gameObject.name);
            enabled = false;
            return;
        }
        
        // Store original volume
        originalVolume = audioSource.volume;
        
        if (showDebugInfo)
            Debug.Log($"Audio will start fading at {fadeStartTime}s for {fadeDuration}s");
    }
    
    void Update()
    {
        if (!autoFade || hasFaded || audioSource == null || !audioSource.isPlaying)
            return;
        
        timer += Time.deltaTime;
        
        // Check if it's time to start fading
        if (timer >= fadeStartTime)
        {
            StartFadeOut();
        }
        
        // Debug info
        if (showDebugInfo && timer < fadeStartTime)
        {
            float timeUntilFade = fadeStartTime - timer;
            Debug.Log($"Time until fade: {timeUntilFade:F1}s");
        }
    }
    
    /// <summary>
    /// Manually trigger fade out
    /// </summary>
    public void StartFadeOut()
    {
        if (hasFaded || audioSource == null)
            return;
        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        fadeCoroutine = StartCoroutine(FadeOutCoroutine());
        hasFaded = true;
        
        if (showDebugInfo)
            Debug.Log("Starting audio fade out...");
    }
    
    /// <summary>
    /// Manually trigger fade in (to restore volume)
    /// </summary>
    public void StartFadeIn()
    {
        if (audioSource == null)
            return;
        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        fadeCoroutine = StartCoroutine(FadeInCoroutine());
        hasFaded = false;
        
        if (showDebugInfo)
            Debug.Log("Starting audio fade in...");
    }
    
    /// <summary>
    /// Reset the fade timer (useful if you want to restart the countdown)
    /// </summary>
    public void ResetTimer()
    {
        timer = 0f;
        hasFaded = false;
        
        if (showDebugInfo)
            Debug.Log("Audio fade timer reset");
    }
    
    /// <summary>
    /// Set when the fade should start (in seconds)
    /// </summary>
    public void SetFadeStartTime(float newTime)
    {
        fadeStartTime = newTime;
        
        if (showDebugInfo)
            Debug.Log($"Fade start time set to {newTime}s");
    }
    
    private IEnumerator FadeOutCoroutine()
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            
            // Smooth fade using smoothstep for more natural feel
            float smoothT = t * t * (3f - 2f * t);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, smoothT);
            
            yield return null;
        }
        
        audioSource.volume = 0f;
        
        if (stopAfterFade)
        {
            audioSource.Stop();
            if (showDebugInfo)
                Debug.Log("Audio fade complete and stopped");
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("Audio fade complete");
        }
    }
    
    private IEnumerator FadeInCoroutine()
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;
        
        // Start playing if not already playing
        if (!audioSource.isPlaying)
            audioSource.Play();
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            
            // Smooth fade using smoothstep
            float smoothT = t * t * (3f - 2f * t);
            audioSource.volume = Mathf.Lerp(startVolume, originalVolume, smoothT);
            
            yield return null;
        }
        
        audioSource.volume = originalVolume;
        
        if (showDebugInfo)
            Debug.Log("Audio fade in complete");
    }
    
    void OnDisable()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }
}