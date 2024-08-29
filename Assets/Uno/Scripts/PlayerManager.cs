/*using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [Header("Player Positions")]
    public Transform[] playerPositions; // Array of positions (bottom, top, left, right)

    private List<Transform> availablePositions;
    private Dictionary<int, Transform> playerPositionMap = new Dictionary<int, Transform>();

    void Start()
    {
        availablePositions = new List<Transform>(playerPositions);

        if (PhotonNetwork.IsConnected)
        {
            AssignPlayerPosition();
        }
    }

    void AssignPlayerPosition()
    {
        // Get the player's index in the room (PhotonNetwork.LocalPlayer.ActorNumber - 1) to maintain consistent order
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        // Assign the local player to the bottom position (Player1) and others accordingly
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            // Local player is the master client and always gets the bottom position
            AssignPositionToPlayer(0, playerIndex);
        }
        else
        {
            // Other players get remaining positions in the order they join
            AssignPositionToPlayer(playerIndex, playerIndex);
        }

        // Notify GameplayManager to set up the player in the assigned position
        GameObject playerObject = PhotonNetwork.Instantiate("PlayerPrefab", Vector3.zero, Quaternion.identity);
        playerObject.transform.position = playerPositionMap[playerIndex].position;

        // Set the UI for the player and register it in the GameplayManager
        playerObject.GetComponent<PlayerUI>().SetPlayerUI($"Player{playerIndex + 1}");
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerIndex + 1)
        {
            GamePlayManager.instance.SetLocalPlayer(playerObject, playerPositionMap[playerIndex]);
        }
        else
        {
            GamePlayManager.instance.SetOtherPlayer(playerObject, playerPositionMap[playerIndex]);
        }
    }

    void AssignPositionToPlayer(int positionIndex, int playerIndex)
    {
        // Assigns a player position and removes it from the available positions
        playerPositionMap[playerIndex] = availablePositions[positionIndex];
        availablePositions.RemoveAt(positionIndex);
    }
}
*/