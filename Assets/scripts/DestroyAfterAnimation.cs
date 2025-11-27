using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    private Animator animator;
    private float animationDuration = 0f;
    private float elapsedTime = 0f;

    void Start()
    {
        // Obtener el Animator del hijo (Visual)
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogError("[DestroyAfterAnimation] No se encontró Animator. Destruyendo en 1 segundo por defecto.");
            Destroy(gameObject, 1f);
            return;
        }

        // Obtener la duración de la animación actual
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        animationDuration = stateInfo.length;

        Debug.Log($"[DestroyAfterAnimation] Animación durará {animationDuration} segundos");
    }

    void Update()
    {
        if (animator == null) return;

        elapsedTime += Time.deltaTime;

        // Cuando la animación termina, destruir el objeto
        if (elapsedTime >= animationDuration)
        {
            Destroy(gameObject);
        }
    }
}
