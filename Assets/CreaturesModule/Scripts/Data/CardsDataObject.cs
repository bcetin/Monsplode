﻿using UnityEngine;
using System.Collections;

public class CardsDataObject : ScriptableObject
{

	public Sprite[] sprites;
	public string[] names,flavorText;
	public Vector4[] inital;
	public int size;
	// public int[] type; FOR FUTURE USE FOR MOVE SYNERGIES
}
