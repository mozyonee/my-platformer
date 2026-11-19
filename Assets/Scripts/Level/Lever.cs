using UnityEngine;
using UnityEngine.Tilemaps;

public class Lever : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject target;
    [SerializeField] Sprite offSprite;
    [SerializeField] Sprite onSprite;
    [SerializeField] Tilemap hazardMap;
    [SerializeField] TileBase hazardTile;
    [SerializeField] Vector2Int hazardRangeX;
    [SerializeField] int hazardY;

    SpriteRenderer spriteRenderer;
    bool isOn;

    void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

    public void Interact()
    {
        isOn = !isOn;
        if (target != null)
            target.SetActive(isOn);
        ApplyHazardCover(isOn);
        if (spriteRenderer == null)
            return;
        if (onSprite != null && offSprite != null)
            spriteRenderer.sprite = isOn ? onSprite : offSprite;
        else
            spriteRenderer.flipX = isOn;
    }

    void ApplyHazardCover(bool covered)
    {
        if (hazardMap == null)
            return;
        for (var x = hazardRangeX.x; x <= hazardRangeX.y; x++)
        {
            var cell = new Vector3Int(x, hazardY, 0);
            hazardMap.SetTile(cell, covered ? null : hazardTile);
        }
    }
}
