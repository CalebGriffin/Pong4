using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    float lowerBound;
    float upperBound;

    float moveSpeed = 4f;

    void Awake()
    {
        lowerBound = -5f + (transform.localScale.y / 2);
        upperBound = 5f - (transform.localScale.y / 2);
    }

    public override void OnNetworkSpawn()
    {
        transform.parent.localEulerAngles = new Vector3(0f, 0f, OwnerClientId * 90f);
    }

    void Update()
    {
        if (IsHost) HostUpdate();
        if (!IsOwner) return;
        HandleMovement();
    }

    void HostUpdate()
    {

    }

    void HandleMovement()
    {
        if (Input.GetKey(KeyCode.LeftArrow)) transform.position = !Mathf.Approximately(transform.parent.localEulerAngles.z, 90f) ? MovePaddle(-transform.up * moveSpeed * Time.deltaTime) : MovePaddle(transform.up * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.RightArrow)) transform.position = !Mathf.Approximately(transform.parent.localEulerAngles.z, 90f) ? MovePaddle(transform.up * moveSpeed * Time.deltaTime) : MovePaddle(-transform.up * moveSpeed * Time.deltaTime);
    }

    Vector3 MovePaddle(Vector3 moveAmount)
    {
        float x = 0f;
        float y = 0f;

        if (Mathf.Approximately(transform.parent.localEulerAngles.z, 0f)) x = 4.9f;
        else if (Mathf.Approximately(transform.parent.localEulerAngles.z, 90f)) y = 4.9f;
        else if (Mathf.Approximately(transform.parent.localEulerAngles.z, 180f)) x = -4.9f;
        else if (Mathf.Approximately(transform.parent.localEulerAngles.z, 270f)) y = -4.9f;

        if (x == 0f) x = Mathf.Clamp(transform.position.x + moveAmount.x, lowerBound, upperBound);
        if (y == 0f) y = Mathf.Clamp(transform.position.y + moveAmount.y, lowerBound, upperBound);

        return new Vector3(x, y, transform.position.z);
    }
}