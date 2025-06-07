using UnityEngine;

[CreateAssetMenu(menuName = "MiJuegoProgramacion/SO Variables/Int Variable", fileName = "NewIntVariable")]
public class IntVariable : SOVariable_Base<int>
{
    public void Add(int amount)
    {
        Value += amount;
    }

    public void Add(IntVariable amount)
    {
        Value += amount.Value;
    }
}