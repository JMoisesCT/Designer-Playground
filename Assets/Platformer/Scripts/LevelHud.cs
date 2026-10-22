using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    // Pantalla del nivel: contador de frutas (arriba a la izquierda) y mensaje central.
    public class LevelHud : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private GameObject _fruitCounter;
        [SerializeField] private Text _fruitText;
        [SerializeField] private Text _messageText;

        private int _collected;
        private int _total;

        private void Start()
        {
            // Si el nivel no tiene frutas, no se muestra el contador.
            _total = FindObjectsByType<Fruit>(FindObjectsSortMode.None).Length;
            _fruitCounter.SetActive(_total > 0);
            _messageText.text = "";
            Refresh();
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
            if (_fruitCounter == null || _fruitText == null || _messageText == null)
                Debug.LogWarning($"[{name}] {nameof(LevelHud)}: faltan referencias.", this);
        }
    }
}
