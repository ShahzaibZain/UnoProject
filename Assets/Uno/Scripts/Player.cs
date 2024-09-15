using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public GameObject CardPanelBG;
    public PlayerCards cardsPanel;
    public string playerName;
    public bool isUserPlayer = false; // Flag to identify the local user player
    public Image avatarImage;
    public Text avatarName;
    public Text messageLbl;
    public ParticleSystem starParticleSystem;
    public Image timerImage;
    public GameObject timerOjbect;

    public int Rank { get; set; } // This property will store the player's rank
    private float totalTimer = 15f;

    [HideInInspector]
    public bool pickFromDeck, unoClicked, choosingColor;
    [HideInInspector]
    public bool isInRoom = true;
    public PhotonView photonView;
    public GamePlayManager GamePlayManager;
    public GameObject parentGO;
    private Vector3 parentGOposition;
    private Vector3 parentGOrotation;
    public bool MyTurn;

    public void SetMyTurn(bool value)
    {
        MyTurn = value;

        if (value)
        {
            Debug.Log(this.parentGO.name + "'s Turn");
        }
        else
        {
            Debug.Log("Not" + this.parentGO.name + "'s Turn");
        }
        OnTurn(MyTurn);
    }

    private void Awake()
    {
        MyTurn = false;
        GamePlayManager = FindObjectOfType<GamePlayManager>();
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // Set parent and position the player correctly
        if (parentGO != null)
        {
            parentGO.transform.SetParent(GamePlayManager.transform, false);
            RectTransform rectTransform = parentGO.GetComponent<RectTransform>();

            if (parentGO.name == "Player1(Clone)")
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 0);

                // Set pivot to center
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                // Set anchored position (Pos X = 0, Pos Y = 110)
                rectTransform.anchoredPosition = new Vector2(0, 110);

                // Set size (Width = 464.884, Height = 110)
                rectTransform.sizeDelta = new Vector2(464.884f, 110);
                if (photonView.IsMine)
                {
                    GamePlayManager.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }

            }
            else if (parentGO.name == "Player2(Clone)")
            {
                // Setting anchors to left middle
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(0, 0.5f);

                // Set pivot to center vertically (middle) and horizontally
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                // Set anchored position (Pos X = 180, Pos Y = 0)
                rectTransform.anchoredPosition = new Vector2(100, 0);

                // Set size (Width = 360, Height = 85)
                rectTransform.sizeDelta = new Vector2(360, 85);

                // Set rotation (Z = -90)
                rectTransform.localRotation = Quaternion.Euler(0, 0, -90);

                // Set scale (X = 1, Y = 1, Z = 1)
                rectTransform.localScale = new Vector3(1, 1, 1);

                if (photonView.IsMine)
                {
                    GamePlayManager.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 90);
                }
            }
            else if (parentGO.name == "Player3(Clone)")
            {
                rectTransform.anchorMin = new Vector2(0.5f, 1);
                rectTransform.anchorMax = new Vector2(0.5f, 1);

                // Set pivot to center vertically (middle) and horizontally
                rectTransform.pivot = new Vector2(0.5f, 0);

                // Set anchored position (Pos X = 180, Pos Y = 0)
                rectTransform.anchoredPosition = new Vector2(0, -50);

                // Set size (Width = 360, Height = 85)
                rectTransform.sizeDelta = new Vector2(440, 85);

                // Set rotation (Z = -90)
                rectTransform.localRotation = Quaternion.Euler(0, 0, 180);

                // Set scale (X = 1, Y = 1, Z = 1)
                rectTransform.localScale = new Vector3(1, 1, 1);
                if (photonView.IsMine)
                {
                    GamePlayManager.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 180);
                }
            }
            else if (parentGO.name == "Player4(Clone)")
            {
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);

                // Set pivot to center vertically (middle) and horizontally
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                // Set anchored position (Pos X = 180, Pos Y = 0)
                rectTransform.anchoredPosition = new Vector2(-110, 0);

                // Set size (Width = 360, Height = 85)
                rectTransform.sizeDelta = new Vector2(360, 85);

                // Set rotation (Z = -90)
                rectTransform.localRotation = Quaternion.Euler(0, 0, 90);

                // Set scale (X = 1, Y = 1, Z = 1)
                rectTransform.localScale = new Vector3(1, 1, 1);
                if (photonView.IsMine)
                {
                    GamePlayManager.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, -90);
                }
            }
        }

        // Set up the player's avatar and name from custom properties
        if (!photonView.IsMine)
        {
            isUserPlayer = false;
            if (photonView.Controller != null && photonView.Controller.CustomProperties.ContainsKey("Name"))
            {
                avatarName.text = photonView.Controller.CustomProperties["Name"].ToString();
            }
            else
            {
                Debug.LogWarning("Custom property 'Name' not found or photonView.Controller is null.");
            }

            if (photonView.Controller != null && photonView.Controller.CustomProperties.ContainsKey("Avatar"))
            {
                int avatarIndex = (int)photonView.Controller.CustomProperties["Avatar"];
                avatarImage.sprite = Resources.Load<Sprite>("Avatar/" + avatarIndex);
            }
            else
            {
                Debug.LogWarning("Custom property 'Avatar' not found or photonView.Controller is null.");
            }

        }
        else
        {
            // This player is the local player, apply the local settings
            isUserPlayer = true;
            SetAvatarProfile(GameManager.PlayerAvatarProfile);
        }
        // Initialize card visibility based on whether this is the user player
        UpdateCardVisibility();
        AddThisPlayerToList();
        // Add player to GamePlayManager's list
    }

    private void AddThisPlayerToList()
    {
        GamePlayManager.players.Add(this);
    }

    public void SetAvatarProfile(AvatarProfile p)
    {
        playerName = p.avatarName;
        if (avatarName != null)
        {
            avatarName.text = p.avatarName;
            avatarName.GetComponent<EllipsisText>().UpdateText();
        }
        if (avatarImage != null)
            avatarImage.sprite = Resources.Load<Sprite>("Avatar/" + p.avatarIndex);
    }


    public bool Timer
    {
        get
        {
            return timerOjbect.activeInHierarchy;
        }
        set
        {
            CancelInvoke("UpdateTimer");
            timerOjbect.SetActive(value);
            if (value)
            {
                timerImage.fillAmount = 1f;
                InvokeRepeating("UpdateTimer", 0f, .1f);
            }
            else
            {
                timerImage.fillAmount = 0f;
            }
        }
    }

    void UpdateTimer()
    {
        timerImage.fillAmount -= 0.1f / totalTimer;
        if (timerImage.fillAmount <= 0)
        {
            Debug.Log("Timer ended");
            if (choosingColor)
            {
                GamePlayManager.instance.colorChoose.HidePopup();
            }
            OnTurnEnd();
            GamePlayManager.instance.NextTurn();
        }
    }

    public void OnTurn(bool isTurn)
    {
        // If it's this player's turn
        if (isTurn)
        {
            Timer = true;
            if (photonView.IsMine)
            {
                unoClicked = false;
                pickFromDeck = false;
                

                if (isUserPlayer)
                {
                    UpdateCardColor();

                    // If no valid cards to play, enable deck click (to pick a card from the deck)
                    if (cardsPanel.AllowedCard.Count == 0)
                    {
                        GamePlayManager.instance.EnableDeckClick();
                    }
                }
            }
            
        }
        else
        {
            // Turn off the timer for players whose turn it is not
            Timer = false;
        }
    }


    public void UpdateCardColor()
    {
        if (isUserPlayer)
        {
            foreach (var item in cardsPanel.AllowedCard)
            {
                item.SetGaryColor(false);
                item.IsClickable = true;
            }
            foreach (var item in cardsPanel.DisallowedCard)
            {
                item.SetGaryColor(true);
                item.IsClickable = false;
            }
            if (cardsPanel.AllowedCard.Count > 0 && cardsPanel.cards.Count == 2)
            {
                GamePlayManager.instance.EnableUnoBtn();
            }
            else
            {
                GamePlayManager.instance.DisableUnoBtn();
            }
        }
    }

    public void AddCard(Card c)
{
        cardsPanel.cards.Add(c);
        c.transform.SetParent(cardsPanel.transform);
        c.onClick = OnCardClick;
        Debug.Log("Added OnCardClick to card: " + c.name);
        c.IsClickable = true;
/*        if (isUserPlayer)
        {
        }*/
/*        else
        {
            c.SetGaryColor(true); // Hide the cards for non-user players
            c.IsClickable = false;
        }*/
        /*if (photonView.IsMine)
        {
            
        }*/
    }

    public void RemoveCard(Card c)
    {
        cardsPanel.cards.Remove(c);
        c.onClick = null;
        c.IsClickable = false;
    }

    public void OnCardClick(Card c)
    {
        Debug.Log("Calling OnCardClick");
        // Broadcast the card to all players before adding to the waste
        Debug.Log("Calling RPC_SyncWastePile");
        GamePlayManager.instance.RPC_SyncWastePile(c);
        Debug.Log("Calling PutCardToWastePile");
        GamePlayManager.instance.PutCardToWastePile(c, this);
        Debug.Log("Calling OnTurnEnd");
        OnTurnEnd();
        //Debug.Log("Changing Turn");
        //GamePlayManager.instance.NextTurn();
        //Debug.Log("Turn Changed");
    }

    public void OnTurnEnd()
    {
        cardsPanel.UpdatePos();
        foreach (var item in cardsPanel.cards)
        {
            item.SetGaryColor(false);
        }
/*        Debug.Log("Ending Turn");
        GamePlayManager.instance.NextTurn();
        Debug.Log("Turn Changed");*/
    }

    public void ShowMessage(string message, bool playStarParticle = false)
    {
        messageLbl.text = message;
        messageLbl.GetComponent<Animator>().SetTrigger("show");
        if (playStarParticle)
        {
            starParticleSystem.gameObject.SetActive(true);
            starParticleSystem.Emit(30);
        }
    }

    public int GetTotalPoints()
    {
        int total = 0;
        foreach (var c in cardsPanel.cards)
        {
            total += c.point;
        }
        return total;
    }

    private void UpdateCardVisibility()
    {
        if (isUserPlayer)
        {
            // Show all cards for the user player
            foreach (var card in cardsPanel.cards)
            {
                card.SetGaryColor(false);
                card.IsClickable = true;
            }
        }
        else
        {
            // Hide all cards for non-user players
            foreach (var card in cardsPanel.cards)
            {
                card.SetGaryColor(true);
                card.IsClickable = false;
            }
        }
    }

}
