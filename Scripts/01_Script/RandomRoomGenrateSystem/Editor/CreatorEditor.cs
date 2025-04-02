#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoomGenrator))]
public class CreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        RoomGenrator dungeonCreator = (RoomGenrator)target;
        if (GUILayout.Button("CreateNewDungeon"))
        {
            dungeonCreator.DefaultSetting();
        }
        if (GUILayout.Button("DestroyDungeon"))
        {
            dungeonCreator.DestroyAll();
        }
    }
}
#endif
