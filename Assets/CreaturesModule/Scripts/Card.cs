using UnityEngine;
using System.Collections;

public class Card {
	public Card(int mons,int rare,int pd,char pc,bool holo,int corner)
	{
		monsplode = mons;
		rarity = rare;
		printChar = pc;
		printDigit = pd;
		bentCorners = corner;
		isHolographic = holo;
	}
	public int monsplode, rarity, printDigit,bentCorners;
	public char printChar;
	public float value;
	public bool isHolographic;
}
