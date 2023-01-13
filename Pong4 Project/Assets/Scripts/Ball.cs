using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    private float speed = 5f;

    void Awake()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        if (x == 0f && y == 0f)
        {
            x = 1f;
        }
        GetComponent<Rigidbody2D>().velocity = new Vector2(x, y) * speed;
    }

    void Update()
    {
        if (CheckIfOutOfBounds())
        {
            // The game is over
        }
    }

    bool CheckIfOutOfBounds()
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Increase score
        }
    }
}
