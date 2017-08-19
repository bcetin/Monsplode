using UnityEngine;
using System.Collections;

public class Card {
	public Card(int mons,int rare,int pd,char pc)
	{
		monsplode = mons;
		rarity = rare;
		printChar = pc;
		printDigit = pd;
	}
	public int monsplode, rarity, printDigit;
	public char printChar;
	public float value;
}
