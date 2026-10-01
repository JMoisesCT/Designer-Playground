using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Float Event Channel")]
public class FloatEventChannel : ScriptableObject
{
    public UnityAction<float> OnEventRaised;

    public void RaiseEvent(float floatParam)
    {
        OnEventRaised?.Invoke(floatParam);
    }
}
