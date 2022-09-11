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

    [System.Serializable]
    public struct Module
    {
        [HideInInspector] public PIECE_TYPE pieceType;
        public GameObject prefab;
        public Vector3 rotation;
    }

    public Module verticalStraight;
    public Module horizontalStraight;
    public Module crossroad;
    public Module rightUpCorner;
    public Module rightDownCorner;
    public Module leftUpCorner;
    public Module leftDownCorner;
    public Module tJunctionDown;
    public Module tJunctionLeft;
    public Module tJunctionRight;
    public Module tJunctionUp;
    public Module endUp;
    public Module endDown;
    public Module endRight;
    public Module endLeft;
    public Module wallTop;
    public Module wallBottom;
    public Module wallLeft;
    public Module wallRight;
    public Module floor;
    public Module cieling;
    public Module pillar;
    public Module doorTop;
    public Module doorBottom;
    public Module doorRight;
    public Module doorLeft;

    public FPController player;

    // patterns
    private int[] verticalStraightPattern = new int[] { 5, 0, 5, 1, 0, 1, 5, 0, 5 };
    private int[] horizontalStraightPattern = new int[] { 5, 1, 5, 0, 0, 0, 5, 1, 5 };
    private int[] crossroadsPattern = new int[] { 1, 0, 1, 0, 0, 0, 1, 0, 1 };

    private int[] junctionTopPattern = new int[] { 1, 0, 1, 0, 0, 0, 5, 1, 5 }; // entering top
    private int[] junctionBottomPattern = new int[] { 5, 1, 5, 0, 0, 0, 1, 0, 1 }; // entering bottom
    private int[] junctionLeftPattern = new int[] { 1, 0, 5, 0, 0, 1, 1, 0, 5 }; // entering left
    private int[] junctionRightPattern = new int[] { 5, 0, 1, 1, 0, 0, 5, 0, 1 }; // entering right

    private int[] deadendTopPattern = new int[] { 5, 0, 5, 1, 0, 1, 5, 1, 5 };
    private int[] deadendLeftPattern = new int[] { 5, 1, 5, 0, 0, 1, 5, 1, 5 };
    private int[] deadendBottomPattern = new int[] { 5, 1, 5, 1, 0, 1, 5, 0, 5 };
    private int[] deadendRightPattern = new int[] { 5, 1, 5, 1, 0, 0, 5, 1, 5 };

    private int[] cornerLeftTopPattern = new int[] { 5, 1, 5, 1, 0, 0, 5, 0, 1 };
    private int[] cornerLeftBottomPattern = new int[] { 5, 0, 1, 1, 0, 0, 5, 1, 5 };
    private int[] cornerRightBottomPattern = new int[] { 1, 0, 5, 5, 0, 1, 5, 1, 5 };
    private int[] cornerRightTopPattern = new int[] { 5, 1, 5, 0, 0, 1, 1, 0, 5 };

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
        ROOM,
        ROOM_WALL,
        ROOM_DOORWAY,
        FLOOR,
        CIELING,
        PILLAR
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

    void Start()
    {
        SetPieceTypes();
        InitialiseMap();
        Generate();
        AddRooms(3, 4, 6);
        DrawMap();
        PlaceFPC();
    }

    public virtual void SetPieceTypes()
    {
        verticalStraight.pieceType = PIECE_TYPE.VERTICAL_STRAIGHT;
        horizontalStraight.pieceType = PIECE_TYPE.HORIZONTAL_STRAIGHT;
        crossroad.pieceType = PIECE_TYPE.CROSSROAD;
        rightUpCorner.pieceType = PIECE_TYPE.RIGHT_UP_CORNER;
        rightDownCorner.pieceType = PIECE_TYPE.RIGHT_DOWN_CORNER;
        leftUpCorner.pieceType = PIECE_TYPE.LEFT_UP_CORNER;
        leftDownCorner.pieceType = PIECE_TYPE.LEFT_DOWN_CORNER;
        tJunctionDown.pieceType = PIECE_TYPE.T_JUNCTION;
        tJunctionLeft.pieceType = PIECE_TYPE.T_LEFT;
        tJunctionRight.pieceType = PIECE_TYPE.T_RIGHT;
        tJunctionUp.pieceType = PIECE_TYPE.T_UPSIDE_DOWN;
        endUp.pieceType = PIECE_TYPE.DEAD_UPSIDE_DOWN;
        endDown.pieceType = PIECE_TYPE.DEADEAND;
        endRight.pieceType = PIECE_TYPE.DEAD_RIGHT;
        endLeft.pieceType = PIECE_TYPE.DEAD_LEFT;
        wallTop.pieceType = PIECE_TYPE.ROOM_WALL;
        wallBottom.pieceType = PIECE_TYPE.ROOM_WALL;
        wallLeft.pieceType = PIECE_TYPE.ROOM_WALL;
        wallRight.pieceType = PIECE_TYPE.ROOM_WALL;
        floor.pieceType = PIECE_TYPE.FLOOR;
        cieling.pieceType = PIECE_TYPE.CIELING;
        pillar.pieceType = PIECE_TYPE.PILLAR;
        doorTop.pieceType = PIECE_TYPE.ROOM_DOORWAY;
        doorBottom.pieceType = PIECE_TYPE.ROOM_DOORWAY;
        doorRight.pieceType = PIECE_TYPE.ROOM_DOORWAY;
        doorLeft.pieceType = PIECE_TYPE.ROOM_DOORWAY;
    }

    public virtual void AddRooms(int count, int minSize, int maxSize)
    {
        this.endDown.pieceType = PIECE_TYPE.DEADEAND;
        for (int c = 0; c < count; c++)
        {
            int startX = Random.Range(3, width - 3);
            int startZ = Random.Range(3, depth - 3);
            int roomWidth = Random.Range(minSize, maxSize);
            int roomDepth = Random.Range(minSize, maxSize);

            for (int x = startX; x < width - 3 && x < startX + roomWidth; x++)
            {
                for (int z = startZ; z < depth - 3 && z < startZ + roomDepth; z++)
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
                        GameObject door = Instantiate(doorTop.prefab);
                        door.transform.position = new Vector3(x * scale, 0, z * scale);
                        door.name = "door-top";
                        //door.transform.rotation = Quaternion.Euler(0, 90, 0);
                        door.transform.rotation = Quaternion.Euler(doorTop.rotation);
                        door.transform.Translate(0, 0, 0.01f);
                    }
                    if (ShouldAddDoorBottom(x, z))
                    {
                        GameObject door = Instantiate(doorBottom.prefab);
                        door.transform.position = new Vector3(x * scale, 0, z * scale);
                        door.name = "door-bottom";
                        door.transform.rotation = Quaternion.Euler(doorBottom.rotation);
                        //door.transform.rotation = Quaternion.Euler(0, -90, 0);
                        door.transform.Translate(0, 0, -0.01f);
                    }
                    if (ShouldAddDoorRight(x, z))
                    {
                        GameObject door = Instantiate(doorRight.prefab);
                        door.transform.position = new Vector3(x * scale, 0, z * scale);
                        door.name = "door-right";
                        door.transform.rotation = Quaternion.Euler(doorRight.rotation);
                        //door.transform.rotation = Quaternion.Euler(0, 180, 0);
                        door.transform.Translate(-0.01f, 0, 0);
                    }
                    if (ShouldAddDoorLeft(x, z))
                    {
                        GameObject door = Instantiate(doorLeft.prefab);
                        door.transform.position = new Vector3(x * scale, 0, z * scale);
                        door.transform.rotation = Quaternion.Euler(doorLeft.rotation);
                        door.name = "door-left";
                        door.transform.Translate(0.01f, 0, 0);
                    }
                }

            }
    }

    private void PlaceWalkablePiece(int x, int z, Vector3 pos)
    {
        int[] matchedPattern = HasMatchedPattern(x, z, new int[][] { verticalStraightPattern, horizontalStraightPattern,
                        cornerLeftBottomPattern, cornerLeftTopPattern, cornerRightBottomPattern, cornerRightTopPattern,
                        crossroadsPattern,
                        deadendBottomPattern, deadendLeftPattern, deadendRightPattern, deadendTopPattern,
                        junctionBottomPattern, junctionLeftPattern, junctionRightPattern, junctionTopPattern });
        if (matchedPattern == deadendBottomPattern || matchedPattern == deadendLeftPattern || matchedPattern == deadendRightPattern || matchedPattern == deadendTopPattern)
        {
            Module endPiece = GetEndPiece(matchedPattern);
            piecePlaces[x, z] = new Pieces(endPiece.pieceType, Instantiate(endPiece.prefab, pos, Quaternion.Euler(endPiece.rotation)));
        }
        else if (matchedPattern == verticalStraightPattern)
        {
            piecePlaces[x, z] = new Pieces(verticalStraight.pieceType, Instantiate(verticalStraight.prefab, pos, Quaternion.Euler(verticalStraight.rotation)));
        }
        else if (matchedPattern == horizontalStraightPattern)
        {
            piecePlaces[x, z] = new Pieces(horizontalStraight.pieceType, Instantiate(horizontalStraight.prefab, pos, Quaternion.Euler(horizontalStraight.rotation)));
        }
        else if (matchedPattern == crossroadsPattern)
        {
            piecePlaces[x, z] = new Pieces(crossroad.pieceType, Instantiate(crossroad.prefab, pos, Quaternion.Euler(crossroad.rotation)));
        }
        else if (matchedPattern == cornerLeftBottomPattern || matchedPattern == cornerLeftTopPattern || matchedPattern == cornerRightBottomPattern || matchedPattern == cornerRightTopPattern)
        {
            Module cornerPiece = GetCornerPiece(matchedPattern);
            piecePlaces[x, z] = new Pieces(cornerPiece.pieceType, Instantiate(cornerPiece.prefab, pos, Quaternion.Euler(cornerPiece.rotation)));

        }
        else if (matchedPattern == junctionBottomPattern || matchedPattern == junctionLeftPattern || matchedPattern == junctionRightPattern || matchedPattern == junctionTopPattern)
        {
            Module junctionPiece = GetJunctionPiece(matchedPattern);
            piecePlaces[x, z] = new Pieces(junctionPiece.pieceType, Instantiate(junctionPiece.prefab, pos, Quaternion.Euler(junctionPiece.rotation)));
        }
        else if (!PositionOnMapEdge(x, z) && ShouldAddRoomFloorPiece(x, z))
        {
            piecePlaces[x, z] = new Pieces(floor.pieceType, Instantiate(floor.prefab, pos, Quaternion.Euler(floor.rotation)));
            Instantiate(cieling.prefab, pos, Quaternion.Euler(cieling.rotation));
            GameObject wall;
            GameObject plr;
            // walls and pillars
            if (ShouldAddWallTop(x, z))
            {
                wall = Instantiate(wallTop.prefab, pos, Quaternion.Euler(wallTop.rotation));
                wall.name = "wall-top";
                if (map[x + 1, z] == 0 && map[x + 1, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x, z)))
                {
                    plr = Instantiate(pillar.prefab);
                    plr.transform.position = new Vector3(x * scale, 0, z * scale);
                    plr.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    plr.name = "pillar-top-right";
                    pillarLocations.Add(new MapLocation(x, z));
                }
                if (map[x - 1, z] == 0 && map[x - 1, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z)))
                {
                    plr = Instantiate(pillar.prefab);
                    plr.transform.position = new Vector3((x - 1) * scale, 0, z * scale);
                    plr.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    plr.name = "pillar-top-left";
                    pillarLocations.Add(new MapLocation(x - 1, z));
                }
            }
            if (ShouldAddWallLeft(x, z))
            {
                wall = Instantiate(wallLeft.prefab, pos, Quaternion.Euler(wallLeft.rotation));
                wall.name = "wall-left";
                if (map[x, z + 1] == 0 && map[x - 1, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z)))
                {
                    plr = Instantiate(pillar.prefab);
                    plr.transform.position = new Vector3((x - 1) * scale, 0, z * scale);
                    plr.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    plr.name = "pillar-left-top";
                    pillarLocations.Add(new MapLocation(x - 1, z));
                }
                if (map[x, z - 1] == 0 && map[x - 1, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z - 1)))
                {
                    plr = Instantiate(pillar.prefab);
                    plr.transform.position = new Vector3((x - 1) * scale, 0, (z - 1) * scale);
                    plr.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    plr.name = "pillar-left-bottom";
                    pillarLocations.Add(new MapLocation(x - 1, z - 1));
                }
            }
            if (ShouldAddWallRight(x, z))
            {
                wall = Instantiate(wallRight.prefab, pos, Quaternion.Euler(wallRight.rotation));
                wall.name = "wall-right";
                if (map[x, z + 1] == 0 && map[x + 1, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x, z)))
                {
                    plr = Instantiate(pillar.prefab);
                    plr.transform.position = new Vector3(x * scale, 0, z * scale);
                    plr.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    plr.name = "pillar-right-top";
                    pillarLocations.Add(new MapLocation(x, z));
                }
                if (map[x, z - 1] == 0 && map[x + 1, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x, z - 1)))
                {
                    plr = Instantiate(pillar.prefab);
                    plr.transform.position = new Vector3(x * scale, 0, (z - 1) * scale);
                    plr.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    plr.name = "pillar-right-bottom";
                    pillarLocations.Add(new MapLocation(x, z - 1));
                }
            }
            if (ShouldAddWallBottom(x, z))
            {
                wall = Instantiate(wallBottom.prefab, pos, Quaternion.Euler(wallBottom.rotation));
                wall.name = "wall-bottom";
                if (map[x + 1, z] == 0 && map[x + 1, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x, z - 1)))
                {
                    plr = Instantiate(pillar.prefab);
                    plr.transform.position = new Vector3(x * scale, 0, (z - 1) * scale);
                    plr.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    plr.name = "pillar-bottom-right";
                    pillarLocations.Add(new MapLocation(x, z - 1));
                }
                if (map[x - 1, z] == 0 && map[x - 1, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z - 1)))
                {
                    plr = Instantiate(pillar.prefab);
                    plr.transform.position = new Vector3((x - 1) * scale, 0, (z - 1) * scale);
                    plr.transform.localScale = new Vector3(1.01f, 1.00f, 1.01f);
                    plr.name = "pillar-bottom-left";
                    pillarLocations.Add(new MapLocation(x - 1, z - 1));
                }
            }
        }
    }

    private Module GetEndPiece(int[] pattern)
    {
        Module module = endRight;
        if (pattern == deadendLeftPattern)
            module = endLeft;
        else if (pattern == deadendTopPattern)
            module = endDown;
        else if (pattern == deadendBottomPattern)
            module = endUp;
        return module;
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

    private Module GetCornerPiece(int[] pattern)
    {
        Module module = leftDownCorner;
        if (pattern == cornerLeftTopPattern)
            module = leftUpCorner;
        else if (pattern == cornerRightTopPattern)
            module = rightUpCorner;
        else if (pattern == cornerRightBottomPattern)
            module = rightDownCorner;
        return module;
    }

    private Module GetJunctionPiece(int[] pattern)
    {
        Module module = tJunctionRight;
        if (pattern == junctionLeftPattern)
            module = tJunctionLeft;
        else if (pattern == junctionTopPattern)
            module = tJunctionDown;
        else if (pattern == junctionBottomPattern)
            module = tJunctionUp;
        return module;
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
