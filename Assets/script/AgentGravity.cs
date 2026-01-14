using UnityEngine;
using UnityEngine.AI;

public class AgentGravity : MonoBehaviour
{
    public float gravity = 20f;
    public float groundCheckDistance = 0.4f;
    public float groundOffset = 0.02f;
    public LayerMask groundMask = -1;

    private NavMeshAgent agent;
    private float verticalVelocity;
    private Collider cachedCollider;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updatePosition = false;
        }

        cachedCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (agent == null)
        {
            return;
        }

        bool grounded = false;
        RaycastHit hit;

        Vector3 origin;
        if (cachedCollider != null)
        {
            Bounds b = cachedCollider.bounds;
            origin = new Vector3(b.center.x, b.min.y + 0.05f, b.center.z);
        }
        else
        {
            origin = transform.position + Vector3.up * 0.1f;
        }

        if (Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            grounded = true;
        }

        if (agent.isOnNavMesh && !agent.isOnOffMeshLink)
        {
            if (grounded)
            {
                if (verticalVelocity < 0f)
                {
                    verticalVelocity = 0f;
                }
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }
        }

        Vector3 move = agent.nextPosition - transform.position;
        move.y = verticalVelocity * Time.deltaTime;

        Vector3 newPos = transform.position + move;

        if (grounded && verticalVelocity <= 0f)
        {
            if (cachedCollider != null)
            {
                float bottomOffset = transform.position.y - cachedCollider.bounds.min.y;
                newPos.y = hit.point.y + bottomOffset + groundOffset;
            }
            else
            {
                newPos.y = hit.point.y + groundOffset;
            }
        }

        transform.position = newPos;
        agent.nextPosition = transform.position;
    }
}
