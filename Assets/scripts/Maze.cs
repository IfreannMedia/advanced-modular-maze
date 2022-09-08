using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLocation
{
    public int x;
    public int z;

    public MapLocation(int _x, int _z)
    {
        x = _x;
        z = _z;
    }
}

public class Maze : MonoBehaviour
{
    public List<MapLocation> directions = new List<MapLocation>() {
                                            new MapLocation(1,0),
                                            new MapLocation(0,1),
                                            new MapLocation(-1,0),
                                            new MapLocation(0,-1) };
    public int width = 30; //x length
    public int depth = 30; //z length
    public byte[,] map;
    public int scale = 6;

    public GameObject straight;
    public GameObject cornerCurved; // use for bottom or top right (-90 on y)
    public GameObject cornerStraight; // use for bottom or top left (90 on y)
    public GameObject crossroads;
    public GameObject junction;
    public GameObject deadend;

    // patterns
    private int[] verticalStraight = new int[] { 5, 0, 5, 1, 0, 1, 5, 0, 5 };
    private int[] horizontalStraight = new int[] { 5, 1, 5, 0, 0, 0, 5, 1, 5 };
    private int[] crossroadsPattern = new int[] { 5, 0, 5, 0, 0, 0, 5, 0, 5 };

    private int[] junctionTop = new int[] { 5, 0, 5, 0, 0, 0, 5, 1, 5 }; // entering top
    private int[] junctionBottom = new int[] { 5, 1, 5, 0, 0, 0, 5, 0, 5 }; // entering bottom
    private int[] junctionLeft = new int[] { 5, 0, 5, 0, 0, 1, 5, 0, 5 }; // entering left
    private int[] junctionRight = new int[] { 5, 0, 5, 1, 0, 0, 5, 0, 5 }; // entering right

    private int[] deadendTop = new int[] { 5, 0, 5, 1, 0, 1, 5, 1, 5 };
    private int[] deadendLeft = new int[] { 5, 1, 5, 0, 0, 1, 5, 1, 5 };
    private int[] deadendBottom = new int[] { 5, 1, 5, 1, 0, 1, 5, 0, 5 };
    private int[] deadendRight = new int[] { 5, 1, 5, 1, 0, 0, 5, 1, 5 };

    private int[] cornerLeftTop = new int[] { 5, 0, 5, 0, 0, 1, 5, 1, 5 };
    private int[] cornerLeftBottom = new int[] { 5, 1, 5, 0, 0, 1, 5, 0, 5 };
    private int[] cornerRightBottom = new int[] { 5, 1, 5, 1, 0, 0, 5, 0, 5 };
    private int[] cornerRightTop = new int[] { 5, 0, 5, 1, 0, 0, 5, 1, 5 };

