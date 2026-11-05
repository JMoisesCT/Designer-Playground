using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Shmup
{
    // Cámara que avanza sola hacia la derecha hasta llegar a la Meta. La nave del jugador viaja con ella.
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-100)] // se coloca antes de que la nave se enganche a ella
    public class ScrollCamera : MonoBehaviour
    {
        private const float PlayerOffset = 8f; // el jugador empieza 8 tiles a la izquierda del centro de la pantalla

        [Tooltip("Velocidad de avance del nivel, en tiles por segundo.")]
        [SerializeField, Range(0f, 8f)] private float _speed = 2f;

        private PixelPerfectCamera _pixelPerfect;
        private Vector3 _position;

        private void Start()
        {
            _pixelPerfect = GetComponent<PixelPerfectCamera>();
            _position = transform.position;
            _position.x = StartX();
            transform.position = _position;
        }

        private void Update()
        {
            _position.x += _speed * Time.deltaTime;
            Vector2 snapped = SnapToPixel(_position);
            transform.position = new Vector3(snapped.x, snapped.y, _position.z);
        }

        public void Stop()
        {
            enabled = false;
        }

        // Los sprites se dibujan en píxeles de pantalla enteros: redondear evita que la nave vibre un píxel al avanzar.
        public Vector2 SnapToPixel(Vector2 value)
        {
            return _pixelPerfect != null ? (Vector2)_pixelPerfect.RoundToPixel(value) : value;
        }

        // La cámara empieza donde esté el jugador: para probar el final del nivel, basta con arrastrarlo hasta allí.
        private float StartX()
        {
            GameObject player = GameObject.FindWithTag("Player");
            return player != null ? player.transform.position.x + PlayerOffset : transform.position.x;
        }

        // Rectángulo verde: la zona que recorrerá la cámara, desde el jugador hasta la Meta.
        private void OnDrawGizmos()
        {
            var view = GetComponent<Camera>();
            float halfHeight = view.orthographicSize;
            float left = StartX() - halfHeight * view.aspect;
            Goal goal = FindFirstObjectByType<Goal>();
            float right = goal != null ? goal.transform.position.x : left + 50f;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3((left + right) / 2f, transform.position.y, 0f),
                new Vector3(right - left, halfHeight * 2f, 0f));
        }
    }
}
