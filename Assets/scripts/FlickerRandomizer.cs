using UnityEngine;

public class FlickerRandomizer : MonoBehaviour
{
    public float minSpeed = 0.8f;  // Velocidad mínima
    public float maxSpeed = 1.2f;  // Velocidad máxima

    void Start()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = Random.Range(minSpeed, maxSpeed);
        }
    }
}
