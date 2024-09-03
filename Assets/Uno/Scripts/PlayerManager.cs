using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks 
{
    /*public GameObject[] playerGameObjects; // Assign Player1 to Player4 in the editor
    public GameObject[] playerCardsGameObjects; // Assign Player1BG to Player4BG in the editor

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            AssignPlayerToUI();
        }
    }

    void AssignPlayerToUI()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        Player playerScript = playerGameObjects[playerIndex].GetComponent<Player>();
        playerScript.isUserPlayer = true;

        // Set up the player's UI elements (e.g., assigning cards panel, etc.)
        playerScript.cardsPanel = playerCardsGameObjects[playerIndex].GetComponent<PlayerCards>();

        // You can also assign the player's name, avatar, etc., here.
        playerScript.SetAvatarProfile(GameManager.PlayerAvatarProfile);
    }*/
}
