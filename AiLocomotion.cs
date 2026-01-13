using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AiLocomotion : MonoBehaviour
{
    public Transform playerTransform;
    private Animator anim;
    private NavMeshAgent agent;
    bool isAttacking = false;

    [SerializeField] public float DistanceStoppedDestination = 2f;
    [SerializeField] private float speedAnimationSmooth = 0.1f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        // Si le joueur n'est pas assigné (ce qui arrive quand on spawn un prefab), on le cherche
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("AiLocomotion: Impossible de trouver le joueur ! Assure-toi que ton Joueur a le Tag 'Player'.");
            }
        }
    }

    void Update()
    {
        agent.stoppingDistance = DistanceStoppedDestination;
        agent.destination = playerTransform.position;

        // mettre à jour animation de marche
        anim.SetFloat("Speed", agent.velocity.magnitude, speedAnimationSmooth, Time.deltaTime);

        // si arrivé à destination et pas en train d’attaquer
        if (AgentHasArrived() && !isAttacking)
        {
            StartCoroutine(Attack());
            Debug.Log("Attack !");
        }
    }

    bool AgentHasArrived()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;
        // tolérance pour vitesse très faible
        if (agent.velocity.sqrMagnitude > 0.1f) return false;

        return true;
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        int layer = anim.GetLayerIndex("Attack Layer");
        anim.SetLayerWeight(layer, 1);
        anim.SetTrigger("Attack");

        // attendre la durée de l’animation
        float duration = anim.GetCurrentAnimatorStateInfo(layer).length;
        yield return new WaitForSeconds(duration);

        anim.SetLayerWeight(layer, 0);
        isAttacking = false;
    }
}
