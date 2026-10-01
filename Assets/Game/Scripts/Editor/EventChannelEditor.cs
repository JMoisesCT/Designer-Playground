using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// Al crear un tipo de canal nuevo, añadir aquí su editor de una línea.
[CustomEditor(typeof(VoidEventChannel))] public class VoidEventChannelEditor : EventChannelEditor { }
[CustomEditor(typeof(BoolEventChannel))] public class BoolEventChannelEditor : EventChannelEditor { }
[CustomEditor(typeof(IntEventChannel))] public class IntEventChannelEditor : EventChannelEditor { }
[CustomEditor(typeof(FloatEventChannel))] public class FloatEventChannelEditor : EventChannelEditor { }
[CustomEditor(typeof(Vector2EventChannel))] public class Vector2EventChannelEditor : EventChannelEditor { }
[CustomEditor(typeof(TransformEventChannel))] public class TransformEventChannelEditor : EventChannelEditor { }
[CustomEditor(typeof(BoolIntEventChannel))] public class BoolIntEventChannelEditor : EventChannelEditor { }

// Inspector de los canales: botón para lanzarlos en Play Mode y lista de quién los escucha.
public abstract class EventChannelEditor : Editor
{
    private MethodInfo _raiseMethod;
    private FieldInfo _eventField;
    private ParameterInfo[] _parameters;
    private object[] _arguments;

    private void OnEnable()
    {
        Type channelType = target.GetType();
        _raiseMethod = channelType.GetMethod("RaiseEvent");
        _eventField = channelType.GetField("OnEventRaised");
        _parameters = _raiseMethod != null ? _raiseMethod.GetParameters() : Array.Empty<ParameterInfo>();
        _arguments = new object[_parameters.Length];

        for (int i = 0; i < _parameters.Length; i++)
        {
            Type parameterType = _parameters[i].ParameterType;
            _arguments[i] = parameterType.IsValueType ? Activator.CreateInstance(parameterType) : null;
        }
    }

    public override bool RequiresConstantRepaint() => Application.isPlaying;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        DrawRaiseSection();
        EditorGUILayout.Space();
        DrawListenersSection();
    }

    private void DrawRaiseSection()
    {
        EditorGUILayout.LabelField("Probar evento", EditorStyles.boldLabel);

        if (_raiseMethod == null)
        {
            EditorGUILayout.HelpBox("Este canal no tiene un método RaiseEvent.", MessageType.Warning);
            return;
        }

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            for (int i = 0; i < _parameters.Length; i++)
            {
                _arguments[i] = DrawArgumentField(ObjectNames.NicifyVariableName(_parameters[i].Name),
                                                  _parameters[i].ParameterType, _arguments[i]);
            }

            if (GUILayout.Button("Raise"))
            {
                _raiseMethod.Invoke(target, _arguments);
            }
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Entra en Play Mode para lanzar el evento.", MessageType.Info);
        }
    }

    private void DrawListenersSection()
    {
        Delegate eventDelegate = _eventField != null ? _eventField.GetValue(target) as Delegate : null;
        Delegate[] listeners = eventDelegate != null ? eventDelegate.GetInvocationList() : Array.Empty<Delegate>();

        EditorGUILayout.LabelField($"Escuchando ahora ({listeners.Length})", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.LabelField("Solo se ve en Play Mode.", EditorStyles.miniLabel);
            return;
        }

        foreach (Delegate listener in listeners)
        {
            EditorGUILayout.BeginHorizontal();
            if (listener.Target is Object unityObject)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(unityObject, typeof(Object), true);
                }
            }
            else
            {
                EditorGUILayout.LabelField(listener.Target != null ? listener.Target.GetType().Name : "(estático)");
            }
            EditorGUILayout.LabelField(listener.Method.Name, GUILayout.Width(160));
            EditorGUILayout.EndHorizontal();
        }
    }

    private static object DrawArgumentField(string label, Type type, object value)
    {
        if (type == typeof(bool)) return EditorGUILayout.Toggle(label, (bool)value);
        if (type == typeof(int)) return EditorGUILayout.IntField(label, (int)value);
        if (type == typeof(float)) return EditorGUILayout.FloatField(label, (float)value);
        if (type == typeof(string)) return EditorGUILayout.TextField(label, (string)value);
        if (type == typeof(Vector2)) return EditorGUILayout.Vector2Field(label, (Vector2)value);
        if (type == typeof(Vector3)) return EditorGUILayout.Vector3Field(label, (Vector3)value);
        if (typeof(Object).IsAssignableFrom(type)) return EditorGUILayout.ObjectField(label, (Object)value, type, true);

        EditorGUILayout.LabelField(label, $"Tipo {type.Name} no soportado");
        return value;
    }
}
