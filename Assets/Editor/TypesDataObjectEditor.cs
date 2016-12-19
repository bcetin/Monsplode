using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
[CustomEditor(typeof(TypesDataObject))]
public class TypesDataObjectEditor : Editor
{

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();
		TypesDataObject myTarget= (TypesDataObject)target;
		EditorGUI.BeginChangeCheck ();
		
		myTarget.size = EditorGUILayout.IntField ("Type Count:",myTarget.size);
		Array.Resize(ref myTarget.names, myTarget.size);
		for (int i = 0; i < myTarget.size; i++)
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Type Name: " + i);
			myTarget.names [i] = EditorGUILayout.TextField (myTarget.names [i]);
			EditorGUILayout.EndHorizontal();
		}
			
		if(EditorGUI.EndChangeCheck ())
			EditorUtility.SetDirty (target);

	}

}
