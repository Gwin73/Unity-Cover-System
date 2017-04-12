using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SimpleCover))]
public class Agent : MonoBehaviour {
    private NavMeshAgent agent;
    private Transform player;
    private readonly List<Vector3> coverDebugPositions = new List<Vector3>();
    private readonly List<Vector3> coverShootDebugPositions = new List<Vector3>();
    private SimpleCover cover;

    private void Start()
    {
        agent = gameObject.GetComponentInChildren<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cover = gameObject.GetComponent<SimpleCover>();
    }

    private void Update()
    {
        StartCoroutine(MarkShootableCovers()); 
        StartCoroutine(MarkCovers());

       /* if (Vector3.Angle(player.forward, transform.position - player.position) > 90) //TODO: 5 Also trigger if no line of sight 
            agent.SetDestination(cover.GetNearestShootableCover(player.position));
        else
            agent.SetDestination(cover.GetNearestShootableCover(transform.position));*/
    }

    private IEnumerator MarkShootableCovers()
    {
        cover.GetShootableCovers().ToList().ForEach(coverShootDebugPositions.Add);
        yield return new WaitForSeconds(1);
    }

    private IEnumerator MarkCovers()
    {
        cover.GetCovers().ToList().ForEach(t => coverDebugPositions.Add(t.position));
        yield return new WaitForSeconds(1);
    }

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
