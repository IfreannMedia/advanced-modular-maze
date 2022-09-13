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

    public Vector2 ToVector()
    {
        return new Vector2(x, z);
    }

    public static MapLocation operator +(MapLocation a, MapLocation b)
       => new MapLocation(a.x + b.x, a.z + b.z);

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return x == ((MapLocation)obj).x && z == ((MapLocation)obj).z;
        }

    }

    public override int GetHashCode()
    {
        return 0;
    }

}

public class Maze : MonoBehaviour
{
    public List<MapLocation> directions = new List<MapLocation>() {
                                            new MapLocation(1,0),
                                            new MapLocation(0,1),
                                            new MapLocation(-1,0),
                                            new MapLocation(0,-1) };

    public List<MapLocation> pillarLocations = new List<MapLocation>();

    public int width = 30; //x length
    public int depth = 30; //z length
    public byte[,] map;
    public int scale = 6;
    public int level = 0;
    public float levelDistance = 2f;

    [System.Serializable]
    public struct Module
    {
        public GameObject prefab;
        public Vector3 rotation;
    }

    public Module VerticalStraight;
    public Module HorizontalStraight;
    public Module Crossroad;
    public Module RightUpCorner;
    public Module RightDownCorner;
    public Module LeftUpCorner;
    public Module LeftDownCorner;
    public Module tIntersection;
    public Module tIntersectionUpsideDown;
    public Module tIntersectionLeft;
    public Module tIntersectionRight;
    public Module Endpiece;
    public Module EndpieceUpsideDown;
    public Module EndpieceRight;
    public Module EndpieceLeft;
    public Module WallpieceTop;
    public Module WallpieceBottom;
    public Module WallpieceRight;
    public Module WallpieceLeft;
    public Module Floorpiece;
    public Module Ceilingpiece;

    public Module Pillar;
    public Module DoorTop;
    public Module DoorBottom;
    public Module DoorRight;
    public Module DoorLeft;


    public GameObject FPC;

    public enum PieceType
    {
        Horizontal_Straight,
        Vertical_Straight,
        Right_Up_Corner,
        Right_Down_Corner,
        Left_Up_Corner,
        Left_Down_Corner,
        T_Junction,
        TUpsideDown,
        TToLeft,
        TToRight,
        DeadEnd,
        DeadUpsideDown,
        DeadToRight,
        DeadToLeft,
        Wall,
        Crossroad,
        Room,
        Manhole
    }

    public struct Pieces
    {
        public PieceType piece;
        public GameObject model;

        public Pieces(PieceType pt, GameObject m)
        {
            piece = pt;
            model = m;
        }
    }

    public Pieces[,] piecePlaces;


    public void Build()
    {
        InitialiseMap();
        Generate();
        AddRooms(5, 4, 10);
        DrawMap();
        PlaceFPC();
    }

    public virtual void AddRooms(int count, int minSize, int maxSize)
    {

    }

