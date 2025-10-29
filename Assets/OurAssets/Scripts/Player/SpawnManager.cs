using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    Transform spawnPoint;

    void Start() => Respawn();

    public void Respawn()
    {
        if (spawnPoint == null) return;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }
}
