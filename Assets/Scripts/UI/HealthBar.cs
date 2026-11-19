using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private IntEventSO healthChanged;
    [SerializeField] private IntEventSO maxHealthChanged;
    [SerializeField] private Image fill;

    private int current;
    private int max = 1;

    private void OnEnable()
    {
        maxHealthChanged.Register(SetMax);
        healthChanged.Register(Redraw);
    }

    private void OnDisable()
    {
        maxHealthChanged.Unregister(SetMax);
        healthChanged.Unregister(Redraw);
    }

    private void SetMax(int value)
    {
        max = Mathf.Max(1, value);
        Apply();
    }

    private void Redraw(int value)
    {
        current = value;
        Apply();
    }

    private void Apply() => fill.fillAmount = (float)current / max;
}