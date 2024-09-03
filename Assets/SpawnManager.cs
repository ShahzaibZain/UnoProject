using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPun
{
    public GamePlayManager gamePlayManager;

    public GameObject Position1;
    public GameObject Position2;
    public GameObject Position3;
    public GameObject Position4;

    public GameObject PlayerPrefab;

    void Start()
    {
        if (Position1.transform.childCount == 0)
        {
            photonView.RPC("InstantiateAtPosition", RpcTarget.AllBuffered, Position1.transform.position, Position1.transform.rotation, 1);
        }
        else if (Position2.transform.childCount == 0)
        {
            photonView.RPC("InstantiateAtPosition", RpcTarget.AllBuffered, Position2.transform.position, Position2.transform.rotation, 2);
        }
        else if (Position3.transform.childCount == 0)
        {
            photonView.RPC("InstantiateAtPosition", RpcTarget.AllBuffered, Position3.transform.position, Position3.transform.rotation, 3);
        }
        else if (Position4.transform.childCount == 0)
        {
            photonView.RPC("InstantiateAtPosition", RpcTarget.AllBuffered, Position4.transform.position, Position4.transform.rotation, 4);
        }
        else
        {
            Debug.LogWarning("All positions are occupied.");
        }
    }

    [PunRPC]
    void InstantiateAtPosition(Vector3 position, Quaternion rotation, int positionIndex)
    {
        // Instantiate the player prefab at the position's location
        GameObject playerInstance = PhotonNetwork.Instantiate(PlayerPrefab.name, position, rotation);

        // Set the instantiated player as a child of the corresponding position
        Transform parentTransform = null;

        switch (positionIndex)
        {
            case 1:
                parentTransform = Position1.transform;
                break;
            case 2:
                parentTransform = Position2.transform;
                break;
            case 3:
                parentTransform = Position3.transform;
                break;
            case 4:
                parentTransform = Position4.transform;
                break;
        }

        playerInstance.transform.SetParent(parentTransform, false);
        gamePlayManager.players.Add(playerInstance.GetComponent<Player>());

        // Set local position, rotation, and scale to zero and one respectively
        playerInstance.transform.localPosition = Vector3.zero;
        playerInstance.transform.localRotation = Quaternion.identity;
        playerInstance.transform.localScale = Vector3.one;
    }
}