    void InitialiseMap()
    {
        map = new byte[width,depth];
        piecePlaces = new Pieces[width, depth];
        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                    map[x, z] = 1;     //1 = wall  0 = corridor
            }
    }

    public virtual void PlaceFPC()
    {
        if (!FPC) return;
        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                if (map[x, z] == 0)
                {
                    FPC.transform.position = new Vector3(x * scale, level, z * scale);
                    return;
                }
            }
    }

    public virtual void Generate()
    {
        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
               if(Random.Range(0,100) < 50)
                 map[x, z] = 0;     //1 = wall  0 = corridor
            }
    }

    void DrawMap()
    {
        int height = (int)(level * scale * levelDistance);
        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                if (map[x, z] == 1)
                {
                    //Vector3 pos = new Vector3(x * scale, 0, z * scale);
                    //GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //wall.transform.localScale = new Vector3(scale, scale, scale);
                    // wall.transform.position = pos;

                    piecePlaces[x, z].piece = PieceType.Wall;
                    piecePlaces[x, z].model = null;
                }
                else if (Search2D(x, z, new int[] { 5, 1, 5, 0, 0, 1, 5, 1, 5 })) //horizontal end piece -|
                {
                    GameObject block = Instantiate(EndpieceRight.prefab);
                    block.transform.position = new Vector3(x * scale, height, z * scale);
                    block.transform.Rotate(EndpieceRight.rotation);
                    block.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.DeadToRight;
                    piecePlaces[x, z].model = block;

                }
                else if (Search2D(x, z, new int[] { 5, 1, 5, 1, 0, 0, 5, 1, 5 })) //horizontal end piece |-
                {
                    GameObject block = Instantiate(EndpieceLeft.prefab);
                    block.transform.position = new Vector3(x * scale, height, z * scale);
                    block.transform.Rotate(EndpieceLeft.rotation);
                    block.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.DeadToLeft;
                    piecePlaces[x, z].model = block;

                }
                else if (Search2D(x, z, new int[] { 5, 1, 5, 1, 0, 1, 5, 0, 5 })) //vertical end piece T
                {
                    GameObject block = Instantiate(Endpiece.prefab);
                    block.transform.position = new Vector3(x * scale, height, z * scale);
                    block.transform.Rotate(Endpiece.rotation);
                    block.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.DeadEnd;
                    piecePlaces[x, z].model = block;
                }
                else if (Search2D(x, z, new int[] { 5, 0, 5, 1, 0, 1, 5, 1, 5 })) //vertical end piece upside downT
                {
                    GameObject block = Instantiate(EndpieceUpsideDown.prefab);
                    block.transform.position = new Vector3(x * scale, height, z * scale);
                    block.transform.Rotate(EndpieceUpsideDown.rotation);
                    block.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.DeadUpsideDown;
                    piecePlaces[x, z].model = block;
                }
                else if (Search2D(x, z, new int[] { 5, 0, 5, 1, 0, 1, 5, 0, 5 })) //vertical straight
                {
                    Vector3 pos = new Vector3(x * scale, height, z * scale);
                    GameObject go = Instantiate(VerticalStraight.prefab, pos, Quaternion.identity);
                    go.transform.Rotate(VerticalStraight.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.Vertical_Straight;
                    piecePlaces[x, z].model = go;
                }
                else if (Search2D(x, z, new int[] { 5, 1, 5, 0, 0, 0, 5, 1, 5 })) //horizontal straight
                {
                    Vector3 pos = new Vector3(x * scale, height, z * scale);
                    GameObject go = Instantiate(HorizontalStraight.prefab, pos, Quaternion.identity);
                    go.transform.Rotate(HorizontalStraight.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.Horizontal_Straight;
                    piecePlaces[x, z].model = go;

                }
                else if (Search2D(x, z, new int[] { 1, 0, 1, 0, 0, 0, 1, 0, 1 })) //crossroad
                {
                    GameObject go = Instantiate(Crossroad.prefab);
                    go.transform.position = new Vector3(x * scale, height, z * scale);
                    go.transform.Rotate(Crossroad.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.Crossroad;
                    piecePlaces[x, z].model = go;
                }
                else if (Search2D(x, z, new int[] { 5, 1, 5, 0, 0, 1, 1, 0, 5 })) //upper left corner
                {
                    GameObject go = Instantiate(LeftUpCorner.prefab);
                    go.transform.position = new Vector3(x * scale, height, z * scale);
                    go.transform.Rotate(LeftUpCorner.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.Left_Up_Corner;
                    piecePlaces[x, z].model = go;
                }
                else if (Search2D(x, z, new int[] { 5, 1, 5, 1, 0, 0, 5, 0, 1 })) //upper right corner
                {
                    GameObject go = Instantiate(RightUpCorner.prefab);
                    go.transform.position = new Vector3(x * scale, height, z * scale);
                    go.transform.Rotate(RightUpCorner.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.Right_Up_Corner;
                    piecePlaces[x, z].model = go;
                }
                else if (Search2D(x, z, new int[] { 5, 0, 1, 1, 0, 0, 5, 1, 5 })) //lower right corner
                {
                    GameObject go = Instantiate(RightDownCorner.prefab);
                    go.transform.position = new Vector3(x * scale, height, z * scale);
                    go.transform.Rotate(RightDownCorner.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.Right_Down_Corner;
                    piecePlaces[x, z].model = go;
                }
                else if (Search2D(x, z, new int[] { 1, 0, 5, 5, 0, 1, 5, 1, 5 })) //lower left corner
                {
                    GameObject go = Instantiate(LeftDownCorner.prefab);
                    go.transform.position = new Vector3(x * scale, height, z * scale);
                    go.transform.Rotate(LeftDownCorner.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.Left_Down_Corner;
                    piecePlaces[x, z].model = go;
                }
                else if (Search2D(x, z, new int[] { 1, 0, 1, 0, 0, 0, 5, 1, 5 })) //tjunc  upsidedown T
                {
                    GameObject go = Instantiate(tIntersectionUpsideDown.prefab);
                    go.transform.position = new Vector3(x * scale, height, z * scale);
                    go.transform.Rotate(tIntersectionUpsideDown.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.TUpsideDown;
                    piecePlaces[x, z].model = go;
                }
                else if (Search2D(x, z, new int[] { 5, 1, 5, 0, 0, 0, 1, 0, 1 })) //tjunc  T
                {
                    GameObject go = Instantiate(tIntersection.prefab);
                    go.transform.position = new Vector3(x * scale, height, z * scale);
                    go.transform.Rotate(tIntersection.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.T_Junction;
                    piecePlaces[x, z].model = go;
                }
                else if (Search2D(x, z, new int[] { 1, 0, 5, 0, 0, 1, 1, 0, 5 })) //tjunc  -|
                {
                    GameObject go = Instantiate(tIntersectionRight.prefab);
                    go.transform.position = new Vector3(x * scale, height, z * scale);
                    go.transform.Rotate(tIntersectionRight.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.TToRight;
                    piecePlaces[x, z].model = go;
                }
                else if (Search2D(x, z, new int[] { 5, 0, 1, 1, 0, 0, 5, 0, 1 })) //tjunc  |-
                {
                    GameObject go = Instantiate(tIntersectionLeft.prefab);
                    go.transform.position = new Vector3(x * scale, height, z * scale);
                    go.transform.Rotate(tIntersectionLeft.rotation);
                    go.transform.SetParent(transform);
                    piecePlaces[x, z].piece = PieceType.TToLeft;
                    piecePlaces[x, z].model = go;
                }
                else if (map[x, z] == 0 && (CountSquareNeighbours(x, z) > 1 && CountDiagonalNeighbours(x, z) >= 1 ||
                                            CountSquareNeighbours(x, z) >= 1 && CountDiagonalNeighbours(x, z) > 1))
                {
                    GameObject floor = Instantiate(Floorpiece.prefab);
                    floor.transform.position = new Vector3(x * scale, height, z * scale);
                    floor.transform.SetParent(transform);

                    GameObject ceiling = Instantiate(Ceilingpiece.prefab);
                    ceiling.transform.position = new Vector3(x * scale, height, z * scale);
                    ceiling.transform.SetParent(transform);

                    piecePlaces[x, z].piece = PieceType.Room;
                    piecePlaces[x, z].model = floor;

                    GameObject pillarCorner;
                    LocateWalls(x, z);
                    if (top)
                    {
                        GameObject wall1 = Instantiate(WallpieceTop.prefab);
                        wall1.transform.position = new Vector3(x * scale, height, z * scale);
                        wall1.transform.Rotate(WallpieceTop.rotation);
                        wall1.name = "Top Wall";
                        wall1.transform.SetParent(transform);

                        if (map[x + 1, z] == 0 && map[x + 1, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x,z)))
                        {
                            pillarCorner = Instantiate(Pillar.prefab);
                            pillarCorner.transform.position = new Vector3(x * scale, height, z * scale);
                            pillarCorner.name = "Top Right";
                            pillarLocations.Add(new MapLocation(x, z));
                            pillarCorner.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                            pillarCorner.transform.SetParent(transform);
                        }

                        if (map[x - 1, z] == 0 && map[x - 1, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z)))
                        {
                            pillarCorner = Instantiate(Pillar.prefab);
                            pillarCorner.transform.position = new Vector3((x-1) * scale, height, z * scale);
                            pillarCorner.name = "Top Left";
                            pillarLocations.Add(new MapLocation(x - 1, z));
                            pillarCorner.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                            pillarCorner.transform.SetParent(transform);
                        }
                    }

                    if (bottom)
                    {
                        GameObject wall2 = Instantiate(WallpieceBottom.prefab);
                        wall2.transform.position = new Vector3(x * scale, height, z * scale);
                        wall2.transform.Rotate(WallpieceBottom.rotation);
                        wall2.name = "Bottom Wall";
                        wall2.transform.SetParent(transform);

                        if (map[x + 1, z] == 0 && map[x + 1, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x, z - 1)))
                        {
                            pillarCorner = Instantiate(Pillar.prefab);
                            pillarCorner.transform.position = new Vector3(x * scale, height, ( z - 1) * scale);
                            pillarCorner.name = "Bottom Right";
                            pillarLocations.Add(new MapLocation(x, z - 1));
                            pillarCorner.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                            pillarCorner.transform.SetParent(transform);
                        }

                        if (map[x - 1, z - 1] == 0 && map[x - 1, z] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z - 1)))
                        {
                            pillarCorner = Instantiate(Pillar.prefab);
                            pillarCorner.transform.position = new Vector3((x - 1) * scale, height, (z - 1) * scale);
                            pillarCorner.name = "Bottom Left";
                            pillarLocations.Add(new MapLocation(x - 1, z - 1));
                            pillarCorner.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                            pillarCorner.transform.SetParent(transform);
                        }
                    }

                    if (right)
                    {
                        GameObject wall3 = Instantiate(WallpieceRight.prefab);
                        wall3.transform.position = new Vector3(x * scale, height, z * scale);
                        wall3.transform.Rotate(WallpieceRight.rotation);
                        wall3.name = "Right Wall";
                        wall3.transform.SetParent(transform);
                        if (map[x + 1, z + 1] == 0 && map[x, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x, z - 1)))
                        {
                            pillarCorner = Instantiate(Pillar.prefab);
                            pillarCorner.transform.position = new Vector3(x * scale, height, (z - 1) * scale);
                            pillarCorner.name = "Right Top";
                            pillarLocations.Add(new MapLocation(x, z - 1));
                            pillarCorner.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                            pillarCorner.transform.SetParent(transform);
                        }

                        if (map[x, z - 1] == 0 && map[x + 1, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x + 1, z - 1)))
                        {
                            pillarCorner = Instantiate(Pillar.prefab);
                            pillarCorner.transform.position = new Vector3((x + 1) * scale, height, (z - 1) * scale);
                            pillarCorner.name = "Right Bottom";
                            pillarLocations.Add(new MapLocation(x + 1, z - 1));
                            pillarCorner.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                            pillarCorner.transform.SetParent(transform);
                        }
                    }

                    if (left)
                    {
                        GameObject wall4 = Instantiate(WallpieceLeft.prefab);
                        wall4.transform.position = new Vector3(x * scale, height, z * scale);
                        wall4.transform.Rotate(WallpieceLeft.rotation);
                        wall4.name = "Left Wall";
                        wall4.transform.SetParent(transform);
                        if (map[x - 1, z + 1] == 0 && map[x, z + 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z)))
                        {
                            pillarCorner = Instantiate(Pillar.prefab);
                            pillarCorner.transform.position = new Vector3((x - 1) * scale, height, z * scale);
                            pillarCorner.name = "Left Top";
                            pillarLocations.Add(new MapLocation(x - 1, z));
                            pillarCorner.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                            pillarCorner.transform.SetParent(transform);
                        }

                        if (map[x - 1, z - 1] == 0 && map[x, z - 1] == 0 && !pillarLocations.Contains(new MapLocation(x - 1, z - 1)))
                        {
                            pillarCorner = Instantiate(Pillar.prefab);
                            pillarCorner.transform.position = new Vector3((x - 1) * scale, height, (z - 1) * scale);
                            pillarCorner.name = "Left Bottom";
                            pillarLocations.Add(new MapLocation(x - 1, z - 1));
                            pillarCorner.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                            pillarCorner.transform.SetParent(transform);
                        }
                    }


                }
            }

        for (int z = 0; z < depth; z++)
            for (int x = 0; x < width; x++)
            {
                if (piecePlaces[x, z].piece != PieceType.Room) continue;
                GameObject doorway;
                LocateDoors(x, z);
                if (top)
                {
                    doorway = Instantiate(DoorTop.prefab);
                    doorway.transform.position = new Vector3(x * scale, height, z * scale);
                    doorway.transform.Rotate(DoorTop.rotation);
                    doorway.name = "Top Doorway";
                    doorway.transform.Translate(0, 0, 0.01f);
                    doorway.transform.SetParent(transform);
                }
                if (bottom)
                {
                    doorway = Instantiate(DoorBottom.prefab);
                    doorway.transform.position = new Vector3(x * scale, height, z * scale);
                    doorway.transform.Rotate(DoorBottom.rotation);
                    doorway.name = "Bottom Doorway";
                    doorway.transform.Translate(0, 0, 0.01f);
                    doorway.transform.SetParent(transform);
                }
                if (left)
                {
                    doorway = Instantiate(DoorLeft.prefab);
                    doorway.transform.position = new Vector3(x * scale, height, z * scale);
                    doorway.transform.Rotate(DoorLeft.rotation);
                    doorway.name = "Left Doorway";
                    doorway.transform.Translate(0, 0, 0.01f);
                    doorway.transform.SetParent(transform);
                }
                if (right)
                {
                    doorway = Instantiate(DoorRight.prefab);
                    doorway.transform.position = new Vector3(x * scale, height, z * scale);
                    doorway.transform.Rotate(DoorRight.rotation);
                    doorway.name = "Right Doorway";
                    doorway.transform.Translate(0, 0, 0.01f);
                    doorway.transform.SetParent(transform);
                }
            }
    }

    bool top;
    bool bottom;
    bool right;
    bool left;

    public void LocateWalls(int x, int z)
    {
        top = false;
        bottom = false;
        right = false;
        left = false;

        if (x <= 0 || x >= width - 1 || z <= 0 || z >= depth - 1) return;
        if (map[x, z + 1] == 1) top = true;
        if (map[x, z - 1] == 1) bottom = true;
        if (map[x + 1, z] == 1) right = true;
        if (map[x - 1, z] == 1) left = true;
    }

    public void LocateDoors(int x, int z)
    {
        top = false;
        bottom = false;
        right = false;
        left = false;

        if (x <= 0 || x >= width - 1 || z <= 0 || z >= depth - 1) return;
        if (piecePlaces[x, z + 1].piece != PieceType.Room && piecePlaces[x, z + 1].piece != PieceType.Wall) top = true;
        if (piecePlaces[x, z - 1].piece != PieceType.Room && piecePlaces[x, z - 1].piece != PieceType.Wall) bottom = true;
        if (piecePlaces[x + 1, z].piece != PieceType.Room && piecePlaces[x + 1, z].piece != PieceType.Wall) right = true;
        if (piecePlaces[x - 1, z].piece != PieceType.Room && piecePlaces[x - 1, z].piece != PieceType.Wall) left = true;
    }

    bool IsRoom(int x, int z)
    {
        return (CountSquareNeighbours(x, z) > 1 && CountDiagonalNeighbours(x, z) >= 1 ||
                                            CountSquareNeighbours(x, z) >= 1 && CountDiagonalNeighbours(x, z) > 1);
    }


    bool Search2D(int c, int r, int[] pattern)
    {
        int count = 0;
        int pos = 0;
        for (int z = 1; z > -2; z--)
        {
            for (int x = -1; x < 2; x++)
            {
                if (pattern[pos] == map[c + x, r + z] || pattern[pos] == 5)
                    count++;
                pos++;
            }
        }
        return (count == 9);
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
        return CountSquareNeighbours(x,z) + CountDiagonalNeighbours(x,z);
    }
}
