using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using System.Linq;

public class MonsplodeFightModule : MonoBehaviour {

	public CreatureDataObject CD;
	public MovesDataObject MD;
	public SpriteRenderer screenSR;
	public KMSelectable[] buttons;
	public Vector3[] AttackMul;
	public GameObject[] GrayLEDS;
	public Transform[] DP,UP;
	float[][] mulLookup;
	bool isActivated=false;
	// Bomb Data Variables
	int batteryCount=0,batteryHolderCount=0,portcount=0,moduleCount=0;
	string serialNumber;
	bool hasRJ45=false,hasSerial=false,hasParallel=false,hasDVI=false; //PORTS
	bool freak=false,freakON=false,hasLitBOB=false,hasCLR=false,hasCAR=false,hasAnyLit=false;

	// Move/Creature Variables
	int crID,correctCount=0;
	public float moveDelta;
	bool revive=false;
	int[] moveIDs;


	void Start()
	{
		Init();
		GetComponent<KMBombModule>().OnActivate += ActivateModule;
	}

	void ActivateModule()
	{
		InitBombData ();
		isActivated = true;
		screenSR.enabled = true;
	}
	void PickCreature()
	{
		crID=Random.Range(0,CD.size);
		/*#if UNITY_EDITOR
		crID = 22;
		#endif*/

		screenSR.sprite = CD.sprites [crID];
	}

