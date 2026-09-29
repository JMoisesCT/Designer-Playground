using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Bool Event Channel")]
public class BoolEventChannel : ScriptableObject
{
    public UnityAction<bool> OnEventRaised;

    public void RaiseEvent(bool boolParam)
    {
        OnEventRaised?.Invoke(boolParam);
    }
}
