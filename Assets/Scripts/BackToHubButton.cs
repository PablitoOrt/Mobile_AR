using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BackToHubButton : MonoBehaviour
{
    public enum LoadMode
    {
        ByName,
        ByIndex
    }

    [Header("Configuración de Carga")]
    [SerializeField] private LoadMode loadMode = LoadMode.ByName;
    [SerializeField] private string targetSceneName = "Hub";
    [SerializeField] private int targetSceneIndex = 0;

    [Header("Input de Escape")]
    [SerializeField] private bool listenEscapeKey = true;

    private void Update()
    {
        if (!listenEscapeKey) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            NavigateToTarget();
        }
    }

    /// <summary>
    /// Invocar desde el evento OnClick() de cualquier Button nativo de tu Canvas en escena.
    /// </summary>
    public void NavigateToTarget()
    {
        if (loadMode == LoadMode.ByName)
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning("[SceneNavigation] 'targetSceneName' está vacío.", this);
                return;
            }

            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            if (targetSceneIndex < 0 || targetSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogWarning($"[SceneNavigation] Índice {targetSceneIndex} fuera de rango en Build Settings.", this);
                return;
            }

            SceneManager.LoadScene(targetSceneIndex);
        }
    }
}