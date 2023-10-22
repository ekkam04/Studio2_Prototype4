using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rotatingbeam : MonoBehaviour
{
   public float rotationSpeed = 60.0f;

   public Rigidbody rb;
   public float hitForce = 60f;

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
            // collision.gameObject.GetComponent<Player>().rb.AddForce(Vector3.forward * hitForce);
            print("Player hit");
            if (rotationSpeed > 0)
            {
                // get pushed in opposite direction
                collision.gameObject.GetComponent<Player>().rb.AddForce(Vector3.back * hitForce);
            }
            else
            {
                // get pushed in opposite direction
                collision.gameObject.GetComponent<Player>().rb.AddForce(Vector3.forward * hitForce);
            }
        }    
    }
}