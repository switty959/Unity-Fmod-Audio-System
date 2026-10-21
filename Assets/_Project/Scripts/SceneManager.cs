using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        PlayMusicForScene(UnitySceneManager.GetActiveScene().name);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Cannot load a scene without a name.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' cannot be loaded. Add and enable it in the Build Profiles scene list.", this);
            return;
        }

        PlayMusicForScene(sceneName);
        UnitySceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        int sceneIndex = UnitySceneManager.GetActiveScene().buildIndex;
        if (sceneIndex < 0)
        {
            Debug.LogError("Cannot reload the current scene. Add and enable it in the Build Profiles scene list.", this);
            return;
        }

        PlayMusicForScene(UnitySceneManager.GetActiveScene().name);
        UnitySceneManager.LoadScene(sceneIndex);
    }

    public void LoadNextScene()
    {
        int sceneIndex = UnitySceneManager.GetActiveScene().buildIndex;
        if (sceneIndex < 0)
        {
            Debug.LogError("Cannot load the next scene because the current scene is not enabled in the Build Profiles scene list.", this);
            return;
        }

        int nextSceneIndex = sceneIndex + 1;
        if (nextSceneIndex >= UnitySceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("There is no next scene in the Build Profiles scene list.", this);
            return;
        }

        string nextScenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(nextSceneIndex);
        string nextSceneName = System.IO.Path.GetFileNameWithoutExtension(nextScenePath);
        PlayMusicForScene(nextSceneName);
        UnitySceneManager.LoadScene(nextSceneIndex);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioID musicId;
        switch (sceneName)
        {
            case "MainMenu":
                musicId = AudioID.MainMenuMusic;
                break;
            case "GamePlay":
                musicId = AudioID.GameplayMusic;
                break;
            default:
                musicId = AudioID.None;
                break;
        }

        if (musicId == AudioID.None)
        {
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("Cannot play scene music because there is no active AudioManager.", this);
            return;
        }

        AudioManager.Instance.PlayMusic(musicId);
    }
}
