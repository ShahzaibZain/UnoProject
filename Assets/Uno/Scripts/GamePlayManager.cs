using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        yield return new WaitForSeconds(3);
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
            BroadcastPlayerOrder();
            photonView.RPC("DealCardsMethod", RpcTarget.All, 7);
            //photonView.RPC("UpdatePlayerTurn", RpcTarget.All, currentPlayerIndex);
        }
    }
    #region SORT PLAYERS LIST
    public void BroadcastPlayerOrder()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Sort the players list by ActorNumber (or any other property you prefer)
            players = players.OrderBy(p => p.photonView.Owner.ActorNumber).ToList();

            // Create an array of player ActorNumbers to broadcast
            int[] playerOrder = players.Select(p => p.photonView.Owner.ActorNumber).ToArray();

            // Use RPC to send the order to all clients
            photonView.RPC("SyncPlayerOrder", RpcTarget.AllBuffered, playerOrder);
        }
    }

    // Sync method to be called on all clients
    [PunRPC]
    public void SyncPlayerOrder(int[] playerOrder)
    {
        // Create a new sorted list using the received order
        List<Player> sortedPlayers = new List<Player>();

        // Match the received ActorNumbers to players on this client
        foreach (int actorNumber in playerOrder)
        {
            Player player = players.Find(p => p.photonView.Owner.ActorNumber == actorNumber);
            if (player != null)
            {
                sortedPlayers.Add(player);
            }
        }

        // Update the local players list to match the master client's order
        players = sortedPlayers;

        Debug.Log("Players list synchronized according to the master client.");
    }
    #endregion

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
            PickCardFromDeck(p);
            yield return new WaitForSeconds(cardDealTime);
        }
    }

