using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LeaveButton : MonoBehaviour
{
    [SerializeField]
    GameObject menuToLeave;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(
            () => {
                if (GameManager.Instance != null) GameManager.Instance.IsPaused = false;
                if (menuToLeave != null) menuToLeave.SetActive(false);
            }
        );
    }
}
