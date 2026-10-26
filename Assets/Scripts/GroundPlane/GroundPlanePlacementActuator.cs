using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class GroundPlanePlacementActuator : MonoBehaviour
{
    [SerializeField] private Transform _anchorStage;

    // Cacheo de instancias por ItemID para evitar Instantiate/Destroy en runtime
    private readonly Dictionary<string, GameObject> _instancesPool = new(8);
    private GameObject _currentActiveObject;

    /// <summary>
    /// Invocar exclusivamente desde el evento Unity OnInteractiveHitTest(HitTestResult) del PlaneFinder
    /// </summary>
    public void PlaceOrUpdateObject(HitTestResult result)
    {
        if (PlacementCatalogService.Instance == null) return;

        PlaceableItemData itemData = PlacementCatalogService.Instance.CurrentItem;
        if (itemData == null || itemData.Prefab == null) return;

        // Desactivar el objeto visible previo
        if (_currentActiveObject != null)
        {
            _currentActiveObject.SetActive(false);
        }

        // Obtener del caché o instanciar por única vez
        if (!_instancesPool.TryGetValue(itemData.ItemID, out GameObject instance))
        {
            Transform parent = _anchorStage != null ? _anchorStage : transform;
            instance = Instantiate(itemData.Prefab, parent);
            _instancesPool.Add(itemData.ItemID, instance);
        }

        _currentActiveObject = instance;
        _currentActiveObject.transform.SetPositionAndRotation(result.Position, result.Rotation);
        _currentActiveObject.transform.localScale = itemData.BaseScale;
        _currentActiveObject.SetActive(true);

        // Disparo seguro de animaciones si el prefab cuenta con Animator
        if (_currentActiveObject.TryGetComponent(out Animator animator))
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
}