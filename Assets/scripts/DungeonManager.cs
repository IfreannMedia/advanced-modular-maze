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
            levelOneDeadEnds.Clear();
            levelOneDeadEndsRight.Clear();
            levelTwoDeadEndsLeft.Clear();
            levelTwoDeadEndsUpsideDown.Clear();
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

                }
            }

            if (levelOneDeadEnds.Count == 0 || levelTwoDeadEndsUpsideDown.Count == 0) break;

            MapLocation bottomOfStairs = levelOneDeadEnds[UnityEngine.Random.Range(0, levelOneDeadEnds.Count)];
            MapLocation topOfStairs = levelTwoDeadEndsUpsideDown[UnityEngine.Random.Range(0, levelTwoDeadEndsUpsideDown.Count)];

            mazes[mazeIndex + 1].xOffset = bottomOfStairs.x - topOfStairs.x + mazes[mazeIndex].xOffset;
            mazes[mazeIndex + 1].zOffset = bottomOfStairs.z - topOfStairs.z + mazes[mazeIndex].zOffset;


            Vector3 stairBottomPos = new Vector3(bottomOfStairs.x * mazes[mazeIndex].scale,
                                                 mazes[mazeIndex].scale * mazes[mazeIndex].level * mazes[mazeIndex].levelDistance,
                                                 bottomOfStairs.z * mazes[mazeIndex].scale);
            Vector3 stairTopPos = new Vector3(topOfStairs.x * mazes[mazeIndex + 1].scale,
                                                 mazes[mazeIndex + 1].scale * mazes[mazeIndex + 1].level * mazes[mazeIndex + 1].levelDistance,
                                                 topOfStairs.z * mazes[mazeIndex + 1].scale);

            GenerateStairWell(mazeIndex, bottomOfStairs, topOfStairs, stairBottomPos, Quaternion.Euler(0,90,0));

        }
        for (int mazeIndex = 0; mazeIndex < mazes.Length - 1; mazeIndex++)
        {
            mazes[mazeIndex + 1].gameObject.transform.Translate(mazes[mazeIndex + 1].xOffset * mazes[mazeIndex + 1].scale,
                0,
                mazes[mazeIndex + 1].zOffset * mazes[mazeIndex + 1].scale);
        }
    }

    private void GenerateStairWell(int mazeIndex, MapLocation bottom, MapLocation top, Vector3 stairPos, Quaternion rot)
    {
        Destroy(mazes[mazeIndex].piecePlaces[bottom.x, bottom.z].model);
        Destroy(mazes[mazeIndex + 1].piecePlaces[top.x, top.z].model);
        //Vector3 stairPos = new Vector3(x * mazes[mazeIndex].scale,
        //                                    mazes[mazeIndex].scale * mazes[mazeIndex].level * levelDistance,
        //                                    z * mazes[mazeIndex].scale);
        mazes[mazeIndex].piecePlaces[bottom.x, bottom.z].model = Instantiate(stairwell, stairPos, rot);
        mazes[mazeIndex].piecePlaces[bottom.x, bottom.z].piece = Maze.PieceType.Manhole;
        mazes[mazeIndex + 1].piecePlaces[top.x, top.z].model = null;
        mazes[mazeIndex + 1].piecePlaces[top.x, top.z].piece = Maze.PieceType.Manhole;
        mazes[mazeIndex].piecePlaces[bottom.x, bottom.z].model.transform.SetParent(mazes[mazeIndex].transform);


    }
}
