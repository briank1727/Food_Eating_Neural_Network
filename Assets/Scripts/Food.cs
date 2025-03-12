using UnityEngine;

public class Food
{
    public Vector2 position;
    public GameObject gameObject;

    public Food(Vector2 position, GameObject foodPrefab, Transform foodParent)
    {
        this.position = position;
        gameObject = Object.Instantiate(foodPrefab, position, Quaternion.identity, foodParent);
    }

    public void DestroyGameObject()
    {
        Object.Destroy(gameObject);
    }

    public Vector2 GetPosition()
    {
        return position;
    }
}