using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    public PowerUpData data;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SnakeController>().ActivatePowerUp(data);
            Destroy(gameObject);
        }
    }
}
