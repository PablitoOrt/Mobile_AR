using UnityEngine;

[CreateAssetMenu(fileName = "NewPlaceableEntity", menuName = "AR/Placeable Entity Config")]
public class PlaceableEntityConfig : ScriptableObject
{
    [field: SerializeField] public string EntityID { get; private set; }
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public int MaxAllowedInstances { get; private set; } = 1;
    [field: SerializeField] public Vector3 LocalScale { get; private set; } = new Vector3(0.2f, 0.2f, 0.2f);

    [Header("Corrección Geométrica de Malla")]
    [Tooltip("Desfase vertical para compensar pivotes en el centro del modelo. Ej: (0, 0.15, 0)")]
    [field: SerializeField] public Vector3 PositionOffset { get; private set; } = Vector3.zero;

    [Tooltip("Corrección de rotación fija para modelos exportados con ejes desalineados.")]
    [field: SerializeField] public Vector3 RotationOffsetEuler { get; private set; } = Vector3.zero;

    [Tooltip("Si es verdadero, orienta la cara frontal del objeto hacia la ARCamera al spawnear.")]
    [field: SerializeField] public bool AlignWithCameraYaw { get; private set; } = true;
}