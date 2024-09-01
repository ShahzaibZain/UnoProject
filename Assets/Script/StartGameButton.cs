using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class StartGameButton : MonoBehaviourPunCallbacks
{
    public Button startButton;
    public Text joinedPlayersCount;
    public static int joinedPlayers;

    public Transform[] spawnPositions;  // Assign spawn positions in the inspector
    public GameObject playerPrefab;  // Assign the PlayerPrefab in the inspector

    void Start()
    {
        // Update the joined players count when the script starts
        UpdateJoinedPlayersCount();

        // Make the start button only interactable by the host
        if (!PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(false);
        }

        startButton.onClick.AddListener(OnStartGameClicked);
    }

    // This method is called when a new player joins the room
    /*public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateJoinedPlayersCount();
    }*/

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdateJoinedPlayersCount();
    }

    // This method is called when a player leaves the room
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        UpdateJoinedPlayersCount();
    }

    // This method updates the player count displayed on the UI
    void UpdateJoinedPlayersCount()
    {
        if (joinedPlayersCount != null)
        {
            joinedPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            joinedPlayersCount.text = joinedPlayers.ToString();
        }
    }

    void OnStartGameClicked()
    {
        // Only the host (MasterClient) will trigger the RPC for everyone
        photonView.RPC("LoadGameSceneForAll", RpcTarget.All);
        //CreatePlayerMethod();
        //PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    [PunRPC]
    void LoadGameSceneForAll()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }
}
