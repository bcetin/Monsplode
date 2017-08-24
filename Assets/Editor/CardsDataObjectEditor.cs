using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[CustomEditor(typeof(CardsDataObject))]
public class CardsDataObjectEditor : Editor
{

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		CardsDataObject myTarget = (CardsDataObject)target;
		EditorGUI.BeginChangeCheck();
		myTarget.size = EditorGUILayout.IntField("Monsplode Type Count:", myTarget.size);
		if (myTarget.names == null) myTarget.names = new string[1];
		if (myTarget.sprites == null) myTarget.sprites = new Sprite[1];
		if (myTarget.inital == null) myTarget.inital = new Vector3[1];
		Array.Resize(ref myTarget.names, myTarget.size);
		Array.Resize(ref myTarget.sprites, myTarget.size);
		Array.Resize(ref myTarget.inital, myTarget.size);
		EditorGUILayout.LabelField("ID Name Sprite Lit>=<Unlit");
		//Debug.Log(myTarget.inital.Length);
		for (int i = 0; i < myTarget.size; i++)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("ID: " + i, GUILayout.Width(45f));
			myTarget.names[i] = EditorGUILayout.TextField(myTarget.names[i], GUILayout.Width(120f))/*.Replace("NEW", "\n")*/;
			myTarget.sprites[i] = (Sprite)EditorGUILayout.ObjectField(myTarget.sprites[i], typeof(Sprite), false, GUILayout.Width(100f));
			/*if (myTarget.inital[i] == null)
				myTarget.inital[i] = Vector3.zero;*/
			int x= EditorGUILayout.IntField((int)myTarget.inital[i].x, GUILayout.Width(30f));
			int y= EditorGUILayout.IntField((int)myTarget.inital[i].y, GUILayout.Width(30f));
			int z= EditorGUILayout.IntField((int)myTarget.inital[i].z, GUILayout.Width(30f));
			myTarget.inital[i] = new Vector3(x,y,z);
			EditorGUILayout.EndHorizontal();
		}
		if (EditorGUI.EndChangeCheck())
			EditorUtility.SetDirty(target);

	}
}
