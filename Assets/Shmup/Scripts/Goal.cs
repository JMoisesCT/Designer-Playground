using System;
using System.Collections;
using UnityEngine;

namespace Shmup
{
    // Final del nivel: la cámara se para al llegar aquí y, cuando no quedan enemigos en pantalla, el nivel se completa.
    public class Goal : MonoBehaviour
    {
        private const float StopDistance = 1f; // la cámara se para justo antes: la Meta nunca llega a verse

        [Tooltip("Nivel que se carga al terminar. Arrastra aquí una escena. Si lo dejas vacío, se repite este nivel.")]
        [SerializeField, ScenePath] private string _nextLevel;

        [Tooltip("Mensaje que aparece en pantalla al terminar.")]
        [SerializeField] private string _message = "¡Nivel completado!";

        [Tooltip("Segundos que se ve el mensaje antes de cambiar de nivel.")]
        [SerializeField, Range(0f, 5f)] private float _delay = 3f;

        [Tooltip("Música al terminar. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _sound;

        private ScrollCamera _camera;
        private PlayerHealth _player;
        private bool _finished;

        private void Start()
        {
            _camera = FindFirstObjectByType<ScrollCamera>();
            _player = FindFirstObjectByType<PlayerHealth>();
        }

        private void Update()
        {
            if (_finished || CameraView.RightEdge < transform.position.x - StopDistance) return;

            _camera.Stop();
            if (_player.IsDead || EnemiesOnScreen()) return;

            _finished = true;
            _player.Protect();
            Sound.Play(_sound);

            LevelHud hud = FindFirstObjectByType<LevelHud>();
            if (hud != null) hud.ShowMessage(_message);

            StartCoroutine(LoadNextLevel());
        }

        private static bool EnemiesOnScreen()
        {
            return Array.Exists(FindObjectsByType<Enemy>(FindObjectsSortMode.None), enemy => enemy.IsActive);
        }

        private IEnumerator LoadNextLevel()
        {
            yield return new WaitForSeconds(_delay);
            if (string.IsNullOrEmpty(_nextLevel)) LevelLoader.Restart();
            else LevelLoader.Load(_nextLevel);
        }

        // Línea azul: donde termina el nivel.
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up * 8f, transform.position + Vector3.down * 8f);
        }
    }
}
