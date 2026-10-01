using UnityEngine;
using UnityEngine.AI;

public class Avoider : MonoBehaviour
{
    public bool showGizmos = true;
    public NavMeshAgent agent;
    public GameObject avoidee;
    public float speed = 5f;
    public bool isSeen;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.agent = GetComponent<NavMeshAgent>();
        Warnings();
        SetSpeed();
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

    private void OnawGizmos()
    {
        if(showGizmos == false)
        {
            return;
        }


    }
}
