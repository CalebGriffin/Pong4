using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNetwork : NetworkBehaviour
{
    float lowerBound;
    float upperBound;

    float moveSpeed = 4f;

    bool gameRunning = false;

    private GameObject gameCanvas;
    private Button playButton;
    private TextMeshProUGUI playersText;

    private GameObject waitingCanvas;

    [SerializeField] private GameObject[] wallArr = new GameObject[4];

    [SerializeField] private GameObject ballPrefab;

    [SerializeField] private Color[] colorArr = new Color[4] { new Color(0.22f, 1f, 0.08f, 1f), new Color(1f, 0.41f, 0.71f, 1f), new Color(0.06f, 0.61f, 1f, 1f), new Color(1f, 0.5f, 0f, 1f) };

    NetworkVariable<Color> playerColor = new NetworkVariable<Color>();

    NetworkVariable<int> score = new NetworkVariable<int>();

    void Awake()
    {
        lowerBound = -5f + (transform.localScale.y / 2);
        upperBound = 5f - (transform.localScale.y / 2);
    }

    void Start()
    {
        Initialize();
    }

    public override void OnNetworkSpawn()
    {
        int playerIndex = (int)OwnerClientId;
        transform.parent.localEulerAngles = new Vector3(0f, 0f, (playerIndex % 4) * 90f);

        playerColor.OnValueChanged += OnStateChanged;
        Initialize();

        //if (IsHost || IsServer)
        //{
            //ClientRpcParams clientRpcParams = new ClientRpcParams
            //{
                //Send = new ClientRpcSendParams
                //{
                    //TargetClientIds = new ulong[] { NetworkManager.Singleton.ConnectedClientsIds[playerIndex] }
                //}
            //};
            //SetColourClientRpc(playerIndex, clientRpcParams);
        //}

        if (IsHost || IsServer) OnHostSpawn();
    }

    void Initialize()
    {
        int playerIndex = (int)OwnerClientId;
        if (IsHost || IsServer) playerColor.Value = colorArr[playerIndex % 4];
        GetComponent<SpriteRenderer>().color = playerColor.Value;
    }

    public override void OnNetworkDespawn()
    {
        if (IsHost || IsServer) OnHostDespawn();
    }

    void OnHostSpawn()
    {
        // This is run when the first player joins
        gameCanvas = GRefs.Singleton.gameCanvas;
        gameCanvas.SetActive(true);
        playButton = GRefs.Singleton.playGameButton;
        playButton.onClick.AddListener(PlayGame);
        playersText = GRefs.Singleton.playersText;
        EnableWaitingCanvasClientRpc();
    }

    void OnClientSpawn()
    {
        waitingCanvas = GRefs.Singleton.waitingCanvas;
        waitingCanvas.SetActive(true);
    }

    void OnHostDespawn()
    {
        // This is run when the last player leaves
    }

    void Update()
    {
        if (IsHost) HostUpdate();
        if (!IsOwner) return;
        HandleMovement();
    }

    void HostUpdate()
    {
        if (!gameRunning) playersText.text = $"Players: {NetworkManager.Singleton.ConnectedClientsIds.Count}";
    }

    public void OnStateChanged(Color oldColor, Color newColor)
    {
        Debug.Log($"Color changed from {oldColor} to {newColor}");
        playerColor.Value = newColor;
        GetComponent<SpriteRenderer>().color = playerColor.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    void DespawnWallServerRpc(int wallIndex, ServerRpcParams serverRpcParams = default)
    {
        wallArr[wallIndex].GetComponent<NetworkObject>().Despawn(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnWallServerRpc(int wallIndex, ServerRpcParams serverRpcParams = default)
    {
        GameObject wallObj = Instantiate(wallArr[wallIndex], wallArr[wallIndex].transform.position, Quaternion.identity);
        wallObj.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    public void UpdateScoreServerRpc()
    {
        score.Value++;
    }

    [ClientRpc]
    void UpdateScoreClientRpc()
    {

    }

    [ClientRpc]
    void SetColourClientRpc(int playerIndex, ClientRpcParams clientRpcParams = default)
    {
        gameObject.GetComponent<SpriteRenderer>().color = colorArr[playerIndex % 4];
    }

    [ClientRpc]
    void EnableWaitingCanvasClientRpc()
    {
        if (IsHost || IsServer) return;
        waitingCanvas = GRefs.Singleton.waitingCanvas;
        waitingCanvas.SetActive(true);
        waitingCanvas.SetActive(true);
    }

    [ClientRpc]
    void DisableWaitingCanvasClientRpc()
    {
        if (IsHost || IsServer) return;
        waitingCanvas.SetActive(false);
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

    void PlayGame()
    {
        if (gameRunning) return;
        gameRunning = true;
        for (int i = wallArr.Length - 1; i >= NetworkManager.Singleton.ConnectedClientsIds.Count; i--)
        {
            SpawnWallServerRpc(i);
        }
        DisableWaitingCanvasClientRpc();
        gameCanvas.SetActive(false);

        // Start the game here
        GameObject ball = Instantiate(ballPrefab, ballPrefab.transform.position, Quaternion.identity);
        ball.GetComponent<NetworkObject>().Spawn();
    }
}