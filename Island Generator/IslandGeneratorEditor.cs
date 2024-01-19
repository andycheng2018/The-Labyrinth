using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC {
	#if UNITY_EDITOR
	[CustomEditor(typeof(IslandGenerator))]
	public class IslandGeneratorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			IslandGenerator islandGenator = (IslandGenerator)target;

			if (DrawDefaultInspector())
			{
				if (islandGenator.autoUpdate) {
					islandGenator.ClearVegetation();
					islandGenator.GenerateIsland();
				}
			}

			if (GUILayout.Button("Generate Island"))
			{
				islandGenator.GenerateIsland();
			}

			if (GUILayout.Button("Clear Vegetations"))
			{
				islandGenator.ClearVegetation();
			}
		}
	}
	#endif
}
