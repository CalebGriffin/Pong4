using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    private float speed = 5f;

    private float randomFactor = 100f;

    void Awake()
    {
        float x = 0f;
        float y = 0f;
        while (x == 0f && y == 0f)
        {
            x = Random.Range(-1f, 1f);
            y = Random.Range(-1f, 1f);
        }
        GetComponent<Rigidbody2D>().velocity = new Vector2(x, y).normalized * speed;
    }

    void Update()
    {
        if (OutOfBounds())
        {
            // The game is over

            // Disable the score canvas on the client
            DisableScoreCanvasClientRpc();

            // Reenable the waiting canvas on the client
            EnableWaitingCanvasClientRpc();

            // Reenable the game canvas on the server
            EnableGameCanvasServerRpc();

            // Destroy the ball and despawn it from the network
            Destroy(gameObject);
        }
    }

    bool OutOfBounds()
    {
        if (transform.position.y > 5f || transform.position.y < -5f)
        {
            return true;
        }
        else if (transform.position.x > 5f || transform.position.x < -5f)
        {
            return true;
        }

        return false;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        DirectionRandomiser();
        GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity.normalized * speed;
        if (collision.gameObject.CompareTag("Player"))
        {
            // Increase score
            UpdateScoreUIClientRpc();
        }
    }

    [ClientRpc]
    void UpdateScoreUIClientRpc()
    {
        GRefs.Singleton.score++;
        GRefs.Singleton.scoreText.text = GRefs.Singleton.score.ToString();
    }

    [ClientRpc]
    void DisableScoreCanvasClientRpc()
    {
        GRefs.Singleton.score = 0;
        GRefs.Singleton.scoreText.text = GRefs.Singleton.score.ToString();
        GRefs.Singleton.scoreCanvas.SetActive(false);
    }

    [ClientRpc]
    void EnableWaitingCanvasClientRpc()
    {
        if (IsHost || IsServer) return;
        GRefs.Singleton.waitingCanvas.SetActive(true);
    }

    [ServerRpc]
    void EnableGameCanvasServerRpc()
    {
        GRefs.Singleton.gameCanvas.SetActive(true);
    }

    void DirectionRandomiser()
    {
        float x = 0f;
        float y = 0f;
        while (x == 0f && y == 0f)
        {
            x = Random.Range(-randomFactor, randomFactor);
            y = Random.Range(-randomFactor, randomFactor);
        }
        GetComponent<Rigidbody2D>().velocity += new Vector2(x, y).normalized;
    }
}
