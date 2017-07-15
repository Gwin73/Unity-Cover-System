using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SimpleCover : MonoBehaviour {
    public float CoverLeanDistance = 1;
    
    Transform player;

    void Start() =>
        player = GameObject.FindGameObjectWithTag("Player").transform;

    public Vector3? GetNearestShootableCover(Vector3 point)
    {
        var sC = GetShootableCovers();
        if (sC.Any())
            return sC.Aggregate((agg, next) => 
                Vector3.Distance(next, point) < Vector3.Distance(agg, point) ? next : agg); //TODO: 1 Use shortest path instead
        return null;
    }
      
    public IEnumerable<Transform> GetCovers() => 
        GameObject.FindGameObjectsWithTag("CoverPoint") //TODO: Cache?
            .Select(g => g.transform)
            .Where(t => !PlayerLOS(t.position))
            .Where(t => Reachable(transform.position, t.position));

    bool PlayerLOS(Vector3 u) =>
        Physics.Raycast(u, player.position - u, out RaycastHit hit) && hit.collider.CompareTag("Player");

    bool Reachable(Vector3 from, Vector3 to) => 
        NavMesh.CalculatePath(from, to, NavMesh.AllAreas, new NavMeshPath());

    public IEnumerable<Vector3> GetShootableCovers() =>
        GetCovers()
            .Where(PlayerShootable)
            .Select(t => t.position);

    bool PlayerShootable(Transform t) =>
        new List<Vector3>{ CoverLeanDistance *- t.right, CoverLeanDistance* t.right }
            .Any(d => PlayerLOS(t.position + d));
}
