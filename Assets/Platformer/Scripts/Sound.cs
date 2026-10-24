using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Platformer
{
    // Reproduce un efecto de sonido. Usa un objeto temporal para que el sonido no se corte si quien lo lanza desaparece.
    public static class Sound
    {
        private const float SilenceLevel = 0.02f; // volumen por debajo del cual se considera silencio

        // Momento en que empieza a sonar cada clip: muchos sonidos de Kenney traen ~0,1 s de silencio al principio
        // y, si no se salta, el sonido llega tarde (el salto suena cuando el personaje ya está en el aire).
        private static readonly Dictionary<AudioClip, float> StartTimes = new();

        public static void Play(AudioClip clip)
        {
            if (clip == null) return;

            var source = new GameObject("Sonido " + clip.name).AddComponent<AudioSource>();
            source.clip = clip;
            source.time = GetStartTime(clip);
            source.Play();
            Object.Destroy(source.gameObject, clip.length);
        }

        private static float GetStartTime(AudioClip clip)
        {
            if (StartTimes.TryGetValue(clip, out float time)) return time;

            // Solo se pueden leer las muestras de los clips con Load Type "Decompress On Load" (los de Kenney lo son).
            time = 0f;
            if (clip.loadType == AudioClipLoadType.DecompressOnLoad && clip.LoadAudioData())
            {
                var samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);
                int first = Array.FindIndex(samples, sample => Mathf.Abs(sample) > SilenceLevel);
                if (first > 0) time = (float)(first / clip.channels) / clip.frequency;
            }
            StartTimes[clip] = time;
            return time;
        }
    }
}
