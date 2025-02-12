﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComPlayer : MonoBehaviour
{
    public GameObject CardPanelBG;
    public PlayerCards cardsPanel;
    public string playerName;
    public bool isUserPlayer; // Flag to identify the local user player
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

    void Start()
    {
        Timer = false;
        // Initialize card visibility based on whether this is the user player
        UpdateCardVisibility();
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
            if (choosingColor)
            {
                if (isUserPlayer)
                {
                    ComputerGamePlayManager.instance.colorChoose.HidePopup();
                }
                ChooseBestColor();
            }
            else if (ComputerGamePlayManager.instance.IsDeckArrow)
            {
                ComputerGamePlayManager.instance.OnDeckClick();
            }
            else if (cardsPanel.AllowedCard.Count > 0)
            {
                OnCardClick(FindBestPutCard());
            }
            else
            {
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
                ComputerGamePlayManager.instance.EnableDeckClick();
            }
        }
        else
        {
            StartCoroutine(DoComputerTurn());
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
                ComputerGamePlayManager.instance.EnableUnoBtn();
            }
            else
            {
                ComputerGamePlayManager.instance.DisableUnoBtn();
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

    public void OnCardClick(Card c)
    {
        if (Timer)
        {
            ComputerGamePlayManager.instance.PutCardToWastePile(c, this);
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

    public IEnumerator DoComputerTurn()
    {
        if (cardsPanel.AllowedCard.Count > 0)
        {
            StartCoroutine(ComputerTurnHasCard(0.25f));
        }
        else
        {
            yield return new WaitForSeconds(Random.Range(1f, totalTimer * .3f));
            ComputerGamePlayManager.instance.EnableDeckClick();
            ComputerGamePlayManager.instance.OnDeckClick();

            if (cardsPanel.AllowedCard.Count > 0)
            {
                StartCoroutine(ComputerTurnHasCard(0.2f));
            }
        }
    }

    private IEnumerator ComputerTurnHasCard(float unoCoef)
    {
        bool unoClick = false;
        float unoPossibality = ComputerGamePlayManager.instance.UnoProbability / 100f;

        if (Random.value < unoPossibality && cardsPanel.cards.Count == 2)
        {
            yield return new WaitForSeconds(Random.Range(1f, totalTimer * unoCoef));
            ComputerGamePlayManager.instance.OnUnoClick();
            unoClick = true;
        }

        yield return new WaitForSeconds(Random.Range(1f, totalTimer * (unoClick ? unoCoef : unoCoef * 2)));
        OnCardClick(FindBestPutCard());
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
            ComputerGamePlayManager.instance.SelectColor(Random.Range(1, 5));
        }
        else
        {
            if (Random.value < 0.7f)
                ComputerGamePlayManager.instance.SelectColor((int)temp);
            else
                ComputerGamePlayManager.instance.SelectColor(Random.Range(1, 5));
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
