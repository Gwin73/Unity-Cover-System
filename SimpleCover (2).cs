using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

struct Edge
{
    public readonly Vector3 v, w;
    public readonly float Length;

    public Edge(Vector3 v, Vector3 w)
    {
        this.v = v;
        this.w = w;
        Length = Vector3.Distance(v, w);
    }

    public override bool Equals(object obj) 
        => obj is Edge e && ((e.v, e.w).Equals((v, w)) || (e.v, e.w).Equals((w, v)));
    
    //return obj is Edge e && (v.Equals(e.v) && w.Equals(e.w) || v.Equals(e.w) && w.Equals(e.v));

    /*if(!(obj is Edge))
        return false;
    Edge e = (Edge) obj;
    return v.Equals(e.v) && w.Equals(e.w) || v.Equals(e.w) && w.Equals(e.v); //Snygga till*/
}

struct Triangle
{
    public readonly Vector3 v1,v2,v3;
    public readonly Vector3[] vertices;
    public readonly Edge[] edges;

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
        vertices = new Vector3[] {v1, v2, v3};
        edges = new[] { new Edge(v1, v2), new Edge(v2, v3), new Edge(v3, v1)};
    }

    public bool Contains(Edge edge) 
        => edges.Any(e => e.Equals(edge));

    public bool Contains(Vector3 vertex) 
        => vertices.Any(v => v.Equals(vertex));
}

public class SimpleCover : MonoBehaviour {
    public GameObject CoverPoint;

    private NavMeshAgent agent;
    private Transform player;

    private List<Vector3> coverDebugPositions = new List<Vector3>();
    private List<Vector3> coverShootDebugPositions = new List<Vector3>();
    private bool pointsPlaced = false;

    private void Start()
    {
        agent = gameObject.GetComponentInChildren<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update ()
    {
        var NMT = NavMesh.CalculateTriangulation();
        var indices = NMT.indices;
        var vertices = NMT.vertices;

        var triangles = indices
            .Select((e, i) => new { vertexIndex = e, index = i })
            .GroupBy(x => x.index / 3, x => x.vertexIndex)
            .Select(grp => grp.Select(vi => vertices[vi]).ToList())
            .Select(l => new Triangle(l[0], l[1], l[2]));

        var edges = triangles
             .SelectMany(t => t.edges)
             .Distinct();

        var edgeInTrianglesCount = edges
            .Select(e => triangles.Count(t => t.Contains(e)))
            .ToList();

        var filtered = edges
            .Select((e, i) => new { edge = e, count = edgeInTrianglesCount[i] })
            .Where(p => p.count == 1)
            .Select(p => p.edge)
            .ToList();

        //Antag att edges ytterand längst
        filtered = filtered
            .Where(e => e.Length < 20)
            .ToList();

        
       foreach (var e in filtered)
       {
            Debug.DrawLine(e.v, e.w, Color.red);
       }

        if (!pointsPlaced)
        {
            PlacePoints(filtered.ToArray());
            pointsPlaced = true;
        }

        coverDebugPositions = new List<Vector3>();
         GetCovers().ForEach(c => coverDebugPositions.Add(c.position));
 
         coverShootDebugPositions = new List<Vector3>();
         GetShootableCovers().ForEach(c => coverShootDebugPositions.Add(c));
         
        /*if (Vector3.Angle(player.forward, transform.position - player.position) > 90) //TODO: 5 Also trigger if no line of sight 
            agent.SetDestination(GetNearestCover(player.position, 100));
        else
            agent.SetDestination(GetNearestCover(transform.position, 100));*/
    }

    private void PlacePoints(Edge[] edges)
    {
       const float step = 3;
       foreach(var edge in edges)
        {
           if (edge.Length < 1)
              continue;

            var pointCount = (int) Math.Ceiling(edge.Length / step);
            var offset = edge.Length/(pointCount + 1);

            for (int i = 1; i <= pointCount; i++)
            {
                Instantiate(CoverPoint, edge.v + i * offset * (edge.w - edge.v).normalized, Quaternion.LookRotation(Quaternion.Euler(0, -90, 0)*(edge.w - edge.v))); //.normalized
              
            }
        }

    }






    /*private Vector3 GetNearestCover(Vector3 point, float maxRadius)
    {
        return GetCovers(maxRadius)
            .Aggregate(
                (agg, next) => //TODO: 1 Use shortest path instead
                    Vector3.Distance(next, point) < Vector3.Distance(agg, point) ? next : agg);
    }*/

    private List<Transform> GetCovers()
    {
        return GameObject.FindGameObjectsWithTag("CoverSlot")
            .Select(g => g.transform)
            .Where(t => !PlayerLOS(t.position))
            .Where(t => Reachable(transform.position, t.position))
            .ToList();
    }

    private bool PlayerLOS(Vector3 u)
    {
        var ray = new Ray(u, player.position - u);
        return Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Player");
    }

    private bool Reachable(Vector3 from, Vector3 to)
    {
        return NavMesh.CalculatePath(from, to, NavMesh.AllAreas, new NavMeshPath());
    }

    private List<Vector3> GetShootableCovers()
    {
        return GetCovers()
            .Where(t => PlayerShootable(t))
            .Select(t => t.position)
            .ToList();
    }

    private bool PlayerShootable(Transform t)
    {
        var directions = new List<Vector3>() {-t.right, t.right }; //Antag markern normal med  cover
        return directions.Any(d => PlayerLOS(t.position + d));

        /* var directions = new List<Vector3>() {-t.right, t.right }; //Antag markern normal med  cover
        foreach (var d in directions)
            if (PlayerLOS(t.position + d))
                return true;
        return false;*/
    }

    private void OnDrawGizmos()
    {
        foreach (Vector3 coverPosition in coverDebugPositions)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(coverPosition, 0.2f);
        }

        foreach (Vector3 coverShootPosition in coverShootDebugPositions)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(coverShootPosition, 0.2f);
        }
    }
}



