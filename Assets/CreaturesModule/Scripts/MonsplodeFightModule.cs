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
            Debug.LogFormat("[MonsplodeFight #{0}] Mountoise is NORMAL type instead.", _moduleId);
            TYPE = 0; //OVERRIDE TYPE TO NORMAL MOUNTO
        }
        if (CD.specials[crea] == "LESS3NORM" && Info.GetBatteryCount() < 3)
        {
            Debug.LogFormat("[MonsplodeFight #{0}] Zapra is NORMAL type instead.", _moduleId);
            TYPE = 0; //OVERRIDE TYPE TO NORMAL ZAPRA
        }
        if (CD.specials[crea] == "LESS3ROCK" && Info.GetBatteryCount() < 3)
        {
            Debug.LogFormat("[MonsplodeFight #{0}] Magmy is ROCK type instead.", _moduleId);
            TYPE = 2; //OVERRIDE TYPE TO ROCK MAGMY
        }
        if (CD.specials[crea] == "CARWATER" && Info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.CAR))
        {
            Debug.LogFormat("[MonsplodeFight #{0}] Asteran is WATER type instead.", _moduleId);
            TYPE = 5;
        }
        if (CD.specials[crea] == "CLRWATER" && Info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.CLR))
        {
            Debug.LogFormat("[MonsplodeFight #{0}] Violan is WATER type instead.", _moduleId);
            TYPE = 5;
        }
        if (CD.specials[crea] == "NOLITDARK" && Info.GetOnIndicators().Count() == 0)
        {
            Debug.LogFormat("[MonsplodeFight #{0}] Myrchat is DARK type instead.", _moduleId);
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
                Debug.LogFormat("[MonsplodeFight #{0}] Finale has 10 damage bonus!", _moduleId);
            }
        }
        if (MD.specials[move] == "SIDE")
        {
            int[] lookUp = { 1, 0, 3, 2 };

            DAMAGE = MD.names[moveIDs[lookUp[buttonLocation]]].Count(char.IsLetter);
            Debug.LogFormat("[MonsplodeFight #{0}] Sidestep base damage is {1}, according to neighbor moves' letter count. ({2})", _moduleId, DAMAGE, MD.names[moveIDs[lookUp[buttonLocation]]].Replace('\n', ' '));
        }
        if (MD.specials[move] == "BOO")
        {
            foreach (char c in Info.GetSerialNumber())
                if (c == 'O' || c == '0')
                    DAMAGE += 3;
            Debug.LogFormat("[MonsplodeFight #{0}] Boo O/0 count: {1}", _moduleId, DAMAGE / 3);
        }
        if (MD.specials[move] == "BPOWER")
        {
            DAMAGE += Info.GetBatteryCount() * 2;
            Debug.LogFormat("[MonsplodeFight #{0}] BPower Battery Count: {1}", _moduleId, DAMAGE / 2);
        }
        if (MD.specials[move] == "SHOCKPORT")
        {
            if (Info.IsPortPresent(KMBombInfoExtensions.KnownPortType.RJ45))
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Shock Has RJ45 Bonus!", _moduleId);
                DAMAGE = 8;
            }
        }
        if (MD.specials[move] == "DARKPORT")
        {
            DAMAGE = Info.GetPortCount();
            Debug.LogFormat("[MonsplodeFight #{0}] DPortal Port Count: {1}", _moduleId, Info.GetPortCount());
        }
        if (MD.specials[move] == "LASTDIGIT")
        {
            DAMAGE = Info.GetSerialNumber().Last() - '0';
            Debug.LogFormat("[MonsplodeFight #{0}] LWord Last Digit: {1}", _moduleId, DAMAGE);
        }
        if (MD.specials[move] == "NOSOLVED")
        {
            if (Info.GetSolvedModuleNames().Count() == 0)
            {
                DAMAGE = 10;
                Debug.LogFormat("[MonsplodeFight #{0}] Void has 10 damage bonus!", _moduleId);
            }
        }
        if (MD.specials[move] == "FIERYMUL")
        {
            DAMAGE = Info.GetBatteryCount() * Info.GetBatteryHolderCount(); // IS IT CORRECT?
            Debug.LogFormat("[MonsplodeFight #{0}] Fsoul Batteries: {1}, Holders: {2}", _moduleId, Info.GetBatteryCount(), Info.GetBatteryHolderCount());
        }
        if (MD.specials[move] == "BIGDIG")
        {
            DAMAGE = Info.GetSerialNumberNumbers().Max();
            Debug.LogFormat("[MonsplodeFight #{0}] Stretch Highest Digit: {1}", _moduleId, DAMAGE);
        }
        if (MD.specials[move] == "SMOLDIG")
        {
            DAMAGE = Info.GetSerialNumberNumbers().Min();
            Debug.LogFormat("[MonsplodeFight #{0}] Shrink Smallest Digit: {1}", _moduleId, DAMAGE);
        }
        if (MD.specials[move] == "GORD")
        {
            if (TYPE == 8)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Appearify has 10 damage bonus!", _moduleId);
                DAMAGE = 10;
            }
        }
        if (MD.specials[move] == "RORG")
        {
            if (TYPE == 2 || TYPE == 6)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Sendify has 10 damage bonus!", _moduleId);
                DAMAGE = 10;
            }
        }
        if (MD.specials[move] == "FREAK")
        {
            if (Info.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.FRQ) || Info.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.FRK))
            {
                DAMAGE = 10;
                Debug.LogFormat("[MonsplodeFight #{0}] Freak Out has 10 damage bonus!", _moduleId);
            }
            else if (Info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.FRQ) || Info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.FRK))
            {
                DAMAGE = 5;
                Debug.LogFormat("[MonsplodeFight #{0}] Freak Out has 5 damage bonus!", _moduleId);
            }
        }
        if (MD.specials[move] == "LENGTH")
        {
            DAMAGE = CD.names[crea].Count(char.IsLetter);
            Debug.LogFormat("[MonsplodeFight #{0}] Opponents Name Length: {1}", _moduleId, DAMAGE);
        }
        if (MD.specials[move] == "BUGSPRAY")
        {
            if (CD.names[crea] == "Melbor" || CD.names[crea] == "Zenlad")
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Bug Spray has 10 damage bonus!", _moduleId);
                DAMAGE = 10;
            }
            // ADD BUGS TO HERE
        }
        if (MD.specials[move] == "MODCNT")
        {
            DAMAGE = Info.GetModuleNames().Count();
            Debug.LogFormat("[MonsplodeFight #{0}] Bedrock Module Count: {1}", _moduleId, Info.GetModuleNames().Count());
        }
        if (MD.specials[move] == "TIMELEFT")
        {
            DAMAGE = Mathf.FloorToInt(Info.GetTime() / 60f);
            Debug.LogFormat("[MonsplodeFight #{0}] Countdown Remaining Minutes: {1}", _moduleId, DAMAGE);
        }

        // CREATURE SPECIALS
        // RIM TEAM
        if (CD.specials[crea] == "PORTRIM")
        {
            if (Info.GetPortCount() > 0 && MD.type[move] == 0)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Caadarim takes no damage from NORMAL type.", _moduleId);
                return 0;
            }
        }
        if (CD.specials[crea] == "PARARIM")
        {
            if (Info.IsPortPresent(KMBombInfoExtensions.KnownPortType.Parallel) && MD.type[move] == 0)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Vellarim takes no damage from NORMAL type.", _moduleId);
                return 0;
            }
        }
        if (CD.specials[crea] == "SERIRIM")
        {
            if (Info.IsPortPresent(KMBombInfoExtensions.KnownPortType.Serial) && MD.type[move] == 0)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Flaurim takes no damage from NORMAL type.", _moduleId);
                return 0;
            }
        }
        if (CD.specials[crea] == "DVIRIM")
        {
            if (Info.IsPortPresent(KMBombInfoExtensions.KnownPortType.DVI) && MD.type[move] == 0)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Gloorim takes no damage from NORMAL type.", _moduleId);
                return 0;
            }
        }

        // NO TEAM
        if (CD.specials[crea] == "NOROCK")
        {
            if (MD.type[move] == 2)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Buhar takes no damage from ROCK type.", _moduleId);
                return 0;
            }
        }
        if (CD.specials[crea] == "NOGRASS")
        {
            if (MD.type[move] == 6)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Nibs takes no damage from GRASS type.", _moduleId);
                return 0;
            }
        }
        if (CD.specials[crea] == "NOWATER")
        {
            if (MD.type[move] == 5)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Ukkens takes no damage from WATER type.", _moduleId);
                return 0;
            }
        }

        //PERCY
        if (CD.specials[crea] == "SPLASH" && MD.specials[move] == "SPLASH")
        {
            Debug.LogFormat("[MonsplodeFight #{0}] SPLASH must be used against Percy.", _moduleId);
            return 1000;
        }

        // DOC
        if (CD.specials[crea] == "DOC" && MD.specials[move] == "BOOM")
        {
            Debug.LogFormat("[MonsplodeFight #{0}] BOOM must be used against Docsplode.", _moduleId);
            return 1000; // INFINITY ENOUGH?
        }
        
        // BOOM & CUTIE PIE CASE
        if (CD.specials[crea] == "LOWEST" && MD.specials[move] == "BOOM")
        {
            return 1000;
        }
        // BOB
        if (CD.specials[crea] == "BOB" && Info.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.BOB) && MD.type[move] != 0)
        {
            Debug.LogFormat("[MonsplodeFight #{0}] Bob takes no damage from non-NORMAL type.", _moduleId);
            return 0; // NON NORMALS CANT HIT
        }

        //CALCULATION LINE (FINALLY!)
        float NETDAMAGE = DAMAGE * mulLookup[MD.type[move]][TYPE];
        Debug.LogFormat("[MonsplodeFight #{0}] Base Damage: {1} | Type Multiplier: {2} | Net Damage: {3}", _moduleId, DAMAGE, mulLookup[MD.type[move]][TYPE], NETDAMAGE);

        //MELBOR
        if (CD.specials[crea] == "ZERO68")
        {
            if ((int)NETDAMAGE == 6 || (int)NETDAMAGE == 8)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Melbor takes 0 damage instead.", _moduleId);
                return 0;
            }
        }

        //POUSE
        if (CD.specials[crea] == "ZERO6MORE")
        {
            if ((int)NETDAMAGE >= 6)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Pouse takes 0 damage instead.", _moduleId);
                return 0;
            }
        }

        //ZENLAD
        if (CD.specials[crea] == "ELECPLUS3" && MD.type[move] == 7)
        {
            Debug.LogFormat("[MonsplodeFight #{0}] Zenlad takes extra 3 net damage from ELECTR.", _moduleId);
            return NETDAMAGE + 3;
        }

        //CLONDAR
        if (CD.specials[crea] == "WATERPLUS3" && MD.type[move] == 5)
        {
            Debug.LogFormat("[MonsplodeFight #{0}] Clondar takes extra 3 net damage from WATER.", _moduleId);
            return NETDAMAGE + 3;
        }

        //LANALUFF
        if (CD.specials[crea] == "LUFF" && MD.type[move] == 1 && HasSameChar(CD.names[crea].ToUpper(), Info.GetSerialNumber()))
        {
            Debug.LogFormat("[MonsplodeFight #{0}] Lanaluff has a common letter with serial. Extra 3 net damage.", _moduleId);
            return NETDAMAGE + 3;
        }

        //PILLOWS
        if (CD.specials[crea] == "2F1W")
        {
            if (MD.type[move] == 4)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Aluga takes extra 2 net damage from FIRE.", _moduleId);
                return NETDAMAGE + 2;
            }
            if (MD.type[move] == 5)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Aluga takes 1 less net damage from WATER.", _moduleId);
                return NETDAMAGE - 1;
            }
        }
        if (CD.specials[crea] == "2W1F")
        {
            if (MD.type[move] == 5)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Lugirit takes extra 2 net damage from WATER.", _moduleId);
                return NETDAMAGE + 2;
            }
            if (MD.type[move] == 4)
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Lugirit takes 1 less net damage from FIRE.", _moduleId);
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
            Debug.LogFormat("[MonsplodeFight #{0}] Pressed button before module has been activated!", _moduleId);
            return;
        }
        Debug.LogFormat("[MonsplodeFight #{0}] Opponent: {1}, using Move {2}: {3}", _moduleId, CD.names[crID], buttonID, MD.names[moveIDs[buttonID]].Replace('\n', ' '));

        if (MD.specials[moveIDs[buttonID]] == "BOOM" && CD.specials[crID] != "DOC")
        {
            Module.HandleStrike();
            Module.HandleStrike();
            Module.HandleStrike();
            Module.HandleStrike();
            Module.HandleStrike();
            //BOOM!
            Debug.LogFormat("[MonsplodeFight #{0}] Pressed BOOM!", _moduleId);
        }

        float mxdmg = -100;
        List<int> winners = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            //buttons [i].GetComponentInChildren<TextMesh>().text=MD.names[moveIDs[i]];
            float dmg = CalcDmg(moveIDs[i], crID, i);

            if (CD.specials[crID] == "LOWEST")
            {
                Debug.LogFormat("[MonsplodeFight #{0}] Negate the calculated number for Cutie Pie calculation.", _moduleId);
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
            Debug.LogFormat("[MonsplodeFight #{0}] Move name({1}): {2} | Calculated damage: {3}", _moduleId, i, MD.names[moveIDs[i]].Replace('\n', ' '), dmg);
        }

        if (winners.Contains(buttonID))
        {
            GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, buttons[buttonID].transform);
            Correct();
        }
        else
        {
            Wrong();
        }
    }

    void Correct()
    {
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
        //GOING DOWN
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
        //GOING UP
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

    IEnumerator ProcessTwitchCommand(string command)
    {
        int btn = -1;
        command = command.ToLowerInvariant().Trim();

        //position based
        if (Regex.IsMatch(command, @"^press [a-zA-Z]+$"))
        {
            command = command.Substring(6).Trim();
            switch (command)
            {
                case "tl": case "lt": case "topleft": case "lefttop": btn = 0; break;
                case "tr": case "rt": case "topright": case "righttop": btn = 1; break;
                case "bl": case "lb": case "buttomleft": case "leftbuttom": btn = 2; break;
                case "br": case "rb": case "bottomright": case "rightbottom": btn = 3; break;
                default: yield break;
            }
        }
        else
        { 
            //direct name with "use"
            if (Regex.IsMatch(command, @"^use [a-z ]+$"))
            {
                command = command.Substring(4).Trim();
            }

            //direct name without "use"
            if (command == MD.names[moveIDs[0]].Replace('\n', ' ').ToLowerInvariant()) btn = 0;
            else if (command == MD.names[moveIDs[1]].Replace('\n', ' ').ToLowerInvariant()) btn = 1;
            else if (command == MD.names[moveIDs[2]].Replace('\n', ' ').ToLowerInvariant()) btn = 2;
            else if (command == MD.names[moveIDs[3]].Replace('\n', ' ').ToLowerInvariant()) btn = 3;
            else yield break;
        }
        if (btn == -1) yield break;

        yield return null;
        if (MD.specials[moveIDs[btn]] == "BOOM" && CD.specials[crID] != "DOC")
        {
            yield return "multiple strikes";
            OnPress(btn);
            yield return "award strikes 5";
        }
        else
        {
            OnPress(btn);
        }
    }
}
