using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] float factor = 0.5f;

    Vector3 startPosition;
    Transform cameraTransform;

    void Start()
    {
        var cam = Camera.main;
        cameraTransform = cam != null ? cam.transform : null;
        startPosition = transform.position;
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;
        var delta = cameraTransform.position;
        transform.position = startPosition + new Vector3(delta.x * factor, delta.y * factor, 0f);
    }
}