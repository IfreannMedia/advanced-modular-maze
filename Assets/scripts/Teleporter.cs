using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject teleportPrefab;
    public int startMaze;
    public int endMaze;

    public void Add(Maze fromMaze, Maze toMaze)
    {
        int fIndex = Random.Range(0, fromMaze.locations.Count);
        int tIndex = Random.Range(0, toMaze.locations.Count);

        Vector3 fpos = fromMaze.piecePlaces[fromMaze.locations[fIndex].x, 
                                            fromMaze.locations[fIndex].z].model.transform.position;

        GameObject fromTeleport = Instantiate(teleportPrefab, fpos, Quaternion.identity);

        Vector3 tpos = toMaze.piecePlaces[toMaze.locations[tIndex].x,
                                            toMaze.locations[tIndex].z].model.transform.position;

        fromTeleport.GetComponent<Teleport>().destination = tpos;
    }

}
