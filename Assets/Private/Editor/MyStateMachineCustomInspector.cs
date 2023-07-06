#if UNITY_EDITOR
// 日本語対応
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MyStateMachine))]
public class MyStateMachineCustomInspector : Editor
{
    private MyStateMachine _stateMachine = null;

    private Enum[] _enumValues = null;
    private string[] _enumNames = null;
    private int _enumLength = -1;

    private Type _previousStateType;

    private void OnEnable()
    {
        _stateMachine = target as MyStateMachine;
        if (_stateMachine != null && _stateMachine.CurrentState != null)
        {
            _previousStateType = _stateMachine.CurrentState.GetType();
            EnumArraySetup();
        }
    }
    public override void OnInspectorGUI()
    {
        // State Type の更新
        // 実行中は変更不可
        serializedObject.Update();
        var currentState = serializedObject.FindProperty("_currentState");
        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
        EditorGUILayout.PropertyField(currentState, new GUIContent("StateType"));
        EditorGUI.EndDisabledGroup();
        serializedObject.ApplyModifiedProperties();


        // 必要であれば、各自セットアップ処理を走らせる。
        if (_previousStateType != _stateMachine.CurrentState.GetType())
        {
            EnumArraySetup();
            _stateMachine.Setup();
            _stateMachine.Conditions.Setup(_stateMachine.CurrentState.GetType(), _enumLength);
        }
        _previousStateType = _stateMachine.CurrentState.GetType();

        ConditionDrawer(serializedObject.FindProperty("_conditions"));
    }

    private void EnumArraySetup()
    {
        Array array = Enum.GetValues(_stateMachine.CurrentState.GetType());
        _enumValues = new Enum[array.Length];
        _enumNames = new string[array.Length];
        _enumLength = array.Length;
        int counter = 0;
        foreach (var e in array)
        {
            _enumValues[counter] = (Enum)e;
            _enumNames[counter] = _enumValues[counter].ToString();
            counter++;
        }
    }

    private InspectorLayoutOptions _inspectorLayoutOptions = new InspectorLayoutOptions();