//Imperativt
/* var edgeInTrianglesCount = 0;
 foreach (var triangle in triangles)
 {
     if (triangle.Contains(edges[0]))
         edgeInTrianglesCount++;
 }*/


//Extract edges and triangles from NavMesh
/* var edges = new List<Edge>();
 var triangles = new List<Triangle>();

 var NMT = NavMesh.CalculateTriangulation();
 var indices = NMT.indices;
 var vertices = NMT.vertices;
 for (var i = 0; i < NMT.indices.Length; i += 3)
 {
     var e1 = new Edge(vertices[indices[i]], vertices[indices[i + 1]]);
     var e2 = new Edge(vertices[indices[i]], vertices[indices[i + 2]]);
     var e3 = new Edge(vertices[indices[i + 1]], vertices[indices[i + 2]]);

     edges.Add(e1);
     edges.Add(e2);
     edges.Add(e3);

     triangles.Add(new Triangle(vertices[indices[i]], vertices[indices[i+1]], vertices[indices[i+2]]));
 }
 edges = edges.Distinct().ToList(); //Fulhack använd set istället*/




/*Func<Vector3, List<Vector3>, Vector3> nearestVert = 
    (vertex, verts) =>
        verts
        .Where(v => v != vertex)
        .Aggregate(
            (agg, next) =>
                Vector3.Distance(next, vertex) < Vector3.Distance(agg, vertex) ? next : agg);*/


//Vertexes near a cover
//var filtered = vertices //May not always work
//   .Where(v => Vector3.Distance(v, nearestVert(v, vertices.ToList())) < 7)
//   .ToList();
//Could also now filter for vertices with only 1 connection
//filtered.ForEach(coverDebugPositions.Add);

//Triangle testing
//indices.Skip(3).Take(3).ToList().ForEach(i => coverDebugPositions.Add(vertices[i]));






//Debug.Log(edges.Count);

//Shortest distance
/*var nearestVertex = vertices
    .Where(v => v != vertices[0])
    .Aggregate(
        (agg, next) =>
            Vector3.Distance(next, vertices[0]) < Vector3.Distance(agg, vertices[0]) ? next : agg);
var distance = Vector3.Distance(vertices[0], nearestVertex);
Debug.Log("Distance btw" + vertices[0] + " and " + nearestVertex + " is " + distance);*/

/*var directions = new List<Vector3>() { -t.right, t.right };//Antag markern normal med  cover
    foreach (var d in directions)
    {
        RaycastHit hit;
        Ray ray = new Ray(t.position + d, player.position - (t.position + d));
        if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Player"))//DO NOT RAYCAST PLAYER
            return true;
    }
    return false;*/

/*return GetCovers(maxRadius)
    .Where(t =>
    {
        var directions = new List<Vector3>() { -t.right, t.right };//Antag markern normal med  cover
        foreach (var d in directions)
        {
            RaycastHit hit;
            Ray ray = new Ray(t.position + d, player.position - (t.position + d));
            if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Player"))//DO NOT RAYCAST PLAYER
                return true;
        }
        return false;
    }
    )
    .Select(t => t.position)
    .ToList();*/

/*return GameObject.FindGameObjectsWithTag("CoverSlot")
    .Select(g => g.transform)
    .Where(
        t =>
        {
            RaycastHit hit;
            if (Physics.Raycast(t.position, player.position - t.position, out hit)) //DO NOT RAYCAST PLAYER
                return !hit.collider.CompareTag("Player");
            return false; //Triggas, aldrig ty infinit ray length
        })
    .Where(
        t =>
        {
            return NavMesh.CalculatePath(transform.position, t.position, NavMesh.AllAreas, new NavMeshPath());
        })
    .ToList();*/



/*private List<Vector3> GetCovers(float maxRadius = 100)
{
    return GameObject.FindGameObjectsWithTag("CoverSlot")
        .Where(
            g =>
            {
                RaycastHit hit;
                if (Physics.Raycast(g.transform.position, player.position - g.transform.position, out hit)) //DO NOT RAYCAST PLAYER
                    return !hit.collider.CompareTag("Player");
                return false; //Triggas, aldrig ty infinit ray length
            })
        .Where(
            (g) =>
            {
                return NavMesh.CalculatePath(transform.position, g.transform.position, NavMesh.AllAreas, new NavMeshPath());
            })
        .Select(g => g.transform.position)
        .ToList();
}

private List<Vector3> GetShootableCovers(float maxRadius = 100)
{
    return GetCovers(maxRadius)
        .Where((p) =>
            {
                //Antag markern normal med  cover
                var directions = new List<Vector3>() { -transform.right, transform.right}; //transform up <- ska vara p.transform fast går ej
                foreach (var d in directions)
                {
                    RaycastHit hit;
                    Ray ray = new Ray(p+d, player.position - p);

                    Debug.DrawLine(p + d, player.position, Color.blue);
                    if (Physics.Raycast(ray, out hit))//DO NOT RAYCAST PLAYER
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        )
        .ToList();
}*/
