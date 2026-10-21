using UnityEngine;

public class DeathTrap : MonoBehaviour
{
    private bool triggered;
    void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("Cannot collect a false coin without a GameManager.", this);
                return;
            }

            if (gameManager.HasEnded)
            {
                return;
            }

            triggered = true;
            gameManager.CollectCoin(transform.position, isFalseCoin: true);
            Destroy(gameObject);
        }
    }
}
