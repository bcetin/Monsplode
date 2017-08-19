using UnityEngine;
using UnityEditor;

public class YourClassAsset
{
	[MenuItem("Assets/Create/CreatureDataObject")]
	public static void CreateAsset ()
	{
		ScriptableObjectUtility.CreateAsset<CreatureDataObject> ();
	}
	[MenuItem("Assets/Create/TypeDataObject")]
	public static void CreateAnotherAsset ()
	{
		ScriptableObjectUtility.CreateAsset<TypesDataObject> ();
	}
	[MenuItem("Assets/Create/MoveDataObject")]
	public static void CreateYetAnotherAsset ()
	{
		ScriptableObjectUtility.CreateAsset<MovesDataObject> ();
	}
	[MenuItem("Assets/Create/CardsDataObject")]
	public static void CreateABrandNewAsset()
	{
		ScriptableObjectUtility.CreateAsset<CardsDataObject>();
	}
}