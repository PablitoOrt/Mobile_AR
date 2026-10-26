using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

public class HubMenuController : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private SceneAsset exercise1Scene;
    [SerializeField] private SceneAsset exercise2Scene;
#endif
    [SerializeField, HideInInspector] private string exercise1SceneName;
    [SerializeField, HideInInspector] private string exercise2SceneName;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject creditsCanvas;

    private void Start()
    {
        ShowMain();
    }

    public void LoadExercise1()
    {
        LoadScene(exercise1SceneName);
    }

    public void LoadExercise2()
    {
        LoadScene(exercise2SceneName);
    }

    public void ShowCredits()
    {
        mainCanvas.SetActive(false);
        creditsCanvas.SetActive(true);
    }

    public void ShowMain()
    {
        creditsCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("HubMenuController: no hay escena asignada.", this);
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        exercise1SceneName = exercise1Scene != null ? exercise1Scene.name : string.Empty;
        exercise2SceneName = exercise2Scene != null ? exercise2Scene.name : string.Empty;
        AddToBuildSettings(exercise1Scene);
        AddToBuildSettings(exercise2Scene);
    }

    private static void AddToBuildSettings(SceneAsset scene)
    {
        if (scene == null)
        {
            return;
        }

        string path = AssetDatabase.GetAssetPath(scene);
        var scenes = EditorBuildSettings.scenes.ToList();
        var existing = scenes.FirstOrDefault(s => s.path == path);
        if (existing != null)
        {
            if (existing.enabled)
            {
                return;
            }
            existing.enabled = true;
        }
        else
        {
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }
#endif
}
