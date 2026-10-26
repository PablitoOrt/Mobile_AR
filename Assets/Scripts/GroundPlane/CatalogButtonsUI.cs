using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatalogButtonsUI : MonoBehaviour
{
    [SerializeField] private Button _buttonTemplate;
    [SerializeField] private TMP_Text _selectedLabel;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = new Color(0.4f, 1f, 0.4f);

    private readonly List<Button> _buttons = new(8);

    private void Start()
    {
        PlacementCatalogService catalog = PlacementCatalogService.Instance;
        if (catalog == null || _buttonTemplate == null) return;

        for (int i = 0; i < catalog.Catalog.Count; i++)
        {
            int index = i;
            Button button = Instantiate(_buttonTemplate, _buttonTemplate.transform.parent);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => catalog.SelectItemByIndex(index));

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = catalog.Catalog[i].ItemID;

            _buttons.Add(button);
        }

        _buttonTemplate.gameObject.SetActive(false);
        catalog.OnSelectionChanged += HandleSelectionChanged;
        HandleSelectionChanged(catalog.CurrentItem);
    }

    private void OnDestroy()
    {
        if (PlacementCatalogService.Instance != null)
            PlacementCatalogService.Instance.OnSelectionChanged -= HandleSelectionChanged;
    }

    private void HandleSelectionChanged(PlaceableItemData item)
    {
        int selected = PlacementCatalogService.Instance.CurrentIndex;
        for (int i = 0; i < _buttons.Count; i++)
            _buttons[i].image.color = i == selected ? _selectedColor : _normalColor;

        if (_selectedLabel != null && item != null)
            _selectedLabel.text = "Colocando: " + item.ItemID;
    }
}
