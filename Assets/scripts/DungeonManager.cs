using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{

    public Maze[] mazes;
    public int width = 30;
    public int depth = 30;
    public float levelDistance = 1.5f;

    public GameObject stairwell;
    List<MapLocation> levelOneDeadEnds = new List<MapLocation>();
    List<MapLocation> levelTwoDeadEndsUpsideDown = new List<MapLocation>();

    List<MapLocation> levelOneDeadEndsRight = new List<MapLocation>();
    List<MapLocation> levelTwoDeadEndsLeft = new List<MapLocation>();

    // Start is called before the first frame update
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
        for (int mazeIndex = 0; mazeIndex < mazes.Length - 1; mazeIndex++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (mazes[mazeIndex].piecePlaces[x, z].piece == Maze.PieceType.DeadEnd)
                    {
                        levelOneDeadEnds.Add(new MapLocation(x, z));
                    }
                    if (mazes[mazeIndex + 1].piecePlaces[x, z].piece == Maze.PieceType.DeadUpsideDown)
                    {
                        levelTwoDeadEndsUpsideDown.Add(new MapLocation(x, z));
                    }
                    if (mazes[mazeIndex].piecePlaces[x, z].piece == Maze.PieceType.DeadToRight)
                    {
                        levelOneDeadEndsRight.Add(new MapLocation(x, z));
                    }
                    if (mazes[mazeIndex + 1].piecePlaces[x, z].piece == Maze.PieceType.DeadToLeft)
                    {
                        levelTwoDeadEndsLeft.Add(new MapLocation(x, z));
                    }

                    //if (mazes[mazeIndex].piecePlaces[x, z].piece == Maze.PieceType.DeadEnd &&
                    //    mazes[mazeIndex + 1].piecePlaces[x, z].piece == Maze.PieceType.DeadUpsideDown)
                    //{
                    //    GenerateStairWell(mazeIndex, x, z, Quaternion.Euler(0, 90, 0));
                    //}
                    //else if (mazes[mazeIndex].piecePlaces[x, z].piece == Maze.PieceType.DeadToLeft
                    //    && mazes[mazeIndex + 1].piecePlaces[x, z].piece == Maze.PieceType.DeadToRight)
                    //{
                    //    GenerateStairWell(mazeIndex, x, z, Quaternion.identity);
                    //}

                }
            }

            if (levelOneDeadEnds.Count == 0 || levelTwoDeadEndsUpsideDown.Count == 0) break;

            MapLocation bottomOfStairs = levelOneDeadEnds[UnityEngine.Random.Range(0, levelOneDeadEnds.Count)];
            MapLocation topOfStairs = levelTwoDeadEndsUpsideDown[UnityEngine.Random.Range(0, levelTwoDeadEndsUpsideDown.Count)];

            Vector3 stairBottomPos = new Vector3(bottomOfStairs.x * mazes[mazeIndex].scale,
                                                 mazes[mazeIndex].scale * mazes[mazeIndex].level * mazes[mazeIndex].levelDistance,
                                                 bottomOfStairs.z * mazes[mazeIndex].scale);
            Vector3 stairTopPos = new Vector3(topOfStairs.x * mazes[mazeIndex + 1].scale,
                                                 mazes[mazeIndex + 1].scale * mazes[mazeIndex + 1].level * mazes[mazeIndex + 1].levelDistance,
                                                 topOfStairs.z * mazes[mazeIndex + 1].scale);

            // visualize places
            GameObject sphere = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), stairBottomPos, Quaternion.identity);
            GameObject sphere2 = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), stairTopPos, Quaternion.identity);
            sphere.name = "BOTTOM STAIRS";
            sphere2.name = "TOP STAIRS";

        }
    }

    private void GenerateStairWell(int mazeIndex, int x, int z, Quaternion rot)
    {
        Destroy(mazes[mazeIndex].piecePlaces[x, z].model);
        Destroy(mazes[mazeIndex + 1].piecePlaces[x, z].model);
        Vector3 stairPos = new Vector3(x * mazes[mazeIndex].scale,
                                            mazes[mazeIndex].scale * mazes[mazeIndex].level * levelDistance,
                                            z * mazes[mazeIndex].scale);
        mazes[mazeIndex].piecePlaces[x, z].model = Instantiate(stairwell, stairPos, rot);
        mazes[mazeIndex].piecePlaces[x, z].piece = Maze.PieceType.Manhole;
        mazes[mazeIndex + 1].piecePlaces[x, z].model = null;
        mazes[mazeIndex + 1].piecePlaces[x, z].piece = Maze.PieceType.Manhole;
    }
}
