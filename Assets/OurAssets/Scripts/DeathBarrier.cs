using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DeathBarrier : MonoBehaviour
{
    void OnValidate() => GetComponent<BoxCollider>().isTrigger = true;

    void Awake() => GetComponent<BoxCollider>().isTrigger = true;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            FirstPersonPlayerCharacter player = other.gameObject.GetComponent<FirstPersonPlayerCharacter>();
            if (player) player.Respawn();
        }
        else
        {
            Holdable holdable = other.gameObject.GetComponent<Holdable>();
            if (holdable) holdable.RespawnObject();
        }
    }
}