	void PickMoves()
	{
		moveIDs = new int[4];
	
		 List<int> movePool= new List<int>();
		for (int i = 0; i < MD.size; i++)
			movePool.Add (i);
		for (int i = 0; i < 4; i++)
		{
			int tem = Random.Range (0, movePool.Count);

			int pickedMove=movePool[tem];
			/*#if UNITY_EDITOR
			pickedMove=23;
			#endif*/
			buttons [i].GetComponentInChildren<TextMesh> ().text = MD.names [pickedMove];
			moveIDs [i] = pickedMove;
			movePool.Remove(pickedMove);
		}


	}
	void PlaceAttackMulMatrix()
	{
		mulLookup=new float[20][];
		for (int i = 0; i < 20; i++)
		{
			mulLookup [i] = new float[20];
			for (int j = 0; j < 20; j++)
				mulLookup[i][j]=1;
		}
		foreach (Vector3 vec in AttackMul)
			mulLookup [(int)vec.x] [(int)vec.y] = vec.z;
	}
	void Init() // IT`S  CHANGING WITH TIME!
	{
		//Pick DATA
		PlaceAttackMulMatrix();
		PickCreature ();
		PickMoves ();
		for (int i = 0; i < 4; i++) {
			int j=i;
			buttons [i].OnInteract += delegate () {
				OnPress (j);
				return false;
			};
		}
	}
	// Update is called once per frame
	float CalcDmg(int move,int crea)
	{
		//DAMAGE CHANGING MOVES
		float DAMAGE=MD.damage[move];
		int TYPE = CD.type [crea];

        //TYPE OVERRIDES
        if (CD.specials[crea] == "STRNORM" && GetComponent<KMBombInfo>().GetStrikes() > 0)
		{
			Debug.Log ("[MonsplodeFight] Mountoise is NORMAL type instead.");
            TYPE = 0; //OVERRIDE TYPE TO NORMAL MOUNTO
		}
        if (CD.specials[crea] == "LESS3NORM" && batteryCount < 3)
		{
			Debug.Log ("[MonsplodeFight] Zapra is NORMAL type instead.");
            TYPE = 0; //OVERRIDE TYPE TO NORMAL ZAPRA
		}
        if (CD.specials[crea] == "LESS3ROCK" && batteryCount < 3)
		{
			Debug.Log ("[MonsplodeFight] Magmy is ROCK type instead.");
            TYPE = 2; //OVERRIDE TYPE TO ROCK MAGMY
		}
        if (CD.specials[crea] == "CARWATER" && hasCAR)
		{
			Debug.Log ("[MonsplodeFight] Asteran is WATER type instead.");
            TYPE = 5; 
		}
        if (CD.specials[crea] == "CLRWATER" && hasCLR)
		{
			Debug.Log ("[MonsplodeFight] Violan is WATER type instead.");
            TYPE = 5;
		}
        if (CD.specials[crea] == "NOLITDARK" && !hasAnyLit)
		{
			Debug.Log ("[MonsplodeFight] Myrchat is DARK type instead.");
            TYPE = 8; //OVERRIDE TYPE TO DARK CAT
		}

        // MOVE SPECIALS
        if (MD.specials [move] == "BOO")
		{
			foreach (char c in serialNumber)
				if (c == 'O' || c == '0')
					DAMAGE += 3;
			Debug.Log ("[MonsplodeFight] Boo O/0 count: " + DAMAGE/3);
		}
		if (MD.specials [move] == "BPOWER")
		{
			DAMAGE += batteryCount*2;
			Debug.Log ("[MonsplodeFight] BPower Battery Count: " + DAMAGE/2);
		}
		if (MD.specials [move] == "SHOCKPORT")
		{
			if (hasRJ45)
			{
				Debug.Log ("[MonsplodeFight] Shock Has RJ45 Bonus!");
				DAMAGE = 8;
			}
		}
		if (MD.specials [move] == "DARKPORT")
		{
			DAMAGE = portcount;
			Debug.Log ("[MonsplodeFight] DPortal Port Count: " + portcount);
		}
		if (MD.specials [move] == "LASTDIGIT")
		{
			if ('0' <=  serialNumber[5] &&  serialNumber[5] <= '9')
				DAMAGE = serialNumber[5]-'0'; // Probably not needed but I don't want to risk deleting this.
			else DAMAGE=0;
			Debug.Log ("[MonsplodeFight] LWord Last Digit: " + DAMAGE);
		}
		if (MD.specials [move] == "NOSOLVED")
		{
			if (GetComponent<KMBombInfo> ().GetSolvedModuleNames ().Count == 0)
			{
				DAMAGE = 10;
				Debug.Log ("[MonsplodeFight] Void has 10 damage bonus!");
			}
		}

		if (MD.specials [move] == "FIERYMUL")
		{
			DAMAGE = batteryCount*batteryHolderCount; // IS IT CORRECT?
			Debug.Log ("[MonsplodeFight] FSoul Batteries: " + batteryCount + " Holders:" + batteryHolderCount);
		}
		if (MD.specials [move] == "BIGDIG")
		{
			int mx=0;
			foreach (char c in serialNumber)
				if ('0' <= c && c <= '9')
					if (mx < c - '0')
						mx = c - '0';
			DAMAGE = mx;
			Debug.Log ("[MonsplodeFight] Stretch Highest Digit: " + DAMAGE);
		}
		if (MD.specials [move] == "SMOLDIG")
		{
			int mn=10;
			foreach (char c in serialNumber)
				if ('0' <= c && c <= '9')
					if (mn > c - '0')
						mn = c - '0';
			DAMAGE = mn;
			Debug.Log ("[MonsplodeFight] Shrink Smallest Digit: " + DAMAGE);
		}
		if (MD.specials [move] == "GORD")
		{
            if(TYPE==8)
            {
				Debug.Log("[MonsplodeFight] Appearify has 10 damage bonus!");
                DAMAGE=10;
            }
		}
		if (MD.specials [move] == "RORG")
		{
            if(TYPE==2 || TYPE==6)
            {
				Debug.Log ("[MonsplodeFight] Sendify has 10 damage bonus!");
				DAMAGE=10;
			}
		}
		if (MD.specials [move] == "FREAK")
		{
			if (freakON)
			{
				DAMAGE = 10;
				Debug.Log ("[MonsplodeFight] Freak Out has 10 damage bonus!");
			}
			else if (freak)
			{
				Debug.Log ("[MonsplodeFight] Freak Out has 5 damage bonus!");
				DAMAGE = 5;
			}
		}
		if (MD.specials [move] == "LENGTH")
		{
			DAMAGE = CD.names[crea].Count(char.IsLetter);
			Debug.Log ("[MonsplodeFight] Opponents Name Length: " + DAMAGE);
		}
		if (MD.specials [move] == "BUGSPRAY")
		{
			if (CD.names [crea] == "Melbor" || CD.names [crea] == "Zenlad")
			{
				Debug.Log ("[MonsplodeFight] Bug Spray has 10 damage bonus!");
				DAMAGE = 10;
			}
			// ADD BUGS TO HERE
		}
		if (MD.specials [move] == "MODCNT")
		{
			DAMAGE = moduleCount;
			Debug.Log ("[MonsplodeFight] Bedrock Module Count: " + moduleCount);
			// ADD BUGS TO HERE
		}
		if (MD.specials [move] == "TIMELEFT")
		{
			DAMAGE = Mathf.FloorToInt(GetComponent<KMBombInfo> ().GetTime()/60f);
			Debug.Log ("[MonsplodeFight] Countdown Remaining Minutes: " + DAMAGE);
		}

		// CREATURE SPECIALS
		// RIM TEAM
		if (CD.specials [crea] == "PORTRIM")
		{
			if (portcount > 0 && MD.type [move] == 0)
			{
				Debug.Log ("[MonsplodeFight] Caadarim takes no damage from NORMAL type.");
				return 0;
			}
		}

		if (CD.specials [crea] == "PARARIM")
		{
			if (hasParallel && MD.type [move] == 0)
			{
				Debug.Log ("[MonsplodeFight] Vellarim takes no damage from NORMAL type.");
				return 0;
			}
		}

		if (CD.specials [crea] == "SERIRIM")
		{
			if (hasSerial && MD.type [move] == 0)
			{
				Debug.Log ("[MonsplodeFight] Flaurim takes no damage from NORMAL type.");
				return 0;
			}
		}

		if (CD.specials [crea] == "DVIRIM")
		{
			if (hasDVI && MD.type [move] == 0)
			{
				Debug.Log ("[MonsplodeFight] Gloorim takes no damage from NORMAL type.");
				return 0;
			}
		}
		// NO TEAM
		if (CD.specials [crea] == "NOROCK")
		{
			if (MD.type [move] == 2)
			{
				Debug.Log ("[MonsplodeFight] Buhar takes no damage from ROCK type.");
				return 0;
			}
		}
		if (CD.specials [crea] == "NOGRASS")
		{
			if (MD.type [move] == 6)
			{
				Debug.Log ("[MonsplodeFight] Nibs takes no damage from GRASS type.");
				return 0;
			}
		}
		if (CD.specials [crea] == "NOWATER")
		{
			if (MD.type [move] == 5)
			{
				Debug.Log ("[MonsplodeFight] Ukkens takes no damage from WATER type.");
				return 0;
			}
		}
		//PERCY
		if (CD.specials [crea] == "SPLASH" && MD.specials [move] == "SPLASH")
		{
			Debug.Log ("[MonsplodeFight] SPLASH must be used against Percy.");
			return 1000;
		}
		// DOC
		if (CD.specials [crea] == "DOC" && MD.specials [move] == "BOOM")
		{
			Debug.Log ("[MonsplodeFight] BOOM must be used against Docsplode.");
			return 1000; // INFINITY ENOUGH?
		}
		// BOB
		if (CD.specials [crea] == "BOB" && hasLitBOB && MD.type[move]!=0)
		{
			Debug.Log ("[MonsplodeFight] Bob takes no damage from non-NORMAL type.");
			return 0; // NON NORMALS CANT HIT
		}

		//CALCULATION LINE (FINALLY!)

		float NETDAMAGE=DAMAGE*mulLookup[MD.type[move]][TYPE];
		Debug.Log ("[MonsplodeFight] Base Damage: " + DAMAGE + " | Type Multipler: " + mulLookup[MD.type[move]][TYPE] + " | Net Damage: " + NETDAMAGE);
		//MELBOR
		if (CD.specials [crea] == "ZERO68")
		{
			if ((int)NETDAMAGE == 6 || (int)NETDAMAGE == 8)
			{
				Debug.Log ("[MonsplodeFight] Melbor takes 0 damage instead.");
				return 0;
			}
		}
		//POUSE
		if (CD.specials [crea] == "ZERO6MORE")
		{
			if ((int)NETDAMAGE >= 6)
			{
				Debug.Log ("[MonsplodeFight] Pouse takes 0 damage instead.");
				return 0;
			}
		}
		//ZENLAD
		if (CD.specials [crea] == "ELECPLUS3" && MD.type [move] == 7)
		{
			Debug.Log ("[MonsplodeFight] Zenlad takes extra 3 net damage from ELECTR.");
			return NETDAMAGE + 3;
		}
		//CLONDAR
		if (CD.specials [crea] == "WATERPLUS3" && MD.type [move] == 5)
		{
			Debug.Log ("[MonsplodeFight] Clondar takes extra 3 net damage from WATER.");
			return NETDAMAGE + 3;
		}
		
		//LANALUFF
		if (CD.specials [crea] == "LUFF" && MD.type [move] == 1 && HasSameChar (CD.names [crea].ToUpper (), serialNumber))
		{
			Debug.Log ("[MonsplodeFight] Lanaluff has a common letter with serial. Extra 3 net damage.");
			return NETDAMAGE + 3;
		}
		//PILLOWS
		if (CD.specials [crea] == "2F1W")
		{
			if (MD.type [move] == 4)
			{
				Debug.Log ("[MonsplodeFight] Aluga takes extra 2 net damage from FIRE.");
				return NETDAMAGE + 2;
			}
			if (MD.type [move] == 5)
			{
				Debug.Log ("[MonsplodeFight] Aluga takes 1 less net damage from WATER.");
				return NETDAMAGE - 1;
			}
		}
		if (CD.specials [crea] == "2W1F")
		{
			if (MD.type [move] == 5)
			{
				Debug.Log ("[MonsplodeFight] Lugirit takes extra 2 net damage from WATER.");
				return NETDAMAGE + 2;
			}
			if (MD.type [move] == 4)
			{
				Debug.Log ("[MonsplodeFight] Lugirit takes 1 less net damage from FIRE.");
				return NETDAMAGE - 1;
			}
		}
		return NETDAMAGE;
	}

