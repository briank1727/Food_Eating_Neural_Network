using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileManager : MonoBehaviour
{
    public static FileManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void SaveNeuralNetwork(Layer[] layers, int inputSize)
    {
        string path = Application.persistentDataPath + "/neural_network.txt";
        System.IO.File.WriteAllText(path, "Input size:\n" + inputSize + "\n");

        for (int i = 0; i < layers.Length; i++)
        {
            System.IO.File.AppendAllText(path, "Layer size:\n" + layers[i].biases.Length + "\n");
            System.IO.File.AppendAllText(path, "Weights:\n");
            for (int j = 0; j < layers[i].weights.Length; j++)
            {
                System.IO.File.AppendAllText(path, string.Join(",", Array.ConvertAll(layers[i].weights[j], x => x.ToString())) + "\n");
            }
            System.IO.File.AppendAllText(path, "Biases:\n" + string.Join(",", Array.ConvertAll(layers[i].biases, x => x.ToString())) + "\n");
        }
        Debug.Log("Neural network saved to " + path);
    }

    public Layer[] LoadNeuralNetwork()
    {
        string path = Application.persistentDataPath + "/neural_network.txt";
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("No saved neural network found at " + path);
            return null;
        }

        string[] lines = System.IO.File.ReadAllLines(path);
        int index = 1; // Skip the inputs line

        int inputSize = int.Parse(lines[index].Trim());
        index++;
        List<Layer> layers = new List<Layer>();

        while (index < lines.Length)
        {
            index++; // Skip the layer size line
            int layerSize = int.Parse(lines[index].Trim());
            index++;
            index++; // Skip "Weights:" line

            float[][] weights = new float[inputSize][];
            for (int i = 0; i < inputSize; i++)
            {
                weights[i] = Array.ConvertAll(lines[index].Split(','), float.Parse);
                index++;
            }

            inputSize = layerSize;

            index++; // Skip "Biases:" line
            float[] biases = Array.ConvertAll(lines[index].Split(','), float.Parse);
            index++;

            layers.Add(new Layer(weights, biases));
        }

        Debug.Log("Neural network loaded from " + path);

        return layers.ToArray();
    }
}
