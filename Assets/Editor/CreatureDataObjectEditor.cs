using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CreatureDataObject))]
public class CreatureDataObjectEditor : Editor
{

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();
		CreatureDataObject myTarget= (CreatureDataObject)target;
		EditorGUI.BeginChangeCheck ();
		//if (myTarget.names==null) IF YOU PASS 50 ITEMS CMERE AND FIX!!!!!!!!!!!
		//	myTarget.names = new string[100];
		//if (myTarget.sprites == null)
		//	myTarget.sprites = new Sprite[100];
		//	myTarget.isMaterial = new bool[100];
			//myTarget.dangerLvl = new int[100];
		//Debug.Log (myTarget.sprites.Length);
		myTarget.size = EditorGUILayout.IntField ("Creature Count:",myTarget.size);
		EditorGUILayout.LabelField ("ID Name Sprite Type Special");
		for (int i = 0; i < myTarget.size; i++)
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("ID: " + i,GUILayout.Width(45f));
			myTarget.names [i] = EditorGUILayout.TextField (myTarget.names [i],GUILayout.Width(120f));
			myTarget.sprites[i]=(Sprite)EditorGUILayout.ObjectField (myTarget.sprites[i],typeof(Sprite),false,GUILayout.Width(100f));
			myTarget.type[i]=EditorGUILayout.Popup(myTarget.type[i],myTarget.typesData.names,GUILayout.Width(100f));
			myTarget.specials [i] = EditorGUILayout.TextField (myTarget.specials [i],GUILayout.Width(120f));
			EditorGUILayout.EndHorizontal();
		
		}
		if(EditorGUI.EndChangeCheck ())
			EditorUtility.SetDirty (target);

	}
}
