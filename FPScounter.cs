using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsCounter;

    private void Update()
    {
        float fps = 1 / Time.deltaTime;
        fpsCounter.text = "FPS: " + Mathf.Round(fps);
    }
}