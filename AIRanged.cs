using UnityEngine;
using UnityEngine.AI;

public class AIRanged : AI
{
    #region Variables
    // Variables
    
    // Shooting
    [Range(0.0f, 1.0f)]
    public float shootProbability = 0.5f;
    [Range(0.0f, 1.0f)]
    public float shootAccuracy = 0.5f;
    private bool shoot;
    public float AIShootDamage = 5f;
    public float AIShootSpeed = 1f;
    [HideInInspector]
    public float AINextShot;
    #endregion

    // Shooting effects
    public GameObject GauderPistolBarrel;

    public ParticleSystem MuzzleFlash;
    public ParticleSystem GunSmoke;

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
        if ( ((distFromPlayer < AIChaseRange || takenDamage) && distFromPlayer > AIAttackRange) && !isDead)
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

        // If ready to shoot
        if (Time.time > AINextShot && isChasing && !isDead)
        {
            // Shoot the player
            ShootPlayer();

            // Reset the nextShot time to a new point in the future
            AINextShot = Time.time + AIShootSpeed;
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


    public void ShootPlayer()
    {
        // Decide whether to shoot based on shootProbability slider
        float chanceToShoot = Random.Range(0.0f, 1.0f);
        if (chanceToShoot > (1.0f - shootProbability))
        {
            shoot = true;
            
            // Play sounds

            // Play particle systems
            MuzzleFlash.Play();
            GunSmoke.Play();

        }

        if (shoot)
        {
            // Decide whether shot hits based on shootAccuracy slider
            float chanceToHit = Random.Range(0.0f, 1.0f);
            // If shot hits
            if (chanceToHit > (1.0f - shootAccuracy))
            {
                // Player takes damage
                GameObject.Find("Player").SendMessage("playerTakeDamage", AIShootDamage);
            }
        }
    }

}
