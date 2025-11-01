using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SellMenuButton : MonoBehaviour
{
    [SerializeField]
    GameObject purchaseMenu;
    [SerializeField]
    GameObject sellMenu;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(
            () =>
            {
                purchaseMenu.SetActive(false);
                sellMenu.SetActive(true);
            }
        );
    }
}
