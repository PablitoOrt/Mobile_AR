using UnityEngine;

[CreateAssetMenu(fileName = "NewPlaceableItem", menuName = "AR/Placeable Item Data")]
public class PlaceableItemData : ScriptableObject
{
    [field: SerializeField] public string ItemID { get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public Vector3 BaseScale { get; private set; } = Vector3.one;
}