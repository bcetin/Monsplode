using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using KMHelper;

public class MonsplodeFightModule : MonoBehaviour
{

    public KMBombInfo Info;
    public KMBombModule Module;
    public KMAudio Audio;
    public CreatureDataObject CD;
    public MovesDataObject MD;
    public SpriteRenderer screenSR;
    public KMSelectable[] buttons;
    public Vector3[] AttackMul;
    public Transform[] DP, UP;
    float[][] mulLookup;
    bool isActivated = false;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    // Move/Creature Variables
    int crID, correctCount = 0;
    public float moveDelta;
    bool revive = false;
    int[] moveIDs;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        Init();
        Module.OnActivate += ActivateModule;
    }

    void ActivateModule()
    {
        isActivated = true;
        screenSR.enabled = true;
    }
    void PickCreature()
    {
        crID = Random.Range(0, CD.size);

        screenSR.sprite = CD.sprites[crID];
    }

    void PickMoves()
    {
        moveIDs = new int[4];

        List<int> movePool = new List<int>();
        for (int i = 0; i < MD.size; i++)
            movePool.Add(i);
        for (int i = 0; i < 4; i++)
        {
            int tem = Random.Range(0, movePool.Count);

            int pickedMove = movePool[tem];
            buttons[i].GetComponentInChildren<TextMesh>().text = MD.names[pickedMove];
            moveIDs[i] = pickedMove;
            movePool.Remove(pickedMove);
        }


    }

    void PlaceAttackMulMatrix()
    {
        mulLookup = new float[20][];
        for (int i = 0; i < 20; i++)
        {
            mulLookup[i] = new float[20];
            for (int j = 0; j < 20; j++)
                mulLookup[i][j] = 1;
        }
        foreach (Vector3 vec in AttackMul)
            mulLookup[(int)vec.x][(int)vec.y] = vec.z;
    }

    void Init() // IT`S CHANGING WITH TIME!
    {
        //Pick DATA
        PlaceAttackMulMatrix();
        PickCreature();
        PickMoves();
        for (int i = 0; i < 4; i++)
        {
            int j = i;
            buttons[i].OnInteract += delegate ()
            {
                OnPress(j);
                return false;
            };
        }
    }

    float CalcDmg(int move, int crea, int buttonLocation)
    {
        //DAMAGE CHANGING MOVES
        float DAMAGE = MD.damage[move];
        int TYPE = CD.type[crea];

        //TYPE OVERRIDES
        if (CD.specials[crea] == "STRNORM" && Info.GetStrikes() > 0)
        {
            Debug.Log("[MonsplodeFight] Mountoise is NORMAL type instead.");
            TYPE = 0; //OVERRIDE TYPE TO NORMAL MOUNTO
        }
        if (CD.specials[crea] == "LESS3NORM" && Info.GetBatteryCount() < 3)
        {
            Debug.Log("[MonsplodeFight] Zapra is NORMAL type instead.");
            TYPE = 0; //OVERRIDE TYPE TO NORMAL ZAPRA
        }
        if (CD.specials[crea] == "LESS3ROCK" && Info.GetBatteryCount() < 3)
        {
            Debug.Log("[MonsplodeFight] Magmy is ROCK type instead.");
            TYPE = 2; //OVERRIDE TYPE TO ROCK MAGMY
        }
        if (CD.specials[crea] == "CARWATER" && Info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.CAR))
        {
            Debug.Log("[MonsplodeFight] Asteran is WATER type instead.");
            TYPE = 5;
        }
        if (CD.specials[crea] == "CLRWATER" && Info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.CLR))
        {
            Debug.Log("[MonsplodeFight] Violan is WATER type instead.");
            TYPE = 5;
        }
        if (CD.specials[crea] == "NOLITDARK" && Info.GetOnIndicators().Count() == 0)
        {
            Debug.Log("[MonsplodeFight] Myrchat is DARK type instead.");
            TYPE = 8; //OVERRIDE TYPE TO DARK CAT
        }

        // MOVE SPECIALS
        /*if (MD.specials [move] == "LOC") DISLOCATE IS CANCELLED
		{
			int[] DMG = {5,3,2,8};

			DAMAGE = DMG[buttonLocation];
			Debug.Log ("[MonsplodeFight] Dislocate base damage is " + DAMAGE + ", according to button location.");
		}*/
        if (MD.specials[move] == "FINALE")
        {
            if (Info.GetSolvedModuleNames().Count == Info.GetSolvableModuleNames().Count - 1)
            {
                DAMAGE = 10;
                Debug.Log("[MonsplodeFight] Finale has 10 damage bonus!");
            }
        }
        if (MD.specials[move] == "SIDE")
        {
            int[] lookUp = { 1, 0, 3, 2 };

            DAMAGE = MD.names[moveIDs[lookUp[buttonLocation]]].Count(char.IsLetter);
            Debug.Log("[MonsplodeFight] Sidestep base damage is " + DAMAGE + ", according to neighbor moves' letter count. (" + MD.names[moveIDs[lookUp[buttonLocation]]].Replace('\n', ' ') + ")");
        }
        if (MD.specials[move] == "BOO")
        {
            foreach (char c in Info.GetSerialNumber())
                if (c == 'O' || c == '0')
                    DAMAGE += 3;
            Debug.Log("[MonsplodeFight] Boo O/0 count: " + DAMAGE / 3);
        }
        if (MD.specials[move] == "BPOWER")
        {
            DAMAGE += Info.GetBatteryCount() * 2;
            Debug.Log("[MonsplodeFight] BPower Battery Count: " + DAMAGE / 2);
        }
        if (MD.specials[move] == "SHOCKPORT")
        {
            if (Info.IsPortPresent(KMBombInfoExtensions.KnownPortType.RJ45))
            {
                Debug.Log("[MonsplodeFight] Shock Has RJ45 Bonus!");
                DAMAGE = 8;
            }
        }
        if (MD.specials[move] == "DARKPORT")
        {
            DAMAGE = Info.GetPortCount();
            Debug.Log("[MonsplodeFight] DPortal Port Count: " + Info.GetPortCount());
        }
        if (MD.specials[move] == "LASTDIGIT")
        {
            DAMAGE = Info.GetSerialNumber().Last() - '0';
            Debug.Log("[MonsplodeFight] LWord Last Digit: " + DAMAGE);
        }
        if (MD.specials[move] == "NOSOLVED")
        {
            if (Info.GetSolvedModuleNames().Count() == 0)
            {
                DAMAGE = 10;
                Debug.Log("[MonsplodeFight] Void has 10 damage bonus!");
            }
        }

        if (MD.specials[move] == "FIERYMUL")
        {
            DAMAGE = Info.GetBatteryCount() * Info.GetBatteryHolderCount(); // IS IT CORRECT?
            Debug.Log("[MonsplodeFight] FSoul Batteries: " + Info.GetBatteryCount() + " Holders:" + Info.GetBatteryHolderCount());
        }
        if (MD.specials[move] == "BIGDIG")
        {
            DAMAGE = Info.GetSerialNumberNumbers().Max();
            Debug.Log("[MonsplodeFight] Stretch Highest Digit: " + DAMAGE);
        }
        if (MD.specials[move] == "SMOLDIG")
        {
            DAMAGE = Info.GetSerialNumberNumbers().Min();
            Debug.Log("[MonsplodeFight] Shrink Smallest Digit: " + DAMAGE);
        }
        if (MD.specials[move] == "GORD")
        {
            if (TYPE == 8)
            {
                Debug.Log("[MonsplodeFight] Appearify has 10 damage bonus!");
                DAMAGE = 10;
            }
        }
        if (MD.specials[move] == "RORG")
        {
            if (TYPE == 2 || TYPE == 6)
            {
                Debug.Log("[MonsplodeFight] Sendify has 10 damage bonus!");
                DAMAGE = 10;
            }
        }
        if (MD.specials[move] == "FREAK")
        {
            if (Info.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.FRQ) || Info.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.FRK))
            {
                DAMAGE = 10;
                Debug.Log("[MonsplodeFight] Freak Out has 10 damage bonus!");
            }
            else if (Info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.FRQ) || Info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.FRK))
            {
                DAMAGE = 5;
                Debug.Log("[MonsplodeFight] Freak Out has 5 damage bonus!");
            }
        }
        if (MD.specials[move] == "LENGTH")
        {
            DAMAGE = CD.names[crea].Count(char.IsLetter);
            Debug.Log("[MonsplodeFight] Opponents Name Length: " + DAMAGE);
        }
        if (MD.specials[move] == "BUGSPRAY")
        {
            if (CD.names[crea] == "Melbor" || CD.names[crea] == "Zenlad")
            {
                Debug.Log("[MonsplodeFight] Bug Spray has 10 damage bonus!");
                DAMAGE = 10;
            }
            // ADD BUGS TO HERE
        }
        if (MD.specials[move] == "MODCNT")
        {
            DAMAGE = Info.GetModuleNames().Count();
            Debug.Log("[MonsplodeFight] Bedrock Module Count: " + Info.GetModuleNames().Count());
            // ADD BUGS TO HERE
        }
        if (MD.specials[move] == "TIMELEFT")
        {
            DAMAGE = Mathf.FloorToInt(Info.GetTime() / 60f);
            Debug.Log("[MonsplodeFight] Countdown Remaining Minutes: " + DAMAGE);
        }

        // CREATURE SPECIALS
        // RIM TEAM
        if (CD.specials[crea] == "PORTRIM")
        {
            if (Info.GetPortCount() > 0 && MD.type[move] == 0)
            {
                Debug.Log("[MonsplodeFight] Caadarim takes no damage from NORMAL type.");
                return 0;
            }
        }

        if (CD.specials[crea] == "PARARIM")
        {
            if (Info.IsPortPresent(KMBombInfoExtensions.KnownPortType.Parallel) && MD.type[move] == 0)
            {
                Debug.Log("[MonsplodeFight] Vellarim takes no damage from NORMAL type.");
                return 0;
            }
        }

        if (CD.specials[crea] == "SERIRIM")
        {
            if (Info.IsPortPresent(KMBombInfoExtensions.KnownPortType.Serial) && MD.type[move] == 0)
            {
                Debug.Log("[MonsplodeFight] Flaurim takes no damage from NORMAL type.");
                return 0;
            }
        }

        if (CD.specials[crea] == "DVIRIM")
        {
            if (Info.IsPortPresent(KMBombInfoExtensions.KnownPortType.DVI) && MD.type[move] == 0)
            {
                Debug.Log("[MonsplodeFight] Gloorim takes no damage from NORMAL type.");
                return 0;
            }
        }
        // NO TEAM
        if (CD.specials[crea] == "NOROCK")
        {
            if (MD.type[move] == 2)
            {
                Debug.Log("[MonsplodeFight] Buhar takes no damage from ROCK type.");
                return 0;
            }
        }
        if (CD.specials[crea] == "NOGRASS")
        {
            if (MD.type[move] == 6)
            {
                Debug.Log("[MonsplodeFight] Nibs takes no damage from GRASS type.");
                return 0;
            }
        }
        if (CD.specials[crea] == "NOWATER")
        {
            if (MD.type[move] == 5)
            {
                Debug.Log("[MonsplodeFight] Ukkens takes no damage from WATER type.");
                return 0;
            }
        }
        //PERCY
        if (CD.specials[crea] == "SPLASH" && MD.specials[move] == "SPLASH")
        {
            Debug.Log("[MonsplodeFight] SPLASH must be used against Percy.");
            return 1000;
        }
        // DOC
        if (CD.specials[crea] == "DOC" && MD.specials[move] == "BOOM")
        {
            Debug.Log("[MonsplodeFight] BOOM must be used against Docsplode.");
            return 1000; // INFINITY ENOUGH?
        }
        // BOB
        if (CD.specials[crea] == "BOB" && Info.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.BOB) && MD.type[move] != 0)
        {
            Debug.Log("[MonsplodeFight] Bob takes no damage from non-NORMAL type.");
            return 0; // NON NORMALS CANT HIT
        }

        //CALCULATION LINE (FINALLY!)

        float NETDAMAGE = DAMAGE * mulLookup[MD.type[move]][TYPE];
        Debug.Log("[MonsplodeFight] Base Damage: " + DAMAGE + " | Type Multipler: " + mulLookup[MD.type[move]][TYPE] + " | Net Damage: " + NETDAMAGE);
        //MELBOR
        if (CD.specials[crea] == "ZERO68")
        {
            if ((int)NETDAMAGE == 6 || (int)NETDAMAGE == 8)
            {
                Debug.Log("[MonsplodeFight] Melbor takes 0 damage instead.");
                return 0;
            }
        }
        //POUSE
        if (CD.specials[crea] == "ZERO6MORE")
        {
            if ((int)NETDAMAGE >= 6)
            {
                Debug.Log("[MonsplodeFight] Pouse takes 0 damage instead.");
                return 0;
            }
        }
        //ZENLAD
        if (CD.specials[crea] == "ELECPLUS3" && MD.type[move] == 7)
        {
            Debug.Log("[MonsplodeFight] Zenlad takes extra 3 net damage from ELECTR.");
            return NETDAMAGE + 3;
        }
        //CLONDAR
        if (CD.specials[crea] == "WATERPLUS3" && MD.type[move] == 5)
        {
            Debug.Log("[MonsplodeFight] Clondar takes extra 3 net damage from WATER.");
            return NETDAMAGE + 3;
        }

        //LANALUFF
        if (CD.specials[crea] == "LUFF" && MD.type[move] == 1 && HasSameChar(CD.names[crea].ToUpper(), Info.GetSerialNumber()))
        {
            Debug.Log("[MonsplodeFight] Lanaluff has a common letter with serial. Extra 3 net damage.");
            return NETDAMAGE + 3;
        }
        //PILLOWS
        if (CD.specials[crea] == "2F1W")
        {
            if (MD.type[move] == 4)
            {
                Debug.Log("[MonsplodeFight] Aluga takes extra 2 net damage from FIRE.");
                return NETDAMAGE + 2;
            }
            if (MD.type[move] == 5)
            {
                Debug.Log("[MonsplodeFight] Aluga takes 1 less net damage from WATER.");
                return NETDAMAGE - 1;
            }
        }
        if (CD.specials[crea] == "2W1F")
        {
            if (MD.type[move] == 5)
            {
                Debug.Log("[MonsplodeFight] Lugirit takes extra 2 net damage from WATER.");
                return NETDAMAGE + 2;
            }
            if (MD.type[move] == 4)
            {
                Debug.Log("[MonsplodeFight] Lugirit takes 1 less net damage from FIRE.");
                return NETDAMAGE - 1;
            }
        }
        return NETDAMAGE;
    }

    bool HasSameChar(string A, string B)
    {
        foreach (char a in A)
            foreach (char b in B)
                if (a == b)
                    return true;
        return false;
    }

    void OnPress(int buttonID)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[buttonID].transform);
        buttons[buttonID].AddInteractionPunch();
        if (!isActivated)
        {
            Debug.Log("[MonsplodeFight] Pressed button before module has been activated!");
            return;
        }
        Debug.Log("[MonsplodeFight] Opponent: " + CD.names[crID] + "\nUsing Move(" + buttonID + "): " + MD.names[moveIDs[buttonID]].Replace('\n', ' '));

        if (MD.specials[moveIDs[buttonID]] == "BOOM" && CD.specials[crID] != "DOC")
        {
            Module.HandleStrike();
            Module.HandleStrike();
            Module.HandleStrike();
            Module.HandleStrike();
            Module.HandleStrike();
            //BOOM!
            Debug.Log("[MonsplodeFight] Pressed BOOM!");
        }

        float mxdmg = -100;
        List<int> winners = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            //buttons [i].GetComponentInChildren<TextMesh>().text=MD.names[moveIDs[i]];
            float dmg = CalcDmg(moveIDs[i], crID, i);
            if (CD.specials[crID] == "LOWEST")
            {
                Debug.Log("[MonsplodeFight] Negate the calculated number for Cutie Pie calculation.");
                dmg = -dmg;
            }
            if (dmg > mxdmg)
            {
                mxdmg = dmg;
                winners.Clear();
                winners.Add(i);
            }
            else if (dmg == mxdmg)
                winners.Add(i);
            Debug.Log("[MonsplodeFight] Move Name(" + i + "): " + MD.names[moveIDs[i]].Replace('\n', ' ') + "\nCalculated Damage: " + dmg);
        }

        if (winners.Contains(buttonID))
        {
            Correct();
        }
        else
        {
            Wrong();
        }
    }

    void Correct()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        correctCount++;
        if (correctCount == 1)
            Module.HandlePass();
        else
            revive = true;
        StartCoroutine(GoDown());
    }

    void Wrong()
    {
        Module.HandleStrike();
        revive = true;
        StartCoroutine(GoDown());
    }

    void TurnStuffOff()
    {
        screenSR.enabled = false;
        foreach (KMSelectable but in buttons)
            but.GetComponentInChildren<TextMesh>().text = "";
    }

    void ResetModule()
    {
        PickCreature();
        PickMoves();
    }

    void TurnStuffOn()
    {
        screenSR.enabled = true;
    }

    IEnumerator GoDown()
    {
        isActivated = false;
        TurnStuffOff();
        //Debug.Log ("GOING DOWN");
        while (buttons[0].transform.position != DP[0].transform.position)
        {
            for (int i = 0; i < 4; i++)
                buttons[i].transform.position = Vector3.MoveTowards(buttons[i].transform.position, DP[i].transform.position, moveDelta);
            yield return new WaitForSeconds(0.02f);
        }
        if (revive)
        {
            revive = false;
            StartCoroutine(GoUp());
        }
    }

    IEnumerator GoUp()
    {
        //Debug.Log ("COMING UP");
        while (buttons[0].transform.position != UP[0].transform.position)
        {
            for (int i = 0; i < 4; i++)
                buttons[i].transform.position = Vector3.MoveTowards(buttons[i].transform.position, UP[i].transform.position, moveDelta);
            yield return new WaitForSeconds(0.02f);
        }
        ResetModule();
        TurnStuffOn();
        isActivated = true;
    }

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        var btn = new List<KMSelectable>();
        command = command.ToLowerInvariant().Trim();

        //position based
        if (Regex.IsMatch(command, @"^press [a-zA-Z]+$"))
        {
            command = command.Substring(6).Trim();
            switch (command)
            {
                case "tl": case "lt": case "topleft": case "lefttop": btn.Add(buttons[0]); break;
                case "tr": case "rt": case "topright": case "righttop": btn.Add(buttons[1]); break;
                case "bl": case "lb": case "buttomleft": case "leftbuttom": btn.Add(buttons[2]); break;
                case "br": case "rb": case "bottomright": case "rightbottom": btn.Add(buttons[3]); break;
            }
        }
        //direct name
        else if (Regex.IsMatch(command, @"^use [a-zA-Z]+$"))
        {
            command = command.Substring(4).Trim();

            if (command == MD.names[moveIDs[0]].Replace('\n', ' ').Replace(" ", string.Empty).ToLowerInvariant()) btn.Add(buttons[0]);
            else if (command == MD.names[moveIDs[1]].Replace('\n', ' ').Replace(" ", string.Empty).ToLowerInvariant()) btn.Add(buttons[1]);
            else if (command == MD.names[moveIDs[2]].Replace('\n', ' ').Replace(" ", string.Empty).ToLowerInvariant()) btn.Add(buttons[2]);
            else if (command == MD.names[moveIDs[3]].Replace('\n', ' ').Replace(" ", string.Empty).ToLowerInvariant()) btn.Add(buttons[3]);
            else return null;
        }
        else return null;

        return btn.ToArray();
    }
}
