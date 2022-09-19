using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FindPathAStar))]
public class FindPath : MonoBehaviour
{

    public GameObject particles;
    private Maze maze;
    private FindPathAStar findPathAStar;
    private GameObject magic;
    PathMarker destination;

    private void Start()
    {
        findPathAStar = GetComponent<FindPathAStar>();
    }

    IEnumerator DisplayMagic()
    {
        List<MapLocation> magicPath = new List<MapLocation>();
        while (destination != null)
        {
            magicPath.Add(new MapLocation(destination.location.x, destination.location.z));
            destination = destination.parent;
        }

        magicPath.Reverse();
        foreach (MapLocation location in magicPath)
        {
            magic.transform.LookAt(maze.piecePlaces[location.x, location.z].model.transform.position + new Vector3(0, 1, 0));

            int loopTimmeout = 0;
            while (Vector2.Distance(new Vector2(magic.transform.position.x, magic.transform.position.z),
                new Vector2(maze.piecePlaces[location.x, location.z].model.transform.position.x,
                                        maze.piecePlaces[location.x, location.z].model.transform.position.z)) > 2
                                        && loopTimmeout < 100)
            {
                loopTimmeout++;
                magic.transform.Translate(0, 0, 10f * Time.deltaTime);
                yield return new WaitForSeconds(0.01f);
            }
        }
        Destroy(magic, 10);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (findPathAStar != null)
            {
                RaycastHit hit;
                Ray ray = new Ray(transform.position, -Vector3.up);
                if (Physics.Raycast(ray, out hit))
                {
                    maze = hit.collider.gameObject.GetComponentInParent<Maze>();
                    MapLoc location = hit.collider.gameObject.GetComponentInParent<MapLoc>();

                    MapLocation current = new MapLocation(location.x, location.z);

                    destination = findPathAStar.Build(maze, current, maze.exitPoint);
                    magic = Instantiate(particles, transform.position, transform.rotation);
                    StartCoroutine("DisplayMagic");
                }
            }
        }
    }
}
