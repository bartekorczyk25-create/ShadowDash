using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowDash.Core
{
    public sealed class BootSceneLoader : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = "Assets/Scenes/MainGameplay.unity";

        private void Start()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
