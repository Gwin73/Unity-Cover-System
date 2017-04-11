using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class DynamicCover : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    private List<Vector3> coverDebugPositions = new List<Vector3>();

    private void Start ()
    {
        agent = gameObject.GetComponentInChildren<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
	
	private void Update ()
	{

	    /*var nm = NavMesh.CalculateTriangulation();
        Mesh mesh = new Mesh();
        mesh.vertices = nm.vertices;
        mesh.triangles = nm.indices;
        AssetDatabase.CreateAsset(mesh, "asd.obj");

        var a = nm.indices
            .GroupBy(x => x)
            .Where(x => x.Count() == 1)
            .Select(x => x.Key)
            .ToList();

        foreach (var v in a)
        {
            coverDebugPositions.Add(nm.vertices[v]);
        }*/


        if (Vector3.Angle(player.forward, transform.position - player.position) > 90) //TODO: 5 Also trigger if no line of sight 
                agent.SetDestination(GetNearestCover(player.position, 100));
            else
                agent.SetDestination(GetNearestCover(transform.position, 100));
    }

    private Vector3 GetNearestCover(Vector3 point, float maxRadius)
    {
        return GetCovers(maxRadius)
            .Aggregate(
                (agg, next) => //TODO: 1 Use shortest path instead
                    Vector3.Distance(next, point) < Vector3.Distance(agg, point) ? next : agg);
    }

    private List<Vector3> GetCovers(float maxRadius) //TODO: 1 Spatial search + 10 long covers multiple coverpositions
    {
        coverDebugPositions = new List<Vector3>();
        List<Vector3> coverPositions = new List<Vector3>();

        GameObject[] covers = GameObject.FindGameObjectsWithTag("Cover");
        foreach (GameObject cover in covers)
        {
            if(Vector3.Distance(cover.transform.position, player.position) >= maxRadius)
                continue;

            Nullable<Vector3> pos = GetHiddenPosition(player.position, cover.transform.position);
            if (pos.HasValue)
            {
                coverPositions.Add(pos.GetValueOrDefault());
                coverDebugPositions.Add(pos.GetValueOrDefault());
            }
        }
        return coverPositions;
    }

    private Nullable<Vector3> GetHiddenPosition(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        Vector3 coverPosition = to;
        for (int i = 0; i < 10; i++)
        {
            coverPosition += 0.25f * direction;
            Vector3 groundPosition = GetGroundPosition(coverPosition);

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(groundPosition, out hit, 0.5f, NavMesh.AllAreas))
                continue;

            if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, new NavMeshPath()))
                return hit.position;
        }
        return null;
    }

    private static Vector3 GetGroundPosition(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, 1 << 8))
            return hit.point;
        if (Physics.Raycast(position, Vector3.up, out hit, 1 << 8))
            return hit.point;
        throw new Exception("No floor found");
    }

    private void OnDrawGizmos()
    {
        foreach (Vector3 coverPosition in coverDebugPositions)
        {
            Gizmos.DrawSphere(coverPosition, 0.2f);
        }
    }

    
    /*private List<Vector3> GetCovers(float maxRadius) //TODO: Spatial search
    {
        coverDebugPositions = new List<Vector3>();
        List<Vector3> coverPositions = new List<Vector3>();

        GameObject[] covers = GameObject.FindGameObjectsWithTag("Cover");
        foreach (GameObject cover in covers)
        {
            if(Vector3.Distance(cover.transform.position, player.position) >= maxRadius)
                continue;
            
            Vector3 direction = (cover.transform.position - player.position).normalized;
            Vector3 coverPosition = cover.transform.position;
            for (int i = 0; i < 10; i++) //TODO: Set maxiterations appropliately
            {
                coverPosition += 0.25f * direction;
                Vector3 floorPosition = getFloorPosition(coverPosition);

                coverDebugPositions.Add(coverPosition);
                coverDebugPositions.Add(floorPosition);

                NavMeshHit hit;
                if (NavMesh.SamplePosition(floorPosition, out hit, 0.5f, NavMesh.AllAreas))
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(gameObject.transform.position, hit.position, NavMesh.AllAreas, path))
                    {
                        coverPositions.Add(hit.position);
                        coverDebugPositions.Add(hit.position);
                    }
                    break;
                }
            }
        }
        return coverPositions;
    }*/
    
    /*List<Vector3> covers = getCovers(maxRadius);
    Vector3 nearest = covers.First();
    float nearestDistance = Vector3.Distance(nearest, player.position);
    foreach (Vector3 cover in covers)
    {
        float tempDist = Vector3.Distance(cover, player.position);
        if (tempDist < nearestDistance)
        {
            nearest = cover;
            nearestDistance = tempDist;
        }
    }
    return nearest;*/
}
