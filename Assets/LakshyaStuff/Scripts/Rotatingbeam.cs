using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotatingbeam : MonoBehaviour
{
   public float rotationSpeed = 60.0f;

   public Rigidbody rb;

void Start()
{
    rb = GetComponent<Rigidbody>();

}
    void Update()
    {
        rb.angularVelocity = Vector3.up * rotationSpeed * Time.deltaTime;
        //transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
    private void OnCollisionEnter(Collision collision)
    {
         if (collision.gameObject.CompareTag("Player"))
        {
          ReduceLives();
        }    
    }
}