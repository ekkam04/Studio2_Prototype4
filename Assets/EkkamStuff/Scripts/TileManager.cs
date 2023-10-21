using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class TileManager : MonoBehaviour
{
    [SerializeField] GameObject tileUndamaged;
    [SerializeField] GameObject tileSlightlyDamaged;

    [SerializeField] GameObject tileVeryDamaged;
    [SerializeField] GameObject[] tileVeryDamagedPieces;

    bool playerIsOnTile = false;
    bool isCrumbling = false;
    float timeSincePlayerSteppedOnTile = 0f;

    void Start()
    {
        tileVeryDamaged.SetActive(false);
        tileSlightlyDamaged.SetActive(false);
        tileUndamaged.SetActive(true);

        // Rotate hexagon tile randomly to make the cracks look more natural
        transform.Rotate(0, 0, Random.Range(0, 6) * 60);
    }

    void Update()
    {
        if (playerIsOnTile && !isCrumbling)
        {
            timeSincePlayerSteppedOnTile += Time.deltaTime;
            if (timeSincePlayerSteppedOnTile >= 1f)
            {
                DamageTile();
                timeSincePlayerSteppedOnTile = 0f;
            }
        }
        else
        {
            timeSincePlayerSteppedOnTile = 0f;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!GetComponent<MeshCollider>().enabled) return;
        if(other.gameObject.tag == "Player")
        {
            DamageTile();
            playerIsOnTile = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (!GetComponent<MeshCollider>().enabled) return;
        if (other.gameObject.tag == "Player")
        {
            playerIsOnTile = false;
        }
    }

    async void DamageTile()
    {
        if (tileUndamaged.activeSelf)
        {
            tileUndamaged.SetActive(false);
            tileSlightlyDamaged.SetActive(true);
        }
        else if (tileSlightlyDamaged.activeSelf)
        {
            tileSlightlyDamaged.SetActive(false);
            tileVeryDamaged.SetActive(true);
        }
        else if (tileVeryDamaged.activeSelf)
        {
            // jitter the tile
            isCrumbling = true;
            for (int i = 0; i < 30; i++)
            {
                Vector3 jitter = new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f));
                tileVeryDamaged.transform.position += jitter;
                await Task.Delay(20);
                tileVeryDamaged.transform.position -= jitter;
            }

            GetComponent<MeshCollider>().enabled = false;
            foreach (GameObject piece in tileVeryDamagedPieces)
            {
                // Add rigidbody to piece and enable mesh collider
                Rigidbody rb = piece.AddComponent<Rigidbody>();
                Physics.IgnoreCollision(piece.GetComponent<Collider>(), FindObjectOfType<Player>().GetComponent<Collider>());
                piece.GetComponent<MeshCollider>().enabled = true;
                rb.AddExplosionForce(100f, transform.position, 10f);
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            }
            await Task.Delay(10000);

            gameObject.SetActive(false);
        }
    }
}
