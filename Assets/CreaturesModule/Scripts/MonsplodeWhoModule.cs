using UnityEngine;
using System.Collections;

public class MonsplodeWhoModule : MonoBehaviour
{
	public CreatureDataObject CD;
	public SpriteRenderer screenSR;
	public KMSelectable[] buttons;
	public Transform[] DP,UP;
	public int timeGain,timeMax;
	int crID;
	public float moveDelta;
	bool leftTrue,isActivated = false,revive=false;

    void Awake()
    {
        GetComponent<KMNeedyModule>().OnNeedyActivation += OnNeedyActivation;
        GetComponent<KMNeedyModule>().OnNeedyDeactivation += OnNeedyDeactivation;
		buttons[0].OnInteract += delegate () {
			OnPress (true);
			return false;
		};
		buttons[1].OnInteract += delegate () {
			OnPress (false);
			return false;
		};
        GetComponent<KMNeedyModule>().OnTimerExpired += OnTimerExpired;
    }

    protected bool Solve()
    {
        GetComponent<KMNeedyModule>().OnPass();
        return false;
    }

    protected void OnNeedyActivation()
    {
		TurnStuffOff ();
		isActivated = true;
		StartCoroutine (GoUp());
    }

    protected void OnNeedyDeactivation()
    {
		StartCoroutine (GoDown());
    }

	protected void OnPress(bool isLeft)
	{
		if (!isActivated)
			return;
		if (isLeft == leftTrue) {
			GetComponent<KMAudio> ().PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.CorrectChime, transform);
			AddTime ();
			ResetModule ();
		}
		else
		{
			ResetModule ();
			GetComponent<KMNeedyModule> ().HandleStrike ();
		}
	}

    protected void OnTimerExpired()
    {
		StartCoroutine (GoDown());
		GetComponent<KMNeedyModule>().HandleStrike();
    }


	void ResetModule()
	{
		revive = true;
		StartCoroutine (GoDown());
	}
	void PickCreatures()
	{
		crID=Random.Range(0,CD.size);
		screenSR.sprite = CD.sprites [crID];
		int hold=Random.Range(0,CD.size);
		if(hold==crID) hold++;
		if (hold == CD.size)
			hold = 0;
		string right=CD.names[crID], wrong=CD.names[hold];
		if (Random.Range (0, 2) == 0)
		{
			buttons [0].GetComponentInChildren<TextMesh> ().text = right;
			buttons [1].GetComponentInChildren<TextMesh> ().text = wrong;
			leftTrue = true;
		}
		else
		{
			buttons [0].GetComponentInChildren<TextMesh> ().text = wrong;
			buttons [1].GetComponentInChildren<TextMesh> ().text = right;
			leftTrue = false;
		}
	}

	IEnumerator GoDown()
	{
		isActivated = false;
		TurnStuffOff ();
		//Debug.Log ("GOING DOWN");
		while(buttons[0].transform.position!=DP[0].transform.position)
		{
			for (int i=0;i<2;i++)
				buttons[i].transform.position=Vector3.MoveTowards (buttons[i].transform.position,DP[i].transform.position , moveDelta);
			yield return new WaitForSeconds(0.02f);
		}
		if (revive)
		{
			revive = false;
			StartCoroutine (GoUp());
		}

	}
	IEnumerator GoUp()
	{
		while(buttons[0].transform.position!=UP[0].transform.position)
		{
			for (int i=0;i<2;i++)
				buttons[i].transform.position=Vector3.MoveTowards (buttons[i].transform.position,UP[i].transform.position , moveDelta);
			yield return new WaitForSeconds(0.02f);
		}
		PickCreatures ();
		TurnStuffOn ();
		isActivated = true;
	}
	void TurnStuffOff()
	{
		screenSR.enabled = false;
		foreach (KMSelectable but in buttons)
			but.GetComponentInChildren<TextMesh> ().text="";
	}

	void TurnStuffOn()
	{
		screenSR.enabled = true;
	}

	protected bool AddTime()
    {
        float time = GetComponent<KMNeedyModule>().GetNeedyTimeRemaining();
        if (time > 0)
        {
			GetComponent<KMNeedyModule>().SetNeedyTimeRemaining(Mathf.Min(time + timeGain,timeMax));
        }

        return false;
    }
}