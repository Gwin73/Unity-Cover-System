using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SimpleCover : MonoBehaviour {
    public float coverLeanDistance = 1;

    private NavMeshAgent agent;
    private Transform player;
    private List<Vector3> coverDebugPositions = new List<Vector3>();
    private List<Vector3> coverShootDebugPositions = new List<Vector3>();

    private void Start()
    {
        agent = gameObject.GetComponentInChildren<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update ()
    {
        //InvokeRepeating("MarkNearestShootableCover", 1, 1); //Fungerar
        //coverShootDebugPositions.Add(GetNearestShootableCover(player.transform.position));//Fungerar
        StartCoroutine("MarkNearestShootableCoverPoint"); //Fungerar bra
        
        /*coverDebugPositions = new List<Vector3>();
        GetCovers().ForEach(c => coverDebugPositions.Add(c.position));
        
 
        coverShootDebugPositions = new List<Vector3>();
        GetShootableCovers().ForEach(c => coverShootDebugPositions.Add(c));*/

        /*if (Vector3.Angle(player.forward, transform.position - player.position) > 90) //TODO: 5 Also trigger if no line of sight 
            agent.SetDestination(GetNearestCover(player.position, 100));
        else
            agent.SetDestination(GetNearestCover(transform.position, 100));*/
    }

    private IEnumerator MarkNearestShootableCoverPoint()
    {
        //Stopwatch sw = new Stopwatch();
        //sw.Start();
       // coverShootDebugPositions = new List<Vector3>();
        coverShootDebugPositions.Add(GetNearestShootableCover(player.transform.position));

        //sw.Stop();
        //UnityEngine.Debug.Log($"getNearestShootableCover: {sw.ElapsedMilliseconds}");
        yield return new WaitForSeconds(1);
    }

    private void MarkNearestShootableCover()
    {
        //Stopwatch sw = new Stopwatch();
        //sw.Start();

        coverShootDebugPositions.Add(GetNearestShootableCover(player.transform.position));

        //sw.Stop();
        //UnityEngine.Debug.Log($"getNearestShootableCover: {sw.ElapsedMilliseconds}");
    }

    private Vector3 GetNearestShootableCover(Vector3 point) =>
       GetShootableCovers() //No elements
           .Aggregate((agg, next) => //TODO: 1 Use shortest path instead + listempty crash 
                   Vector3.Distance(next, point) < Vector3.Distance(agg, point) ? next : agg);

    private IEnumerable<Transform> GetCovers() => 
        GameObject.FindGameObjectsWithTag("CoverPoint")
            .Select(g => g.transform)
            .Where(t => !PlayerLOS(t.position))
            .Where(t => Reachable(transform.position, t.position));

    private bool PlayerLOS(Vector3 u) =>
        Physics.Raycast(u, player.position - u, out RaycastHit hit) && hit.collider.CompareTag("Player");

    private bool Reachable(Vector3 from, Vector3 to) => 
        NavMesh.CalculatePath(from, to, NavMesh.AllAreas, new NavMeshPath());

    private IEnumerable<Vector3> GetShootableCovers() =>
        GetCovers()
            .Where(t => PlayerShootable(t))
            .Select(t => t.position);

    private bool PlayerShootable(Transform t) =>
        new List<Vector3>() { coverLeanDistance *- t.right, coverLeanDistance* t.right }
            .Any(d => PlayerLOS(t.position + d));

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (Vector3 coverPosition in coverDebugPositions)
            Gizmos.DrawSphere(coverPosition, 0.2f);

        Gizmos.color = Color.blue;
        foreach (Vector3 coverShootPosition in coverShootDebugPositions)
            Gizmos.DrawSphere(coverShootPosition, 0.2f);
    }
}
