using UnityEngine;

public class FoodPickup : MonoBehaviour
{
    public FoodData data;

    private void OnTriggerEnter(Collider other)
    {
        SnakeController snake = other.GetComponent<SnakeController>();
        if (snake != null)
        {
            snake.EatFood(this);
        }
    }
}
