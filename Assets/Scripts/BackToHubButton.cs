using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
            NavigateToTarget();
    }

    public void NavigateToTarget()
    {
        if (loadMode == LoadMode.ByName)
        {
            if (string.IsNullOrEmpty(targetSceneName)) return;

            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            if (targetSceneIndex < 0 || targetSceneIndex >= SceneManager.sceneCountInBuildSettings) return;

            SceneManager.LoadScene(targetSceneIndex);
        }
    }
}