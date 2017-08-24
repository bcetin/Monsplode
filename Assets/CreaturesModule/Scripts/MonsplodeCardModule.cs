using UnityEngine;
using System.Collections;
using KMHelper;

public class MonsplodeCardModule : MonoBehaviour
{
	public KMBombInfo Info;
	public KMBombModule Module;
	public KMAudio Audio;
	public CardsDataObject CD;
	public KMSelectable next,prev,keep,trade;
	public TextMesh deckTM,offerTM;
	public SpriteRenderer deckSR, offerSR;
	public int deckSize, offerCount;
	int lowestCardInDeck=0;
	bool isActivated = false;
	private static int _moduleIdCounter = 1;
	private int _moduleId;
	public char[] raritySymbols;
	public Card[] deck;
	public Card offer;

	public int currentOffer=0,correctOffer=0, currentDeck=0;
	public enum SerialStates
	{
		numbers,letters,both
	};
	SerialStates state;
	//FOR TESTING
	/*
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
	}*/
	private void PrintDebug(string str)
	{
		Debug.LogFormat("[MonsplodeCards #{0}] "+str, _moduleId);
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
		deckSR.enabled = false;
		offerSR.enabled = false;
		deckTM.text = "";
		offerTM.text = "";
	}
	void ShowCardBacks()
	{
		deckSR.enabled = false;
		offerSR.enabled = false;
		deckTM.text = "";
		offerTM.text = "";
	}
	void ActivateModule()
	{
		Init();
		isActivated = true;
		deckSR.enabled = true;
		offerSR.enabled = true;
		UpdateCardVisuals();
		//screenSR.enabled = true;
	}

