using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PurchaseMenuButton : MonoBehaviour
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
                purchaseMenu.SetActive(true);
                sellMenu.SetActive(false);
            }
        );
    }
}
