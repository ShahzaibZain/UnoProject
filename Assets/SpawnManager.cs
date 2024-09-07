using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public GamePlayManager gamePlayManager;
    private int prefabRotation;
    private Vector3 prefabPosition;
    public Transform ParentTransform;  // Array of positions (Bottom, Left, Top, Right)

    [Header("Player Prefabs")]
    public GameObject Player1Prefab;
    public GameObject Player2Prefab;
    public GameObject Player3Prefab;
    public GameObject Player4Prefab;

    private void Start()
    {
        // Get the player's index based on their order of joining the room
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        // Ensure playerIndex doesn't exceed the number of positions available
        playerIndex = Mathf.Clamp(playerIndex, 0, 3);

        // Instantiate the appropriate player prefab based on the player's index
        GameObject playerInstance = null;
        switch (playerIndex)
        {
            case 0:
                playerInstance = PhotonNetwork.Instantiate(Player1Prefab.name, Vector3.zero, Quaternion.identity);
                //prefabRotation = 0;
                //prefabPosition = new Vector3(0,0,0);
                break;
            case 1:
                playerInstance = PhotonNetwork.Instantiate(Player2Prefab.name, Vector3.zero, Quaternion.identity);
                //prefabRotation = -90;
                //prefabPosition = new Vector3(-734.6667f, 0, 0);
                break;
            case 2:
                playerInstance = PhotonNetwork.Instantiate(Player3Prefab.name, Vector3.zero, Quaternion.identity);
                //prefabRotation = 180;
                //prefabPosition = new Vector3(0, 360, 0);
                break;
            case 3:
                playerInstance = PhotonNetwork.Instantiate(Player4Prefab.name, Vector3.zero, Quaternion.identity);
                //prefabRotation = 90;
                //prefabPosition = new Vector3(0, 0, 0);
                break;
            default:
                Debug.LogError("Player index out of range or no prefab assigned!");
                return;
        }
    }
}