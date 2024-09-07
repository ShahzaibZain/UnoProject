using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_InputField createInputField;
    public TMP_InputField joinInputField;
    public Button createButton;
    public TextMeshProUGUI statusText;
    public GameObject roomButtonPrefab;
    public Transform scrollViewContent;
    public Button startGameButton;
    public TextMeshProUGUI playerCountText;

    private Dictionary<string, GameObject> roomButtons = new Dictionary<string, GameObject>();
    ExitGames.Client.Photon.Hashtable playerproperties = new ExitGames.Client.Photon.Hashtable();

    private void Start()
    {
        SetUIInteractable(false);
        // Don't disconnect unless necessary
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
            statusText.text = "Connecting to Photon...";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void GoToHome()
    {
        SceneManager.LoadScene("HomeScene");
    }
    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to Master Server. You can now create or join a room.";
        playerproperties["Name"] = PlayerPrefs.GetString("PlayerName");
        playerproperties["Avatar"] = PlayerPrefs.GetInt("PlayerAvatarIndex");
        PhotonNetwork.SetPlayerCustomProperties(playerproperties);
        SetUIInteractable(true);
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "Joined Lobby. Ready to create or join a room.";
    }

    public void CreateRoom()
    {
        string roomName = createInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            statusText.text = "Room name cannot be empty!";
            return;
        }

        SetUIInteractable(false);
        statusText.text = "Creating Room...";

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4, // Set maximum players in the room
            BroadcastPropsChangeToAll = true,
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        statusText.text = $"Room '{PhotonNetwork.CurrentRoom.Name}' created successfully.";
        AddRoomButton(PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel("GamePlay"); // Load the gameplay scene
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Room creation failed: {message}.";
        SetUIInteractable(true);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Failed to join room: {message}.";
        SetUIInteractable(true);
    }

    public void AddRoomButton(string roomName)
    {
        if (roomButtons.ContainsKey(roomName))
        {
            Debug.LogWarning($"Button for room '{roomName}' already exists.");
            return;
        }

        GameObject roomButton = Instantiate(roomButtonPrefab, scrollViewContent);
        TextMeshProUGUI buttonText = roomButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = roomName;

        Button buttonComponent = roomButton.GetComponent<Button>();
        buttonComponent.onClick.AddListener(() => JoinRoom(roomName));

        roomButtons[roomName] = roomButton;
    }

    public void JoinRoom(string roomName)
    {
        statusText.text = $"Joining Room '{roomName}'...";
        PhotonNetwork.JoinRoom(roomName);
    }

    public void JoinRoom()
    {
        string roomName = joinInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            statusText.text = "Room name cannot be empty!";
            return;
        }

        SetUIInteractable(false);
        statusText.text = $"Joining Room '{roomName}'...";
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnJoinedRoom()
    {
        statusText.text = $"Joined Room: {PhotonNetwork.CurrentRoom.Name}.";
        PhotonNetwork.LoadLevel("GamePlay"); // Load the gameplay scene
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Remove old room buttons
        foreach (var roomButton in roomButtons.Values)
        {
            Destroy(roomButton);
        }
        roomButtons.Clear();

        // Create new buttons for available rooms
        foreach (var room in roomList)
        {
            if (room.IsOpen && room.IsVisible && room.PlayerCount < room.MaxPlayers)
            {
                AddRoomButton(room.Name);
            }
        }
    }

    private void SetUIInteractable(bool interactable)
    {
        createButton.interactable = interactable;
        createInputField.interactable = interactable;
        joinInputField.interactable = interactable;
    }
}
