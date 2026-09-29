using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Transform Event Channel")]
public class TransformEventChannel : ScriptableObject
{
    public UnityAction<Transform> OnEventRaised;

    public void RaiseEvent(Transform transformParam)
    {
        OnEventRaised?.Invoke(transformParam);
    }
}
