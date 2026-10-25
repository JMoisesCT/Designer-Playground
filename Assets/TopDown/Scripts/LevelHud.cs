using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TopDown
{
    // Pantalla del nivel: corazones y contador de monedas (arriba a la izquierda) y mensaje central.
    public class LevelHud : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Primer corazón. Se copia tantas veces como corazones tenga el jugador.")]
        [SerializeField] private Image _heart;
        [SerializeField] private Sprite _fullHeart;
        [SerializeField] private Sprite _emptyHeart;
        [SerializeField] private GameObject _coinCounter;
        [SerializeField] private Text _coinText;
        [SerializeField] private Text _messageText;

        private readonly List<Image> _hearts = new();
        private int _collected;
        private int _total;

        private void Start()
        {
            // Si el nivel no tiene monedas, no se muestra el contador.
            _total = FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;
            _coinCounter.SetActive(_total > 0);
            _messageText.text = "";
            RefreshCoins();
        }

        public void ShowHealth(int health, int maxHealth)
        {
            if (_hearts.Count == 0) _hearts.Add(_heart);
            while (_hearts.Count < maxHealth)
            {
                _hearts.Add(Instantiate(_heart, _heart.transform.parent));
            }

            for (int i = 0; i < _hearts.Count; i++)
            {
                _hearts[i].gameObject.SetActive(i < maxHealth);
                _hearts[i].sprite = i < health ? _fullHeart : _emptyHeart;
            }
        }

        public void AddCoin()
        {
            _collected++;
            RefreshCoins();
        }

        public void ShowMessage(string message)
        {
            _messageText.text = message;
        }

        private void RefreshCoins()
        {
            _coinText.text = $"{_collected} / {_total}";
        }

        private void OnValidate()
        {
            if (_heart == null || _fullHeart == null || _emptyHeart == null ||
                _coinCounter == null || _coinText == null || _messageText == null)
                Debug.LogWarning($"[{name}] {nameof(LevelHud)}: faltan referencias.", this);
        }
    }
}
