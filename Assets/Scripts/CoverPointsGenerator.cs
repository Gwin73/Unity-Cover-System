using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static System.Linq.Enumerable;

public class CoverPointsGenerator{
    private struct Edge
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
    }

    private struct Triangle
    {
        public readonly Edge[] edges;

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            edges = new[] { new Edge(v1, v2), new Edge(v2, v3), new Edge(v3, v1) };
        }

        public bool Contains(Edge edge) =>
            edges.Any(e => e.Equals(edge));
    }

    public GameObject CoverPoint;
    public Transform CoverPointParent;
    public float CoverPointsDistance = 3;
    public float MaxInnerEdgeLength = 20;

    public void Generate()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        var triangles = GetNavMeshTriangles(NavMesh.CalculateTriangulation());
        var edges = GetMeshEdges(triangles);
        var inTrianglesCount = InTrianglesCount(edges, triangles);

        var filtered = edges
            .Select((e, i) => new { edge = e, count = inTrianglesCount[i]})
            .Where(p => p.count == 1)
            .Select(p => p.edge)
            .Where(e => e.Length < MaxInnerEdgeLength) //Assume that outer edges are longest.
            .ToList();

        PlacePoints(filtered);

        sw.Stop();
        //UnityEngine.Debug.Log("Total: " + sw.ElapsedMilliseconds);
    }

    public void Remove() =>
        GameObject.FindGameObjectsWithTag(CoverPoint.tag).ToList().ForEach(GameObject.DestroyImmediate);
    
    private IEnumerable<Triangle> GetNavMeshTriangles(NavMeshTriangulation nmt) =>
        nmt.indices
            .Select((e, i) => new { vertexIndex = e, index = i })
            .GroupBy(x => x.index / 3, x => x.vertexIndex)
            .Select(grp => grp.Select(vi => nmt.vertices[vi]).ToList())
            .Select(l => new Triangle(l[0], l[1], l[2]));

    private IEnumerable<Edge> GetMeshEdges(IEnumerable<Triangle> triangles) =>
        triangles
             .SelectMany(t => t.edges)
             .Distinct();

    private List<int> InTrianglesCount(IEnumerable<Edge> edges, IEnumerable<Triangle> triangles) =>
        edges
            .Select(e => triangles.Count(t => t.Contains(e)))
            .ToList();

    private void PlacePoints(List<Edge> edges) =>
        edges.ForEach(e  =>
        {
            if (e.Length >= 1)
            {
                var pointCount = (int) Math.Ceiling(e.Length / CoverPointsDistance);
                var offset = e.Length / (pointCount + 1);
                var d = (e.w - e.v).normalized;
                Range(1, pointCount).ToList().ForEach(i =>
                {
                   var go = GameObject.Instantiate(CoverPoint, e.v + i*offset*d, Quaternion.LookRotation(Quaternion.Euler(0, -90, 0) * d), CoverPointParent);
                   Undo.RecordObject(go, "Instatiating cover-points");
                });
            }
        });
}