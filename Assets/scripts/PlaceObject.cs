using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceObject : MonoBehaviour
{
    public GameObject item;
    [Range(1,40)]public int chanceOfAppearance = 20;
    public Maze.PieceType location;

    public void PlaceItems(Maze maze)
    {
        for (int x = 0; x < maze.width; x++)
        {
            for (int z = 0; z < maze.depth; z++)
            {
                if (maze.piecePlaces[x,z].piece == location && Random.Range(1, chanceOfAppearance) == 10)
                {
                    GameObject go = Instantiate(item, maze.piecePlaces[x, z].model.transform.position, Quaternion.identity);
                    go.transform.SetParent(maze.piecePlaces[x, z].model.transform);
                    go.name = "placed_item";
                }
            }
        }
    }
}
