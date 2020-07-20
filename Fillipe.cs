using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Fillipe : AI
{
    #region Variables
    // Variables

    // Attacks
    public float AIThrowRange;
    [HideInInspector]
    public float AINextThrow;
    public float AIThrowCoolDown;
    public GameObject fillipeHand;
    public GameObject food;
    public GameObject debris;
    public GameObject dust;

    private GameObject Player;

    public float AIChargeRange;
    public float AINoChargeRange;
    [HideInInspector]
    public float AINextCharge;
    public float AIChargeCoolDown;
    public float AIChargeDamage;

    // States
    public bool isThrowing;
    public bool isCharging;
    public bool isFailCharged;
    public bool chargeStats;

    #endregion

    override public void Start()
    {
        // Setup
        target = PlayerTracker.transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        Player = GameObject.FindGameObjectWithTag("Player");

        // States
        isIdle = true;
        isChasing = false;
        isAttacking = false;
        isThrowing = false;
        isCharging = false;
        isFailCharged = false;
        chargeStats = false;

        SetKinematic(true);
    }


    override public void Update()
    {
        // Set animator depending on state
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isChasing", isChasing);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isThrowing", isThrowing);
        animator.SetBool("isCharging", isCharging);
        animator.SetBool("isFailCharged", isFailCharged);
        isFailCharged = false;

        // Distance between Player and AI
        float distFromPlayer = Vector3.Distance(target.transform.position, transform.position);

        // Look at player unless idle
        if (!isIdle && !isDead)
        {
            // Face the player
            FaceTarget();
        }

        // If ready to chase
        if (((distFromPlayer < AIChaseRange || takenDamage) && distFromPlayer > AIAttackRange) && !isDead && !isCharging)
        {
            // Chase player
            isIdle = false;
            isAttacking = false;
            isCharging = false;
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

        #region Cooldown Abilities
        // If ready to throw
        if (Time.time > AINextThrow && distFromPlayer <= AIThrowRange && !isDead)
        {
            // Throw attack the player via animation event
            animator.SetTrigger("isThrowing");
            // Reset the nextThrow time to a new point in the future
            AINextThrow = Time.time + AIThrowCoolDown;
        }

        // If ready to charge
        if (Time.time > AINextCharge && distFromPlayer <= AIChargeRange && distFromPlayer >= AINoChargeRange && !isDead)
        {
            isChasing = false;
            isCharging = true;
            // Charge attack the player
            ChargeAttack();
            // Reset the nextCharge time to a new point in the future
            AINextCharge = Time.time + AIChargeCoolDown;
        }

        // Check for charge stats 
        if (chargeStats)
        {
            agent.speed = 3f;
            agent.angularSpeed = 15;
            agent.stoppingDistance = 0;
        }
        else
        {
            agent.speed = 1.2f;
            agent.angularSpeed = 50;
            agent.stoppingDistance = 1.5f;
        }

        if (agent.isOnNavMesh)
        {
        // Stop movement when in certain animations (e.g. throwing)
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("stopMovement"))
        {
            agent.isStopped = true;
        }
            else agent.isStopped = false;
        }
            #endregion
    }

        override public void EnemyAttack()
        {
            // Used for raycast
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 origin = transform.position;
            //Debug.DrawRay(origin + (transform.up * 0.6f) + (transform.forward * 0.5f), forward, Color.white, AIAttackRange);
            // The raycast origin is moved to always hit the player
            if (Physics.Raycast(origin + (transform.up * 0.6f) + (transform.forward * 0.5f), forward, out RaycastHit hit, AIAttackRange))
            {
                if (hit.transform.gameObject.tag == "Player")
                {
                hit.transform.gameObject.SendMessage("playerTakeDamage", AIAttackDamage);
                }
            }
        }

        public void ThrowAttack()
        {
        // Instantiate food

        Instantiate(food, fillipeHand.transform.position, Quaternion.identity);
        }

    #region Charge Stuff
    public void ChargeAttack()
        {
        // Increase boss' stats
        chargeStats = true;

        // Pause code while charge attack ensues
        StartCoroutine(Charging());
        }
        
        // Coroutine which is called upon charging
        private IEnumerator Charging()
        {
        yield return new WaitForSeconds(2);

        isCharging = false;
        isChasing = true;

        // Reset boss' stats
        chargeStats = false;
        }

    // Used for when boss is charging, will stop the coroutine and perform necessary actions such as stunning the player
    public void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.tag == "Player" && isCharging)
        {
            // Re-enable boss functionality
            StopCoroutine(Charging());

            // Play stun effect on screen

            // Stun the player briefly
            // This is achieved inside the PlayerMove script on the Player via a coroutine
            Player.GetComponent<PlayerMove>().playerStunned();
        }
        else if (coll.gameObject.tag == "Obstacle" && isCharging)
        {
            // Re-enable boss functionality
            StopCoroutine(Charging());

            isFailCharged = true;
            // Knock Boss Down
            animator.SetTrigger("isFailCharged");

            // Destroy the obstacle
            Destroy(coll.gameObject);
            // Spawn dust particle effect
            GameObject.Instantiate(dust, coll.transform.position, dust.transform.rotation);
            Destroy(dust);
            // Instantiate debris prefab
            Instantiate(debris, coll.transform.position, debris.transform.rotation);


            // Stun Boss
            // This is achieved via animation tag
        }
    }
    #endregion

    override public void TakeDamage(float amount)
    {
        // Take damage
        takenDamage = true;
        AIHealth -= amount;

        //if (col.CompareTag("Bullet"))
        //{
        //    AIHealth -= 5f;
        //}

        // BloodFX
        GameObject.Instantiate(bloodSplatter, transform.position, Quaternion.identity);
        Destroy(bloodSplatter);

        // Should enemy die?
        if (AIHealth <= 0f)
        {
            Die();
            Debug.Log("0 Health");
        }
    }

    override public void Die()
    {
        isDead = true;

        // Remove states:
        animator.SetBool("isIdle", false);
        animator.SetBool("isChasing", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isStaggered1", false);
        animator.SetBool("isStaggered2", false);
        animator.SetBool("isThrowing", false);
        animator.SetBool("isCharging", false);
        animator.SetBool("isFailCharged", false);

        // Add Dead State
        animator.SetTrigger("isDead");

        // Turn on Ragdoll 
        SetKinematic(false);

        //// Enable/Disable stuff
        //GetComponent<Animator>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = false;
        //GetComponent<Rigidbody>().useGravity = true;


        //// Destroy the enemy after a delay
        //Destroy(gameObject, 10f);
    }

    // Range indicators
    override public void OnDrawGizmosSelected()
        {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AIChaseRange);
        Gizmos.DrawWireSphere(transform.position, AIAttackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, AIThrowRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, AIChargeRange);
        Gizmos.DrawWireSphere(transform.position, AINoChargeRange);
    }

}
