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
    //public GameObject cornerCurved; // use for bottom or top right (-90 on y) -- only relevant for the piped map
    public GameObject cornerStraight;
    public GameObject crossroads;
    public GameObject junction;
    public GameObject deadend;
    public GameObject wallPiece;
    public GameObject floorPiece;
    public GameObject cielingPiece;
    public GameObject cornerPillar;
    public GameObject doorway;

    public FPController player;

    // patterns
    private int[] verticalStraight = new int[] { 5, 0, 5, 1, 0, 1, 5, 0, 5 };
    private int[] horizontalStraight = new int[] { 5, 1, 5, 0, 0, 0, 5, 1, 5 };
    private int[] crossroadsPattern = new int[] { 1, 0, 1, 0, 0, 0, 1, 0, 1 };

    private int[] junctionTop = new int[] { 1, 0, 1, 0, 0, 0, 5, 1, 5 }; // entering top
    private int[] junctionBottom = new int[] { 5, 1, 5, 0, 0, 0, 1, 0, 1 }; // entering bottom
    private int[] junctionLeft = new int[] { 1, 0, 5, 0, 0, 1, 1, 0, 5 }; // entering left
    private int[] junctionRight = new int[] { 5, 0, 1, 1, 0, 0, 5, 0, 1 }; // entering right

    private int[] deadendTop = new int[] { 5, 0, 5, 1, 0, 1, 5, 1, 5 };
    private int[] deadendLeft = new int[] { 5, 1, 5, 0, 0, 1, 5, 1, 5 };
    private int[] deadendBottom = new int[] { 5, 1, 5, 1, 0, 1, 5, 0, 5 };
    private int[] deadendRight = new int[] { 5, 1, 5, 1, 0, 0, 5, 1, 5 };

    private int[] cornerLeftTop = new int[] { 5, 1, 5, 1, 0, 0, 5, 0, 1 };
    private int[] cornerLeftBottom = new int[] { 5, 0, 1, 1, 0, 0, 5, 1, 5 };
    private int[] cornerRightBottom = new int[] { 1, 0, 5, 5, 0, 1, 5, 1, 5 };
    private int[] cornerRightTop = new int[] { 5, 1, 5, 0, 0, 1, 1, 0, 5 };

    private readonly int wildcard = 5;

    private List<MapLocation> pillarLocations = new List<MapLocation>();

    public enum PIECE_TYPE
    {
        HORIZONTAL_STRAIGHT,
        VERTICAL_STRAIGHT,
        RIGHT_UP_CORNER,
        RIGHT_DOWN_CORNER,
        LEFT_UP_CORNER,
        LEFT_DOWN_CORNER,
        T_JUNCTION,
        T_UPSIDE_DOWN,
        T_LEFT,
        T_RIGHT,
        DEADEAND,
        DEAD_UPSIDE_DOWN,
        DEAD_LEFT,
        DEAD_RIGHT,
        WALL,
        CROSSROAD,
        ROOM
    }

    public struct Pieces
    {
        public PIECE_TYPE piece;
        public GameObject model;

        public Pieces(PIECE_TYPE pt, GameObject mo)
        {
            this.piece = pt;
            this.model = mo;
        }
    }

    public Pieces[,] piecePlaces;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseMap();
        Generate();
        // add rooms here, same as setting walkable 0/non walkable 1
        AddRooms(3, 4, 6);
        DrawMap();
        PlaceFPC();
    }

    public virtual void AddRooms(int count, int minSize, int maxSize)
    {
        for (int c = 0; c < count; c++)
        {
            int startX = Random.Range(3, width - 3);
            int startZ = Random.Range(3, depth - 3);
            int roomWidth = Random.Range(minSize, maxSize);
            int roomDepth = Random.Range(minSize, maxSize);

            for (int x = startX; x < width - 3 && x < startX + roomWidth; x++)
            {
                for (int z = startZ; z < depth - 3 && z < startZ + roomWidth; z++)
                {
                    map[x, z] = 0;
                }
            }
        }
    }

    public virtual void PlaceFPC()
    {
        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                if (map[x, z] == 0)
                {
                    player.transform.position = new Vector3(x * scale, 0, z * scale);
                    player.transform.rotation = Quaternion.Euler(Vector3.forward);
                    return;
                }
            }
    }

    void InitialiseMap()
    {
        map = new byte[width, depth];
        piecePlaces = new Pieces[width, depth];
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
                    //GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //wall.transform.localScale = new Vector3(scale, scale, scale);
                    //wall.transform.position = pos;
                    //Debug.Log("setting piece to wall: " + x + ", " + z);
                    piecePlaces[x, z] = new Pieces(PIECE_TYPE.WALL, null);
                }
                else
                {
                    PlaceWalkablePiece(x, z, pos);
                }

            }

        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                if (piecePlaces[x, z].piece != PIECE_TYPE.ROOM) continue;
                Vector3 pos = new Vector3(x * scale, 0, z * scale);
                if (map[x, z] != 1)
                {
   
                    if (ShouldAddDoorTop(x, z))
                    {
                        GameObject door = Instantiate(doorway);
                        door.transform.position = new Vector3(x * scale, 0, z * scale);
                        door.name = "door-top";
                        door.transform.rotation = Quaternion.Euler(0, 90, 0);
                        door.transform.Translate(0, 0, 0.01f);
                    }
                    if (ShouldAddDoorBottom(x, z))
                    {
                        GameObject door = Instantiate(doorway);
                        door.transform.position = new Vector3(x * scale, 0, z * scale);
                        door.name = "door-bottom";
                        door.transform.rotation = Quaternion.Euler(0, -90, 0);
                        door.transform.Translate(0, 0, -0.01f);
                    }
                    if (ShouldAddDoorRight(x, z))
                    {
                        GameObject door = Instantiate(doorway);
                        door.transform.position = new Vector3(x * scale, 0, z * scale);
                        door.name = "door-right";
                        door.transform.rotation = Quaternion.Euler(0, 180, 0);
                        door.transform.Translate(-0.01f, 0, 0);
                    }
                    if (ShouldAddDoorLeft(x, z))
                    {
                        GameObject door = Instantiate(doorway);
                        door.transform.position = new Vector3(x * scale, 0, z * scale);
                        door.name = "door-left";
                        door.transform.Translate(0.01f, 0, 0);
                    }
                }

            }
    }

    private void PlaceWalkablePiece(int x, int z, Vector3 pos)
    {
        int[] matchedPattern = HasMatchedPattern(x, z, new int[][] { verticalStraight, horizontalStraight,
                        cornerLeftBottom, cornerLeftTop, cornerRightBottom, cornerRightTop,
                        crossroadsPattern,
                        deadendBottom, deadendLeft, deadendRight, deadendTop,
                        junctionBottom, junctionLeft, junctionRight, junctionTop });
        if (matchedPattern == deadendBottom || matchedPattern == deadendLeft || matchedPattern == deadendRight || matchedPattern == deadendTop)
        {
            piecePlaces[x, z] = new Pieces(GetDeadEndPieceType(matchedPattern), Instantiate(deadend, pos, GetDeadendRotation(matchedPattern)));
        }
        else if (matchedPattern == verticalStraight)
        {
            piecePlaces[x, z] = new Pieces(PIECE_TYPE.VERTICAL_STRAIGHT, Instantiate(straight, pos, Quaternion.Euler(0, 90, 0)));

        }
        else if (matchedPattern == horizontalStraight)
        {
            piecePlaces[x, z] = new Pieces(PIECE_TYPE.HORIZONTAL_STRAIGHT, Instantiate(straight, pos, Quaternion.identity));
        }
        else if (matchedPattern == crossroadsPattern)
        {
            piecePlaces[x, z] = new Pieces(PIECE_TYPE.CROSSROAD, Instantiate(crossroads, pos, Quaternion.identity));
        }
        else if (matchedPattern == cornerLeftBottom || matchedPattern == cornerLeftTop || matchedPattern == cornerRightBottom || matchedPattern == cornerRightTop)
        {
            piecePlaces[x, z] = new Pieces(GetCornerPieceType(matchedPattern), Instantiate(cornerStraight, pos, GetCornerRotation(matchedPattern)));

        }
        else if (matchedPattern == junctionBottom || matchedPattern == junctionLeft || matchedPattern == junctionRight || matchedPattern == junctionTop)
        {
            piecePlaces[x, z] = new Pieces(GetJunctionPieceType(matchedPattern), Instantiate(junction, pos, GetJunctionRotation(matchedPattern)));
        }
        else if (!PositionOnMapEdge(x, z) && ShouldAddRoomFloorPiece(x, z))
        {
            piecePlaces[x, z] = new Pieces(PIECE_TYPE.ROOM, Instantiate(floorPiece, pos, Quaternion.identity));
            Instantiate(cielingPiece, pos, Quaternion.identity);
            GameObject wall;
            GameObject pillar;
            // walls and pillars
            if (ShouldAddWallTop(x, z))
            {
                Debug.Log("WILL NOW set top wall: " + x + ", " + z);
                wall = Instantiate(wallPiece, pos, Quaternion.identity);
                wall.name = "wall-top";
                if (map[x + 1, z] == 0 && map[x + 1, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x, z)))
                {
                    pillar = Instantiate(cornerPillar);
                    pillar.transform.position = new Vector3(x * scale, 0, z * scale);
                    pillar.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    pillar.name = "pillar-top-right";
                    pillarLocations.Add(new MapLocation(x, z));
                }
                if (map[x - 1, z] == 0 && map[x - 1, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z)))
                {
                    pillar = Instantiate(cornerPillar);
                    pillar.transform.position = new Vector3((x - 1) * scale, 0, z * scale);
                    pillar.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    pillar.name = "pillar-top-left";
                    pillarLocations.Add(new MapLocation(x - 1, z));
                }
            }
            if (ShouldAddWallLeft(x, z))
            {
                wall = Instantiate(wallPiece, pos, Quaternion.Euler(0, -90, 0));
                wall.name = "wall-left";
                if (map[x, z + 1] == 0 && map[x - 1, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z)))
                {
                    pillar = Instantiate(cornerPillar);
                    pillar.transform.position = new Vector3((x - 1) * scale, 0, z * scale);
                    pillar.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    pillar.name = "pillar-left-top";
                    pillarLocations.Add(new MapLocation(x - 1, z));
                }
                if (map[x, z - 1] == 0 && map[x - 1, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z - 1)))
                {
                    pillar = Instantiate(cornerPillar);
                    pillar.transform.position = new Vector3((x - 1) * scale, 0, (z - 1) * scale);
                    pillar.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    pillar.name = "pillar-left-bottom";
                    pillarLocations.Add(new MapLocation(x - 1, z - 1));
                }
            }
            if (ShouldAddWallRight(x, z))
            {
                wall = Instantiate(wallPiece, pos, Quaternion.Euler(0, 90, 0));
                wall.name = "wall-right";
                if (map[x, z + 1] == 0 && map[x + 1, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x, z)))
                {
                    pillar = Instantiate(cornerPillar);
                    pillar.transform.position = new Vector3(x * scale, 0, z * scale);
                    pillar.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    pillar.name = "pillar-right-top";
                    pillarLocations.Add(new MapLocation(x, z));
                }
                if (map[x, z - 1] == 0 && map[x + 1, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x, z - 1)))
                {
                    pillar = Instantiate(cornerPillar);
                    pillar.transform.position = new Vector3(x * scale, 0, (z - 1) * scale);
                    pillar.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    pillar.name = "pillar-right-bottom";
                    pillarLocations.Add(new MapLocation(x, z - 1));
                }
            }
            if (ShouldAddWallBottom(x, z))
            {
                wall = Instantiate(wallPiece, pos, Quaternion.Euler(0, 180, 0));
                wall.name = "wall-bottom";
                if (map[x + 1, z] == 0 && map[x + 1, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x, z - 1)))
                {
                    pillar = Instantiate(cornerPillar);
                    pillar.transform.position = new Vector3(x * scale, 0, (z - 1) * scale);
                    pillar.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    pillar.name = "pillar-bottom-right";
                    pillarLocations.Add(new MapLocation(x, z - 1));
                }
                if (map[x - 1, z] == 0 && map[x - 1, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z - 1)))
                {
                    pillar = Instantiate(cornerPillar);
                    pillar.transform.position = new Vector3((x - 1) * scale, 0, (z - 1) * scale);
                    pillar.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    pillar.name = "pillar-bottom-left";
                    pillarLocations.Add(new MapLocation(x - 1, z - 1));
                }
            }
        }
    }

    private bool ShouldAddDoorTop(int x, int z)
    {
        return piecePlaces[x, z + 1].piece != PIECE_TYPE.WALL && piecePlaces[x - 1, z + 1].piece == PIECE_TYPE.WALL && piecePlaces[x + 1, z + 1].piece == PIECE_TYPE.WALL;
    }

    private bool ShouldAddDoorRight(int x, int z)
    {
        return piecePlaces[x + 1, z].piece != PIECE_TYPE.WALL && piecePlaces[x + 1, z + 1].piece == PIECE_TYPE.WALL && piecePlaces[x + 1, z - 1].piece == PIECE_TYPE.WALL;
    }

    private bool ShouldAddDoorBottom(int x, int z)
    {
        return piecePlaces[x, z - 1].piece != PIECE_TYPE.WALL && piecePlaces[x - 1, z - 1].piece == PIECE_TYPE.WALL && piecePlaces[x + 1, z - 1].piece == PIECE_TYPE.WALL;
    }

    private bool ShouldAddDoorLeft(int x, int z)
    {
        return piecePlaces[x - 1, z].piece != PIECE_TYPE.WALL && piecePlaces[x - 1, z + 1].piece == PIECE_TYPE.WALL && piecePlaces[x - 1, z - 1].piece == PIECE_TYPE.WALL;
    }

    private bool ShouldAddWallTop(int x, int z)
    {
        return map[x, z + 1] == 1;
    }

    private bool ShouldAddWallRight(int x, int z)
    {
        return map[x + 1, z] == 1;
    }

    private bool ShouldAddWallBottom(int x, int z)
    {
        return map[x, z - 1] == 1;
    }

    private bool ShouldAddWallLeft(int x, int z)
    {
        return map[x - 1, z] == 1;
    }

    private bool PositionOnMapEdge(int x, int z)
    {
        return x <= 0 || x >= width - 1 || z <= 0 || z >= depth - 1;
    }


    public virtual bool ShouldAddRoomFloorPiece(int x, int z)
    {
        return map[x, z] == 0
            && (CountSquareNeighbours(x, z) > 1
            && CountDiagonalNeighbours(x, z) >= 1
            || CountSquareNeighbours(x, z) >= 1
            && CountDiagonalNeighbours(x, z) > 1);
    }

    private Quaternion GetCornerRotation(int[] pattern)
    {
        Quaternion val = Quaternion.identity;
        if (pattern == cornerLeftTop)
            val = Quaternion.Euler(0, 90, 0);
        else if (pattern == cornerRightTop)
            val = Quaternion.Euler(0, -180, 0);
        else if (pattern == cornerRightBottom)
            val = Quaternion.Euler(0, -90, 0);
        return val;
    }

    private PIECE_TYPE GetCornerPieceType(int[] pattern)
    {
        PIECE_TYPE val = PIECE_TYPE.LEFT_DOWN_CORNER;
        if (pattern == cornerLeftTop)
            val = PIECE_TYPE.LEFT_UP_CORNER;
        else if (pattern == cornerRightTop)
            val = PIECE_TYPE.RIGHT_UP_CORNER;
        else if (pattern == cornerRightBottom)
            val = PIECE_TYPE.RIGHT_DOWN_CORNER;
        Debug.Log("got corner piece " + val + "for pattern: " + pattern[0] + ", " + pattern[1] + ", " + pattern[2] + ", " + pattern[3] + ", " + pattern[4] + ", " + pattern[5] + ", " + pattern[6] + ", " + pattern[7] + ", " + pattern[8]);
        return val;
    }

    private Quaternion GetDeadendRotation(int[] pattern)
    {
        Quaternion val = Quaternion.identity;
        if (pattern == deadendLeft)
            val = Quaternion.Euler(0, 180, 0);
        else if (pattern == deadendTop)
            val = Quaternion.Euler(0, -90, 0);
        else if (pattern == deadendBottom)
            val = Quaternion.Euler(0, 90, 0);
        return val;
    }

    private PIECE_TYPE GetDeadEndPieceType(int[] pattern)
    {
        PIECE_TYPE val = PIECE_TYPE.DEAD_RIGHT;
        if (pattern == deadendLeft)
            val = PIECE_TYPE.DEAD_LEFT;
        else if (pattern == deadendTop)
            val = PIECE_TYPE.DEAD_UPSIDE_DOWN;
        else if (pattern == deadendBottom)
            val = PIECE_TYPE.DEADEAND;
        Debug.Log("got deadend piece " + val + "for pattern: " + pattern[0] + ", " + pattern[1] + ", " + pattern[2] + ", " + pattern[3] + ", " + pattern[4] + ", " + pattern[5] + ", " + pattern[6] + ", " + pattern[7] + ", " + pattern[8]);
        return val;
    }

    private Quaternion GetJunctionRotation(int[] pattern)
    {
        Quaternion val = Quaternion.identity;
        if (pattern == junctionLeft)
            val = Quaternion.Euler(0, 180, 0);
        else if (pattern == junctionTop)
            val = Quaternion.Euler(0, -90, 0);
        else if (pattern == junctionBottom)
            val = Quaternion.Euler(0, 90, 0);
        return val;
    }

    private PIECE_TYPE GetJunctionPieceType(int[] pattern)
    {
        PIECE_TYPE val = PIECE_TYPE.T_RIGHT;
        if (pattern == junctionLeft)
            val = PIECE_TYPE.T_LEFT;
        else if (pattern == junctionTop)
            val = PIECE_TYPE.T_UPSIDE_DOWN;
        else if (pattern == junctionBottom)
            val = PIECE_TYPE.T_JUNCTION;
        Debug.Log("got junction piece " + val + "for pattern: " + pattern[0] + ", " + pattern[1] + ", " + pattern[2] + ", " + pattern[3] + ", " + pattern[4] + ", " + pattern[5] + ", " + pattern[6] + ", " + pattern[7] + ", " + pattern[8]);
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
