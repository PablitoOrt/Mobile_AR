using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Vuforia;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlaneFinderBehaviour))]
public class ARGroundPlacementManager : MonoBehaviour
{
    [System.Serializable]
    public class EntityPoolRuntime
    {
        public PlaceableEntityConfig config;
        [HideInInspector] public List<GameObject> activeInstances;
        [HideInInspector] public int circularIndex;
    }

    [Header("Referencias")]
    [SerializeField] private Transform anchorStage;
    [SerializeField] private Transform arCameraTransform;

    [Header("Catálogo Configurado")]
    [SerializeField] private EntityPoolRuntime[] entityCatalog;
    [SerializeField] private int defaultSelectedIndex = 0;

    // Buffer estático para el raycast de UI sin allocations en memoria
    private static readonly List<RaycastResult> S_UiRaycastResults = new(16);
    private static PointerEventData s_CachedPointerData;

    private PlaneFinderBehaviour _planeFinder;
    private int _currentSelectionIndex;

    private void Awake()
    {
        _planeFinder = GetComponent<PlaneFinderBehaviour>();

        if (anchorStage == null)
        {
            var anchor = Object.FindAnyObjectByType<AnchorBehaviour>();
            if (anchor != null) anchorStage = anchor.transform;
        }

        if (arCameraTransform == null && Camera.main != null)
        {
            arCameraTransform = Camera.main.transform;
        }

        // Inicializar los pools de instancias por categoría
        if (entityCatalog != null)
        {
            for (int i = 0; i < entityCatalog.Length; i++)
            {
                int capacity = Mathf.Max(1, entityCatalog[i].config.MaxAllowedInstances);
                entityCatalog[i].activeInstances = new List<GameObject>(capacity);
                entityCatalog[i].circularIndex = 0;
            }
        }

        _currentSelectionIndex = Mathf.Clamp(defaultSelectedIndex, 0, (entityCatalog?.Length ?? 1) - 1);
    }

    private void OnEnable()
    {
        if (_planeFinder != null)
        {
            _planeFinder.OnInteractiveHitTest.RemoveListener(OnVuforiaHitTest);
            _planeFinder.OnInteractiveHitTest.AddListener(OnVuforiaHitTest);
        }
    }

    private void OnDisable()
    {
        if (_planeFinder != null)
        {
            _planeFinder.OnInteractiveHitTest.RemoveListener(OnVuforiaHitTest);
        }
    }

    private void Update()
    {
        EvaluateInput();
    }

    private void EvaluateInput()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
        {
            ProcessTap(touchscreen.primaryTouch.position.ReadValue());
            return;
        }

        var pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            ProcessTap(pointer.position.ReadValue());
        }
    }

    private void ProcessTap(Vector2 screenPos)
    {
        if (IsPointerOverUI(screenPos)) return;

        if (_planeFinder != null)
        {
            _planeFinder.PerformHitTest(screenPos);
        }
    }

    private void OnVuforiaHitTest(HitTestResult result)
    {
        if (entityCatalog == null || _currentSelectionIndex >= entityCatalog.Length) return;

        EntityPoolRuntime pool = entityCatalog[_currentSelectionIndex];
        PlaceableEntityConfig cfg = pool.config;

        if (cfg == null || cfg.Prefab == null) return;

        // 1. Cálculo de Posición con corrección de elevación sobre el plano
        Vector3 finalPosition = result.Position + (result.Rotation * cfg.PositionOffset);

        // 2. Cálculo de Rotación (Corrección de FBX + Alineación con la Cámara)
        Quaternion finalRotation = result.Rotation * Quaternion.Euler(cfg.RotationOffsetEuler);

        if (cfg.AlignWithCameraYaw && arCameraTransform != null)
        {
            Vector3 cameraDirection = finalPosition - arCameraTransform.position;
            cameraDirection.y = 0f; // Anular inclinación vertical (Pitch)

            if (cameraDirection.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(cameraDirection.normalized, Vector3.up);
                finalRotation = lookRotation * Quaternion.Euler(cfg.RotationOffsetEuler);
            }
        }

        // 3. Resolución de Cuotas (1 Mesa, 2 Sillas)
        GameObject targetInstance;

        if (pool.activeInstances.Count < cfg.MaxAllowedInstances)
        {
            // Aún no alcanzamos el límite: instanciamos y registramos
            Transform parent = anchorStage != null ? anchorStage : transform;
            targetInstance = Instantiate(cfg.Prefab, finalPosition, finalRotation, parent);
            pool.activeInstances.Add(targetInstance);
        }
        else
        {
            // Límite alcanzado: reciclamos la instancia más antigua (política FIFO)
            targetInstance = pool.activeInstances[pool.circularIndex];
            targetInstance.transform.SetPositionAndRotation(finalPosition, finalRotation);
            pool.circularIndex = (pool.circularIndex + 1) % cfg.MaxAllowedInstances;
        }

        targetInstance.transform.localScale = cfg.LocalScale;
        targetInstance.SetActive(true);

        // Resetear animadores si existen en el modelo
        if (targetInstance.TryGetComponent(out Animator anim))
        {
            anim.Rebind();
            anim.Update(0f);
        }
    }

    /// <summary>
    /// Invocar desde los eventos OnClick() de la UI.
    /// </summary>
    public void SelectCatalogIndex(int index)
    {
        if (index < 0 || entityCatalog == null || index >= entityCatalog.Length) return;
        _currentSelectionIndex = index;
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