using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class SceneLoaderButton : MonoBehaviour
{
    [SerializeField]
    GameObject loadingScreen;
    [SerializeField, Min(0)]
    int nextSceneIndex = 1;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(
            async () => {
                if (loadingScreen != null) loadingScreen.SetActive(true);
                await SceneManager.LoadSceneAsync(nextSceneIndex);
            }
        );
    }
}
