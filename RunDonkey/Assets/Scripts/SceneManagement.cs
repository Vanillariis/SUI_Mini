using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("IntroScene");
    }

    public void retryGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
