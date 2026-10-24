using UnityEngine;

namespace Platformer
{
    // Reproduce un efecto de sonido. Usa un objeto temporal para que el sonido no se corte si quien lo lanza desaparece.
    public static class Sound
    {
        public static void Play(AudioClip clip)
        {
            if (clip == null) return;

            var source = new GameObject("Sonido " + clip.name).AddComponent<AudioSource>();
            source.PlayOneShot(clip);
            Object.Destroy(source.gameObject, clip.length);
        }
    }
}
