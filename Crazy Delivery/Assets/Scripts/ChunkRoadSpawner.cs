using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChunkRoadSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> roads;
    [SerializeField] GameObject[] roadChain = new GameObject[6];
    private GameObject road;
    
    [SerializeField] private bool firstSpawn = true;
    [SerializeField] private float _roadLength;
    private int counter = 4;
    
    public void Spawn()
    {
        Vector3 position = new Vector3((road.transform.position.z + _roadLength) * counter, 0, 0);
        road = Instantiate(roads[Random.Range(0, roads.Count)], position, Quaternion.identity);
        if (firstSpawn)
        {
            roadChain[roadChain.Length - 2] = road;
            firstSpawn = false;
        }
        else
        {
            roadChain[roadChain.Length - 1] = road;
            ChunkRemove();
        }
        counter++;
    }
    
    private void Start()
    {
        road = Instantiate(roads[Random.Range(0, roads.Count)], transform.position, Quaternion.identity);
        for (int i = 0; i < roadChain.Length - 2; i++)
        {
            if (i == roadChain.Length - 3)
            {
                roadChain[i] = road;
            }
            else
            {
                roadChain[i] = GameObject.Find($"Chunk{i}");
            }
        }
    }

    private void ChunkRemove()
    {
        Destroy(roadChain[0]);
        for (int i = 1; i < roadChain.Length; i++)
        {
            roadChain[i - 1] = roadChain[i];
        }
        roadChain[roadChain.Length - 1] = null;
    }
}
