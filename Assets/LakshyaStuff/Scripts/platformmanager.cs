using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class platformmanager : MonoBehaviour
{
    // public GameObject[] platforms; 
    public List<GameObject> platforms = new List<GameObject>();
    public GameObject indicatorPrefab; 
    public float destructionInterval = 5.0f; 
    // public float indicatorDuration = 2.0f; 
    public float timer;

    private void Start()
    {
        // StartCoroutine(DestroyRandomPlatformWithIndicator());
    }

    private async void Update()
    {
        timer += Time.deltaTime;
        if (timer >= destructionInterval && platforms.Count > 1 && GameManager.instance.gameStarted)
        {
            timer = 0.0f;
            int randomIndex = Random.Range(0, platforms.Count);
            GameObject platformToDestroy = platforms[randomIndex];
            platforms.RemoveAt(randomIndex);

            // jitter the platform
            for (int i = 0; i < 30; i++)
            {
                Vector3 jitter = new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f));
                platformToDestroy.transform.position += jitter;
                await Task.Delay(50);
                platformToDestroy.transform.position -= jitter;
            }

            Destroy(platformToDestroy);
        }
    }

    // IEnumerator DestroyRandomPlatformWithIndicator()
    // {
    //     while (true)
    //     {
    //         yield return new WaitForSeconds(destructionInterval);

    //         int randomIndex = Random.Range(0, platforms.Length);
    //         GameObject platformToDestroy = platforms[randomIndex];

    //         // GameObject indicator = Instantiate(indicatorPrefab, platformToDestroy.transform.position, Quaternion.identity);

    //         // Destroy(indicator, indicatorDuration);

    //         // jitter the platform
    //         for (int i = 0; i < 30; i++)
    //         {
    //             Vector3 jitter = new Vector3(Random.Range(-0.1f, 0.1f), 0f, Random.Range(-0.1f, 0.1f));
    //             platformToDestroy.transform.position += jitter;
    //             yield return new WaitForSeconds(0.1f);
    //             platformToDestroy.transform.position -= jitter;
    //         }

    //         // yield return new WaitForSeconds(indicatorDuration);

    //         Destroy(platformToDestroy);
    //     }
    // }
}