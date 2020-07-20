using UnityEngine;
using UnityEngine.AI;


public class AI : MonoBehaviour
{
    #region Variables
    // Variables

    // States
    public bool isIdle;
    public bool isChasing;
    public bool isAttacking;

    public bool isDead;
    public bool takenDamage;

    // Stats
    public float AIHealth = 20f;
    public float AIAttackDamage = 10f;
    public float AIAttackSpeed = 1f;
    public float AIAttackRange = 2f;
    [HideInInspector]
    public float AINextAttack;
    public float AIChaseRange;

    // Particle Systems#    
    public GameObject bloodSplatter;
    
    // Staggering
    [HideInInspector]
    public int randomStagger = 1;
    [HideInInspector]
    public readonly int numberOfStaggers = 2;

    // Chasing
    public GameObject PlayerTracker;
    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public NavMeshAgent agent;

    // Other
    [HideInInspector]
    public Animator animator;
    #endregion


    
    virtual public void Start()
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


    virtual public void Update()
    {
        // Set animator depending on state
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isChasing", isChasing);
        animator.SetBool("isAttacking", isAttacking);

        // Distance between Player and AI
        float distFromPlayer = Vector3.Distance(target.transform.position, transform.position);

        // Look at player unless idle or dead
        if (!isIdle && !isDead)
        {
            // Face the player
            FaceTarget();
        }

        // If ready to chase
        if ( distFromPlayer < AIChaseRange && distFromPlayer > AIAttackRange && !isDead)
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
            {  
                isChasing = false;
                isAttacking = true;

                // Attack the player
                EnemyAttack();

                // Reset the nextAttack time to a new point in the future
                AINextAttack = Time.time + AIAttackSpeed;
            }
        }
    }


    virtual public void EnemyAttack()
    {
        // Used for raycast
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 origin = transform.position;

        if (Physics.Raycast(origin, forward, out RaycastHit hit, AIAttackRange))
        {
            if (hit.transform.gameObject.tag == "Player")
            {
                hit.transform.gameObject.SendMessageUpwards("playerTakeDamage", AIAttackDamage);
            }
        }
    }


    public void FaceTarget()
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }


    virtual public void TakeDamage(float amount)
    {
        // Take damage
        takenDamage = true;
        AIHealth -= amount;

        //if (col.CompareTag("Bullet"))
        //{
        //    AIHealth -= 5f;
        //}



        // Play random stagger animation
        randomStagger = Random.Range(1, numberOfStaggers + 1);
        animator.SetTrigger("isStaggered" + randomStagger);
        //Debug.Log("Just played Stagger number " + randomStagger);

        // BloodFX
        //GameObject.Instantiate(bloodSplatter, transform.position, Quaternion.identity);
        //Destroy(bloodSplatter);

        // Should enemy die?
        if (AIHealth <= 0f)
        {
            Die();
            Debug.Log("0 Health");
        }
    }


    // Sets Ragdoll's rbs kinematic values
    public void SetKinematic(bool newValue)
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in bodies)
        {
            rb.isKinematic = newValue;
        }
    }


    virtual public void Die()
    {
        isDead = true;

        // Remove states:
        animator.SetBool("isIdle", false);
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isStaggered1", false);

        // Turn on Ragdoll 
        SetKinematic(false);

        // Enable/Disable stuff
        GetComponent<Animator>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().useGravity = true;
        

        // Destroy the enemy after a delay
        Destroy(gameObject, 10f);
        //Debug.Log("Is Dead");
    }


    // Used to draw radii etc.
    virtual public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AIChaseRange);
        Gizmos.DrawWireSphere(transform.position, AIAttackRange);
    }

}


