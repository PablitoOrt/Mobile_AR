using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Vuforia;

public class GroundPlanePlacementActuator : MonoBehaviour
{
    [SerializeField] private bool _faceCamera = true;

    private readonly List<GameObject> _placedObjects = new(16);
    private readonly List<RaycastResult> _uiHits = new(8);

    public void PlaceOrUpdateObject(HitTestResult result)
    {
        if (result == null || IsPointerOverUI())
            return;

        if (PlacementCatalogService.Instance == null) return;

        PlaceableItemData itemData = PlacementCatalogService.Instance.CurrentItem;
        if (itemData == null || itemData.Prefab == null) return;

        Quaternion rotation = _faceCamera ? RotationFacingCamera(result.Position) : result.Rotation;
        AnchorBehaviour anchor = VuforiaBehaviour.Instance.ObserverFactory.CreateAnchorBehaviour($"Anchor_{itemData.ItemID}_{_placedObjects.Count}", result);

        GameObject instance;
        if (anchor != null)
        {
            instance = Instantiate(itemData.Prefab, anchor.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Inverse(result.Rotation) * rotation;
            _placedObjects.Add(anchor.gameObject);
        }
        else
        {
            instance = Instantiate(itemData.Prefab, result.Position, rotation);
            _placedObjects.Add(instance);
        }

        instance.transform.localScale = itemData.BaseScale;
    }

    public void UndoLast()
    {
        if (_placedObjects.Count == 0) return;

        int last = _placedObjects.Count - 1;
        if (_placedObjects[last] != null)
            Destroy(_placedObjects[last]);
        _placedObjects.RemoveAt(last);
    }

    public void ClearAll()
    {
        foreach (GameObject placed in _placedObjects)
            if (placed != null)
                Destroy(placed);
        _placedObjects.Clear();
    }

    private Quaternion RotationFacingCamera(Vector3 position)
    {
        Vector3 toCamera = Camera.main.transform.position - position;
        toCamera.y = 0f;
        return toCamera.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(toCamera) : Quaternion.identity;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null || Pointer.current == null)
            return false;

        var eventData = new PointerEventData(EventSystem.current) { position = Pointer.current.position.ReadValue() };
        _uiHits.Clear();
        EventSystem.current.RaycastAll(eventData, _uiHits);
        return _uiHits.Count > 0;
    }
}
