using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class Avoider : MonoBehaviour
{
    public bool showGizmos = true;
    public NavMeshAgent agent;
    public GameObject avoidee;
    public float speed = 5f;
    public bool isSeen;
    public bool wasSeen;

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

            yield return new WaitForSeconds(0.5f); // tells the avoider to wait a little before checking if the player has seen them
        }
    }

    public void FindSpot()
    {
        hidingSpots.Clear();
        
        var sampler = new PoissonDiscSampler(size_x, size_y, cellsize);
        
        foreach (var point in sampler.Samples())
        {
            Vector3 worldPoint = new Vector3(point.x - size_x / 2f + avoidee.transform.position.x,
            transform.position.y,
            point.y - size_y / 2f + avoidee.transform.position.z);

            float distanceFromPlayer = Vector3.Distance(worldPoint, avoidee.transform.position);

            if(!CheckVisibility(worldPoint) && distanceFromPlayer > 5f)
            {
                hidingSpots.Add(worldPoint);
            }

        }

        if (hidingSpots.Count > 0)
        {
            int randomIndex = Random.Range(0, hidingSpots.Count);
            Vector3 hidingSpot = hidingSpots[randomIndex];
            agent.SetDestination(hidingSpot);
        }

    }

    public bool CheckVisibility(Vector3 point)
    {
        Vector3 direction = point - avoidee.transform.position;
        float distance = direction.magnitude;

        RaycastHit hit;
        
        // checks if the raycast hits anything but the player
        if (Physics.Raycast(avoidee.transform.position, direction.normalized, out hit, distance))
        {
            //avoidees vision is blocked
            if (hit.transform == transform)
            {
                return true;
            }
            return false;
        }

        return true;

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
