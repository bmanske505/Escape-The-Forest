using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Stalker : MonoBehaviour
{

    [Header("Movement")]
    public float wanderSpeed = 3.5f;
    public float chaseSpeed = 6f;
    public float wanderRadius = 10f;
    public float wanderInterval = 3f;

    [Header("Vision")]
    public float viewDistance = 15f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;

    private NavMeshAgent agent;
    private Transform player;

    private Vector3 lastKnownPlayerPosition;
    private float wanderTimer;

    private enum State { Wandering, Chasing, Searching }
    private State currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentState = State.Wandering;
        wanderTimer = wanderInterval;
        agent.speed = wanderSpeed;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Wandering:
                Wander();
                LookForPlayer();
                break;

            case State.Chasing:
                ChasePlayer();
                break;

            case State.Searching:
                SearchLastKnownPosition();
                LookForPlayer();
                break;
        }
    }

    // ================= STATES =================

    void Wander()
    {
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderInterval)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
            agent.SetDestination(newPos);
            wanderTimer = 0f;
        }
    }

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
        }
        else
        {
            agent.speed = wanderSpeed;
            agent.SetDestination(lastKnownPlayerPosition);
            currentState = State.Searching;
        }
    }

    void SearchLastKnownPosition()
    {
        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            currentState = State.Wandering;
            wanderTimer = wanderInterval;
        }
    }

    // ================= VISION =================

    void LookForPlayer()
    {
        if (CanSeePlayer())
        {
            currentState = State.Chasing;
            lastKnownPlayerPosition = player.position;
        }
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > viewDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2f)
            return false;

        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer,
            distanceToPlayer, obstacleMask))
        {
            return false;
        }

        return true;
    }

    // ================= UTIL =================

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }

    // ================= PLAYER CATCH =================

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
