using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    [SerializeField]
    List<GameObject> prefabs;

    [SerializeField]
    GameObject VerticalBorder1, HorizontalBorder1, VerticalBorder2, HorizontalBorder2;

    [SerializeField]
    float spawnInterval = 2f, elapsedTime = 0f;

    List<GameObject> Objects;

    void Start()
    {
        Objects = new List<GameObject>();

        foreach (GameObject obj in prefabs)
        {
            GameObject gameObject = Instantiate(obj);
            gameObject.SetActive(false);
            Objects.Add(gameObject);
        }

        InvokeRepeating(nameof(SpawnObject), elapsedTime, spawnInterval);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SpawnObject()
    {
        int index = Random.Range(0, prefabs.Count);
        Vector3 spawnPos = GetRandomSpawnPosition();

        if (index >= 0 && index < Objects.Count)
        {
            GameObject obj = Objects[index];
            if (obj != null)
            {
                obj.SetActive(true);
                obj.transform.position = spawnPos;
            }
            else
            {
                Debug.LogError("Spawned object is null");
            }
        }
        else
        {
            Debug.LogError("Index out of range");
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float minX = HorizontalBorder1.transform.position.x - HorizontalBorder1.transform.localScale.x / 2;
        float maxX = HorizontalBorder2.transform.position.x + HorizontalBorder2.transform.localScale.x / 2;
        float minY = VerticalBorder1.transform.position.y - VerticalBorder1.transform.localScale.y / 2;
        float maxY = VerticalBorder2.transform.position.y + VerticalBorder2.transform.localScale.y / 2;

        float lerpX = Mathf.Lerp(minX, maxX, Random.value);
        float lerpY = Mathf.Lerp(minY, maxY, Random.value);

        return new Vector3(lerpX, lerpY, 0f);
    }
}
