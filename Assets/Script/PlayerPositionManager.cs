using Photon.Pun;
using UnityEngine;

public class PlayerPositionManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;  // Reference to the player prefab
    public Transform[] playerPositions;  // Positions where player prefabs should be placed (Bottom, Left, Top, Right)
    public GamePlayManager gamePlayManager;
    public void StartMethod()
    {
        if (photonView.IsMine)
        {
            int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;  // Get player index based on joining order

            // Assign the local player to an available position
            photonView.RPC("AssignPlayerPosition", RpcTarget.AllBuffered, playerIndex);
        }
    }

    [PunRPC]
    private void AssignPlayerPosition(int playerIndex)
    {
        if (playerIndex < playerPositions.Length && playerPositions[playerIndex].childCount == 0)
        {
            // Instantiate the player prefab at the designated position
            GameObject playerInstance = PhotonNetwork.Instantiate(playerPrefab.name, playerPositions[playerIndex].position, playerPositions[playerIndex].rotation);
            gamePlayManager.players.Add(playerInstance.GetComponent<Player>());
            // Set the instantiated player as a child of the corresponding position
            playerInstance.transform.SetParent(playerPositions[playerIndex], false);
            playerInstance.transform.localPosition = Vector3.zero;
            playerInstance.transform.localRotation = Quaternion.Euler(Vector3.zero);
            playerInstance.transform.localScale = Vector3.one;
        }
    }
}