	float CalculateCardValue(Card c)
	{
		//Debug.Log(c.monsplode);
		int value=(int)CD.inital[c.monsplode][(int)state];
		//Debug.Log("THISSS"+ Vector3.zero[0]);
		if (c.printChar - 'A' + 1 == c.printDigit)
		{
			PrintDebug("Print version letter " + c.printChar + "'s alphabetical position is equal to print version numeral. Card is fake with 0 value.");
			return 0f;
		}
		PrintDebug("First 2 characters of Serial: " + state + " | Monsplode: " + CD.names[c.monsplode] + " | Using initial value: " + value);
		// Indicator part
		int on=0, off=0;
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
			value+=2;
			PrintDebug("Print version numeral is equal to battery count. Add 2.\nCurrent value: " + value);
		}
		// Bob -> =8 rule
		if (c.monsplode == 0) // ADD NEW BOBS HERE IF ANY SHOWS UP
		{
			if(Info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.BOB))
			{
				value = 8;
				PrintDebug("There is a lit BOB indicator and monsplode on the card is a Bob variation.\n Value is now 8.");
			}
		}
		// C4 -> =5 rule
		if (c.printChar == 'C' && c.printDigit == 4)
		{
			value = 5;
			PrintDebug("Print version is 'C4'\n Value is now 5.");
		}
		// H + >2 battery -? replace rule
		if (c.printChar == 'H' && Info.GetBatteryCount()>2)
		{
			value = c.printDigit;
			PrintDebug("Print version letter is 'H' and there are more than 2 batteries. Replacing value with print version numeral. Value is now " + value + ".");
		}
		// 7 && no indicator -> -3 rule
		if (c.printDigit == 7)
		{
			bool tem = true;
			foreach (string s in Info.GetIndicators())
				tem = false;
			if (tem)
			{
				value -=3;
				PrintDebug("Print version numeral is 7 and there are no indicators. Subtract 3.\n Value is now " + value +".");
			}
		}
		// E && Serial port -> +2 rule
		if (c.printChar == 'E' && Info.IsPortPresent(KMBombInfoExtensions.KnownPortType.Serial))
		{
			value += 2;
			PrintDebug("Print version letter is 'E' and there is a Serial port present. Add 2.\n Current value: " + value);
		}
		// 1 -> -1 rule
		if (c.printDigit == -1)
		{
			value -= 1;
			PrintDebug("Print version numeral is 1. Subtract -1.\n Current value: " + value);
		}
		if (value < 0)
		{
			PrintDebug("Value is negative. Card has no value. Value is 0.");
			value = 0;
		}
		// >5 && K in serial -> +3 rule CANCELLED FOR NOW
		/*if (c.printDigit > 5 && Info.GetSerialNumber().Contains("K"))
		{
			value += 3;
			PrintDebug("Print version numeral is 1. Subtract -1.\n Current value: " + value);
		}*/
		float[] mulLookup=new float[]{1f,1.25f,1.5f,1.75f};
		float mul = mulLookup[c.rarity];
		PrintDebug("Base rarity multipler from the symbol: " + mul);
		if (c.isHolographic)
		{
			mul += 0.5f;
			PrintDebug("Card is holographic. Add 0.5. Current multipler: " + mul);
		}
		mul -= 0.25f *c.bentCorners;
		PrintDebug("There are " + c.bentCorners + " bent corners. Subtract " + c.bentCorners * 0.25f + ". Current multipler: " + mul);
		PrintDebug("Final value is: " + value + " X " + mul + " = " + value * mul);
		return value*mul;
	}

	Card MakeCard()
	{
		int bent = 0;
		for (int i = 0; i < 4; i++)
			if(Random.value < 0.20)
				bent++;
		Card c = new Card(Random.Range(0,CD.size) , Random.Range(0,4), Random.Range(1,10) , (char)Random.Range('A','I'+1),Random.value<0.15,bent); // SHOuLD RARE CARDS BE RARE? Bent corners and holo
		string[] mulNames = new string[]{"Common","Uncommon","Rare","Very Rare"};
		PrintDebug("Monsplode: "+CD.names[c.monsplode]+" | Rarity: " + mulNames[c.rarity] +"\nPrint Version: " + c.printChar+c.printDigit+ " | Holographic: "+c.isHolographic+ " | Bent Corners: " + c.bentCorners);
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
		if (correctOffer == offerCount || !isActivated) // This means module is passed
			return;

		currentDeck = Mathf.Min(currentDeck + 1,deckSize-1);
		UpdateCardVisuals();
	}
	void PrevCardPress()
	{
		if (correctOffer == offerCount || !isActivated) // This means module is passed
			return;

		currentDeck = Mathf.Max(currentDeck - 1,0);
		UpdateCardVisuals();
	}
	void KeepPress()
	{
		if (correctOffer == offerCount || !isActivated) // This means module is passed
			return;

		if (deck[lowestCardInDeck].value < offer.value)
		{
			PrintDebug("Wrong! Deck card #" + (lowestCardInDeck+1) +" had lower value than the offer.");
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
			if(deck[lowestCardInDeck].value > offer.value)
				PrintDebug("Wrong! All of your cards had higher value than the offer. Offered card is now deck card #" + (currentDeck+1) + ".");
			else
				PrintDebug("Wrong! Deck card #" + (lowestCardInDeck + 1) + " had lower value than the deck card #"+ (currentDeck + 1) +". Offered card is now deck card #" + (currentDeck + 1) + ".");
			Module.HandleStrike();
		}
		else
		{
			PrintDebug("You did the right trade! Offered card is now deck card #" + (currentDeck+1) + ".");
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
	string CardToText(Card c)
	{
		return "Monsplode: " + CD.names[c.monsplode] + "\nRarity: " + raritySymbols[c.rarity] + "\nPrint Version: " + c.printChar + c.printDigit + "\nHolographic: " + c.isHolographic + "\nBent Corners: " + c.bentCorners;
	}
	void UpdateCardVisuals()
	{
		deckTM.text = CardToText(deck[currentDeck]);
		offerTM.text = CardToText(offer);
		deckSR.sprite = CD.sprites[deck[currentDeck].monsplode];
		offerSR.sprite = CD.sprites[offer.monsplode];
	}
	void ResetOffer()
	{
		PrintDebug("Generating the offered card #" + (currentOffer + 1));
		offer = MakeCard();
		PrintDebug("Value of offered card #" + (currentOffer + 1) + ": "+offer.value);
	}
	void Init()
	{
		// Calculate serial state for monplode initial variation
		char first= Info.GetSerialNumber()[0], second= Info.GetSerialNumber()[1];
		if ('A' <= first && first <= 'Z')
		{
			if ('A' <= second && second <= 'Z')
				state = SerialStates.letters;
			else
				state = SerialStates.both;
		}
		else
		{
			if ('A' <= second && second <= 'Z')
				state = SerialStates.both;
			else
				state = SerialStates.numbers;
		}
		// Create both deck and offers
		deck = new Card[deckSize];
		for (int i = 0; i < deckSize; i++)
		{
			PrintDebug("Generating the deck card #" + (i +1));
			deck[i] = MakeCard();
			PrintDebug("Value of deck card #" + (i + 1) + ": " + deck[i].value);
		}

		ResetOffer();

		//PrintOutDeck();
		
		CalculateLowestCardInDeck();
	}
}
