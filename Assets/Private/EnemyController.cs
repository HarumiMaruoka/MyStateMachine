// 日本語対応
using System;
using UnityEngine;

[DefaultExecutionOrder(150)]
public class EnemyController : MonoBehaviour
{
    private MyStateMachine _stateMachine = null;

    private void Start()
    {
        _stateMachine = GetComponent<MyStateMachine>();

        //_stateMachine.Enter[EnemyState.Move] += () => { Debug.Log("移動開始！"); };
        //_stateMachine.Exit[EnemyState.Move] += () => { Debug.Log("移動終了.."); };
        //_stateMachine.StateMachineUpdate[EnemyState.Move] += () => { Debug.Log("移動中。。。ﾄｯﾄｺﾄｯﾄｺ"); };

        //_stateMachine.Enter[EnemyState.Jump] += () => { Debug.Log("ジャンプ開始！"); };
        //_stateMachine.Exit[EnemyState.Jump] += () => { Debug.Log("ジャンプ終了.."); };
        //_stateMachine.StateMachineUpdate[EnemyState.Jump] += () => { Debug.Log("ジャンプ中。。。ﾊﾟﾀﾊﾟﾀﾊﾟﾀ"); };
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            _stateMachine.AddState(EnemyState.Move);
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            _stateMachine.RemoveState(EnemyState.Move);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _stateMachine.AddState(EnemyState.Jump);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _stateMachine.RemoveState(EnemyState.Jump);
        }
    }

}
[Flags, Serializable]
public enum EnemyState
{
    Move =
        0b00000000_00000000_00000000_00000001,
    Jump =
        0b00000000_00000000_00000000_00000010,
    AAA =
        0b00000000_00000000_00000000_00000100,
    BBBB =
        0b00000000_00000000_00000000_00001000,
    CCCC =
        0b00000000_00000000_00000000_00010000,
    DD =
        0b00000000_00000000_00000000_00100000,
    EEEEE =
        0b00000000_00000000_00000000_01000000,
    FFFFFFFF =
        0b00000000_00000000_00000000_10000000,
    GGGGGGGGG =
        0b00000000_00000000_00000001_00000000,
    HHHHH =
        0b00000000_00000000_00000010_00000000,
    II =
        0b00000000_00000000_00000100_00000000,
}