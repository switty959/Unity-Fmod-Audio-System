using UnityEngine;

public class pickUp : MonoBehaviour
{
    private bool collected;

    void Update()
    {
        transform.Rotate(new Vector3(0f, 0f, 1f) * Time.deltaTime * 50f);
    }
    void OnTriggerEnter(Collider other)
    {
        if (!collected && other.CompareTag("Player"))
        {
            GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("Cannot collect a coin without a GameManager.", this);
                return;
            }

            if (gameManager.HasEnded)
            {
                return;
            }

            collected = true;
            gameManager.CollectCoin(transform.position);
            Destroy(gameObject);
        }
    }
}
