using UnityEngine;
using System.Collections;
using KMHelper;
using System.Linq;

public class MonsplodeCardModule : MonoBehaviour
{
	public KMBombInfo Info;
	public KMBombModule Module;
	public KMAudio Audio;
	public CardsDataObject CD;
	public KMSelectable next, prev, keep, trade;
	public TextMesh deckTM, offerTM, deckVersion, offerVersion, deckRarity, offerRarity, deckFlavor, offerFlavor;
	public SpriteRenderer deckSR, offerSR, deckBack, offerBack;
	public int deckSize, offerCount;
	int lowestCardInDeck = 0;
	bool isActivated = false;
	private static int _moduleIdCounter = 1;
	private int _moduleId;
	public char[] raritySymbols;
	public Card[] deck;
	public Card offer;
	public SpriteRenderer[] DeckFlaps, DeckNormals, DeckBents, OfferFlaps, OfferNormals, OfferBents, StageCount;
	public int currentOffer = 0, correctOffer = 0, currentDeck = 0;
	public GameObject deckGlitters, offerGlitters, deckFrontFace, offerFrontFace;
	public enum SerialStates
	{
		numbers, letters, numlet, letnum
	};
	SerialStates state;
	private void PrintDebug(string str)
	{
		Debug.LogFormat("[MonsplodeCards #{0}] " + str, _moduleId);
	}
	void Start()
	{
		_moduleId = _moduleIdCounter++;

	}
	private void Awake()
	{
		Module.OnActivate += ActivateModule;
		keep.OnInteract += delegate ()
		{
			KeepPress();
			return false;
		};
		trade.OnInteract += delegate ()
		{
			TradePress();
			return false;
		};
		prev.OnInteract += delegate ()
		{
			PrevCardPress();
			return false;
		};
		next.OnInteract += delegate ()
		{
			NextCardPress();
			return false;
		};
		// Turnoff cards for start.
		deckBack.enabled = offerBack.enabled = true; // Enable Card Backs
		deckFrontFace.SetActive(false);
		offerFrontFace.SetActive(false);
		foreach (SpriteRenderer sr in DeckNormals)
			sr.enabled = false;
		foreach (SpriteRenderer sr in DeckBents)
			sr.enabled = false;
		foreach (SpriteRenderer sr in DeckFlaps)
			sr.enabled = false;
		foreach (SpriteRenderer sr in OfferNormals)
			sr.enabled = false;
		foreach (SpriteRenderer sr in OfferBents)
			sr.enabled = false;
		foreach (SpriteRenderer sr in OfferFlaps)
			sr.enabled = false;
		/*deckSR.enabled = offerSR.enabled = false; // Turn Off Monsplodes
		deckGlitters.SetActive(false);
		deckTM.text = "";
		offerTM.text = "";*/
	}
	void ShowCardBacks()
	{
		isActivated = false;
		deckFrontFace.SetActive(false);
		offerFrontFace.SetActive(false);
		deckBack.enabled = offerBack.enabled = true;
		foreach (SpriteRenderer sr in DeckNormals)
			sr.enabled = false;
		foreach (SpriteRenderer sr in DeckBents)
			sr.enabled = false;
		foreach (SpriteRenderer sr in DeckFlaps)
			sr.enabled = false;
		foreach (SpriteRenderer sr in OfferNormals)
			sr.enabled = false;
		foreach (SpriteRenderer sr in OfferBents)
			sr.enabled = false;
		foreach (SpriteRenderer sr in OfferFlaps)
			sr.enabled = false;
		UpdateStageCounter();
	}
	void ActivateModule()
	{
		Init();
		isActivated = true;
		deckFrontFace.SetActive(true);
		offerFrontFace.SetActive(true);
		/*deckSR.enabled = true;
		offerSR.enabled = true;*/
		deckBack.enabled = offerBack.enabled = false;
		UpdateCardVisuals();
		//screenSR.enabled = true;
	}
	void UpdateStageCounter()
	{
		for (int i = 0; i < 3; i++)
			if (i < correctOffer)
				StageCount[i].color = Color.green;
			else
				StageCount[i].color = new Color(1, 1, 1, 0.4f);
	}
	float CalculateCardValue(Card c)
	{
		//Debug.Log(c.monsplode);
		int value = (int) CD.inital[c.monsplode][(int) state];
		//Debug.Log("THISSS"+ Vector3.zero[0]);
		if (c.printChar - 'A' + 1 == c.printDigit)
		{
			PrintDebug("Print version letter " + c.printChar + "'s alphabetical position is equal to print version numeral. Card is fake with 0 value.");
			return 0f;
		}
		string[] formalStateNames = new string[4] { "##", "XX", "#X", "X#" };
		PrintDebug("First 2 characters of Serial: " + formalStateNames[(int) state] + " | Monsplode: " + CD.names[c.monsplode].Replace('\n', ' ') + " | Using initial value: " + value);
		// Indicator part
		int on = 0, off = 0;
		foreach (string ind in Info.GetOnIndicators())
			if (ind.Contains(c.printChar.ToString()))
				on++;
		foreach (string ind in Info.GetOffIndicators())
			if (ind.Contains(c.printChar.ToString()))
				off++;
		value += on - off;
		PrintDebug("There are " + on + " lit, " + off + " unlit indicators that contain the letter '" + c.printChar + "'.\n Current value: " + value);
		// Battery part
		if (Info.GetBatteryCount() == 0)
			PrintDebug("There are no batteries, keep the current value.");
		else if (Info.GetBatteryCount() < c.printDigit)
		{
			value++;
			PrintDebug("Print version numeral is greater than battery count. Add 1.\nCurrent value: " + value);
		}
		else if (Info.GetBatteryCount() > c.printDigit)
		{
			value--;
			PrintDebug("Print version numeral is less than battery count. Subtract 1.\nCurrent value: " + value);
		}
		else
		{
			value += 2;
			PrintDebug("Print version numeral is equal to battery count. Add 2.\nCurrent value: " + value);
		}
		// Multipler part
		float[] mulLookup = new float[] { 1f, 1.25f, 1.5f, 1.75f };
		float mul = mulLookup[c.rarity];
		PrintDebug("Base rarity multipler from the symbol: " + mul);
		if (c.isHolographic)
		{
			mul += 0.5f;
			PrintDebug("Card is holographic. Add 0.5. Current multipler: " + mul);
		}
		mul -= 0.25f * c.bentCorners;
		PrintDebug("There are " + c.bentCorners + " bent corners. Subtract " + c.bentCorners * 0.25f + ". Current multipler: " + mul);
		PrintDebug("Final value is: " + value + " X " + mul + " = " + value * mul);
		return value * mul;
	}

