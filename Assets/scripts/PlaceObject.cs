using UnityEngine;

public class PlaceObject : MonoBehaviour
{
    public GameObject item;
    [Range(1,100)]public int chanceOfAppearance = 20;
    public Maze.PieceType location;

    Maze maze;

    public void PlaceItems()
    {
        maze = GetComponent<Maze>();
        if (maze == null)
            return;

        for (int x = 0; x < maze.width; x++)
        {
            for (int z = 0; z < maze.depth; z++)
            {
                if (maze.piecePlaces[x,z].piece == location && Random.Range(1, 100) < chanceOfAppearance)
                {
                    Instantiate(item, maze.piecePlaces[x, z].model.transform);
                }
            }
        }
    }
}
