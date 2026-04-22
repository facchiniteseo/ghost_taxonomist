using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GhostAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 5f;
    public float detectionAngle = 120f;

    [Header("Chase")]
    public float chaseStopDistance = 20f;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Patrol")]
    public float waypointReachedDistance = 1f;

    [Header("Camera morte")]
    public Camera ghostCamera;
    public float dissolveDuration = 2f;
    public LayerMask wallLayers;

    [Tooltip("Ogni quanti secondi controlla se la cam è dentro un muro")]
    public float intervalloControlloCamera = 0.2f;

    // Salvata da Start() — è la posizione/rotazione che hai impostato tu nel prefab
    private Vector3 cameraLocalPosSalvata;
    private Quaternion cameraLocalRotSalvata;

    private NavMeshAgent agent;
    private Animator animator;
    public Transform player;

    private enum GhostState { Patrol, Chase }
    private GhostState currentState = GhostState.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = patrolSpeed;
        SetNewRandomWaypoint();

        if (ghostCamera != null)
        {
            ghostCamera.depth = -2;

            // Salva la posizione e rotazione che hai impostato tu nel prefab
            cameraLocalPosSalvata = ghostCamera.transform.localPosition;
            cameraLocalRotSalvata = ghostCamera.transform.localRotation;

            StartCoroutine(ControllaCamera());
        }
    }

    private IEnumerator ControllaCamera()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalloControlloCamera);
            AggiornaPosizioneCameraSeNecessario();
        }
    }

    private void AggiornaPosizioneCameraSeNecessario()
    {
        if (ghostCamera == null) return;

        Vector3 fantasmaPos = transform.position + Vector3.up * 1f;
        Vector3 targetPos = transform.TransformPoint(cameraLocalPosSalvata);
        Vector3 direzione = targetPos - fantasmaPos;
        float distanza = direzione.magnitude;

        if (Physics.Raycast(fantasmaPos, direzione.normalized, out RaycastHit hit, distanza, wallLayers))
        {
            // Muro trovato — sposta la cam appena davanti al punto di impatto
            ghostCamera.transform.position = hit.point - direzione.normalized * 0.3f;
            ghostCamera.transform.LookAt(fantasmaPos);
        }
        else
        {
            // Nessun muro — ripristina esattamente la posizione che hai impostato tu
            ghostCamera.transform.localPosition = cameraLocalPosSalvata;
            ghostCamera.transform.localRotation = cameraLocalRotSalvata;
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case GhostState.Patrol: HandlePatrol(); break;
            case GhostState.Chase: HandleChase(); break;
        }
    }

    void HandlePatrol()
    {
        animator.SetBool("isMoving", true);
        animator.SetBool("isChasing", false);

        if (agent.remainingDistance <= waypointReachedDistance && !agent.pathPending)
            SetNewRandomWaypoint();

        if (CanSeePlayer())
        {
            currentState = GhostState.Chase;
            agent.speed = chaseSpeed;
        }
    }

    void HandleChase()
    {
        animator.SetBool("isMoving", true);
        animator.SetBool("isChasing", true);
        agent.SetDestination(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer >= chaseStopDistance)
        {
            currentState = GhostState.Patrol;
            agent.speed = patrolSpeed;
            SetNewRandomWaypoint();
        }
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0;
        float distance = directionToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (distance > detectionRadius) return false;
        if (angle > detectionAngle / 2f) return false;
        return true;
    }

    void SetNewRandomWaypoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 15f;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 15f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            SetNewRandomWaypoint();
    }

    public void TriggerDissolve()
    {
        animator.SetTrigger("isDissolving");
        agent.isStopped = true;
    }

    public void AttivaCamera()
    {
        if (ghostCamera != null)
            ghostCamera.depth = 2;
    }

    public void DisattivaCamera()
    {
        if (ghostCamera != null)
            ghostCamera.depth = -2;
    }

    public float GetDissolveDuration() => dissolveDuration;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
        if (respawn == null || respawn.IsInvincible) return;

        respawn.Respawn();
    }
}