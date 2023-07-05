// 日本語対応
using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-150)] // 初期化を早めにしてほしいので
public class MyStateMachine : MonoBehaviour
{
    [SerializeField, SerializeReference, EnumType]
    private Enum _currentState;
    [SerializeField, SerializeReference]
    private MyStateMachineCondition _conditions;

    private Enum[] _enumValues = null;
    private Dictionary<Enum, Action> _enter = new Dictionary<Enum, Action>();
    private Dictionary<Enum, Action> _update = new Dictionary<Enum, Action>();
    private Dictionary<Enum, Action> _exit = new Dictionary<Enum, Action>();

    public Enum[] EnumValues => _enumValues;
    public Dictionary<Enum, Action> Enter => _enter;
    public Dictionary<Enum, Action> StateMachineUpdate => _update;
    public Dictionary<Enum, Action> Exit => _exit;
    public Enum CurrentState => _currentState;
    public MyStateMachineCondition Conditions { get => _conditions; set => _conditions = value; }

    public void Setup()
    {
        if (_conditions == null) _conditions = new MyStateMachineCondition();

        _enter.Clear();
        _update.Clear();
        _exit.Clear();

        var array = Enum.GetValues(_currentState.GetType());
        _enumValues = new Enum[array.Length];
        int counter = 0;
        foreach (var e in array)
        {
            _enumValues[counter] = (Enum)e;
            counter++;
        }

        for (int i = 0; i < _enumValues.Length; i++)
        {
            _enter.TryAdd(_enumValues[i], default);
            _update.TryAdd(_enumValues[i], default);
            _exit.TryAdd(_enumValues[i], default);
        }
    }
    public void Awake()
    {
        Setup();
    }
    /// <summary>
    /// 毎フレーム実行してください
    /// </summary>
    private void Update()
    {
        foreach (var e in _update)
        {
            try
            {
                if (_currentState.HasFlag(e.Key))
                {
                    e.Value?.Invoke();
                }
            }
            catch (ArgumentException)
            {
                Debug.LogWarning("型が変更されました。");
                return;
            }
        }
    }
    public void AddState(Enum state)
    {
        if (_currentState.HasFlag(state)) // 既に追加されていたら実行しない
        {
            return;
        }

        _conditions.Draw();
        // インスペクタから設定した値によって、無視する。
        for (int i = 0; i < _enumValues.Length; i++)
        {
            var leftIndex = _conditions.GetIndex(_enumValues[i]);
            var topIndex = _conditions.GetIndex(state);
            if (_currentState.HasFlag(_enumValues[i]) && !_conditions.GetValue(leftIndex, topIndex))
            {
                Debug.LogError($"”{_enumValues[i]}”が有効な時は”{state}”に遷移できません。");
                return;
            }
        }

        _enter[state]?.Invoke();
        Int64 currentValue = Convert.ToInt64(_currentState);
        Int64 addValue = Convert.ToInt64(state);
        Int64 result = currentValue | addValue;
        _currentState = (Enum)Enum.ToObject(_currentState.GetType(), result);
    }
    public void RemoveState(Enum state)
    {
        if (!_currentState.HasFlag(state)) // 既に削除されていたら実行しない
        {
            return;
        }

        _exit[state]?.Invoke();
        Int64 currentValue = Convert.ToInt64(_currentState);
        Int64 removeValue = Convert.ToInt64(state);
        Int64 result = currentValue & ~removeValue;
        _currentState = (Enum)Enum.ToObject(_currentState.GetType(), result);
    }
}

[Serializable]
public class MyStateMachineCondition
{
    [SerializeField, SerializeReference]
    private bool[] _conditions;
    [SerializeField]
    private int _size;

    public bool[] Conditions => _conditions;

    public int Size { get => _size; }

    public void Setup(Type type)
    {
        var values = Enum.GetValues(type);
        _conditions = new bool[values.Length * values.Length];
        _size = values.Length;
    }
    public bool GetValue(int outIndex, int inIndex)
    {
        int index = outIndex * _size + inIndex;
        if (index >= 0 && index < _conditions.Length)
        {
            return _conditions[index];
        }
        else
        {
            // エラー処理など、適切な対応を行う
            Debug.LogError($"範囲外が指定されました \n" +
                $"index :{index}, outIndex :{outIndex}, inIndex :{inIndex}");
            return false; // または適切なデフォルト値を返すなど
        }
    }
    public void SetValue(int outIndex, int inIndex, bool value)
    {
        int index = outIndex * Size + inIndex;
        if (index >= 0 && index < _conditions.Length)
        {
            _conditions[index] = value;
        }
        else
        {
            // エラー処理など、適切な対応を行う
            Debug.LogError($"範囲外が指定されました \n" +
                $"index :{index}, outIndex :{outIndex}, inIndex :{inIndex}");
        }
    }
    public int GetIndex(Enum target)
    {
        var values = Enum.GetValues(target.GetType());
        return Array.IndexOf(values, target);
    }

    public void Draw()
    {
        for (int i = 0; i < _size; i++)
        {
            string line = "";
            for (int j = 0; j < _size; j++)
            {
                line += $"{GetValue(i, j)}, ";
            }
            //Debug.Log(line);
        }
    }
}