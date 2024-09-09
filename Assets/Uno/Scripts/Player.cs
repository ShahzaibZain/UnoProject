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

    private void Awake()
    {
        GamePlayManager = FindObjectOfType<GamePlayManager>();
        photonView = GetComponent<PhotonView>();

    }

    #region PlayerConstructor
    public int ActorNumber { get; set; }
    public string Nickname { get; set; }

    // Constructor to initialize from Photon.Realtime.Player
    public Player(Photon.Realtime.Player photonPlayer)
    {
        this.ActorNumber = photonPlayer.ActorNumber;
        this.Nickname = photonPlayer.NickName;
    }
    #endregion

    private void Start()
    {
        // Set parent and position the player correctly
        if (parentGO != null)
        {
            parentGO.transform.SetParent(GamePlayManager.transform, false);
            RectTransform rectTransform = parentGO.GetComponent<RectTransform>();

            if (parentGO.name == "Player1(Clone)")
            {
                /*parentGOposition = new Vector3(0, -504, 0);
                parentGOrotation = new Vector3(0, 0, 0);*/
                rectTransform.anchorMin = new Vector2(0.5f, 0);
                rectTransform.anchorMax = new Vector2(0.5f, 0);

                // Set pivot to center
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                // Set anchored position (Pos X = 0, Pos Y = 110)
                rectTransform.anchoredPosition = new Vector2(0, 110);

                // Set size (Width = 464.884, Height = 110)
                rectTransform.sizeDelta = new Vector2(464.884f, 110);

            }
            else if (parentGO.name == "Player2(Clone)")
            {
                /*parentGOposition = new Vector3(-950, 0, 0);
                parentGOrotation = new Vector3(0, 0, -90);*/
                // Setting anchors to left middle
                rectTransform.anchorMin = new Vector2(0, 0.5f);
                rectTransform.anchorMax = new Vector2(0, 0.5f);

                // Set pivot to center vertically (middle) and horizontally
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                // Set anchored position (Pos X = 180, Pos Y = 0)
                rectTransform.anchoredPosition = new Vector2(180, 0);

                // Set size (Width = 360, Height = 85)
                rectTransform.sizeDelta = new Vector2(360, 85);

                // Set rotation (Z = -90)
                rectTransform.localRotation = Quaternion.Euler(0, 0, -90);

                // Set scale (X = 1, Y = 1, Z = 1)
                rectTransform.localScale = new Vector3(1, 1, 1);
            }
            else if (parentGO.name == "Player3(Clone)")
            {
                /*parentGOposition = new Vector3(0, 0, 0);
                parentGOrotation = new Vector3(0, 0, -90);*/
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
            }
            else if (parentGO.name == "Player4(Clone)")
            {
                /*parentGOposition = new Vector3(0, 0, 0);
                parentGOrotation = new Vector3(0, 0, -90);*/
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);

                // Set pivot to center vertically (middle) and horizontally
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                // Set anchored position (Pos X = 180, Pos Y = 0)
                rectTransform.anchoredPosition = new Vector2(-180, 0);

                // Set size (Width = 360, Height = 85)
                rectTransform.sizeDelta = new Vector2(360, 85);

                // Set rotation (Z = -90)
                rectTransform.localRotation = Quaternion.Euler(0, 0, 90);

                // Set scale (X = 1, Y = 1, Z = 1)
                rectTransform.localScale = new Vector3(1, 1, 1);
            }
            /*parentGO.transform.localPosition = parentGOposition;
            parentGO.transform.localRotation = Quaternion.Euler(parentGOrotation);
            parentGO.transform.localScale = Vector3.one;*/
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
        //photonView.RPC("AddThisPlayerToList", RpcTarget.AllBuffered);
    }

    //[PunRPC]
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
        //timerImage.fillAmount -= 0.1f / totalTimer;
        if (timerImage.fillAmount <= 0)
        {
            if (choosingColor)
            {
                if (isUserPlayer)
                {
                    GamePlayManager.instance.colorChoose.HidePopup();
                }
                ChooseBestColor();
            }
            else if (GamePlayManager.instance.IsDeckArrow)
            {
                GamePlayManager.instance.OnDeckClick();
            }
            else if (cardsPanel.AllowedCard.Count > 0)
            {
                //photonView.RPC("OnCardClick", RpcTarget.AllBuffered, FindBestPutCard());
                OnCardClick(FindBestPutCard());
            }
            else
            {
                //photonView.RPC("OnTurnEnd", RpcTarget.AllBuffered);
                OnTurnEnd();
            }
        }
    }

    public void OnTurn()
    {
        unoClicked = false;
        pickFromDeck = false;
        Timer = true;

        if (isUserPlayer)
        {
            UpdateCardColor();
            if (cardsPanel.AllowedCard.Count == 0)
            {
                GamePlayManager.instance.EnableDeckClick();
            }
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
        if (isUserPlayer)
        {
            c.onClick = OnCardClick;
            c.IsClickable = false;
        }
        else
        {
            c.SetGaryColor(true); // Hide the cards for non-user players
            c.IsClickable = false;
        }
    }

    public void RemoveCard(Card c)
    {
        cardsPanel.cards.Remove(c);
        c.onClick = null;
        c.IsClickable = false;
    }

    /*    public void OnCardClick(Card c)
        {
            if (Timer)
            {
                GamePlayManager.instance.PutCardToWastePile(c, this);
                OnTurnEnd();
            }
        }*/

    public void OnCardClick(Card c)
    {
        if (Timer)
        {
            // Broadcast the card to all players before adding to the waste
            GamePlayManager.instance.RPC_SyncWastePile(c);
            GamePlayManager.instance.PutCardToWastePile(c, this);
            OnTurnEnd();
        }
    }


    public void OnTurnEnd()
    {
        if (!choosingColor) Timer = false;
        cardsPanel.UpdatePos();
        foreach (var item in cardsPanel.cards)
        {
            item.SetGaryColor(false);
        }
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

    public Card FindBestPutCard()
    {
        List<Card> allow = cardsPanel.AllowedCard;
        allow.Sort((x, y) => y.Type.CompareTo(x.Type));
        return allow[0];
    }

    public void ChooseBestColor()
    {
        CardType temp = CardType.Other;
        if (cardsPanel.cards.Count == 1)
        {
            temp = cardsPanel.cards[0].Type;
        }
        else
        {
            int max = 1;
            for (int i = 0; i < 5; i++)
            {
                if (cardsPanel.GetCount((CardType)i) > max)
                {
                    max = cardsPanel.GetCount((CardType)i);
                    temp = (CardType)i;
                }
            }
        }

        if (temp == CardType.Other)
        {
            GamePlayManager.instance.SelectColor(Random.Range(1, 5));
        }
        else
        {
            if (Random.value < 0.7f)
                GamePlayManager.instance.SelectColor((int)temp);
            else
                GamePlayManager.instance.SelectColor(Random.Range(1, 5));
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
