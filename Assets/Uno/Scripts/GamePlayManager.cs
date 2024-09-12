using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable 0618

public class GamePlayManager : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip music_win_clip;
    public AudioClip music_loss_clip;
    public AudioClip draw_card_clip;
    public AudioClip throw_card_clip;
    public AudioClip uno_btn_clip;
    public AudioClip choose_color_clip;

    [Header("Gameplay")]

    public float cardDealTime = 0.05f;
    public Card _cardPrefab;
    public Transform cardDeckTransform;
    public Image cardWastePile;
    public GameObject arrowObject, unoBtn, cardDeckBtn;
    public Popup colorChoose, playerChoose, noNetwork;
    public GameObject loadingView, rayCastBlocker;
    public Animator cardEffectAnimator;
    public ParticleSystem wildCardParticle;
    public GameObject menuButton;

    [Header("Player Setting")]
    public List<Player> players;
    public bool clockwiseTurn = true;
    public int currentPlayerIndex = 0;
    public Player CurrentPlayer { get { return players[currentPlayerIndex]; } }

    [Header("Game Over")]
    public GameObject gameOverPopup;
    public ParticleSystem starParticle;
    public List<GameObject> playerObject;
    public GameObject loseTimerAnimation;
    public List<Card> cards;
    private List<Card> wasteCards;

    public CardType CurrentType
    {
        get { return _currentType; }
        set { _currentType = value; cardWastePile.color = value.GetColor(); }
    }

    public CardValue CurrentValue
    {
        get { return _currentValue; }
        set { _currentValue = value; }
    }

    [SerializeField] CardType _currentType;
    [SerializeField] CardValue _currentValue;

    public bool IsDeckArrow
    {
        get { return arrowObject.activeSelf; }
    }
    public static GamePlayManager instance;

    System.DateTime pauseTime;
    int fastForwardTime = 0;
    bool setup = false, multiplayerLoaded = false, gameOver = false;

    public GameObject playerPrefab;
    public PhotonView photonView;
    public GameObject cardPrefab;

    private void Start()
    {
        instance = this;
        Input.multiTouchEnabled = false;
        Time.timeScale = 1;

        StartCoroutine(StartMultiPlayerGameMode());
        
    }

    IEnumerator StartMultiPlayerGameMode()
    {
        loadingView.SetActive(true);
        yield return new WaitForSeconds(0);
        loadingView.SetActive(false);
        cardDeckBtn.SetActive(true);
        cardWastePile.gameObject.SetActive(true);
        SetupGame();
        multiplayerLoaded = true;
    }

    void SetupGame()
    {
        menuButton.SetActive(true);
        currentPlayerIndex = 0;

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncCardDeck", RpcTarget.All);
            photonView.RPC("DealCardsMethod", RpcTarget.All, 7);
            //photonView.RPC("UpdatePlayerTurn", RpcTarget.All, currentPlayerIndex);
        }
    }


    [PunRPC]
    void SyncCardDeck()
    {
        CreateDeck();
    }

    List<Card> CreateDeck()
    {
        cards = new List<Card>();
        wasteCards = new List<Card>();
        for (int j = 1; j <= 4; j++)
        {
            cards.Add(CreateCardOnDeck(CardType.Other, CardValue.Wild));
            cards.Add(CreateCardOnDeck(CardType.Other, CardValue.DrawFour));
        }
        for (int i = 0; i <= 12; i++)
        {
            for (int j = 1; j <= 4; j++)
            {
                cards.Add(CreateCardOnDeck((CardType)j, (CardValue)i));
                cards.Add(CreateCardOnDeck((CardType)j, (CardValue)i));
            }
        }

        cards.Shuffle();
        return cards;
    }

    Card CreateCardOnDeck(CardType t, CardValue v)
    {
        Card temp = Instantiate(_cardPrefab, cardDeckTransform.position, Quaternion.identity, cardDeckTransform);
        temp.Type = t;
        temp.Value = v;
        temp.IsOpen = false;
        temp.CalcPoint();
        temp.name = t.ToString() + "_" + v.ToString();
        return temp;
    }

    [PunRPC]
    private void DealCardsMethod(int NumberOfCards)
    {
        StartCoroutine(DealCards(NumberOfCards));
    }

    IEnumerator DealCards(int total)
    {
        yield return new WaitForSeconds(1f);
        for (int t = 0; t < total; t++)
        {
            for (int i = 0; i < players.Count; i++)
            {
                PickCardFromDeck(players[i]);
                yield return new WaitForSeconds(cardDealTime); // Wait for the delay between card deals
            }
        }

        yield return new WaitForSeconds(1.5f);
        int a = 0;
        while (cards[a].Type == CardType.Other)
        {
            a++;
        }

        RPC_SyncWastePileAtStart(cards[a]);

        cards.RemoveAt(a);

        for (int i = 0; i < players.Count; i++)
        {
            players[i].cardsPanel.UpdatePos();
        }

        setup = true;
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("UpdatePlayerTurn", RpcTarget.All, players[currentPlayerIndex].photonView.OwnerActorNr);
        }
    }

    #region SYNC PILE AT START
    [PunRPC]
    void SyncWastePileAtStart(CardType cardType, CardValue cardValue)
    {
        CurrentType = cardType;
        CurrentValue = cardValue;
        Card card = CreateCard(cardType, cardValue, cardDeckBtn);
        Debug.Log("Card is " + cardType + " " + cardValue);
        if (card != null)
        {
            // Sync the waste pile visually
            wasteCards.Add(card);
            card.IsOpen = true;
            card.transform.SetParent(cardWastePile.transform, true);
            card.SetTargetPosAndRot(new Vector3(Random.Range(-15f, 15f), Random.Range(-15f, 15f), 1),
                card.transform.localRotation.eulerAngles.z + Random.Range(-15f, 15f));
        }
    }

    public void RPC_SyncWastePileAtStart(Card card)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncWastePileAtStart", RpcTarget.All, card.Type, card.Value);
            photonView.RPC("UpdatePos", RpcTarget.All);
        }
    }
    #endregion

    IEnumerator DealCardsToPlayer(Player p, int NoOfCard = 1, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        for (int t = 0; t < NoOfCard; t++)
        {
            PickCardFromDeck(p, true);
            yield return new WaitForSeconds(cardDealTime);
        }
    }

    public Card PickCardFromDeck(Player p, bool updatePos = false)
    {
        if (cards.Count == 0)
        {
            print("Card Over");
            while (wasteCards.Count > 5)
            {
                cards.Add(wasteCards[0]);
                wasteCards[0].transform.SetParent(cardDeckTransform);
                wasteCards[0].transform.localPosition = Vector3.zero;
                wasteCards[0].transform.localRotation = Quaternion.Euler(Vector3.zero);
                wasteCards[0].IsOpen = false;
                wasteCards.RemoveAt(0);
            }
        }
        Card temp = cards[0];
        p.AddCard(cards[0]);
        cards[0].IsOpen = p.isUserPlayer;
        if (updatePos)
            p.cardsPanel.UpdatePos();
        else
            cards[0].SetTargetPosAndRot(Vector3.zero, 0f);
        cards.RemoveAt(0);
        GameManager.PlaySound(throw_card_clip);
        return temp;
    }

    public void PutCardToWastePile(Card c, Player p = null)
    {
        if (p == null)
        {
            Debug.Log("Player is null");

        }
        if (p != null)
        {
            p.RemoveCard(c);
            Destroy(c.gameObject);
            if (p.cardsPanel.cards.Count == 1 && !p.unoClicked)
            {
                ApplyUnoCharge(CurrentPlayer);
            }
            GameManager.PlaySound(draw_card_clip);
        }

        if (p != null)
        {
            if (p.cardsPanel.cards.Count == 0)
            {
                Invoke("SetupGameOver", 2f);
                return;
            }

            if (c.Type == CardType.Other)
            {
                //CurrentPlayer.Timer = true;
                CurrentPlayer.choosingColor = true;
                if (CurrentPlayer.isUserPlayer)
                {
                    colorChoose.ShowPopup();
                }
            }
            if (c.Value == CardValue.Reverse)
            {
                clockwiseTurn = !clockwiseTurn;
                cardEffectAnimator.Play(clockwiseTurn ? "ClockWiseAnim" : "AntiClockWiseAnim");
                Invoke("NextTurn", 1.5f);
            }
            else if (c.Value == CardValue.Skip)
            {
                NextPlayerIndex();
                CurrentPlayer.ShowMessage("Turn Skipped!");
                Invoke("NextTurn", 1.5f);
            }
            else if (c.Value == CardValue.DrawTwo)
            {
                NextPlayerIndex();
                CurrentPlayer.ShowMessage("+2");
                wildCardParticle.Emit(30);
                StartCoroutine(DealCardsToPlayer(CurrentPlayer, 2, .5f));
                NextPlayerIndex();
                Invoke("NextTurn", 1.5f);
            }
            else
            {
                NextTurn();
            }
        }
    }

    public Card CreateCard(CardType type, CardValue value)
    {
        GameObject cardObject = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, 0)), players[currentPlayerIndex].transform);  // Create a new GameObject
        cardObject.transform.localPosition = Vector3.zero;
        Card card = cardObject.GetComponent<Card>();
        card.Type = type;
        card.Value = value;
        return card;
    }

    private Card CreateCard(CardType type, CardValue value, GameObject CardPileContainer)
    {
        GameObject cardObject = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, 0)), CardPileContainer.transform);  // Create a new GameObject
        cardObject.transform.localPosition = Vector3.zero;
        Card card = cardObject.GetComponent<Card>();
        card.Type = type;
        card.Value = value;

        return card;
    }

    /*    public Card GetCardById(int cardID)
        {
            // Loop through the list and find the card with the matching cardID
            if (CardManager.instance.allCards.ContainsKey(cardID))
            {
                return CardManager.instance.allCards[cardID];
            }
            return null;  // Return null if no card with the given ID is found
        }*/

    [PunRPC]
    void SyncWastePile(CardType cardType, CardValue cardValue)
    {
        CurrentType = cardType;
        CurrentValue = cardValue;
        Card card = CreateCard(cardType, cardValue);
        Debug.Log("Card is " + cardType + " " + cardValue);
        if (card != null)
        {
            // Sync the waste pile visually
            wasteCards.Add(card);
            card.IsOpen = true;
            card.transform.SetParent(cardWastePile.transform, true);
            card.SetTargetPosAndRot(new Vector3(Random.Range(-15f, 15f), Random.Range(-15f, 15f), 1),
                card.transform.localRotation.eulerAngles.z + Random.Range(-15f, 15f));
        }
    }
    public void RPC_SyncWastePile(Card card)
    {
        /*if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncWastePile", RpcTarget.All, card.Type, card.Value);
            photonView.RPC("UpdatePos", RpcTarget.All);
        }*/
        photonView.RPC("SyncWastePile", RpcTarget.All, card.Type, card.Value);
        photonView.RPC("UpdatePos", RpcTarget.All);
    }

    [PunRPC]
    void UpdatePos()
    {
        foreach (var player in players)
        {
            player.cardsPanel.UpdatePos();
        }
    }

    #region
    public void NextTurn()
    {
        /*if (PhotonNetwork.IsMasterClient)
        {
            int step = clockwiseTurn ? 1 : -1;
            currentPlayerIndex = Mod(currentPlayerIndex + step, players.Count);
            int CurrentPlayerID = players[currentPlayerIndex].photonView.OwnerActorNr;
            // Notify all clients of the turn change
            photonView.RPC("UpdatePlayerTurn", RpcTarget.All, CurrentPlayerID);
        }*/
        int step = clockwiseTurn ? 1 : -1;
        currentPlayerIndex = Mod(currentPlayerIndex + step, players.Count);
        int CurrentPlayerID = players[currentPlayerIndex].photonView.OwnerActorNr;
        // Notify all clients of the turn change
        photonView.RPC("UpdatePlayerTurn", RpcTarget.All, CurrentPlayerID);
    }
    public void NextPlayerIndex()
    {
        int step = clockwiseTurn ? 1 : -1;
        currentPlayerIndex = Mod(currentPlayerIndex + step, players.Count);
    }

    [PunRPC]
    void UpdatePlayerTurn(int photonPlayerID)
    {
        /*if (playerIndex >= 0 && playerIndex < players.Count)
        {
            // Iterate over all players and set their timer based on the current player's turn
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].photonView.ControllerActorNr == players[playerIndex].photonView.ControllerActorNr)
                {
                    players[i].SetMyTurn(true);
                    Debug.Log(players[i].parentGO.name + "'s turn");
                }

                else
                {
                    // Ensure MyTurn is false for all other players
                    players[i].SetMyTurn(false);
                    Debug.Log("Not" + players[i].parentGO.name + "'s turn");
                }
            }


        }
        else
        {
            Debug.LogError("Invalid playerIndex received: " + playerIndex);
        }*/
        foreach (Player player in players)
        {
            if (player.photonView.OwnerActorNr == photonPlayerID)
            {
                // This player is the current player

                player.SetMyTurn(true);

            }
            else
            {
                // Not the current player, disable their turn
                player.SetMyTurn(false);
            }
        }

    }
    #endregion

    public void EndTurn()
    {
        NextTurn();
    }

    private int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public void OnColorSelect(int i)
    {
        if (!colorChoose.isOpen) return;
        colorChoose.HidePopup();

        SelectColor(i);
    }

    public void SelectColor(int i)
    {
        CurrentPlayer.Timer = false;
        CurrentPlayer.choosingColor = false;

        CurrentType = (CardType)i;
        cardEffectAnimator.Play("DrawFourAnim");
        if (CurrentValue == CardValue.Wild)
        {
            wildCardParticle.gameObject.SetActive(true);
            wildCardParticle.Emit(30);
            Invoke("NextTurn", 1.5f);
            GameManager.PlaySound(choose_color_clip);
        }
        else
        {
            NextPlayerIndex();
            CurrentPlayer.ShowMessage("+4");
            StartCoroutine(DealCardsToPlayer(CurrentPlayer, 4, .5f));
            NextPlayerIndex();
            Invoke("NextTurn", 2f);
            GameManager.PlaySound(choose_color_clip);
        }
    }

    public void EnableDeckClick()
    {
        arrowObject.SetActive(true);
    }

    public void OnDeckClick()
    {
        if (!setup) return;

        if (arrowObject.activeInHierarchy)
        {
            arrowObject.SetActive(false);
            CurrentPlayer.pickFromDeck = true;
            PickCardFromDeck(CurrentPlayer, true);
            if (CurrentPlayer.cardsPanel.AllowedCard.Count == 0 || (!CurrentPlayer.Timer && CurrentPlayer.isUserPlayer))
            {
                //CurrentPlayer.OnTurnEnd();
                NextTurn();
            }
            else
            {
                CurrentPlayer.UpdateCardColor();
            }
        }
        else if (!CurrentPlayer.pickFromDeck && CurrentPlayer.isUserPlayer)
        {
            PickCardFromDeck(CurrentPlayer, true);
            CurrentPlayer.pickFromDeck = true;
            CurrentPlayer.UpdateCardColor();
        }
    }

    public void EnableUnoBtn()
    {
        unoBtn.GetComponent<Button>().interactable = true;
    }

    public void DisableUnoBtn()
    {
        unoBtn.GetComponent<Button>().interactable = false;
    }

    public void OnUnoClick()
    {
        DisableUnoBtn();
        CurrentPlayer.ShowMessage("Uno!", true);
        CurrentPlayer.unoClicked = true;
        GameManager.PlaySound(uno_btn_clip);
    }

    public void ApplyUnoCharge(Player p)
    {
        DisableUnoBtn();
        CurrentPlayer.ShowMessage("Uno Charges");
        StartCoroutine(DealCardsToPlayer(p, 2, .3f));
    }

    public void SetupGameOver()
    {
        gameOver = true;

        // Remove players not in the room
        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (!players[i].isInRoom)
            {
                players.RemoveAt(i);
            }
        }

        // Sort players by total points (highest points first)
        players.Sort((x, y) => y.GetTotalPoints().CompareTo(x.GetTotalPoints()));

        // Assign ranks to players based on their position in the sorted list
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Rank = i + 1; // Rank starts from 1
        }

        // Display ranks (this is just an example; modify it based on your UI)
        foreach (var player in players)
        {
            Debug.Log($"{player.avatarName} is ranked {player.Rank} with {player.GetTotalPoints()} points.");
        }

        // Handle the winner and the rest of the game-over logic
        var roundWinner = players[0];

        // Remove the winner of the round from the game
        players.RemoveAt(0);

        // If only one player remains, declare them as the final winner
        if (players.Count == 1)
        {
            var finalWinner = players[0];
            starParticle.gameObject.SetActive(finalWinner.isUserPlayer);
            playerObject[0].GetComponentsInChildren<Image>()[1].sprite = finalWinner.avatarImage.sprite;
            gameOverPopup.SetActive(true);
            gameOverPopup.GetComponent<Animator>().enabled = finalWinner.isUserPlayer;
            gameOverPopup.GetComponentInChildren<Text>().text = finalWinner.isUserPlayer ? "You are the last man standing!" : "You lost. Better luck next time!";
            GameManager.PlaySound(finalWinner.isUserPlayer ? music_win_clip : music_loss_clip);
        }
        else
        {
            // Continue the game with the remaining players
            gameOverPopup.SetActive(false);
            setup = true;

            // Continue with the next player in the turn order
            NextTurn();
        }
    }
}
    [System.Serializable]
    public class AvatarProfile
    {
        public int avatarIndex;
        public string avatarName;
    }

    public class AvatarProfiles
    {
        public List<AvatarProfile> profiles;
    }
