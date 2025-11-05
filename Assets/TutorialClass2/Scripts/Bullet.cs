using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;

    public float speed = 10f;

    public GameObject hitEffect;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 碰撞到角色时，改为根据碰撞体找到对应的 PlayerManager
        if (other.CompareTag("Player"))
        {
            PlayerManager pm = other.GetComponent<PlayerManager>();
            if (pm != null)
            {
                Vector3 knockbackDirection = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                pm.TakeDamage(damage, knockbackDirection);
            }

            Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Ground"))
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }
    }
}