using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillipeFood : MonoBehaviour
{
    private Rigidbody rb;
    public int upForce;
    public int forwardForce;
    public GameObject player;
    public int lifeTime;

    public float poisonDamage = 2f;
    private float poisonTickDelay = 1f;
    public float poisonDuration = 5f;
    private int poisonCurrentTick = 0;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Get direction towards player
        player = GameObject.FindWithTag("Player");
        Vector3 dirToPlayer = player.transform.position - this.transform.position;

        // Apply upwards force
        rb.AddForce(transform.up * upForce);
        // Apply forwards force
        rb.AddForce(dirToPlayer * forwardForce);

        // Apply lifetime 
        Destroy(gameObject, lifeTime);
    }

    public void OnCollisionEnter(Collision other)
    {
        // If hits player, damage them
        if (other.gameObject.tag == "Player")
        {
            // Cause the player to take more damage for a short period
            StartCoroutine(PoisonDamage());
            player.GetComponent<PlayerMove>().playerSlowed();
        }
    }

    IEnumerator PoisonDamage()
    {
        // Until the duration runs out
        while (poisonCurrentTick < poisonDuration)
        {
            // Damage the player
            player.SendMessage("playerTakeDamage", poisonDamage);
            // Wait until next tick
            yield return new WaitForSeconds(poisonTickDelay);
            // Increase number of ticks
            poisonCurrentTick++;
        }

        Destroy(this);
    }

}
