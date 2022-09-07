using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : Maze
{

    public override void Generate()
    {
        //for (int i = 0; i < 2; i++)
       //    CrawlV();

       //for(int i = 0; i < 3; i++)
        //    CrawlH();
    }

    void CrawlV()
    {
        bool done = false;
        int x = Random.Range(1,width-1);
        int z = 1;

        while (!done)
        {
            map[x, z] = 0;
            if (Random.Range(0, 100) < 50)
                x += Random.Range(-1, 2);
            else
                z += Random.Range(0, 2);
            done |= (x < 1 || x >= width-1 || z < 1 || z >= depth-1);
        }
    }

    void CrawlH()
    {
        bool done = false;
        int x = 1;
        int z = Random.Range(1,depth-1);

        while (!done)
        {
            map[x, z] = 0;
            if (Random.Range(0, 100) < 50)
                x += Random.Range(0, 2);
            else
                z += Random.Range(-1, 2);
            done |= (x < 1 || x >= width-1 || z < 1 || z >= depth-1);
        }
    }

}
