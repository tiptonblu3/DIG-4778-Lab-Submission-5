using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

namespace AvoiderFJJ
{
public class Avoider : MonoBehaviour //code made by Freddie Brailsford, Jordon Dubin, and Juan Martinez.
{
    public bool showGizmos = true;
    public NavMeshAgent agent;
    public GameObject avoidee;
    public float speed = 5f;
    public bool isSeen;
    public bool wasSeen;
    public float timer = 0.5f;

    private List<Vector3> hidingSpots = new List<Vector3>(); //list that stores hiding spots for avoider

    [SerializeField] private float size_x = 10f;
    [SerializeField] private float size_y = 10f;
    [SerializeField] private float cellsize = 1f;


    LayerMask layerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.agent = GetComponent<NavMeshAgent>();
        Warnings();
        SetSpeed();

        StartCoroutine(GeneralLoop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSpeed()
    {
        if(agent != null)
        {
            agent.speed = speed;
        }
    }

    public void Warnings()
    {
        if(agent == null)
        {
            Debug.LogWarning("Please make the object a NavMesh Agent and bake a NavMesh.");
        }

        if(avoidee == null)
        {
            Debug.LogWarning("Please assign the object an Avoidee to avoid.");
        }
    }

    private void OnDrawGizmos()
    {
        if(showGizmos == false)
        {
            return;
        }

        if(hidingSpots.Count > 0)
        {
            foreach(Vector3 spot in hidingSpots)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(spot - Vector3.right * 0.5f, spot + Vector3.right * 0.5f);
                Gizmos.DrawLine(spot - Vector3.forward * 0.5f, spot + Vector3.forward * 0.5f);
            }
        }
    }

    private IEnumerator GeneralLoop()
    {
        while(true)
        {
            isSeen = CheckVisibility(transform.position);
            
             Debug.Log("Avoider seen: " + isSeen);

            
            if(isSeen && !wasSeen) // check if the player can see the avoider
            {
                FindSpot(); // if seen, find new hiding spot
            }

            wasSeen = isSeen;

            yield return new WaitForSeconds(timer); // tells the avoider to wait a little before checking if the player has seen them
        }
    }

    public void FindSpot()
    {
        hidingSpots.Clear();

        var sampler = new PoissonDiscSampler(size_x, size_y, cellsize);
        NavMeshHit hit; // Declare hit here so it's accessible


        foreach (var point in sampler.Samples())
        {
            Debug.Log("Valid hiding spots found: " + hidingSpots.Count);
            Vector3 worldPoint = new Vector3(point.x - size_x / 2f + transform.position.x,
            transform.position.y,
            point.y - size_y / 2f + transform.position.z);

            if (NavMesh.SamplePosition(worldPoint, out hit, 5.0f, NavMesh.AllAreas))
            {
                Vector3 validNavMeshPoint = hit.position;
                float distanceFromPlayer = Vector3.Distance(validNavMeshPoint, avoidee.transform.position);
                float distanceFromAgent = Vector3.Distance(validNavMeshPoint, transform.position);

                // Check visibility and distance using the valid NavMesh position
                if (!CheckVisibility(validNavMeshPoint) && distanceFromPlayer > 5f)
                {
                    hidingSpots.Add(validNavMeshPoint);
                }
            }

        }

    if (hidingSpots.Count > 0)
    {
        Vector3 bestSpot = hidingSpots[0];
        float bestScore = float.MinValue;

        foreach (Vector3 spot in hidingSpots)
        {
            float distToPlayer = Vector3.Distance(spot, avoidee.transform.position);
            float distToAgent = Vector3.Distance(spot, transform.position);

            // Scoring formula: Rewards being far from the player, 
            // but heavily penalizes spots that require running too far (stops border hugging).
            float score = distToPlayer - (distToAgent * 1.5f);

            if (score > bestScore)
            {
                bestScore = score;
                bestSpot = spot;
            }
        }

        agent.SetDestination(bestSpot);
    }
    else
    {
        // If no cover is found, force a direct flee vector away from the player
        Vector3 fleeDir = (transform.position - avoidee.transform.position).normalized;
        Vector3 fallbackPos = transform.position + fleeDir * 5f;
        if (NavMesh.SamplePosition(fallbackPos, out hit, 5.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    }

    public bool CheckVisibility(Vector3 point)
    {
        Vector3 origin = avoidee.transform.position + Vector3.up * 1.5f; // Player eye level
        
        // Check multiple points (Center, Left, Right) around the target position 
        // to prevent rays from slipping past box corners.
        Vector3[] targetOffsets = new Vector3[]
        {
            point + Vector3.up * 1.0f,                         // Center
            point + Vector3.up * 1.0f + Vector3.right * 0.5f,  // Right offset
            point + Vector3.up * 1.0f - Vector3.right * 0.5f   // Left offset
        };

        foreach (var targetPos in targetOffsets)
        {
            Vector3 direction = targetPos - origin;
            float distance = direction.magnitude;

            RaycastHit hit;
            if (Physics.Raycast(origin, direction.normalized, out hit, distance))
            {
                // If checking the avoider's actual position:
                if (point == transform.position)
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    {
                        return true; // Player sees the avoider
                    }
                }
                else
                {
                    // If checking a candidate hiding spot:
                    // If the ray hits the avoider or doesn't hit a wall/box first, it's visible.
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    {
                        return true; // Visible from this angle
                    }
                }
            }
            else
            {
                // If ray hits nothing at all, it's a completely open line of sight!
                if (point != transform.position)
                {
                    return true; // Visible
                }
            }
        }

        // If all rays are blocked by obstacles (walls/boxes), it is truly hidden.
        return (point == transform.position) ? false : false; 
    }

    public Vector3 ClosestPoint()
    {
        Vector3 closestSpot = hidingSpots[0]; // makes the closest spot equal to one of the points in the list

        float closestDistance = Vector3.Distance(transform.position, closestSpot);

        foreach (Vector3 spot in hidingSpots) 
        {
            float distance = Vector3.Distance(transform.position, spot); // measures the distance between the avoider and each hiding spot.

            if(distance < closestDistance) // if the distance is shorter than the closest distance
            {
                closestSpot = spot; // this spot becomes the new closest spot
                closestDistance = distance; // this distance becomes the closest distance
            }
        }

        return closestSpot;
    }

}
}
