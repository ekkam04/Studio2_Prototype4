using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

public class TileManager : MonoBehaviour
{
    public bool crumbleTiles = false;

    [SerializeField] GameObject tileUndamaged;
    [SerializeField] GameObject tileSlightlyDamaged;

    [SerializeField] GameObject tileVeryDamaged;
    [SerializeField] GameObject[] tileVeryDamagedPieces;

    List<Player> playersOnTile = new List<Player>();

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
        if (!crumbleTiles) return;
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
        if (!crumbleTiles) return;
        if (!GetComponent<MeshCollider>().enabled) return;
        if(other.gameObject.tag == "Player")
        {
            playerIsOnTile = true;
            playersOnTile.Add(other.gameObject.GetComponent<Player>());
            DamageTile();
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (!crumbleTiles) return;
        if (!GetComponent<MeshCollider>().enabled) return;
        if (other.gameObject.tag == "Player")
        {
            Player player = other.gameObject.GetComponent<Player>();
            playerIsOnTile = false;
            playersOnTile.Remove(player);

            if (player.isJumping)
            {
                RumbleManager.instance.StopContinuousRumble(other.gameObject.GetComponent<PlayerInput>().devices[0] as Gamepad);
            }
        }
    }

    async void DamageTile()
    {
        if (tileUndamaged.activeSelf)
        {
            tileUndamaged.SetActive(false);
            tileSlightlyDamaged.SetActive(true);
            foreach (Player player in playersOnTile)
            {
                RumbleManager.instance.RumblePulse(player.gameObject.GetComponent<PlayerInput>().devices[0] as Gamepad, 0.5f, 0.5f, 0.1f);
            }
        }
        else if (tileSlightlyDamaged.activeSelf)
        {
            tileSlightlyDamaged.SetActive(false);
            tileVeryDamaged.SetActive(true);
            foreach (Player player in playersOnTile)
            {
                RumbleManager.instance.RumblePulse(player.gameObject.GetComponent<PlayerInput>().devices[0] as Gamepad, 0.5f, 0.5f, 0.2f);
            }
        }
        else if (tileVeryDamaged.activeSelf)
        {
            foreach (Player player in playersOnTile)
            {
                RumbleManager.instance.StartRumbleContinuous(player.gameObject.GetComponent<PlayerInput>().devices[0] as Gamepad, 0.5f, 0.5f);
            }

            // jitter the tile
            isCrumbling = true;
            for (int i = 0; i < 30; i++)
            {
                Vector3 jitter = new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f));
                tileVeryDamaged.transform.position += jitter;
                await Task.Delay(20);
                tileVeryDamaged.transform.position -= jitter;
            }

            foreach (Player player in playersOnTile)
            {
                RumbleManager.instance.StopContinuousRumble(player.gameObject.GetComponent<PlayerInput>().devices[0] as Gamepad);
            }

            GetComponent<MeshCollider>().enabled = false;
            foreach (GameObject piece in tileVeryDamagedPieces)
            {
                // Add rigidbody to piece and enable mesh collider
                Rigidbody rb = piece.AddComponent<Rigidbody>();
                MeshCollider mc = piece.GetComponent<MeshCollider>();

                Physics.IgnoreCollision(piece.GetComponent<Collider>(), FindObjectOfType<Player>().GetComponent<Collider>());
                // mc.enabled = true;
                // mc.isTrigger = true;
                rb.AddExplosionForce(100f, transform.position, 10f);
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            }
            
            Invoke("HideTile", 10f);
        }
    }

    void HideTile()
    {
        gameObject.SetActive(false);
    }
}
