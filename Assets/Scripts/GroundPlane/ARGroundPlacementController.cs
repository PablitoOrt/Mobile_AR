using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Vuforia;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlaneFinderBehaviour))]
public class ARGroundPlacementController : MonoBehaviour
{
    public enum PlaceableObjectType : byte
    {
        Cube = 0,
        Sphere = 1
    }

    [System.Serializable]
    public struct PlaceableDefinition
    {
        public PlaceableObjectType type;
        public GameObject prefab;
        public Vector3 localScale;
    }

    [Header("Referencias de Vuforia")]
    [SerializeField] private Transform anchorStage;

    [Header("Definición de Modelos (Zero Lookup)")]
    [SerializeField] private PlaceableDefinition[] placeableDefinitions;
    [SerializeField] private PlaceableObjectType defaultObjectType = PlaceableObjectType.Cube;

    // Buffer estático reutilizable para Raycasting de UI sin Garbage Collection
    private static readonly List<RaycastResult> S_UiRaycastResults = new(16);
    private static PointerEventData s_CachedPointerData;

    private PlaneFinderBehaviour _planeFinder;
    private PlaceableObjectType _currentObjectType;
    private int _placedObjectsCounter;

    public PlaceableObjectType CurrentType => _currentObjectType;

    private void Awake()
    {
        _planeFinder = GetComponent<PlaneFinderBehaviour>();
        _currentObjectType = defaultObjectType;

        if (anchorStage == null)
        {
            var anchor = Object.FindAnyObjectByType<AnchorBehaviour>();
            if (anchor != null)
            {
                anchorStage = anchor.transform;
            }
            else
            {
                Debug.LogError("[ARPlacement] No se encontró AnchorBehaviour en la escena.");
            }
        }
    }

    private void OnEnable()
    {
        if (_planeFinder != null)
        {
            _planeFinder.OnInteractiveHitTest.RemoveListener(OnVuforiaHitTestResult);
            _planeFinder.OnInteractiveHitTest.AddListener(OnVuforiaHitTestResult);
        }
    }

    private void OnDisable()
    {
        if (_planeFinder != null)
        {
            _planeFinder.OnInteractiveHitTest.RemoveListener(OnVuforiaHitTestResult);
        }
    }

    private void Update()
    {
        EvaluateTouchInput();
    }

    private void EvaluateTouchInput()
    {
        // Polling determinista sin eventos de callback
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            var touch = touchscreen.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                Vector2 touchPosition = touch.position.ReadValue();
                ProcessPointerInteraction(touchPosition);
                return;
            }
        }

        // Fallback de PC / Editor con Pointer / Mouse
        var pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            Vector2 pointerPosition = pointer.position.ReadValue();
            ProcessPointerInteraction(pointerPosition);
        }
    }

    private void ProcessPointerInteraction(Vector2 screenPosition)
    {
        if (IsPointerOverUI(screenPosition)) return;

        if (_planeFinder != null)
        {
            _planeFinder.PerformHitTest(screenPosition);
        }
    }

    private void OnVuforiaHitTestResult(HitTestResult result)
    {
        PlaceSelectedObject(result.Position, result.Rotation);
    }

    private void PlaceSelectedObject(Vector3 position, Quaternion rotation)
    {
        PlaceableDefinition definition = GetDefinition(_currentObjectType);

        if (definition.prefab == null)
        {
            Debug.LogError($"[ARPlacement] Error: Prefab no asignado para el tipo {_currentObjectType}");
            return;
        }

        Transform targetParent = anchorStage != null ? anchorStage : transform;
        GameObject spawned = Instantiate(definition.prefab, position, rotation, targetParent);

        if (definition.localScale != Vector3.zero)
        {
            spawned.transform.localScale = definition.localScale;
        }

        _placedObjectsCounter++;

        if (spawned.TryGetComponent(out Animator animator))
        {
            animator.Rebind();
            animator.Update(0f);
        }

        //VuforiaDebugger.Instance?.AddPrint($"Instanciado {_currentObjectType} #{_placedObjectsCounter}", Color.green);
    }

    public void SetObjectType(int typeIndex)
    {
        _currentObjectType = (PlaceableObjectType)typeIndex;
        //VuforiaDebugger.Instance?.AddPrint($"Selección activa: {_currentObjectType}", Color.cyan);
    }

    public void SelectCube() => SetObjectType((int)PlaceableObjectType.Cube);
    public void SelectSphere() => SetObjectType((int)PlaceableObjectType.Sphere);

    private PlaceableDefinition GetDefinition(PlaceableObjectType targetType)
    {
        if (placeableDefinitions == null) return default;

        for (int i = 0; i < placeableDefinitions.Length; i++)
        {
            if (placeableDefinitions[i].type == targetType)
            {
                return placeableDefinitions[i];
            }
        }

        return default;
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        if (s_CachedPointerData == null)
        {
            s_CachedPointerData = new PointerEventData(EventSystem.current);
        }

        s_CachedPointerData.position = screenPosition;
        S_UiRaycastResults.Clear();

        EventSystem.current.RaycastAll(s_CachedPointerData, S_UiRaycastResults);

        return S_UiRaycastResults.Count > 0;
    }
}