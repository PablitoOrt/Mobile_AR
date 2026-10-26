using TMPro;
using UnityEngine;

public class TrackingStatusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private string searchingMessage = "apunta la camara a la imagen";
    [SerializeField] private string foundMessage = "imagen detectada";
    [SerializeField] private Color searchingColor = Color.white;
    [SerializeField] private Color foundColor = new Color(0.4f, 1f, 0.4f);

    void Start()
    {
        OnLost();
    }

    public void OnFound()
    {
        label.text = foundMessage;
        label.color = foundColor;
    }

    public void OnLost()
    {
        label.text = searchingMessage;
        label.color = searchingColor;
    }
}
