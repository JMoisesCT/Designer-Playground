using UnityEngine;

// Aspecto de un personaje jugable: su Animator Override Controller y el sprite que se ve en el editor.
[CreateAssetMenu(menuName = "Designer Playground/Jugador/Character Skin", fileName = "CharacterSkin")]
public class CharacterSkinSO : ScriptableObject
{
    [Tooltip("Nombre que aparece en los desplegables.")]
    [SerializeField] private string _displayName;

    [Tooltip("Override Controller del personaje (basado en Player.controller).")]
    [SerializeField] private RuntimeAnimatorController _animatorController;

    [Tooltip("Sprite que se muestra en la escena sin dar a Play (primer frame de Idle).")]
    [SerializeField] private Sprite _previewSprite;

    public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;
    public RuntimeAnimatorController AnimatorController => _animatorController;
    public Sprite PreviewSprite => _previewSprite;

    private void OnValidate()
    {
        if (_animatorController == null) Debug.LogWarning($"[{name}] {nameof(CharacterSkinSO)} sin Animator Controller.", this);
    }
}
