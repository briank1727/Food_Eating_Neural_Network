using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Environment : MonoBehaviour
{
    #region Singleton
    public static Environment instance;

    void Singleton()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    [Header("Mutation Settings")]
    [Range(0.0f, 10.0f)][SerializeField] private float mutationAmount = 0.8f;
    [Range(0.0f, 1.0f)][SerializeField] private float mutationChance = 0.2f;
    [SerializeField] private int[] networkShape = new int[] { 16, 32, 32, 2 };
    [SerializeField] private int numBestCreatures = 5;
    [Header("Environment Settings")]
    [SerializeField] private int numCreatures = 10;
    [SerializeField] private int numFood = 100;
    [SerializeField] private float environmentRadius = 15.0f;
    [SerializeField] private float generationTime = 10.0f;
    [SerializeField] private GameObject ground;
    [Header("Food Settings")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Transform foodParent;
    [SerializeField] private float foodRadius = 0.5f;
    [Header("Creature Settings")]
    [SerializeField] private GameObject creaturePrefab;
    [SerializeField] private Transform creatureParent;
    [SerializeField] private float creatureRadius = 0.5f;
    [SerializeField] private float movementSpeed = 5.0f;
    [SerializeField] private float turningSpeed = 5.0f;
    [SerializeField] int numRays = 8;
    [SerializeField] float rayAngleSpread = 45.0f;
    [SerializeField] float maxRayDist = 5.0f;
    [Header("Animation Settings")]
    [SerializeField] private float animationTime = 0.2f;
    [Header("Debug Settings")]
    [SerializeField] private bool seeFoodDistances = false;
    [SerializeField] private bool seeWorldEdgeDistances = false;
    private List<Creature> creatures = new List<Creature>();
    private List<Creature> bestCreatures = new List<Creature>();
    private List<Food> foods = new List<Food>();
    private bool runFixedUpdate = false;
    private EnvironmentUI environmentUI;
    private bool startedCoroutine = false;
    private int generation = 0;
    void Awake()
    {
        Singleton();
    }

    void Start()
    {
        ground.transform.localScale = new Vector3(environmentRadius * 2, environmentRadius * 2, 0);
        ground.SetActive(true);

        environmentUI = GetComponent<EnvironmentUI>();
        environmentUI.UpdateText(0, 0.0f);
    }

    void FixedUpdate()
    {
        if (runFixedUpdate)
        {
            foreach (Creature creature in creatures)
            {
                float angleStep = rayAngleSpread / (numRays - 1);
                float startAngle = -rayAngleSpread / 2;
                float[] foodDistances = new float[numRays];
                float[] worldEdgeDistances = new float[numRays];

                for (int i = 0; i < numRays; i++)
                {
                    float angle = startAngle + i * angleStep;
                    Vector2 direction = Util.RotateVector(creature.GetDirection(), angle);

                    float minDist = maxRayDist;

                    for (int j = 0; j < foods.Count; j++)
                    {
                        if (creature.GetFoodEaten().Contains(j))
                            continue;
                        Food food = foods[j];
                        float dist = Util.RayIntersectDist(creature.GetPosition(), direction, maxRayDist, food.GetPosition(), foodRadius);
                        if (dist < minDist)
                            minDist = dist;
                    }

                    foodDistances[i] = minDist;
                    if (seeFoodDistances)
                        Debug.DrawRay(creature.GetPosition(), direction * minDist, Color.black);
                }

                for (int i = 0; i < numRays; i++)
                {
                    float angle = startAngle + i * angleStep;
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * creature.GetDirection();
                    worldEdgeDistances[i] = Util.GetRayDistanceToCircleEdge(creature.GetPosition(), direction, environmentRadius, maxRayDist);

                    if (seeWorldEdgeDistances)
                        Debug.DrawRay(creature.GetPosition(), direction * worldEdgeDistances[i], Color.red);
                }

                creature.Move(foodDistances, worldEdgeDistances);
            }

            for (int i = creatures.Count - 1; i >= 0; i--)
                if (creatures[i].GetPosition().magnitude > environmentRadius)
                    DeleteCreature(i);

            for (int i = 0; i < creatures.Count; i++)
            {
                Creature creature = creatures[i];
                for (int j = 0; j < foods.Count; j++)
                    if (!creature.GetFoodEaten().Contains(j) && Vector2.Distance(creature.GetPosition(), foods[j].GetPosition()) < foodRadius + creatureRadius)
                    {
                        creature.EatFood(j);
                        if (creature.IsRendered())
                            foods[j].DestroyGameObject();
                    }
            }
        }
    }

    IEnumerator Cycle()
    {
        while (true)
        {
            for (int i = 0; i < numCreatures; i++)
                if (bestCreatures.Count > 0)
                {
                    Creature randomCreature = bestCreatures[Random.Range(0, bestCreatures.Count)];
                    InstantiateCreature(randomCreature.GetLayersCopy(), (numFood - randomCreature.GetFoodEaten().Count) / (float)numFood, i == 0);
                }
                else
                    InstantiateCreature(createGameObject: i == 0);

            float mean = 0.0f;
            foreach (Creature bestCreature in bestCreatures)
                mean += bestCreature.GetFoodEaten().Count;
            mean /= bestCreatures.Count;
            environmentUI.UpdateText(++generation, mean);
            yield return new WaitForSeconds(animationTime);
            while (foods.Count > 0)
                DeleteFood(0);
            creaturePrefab.transform.localScale = new Vector3(creatureRadius * 2, creatureRadius * 2, 0);
            foodPrefab.transform.localScale = new Vector3(foodRadius * 2, foodRadius * 2, 0);

            for (int i = 0; i < numFood; i++)
                InstantiateFood();
            yield return new WaitForSeconds(animationTime);
            runFixedUpdate = true;
            yield return new WaitForSeconds(generationTime);
            runFixedUpdate = false;
            foreach (Creature bestCreature in bestCreatures)
                creatures.Add(bestCreature);
            creatures.Sort((a, b) => b.GetFoodEaten().Count.CompareTo(a.GetFoodEaten().Count));
            bestCreatures.Clear();
            for (int i = 0; i < Mathf.Min(numBestCreatures, creatures.Count); i++)
                bestCreatures.Add(creatures[i]);

            while (creatures.Count > 0)
                DeleteCreature(0);

        }
    }

    void InstantiateCreature(Layer[] layers = null, float mutationProportion = 1.0f, bool createGameObject = true)
    {
        Vector2 position;
        int attempts = 0;
        do
        {
            position = Random.insideUnitCircle * environmentRadius * 0.9f;
            attempts++;
        } while (creatures.Exists(c => Vector2.Distance(c.GetPosition(), position) < 1.0f) && attempts < 100);

        Vector2 direction = Random.insideUnitCircle.normalized;
        Creature creature = new Creature(networkShape, creaturePrefab, creatureParent, position, direction, turningSpeed, movementSpeed, layers, mutationAmount * mutationProportion, mutationChance, createGameObject);
        creatures.Add(creature);
    }

    void InstantiateFood()
    {
        Vector2 position;
        int attempts = 0;
        do
        {
            position = Random.insideUnitCircle * environmentRadius * 0.9f;
            attempts++;
        } while (foods.Exists(f => Vector2.Distance(f.GetPosition(), position) < 1.0f) && attempts < 100);

        Food food = new Food(position, foodPrefab, foodParent);
        foods.Add(food);
    }

    void DeleteCreature(int index)
    {
        creatures[index].DestroyGameObject();
        creatures.RemoveAt(index);
    }

    void DeleteFood(int index)
    {
        foods[index].DestroyGameObject();
        foods.RemoveAt(index);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, environmentRadius);
    }

    public float GetAnimationTime()
    {
        return animationTime;
    }

    public void SaveCurrentNetwork()
    {
        if (bestCreatures.Count > 0)
            FileManager.instance.SaveNeuralNetwork(bestCreatures[0].GetLayersCopy(), numRays * 2);
        else
            Debug.Log("No best creature to save");
    }

    public void LoadCurrentNetwork()
    {
        Layer[] loadedLayers = FileManager.instance.LoadNeuralNetwork();
        if (loadedLayers != null && loadedLayers.Length == networkShape.Length - 1)
        {
            for (int i = 0; i < loadedLayers.Length; i++)
            {
                if (loadedLayers[i].nodes.Length != networkShape[i + 1] || loadedLayers[i].weights.Length != networkShape[i] || loadedLayers[i].weights[0].Length != networkShape[i + 1] || loadedLayers[i].biases.Length != networkShape[i + 1])
                {
                    Debug.LogError("Loaded network shape does not match the defined network shape.");
                    Debug.Log(loadedLayers[i].nodes.Length);
                    Debug.Log(networkShape[i + 1]);
                    Debug.Log(loadedLayers[i].weights.Length);
                    Debug.Log(networkShape[i]);
                    Debug.Log(loadedLayers[i].weights[0].Length);
                    Debug.Log(networkShape[i + 1]);
                    Debug.Log(loadedLayers[i].biases.Length);
                    Debug.Log(networkShape[i + 1]);
                    return;
                }
            }
            Creature newCreature = new Creature(networkShape, creaturePrefab, creatureParent, Vector2.zero, Vector2.up, turningSpeed, movementSpeed, loadedLayers, 0.0f, 0.0f, false);
            bestCreatures.Add(newCreature);

            Debug.Log("Network loaded successfully.");
        }
        else
        {
            Debug.LogError("Loaded network shape does not match the defined network shape.");
        }
    }

    public void StartCycle()
    {
        if (!startedCoroutine)
        {
            StartCoroutine(Cycle());
            startedCoroutine = true;
        }
    }
}
