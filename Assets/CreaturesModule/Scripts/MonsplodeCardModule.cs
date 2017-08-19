using UnityEngine;
using System.Collections;

public class MonsplodeCardModule : MonoBehaviour
{
	public KMBombInfo Info;
	public KMBombModule Module;
	public KMAudio Audio;
	public CardsDataObject CD;
	public KMSelectable next,prev,keep,trade;
	public int deckSize, offerCount;
	int lowestCardInDeck=0;
	bool isActivated = false;
	private static int _moduleIdCounter = 1;
	private int _moduleId;
	public Card[] offers,deck;

	public int currentOffer=0, currentDeck=0;

	//FOR TESTING
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
			PrevCardPress();
		if (Input.GetKeyDown(KeyCode.LeftArrow))
			NextCardPress();
		if (Input.GetKeyDown(KeyCode.K))
			KeepPress();
		if (Input.GetKeyDown(KeyCode.L))
			TradePress();
	}
	private void PrintDebug(string str)
	{
		Debug.LogFormat("[MonsplodeCards #{0}] "+str, _moduleId);
	}
	void Start()
	{
		_moduleId = _moduleIdCounter++;
		Init();
		Module.OnActivate += ActivateModule;
		//Info.OnBombExploded += BombExploded;
	}

	void ActivateModule()
	{
		isActivated = true;
		//screenSR.enabled = true;
	}

	float CalculateCardValue(Card c)
	{
		return Random.Range(0f,10f);
	}

	Card MakeCard()
	{
		Card c = new Card(Random.Range(0,CD.size) , Random.Range(0,4), Random.Range(0,10) , (char)Random.Range('A','E'+1)); // SHOuLD RARE CARDS BE RARE?
		c.value = CalculateCardValue(c);
		return c;
	}
	void CalculateLowestCardInDeck()
	{
		float score = 99;
		for (int i = 0; i < deckSize; i++)
			if (score >deck[i].value)
			{
				score = deck[i].value;
				lowestCardInDeck = i;
			}
	}
	void NextCardPress()
	{
		currentDeck = (currentDeck + 1) % deckSize;
		PrintDebug(currentDeck+"");
	}
	void PrevCardPress()
	{
		currentDeck = (currentDeck - 1 + deckSize) % deckSize;
		PrintDebug(currentDeck+"");
	}
	void KeepPress()
	{
		if(deck[lowestCardInDeck].value < offers[currentOffer].value)
			Module.HandleStrike();
		currentOffer++;
		CalculateLowestCardInDeck();
		PrintOutDeck();
		if (currentOffer == offerCount)
		{
			//TURNOFF INTERACTIONS
			Module.HandlePass();
		}
	}
	void TradePress()
	{
		if (deck[lowestCardInDeck].value > offers[currentOffer].value || deck[currentDeck].value>deck[lowestCardInDeck].value)
			Module.HandleStrike();
		deck[currentDeck] = offers[currentOffer];
		currentOffer++;
		CalculateLowestCardInDeck();
		PrintOutDeck();
		if (currentOffer == offerCount)
		{
			//TURNOFF INTERACTIONS
			Module.HandlePass();
		}

	}
	void PrintOutDeck()
	{
		for (int i = 0; i < deckSize; i++)
		{
			PrintDebug("Deck: " + i + " Value: " + deck[i].value);
		}
		for (int i = 0; i < offerCount; i++)
		{
			PrintDebug("Offer: " + i + " Value: " + offers[i].value);
		}
	}
	void Init()
	{
		offers = new Card[offerCount];
		deck = new Card[deckSize];
		for (int i = 0; i < deckSize; i++)
		{
			deck[i] = MakeCard();
		}
		for (int i = 0; i < offerCount; i++)
		{
			offers[i] = MakeCard();
		}
		PrintOutDeck();
		CalculateLowestCardInDeck();
	}
}
