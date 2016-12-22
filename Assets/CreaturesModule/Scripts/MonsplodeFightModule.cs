using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;

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
	//

	void Start()
	{
		//fukenTypeData = TD.mulLookup; WORKAROUDN!!!!
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
		crID = 17;
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
	void Init() // ITS  CHANGING WITH TIME!
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
            TYPE = 0; //OVERRIDE TYPE TO NORMAL MOUNTO
        if (CD.specials[crea] == "LESS3NORM" && batteryCount < 3)
            TYPE = 0; //OVERRIDE TYPE TO NORMAL ZAPRA
        if (CD.specials[crea] == "LESS3ROCK" && batteryCount < 3)
            TYPE = 2; //OVERRIDE TYPE TO ROCK MAGMY
        if (CD.specials[crea] == "CARWATER" && hasCAR)
            TYPE = 5; //OVERRIDE TYPE TO ROCK MAGMY
        if (CD.specials[crea] == "CLRWATER" && hasCLR)
            TYPE = 5; //OVERRIDE TYPE TO ROCK MAGMY
        if (CD.specials[crea] == "NOLITDARK" && !hasAnyLit)
            TYPE = 8; //OVERRIDE TYPE TO DARK CAT

        // MOVE SPECIALS
        if (MD.specials [move] == "BOO")
		{
			foreach (char c in serialNumber)
				if (c == 'O' || c == '0')
					DAMAGE += 3;
			Debug.Log ("Boo O/0 count: " + DAMAGE/3);
		}
		if (MD.specials [move] == "BPOWER")
		{
			DAMAGE += batteryCount*2;
			Debug.Log ("BPower Battery Count: " + DAMAGE/2);
		}
		if (MD.specials [move] == "SHOCKPORT")
		{
			if (hasRJ45)
			{
				Debug.Log ("Shock Has RJ45 Bonus!");
				DAMAGE = 8;
			}
		}
		if (MD.specials [move] == "DARKPORT")
		{
			DAMAGE = portcount;
			Debug.Log ("DPortal Port Count: " + portcount);
		}
		if (MD.specials [move] == "LASTDIGIT")
		{
			if ('0' <=  serialNumber[5] &&  serialNumber[5] <= '9')
				DAMAGE = serialNumber[5]-'0'; // Probably not needed but I don't want to risk deleting this.
			else DAMAGE=0;
			Debug.Log ("LWord Last Digit: " + DAMAGE);
		}
		if (MD.specials [move] == "NOSOLVED")
		{
			if (GetComponent<KMBombInfo> ().GetSolvedModuleNames ().Count == 0)
			{
				DAMAGE = 10;
				Debug.Log ("Void has 10 damage bonus!");
			}
		}

		if (MD.specials [move] == "FIERYMUL")
		{
			DAMAGE = batteryCount*batteryHolderCount; // IS IT CORRECT?
			Debug.Log ("FSoul Batteries: " + batteryCount + " Holders:" + batteryHolderCount);
		}
		if (MD.specials [move] == "BIGDIG")
		{
			int mx=0;
			foreach (char c in serialNumber)
				if ('0' <= c && c <= '9')
					if (mx < c - '0')
						mx = c - '0';
			DAMAGE = mx;
			Debug.Log ("Stretch Highest Digit: " + DAMAGE);
		}
		if (MD.specials [move] == "SMOLDIG")
		{
			int mn=10;
			foreach (char c in serialNumber)
				if ('0' <= c && c <= '9')
					if (mn > c - '0')
						mn = c - '0';
			DAMAGE = mn;
			Debug.Log ("Shrink Smallest Digit: " + DAMAGE);
		}
		if (MD.specials [move] == "GORD")
		{
            if(TYPE==8)
            {
                Debug.Log("Appearify has 10 damage bonus!");
                DAMAGE=10;
            }
		}
		if (MD.specials [move] == "RORG")
		{
            if(TYPE==2 || TYPE==6)
            {
				Debug.Log ("Sendify has 10 damage bonus!");
				DAMAGE=10;
			}
		}
		if (MD.specials [move] == "FREAK")
		{
			if (freakON)
			{
				DAMAGE = 10;
				Debug.Log ("Freak Out has 10 damage bonus!");
			}
			else if (freak)
			{
				Debug.Log ("Freak Out has 5 damage bonus!");
				DAMAGE = 5;
			}
		}
		if (MD.specials [move] == "LENGTH")
		{
			DAMAGE = CD.names[crea].Length;
			Debug.Log ("Opponents Name Length: " + CD.names[crea].Length);
		}
		if (MD.specials [move] == "BUGSPRAY")
		{
			if (CD.names [crea] == "Melbor" || CD.names [crea] == "Zenlad")
			{
				Debug.Log ("Bug Spray has 10 damage bonus!");
				DAMAGE = 10;
			}
			// ADD BUGS TO HERE
		}
		if (MD.specials [move] == "MODCNT")
		{
			DAMAGE = moduleCount;
			Debug.Log ("Bedrock Module Count: " + moduleCount);
			// ADD BUGS TO HERE
		}
		if (MD.specials [move] == "TIMELEFT")
		{
			DAMAGE = Mathf.FloorToInt(GetComponent<KMBombInfo> ().GetTime()/60f);
			Debug.Log ("Countdown Remaining Minutes: " + DAMAGE);
		}

		// CREATURE SPECIALS
		// RIM TEAM
		if (CD.specials [crea] == "PORTRIM")
		{
			if (portcount > 0 && MD.type [move] == 0)
				return 0;
		}

		if (CD.specials [crea] == "PARARIM")
		{
			if (hasParallel && MD.type [move] == 0)
				return 0;
		}

		if (CD.specials [crea] == "SERIRIM")
		{
			if (hasSerial && MD.type [move] == 0)
				return 0;
		}

		if (CD.specials [crea] == "DVIRIM")
		{
			if (hasDVI && MD.type [move] == 0)
				return 0;
		}
		// NO TEAM
		if (CD.specials [crea] == "NOROCK")
		{
			if (MD.type [move] == 2)
				return 0;
		}
		if (CD.specials [crea] == "NOGRASS")
		{
			if (MD.type [move] == 6)
				return 0;
		}
		if (CD.specials [crea] == "NOWATER")
		{
			if (MD.type [move] == 5)
				return 0;
		}
		// DOC
		if (CD.specials [crea] == "DOC" && MD.specials [move] == "BOOM")
			return 1000; // INFINITY ENOUGH?
		// BOB
		if (CD.specials [crea] == "BOB" && hasLitBOB && MD.type[move]!=0)
			return 0; // NON NORMALS CANT HIT

		//CALCULATION LINE (FINALLY!)

		float NETDAMAGE=DAMAGE*mulLookup[MD.type[move]][TYPE];

		//MELBOR
		if (CD.specials [crea] == "ZERO68")
			if ((int)NETDAMAGE == 6 || (int)NETDAMAGE == 8) return 0;
		//POUSE
		if (CD.specials [crea] == "ZERO6MORE")
			if ((int)NETDAMAGE >= 6) return 0;
		//ZENLAD
		if (CD.specials [crea] == "ELECPLUS3" && MD.type [move] == 7)
			return NETDAMAGE + 3;
		//CLONDAR
		if (CD.specials [crea] == "WATERPLUS3" && MD.type [move] == 5)
			return NETDAMAGE + 3;
		
		//LANALUFF
		if (CD.specials [crea] == "LUFF" && MD.type [move] == 1 && HasSameChar (CD.names [crea].ToUpper(), serialNumber))
			return NETDAMAGE + 3;
		//PILLOWS
		if (CD.specials [crea] == "2F1W")
		{
			if (MD.type [move] == 4)
				return NETDAMAGE + 2;
			if (MD.type [move] == 5)
				return NETDAMAGE - 1;
		}
		if (CD.specials [crea] == "2W1F")
		{
			if (MD.type [move] == 5)
				return NETDAMAGE + 2;
			if (MD.type [move] == 4)
				return NETDAMAGE - 1;
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
			Debug.Log("Pressed button before module has been activated!");
			return;
		}
		Debug.Log("Opponent: " + CD.names[crID] + "\nUsing Move("+buttonID+"): "+ MD.names [moveIDs [buttonID]].Replace('\n',' '));
//		
		if (MD.specials [moveIDs [buttonID]] == "BOOM" && CD.specials[crID]!="DOC")
		{
			GetComponent<KMBombModule>().HandleStrike();
			GetComponent<KMBombModule>().HandleStrike();
			GetComponent<KMBombModule>().HandleStrike();
			GetComponent<KMBombModule>().HandleStrike();
			GetComponent<KMBombModule>().HandleStrike();
			//BOOM!
			Debug.Log("Pressed BOOM!");
		}
//		BOOM?
//

		float mxdmg=0;
		List<int> winners= new List<int>();
		for(int i = 0; i < 4; i++)
		{
			buttons [i].GetComponentInChildren<TextMesh>().text=MD.names[moveIDs[i]];
			float dmg = CalcDmg (moveIDs[i],crID);
			if (dmg > mxdmg)
			{
				mxdmg=dmg;
				winners.Clear ();
				winners.Add (i);
			}
			else if (dmg == mxdmg)
				winners.Add (i);
			Debug.Log ("Move Name("+i+"): " + MD.names[moveIDs[i]].Replace('\n',' ') + "\nCalculated Damage: "+dmg);
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
		foreach (string query in new List<string> { KMBombInfo.QUERYKEY_GET_BATTERIES, KMBombInfo.QUERYKEY_GET_INDICATOR, KMBombInfo.QUERYKEY_GET_PORTS, KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, "example"})
		{
			List<string> queryResponse = GetComponent<KMBombInfo>().QueryWidgets(query, null);

			if (queryResponse.Count > 0)
			{
				Debug.Log(queryResponse[0]);
			}
		}
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
