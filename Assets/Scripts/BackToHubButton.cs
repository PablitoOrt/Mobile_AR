using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEditor;

public class BackToHubButton : MonoBehaviour
{
    [SerializeField] private SceneAsset hubScene;
    [SerializeField, HideInInspector] private string hubSceneName = "Hub";
    [SerializeField] private string buttonText = "< Hub";
    [SerializeField] private Vector2 buttonSize = new Vector2(220f, 90f);
    [SerializeField] private Vector2 margin = new Vector2(30f, 30f);

    private void Start()
    {
        CreateButton();

        if (EventSystem.current == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            LoadHub();
    }

    public void LoadHub()
    {
        if (string.IsNullOrEmpty(hubSceneName))
        {
            Debug.LogWarning("BackToHubButton: no hay escena del Hub asignada.", this);
            return;
        }
        SceneManager.LoadScene(hubSceneName);
    }

    private void CreateButton()
    {
        var canvasObject = new GameObject("BackToHubCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        var buttonObject = new GameObject("BackToHubButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvasObject.transform, false);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(margin.x, -margin.y);
        rect.sizeDelta = buttonSize;
        buttonObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
        buttonObject.GetComponent<Button>().onClick.AddListener(LoadHub);

        var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        var label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = buttonText;
        label.fontSize = 40f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
    }
    private void OnValidate()
    {
        if (hubScene != null)
            hubSceneName = hubScene.name;
    }
}
