using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public void retryGame()
    {
        SceneManager.LoadScene("BasicScene");
    }
}
