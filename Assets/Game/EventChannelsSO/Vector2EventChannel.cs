using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Vector2 Event Channel")]
public class Vector2EventChannel : ScriptableObject
{
    public UnityAction<Vector2> OnEventRaised;

    public void RaiseEvent(Vector2 vector2Param)
    {
        OnEventRaised?.Invoke(vector2Param);
    }
}
