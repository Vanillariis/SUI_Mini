using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public GameObject[] canvases;
    private int currentIndex = 0;

    void Start()
    {
        ShowCanvas(0);
    }

    public void NextCanvas()
    {
        if (currentIndex < canvases.Length - 1)
        {
            currentIndex++;
            ShowCanvas(currentIndex);
        }
    }

    public void PreviousCanvas()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowCanvas(currentIndex);
        }
    }

    void ShowCanvas(int index)
    {
        for (int i = 0; i < canvases.Length; i++)
        {
            canvases[i].SetActive(i == index);
        }
    }
}