using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
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
        for (int mazeIndex = 0; mazeIndex < mazes.Length - 1; mazeIndex++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
