using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    [SerializeField]
    List<GameObject> prefabs;
    [SerializeField]
    float xPosition, yPosition;
    float time = 0, screenWidth, screenHeight;
    ObjectScript script;
    [SerializeField]
    int poolSize = 10;
    List<GameObject> Objects;

    void Start()
    {
        screenWidth = Camera.main.orthographicSize * Camera.main.aspect;
        screenHeight = Camera.main.orthographicSize * 2;
        Objects = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            int prefabIndex = i % prefabs.Count;
            GameObject gameObject = Instantiate(prefabs[prefabIndex]);
            gameObject.SetActive(false);
            Objects.Add(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        SpawnObject(new Vector3(), 0);
    }

    void SpawnObject(Vector3 spawnPos, int index)
    {
        if (index < 0 || index >= prefabs.Count)
        {
            Debug.LogError("Index out of range");
            return;
        }

        time += Time.deltaTime;
        if (time >= 2)
        {
            GameObject gameObject = GetFreeGameObject();
            if (gameObject != null)
            {
                xPosition = Random.Range(0, 2) == 0 ? (screenWidth - 20) : -(screenWidth - 20);
                yPosition = Random.Range(-screenHeight, screenHeight);
                spawnPos = new Vector3(xPosition, yPosition);
                gameObject.transform.position = spawnPos;
                //script = gameObject.GetComponent<ObjectScript>();
                time = 0;
            }
        }
    }
    private GameObject GetFreeGameObject()
    {
        foreach (GameObject gameObject in Objects)
        {
            if (gameObject.activeSelf == false)
            {
                gameObject.SetActive(true);
                return gameObject;
            }
        }
        return null;
    }
}