	bool HasSameChar(string A,string B)
	{
		foreach (char a in A)
			foreach (char b in B)
				if (a == b)
					return true;
		return false;
	}
	void OnPress(int buttonID)
	{
		if (!isActivated)
		{
			Debug.Log("[MonsplodeFight] Pressed button before module has been activated!");
			return;
		}
		Debug.Log("[MonsplodeFight] Opponent: " + CD.names[crID] + "\nUsing Move("+buttonID+"): "+ MD.names [moveIDs [buttonID]].Replace('\n',' '));
//		
		if (MD.specials [moveIDs [buttonID]] == "BOOM" && CD.specials[crID]!="DOC")
		{
			GetComponent<KMBombModule>().HandleStrike();
			GetComponent<KMBombModule>().HandleStrike();
			GetComponent<KMBombModule>().HandleStrike();
			GetComponent<KMBombModule>().HandleStrike();
			GetComponent<KMBombModule>().HandleStrike();
			//BOOM!
			Debug.Log("[MonsplodeFight] Pressed BOOM!");
		}

		float mxdmg=0;
		List<int> winners= new List<int>();
		for(int i = 0; i < 4; i++)
		{
			buttons [i].GetComponentInChildren<TextMesh>().text=MD.names[moveIDs[i]];
			float dmg = CalcDmg (moveIDs[i],crID);
			if (CD.specials [crID] == "LOWEST")
			{
				Debug.Log("[MonsplodeFight] Negate the calculated number for Cutie Pie calculation.");
				dmg = -dmg;
			}
			if (dmg > mxdmg)
			{
				mxdmg=dmg;
				winners.Clear ();
				winners.Add (i);
			}
			else if (dmg == mxdmg)
				winners.Add (i);
			Debug.Log ("[MonsplodeFight] Move Name("+i+"): " + MD.names[moveIDs[i]].Replace('\n',' ') + "\nCalculated Damage: "+dmg);
		}

		if ( winners.Contains (buttonID))
		{
			Correct ();
		}
		else
		{
			Wrong ();
		}
	}

