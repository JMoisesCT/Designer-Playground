using UnityEngine;
using UnityEngine.InputSystem;

namespace Shmup
{
    // Nave del jugador: se mueve por la pantalla mientras la cámara avanza y dispara mientras se mantiene pulsado el botón.
    public class PlayerController : MonoBehaviour
    {
        private const int MaxWeaponLevel = 3;
        private const float ScreenMargin = 1f;    // distancia mínima al borde de la pantalla
        private const float NoseDistance = 0.75f; // las balas salen del morro
        private const float DoubleShotGap = 0.3f;
        private const float SpreadAngle = 12f;
        private const float ShotVolume = 0.35f;

        [Header("Movimiento")]
        [Tooltip("Velocidad de la nave, en tiles por segundo.")]
        [SerializeField, Range(1f, 20f)] private float _moveSpeed = 9f;

        [Header("Disparo")]
        [Tooltip("Disparos por segundo.")]
        [SerializeField, Range(1f, 20f)] private float _fireRate = 6f;

        [Tooltip("Arma inicial: 1 = una bala, 2 = doble, 3 = triple en abanico. Cada Mejora la sube un nivel.")]
        [SerializeField, Range(1, MaxWeaponLevel)] private int _weaponLevel = 1;

        [Tooltip("Sonido de cada disparo. Déjalo vacío para que no suene.")]
        [SerializeField] private AudioClip _shotSound;

        [Header("Referencias")]
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _fireAction;

        private ScrollCamera _camera;
        private Vector2 _viewPosition; // posición dentro de la pantalla, respecto a su centro
        private float _nextShotTime;

        private void OnEnable()
        {
            _moveAction.action.Enable();
            _fireAction.action.Enable();
        }

        // La nave se hace hija de la cámara: así avanza con ella sin hacer nada más.
        private void Start()
        {
            _camera = FindFirstObjectByType<ScrollCamera>();
            transform.SetParent(_camera.transform, true);
            _viewPosition = transform.localPosition;
        }

        private void Update()
        {
            Vector2 input = Vector2.ClampMagnitude(_moveAction.action.ReadValue<Vector2>(), 1f);
            _viewPosition += input * (_moveSpeed * Time.deltaTime);

            float maxX = CameraView.HalfWidth - ScreenMargin;
            float maxY = CameraView.HalfHeight - ScreenMargin;
            _viewPosition.x = Mathf.Clamp(_viewPosition.x, -maxX, maxX);
            _viewPosition.y = Mathf.Clamp(_viewPosition.y, -maxY, maxY);

            Vector2 snapped = _camera.SnapToPixel(_viewPosition);
            transform.localPosition = new Vector3(snapped.x, snapped.y, transform.localPosition.z);

            if (_fireAction.action.IsPressed() && Time.time >= _nextShotTime)
            {
                _nextShotTime = Time.time + 1f / _fireRate;
                Fire();
            }
        }

        public void UpgradeWeapon()
        {
            _weaponLevel = Mathf.Min(_weaponLevel + 1, MaxWeaponLevel);
        }

        private void Fire()
        {
            Sound.Play(_shotSound, ShotVolume);
            switch (_weaponLevel)
            {
                case 1:
                    Shoot(0f, 0f);
                    break;
                case 2:
                    Shoot(-DoubleShotGap, 0f);
                    Shoot(DoubleShotGap, 0f);
                    break;
                default:
                    Shoot(0f, 0f);
                    Shoot(0f, SpreadAngle);
                    Shoot(0f, -SpreadAngle);
                    break;
            }
        }

        private void Shoot(float sideOffset, float angle)
        {
            Vector3 position = transform.position + transform.up * NoseDistance + transform.right * sideOffset;
            Instantiate(_bulletPrefab, position, transform.rotation * Quaternion.Euler(0f, 0f, angle));
        }

        private void OnValidate()
        {
            if (_bulletPrefab == null || _moveAction == null || _fireAction == null)
                Debug.LogWarning($"[{name}] {nameof(PlayerController)}: faltan referencias.", this);
        }
    }
}
