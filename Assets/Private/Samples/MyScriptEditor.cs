#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MyScript))]
public class MyScriptEditor : Editor
{
    private SerializedProperty myClassProperty;

    private void OnEnable()
    {
        myClassProperty = serializedObject.FindProperty("myClassProperty");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // クラス型のプロパティにアクセスして値を設定する
        myClassProperty.FindPropertyRelative("a").intValue =
        EditorGUILayout.IntField(myClassProperty.FindPropertyRelative("a").intValue);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif