using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;
    public bool keepUpright;

    [Header("Scaling Settings")]
    public float maxScale = 2f;      
    public float minScale = 1f;      
    public float maxDistance = 50f;
    public bool scaleAtDistance;
    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera != null)
        {
            Vector3 lookDirection = mainCamera.transform.position - transform.position;
            if (keepUpright)
            {
                lookDirection.y = 0; 
            }
            transform.rotation = Quaternion.LookRotation(lookDirection);
            if (scaleAtDistance)
            {
                float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
                float scaleFactor = Mathf.Lerp(minScale, maxScale, distance / maxDistance);
                transform.localScale = Vector3.one * Mathf.Clamp(scaleFactor, minScale, maxScale);
            }
        }
    }
}