    // 条件をグリッド状に表示する
    private void ConditionDrawer(SerializedProperty conditions)
    {
        _inspectorLayoutOptions ??= new InspectorLayoutOptions(); // 謎のタイミングでインスタンスが破棄、あるいは参照が外れるので、そのための処理
        serializedObject.Update();

        // 上部ラベルを表示する
        DrawTopLabel();

        // 現在のステートを表示する
        DrawCurrentState();

        // 左部ラベルとトグルの表示、更新処理を行う
        DrawLeftLabelAndToggle(conditions);

        // 全てのトグルに対して共通の値を設定するボタンを表示、更新処理を行う
        DrawSetConditionsAllButton(conditions);

        serializedObject.ApplyModifiedProperties();
    }
    private void DrawTopLabel()
    {
        // 上部ラベルの表示
        using (new EditorGUILayout.HorizontalScope(_inspectorLayoutOptions.TopLabelOuterWidthOption))
        {
            EditorGUILayout.LabelField("", _inspectorLayoutOptions.LeftLabelWidthOption);
            for (int i = 0; i < _enumNames.Length; i++)
            {
                using (new EditorGUILayout.VerticalScope(_inspectorLayoutOptions.TopLabelHeightOption))
                {
                    for (int j = 0; j < _enumNames[i].Length; j++)
                    {
                        EditorGUILayout.LabelField(_enumNames[i][j].ToString(), _inspectorLayoutOptions.TopLabelInnerWidthOption);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 現在の状態を表示する
    /// </summary>
    private void DrawCurrentState()
    {
        EditorGUI.BeginDisabledGroup(true); // 基本操作しないためインスペクタからの操作を無効にする。
        using (new EditorGUILayout.HorizontalScope(_inspectorLayoutOptions.CurrentToggleStateWidthOption))
        {
            EditorGUILayout.LabelField("Current State", _inspectorLayoutOptions.LeftLabelWidthOption);
            for (int i = 0; i < _enumValues.Length; i++)
            {
                var value = false;
                if (_stateMachine.CurrentState != null) value = _stateMachine.CurrentState.HasFlag(_enumValues[i]);
                EditorGUILayout.Toggle(value, _inspectorLayoutOptions.ToggleStyle, _inspectorLayoutOptions.CurrentToggleStateWidthOption);
            }
        }
        EditorGUI.EndDisabledGroup();
    }
    /// <summary>
    /// 左部ラベルとトグルの表示、更新処理を行う
    /// </summary>
    private void DrawLeftLabelAndToggle(SerializedProperty conditions)
    {
        EditorGUILayout.BeginVertical(); // 縦に並べる
        for (int i = 0; i < _enumLength; i++)
        {
            EditorGUILayout.BeginHorizontal(); // 横に並べる
            // 左部ラベルを表示する
            EditorGUILayout.LabelField(_enumNames[i], _inspectorLayoutOptions.LeftLabelWidthOption);
            // トグルを表示する
            for (int j = 0; j < _enumLength; j++)
            {
                if (i == j) { GUILayout.Space(InspectorLayoutOptions._toggleWidth); continue; }

                try
                {
                    int index = i * _enumLength + j;
                    SerializedProperty property =
                        conditions.FindPropertyRelative("_conditions").GetArrayElementAtIndex(index);

                    property.boolValue =
                        EditorGUILayout.Toggle(property.boolValue, _inspectorLayoutOptions.ToggleStyle,
                        _inspectorLayoutOptions.ToggleWidthOption, _inspectorLayoutOptions.ToggleHeightOption);
                }
                catch (NullReferenceException)
                {
                    Debug.LogError("Error!");
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
    private void DrawSetConditionsAllButton(SerializedProperty conditions)

    {// 全てのトグルに対して共通の値を設定する
        EditorGUILayout.BeginHorizontal(); // 横に並べる
        if (GUILayout.Button("Select All"))
        {
            // 全てのトグルを有効にする
            SetConditionsAll(
                conditions.FindPropertyRelative("_conditions"),
                true);
        }
        if (GUILayout.Button("Unselect All"))
        {
            // 全てのトグルを無効にする
            SetConditionsAll(
                conditions.FindPropertyRelative("_conditions"),
                false);
        }
        EditorGUILayout.EndHorizontal();
    }
    private void SetConditionsAll(SerializedProperty arraySerializedProperty, bool value)
    {
        for (int i = 0; i < _enumLength; i++)
        {
            for (int j = 0; j < _enumLength; j++)
            {
                int index = i * _enumLength + j;
                arraySerializedProperty.GetArrayElementAtIndex(index).boolValue = value;
            }
        }
    }
    private class InspectorLayoutOptions
    {
        // ----- トグル関連の値 ----- //
        public const float _toggleWidth = 16f;
        public const float _toggleHeight = 16f;

        private GUIStyle _toggleStyle;

        // ----- 上部ラベル関連の値 ----- //
        public const float _topOuterLabelWidth = 16f;
        public const float _topLabelHeight = 16f;

        public const float _topInnerLabelWidth = 12.4f;

        // ----- 左部ラベル関連の値 ----- //
        public const float _labelWidth = 120f;

        // ----- カレントステートオプション ----- //
        public const float _currentToggleStateWidth = 15.68f;

        // ----- トグルオプション ----- //
        public GUILayoutOption ToggleWidthOption => GUILayout.Width(_toggleWidth);
        public GUILayoutOption ToggleHeightOption => GUILayout.Height(_toggleHeight);
        public GUIStyle ToggleStyle => _toggleStyle ??= new GUIStyle(GUI.skin.toggle);

        // ----- 上部ラベルオプション ----- //
        public GUILayoutOption TopLabelOuterWidthOption => GUILayout.Width(_topOuterLabelWidth);
        public GUILayoutOption TopLabelInnerWidthOption => GUILayout.Width(_topInnerLabelWidth);
        public GUILayoutOption TopLabelHeightOption => GUILayout.Height(_topLabelHeight);

        // ----- 上部ラベルオプション ----- //
        public GUILayoutOption LeftLabelWidthOption => GUILayout.Width(_labelWidth);

        // ----- カレントステートオプション ----- //
        public GUILayoutOption CurrentToggleStateWidthOption => GUILayout.Width(_currentToggleStateWidth);
    }
}
[Flags, Serializable]
public enum Sample
{
    a = 0b1,
    b = 0b10,
    c = 0b100,
    d = 0b1000,
    e = 0b10000,
    f = 0b100000,
    g = 0b1000000,
    h = 0b10000000,
}
#endif