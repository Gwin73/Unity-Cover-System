using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static System.Linq.Enumerable;

[Serializable]
public class CoverPointsGenerator{
    struct Edge
    {
        public readonly Vector3 V, W;
        public readonly float Length;

        public Edge(Vector3 v, Vector3 w)
        {
            V = v;
            W = w;
            Length = Vector3.Distance(v, w);
        }

        public override bool Equals(object obj)
            => obj is Edge e && ((e.V, e.W).Equals((V, W)) || (e.V, e.W).Equals((W, V)));
    }

    struct Triangle
    {
        public readonly Edge[] Edges;

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Edges = new[] { new Edge(v1, v2), new Edge(v2, v3), new Edge(v3, v1) };
        }
            
        public bool Contains(Edge edge) =>
            Edges.Any(e => e.Equals(edge));
    }

    public GameObject CoverPoint;
    public Transform CoverPointParent;
    public float CoverPointsDistance = 3;
    public float MaxInnerEdgeLength = 20;

    public void Generate()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        var triangles = GetTriangles(NavMesh.CalculateTriangulation());
        var edges = GetEdges(triangles);
        var inTrianglesCount = InTrianglesCount(edges, triangles);

        var filtered = edges
            .Select((e, i) => new { edge = e, count = inTrianglesCount[i]})
            .Where(p => p.count == 1)
            .Select(p => p.edge)
            .ToList();

        PlacePoints(filtered);

        sw.Stop();
        UnityEngine.Debug.Log("Total: " + sw.ElapsedMilliseconds);
    }

    public void Remove() =>
        GameObject.FindGameObjectsWithTag(CoverPoint.tag).ToList().ForEach(Undo.DestroyObjectImmediate);

    IEnumerable<Triangle> GetTriangles(NavMeshTriangulation nmt) =>
        nmt.indices
            .Select((e, i) => new {vertexIndex = e, index = i })
            .GroupBy(x => x.index / 3, x => x.vertexIndex)
            .Select(grp => grp.Select(vi => nmt.vertices[vi]).ToList())
            .Select(l => new Triangle(l[0], l[1], l[2]));

    IEnumerable<Edge> GetEdges(IEnumerable<Triangle> triangles) =>
        triangles
             .SelectMany(t => t.Edges)
             .Distinct();

    List<int> InTrianglesCount(IEnumerable<Edge> edges, IEnumerable<Triangle> triangles) =>
        edges
            .Select(e => triangles.Count(t => t.Contains(e)))
            .ToList();

    void PlacePoints(List<Edge> edges) =>
        edges.ForEach(e  =>
        {
            if (!(e.Length >= 1)) return;

            var pointCount = (int) Math.Ceiling(e.Length / CoverPointsDistance);
            var offset = e.Length / (pointCount + 1);
            var d = (e.W - e.V).normalized;
            Range(1, pointCount).ToList().ForEach(i =>
            {
                var go = GameObject.Instantiate(CoverPoint, e.V + i*offset*d, Quaternion.LookRotation(Quaternion.Euler(0, -90, 0) * d), CoverPointParent);
                Undo.RegisterCreatedObjectUndo(go, "Instatiating cover-points");
            });
        });
}