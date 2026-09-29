using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Bool Int Event Channel")]
public class BoolIntEventChannel : ScriptableObject
{
    public UnityAction<bool, int> OnEventRaised;

    public void RaiseEvent(bool boolParam, int intParam)
    {
        OnEventRaised?.Invoke(boolParam, intParam);
    }
}
