using System.Collections;
using UnityEngine;

public class AnimalPoolManager : MonoBehaviour
{
    public static AnimalPoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RespawnAnimal(Animal animal, float delay, Vector3 respawnPosition)
    {
        StartCoroutine(RespawnRoutine(animal, delay, respawnPosition));
    }

    private IEnumerator RespawnRoutine(Animal animal, float delay, Vector3 respawnPosition)
    {
        animal.gameObject.SetActive(false);

        yield return new WaitForSeconds(delay);

        animal.ResetAnimal(respawnPosition);
        animal.gameObject.SetActive(true);
    }
}