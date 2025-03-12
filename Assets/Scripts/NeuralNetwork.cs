using UnityEngine;

public class NeuralNetwork
{
    Layer[] layers;

    public NeuralNetwork(int[] networkShape)
    {
        layers = new Layer[networkShape.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(networkShape[i], networkShape[i + 1]);
        }
    }

    public float[] Brain(float[] input)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].Forward(input);
            input = layers[i].nodes;
        }
        return input;
    }

    public void CopyLayers(Layer[] newLayers)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            for (int j = 0; j < layers[i].weights.Length; j++)
                for (int k = 0; k < layers[i].weights[j].Length; k++)
                    layers[i].weights[j][k] = newLayers[i].weights[j][k];

            for (int j = 0; j < layers[i].biases.Length; j++)
                layers[i].biases[j] = newLayers[i].biases[j];
        }
    }

    public Layer[] GetLayersCopy()
    {
        Layer[] newLayers = new Layer[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            newLayers[i] = new Layer(layers[i].weights.Length, layers[i].weights[0].Length);
            for (int j = 0; j < layers[i].weights.Length; j++)
                for (int k = 0; k < layers[i].weights[j].Length; k++)
                    newLayers[i].weights[j][k] = layers[i].weights[j][k];

            for (int j = 0; j < layers[i].biases.Length; j++)
                newLayers[i].biases[j] = layers[i].biases[j];
        }
        return newLayers;
    }

    public void Mutate(float mutationAmount, float mutationChance)
    {
        foreach (Layer layer in layers)
        {
            for (int i = 0; i < layer.weights.Length; i++)
                for (int j = 0; j < layer.weights[i].Length; j++)
                    if (Random.value < mutationChance)
                        layer.weights[i][j] += Random.Range(-mutationAmount, mutationAmount);

            for (int i = 0; i < layer.biases.Length; i++)
                if (Random.value < mutationChance)
                    layer.biases[i] += Random.Range(-mutationAmount, mutationAmount);
        }
    }

}

public class Layer
{
    public float[][] weights;
    public float[] biases;
    public float[] nodes;

    public Layer(int inputSize, int outputSize)
    {
        weights = new float[inputSize][];
        for (int i = 0; i < inputSize; i++)
            weights[i] = new float[outputSize];
        biases = new float[outputSize];
        nodes = new float[outputSize];
    }

    public Layer(float[][] weights, float[] biases)
    {
        this.weights = weights;
        this.biases = biases;
        nodes = new float[biases.Length];
    }

    public void Forward(float[] input)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i] = 0;
            for (int j = 0; j < input.Length; j++)
            {
                nodes[i] += weights[j][i] * input[j];
            }
            nodes[i] += biases[i];
            nodes[i] = Activation(nodes[i]);
        }
    }

    public float Activation(float x)
    {
        return (float)System.Math.Tanh(x);
    }
}