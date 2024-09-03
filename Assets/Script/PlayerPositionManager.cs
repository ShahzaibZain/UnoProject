using Photon.Pun;
using UnityEngine;

public class PlayerPositionManager : MonoBehaviourPunCallbacks
{
    public GameObject[] JoinedPlayers;
    public Transform[] PlayerPositions;

    private void Start()
    {
        //add all gameobjects with Player component to JoinedPlayers
        Player[] allPlayers = FindObjectsOfType<Player>();
        JoinedPlayers = new GameObject[allPlayers.Length];

        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i].GetComponent<Player>() != null)
            {
                JoinedPlayers[i] = allPlayers[i].gameObject;
            }
        }

        if (PhotonNetwork.IsConnected)
        {
            // Ensure positions are assigned when all players have joined
            AssignPlayerPositions();
        }
    }

    private void AssignPlayerPositions()
    {
        // Ensure all players are correctly assigned to their positions
        for (int i = 0; i < JoinedPlayers.Length; i++)
        {
            GameObject player = JoinedPlayers[i];
            if (player != null && player.transform.parent == null)
            {
                // Find an available position for this player
                for (int j = 0; j < PlayerPositions.Length; j++)
                {
                    if (PlayerPositions[j].childCount == 0)
                    {
                        player.transform.SetParent(PlayerPositions[j]);
                        player.transform.localPosition = Vector3.zero; // Reset local position
                        player.transform.localRotation = Quaternion.identity; // Reset local rotation
                        break; // Exit the loop once the position is assigned
                    }
                }
            }
        }
    }
}
