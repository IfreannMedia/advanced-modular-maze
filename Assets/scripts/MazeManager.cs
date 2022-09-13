using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{

    public Maze[] mazes;
    public int width = 30;
    public int depth = 30;
    public float levelDistance = 2f;

    public GameObject deadEndManholeLadder;
    public GameObject deadEndManholeUp;
    public GameObject straightManholeLadder;
    public GameObject straightManholeUp;

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
                    if (mazes[mazeIndex].piecePlaces[x, z].piece == mazes[mazeIndex + 1].piecePlaces[x, z].piece)
                    {
                        if (mazes[mazeIndex].piecePlaces[x, z].piece == Maze.PieceType.Vertical_Straight)
                        {
                            Destroy(mazes[mazeIndex].piecePlaces[x, z].model);
                            Destroy(mazes[mazeIndex + 1].piecePlaces[x, z].model);
                            Vector3 upManholePos = new Vector3(x * mazes[mazeIndex].scale,
                                                                mazes[mazeIndex].scale * mazes[mazeIndex].level * levelDistance,
                                                                z * mazes[mazeIndex].scale);
                            mazes[mazeIndex].piecePlaces[x, z].model = Instantiate(straightManholeUp, upManholePos, Quaternion.identity);
                            mazes[mazeIndex].piecePlaces[x, z].piece = Maze.PieceType.Manhole;
                            Vector3 downManholePos = new Vector3(x * mazes[mazeIndex + 1].scale,
                                        mazes[mazeIndex + 1].scale * mazes[mazeIndex + 1].level * levelDistance,
                                        z * mazes[mazeIndex + 1].scale);
                            mazes[mazeIndex + 1].piecePlaces[x, z].model = Instantiate(straightManholeLadder, downManholePos, Quaternion.identity);
                            mazes[mazeIndex + 1].piecePlaces[x, z].piece = Maze.PieceType.Manhole;


                        }
                        else if (mazes[mazeIndex].piecePlaces[x, z].piece == Maze.PieceType.DeadEnd)
                        {
                            Destroy(mazes[mazeIndex].piecePlaces[x, z].model);
                            Destroy(mazes[mazeIndex + 1].piecePlaces[x, z].model);
                            Vector3 upManholePos = new Vector3(x * mazes[mazeIndex].scale,
                                                                mazes[mazeIndex].scale * mazes[mazeIndex].level * levelDistance,
                                                                z * mazes[mazeIndex].scale);
                            mazes[mazeIndex].piecePlaces[x, z].model = Instantiate(deadEndManholeUp, upManholePos, Quaternion.identity);
                            mazes[mazeIndex].piecePlaces[x, z].piece = Maze.PieceType.Manhole;

                            Vector3 downManholePos = new Vector3(x * mazes[mazeIndex + 1].scale,
                                        mazes[mazeIndex + 1].scale * mazes[mazeIndex + 1].level * levelDistance,
                                        z * mazes[mazeIndex + 1].scale);
                            mazes[mazeIndex + 1].piecePlaces[x, z].model = Instantiate(deadEndManholeLadder, downManholePos, Quaternion.identity);
                            mazes[mazeIndex + 1].piecePlaces[x, z].piece = Maze.PieceType.Manhole;
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
