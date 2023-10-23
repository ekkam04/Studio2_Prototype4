using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class platformmanager : MonoBehaviour
{
    public GameObject[] platforms; 
    public GameObject indicatorPrefab; 
    public float destructionInterval = 5.0f; 
    public float indicatorDuration = 2.0f; 

    private void Start()
    {
        StartCoroutine(DestroyRandomPlatformWithIndicator());
    }

    IEnumerator DestroyRandomPlatformWithIndicator()
    {
        while (true)
        {
            yield return new WaitForSeconds(destructionInterval);

            int randomIndex = Random.Range(0, platforms.Length);
            GameObject platformToDestroy = platforms[randomIndex];

            GameObject indicator = Instantiate(indicatorPrefab, platformToDestroy.transform.position, Quaternion.identity);

            Destroy(indicator, indicatorDuration);

            yield return new WaitForSeconds(indicatorDuration);

            Destroy(platformToDestroy);
        }
    }
}