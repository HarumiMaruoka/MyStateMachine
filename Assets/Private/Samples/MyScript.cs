// 日本語対応
using UnityEngine;

public class MyScript : MonoBehaviour
{
    [SerializeField]
    Test myClassProperty;
}
[System.Serializable]
public class Test
{
    [SerializeField]
    int a;
}