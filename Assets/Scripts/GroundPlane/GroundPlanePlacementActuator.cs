using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class GroundPlanePlacementActuator : MonoBehaviour
{
    [SerializeField] private PlaneFinderBehaviour _planeFinder;
    [SerializeField] private Transform _anchorStage;

    private readonly Dictionary<string, GameObject> _instancesPool = new(8);
    private GameObject _currentActiveObject;

    private void Awake()
    {
        if (_planeFinder == null)
        {
            _planeFinder = Object.FindAnyObjectByType<PlaneFinderBehaviour>();
            if (_planeFinder != null)
            {
                Debug.Log("[GroundPlaneActuator] PlaneFinderBehaviour auto-enlazado con éxito.");
            }
            else
            {
                Debug.LogError("[GroundPlaneActuator] CRÍTICO: No se encontró 'PlaneFinderBehaviour' en la escena.");
            }
        }

        if (_anchorStage == null)
        {
            var anchor = Object.FindAnyObjectByType<AnchorBehaviour>();
            if (anchor != null)
            {
                _anchorStage = anchor.transform;
                Debug.Log($"[GroundPlaneActuator] AnchorStage auto-enlazado a: {anchor.name}");
            }
            else
            {
                Debug.LogError("[GroundPlaneActuator] CRÍTICO: No se encontró 'AnchorBehaviour' (Ground Plane Stage) en la escena.");
            }
        }
    }

    /// <summary>
    /// Enlazar este método en el Inspector a 'On Interactive Hit Test (HitTestResult)' del PlaneFinder
    /// </summary>
    public void PlaceOrUpdateObject(HitTestResult result)
    {
        Debug.Log($"<color=cyan>[GroundPlaneActuator]</color> Tap recibido. Pose detectada en posición: {result.Position}");

        if (PlacementCatalogService.Instance == null)
        {
            Debug.LogError("[GroundPlaneActuator] Error: No existe instancia activa de 'PlacementCatalogService' en la escena.");
            return;
        }

        PlaceableItemData itemData = PlacementCatalogService.Instance.CurrentItem;
        if (itemData == null)
        {
            Debug.LogError("[GroundPlaneActuator] Error: 'CurrentItem' en PlacementCatalogService es nulo. Revisa la lista del catálogo.");
            return;
        }

        if (itemData.Prefab == null)
        {
            Debug.LogError($"[GroundPlaneActuator] Error: El ítem '{itemData.ItemID}' no tiene Prefab asignado en su ScriptableObject.");
            return;
        }

        // Ocultar objeto previo
        if (_currentActiveObject != null)
        {
            _currentActiveObject.SetActive(false);
        }

        // Recuperar del pool o instanciar
        if (!_instancesPool.TryGetValue(itemData.ItemID, out GameObject instance) || instance == null)
        {
            Transform parent = _anchorStage != null ? _anchorStage : transform;
            instance = Instantiate(itemData.Prefab, parent);
            _instancesPool[itemData.ItemID] = instance;
            Debug.Log($"<color=yellow>[GroundPlaneActuator]</color> Instanciado nuevo prefab: '{itemData.ItemID}' bajo '{parent.name}'.");
        }
        else
        {
            Debug.Log($"<color=green>[GroundPlaneActuator]</color> Reutilizando instancia cacheada de: '{itemData.ItemID}'.");
        }

        _currentActiveObject = instance;
        _currentActiveObject.transform.SetPositionAndRotation(result.Position, result.Rotation);
        _currentActiveObject.transform.localScale = itemData.BaseScale;
        _currentActiveObject.SetActive(true);

        if (_currentActiveObject.TryGetComponent(out Animator animator))
        {
            animator.Rebind();
            animator.Update(0f);
        }

        Debug.Log($"<color=green>[GroundPlaneActuator] ÉXITO:</color> Objeto '{itemData.ItemID}' posicionado en {result.Position}");
    }

    /// <summary>
    /// Opcional: enlazar a 'On Automatic Hit Test' solo para verificar si Vuforia ve el piso
    /// </summary>
    public void DebugFloorTracking(HitTestResult result)
    {
        // Si entra acá, Vuforia confirmó que está viendo una superficie plana válida
    }
}