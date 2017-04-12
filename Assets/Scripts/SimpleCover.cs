using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SimpleCover : MonoBehaviour {
    public float CoverLeanDistance = 1;
    
    private Transform player;

    private void Start() =>
        player = GameObject.FindGameObjectWithTag("Player").transform;

    public Vector3 GetNearestShootableCover(Vector3 point) =>
       GetShootableCovers() //No elements
           .Aggregate((agg, next) => //TODO: 1 Use shortest path instead + listempty crash 
                   Vector3.Distance(next, point) < Vector3.Distance(agg, point) ? next : agg);

    public IEnumerable<Transform> GetCovers() => 
        GameObject.FindGameObjectsWithTag("CoverPoint") //TODO: Cache?
            .Select(g => g.transform)
            .Where(t => !PlayerLOS(t.position))
            .Where(t => Reachable(transform.position, t.position));

    private bool PlayerLOS(Vector3 u) =>
        Physics.Raycast(u, player.position - u, out RaycastHit hit) && hit.collider.CompareTag("Player");

    private bool Reachable(Vector3 from, Vector3 to) => 
        NavMesh.CalculatePath(from, to, NavMesh.AllAreas, new NavMeshPath());

    public IEnumerable<Vector3> GetShootableCovers() =>
        GetCovers()
            .Where(PlayerShootable)
            .Select(t => t.position);

    private bool PlayerShootable(Transform t) =>
        new List<Vector3>() { CoverLeanDistance *- t.right, CoverLeanDistance* t.right }
            .Any(d => PlayerLOS(t.position + d));
}
