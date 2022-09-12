using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{

    public Maze[] mazes;
    public int width = 30;
    public int depth = 30;

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
            mazes[i].Build();
        }

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (mazes[0].piecePlaces[x,z].piece == mazes[1].piecePlaces[x,z].piece)
                {
                    if (mazes[0].piecePlaces[x,z].piece == Maze.PieceType.Vertical_Straight)
                    {
                        Destroy(mazes[0].piecePlaces[x, z].model);
                        Destroy(mazes[1].piecePlaces[x, z].model);
                        Vector3 upManholePos = new Vector3(x * mazes[0].scale, 
                                                            mazes[0].scale * mazes[0].level * 2,
                                                            z * mazes[0].scale);
                        mazes[0].piecePlaces[x, z].model = Instantiate(straightManholeUp, upManholePos, Quaternion.identity);

                        Vector3 downManholePos = new Vector3(x * mazes[1].scale,
                                    mazes[1].scale * mazes[1].level * 2,
                                    z * mazes[1].scale);
                        mazes[1].piecePlaces[x, z].model = Instantiate(straightManholeLadder, downManholePos, Quaternion.identity);

                       

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
