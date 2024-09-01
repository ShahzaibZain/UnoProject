using Photon.Pun;
using UnityEngine;

public class PlayerPositionManager : MonoBehaviourPunCallbacks
{
    public GameObject[] players;  // References to the player objects (Player1, Player2, Player3, Player4)
    public GameObject[] playerscards;  // References to the players' card objects (Player1BG, Player2BG, Player3BG, Player4BG)
    public GameObject[] playerPositions;  // Positions where players should be placed (Bottom, Left, Top, Right)
    public GameObject[] cardsPositions;  // Positions where players' cards should be placed (Bottom, Left, Top, Right)

    private void Start()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;  // Get player index based on joining order
        RearrangePlayers(playerIndex);
    }

    private void RearrangePlayers(int playerIndex)
    {
        int totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

        GameObject[] rearrangedPlayers = new GameObject[players.Length];
        GameObject[] rearrangedCards = new GameObject[playerscards.Length];

        for (int i = 0; i < players.Length; i++)
        {
            rearrangedPlayers[i] = players[i];
            rearrangedCards[i] = playerscards[i];
        }

        if (totalPlayers == 2)
        {
            // For 2 players, assign bottom and top positions
            rearrangedPlayers[0] = players[playerIndex];
            rearrangedPlayers[1] = players[(playerIndex + 2)];
            rearrangedCards[0] = playerscards[playerIndex];
            rearrangedCards[1] = playerscards[(playerIndex + 2)];

            // Place the first player at the bottom and the second player at the top
            rearrangedPlayers[0].transform.position = playerPositions[0].transform.position;
            rearrangedPlayers[0].transform.rotation = playerPositions[0].transform.rotation;

            rearrangedPlayers[1].transform.position = playerPositions[2].transform.position;
            rearrangedPlayers[1].transform.rotation = playerPositions[2].transform.rotation;

            rearrangedCards[0].transform.position = cardsPositions[0].transform.position;
            rearrangedCards[0].transform.rotation = cardsPositions[0].transform.rotation;

            rearrangedCards[1].transform.position = cardsPositions[2].transform.position;
            rearrangedCards[1].transform.rotation = cardsPositions[2].transform.rotation;
        }
        else if (totalPlayers == 3)
        {
            // For 3 players, assign bottom, left, and right positions
            rearrangedPlayers[0] = players[playerIndex];
            rearrangedPlayers[1] = players[(playerIndex + 1) % totalPlayers];
            rearrangedPlayers[2] = players[(playerIndex + 2) % totalPlayers];
            rearrangedCards[0] = playerscards[playerIndex];
            rearrangedCards[1] = playerscards[(playerIndex + 1) % totalPlayers];
            rearrangedCards[2] = playerscards[(playerIndex + 2) % totalPlayers];

            // Place the first player at the bottom, second player at the left, and third player at the right
            rearrangedPlayers[0].transform.position = playerPositions[0].transform.position;
            rearrangedPlayers[0].transform.rotation = playerPositions[0].transform.rotation;

            rearrangedPlayers[1].transform.position = playerPositions[1].transform.position;
            rearrangedPlayers[1].transform.rotation = playerPositions[1].transform.rotation;

            rearrangedPlayers[2].transform.position = playerPositions[3].transform.position;
            rearrangedPlayers[2].transform.rotation = playerPositions[3].transform.rotation;

            rearrangedCards[0].transform.position = cardsPositions[0].transform.position;
            rearrangedCards[0].transform.rotation = cardsPositions[0].transform.rotation;

            rearrangedCards[1].transform.position = cardsPositions[1].transform.position;
            rearrangedCards[1].transform.rotation = cardsPositions[1].transform.rotation;

            rearrangedCards[2].transform.position = cardsPositions[3].transform.position;
            rearrangedCards[2].transform.rotation = cardsPositions[3].transform.rotation;
        }
        else if (totalPlayers == 4)
        {
            // For 4 players, use full rotation logic
            for (int i = 0; i < players.Length; i++)
            {
                int newIndex = (i + playerIndex) % players.Length;
                rearrangedPlayers[newIndex] = players[i];
                rearrangedCards[newIndex] = playerscards[i];
            }

            // Assign the players and their cards to the new positions
            for (int i = 0; i < players.Length; i++)
            {
                rearrangedPlayers[i].transform.position = playerPositions[i].transform.position;
                rearrangedPlayers[i].transform.rotation = playerPositions[i].transform.rotation;

                rearrangedCards[i].transform.position = cardsPositions[i].transform.position;
                rearrangedCards[i].transform.rotation = cardsPositions[i].transform.rotation;
            }
        }
    }
}
