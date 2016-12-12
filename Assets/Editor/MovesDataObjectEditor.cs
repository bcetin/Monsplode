using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MovesDataObject))]
public class MovesDataObjectEditor : Editor
{

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();
		MovesDataObject myTarget= (MovesDataObject)target;
		EditorGUI.BeginChangeCheck ();
		myTarget.size = EditorGUILayout.IntField ("Move Count:",myTarget.size);
		EditorGUILayout.LabelField ("ID Name Type Damage Special");
		for (int i = 0; i < myTarget.size; i++)
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("ID: " + i,GUILayout.Width(45f));
			myTarget.names [i] = EditorGUILayout.TextField (myTarget.names [i],GUILayout.Width(120f)).Replace("NEW","\n");
			myTarget.type[i]=EditorGUILayout.Popup(myTarget.type[i],myTarget.typesData.names,GUILayout.Width(100f));
			myTarget.damage [i] = EditorGUILayout.IntField (myTarget.damage [i],GUILayout.Width(120f));
			myTarget.specials [i] = EditorGUILayout.TextField (myTarget.specials [i],GUILayout.Width(120f));
			EditorGUILayout.EndHorizontal();
		}
		if(EditorGUI.EndChangeCheck ())
			EditorUtility.SetDirty (target);

	}
}
