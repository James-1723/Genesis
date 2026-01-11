using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // [Header("Testing UI Components")]
    // public TextMesh testingDisplayTextUI;
    // public TextMesh testingDisplayTextUI2;

    // Fortune Text / Emotion
    public Dictionary<string, string> fortuneEmotionData;
    public GetHandPosition getHandPosition;
    
    // Timer
    public float timer;
    
    void Start()
    {

    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    IEnumerator UpdateHandPositionDisplay()
    {
        while (true)
        {
            if (getHandPosition != null)
            {
                string displayText = getHandPosition.ReturnHandPosition();
                // testingDisplayTextUI2.text = displayText;
                //Debug.Log("Update Hand Position: " + displayText);
            }
            else
            {
                // testingDisplayTextUI.text = "No Hand Position";
                // testingDisplayTextUI2.text = "No Hand Position";
                //Debug.Log("No Hand Position");
            }
            yield return new WaitForSeconds(3f);
        }
    }
}
