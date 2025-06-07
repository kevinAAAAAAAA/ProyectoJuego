using UnityEngine;

public abstract class SOVariable_Base<T> : ScriptableObject
{
    [Header("Valor en Editor (Para Reseteo o Default)")]
    [SerializeField]
    private T initialValue;

    [Header("Valor en Tiempo de Ejecución (No se guarda en el Asset)")]
    [SerializeField] 
    private T runtimeValue;

    public T Value
    {
        get { return runtimeValue; }
        set
        {
            if (!object.Equals(runtimeValue, value))
            {
                runtimeValue = value;
            }
        }
    }

    private void OnEnable()
    {
    }

    public void ResetToInitialValue()
    {
        runtimeValue = initialValue;
    }

    public void Set(SOVariable_Base<T> variable)
    {
        Value = variable.Value;
    }

    public override string ToString()
    {
        return runtimeValue != null ? runtimeValue.ToString() : "null";
    }
}