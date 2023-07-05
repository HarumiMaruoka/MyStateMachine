#if UNITY_EDITOR
// 日本語対応
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MyStateMachine))]
public class MyStateMachineCustomInspector : Editor
{
    private MyStateMachine _stateMachine = null;

    private Type _previousStateType;

    private int _initializeConditionsCount = 0;

    private void OnEnable()
    {
        _stateMachine = target as MyStateMachine;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            _initializeConditionsCount = 0;
        }
    }
    Enum[] _enumValues = null;
    public override void OnInspectorGUI()
    {
        // Current State の更新
        serializedObject.Update();

        var currentState = serializedObject.FindProperty("_currentState");
        EditorGUILayout.PropertyField(currentState, new GUIContent("StateType"));

        serializedObject.ApplyModifiedProperties();

        // EnumのArreyを作成する
        if (_enumValues == null || _previousStateType != _stateMachine.CurrentState.GetType())
        {
            Array array = Enum.GetValues(_stateMachine.CurrentState.GetType());
            _enumValues = new Enum[array.Length];
            int counter = 0;
            foreach (var e in array)
            {
                _enumValues[counter] = (Enum)e;
                counter++;
            }
        }

        // 必要であれば、各自セットアップ処理を走らせる。
        if (_previousStateType != _stateMachine.CurrentState.GetType())
        {
            if (_initializeConditionsCount > 0)
            {
                _stateMachine.Setup();
                _stateMachine.Conditions.Setup(_stateMachine.CurrentState.GetType());
            }
            else
            {
                _initializeConditionsCount++;
            }
        }

        _previousStateType = _stateMachine.CurrentState.GetType();

        ConditionDrawer(serializedObject.FindProperty("_conditions"),
            _stateMachine.CurrentState.GetType(), _enumValues);
    }

    // ===== 条件関連 ============================================================================================================== //
    // ----- トグル関連の値 ----- //
    private const float _toggleWidth = 16f;
    private const float _toggleHeight = 16f;

    private GUIStyle _toggleStyle = null;

    private GUILayoutOption[] _toggleGuiOptions = new GUILayoutOption[2];

    // ----- 上部ラベル関連の値 ----- //
    private const float _topOuterLabelWidth = 16f;
    private const float _topLabelHeight = 16f;

    private GUILayoutOption[] _topLabelGuiOptions = new GUILayoutOption[2];

    private const float _topInnerLabelWidth = 12.4f;

    // ----- 左部ラベル関連の値 ----- //
    private const float _labelWidth = 120f;

    // 条件をグリッド状に表示する
    private void ConditionDrawer(SerializedProperty conditions, Type type, Enum[] enumValues)
    {
        serializedObject.Update();
        // 値のセットアップ
        GUILayoutOption toggleWidthOption = GUILayout.Width(_toggleWidth);
        GUILayoutOption toggleHeightOption = GUILayout.Height(_toggleHeight);

        _toggleGuiOptions = new GUILayoutOption[2];
        _toggleGuiOptions[0] = toggleWidthOption;
        _toggleGuiOptions[1] = toggleHeightOption;

        GUILayoutOption topLabelWidthOption = GUILayout.Width(_topOuterLabelWidth);
        GUILayoutOption topLabelHeightOption = GUILayout.Height(_topLabelHeight);

        _topLabelGuiOptions = new GUILayoutOption[2];
        _topLabelGuiOptions[0] = topLabelHeightOption;
        _topLabelGuiOptions[1] = topLabelWidthOption;

        _toggleStyle ??= new GUIStyle(GUI.skin.toggle);

        GUILayoutOption labelWidthOption = GUILayout.Width(_labelWidth);

        GUILayoutOption currentStateWidthOption = GUILayout.Width(15.68f);

        // Enumに登録されたフィールドの名前をすべて取得する
        Array typeValues = Enum.GetValues(type);
        int typeLength = typeValues.Length;

        string[] enumNames = new string[typeValues.Length];

        for (int i = 0; i < typeLength; i++)
        {
            enumNames[i] = typeValues.GetValue(i).ToString();
        }

        // 上部ラベルの表示
        using (new EditorGUILayout.HorizontalScope(topLabelWidthOption))
        {
            EditorGUILayout.LabelField("", labelWidthOption);
            for (int i = 0; i < enumNames.Length; i++)
            {
                using (new EditorGUILayout.VerticalScope(topLabelHeightOption))
                {
                    for (int j = 0; j < enumNames[i].Length; j++)
                    {
                        EditorGUILayout.LabelField(enumNames[i][j].ToString(), GUILayout.Width(_topInnerLabelWidth));
                    }
                }
            }
        }

        // 現在の状態を表示する
        using (new EditorGUILayout.HorizontalScope(currentStateWidthOption))
        {
            EditorGUILayout.LabelField("Current State", labelWidthOption);
            for (int i = 0; i < enumValues.Length; i++)
            {
                var value = false;
                if (_stateMachine.CurrentState != null) value = _stateMachine.CurrentState.HasFlag(enumValues[i]);
                EditorGUILayout.Toggle(value, _toggleStyle, currentStateWidthOption);
            }
        }

        conditions.FindPropertyRelative("_size").intValue = typeLength;
        // 左部ラベルとトグルを表示
        using (new EditorGUILayout.VerticalScope())
        {
            for (int i = 0; i < typeLength; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(enumNames[i], labelWidthOption);
                    for (int j = 0; j < typeLength; j++)
                    {
                        if (i == j) { GUILayout.Space(_toggleWidth); continue; }

                        int index = i * typeLength + j;

                        try
                        {
                            ToggleUpdate(conditions.FindPropertyRelative("_conditions"), index);
                        }
                        catch (NullReferenceException)
                        {
                            return;
                        }
                    }
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select All"))
                {
                    SetConditionsAll(
                        conditions.FindPropertyRelative("_conditions"),
                        enumNames.Length,
                        true);
                }
                if (GUILayout.Button("Unselect All"))
                {
                    SetConditionsAll(
                        conditions.FindPropertyRelative("_conditions"),
                        enumNames.Length,
                        false);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
    private void ToggleUpdate(SerializedProperty arrayProperty, int index)
    {
        arrayProperty.GetArrayElementAtIndex(index).boolValue =
            EditorGUILayout.Toggle(arrayProperty.GetArrayElementAtIndex(index).boolValue, _toggleStyle, _toggleGuiOptions);
    }
    private void SetConditionsAll(SerializedProperty arraySerializedProperty, int length, bool value)
    {
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < length; j++)
            {
                int index = i * length + j;
                arraySerializedProperty.GetArrayElementAtIndex(index).boolValue = value;
            }
        }
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