using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Vector3 startingPosition;
    public Vector3 endingPosition;
    public float speed = 1f;
    bool isMoving = false;
    bool roundStarted = false;

    public Player[] players;
    public float playerEliminationYPosition = -40f;

    void Start()
    {
        transform.position = startingPosition;
        players = FindObjectsOfType<Player>();
        roundStarted = true;
    }

    void Update()
    {
        // if isMoving, move camera from startingPosition to endingPosition
        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, endingPosition, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, endingPosition) < 0.05f)
            {
                isMoving = false;
            }
        }

        // if any player falls below playerEliminationYPosition, eliminate them
        foreach (Player player in players)
        {
            if (player.transform.position.y < playerEliminationYPosition)
            {
                player.GetEliminated();
            }
        }

        // if only one player isEliminated false is left active, end the round
        int playersEliminated = 0;
        foreach (Player player in players)
        {
            if (player.isEliminated)
            {
                playersEliminated++;
            }
        }
        if (playersEliminated == players.Length - 1 && roundStarted)
        {
            roundStarted = false;
            EndRound();
            print("Round ended");
        }
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    void EndRound()
    {
        GameManager.instance.EndRound();
    }
}