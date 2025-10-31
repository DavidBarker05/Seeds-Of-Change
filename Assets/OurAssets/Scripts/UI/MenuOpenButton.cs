using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuOpenButton : MonoBehaviour
{
    [SerializeField]
    GameObject menuToHide;
    [SerializeField]
    GameObject menuToShow;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(
            () => {
                menuToHide.SetActive(false);
                menuToShow.SetActive(true);
            }
        );
    }
}
