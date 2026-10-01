using UnityEngine;
using UnityEngine.AI;

public class Avoider : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public bool isSeen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
