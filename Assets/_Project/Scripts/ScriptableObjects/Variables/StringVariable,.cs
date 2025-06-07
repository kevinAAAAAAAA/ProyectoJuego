using UnityEngine;

[CreateAssetMenu(menuName = "MiJuegoProgramacion/SO Variables/String Variable", fileName = "NewStringVariable")]
public class StringVariable : SOVariable_Base<string>
{

    public void Clear()
    {
        Value = string.Empty;
    }
}