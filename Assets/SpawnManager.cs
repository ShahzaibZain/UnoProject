/*using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public GamePlayManager gamePlayManager;

    public Transform[] playerPositions;  // Array of positions (Bottom, Left, Top, Right)
    public GameObject playerPrefab;

    private void Start()
    {
        // Ensure all players are instantiated and assigned positions correctly
        if (PhotonNetwork.IsMasterClient)
        {
            AssignPlayerPositions();
        }
    }

    void AssignPlayerPositions()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int actorNumber = PhotonNetwork.PlayerList[i].ActorNumber;
            photonView.RPC("InstantiatePlayerAtPosition", RpcTarget.AllBuffered, actorNumber, i);
        }
    }

    [PunRPC]
    void InstantiatePlayerAtPosition(int actorNumber, int positionIndex)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            if (transform.childCount == 0)  // Ensure that player instance is created only once
            {
                Transform targetPosition = playerPositions[positionIndex];
                GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, targetPosition.position, targetPosition.rotation);

                playerInstance.transform.SetParent(targetPosition, false);
                playerInstance.transform.localPosition = Vector3.zero;
                playerInstance.transform.localRotation = Quaternion.identity;
                playerInstance.transform.localScale = Vector3.one;

                // Optionally store the player instance in the GamePlayManager or other relevant component
                // gamePlayManager.players.Add(playerInstance.GetComponent<Player>());
            }
        }
    }
}*/



using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    public GamePlayManager gamePlayManager;

    public Transform[] playerPositions;  // Array of positions (Bottom, Left, Top, Right)
    public GameObject playerPrefab;

    private void Start()
    {
        if (playerPositions[0].childCount == 0)
        {
            GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, playerPositions[0].position, playerPositions[0].rotation);

            playerInstance.transform.SetParent(playerPositions[0], false);
            playerInstance.transform.localPosition = Vector3.zero;
            playerInstance.transform.localRotation = Quaternion.identity;
            playerInstance.transform.localScale = Vector3.one;
        }
        else if (playerPositions[1].childCount == 0)
        {
            GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, playerPositions[1].position, playerPositions[1].rotation);

            playerInstance.transform.SetParent(playerPositions[1], false);
            playerInstance.transform.localPosition = Vector3.zero;
            playerInstance.transform.localRotation = Quaternion.identity;
            playerInstance.transform.localScale = Vector3.one;
        }
        else if (playerPositions[2].childCount == 0)
        {
            GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, playerPositions[2].position, playerPositions[2].rotation);

            playerInstance.transform.SetParent(playerPositions[2], false);
            playerInstance.transform.localPosition = Vector3.zero;
            playerInstance.transform.localRotation = Quaternion.identity;
            playerInstance.transform.localScale = Vector3.one;
        }
        else if (playerPositions[3].childCount == 0)
        {
            GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, playerPositions[3].position, playerPositions[3].rotation);

            playerInstance.transform.SetParent(playerPositions[3], false);
            playerInstance.transform.localPosition = Vector3.zero;
            playerInstance.transform.localRotation = Quaternion.identity;
            playerInstance.transform.localScale = Vector3.one;
        }
    }
}
/*    private void Update()
    {
        GameObject PlayerPrefabs = FindObjectOfType<Player>().gameObject;
        for (int i = 0; i < playerPositions.Length; i++)
        {
            if (PlayerPrefabs.gameObject.transform.parent != null || 
                PlayerPrefabs.gameObject.transform.parent.name == playerPositions[i].name || 
                playerPositions[i].childCount != 0)
            {
                return;
            }
            else
            {
                PlayerPrefabs.transform.SetParent(playerPositions[i]);
            }

        }
        
    }

/*private void Start()
{
    // Make sure the positions array is large enough
    if (playerPositions.Length < PhotonNetwork.PlayerList.Length)
    {
        Debug.LogError("Not enough player positions for the number of players.");
        return;
    }
    // Ensure the player instantiation happens for all players
    if (PhotonNetwork.IsMasterClient)
    {
        AssignPlayerPositions();
    }
}

void AssignPlayerPositions()
{
    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
    {
        int actorNumber = PhotonNetwork.PlayerList[i].ActorNumber;
        photonView.RPC("InstantiatePlayerAtPosition", RpcTarget.AllBuffered, actorNumber, i);
    }
}

[PunRPC]
void InstantiatePlayerAtPosition(int actorNumber, int positionIndex)
{
    if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
    {
        Transform targetPosition = playerPositions[positionIndex];
        GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, targetPosition.position, targetPosition.rotation);

        playerInstance.transform.SetParent(targetPosition, false);
        playerInstance.transform.localPosition = Vector3.zero;
        playerInstance.transform.localRotation = Quaternion.identity;
        playerInstance.transform.localScale = Vector3.one;

        // gamePlayManager.players.Add(playerInstance.GetComponent<Player>());
    }
}*//*
}
*/