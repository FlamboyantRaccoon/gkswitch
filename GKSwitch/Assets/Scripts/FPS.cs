using UnityEngine;
using TMPro;

public sealed class FPS : MonoBehaviour
{
    public string formatedString = "{value} FPS";
    public TMP_Text txtFps;

    public float updateRateSeconds = 4.0F;

    int frameCount = 0;
    float dt = 0.0F;
    float fps = 0.0F;

    void Update()
    {
        frameCount++;
        dt += Time.unscaledDeltaTime;
        if (dt > 1.0 / updateRateSeconds)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0F / updateRateSeconds;
        }
        txtFps.text = formatedString.Replace("{value}", System.Math.Round(fps, 1).ToString("0.0"));
    }


}
