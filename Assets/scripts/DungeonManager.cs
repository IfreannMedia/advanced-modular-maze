using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{

    public Maze[] mazes;
    public int width = 30;
    public int depth = 30;
    public float levelDistance = 1.5f;

    public GameObject stairwell;

    void Start()
    {
        int level = 0;
        for (int i = 0; i < mazes.Length; i++)
        {
            mazes[i].width = width;
            mazes[i].depth = depth;
            mazes[i].level = level++;
            mazes[i].levelDistance = levelDistance;
            mazes[i].Build();
        }

        // update width hand depth according to maze
        width += mazes[0].scale;
        depth += mazes[0].scale;

        for (int mazeIndex = 0; mazeIndex < mazes.Length - 1; mazeIndex++)
        {
            if (PlaceStairs(mazeIndex, 90f, Maze.PieceType.DeadEnd, Maze.PieceType.DeadUpsideDown, stairwell)) continue;
            if (PlaceStairs(mazeIndex, 0f, Maze.PieceType.DeadToLeft, Maze.PieceType.DeadToRight, stairwell)) continue;
            if (PlaceStairs(mazeIndex, 180f, Maze.PieceType.DeadToRight, Maze.PieceType.DeadToLeft, stairwell)) continue;
            PlaceStairs(mazeIndex, -90f, Maze.PieceType.DeadUpsideDown, Maze.PieceType.DeadEnd, stairwell);
        }
        for (int mazeIndex = 0; mazeIndex < mazes.Length - 1; mazeIndex++)
        {
            mazes[mazeIndex + 1].gameObject.transform.Translate(mazes[mazeIndex + 1].xOffset * mazes[mazeIndex + 1].scale,
                0,
                mazes[mazeIndex + 1].zOffset * mazes[mazeIndex + 1].scale);
        }

        Teleporter[] teleporters = GetComponents<Teleporter>();
        if (teleporters.Length > 0)
            foreach (Teleporter teleporter in teleporters)
            {
                teleporter.Add(mazes[teleporter.startMaze], mazes[teleporter.endMaze]);
            }
    }

    bool PlaceStairs(int mazeIndex, float rotAngle, Maze.PieceType bottomType, Maze.PieceType upperType, GameObject stairPrefab)
    {
        List<MapLocation> startingLocations = new List<MapLocation>();
        List<MapLocation> endingLocations = new List<MapLocation>();

        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                if (mazes[mazeIndex].piecePlaces[x, z].piece == bottomType)
                    startingLocations.Add(new MapLocation(x, z));

                if (mazes[mazeIndex + 1].piecePlaces[x, z].piece == upperType)
                    endingLocations.Add(new MapLocation(x, z));
            }

        if (startingLocations.Count == 0 || endingLocations.Count == 0) return false;

        MapLocation bottomOfStairs = startingLocations[UnityEngine.Random.Range(0, startingLocations.Count)];
        MapLocation topOfStairs = endingLocations[UnityEngine.Random.Range(0, endingLocations.Count)];

        mazes[mazeIndex + 1].xOffset = bottomOfStairs.x - topOfStairs.x + mazes[mazeIndex].xOffset;
        mazes[mazeIndex + 1].zOffset = bottomOfStairs.z - topOfStairs.z + mazes[mazeIndex].zOffset;

        Vector3 stairPosBottom = new Vector3(bottomOfStairs.x * mazes[mazeIndex].scale,
                                                    mazes[mazeIndex].scale * mazes[mazeIndex].level
                                                            * mazes[mazeIndex].levelDistance,
                                                    bottomOfStairs.z * mazes[mazeIndex].scale);

        Destroy(mazes[mazeIndex].piecePlaces[bottomOfStairs.x, bottomOfStairs.z].model);
        Destroy(mazes[mazeIndex + 1].piecePlaces[topOfStairs.x, topOfStairs.z].model);

        GameObject stairs = Instantiate(stairPrefab, stairPosBottom, Quaternion.identity);
        stairs.transform.Rotate(0, rotAngle, 0);
        mazes[mazeIndex].piecePlaces[bottomOfStairs.x, bottomOfStairs.z].model = stairs;
        mazes[mazeIndex].piecePlaces[bottomOfStairs.x, bottomOfStairs.z].piece = Maze.PieceType.Manhole;
        mazes[mazeIndex].piecePlaces[bottomOfStairs.x, bottomOfStairs.z].model.GetComponent<MapLoc>().x = bottomOfStairs.x;
        mazes[mazeIndex].piecePlaces[bottomOfStairs.x, bottomOfStairs.z].model.GetComponent<MapLoc>().z = bottomOfStairs.z;

        mazes[mazeIndex + 1].piecePlaces[topOfStairs.x, topOfStairs.z].model = null;
        mazes[mazeIndex + 1].piecePlaces[topOfStairs.x, topOfStairs.z].piece = Maze.PieceType.Manhole;

        stairs.transform.SetParent(mazes[mazeIndex].gameObject.transform);

        mazes[mazeIndex].exitPoint = new MapLocation(bottomOfStairs.x, bottomOfStairs.z);
        mazes[mazeIndex+1].entryPoint = new MapLocation(topOfStairs.x, topOfStairs.z);
        return true;
    }
}
