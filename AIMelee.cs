using UnityEngine;
using UnityEngine.AI;

public class AIMelee : AI
{
    #region Variables
    // Variables

    #endregion



    override public void Start()
    {
        // Setup
        target = PlayerTracker.transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // States
        isIdle = true;
        isChasing = false;
        isAttacking = false;
        SetKinematic(true);
    }


    override public void Update()
    {
        // Set animator depending on state
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isChasing", isChasing);
        animator.SetBool("isAttacking", isAttacking);

        // Distance between Player and AI
        float distFromPlayer = Vector3.Distance(target.transform.position, transform.position);

        // Look at player unless idle
        if (!isIdle && !isDead)
        {
            // Face the player
            FaceTarget();
        }

        // If ready to chase
        if (((distFromPlayer < AIChaseRange || takenDamage) && distFromPlayer > AIAttackRange) && !isDead)
        {
            // Chase player
            isIdle = false;
            isAttacking = false;
            isChasing = true;
            agent.SetDestination(target.position);
        }

        // If ready to attack
        if (Time.time > AINextAttack && distFromPlayer <= AIAttackRange && !isDead)
        {
                isChasing = false;
                isAttacking = true;

                // Attack the player
                EnemyAttack();

                // Reset the nextAttack time to a new point in the future
                AINextAttack = Time.time + AIAttackSpeed;
        }
    }


    override public void EnemyAttack()
    {
        // Used for raycast
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 origin = transform.position;

        if (Physics.Raycast(origin, forward, out RaycastHit hit, AIAttackRange))
        {
            if (hit.transform.gameObject.tag == "Player")
            {
                hit.transform.gameObject.SendMessage("playerTakeDamage", AIAttackDamage);
            }
        }
    }

}
