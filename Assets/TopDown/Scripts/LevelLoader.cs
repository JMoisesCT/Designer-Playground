using UnityEngine.SceneManagement;

namespace TopDown
{
    // Carga niveles. En el editor funciona aunque la escena no esté en Build Settings.
    public static class LevelLoader
    {
        public static void Load(string scenePath)
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(scenePath);
#endif
        }

        public static void Restart()
        {
            Load(SceneManager.GetActiveScene().path);
        }
    }
}
