using UnityEngine;

[CreateAssetMenu(menuName = "MiJuegoProgramacion/SO Variables/Bool Variable", fileName = "NewBoolVariable")]
public class BoolVariable : SOVariable_Base<bool>
{
    public void Toggle()
    {
        Value = !Value;
    }

    public void SetTrue()
    {
        Value = true;
    }

    public void SetFalse()
    {
        Value = false;
    }
}