using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class HubMenuController : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Configuración en Editor")]
    [SerializeField] private SceneAsset exercise1SceneAsset;
    [SerializeField] private SceneAsset exercise2SceneAsset;
#endif

    [Header("Identificadores de Escena")]
    [SerializeField] private string exercise1SceneName = "Exercise1";
    [SerializeField] private string exercise2SceneName = "Exercise2";

    [Header("Paneles de Interfaz")]
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject creditsCanvas;

    private void Start()
    {
        ShowMain();
    }

    public void LoadExercise1()
    {
        ExecuteSceneLoad(exercise1SceneName);
    }

    public void LoadExercise2()
    {
        ExecuteSceneLoad(exercise2SceneName);
    }

    public void ShowCredits()
    {
        if (mainCanvas != null) mainCanvas.SetActive(false);
        if (creditsCanvas != null) creditsCanvas.SetActive(true);
    }

    public void ShowMain()
    {
        if (creditsCanvas != null) creditsCanvas.SetActive(false);
        if (mainCanvas != null) mainCanvas.SetActive(true);
    }

    private void ExecuteSceneLoad(string targetScene)
    {
        if (string.IsNullOrEmpty(targetScene)) return;

        SceneManager.LoadScene(targetScene);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (exercise1SceneAsset != null)
        {
            exercise1SceneName = exercise1SceneAsset.name;
        }

        if (exercise2SceneAsset != null)
        {
            exercise2SceneName = exercise2SceneAsset.name;
        }
    }
#endif
}