using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
#if UNITY_EDITOR
    [CustomEditor(typeof(DungeonGenerator))]
    public class DungeonGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DungeonGenerator dungeonGenerator = (DungeonGenerator)target;

            if (DrawDefaultInspector())
            {

            }

            if (GUILayout.Button("Generate Dungeon"))
            {
                dungeonGenerator.GenerateDungeon();
            }

            if (GUILayout.Button("Clear Rooms"))
            {
                dungeonGenerator.ClearRooms();
            }
        }
    }
#endif
}