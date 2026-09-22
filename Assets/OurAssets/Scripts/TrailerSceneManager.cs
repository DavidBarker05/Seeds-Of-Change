using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TrailerSceneManager : MonoBehaviour
{
    public static TrailerSceneManager Instance { get; private set; }

    public const int CROP_SCENE_INDEX = 0;
    public const int MAIN_SCENE_INDEX = 1;
    public const int HOUSE_SCENE_INDEX = 2;
    public const int MARKET_SCENE_INDEX = 3;

    int m_CurrentSceneIndex = CROP_SCENE_INDEX;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (Instance != this) return;
        InputManagerScript.Instance.AddTrailerHotkeyAction(0, LoadPrevScene);
        InputManagerScript.Instance.AddTrailerHotkeyAction(1, LoadNextScene);
    }

    void OnDestroy()
    {
        if (Instance != this) return;
        InputManagerScript.Instance.RemoveTrailerHotkeyAction(0, LoadPrevScene);
        InputManagerScript.Instance.RemoveTrailerHotkeyAction(1, LoadNextScene);
    }

    int PrevSceneIndex => m_CurrentSceneIndex switch
    {
        CROP_SCENE_INDEX => throw new System.IndexOutOfRangeException("Crop Scene has no previous scene"),
        MAIN_SCENE_INDEX => CROP_SCENE_INDEX,
        HOUSE_SCENE_INDEX => MAIN_SCENE_INDEX,
        MARKET_SCENE_INDEX => HOUSE_SCENE_INDEX,
        _ => throw new System.NotImplementedException($"Unknown scene index {m_CurrentSceneIndex}")
    };

    int NextSceneIndex => m_CurrentSceneIndex switch
    {
        CROP_SCENE_INDEX => MAIN_SCENE_INDEX,
        MAIN_SCENE_INDEX => HOUSE_SCENE_INDEX,
        HOUSE_SCENE_INDEX => MARKET_SCENE_INDEX,
        MARKET_SCENE_INDEX => throw new System.IndexOutOfRangeException("Market Scene has no next scene"),
        _ => throw new System.NotImplementedException()
    };

    void LoadPrevScene(InputAction.CallbackContext ctx)
    {
        try
        {
            m_CurrentSceneIndex = PrevSceneIndex;
            SceneManager.LoadScene(m_CurrentSceneIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    void LoadNextScene(InputAction.CallbackContext ctx)
    {
        try
        {
            m_CurrentSceneIndex = NextSceneIndex;
            SceneManager.LoadScene(m_CurrentSceneIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}
