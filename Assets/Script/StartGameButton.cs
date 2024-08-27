using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class StartGameButton : MonoBehaviourPun
{
    public Button startButton;

    void Start()
    {
        // Make the start button only interactable by the host
        if (!PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(false);
        }

        startButton.onClick.AddListener(OnStartGameClicked);
    }

    void OnStartGameClicked()
    {
        // Only the host (MasterClient) will trigger the RPC for everyone
        photonView.RPC("LoadGameSceneForAll", RpcTarget.All);
    }

    [PunRPC]
    void LoadGameSceneForAll()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }
}
