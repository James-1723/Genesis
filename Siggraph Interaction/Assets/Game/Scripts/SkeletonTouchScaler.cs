using UnityEngine;

public class SkeletonTouchScaler : MonoBehaviour
{
    private imagescaler scaler;

    void Start()
    {
        scaler = GetComponent<imagescaler>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("handMesh") || other.transform.root.name.Contains("OVRHand"))
        {
            Debug.Log("Hand touched image!");
            scaler?.ScaleUp();
        }
    }
}
