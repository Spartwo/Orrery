using UnityEngine;

namespace Universe
{
    public class Exit : MonoBehaviour
    {
        /// <summary>
        /// Closes the application. In the Unity Editor, it stops Play Mode.
        /// </summary>
        public void Quit()
        {
#if UNITY_EDITOR
            // If running in the editor, stop play mode
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running in a built application, quit the application
        Application.Quit();
#endif
        }
    }
}