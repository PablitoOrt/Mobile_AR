using UnityEngine;
using Vuforia;

public class GroundPlaneObjectSwitcher : MonoBehaviour
{
    [Header("Objetos dentro del Ground Plane Stage")]
    [SerializeField] private GameObject[] placeableObjects;

    private int _currentIndex = 0;
    private bool _hasPlacedOnce = false;

    private void Awake()
    {
        // Al arrancar, aseguramos que ninguno sea visible hasta el primer tap
        SetAllObjectsActive(false);
    }

    /// <summary>
    /// Enlazar este método al evento 'On Interactive Hit Test (HitTestResult)' del Plane Finder
    /// </summary>
    public void OnPlaneTapped(HitTestResult result)
    {
        _hasPlacedOnce = true;
        UpdateActiveObject();
    }

    /// <summary>
    /// Invocar desde los botones de la UI (ej: botón A = 0, botón B = 1)
    /// </summary>
    public void SelectObjectToPlace(int index)
    {
        if (index < 0 || index >= placeableObjects.Length) return;
        _currentIndex = index;

        // Si ya se tocó el plano al menos una vez, conmutar inmediatamente
        if (_hasPlacedOnce)
        {
            UpdateActiveObject();
        }
    }

    private void UpdateActiveObject()
    {
        for (int i = 0; i < placeableObjects.Length; i++)
        {
            if (placeableObjects[i] != null)
            {
                placeableObjects[i].SetActive(i == _currentIndex);
            }
        }
    }

    private void SetAllObjectsActive(bool active)
    {
        for (int i = 0; i < placeableObjects.Length; i++)
        {
            if (placeableObjects[i] != null)
            {
                placeableObjects[i].SetActive(active);
            }
        }
    }
}