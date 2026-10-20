using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LoadSceneButton : MonoBehaviour
{
    [SerializeField]
    int m_SceneIndexToLoad = 0;
    [SerializeField]
    LoadingScreen m_LoadingScreen;

    void Awake() => GetComponent<Button>().onClick.AddListener(() =>
    {
        Time.timeScale = 1f;
        m_LoadingScreen.SceneIndexToLoad = m_SceneIndexToLoad;
        m_LoadingScreen.gameObject.SetActive(true);
    });
}
