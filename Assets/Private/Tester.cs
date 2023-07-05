// 日本語対応
using UnityEngine;
using UnityEngine.UI;

public class Tester : MonoBehaviour
{
    [SerializeField, SerializeReference, EnumType]
    private System.Enum aaa;
    [SerializeField, SerializeReference, SubclassSelector]
    private a GetA;

    [SerializeField]
    private Text enumText;
    [SerializeField]
    private Text subClassText;


    private void OnValidate()
    {
        enumText.text = aaa.GetType().Name;
        subClassText.text = GetA.GetType().Name;
    }
}
public interface a { }

public class AAAAA : a
{

}
public class BBBBB : a
{

}
public class ACCCAAAA : a
{

}