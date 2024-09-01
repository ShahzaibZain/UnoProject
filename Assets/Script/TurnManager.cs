using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public int currentPlayerIndex = 0;  // Index of the player whose turn it is
    private int totalPlayers;

    void Start()
    {
        totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

        // Initialize the first turn
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StartTurn", RpcTarget.AllBuffered, currentPlayerIndex);
        }
    }

    [PunRPC]
    void StartTurn(int playerIndex)
    {
        currentPlayerIndex = playerIndex;

        // Notify players whose turn it is
        if (PhotonNetwork.LocalPlayer.ActorNumber == currentPlayerIndex + 1)
        {
            // Allow this player to take their turn
            EnablePlayerTurn();
        }
        else
        {
            // Disable input for other players
            DisablePlayerTurn();
        }
    }

    public void EndTurn()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == currentPlayerIndex + 1)
        {
            // End current player's turn and move to the next player
            currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;
            photonView.RPC("StartTurn", RpcTarget.AllBuffered, currentPlayerIndex);
        }
    }

    void EnablePlayerTurn()
    {
        // Enable UI or player controls for the current player
        Debug.Log("It's your turn!");
        // Implement code here to allow this player to make their move
    }

    void DisablePlayerTurn()
    {
        // Disable UI or player controls for other players
        Debug.Log("Wait for your turn.");
        // Implement code here to prevent this player from making a move
    }
}