    private readonly int wildcard = 5;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseMap();
        Generate();
        DrawMap();
    }

    void InitialiseMap()
    {
        map = new byte[width, depth];
        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                map[x, z] = 1;     //1 = wall  0 = corridor
            }
    }

    public virtual void Generate()
    {
        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                if (Random.Range(0, 100) < 50)
                    map[x, z] = 0;     //1 = wall  0 = corridor
            }
    }

    void DrawMap()
    {
        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x * scale, 0, z * scale);
                if (map[x, z] == 1)
                {
                    // draws a white wall ie non-walkable block
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.localScale = new Vector3(scale, scale, scale);
                    wall.transform.position = pos;
                }
                else
                {
                    PlaceWalkablePiece(z, x, pos);
                }

            }
    }

    private void PlaceWalkablePiece(int z, int x, Vector3 pos)
    {
        int[] matchedPattern = HasMatchedPattern(x, z, new int[][] { verticalStraight, horizontalStraight,
                        cornerLeftBottom, cornerLeftTop, cornerRightBottom, cornerRightTop,
                        crossroadsPattern,
                        deadendBottom, deadendLeft, deadendRight, deadendTop,
                        junctionBottom, junctionLeft, junctionRight, junctionTop });
        if (matchedPattern == verticalStraight)
        {
            Instantiate(straight, pos, Quaternion.identity);
        }
        else if (matchedPattern == horizontalStraight)
        {
            Instantiate(straight, pos, Quaternion.Euler(0, 90, 0));
        }
        else if (matchedPattern == cornerLeftBottom || matchedPattern == cornerLeftTop || matchedPattern == cornerRightBottom || matchedPattern == cornerRightTop)
        {
            Quaternion rot = GetCornerRotation(matchedPattern);
            Instantiate(GetCornerPiece(matchedPattern), pos, rot);
        }
        else if (matchedPattern == crossroadsPattern)
        {
            Instantiate(crossroads, pos, Quaternion.identity);
        }
        else if (matchedPattern == deadendBottom || matchedPattern == deadendLeft || matchedPattern == deadendRight || matchedPattern == deadendTop)
        {
            Instantiate(deadend, pos, GetDeadendRotation(matchedPattern));
        }
        else if (matchedPattern == junctionBottom || matchedPattern == junctionLeft || matchedPattern == junctionRight || matchedPattern == junctionTop)
        {
            Instantiate(junction, pos, GetJunctionRotation(matchedPattern));
        }
    }

    private GameObject GetCornerPiece(int[] pattern)
    {
        if (pattern == cornerLeftBottom || pattern == cornerLeftTop)
            return cornerStraight;
        else
            return cornerCurved;
    }

    private Quaternion GetCornerRotation(int[] pattern)
    {
        Quaternion val = Quaternion.identity;
        if (pattern == cornerLeftTop)
            val = Quaternion.Euler(0, -90, 0);
        else if (pattern == cornerRightTop)
            val = Quaternion.Euler(0, 90, 0);
        else if (pattern == cornerLeftBottom)
            val = Quaternion.Euler(0, 180, 0);
        else if (pattern == cornerRightBottom)
            val = Quaternion.Euler(0, 180, 0);
        return val;
    }

    private Quaternion GetDeadendRotation(int[] pattern)
    {
        Quaternion val = Quaternion.identity;
        if (pattern == deadendLeft)
            val = Quaternion.Euler(0, 90, 0);
        else if (pattern == deadendRight)
            val = Quaternion.Euler(0, -90, 0);
        else if (pattern == deadendTop)
            val = Quaternion.Euler(0, 180, 0);
        return val;
    }

    private Quaternion GetJunctionRotation(int[] pattern)
    {
        Quaternion val = Quaternion.identity;
        if (pattern == junctionBottom)
            val = Quaternion.Euler(0, 90, 0);
        else if (pattern == junctionLeft)
            val = Quaternion.Euler(0, 180, 0);
        else if (pattern == junctionTop)
            val = Quaternion.Euler(0, -90, 0);
        return val;
    }

    private int[] HasMatchedPattern(int x, int z, int[][] possibleMatches)
    {
        int[] matchedPattern = null;
        for (int i = 0; i < possibleMatches.Length; i++)
        {
            matchedPattern = Search2D(x, z, possibleMatches[i]);
            if (matchedPattern != null)
                break;
        }
        return matchedPattern;
    }

    int[] Search2D(int cellPos, int rowPos, int[] pattern)
    {
        int matches = 0;
        int pos = 0;
        // loop over 3x3 grid along z and x axis, top down view of our maze position
        // ie 3 z position, and for each z position, 3 x positions
        // ie make a code that is 9 digits long, each digit representing a grid square,
        // then check the map positions against this code (if they are open/walkable or a wall)
        // starting at top left square in the grid (z=1, x=-1)
        // 5 indicates a wildcard we don't care
        for (int z = 1; z > -2; z--)
        {
            for (int x = -1; x < 2; x++)
            {
                if (pattern[pos] == map[cellPos + x, rowPos + z] || pattern[pos] == wildcard)
                    matches++;
                pos++;
            }
        }
        return matches == 9 ? pattern : null;
    }

    public int CountSquareNeighbours(int x, int z)
    {
        int count = 0;
        if (x <= 0 || x >= width - 1 || z <= 0 || z >= depth - 1) return 5;
        if (map[x - 1, z] == 0) count++;
        if (map[x + 1, z] == 0) count++;
        if (map[x, z + 1] == 0) count++;
        if (map[x, z - 1] == 0) count++;
        return count;
    }

    public int CountDiagonalNeighbours(int x, int z)
    {
        int count = 0;
        if (x <= 0 || x >= width - 1 || z <= 0 || z >= depth - 1) return 5;
        if (map[x - 1, z - 1] == 0) count++;
        if (map[x + 1, z + 1] == 0) count++;
        if (map[x - 1, z + 1] == 0) count++;
        if (map[x + 1, z - 1] == 0) count++;
        return count;
    }

    public int CountAllNeighbours(int x, int z)
    {
        return CountSquareNeighbours(x, z) + CountDiagonalNeighbours(x, z);
    }
}
