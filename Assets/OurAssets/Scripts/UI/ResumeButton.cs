using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResumeButton : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(
            () => {
                GameManager.Instance.IsPaused = false;
            }
        );
    }
}
