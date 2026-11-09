using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResetHoldablesButton : MonoBehaviour
{
    void Awake() => GetComponent<Button>().onClick.AddListener(() =>
    {
        Holdable[] holdables = FindObjectsByType<Holdable>();
        foreach (Holdable holdable in holdables)
            if (!holdable.Held) holdable.RespawnObject();
    });
}