/*    IEnumerator DealCardsToNextPlayer(int NoOfCard = 1, float delay = 0f)
    {
        int step = clockwiseTurn ? 1 : -1;
        int NextPlayerIndex = Mod(currentPlayerIndex + step, players.Count);
        Player NextPlayer = players[NextPlayerIndex];
        yield return new WaitForSeconds(delay);
        for (int t = 0; t < NoOfCard; t++)
        {
            PickCardFromDeck(NextPlayer);
            yield return new WaitForSeconds(cardDealTime);
        }
    }*/

    /*    public Card PickCardFromDeck(Player p, bool updatePos = false)
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
        }*/

    [PunRPC]
    public void SyncPickCardFromDeck(int cardIndex, int playerID)
    {
        Player player = players.Find(p => p.photonView.OwnerActorNr == playerID);
        Debug.Log("Player is " + player.transform.parent.name);
        if (player != null && cards.Count > cardIndex)
        {
            Card pickedCard = cards[cardIndex];
            player.AddCard(pickedCard);
            pickedCard.IsOpen = player.isUserPlayer;
            cards.RemoveAt(cardIndex);  // Remove card from deck
            player.cardsPanel.UpdatePos();  // Update the player's card positions
        }
    }

    public void PickCardFromDeck(Player p)
    {
        if (PhotonNetwork.IsMasterClient && cards.Count > 0)
        {
            int cardIndex = 0;  // Always pick the top card from the deck
            photonView.RPC("SyncPickCardFromDeck", RpcTarget.All, cardIndex, p.photonView.OwnerActorNr);
        }
    }
    public void TakeCardFromDeck(Player p)
    {
        if (cards.Count > 0)
        {
            int cardIndex = 0;  // Always pick the top card from the deck
            photonView.RPC("SyncPickCardFromDeck", RpcTarget.All, cardIndex, p.photonView.OwnerActorNr);
        }
    }

    public void PutCardToWastePile(Card c, Player p = null)
    {
        if (p == null)
        {
            Debug.Log("Player is null");

        }
        if (p != null)
        {
            Debug.Log("PutCardToWastePile player " + p.parentGO.name);
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
                CurrentPlayer.Timer = true;
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
                //NextPlayerIndex();
                int step = clockwiseTurn ? 1 : -1;
                int tempPlayerIndex = Mod(currentPlayerIndex + step, players.Count); 
                players[tempPlayerIndex].ShowMessage("Turn Skipped!");
                Invoke("SkipTurn", 1.5f);
            }
            else if (c.Value == CardValue.DrawTwo)
            {
                //NextPlayerIndex();
                int step = clockwiseTurn ? 1 : -1;
                int tempPlayerIndex = Mod(currentPlayerIndex + step, players.Count);
                players[tempPlayerIndex].ShowMessage("+2");
                wildCardParticle.Emit(30);
                StartCoroutine(DealCardsToPlayer(players[tempPlayerIndex], 2, .5f));
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
        Debug.Log(players[currentPlayerIndex].MyTurn + "Current player index: " + currentPlayerIndex);
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

    #region NEXT TURN
    public void SkipTurn()
    {
        // Notify all clients of the turn change
        /*        if (PhotonNetwork.IsMasterClient)
        */
        photonView.RPC("SkipNextPlayerTurn", RpcTarget.All);

    }
    [PunRPC]
    void SkipNextPlayerTurn()
    {
        int step = clockwiseTurn ? 2 : -2;
        currentPlayerIndex = Mod(currentPlayerIndex + step, players.Count);
        Debug.Log("CurrentPlayerIndex Updated: " + currentPlayerIndex);
        int CurrentPlayerID = players[currentPlayerIndex].photonView.OwnerActorNr;
        foreach (Player player in players)
        {
            if (player.photonView.OwnerActorNr == CurrentPlayerID)
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

    public void NextTurn()
    {
        // Notify all clients of the turn change
/*        if (PhotonNetwork.IsMasterClient)
*/            photonView.RPC("NextPlayerTurn", RpcTarget.All);

    }

    [PunRPC]
    void NextPlayerTurn()
    {
        int step = clockwiseTurn ? 1 : -1;
        currentPlayerIndex = Mod(currentPlayerIndex + step, players.Count);
        Debug.Log("CurrentPlayerIndex Updated: " + currentPlayerIndex);
        int CurrentPlayerID = players[currentPlayerIndex].photonView.OwnerActorNr;
        foreach (Player player in players)
        {
            if (player.photonView.OwnerActorNr == CurrentPlayerID)
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

    public void UpdateTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Notify all clients of the turn change
            photonView.RPC("UpdatePlayerTurn", RpcTarget.All, players[currentPlayerIndex].photonView.OwnerActorNr);
        }
    }

    public void NextPlayerIndex()
    {
        int step = clockwiseTurn ? 1 : -1;
        currentPlayerIndex = Mod(currentPlayerIndex + step, players.Count);
    }

    [PunRPC]
    void UpdatePlayerTurn(int CurrentPlayerID)
    {
        foreach (Player player in players)
        {
            if (player.photonView.OwnerActorNr == CurrentPlayerID)
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

    private int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public void OnColorSelect(int i)
    {
        if (!colorChoose.isOpen) return;
        colorChoose.HidePopup();

        //SelectColor(i);

        photonView.RPC("SelectColor", RpcTarget.All, i);
    }
    [PunRPC]
    public void SelectColor(int i)
    {
//        CurrentPlayer.Timer = false;
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
            /*int step = clockwiseTurn ? 1 : -1;
            int tempPlayerIndex = Mod(currentPlayerIndex + step, players.Count);
            //NextPlayerIndex();
            Debug.Log("tempPlayerIndex: " + tempPlayerIndex);*/
            players[currentPlayerIndex].ShowMessage("+4");
            StartCoroutine(DealCardsToPlayer(players[currentPlayerIndex], 4, .5f));
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
        photonView.RPC("OnDeckClickMethod", RpcTarget.All);
    }

    [PunRPC]
    public IEnumerator OnDeckClickMethod()
    {
        if (!setup) yield return null;

        if (arrowObject.activeInHierarchy)
        {
            arrowObject.SetActive(false);
            CurrentPlayer.pickFromDeck = true;
            Debug.Log("OnDeckClick current player is " + CurrentPlayer.gameObject.transform.parent.name);
            TakeCardFromDeck(CurrentPlayer);
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Changing turn after deck click");
            if (CurrentPlayer.cardsPanel.AllowedCard.Count == 0 || (!CurrentPlayer.Timer && CurrentPlayer.isUserPlayer))
            {
                //CurrentPlayer.OnTurnEnd();
                Invoke("NextTurn", 2f);
            }
            else
            {
                CurrentPlayer.UpdateCardColor();
            }
        }
        else if (!CurrentPlayer.pickFromDeck && CurrentPlayer.isUserPlayer)
        {
            TakeCardFromDeck(CurrentPlayer);
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
