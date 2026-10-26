using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Vuforia;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlaneFinderBehaviour))]
public class ARGroundPlacementController : MonoBehaviour
{
    [System.Serializable]
    public struct PlaceableDefinition
    {
        public string displayName;
        public GameObject prefab;
        public Vector3 baseScale;
    }

    [Header("Referencias de Vuforia")]
    [SerializeField] private Transform anchorStage;

    [Header("Catálogo de Modelos")]
    [SerializeField] private PlaceableDefinition[] placeableDefinitions;
    [SerializeField] private int initialIndex = 0;

    // Buffer estático para el raycast de UI sin generar Garbage Collection
    private static readonly List<RaycastResult> S_UiRaycastResults = new(16);
    private static PointerEventData s_CachedPointerData;

    private PlaneFinderBehaviour _planeFinder;
    private GameObject[] _instantiatedPool;
    private int _currentIndex = -1;
    private GameObject _currentActiveInstance;
    private Pose _lastPlacedPose;
    private bool _hasPlacedAtLeastOnce;

    private void Awake()
    {
        _planeFinder = GetComponent<PlaneFinderBehaviour>();

        if (anchorStage == null)
        {
            var anchor = Object.FindAnyObjectByType<AnchorBehaviour>();
            if (anchor != null) anchorStage = anchor.transform;
        }

        if (placeableDefinitions != null && placeableDefinitions.Length > 0)
        {
            _instantiatedPool = new GameObject[placeableDefinitions.Length];
            _currentIndex = Mathf.Clamp(initialIndex, 0, placeableDefinitions.Length - 1);
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
        EvaluateInput();
    }

    private void EvaluateInput()
    {
        // Touchscreen primario en móvil
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            var touch = touchscreen.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                ProcessTap(touch.position.ReadValue());
                return;
            }
        }

        // Pointer fallback para pruebas en Editor
        var pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            ProcessTap(pointer.position.ReadValue());
        }
    }

    private void ProcessTap(Vector2 screenPosition)
    {
        if (IsPointerOverUI(screenPosition)) return;

        if (_planeFinder != null)
        {
            _planeFinder.PerformHitTest(screenPosition);
        }
    }

    private void OnVuforiaHitTestResult(HitTestResult result)
    {
        _lastPlacedPose = new Pose(result.Position, result.Rotation);
        _hasPlacedAtLeastOnce = true;
        ApplyPlacement(_lastPlacedPose);
    }

    private void ApplyPlacement(Pose targetPose)
    {
        if (_currentIndex < 0 || _currentIndex >= placeableDefinitions.Length) return;

        PlaceableDefinition def = placeableDefinitions[_currentIndex];
        if (def.prefab == null) return;

        // Ocultar la instancia activa previa
        if (_currentActiveInstance != null)
        {
            _currentActiveInstance.SetActive(false);
        }

        // Instanciar solo si no existe en el pool
        if (_instantiatedPool[_currentIndex] == null)
        {
            Transform parent = anchorStage != null ? anchorStage : transform;
            _instantiatedPool[_currentIndex] = Instantiate(def.prefab, targetPose.position, targetPose.rotation, parent);
        }

        GameObject targetObj = _instantiatedPool[_currentIndex];
        targetObj.transform.SetPositionAndRotation(targetPose.position, targetPose.rotation);
        targetObj.transform.localScale = def.baseScale != Vector3.zero ? def.baseScale : Vector3.one;
        targetObj.SetActive(true);

        _currentActiveInstance = targetObj;

        // Reinicio del árbol de animación para mallas esqueléticas
        if (_currentActiveInstance.TryGetComponent(out Animator anim))
        {
            anim.Rebind();
            anim.Update(0f);
        }
        else
        {
            // Búsqueda en hijos si el Animator está en el sub-nodo de la malla
            var childAnim = _currentActiveInstance.GetComponentInChildren<Animator>();
            if (childAnim != null)
            {
                childAnim.Rebind();
                childAnim.Update(0f);
            }
        }
    }

    /// <summary>
    /// Invocar desde los botones de la UI (ej. OnClick -> SetModelIndex(0), SetModelIndex(1))
    /// </summary>
    public void SetModelIndex(int newIndex)
    {
        if (newIndex < 0 || newIndex >= placeableDefinitions.Length) return;
        if (newIndex == _currentIndex) return;

        _currentIndex = newIndex;

        // Si ya hay un anclaje colocado en el suelo, permutar el modelo inmediatamente en la misma posición
        if (_hasPlacedAtLeastOnce)
        {
            ApplyPlacement(_lastPlacedPose);
        }
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