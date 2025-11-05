using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    // 金币旋转速度
    public float rotateSpeed = 1f;

    // 金币旋转方法
    void Update()
    {
        transform.Rotate(0, rotateSpeed, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerManager pm = other.GetComponent<PlayerManager>();
            if (pm != null)
            {
                pm.GetCoin();
            }
            Destroy(gameObject);
        }
    }
}