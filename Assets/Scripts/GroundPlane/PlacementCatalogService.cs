using System;
using UnityEngine;

public class PlacementCatalogService : MonoBehaviour
{
    public static PlacementCatalogService Instance { get; private set; }

    [SerializeField] private PlaceableItemData[] _catalog;

    public PlaceableItemData CurrentItem { get; private set; }
    public event Action<PlaceableItemData> OnSelectionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_catalog != null && _catalog.Length > 0)
        {
            CurrentItem = _catalog[0];
        }
    }

    public void SelectItemByIndex(int index)
    {
        if (index < 0 || _catalog == null || index >= _catalog.Length) return;

        CurrentItem = _catalog[index];
        OnSelectionChanged?.Invoke(CurrentItem);
    }
}