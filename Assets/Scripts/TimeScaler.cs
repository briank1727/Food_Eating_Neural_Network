using UnityEngine;

public class TimeScaler : MonoBehaviour
{
    [Range(0.0f, 100.0f)][SerializeField] private float timeScale = 1.0f;
    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
    }
}
