using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Rotatingbeam : MonoBehaviour
{
   public float rotationSpeed = 2f;
   float rotationSpeedMax;

   public Rigidbody rb;
   public float hitForce = 60f;

void Start()
{
    rb = GetComponent<Rigidbody>();
    rotationSpeedMax = rotationSpeed * 2f;
}
    void Update()
    {
        if (rotationSpeed < rotationSpeedMax)
        {
            rotationSpeed += Time.deltaTime * 0.1f;
        }
    }

    void FixedUpdate()
    {
        rb.angularVelocity = Vector3.up * rotationSpeed;
        rb.maxAngularVelocity = rotationSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
         if (collision.gameObject.CompareTag("Player"))
        {
            // collision.gameObject.GetComponent<Player>().rb.AddForce(Vector3.forward * hitForce);
            print("Player hit");
            RumbleManager.instance.RumblePulse(collision.gameObject.GetComponent<PlayerInput>().devices[0] as Gamepad, 0.5f, 0.5f, 0.2f);
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