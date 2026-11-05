using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shmup
{
    // Pantalla del nivel: vidas (arriba a la izquierda), puntos (arriba a la derecha) y mensaje central.
    public class LevelHud : MonoBehaviour
    {
        private static readonly Color LostLife = new(0f, 0f, 0f, 0.35f);

        [Header("Referencias")]
        [Tooltip("Primer icono de vida. Se copia tantas veces como vidas tenga el jugador.")]
        [SerializeField] private Image _life;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _messageText;

        private readonly List<Image> _lives = new();
        private int _score;

        private void Start()
        {
            _messageText.text = "";
            RefreshScore();
        }

        public void ShowHealth(int health, int maxHealth)
        {
            if (_lives.Count == 0) _lives.Add(_life);
            while (_lives.Count < maxHealth)
            {
                _lives.Add(Instantiate(_life, _life.transform.parent));
            }

            for (int i = 0; i < _lives.Count; i++)
            {
                _lives[i].color = i < health ? Color.white : LostLife;
            }
        }

        public void AddScore(int points)
        {
            _score += points;
            RefreshScore();
        }

        public void ShowMessage(string message)
        {
            _messageText.text = message;
        }

        private void RefreshScore()
        {
            _scoreText.text = _score.ToString("000000");
        }

        private void OnValidate()
        {
            if (_life == null || _scoreText == null || _messageText == null)
                Debug.LogWarning($"[{name}] {nameof(LevelHud)}: faltan referencias.", this);
        }
    }
}