	void Correct()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime,transform);
		correctCount++;
		if (correctCount == 1)
			GetComponent<KMBombModule> ().HandlePass ();
		else
			revive = true;
		StartCoroutine (GoDown());

		
	}
	void TurnStuffOff()
	{
		screenSR.enabled = false;
		foreach (KMSelectable but in buttons)
			but.GetComponentInChildren<TextMesh> ().text="";
	}
	void ResetModule()
	{
		PickCreature ();
		PickMoves ();
	}
	void TurnStuffOn()
	{
		screenSR.enabled = true;
	}
	IEnumerator GoDown()
	{
		isActivated = false;
		TurnStuffOff ();
		//Debug.Log ("GOING DOWN");
		while(buttons[0].transform.position!=DP[0].transform.position)
		{
			for (int i=0;i<4;i++)
				buttons[i].transform.position=Vector3.MoveTowards (buttons[i].transform.position,DP[i].transform.position , moveDelta);
				yield return new WaitForSeconds(0.02f);
		}
		if(revive)
		{
			revive=false;
			StartCoroutine(GoUp());
		}
	}
	IEnumerator GoUp()
	{
		//Debug.Log ("COMING UP");
		while(buttons[0].transform.position!=UP[0].transform.position)
		{
			for (int i=0;i<4;i++)
				buttons[i].transform.position=Vector3.MoveTowards (buttons[i].transform.position,UP[i].transform.position , moveDelta);
			yield return new WaitForSeconds(0.02f);
		}
		ResetModule ();
		TurnStuffOn ();
		isActivated = true;
	}
	void Wrong()
	{
		GetComponent<KMBombModule>().HandleStrike();
		revive = true;
		StartCoroutine(GoDown());
	}
	void InitBombData()
	{
		// STUFFF
		/*foreach (string query in new List<string> { KMBombInfo.QUERYKEY_GET_BATTERIES, KMBombInfo.QUERYKEY_GET_INDICATOR, KMBombInfo.QUERYKEY_GET_PORTS, KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, "example"})
		{
			List<string> queryResponse = GetComponent<KMBombInfo>().QueryWidgets(query, null);

			if (queryResponse.Count > 0)
			{
				Debug.Log(queryResponse[0]);
			}
		}*/
		// STUFFF
		//BATTERIES


		List<string> responses = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
		foreach (string response in responses)
		{
			Dictionary<string, int> responseDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(response);
			batteryCount += responseDict["numbatteries"];
			batteryHolderCount++;
		}
		//SERIAL

		responses = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
		foreach (string response in responses)
		{
			Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
			serialNumber= responseDict["serial"];
		}
		//PORTS
		responses = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_PORTS, null);
		foreach (string response in responses)
		{
			Dictionary<string,List<string>> responseDict = JsonConvert.DeserializeObject<Dictionary<string,List<string>>>(response);
			List<string> ports=responseDict["presentPorts"];
			if (ports.Contains ("RJ45"))
				hasRJ45 = true;
			if (ports.Contains ("DVI"))
				hasDVI = true;
			if (ports.Contains ("Parallel"))
				hasParallel = true;
			if (ports.Contains ("Serial"))
				hasSerial = true;
			portcount += ports.Count;
		}
		//INDICATORS
		responses = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, null);
		foreach (string response in responses)
		{
			Dictionary<string,string> responseDict = JsonConvert.DeserializeObject<Dictionary<string,string>>(response);
			if(responseDict["label"]=="FRQ" || responseDict["label"]=="FRK")
			{
				freak = true;
				if(responseDict["on"]=="True")
					freakON=true;
			}
			if (responseDict ["label"] == "BOB" && responseDict ["on"] == "True")
				hasLitBOB = true;
			if (responseDict ["label"] == "CAR")
				hasCAR = true;
			if (responseDict ["label"] == "CLR")
				hasCLR = true;
			if (responseDict ["on"] == "True")
				hasAnyLit = true;
		}
		moduleCount = GetComponent<KMBombInfo> ().GetModuleNames ().Count;
	
			
		//haveRJ45
		#if UNITY_EDITOR
		serialNumber="BOOO0F";
		batteryCount=3;
		#endif
			
	}
}
