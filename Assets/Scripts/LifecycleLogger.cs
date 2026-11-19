using UnityEngine;

public class LifecycleLogger : MonoBehaviour
{
    [SerializeField] string label = "Logger";
    int updateLogCount;

    void Awake()
    {
        if (label == "Logger")
            label = name;
        Debug.Log($"[{Time.frameCount}] Awake {label}", this);
    }

    void OnEnable() => Debug.Log($"[{Time.frameCount}] OnEnable {label}", this);

    void Start() => Debug.Log($"[{Time.frameCount}] Start {label}", this);

    void Update()
    {
        if (updateLogCount >= 3)
            return;
        Debug.Log($"[{Time.frameCount}] Update {label} #{updateLogCount + 1}", this);
        updateLogCount++;
    }

    void OnDisable() => Debug.Log($"[{Time.frameCount}] OnDisable {label}", this);

    void OnDestroy() => Debug.Log($"[{Time.frameCount}] OnDestroy {label}", this);
}