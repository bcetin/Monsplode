using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TypesDataObject))]
public class TypesDataObjectEditor : Editor
{

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI ();
		TypesDataObject myTarget= (TypesDataObject)target;
		EditorGUI.BeginChangeCheck ();
		/*if(myTarget.mulLookup==null)
		{
			myTarget.mulLookup=new float[20][];
			for (int i = 0; i < 20; i++)
				myTarget.mulLookup [i] = new float[20];
		}*/
		myTarget.size = EditorGUILayout.IntField ("Type Count:",myTarget.size);
		for (int i = 0; i < myTarget.size; i++)
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Type Name: " + i);
			myTarget.names [i] = EditorGUILayout.TextField (myTarget.names [i]);
			EditorGUILayout.EndHorizontal();
		}

		/*EditorGUILayout.BeginHorizontal ();
		GUILayout.Label("CORNER",GUILayout.Width(50));
		for (int i = 0; i < myTarget.size; i++)
		{
			GUILayout.Label (myTarget.names[i],GUILayout.Width(50));
		}
		EditorGUILayout.EndHorizontal();
		for (int i = 0; i < myTarget.size; i++)
		{
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (myTarget.names[i],GUILayout.Width(50));
			for (int j = 0; j < myTarget.size; j++)
			{
				myTarget.mulLookup[i][j]=EditorGUILayout.FloatField(myTarget.mulLookup[i][j],GUILayout.Width(50));
			}
			EditorGUILayout.EndHorizontal();
		}*/
		/*if (GUILayout.Button ("PUT data to PREFAB"))
		{
			//COPY(GameObject.Find ("CreatureModule").GetComponent<CreatureModule>().fukenTypeData,myTarget.mulLookup);
			Undo.RecordObject (myTarget.prefab,"Applied Shit To Prefab");
			myTarget.prefab.GetComponent<CreatureModule>().fukenTypeData=new float[20][];
			for (int i = 0; i < 20; i++)
			{
				myTarget.prefab.GetComponent<CreatureModule>().fukenTypeData [i] = new float[20];
				for (int j = 0; j < 20; j++)
					myTarget.prefab.GetComponent<CreatureModule>().fukenTypeData [i] [j] =myTarget.mulLookup [i][j];
			}

			Undo.FlushUndoRecordObjects ();
			/*
			GameObject FML = GameObject.Find ("CreatureModule");
			for (int i = 0; i < 20; i++)
				for (int j = 0; j < 20; j++)
					Debug.Log(i+" "+j+" " +FML.GetComponent<CreatureModule>().fukenTypeData[i][i]);
		}
		if (GUILayout.Button ("TAKE data from Prefab"))
		{
			//COPY(myTarget.mulLookup,GameObject.Find ("CreatureModule").GetComponent<CreatureModule> ().fukenTypeData);
			myTarget.mulLookup=new float[20][];
			for (int i = 0; i < 20; i++)
			{
				myTarget.mulLookup [i] = new float[20];
				for (int j = 0; j < 20; j++)
					myTarget.mulLookup [i] [j] = myTarget.prefab.GetComponent<CreatureModule>().fukenTypeData[i][j];
			}
		}*/
		if(EditorGUI.EndChangeCheck ())
			EditorUtility.SetDirty (target);

	}
	/*
	public void COPY(float[][] A,float[][] B)
	{
		A=new float[20][];
		for (int i = 0; i < 20; i++)
		{
			A [i] = new float[20];
			for (int j = 0; j < 20; j++)
				A [i] [j] = B [i] [j];
		}
	}*/
}
