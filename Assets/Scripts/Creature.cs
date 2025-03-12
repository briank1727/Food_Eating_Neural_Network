using UnityEngine;
using System.Collections.Generic;

public class Creature
{
    private NeuralNetwork neuralNetwork;
    private GameObject gameObject;
    private Vector2 position;
    private Vector2 direction;
    private float turningSpeed;
    private float movementSpeed;
    private HashSet<int> foodPickedUp;

    public Creature(int[] networkShape, GameObject creaturePrefab, Transform creatureParent, Vector2 position, Vector2 direction, float turningSpeed, float movementSpeed, Layer[] layers = null, float mutationAmount = 0.0f, float mutationChance = 0.0f, bool createGameObject = true)
    {
        this.neuralNetwork = new NeuralNetwork(networkShape);
        if (createGameObject)
            this.gameObject = GameObject.Instantiate(creaturePrefab, (Vector3)position, Quaternion.LookRotation(Vector3.forward, direction), creatureParent);
        this.position = position;
        this.direction = direction;
        this.turningSpeed = turningSpeed;
        this.movementSpeed = movementSpeed;
        this.foodPickedUp = new HashSet<int>();
        if (layers != null)
            neuralNetwork.CopyLayers(layers);
        neuralNetwork.Mutate(mutationAmount, mutationChance);
    }

    public void Move(float[] foodDistances, float[] worldEdgeDistances)
    {
        float[] input = new float[foodDistances.Length + worldEdgeDistances.Length];
        foodDistances.CopyTo(input, 0);
        worldEdgeDistances.CopyTo(input, foodDistances.Length);
        float[] output = neuralNetwork.Brain(input);
        float LR = output[0];

        direction = Util.RotateVector(direction, LR * turningSpeed * Time.fixedDeltaTime);
        position += movementSpeed * direction * Time.fixedDeltaTime;
        if (gameObject != null)
        {
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }
    }

    public HashSet<int> GetFoodEaten()
    {
        return foodPickedUp;
    }

    public void EatFood(int index)
    {
        foodPickedUp.Add(index);
    }

    public void MoveForward()
    {
        position += movementSpeed * direction * Time.fixedDeltaTime;
        if (gameObject != null)
            gameObject.transform.position = position;
    }

    public Vector2 GetPosition()
    {
        return position;
    }

    public Vector2 GetDirection()
    {
        return direction;
    }

    public void TurnLeft()
    {
        direction = Util.RotateVector(direction, turningSpeed * Time.fixedDeltaTime);
        if (gameObject != null)
            gameObject.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    public void TurnRight()
    {
        direction = Util.RotateVector(direction, -turningSpeed * Time.fixedDeltaTime);
        if (gameObject != null)
            gameObject.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    public void DestroyGameObject()
    {
        if (gameObject != null)
            GameObject.Destroy(gameObject);
    }

    public Layer[] GetLayersCopy()
    {
        return neuralNetwork.GetLayersCopy();
    }

    public bool IsRendered()
    {
        return gameObject != null;
    }
}