using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class MonsplodeWhoModule : MonoBehaviour
{
    public KMAudio Audio;
    public KMNeedyModule NeedyModule;
    public CreatureDataObject CD;
    public SpriteRenderer screenSR;
    public KMSelectable[] buttons;
    public Transform[] DP, UP;
    public int timeGain, timeMax;
    public KMModSettings modSet;
    int crID;
    public float moveDelta;
    bool leftTrue, isActivated = false, revive = false;
    private string textLeft, textRight;
    bool alarmEnabled = false;
    KMAudio.KMAudioRef audioRef = null;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        string[] setWords = modSet.Settings.Split(new char[] { ' ', '\n', '\t', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (setWords != null && setWords.Length > 1) {
            bool ae = false;
            if (bool.TryParse(setWords[1], out ae)) {
                alarmEnabled = ae;
            }
        }
    }

    void Awake()
    {
        NeedyModule.OnNeedyActivation += OnNeedyActivation;
        NeedyModule.OnNeedyDeactivation += OnNeedyDeactivation;
        NeedyModule.OnTimerExpired += OnTimerExpired;
        buttons[0].OnInteract += delegate ()
        {
            OnPress(true, 0);
            return false;
        };
        buttons[1].OnInteract += delegate ()
        {
            OnPress(false, 1);
            return false;
        };
    }

    void Update() {
        if (!alarmEnabled) return;
        if (isActivated && audioRef == null && NeedyModule.GetNeedyTimeRemaining() < 5f) {
            audioRef = Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.NeedyWarning, this.transform);
        }
        else if (audioRef != null && (!isActivated || NeedyModule.GetNeedyTimeRemaining() >= 5f)) {
            audioRef.StopSound();
            audioRef = null;
		}
	}

    protected bool Solve()
    {
        NeedyModule.OnPass();
        return false;
    }

    protected void OnNeedyActivation()
    {
        TurnStuffOff();
        isActivated = true;
        StartCoroutine(GoUp());
    }

    protected void OnNeedyDeactivation()
    {
        StartCoroutine(GoDown());
    }

    protected void OnPress(bool isLeft, int buttonID)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[buttonID].transform);
        buttons[buttonID].AddInteractionPunch();

        if (!isActivated)
            return;
        if (isLeft == leftTrue)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, buttons[buttonID].transform);
            Debug.LogFormat("[Who's that Monsplode? #{0}] Answer is correct! Added 20 seconds.", _moduleId);
            AddTime();
            ResetModule();
        }
        else
        {
            Debug.LogFormat("[Who's that Monsplode? #{0}] Answer is incorrect! Strike!", _moduleId);
            ResetModule();
            GetComponent<KMNeedyModule>().HandleStrike();
        }
    }

    protected void OnTimerExpired()
    {
        StartCoroutine(GoDown());
        NeedyModule.HandleStrike();
    }

    void ResetModule()
    {
        revive = true;
        StartCoroutine(GoDown());
        if (audioRef != null) {
            audioRef.StopSound();
            audioRef = null;
		}
    }

    void PickCreatures()
    {
        crID = Random.Range(0, CD.size);
        screenSR.sprite = CD.sprites[crID];
        int hold = Random.Range(0, CD.size);
        if (hold == crID) hold++;
        if (hold == CD.size)
            hold = 0;
        string right = CD.names[crID], wrong = CD.names[hold];
        if (Random.Range(0, 2) == 0)
        {
            buttons[0].GetComponentInChildren<TextMesh>().text = right;
            buttons[1].GetComponentInChildren<TextMesh>().text = wrong;
            leftTrue = true;
            textLeft = right;
            textRight = wrong;
            Debug.LogFormat("[Who's that Monsplode? #{0}] Correct answer is {1}, which is the left button.", _moduleId, right);
        }
        else
        {
            buttons[0].GetComponentInChildren<TextMesh>().text = wrong;
            buttons[1].GetComponentInChildren<TextMesh>().text = right;
            leftTrue = false;
            textLeft = wrong;
            textRight = right;
            Debug.LogFormat("[Who's that Monsplode? #{0}] Correct answer is {1}, which is the right button.", _moduleId, right);
        }
    }

    IEnumerator GoDown()
    {
        isActivated = false;
        TurnStuffOff();
        //Debug.Log ("GOING DOWN");
        while (buttons[0].transform.position != DP[0].transform.position)
        {
            for (int i = 0; i < 2; i++)
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
        while (buttons[0].transform.position != UP[0].transform.position)
        {
            for (int i = 0; i < 2; i++)
                buttons[i].transform.position = Vector3.MoveTowards(buttons[i].transform.position, UP[i].transform.position, moveDelta);
            yield return new WaitForSeconds(0.02f);
        }
        PickCreatures();
        TurnStuffOn();
        isActivated = true;
    }

    void TurnStuffOff()
    {
        screenSR.enabled = false;
        foreach (KMSelectable but in buttons)
            but.GetComponentInChildren<TextMesh>().text = "";
    }

    void TurnStuffOn()
    {
        screenSR.enabled = true;
    }

    protected bool AddTime()
    {
        float time = NeedyModule.GetNeedyTimeRemaining();
        if (time > 0)
        {
            NeedyModule.SetNeedyTimeRemaining(Mathf.Min(time + timeGain, timeMax));
        }
        return false;
    }

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        var btn = new List<KMSelectable>();
        command = command.ToLowerInvariant().Trim();

        //position based
        if (Regex.IsMatch(command, @"^press [a-zA-Z]+$"))
        {
            command = command.Substring(6).Trim();

            if (command.Equals("1") || command.Equals("left") || command.Equals("l")) btn.Add(buttons[0]);
            else if (command.Equals("2") || command.Equals("right") || command.Equals("r")) btn.Add(buttons[1]);
            else return null;

            return btn.ToArray();
        }

        //direct name with "name"
        else if (Regex.IsMatch(command, @"^name [a-z ]+$"))
        {
            command = command.Substring(5).Trim();
        }

        //direct name without "name"
        if (command == textLeft.ToLowerInvariant()) btn.Add(buttons[0]);
        else if (command == textRight.ToLowerInvariant()) btn.Add(buttons[1]);
        else return null;

        return btn.ToArray();
    }
}
