using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    // Pantalla del nivel: contador de frutas y cargas de dash (arriba a la izquierda) y mensaje central.
    public class LevelHud : MonoBehaviour
    {
        private static readonly Color UsedDashColor = new(1f, 1f, 1f, 0.25f);

        [Header("Referencias")]
        [SerializeField] private GameObject _fruitCounter;
        [SerializeField] private Text _fruitText;
        [SerializeField] private Text _messageText;

        [Tooltip("Primer icono de dash. Se copia tantas veces como cargas tenga el jugador.")]
        [SerializeField] private Image _dashIcon;

        private readonly List<Image> _dashIcons = new();
        private PlayerController _player;
        private int _collected;
        private int _total;

        private void Start()
        {
            // Si el nivel no tiene frutas, no se muestra el contador.
            _total = FindObjectsByType<Fruit>(FindObjectsSortMode.None).Length;
            _fruitCounter.SetActive(_total > 0);
            _messageText.text = "";
            _player = FindFirstObjectByType<PlayerController>();
            _dashIcons.Add(_dashIcon);
            Refresh();
        }

        private void Update()
        {
            int max = _player != null ? _player.MaxDashCharges : 0;
            int charges = _player != null ? _player.DashCharges : 0;
            while (_dashIcons.Count < max)
            {
                _dashIcons.Add(Instantiate(_dashIcon, _dashIcon.transform.parent));
            }

            // Iconos llenos = cargas que quedan; sin dash no se muestra ninguno.
            for (int i = 0; i < _dashIcons.Count; i++)
            {
                _dashIcons[i].gameObject.SetActive(i < max);
                _dashIcons[i].color = i < charges ?Color.white : UsedDashColor;
            }
        }

        public void AddFruit()
        {
            _collected++;
            Refresh();
        }

        public void ShowMessage(string message)
        {
            _messageText.text = message;
        }

        private void Refresh()
        {
            _fruitText.text = $"{_collected} / {_total}";
        }

        private void OnValidate()
        {
            if (_fruitCounter == null || _fruitText == null || _messageText == null || _dashIcon == null)
                Debug.LogWarning($"[{name}] {nameof(LevelHud)}: faltan referencias.", this);
        }
    }
}
