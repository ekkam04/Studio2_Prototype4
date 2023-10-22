using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemymovement : MonoBehaviour
{
    public float speed = 5.0f;
    public float despawnDelay = 5.0f;

    private void Start()
    {
        Invoke("DespawnEnemy", despawnDelay);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void DespawnEnemy()
    {
        Destroy(gameObject);
    }
}