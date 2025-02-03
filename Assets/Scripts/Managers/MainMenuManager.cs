using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip _hoverAudioClip;

    public void StartGame()
    {
        SceneManager.LoadScene("Level");
    }

    public void Options()
    {
        SceneManager.LoadScene("Options");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void PlayHoverSound()
    {
        SoundFXManager.Instance.PlaySoundFXClip(_hoverAudioClip, transform, 1f);
    }
}
