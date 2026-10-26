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
        SetAllObjectsActive(false);
    }

    public void OnPlaneTapped(HitTestResult result)
    {
        _hasPlacedOnce = true;
        UpdateActiveObject();
    }

    public void SelectObjectToPlace(int index)
    {
        if (index < 0 || index >= placeableObjects.Length) return;
        _currentIndex = index;

        if (_hasPlacedOnce)
            UpdateActiveObject();
    }

    private void UpdateActiveObject()
    {
        for (int i = 0; i < placeableObjects.Length; i++)
            if (placeableObjects[i] != null)
                placeableObjects[i].SetActive(i == _currentIndex);
    }

    private void SetAllObjectsActive(bool active)
    {
        for (int i = 0; i < placeableObjects.Length; i++)
            if (placeableObjects[i] != null)
                placeableObjects[i].SetActive(active);
    }
}