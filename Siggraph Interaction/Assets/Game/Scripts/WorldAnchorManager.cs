using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAnchorManager : MonoBehaviour
{
    [SerializeField] private Transform worldRoot; // 將所有物件放在這個根物件下
    private OVRSpatialAnchor worldAnchor;
    
    async void Start()
    {
        // 確保所有物件都是worldRoot的子物件
        ReparentAllObjects();
        
        // 創建世界錨點
        await CreateWorldAnchor();
    }
    
    void ReparentAllObjects()
    {
        // 找到所有需要固定的物件
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building"); // 給你的建築物加上這個tag
        GameObject[] fountains = GameObject.FindGameObjectsWithTag("Fountain");
        
        foreach (var building in buildings)
        {
            if (building.transform.parent != worldRoot)
            {
                building.transform.SetParent(worldRoot, true);
            }
        }
        
        foreach (var fountain in fountains)
        {
            if (fountain.transform.parent != worldRoot)
            {
                fountain.transform.SetParent(worldRoot, true);
            }
        }
    }
    
    async System.Threading.Tasks.Task CreateWorldAnchor()
    {
        worldAnchor = worldRoot.gameObject.AddComponent<OVRSpatialAnchor>();
        
        while (!worldAnchor.Created)
        {
            await System.Threading.Tasks.Task.Yield();
        }
        
        bool success = await worldAnchor.SaveAnchorAsync();
        if (success)
        {
            Debug.Log("World anchor saved successfully");
        }
        else
        {
            Debug.LogError("Failed to save world anchor");
        }
    }
}