	Card MakeCard()
	{
		int bent = 0;
		for (int i = 0; i < 4; i++)
			if (Random.value < 0.20)
				bent++;
		Card c = new Card(Random.Range(0, CD.size), Random.Range(0, 4), Random.Range(1, 10), (char) Random.Range('A', 'I' + 1), Random.value < 0.21, bent); // SHOuLD RARE CARDS BE RARE? Bent corners and holo
		string[] mulNames = new string[] { "Common", "Uncommon", "Rare", "Very Rare" };
		PrintDebug("Monsplode: " + CD.names[c.monsplode].Replace('\n', ' ') + " | Rarity: " + mulNames[c.rarity] + "\nPrint Version: " + c.printChar + c.printDigit + " | Holographic: " + c.isHolographic + " | Bent Corners: " + c.bentCorners);
		c.value = CalculateCardValue(c);
		return c;
	}
	void CalculateLowestCardInDeck()
	{
		float score = 99;
		for (int i = 0; i < deckSize; i++)
			if (score > deck[i].value)
			{
				score = deck[i].value;
				lowestCardInDeck = i;
			}
	}
	void NextCardPress()
	{
		if (correctOffer == offerCount || !isActivated) // This means module is passed
			return;

		currentDeck = Mathf.Min(currentDeck + 1, deckSize - 1);
		UpdateCardVisuals();
	}
	void PrevCardPress()
	{
		if (correctOffer == offerCount || !isActivated) // This means module is passed
			return;

		currentDeck = Mathf.Max(currentDeck - 1, 0);
		UpdateCardVisuals();
	}
	void KeepPress()
	{
		if (correctOffer == offerCount || !isActivated) // This means module is passed
			return;

		if (deck[lowestCardInDeck].value < offer.value)
		{
			PrintDebug("Wrong! Deck card #" + (lowestCardInDeck + 1) + " had lower value than the offer.");
			Module.HandleStrike();
		}
		else
		{
			PrintDebug("Keeping your cards was the correct decision!");
			correctOffer++;
		}
		if (correctOffer == offerCount)
		{
			//TURNOFF INTERACTIONS
			// SHOW CARDBACK
			Module.HandlePass();
			ShowCardBacks();
			return;
		}
		currentOffer++;
		ResetOffer();
		CalculateLowestCardInDeck();
		//PrintOutDeck();
		UpdateCardVisuals();
	}
	void TradePress()
	{
		if (correctOffer == offerCount || !isActivated) // This means module is passed
			return;

		if (deck[lowestCardInDeck].value > offer.value || deck[currentDeck].value > deck[lowestCardInDeck].value)
		{
			if (deck[lowestCardInDeck].value > offer.value)
				PrintDebug("Wrong! All of your cards had higher value than the offer. Offered card is now deck card #" + (currentDeck + 1) + ".");
			else
				PrintDebug("Wrong! Deck card #" + (lowestCardInDeck + 1) + " had lower value than the deck card #" + (currentDeck + 1) + ". Offered card is now deck card #" + (currentDeck + 1) + ".");
			Module.HandleStrike();
		}
		else
		{
			PrintDebug("You did the right trade! Offered card is now deck card #" + (currentDeck + 1) + ".");
			correctOffer++;
		}
		currentOffer++;
		if (correctOffer == offerCount)
		{
			//TURNOFF INTERACTIONS
			// SHOW CARDBACK
			Module.HandlePass();
			ShowCardBacks();
			return;
		}
		deck[currentDeck] = offer;
		ResetOffer();
		currentOffer++;
		CalculateLowestCardInDeck();
		UpdateCardVisuals();
		//PrintOutDeck();



	}
	/*void PrintOutDeck()
	{
		for (int i = 0; i < deckSize; i++)
		{
			PrintDebug("Deck: " + i + " Value: " + deck[i].value);
		}
		for (int i = 0; i < offerCount; i++)
		{
			PrintDebug("Offer: " + i + " Value: " + offers[i].value);
		}
	}*/
	/*
	string CardToText(Card c)
	{
		return "Monsplode: " + CD.names[c.monsplode].Replace('\n',' ') + "\nRarity: " + raritySymbols[c.rarity] + "\nPrint Version: " + c.printChar + c.printDigit + "\nHolographic: " + c.isHolographic + "\nBent Corners: " + c.bentCorners;
	}
	*/
	void HandleFlapVisuals(bool[] which, SpriteRenderer[] normal, SpriteRenderer[] bent, SpriteRenderer[] flap)
	{
		for (int i = 0; i < 4; i++)
			if (which[i])
			{
				normal[i].enabled = false;
				bent[i].enabled = flap[i].enabled = true;
			}
			else
			{
				normal[i].enabled = true;
				bent[i].enabled = flap[i].enabled = false;
			}
	}
	bool[] GenerateFlapState(int seed, int count) // MAke flaps random for each card but make it stable per card.
	{
		bool[] arr = new bool[4];
		for (int i = 0; i < 4; i++)
			arr[i] = false;
		int add = seed % 4, start = seed % 5;
		if (add == 0) add = -1;
		if (add == 2) add = -3;
		if (start == 4) start = 2;
		for (int i = 0; i < count; i++)
		{
			arr[start] = true;
			start = (start + add + 4) % 4;
		}
		return arr;
	}
	void UpdateCardVisuals()
	{
		HandleFlapVisuals(GenerateFlapState(deck[currentDeck].printDigit + deck[currentDeck].printChar, deck[currentDeck].bentCorners), DeckNormals, DeckBents, DeckFlaps); // Bent Corners
		HandleFlapVisuals(GenerateFlapState(offer.printDigit + offer.printChar, offer.bentCorners), OfferNormals, OfferBents, OfferFlaps);

		deckRarity.text = raritySymbols[deck[currentDeck].rarity] + ""; // Rarity Symbol
		offerRarity.text = raritySymbols[offer.rarity] + "";

		deckVersion.text = deck[currentDeck].printChar + "" + deck[currentDeck].printDigit; // Print Version
		offerVersion.text = offer.printChar + "" + offer.printDigit;

		deckSR.sprite = CD.sprites[deck[currentDeck].monsplode]; // Monsplode Image
		offerSR.sprite = CD.sprites[offer.monsplode];

		deckGlitters.SetActive(deck[currentDeck].isHolographic); //Holographic
		offerGlitters.SetActive(offer.isHolographic);

		deckTM.text = CD.names[deck[currentDeck].monsplode]; // Card Name
		offerTM.text = CD.names[offer.monsplode];

		deckFlavor.text = CD.flavorText[deck[currentDeck].monsplode]; // Flavor Text
		offerFlavor.text = CD.flavorText[offer.monsplode];

		UpdateStageCounter();
	}
	void ResetOffer()
	{
		PrintDebug("Generating the offered card #" + (currentOffer + 1));
		offer = MakeCard();
		PrintDebug("Value of offered card #" + (currentOffer + 1) + ": " + offer.value);
	}
	void Init()
	{
		// Calculate serial state for monplode initial variation
		char first = Info.GetSerialNumber()[0], second = Info.GetSerialNumber()[1];
		if ('A' <= first && first <= 'Z')
		{
			if ('A' <= second && second <= 'Z')
				state = SerialStates.letters;
			else
				state = SerialStates.letnum;
		}
		else
		{
			if ('A' <= second && second <= 'Z')
				state = SerialStates.numlet;
			else
				state = SerialStates.numbers;
		}
		// Create both deck and offers
		deck = new Card[deckSize];
		for (int i = 0; i < deckSize; i++)
		{
			PrintDebug("Generating the deck card #" + (i + 1));
			deck[i] = MakeCard();
			PrintDebug("Value of deck card #" + (i + 1) + ": " + deck[i].value);
		}

		ResetOffer();

		//PrintOutDeck();

		CalculateLowestCardInDeck();
	}

	public string TwitchHelpMessage = "Cycle through the cards using !{0} cycle. To keep the current card use !{0} keep. To trade the current card !{0} trade.";
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] split = command.ToLowerInvariant().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
		string last = split.Last();

		if (split[0] == "cycle" && split.Length == 1)
		{
			yield return null;

			for (int i = 0; i < 2; i++)
			{
				next.OnInteract();
				yield return new WaitForSeconds(7);
			}

			for (int i = 0; i < 2; i++)
			{
				prev.OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}
		else if ((last == "trade" || last == "t") && (split.Length == 1 || (split.Length == 2 && split[0] == "press")))
		{
			yield return null;

			keep.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
		else if ((last == "keep" || last == "k") && (split.Length == 1 || (split.Length == 2 && split[0] == "press")))
		{
			yield return null;

			trade.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
	}
}
