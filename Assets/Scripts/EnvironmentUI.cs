using UnityEngine;
using TMPro;
public class EnvironmentUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI generationText;

    public void UpdateText(int generation, float mean)
    {
        generationText.text = "Generation: " + generation + "\nMean Fitness: " + mean.ToString("F2");
    }
}
