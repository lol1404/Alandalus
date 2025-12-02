using UnityEngine;
using System.Collections;

/// <summary>
/// Controla el comportamiento de una flecha: movimiento, colisión, daño y ciclo de vida.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ArrowProjectile : MonoBehaviour
{
    [Header("Configuración de Clavado")]
    [Tooltip("Sprite que se usa cuando la flecha se clava en el suelo.")]
    public Sprite stuckArrowSprite;
    [Tooltip("Tiempo que la flecha permanece clavada antes de desaparecer.")]
    public float stuckDuration = 3.0f;
    [Tooltip("Tiempo que dura el fade-out al desaparecer.")]
    public float fadeOutDuration = 1.0f;

    [Header("Configuración de Colisión")]
    [Tooltip("Capas de física que detendrán la flecha (ej: Obstacles, Walls).")]
    public LayerMask obstacleLayers;

    // Estado interno
    private BowData bowData;
    private Vector2 direction;
    private Rigidbody2D rb;
    private Vector3 startPosition;
    private bool hasHit = false;
    private int pierceCount;
    private SpriteRenderer spriteRenderer;

    public void Initialize(BowData data, Vector2 dir, Vector2 initialVelocity)
    {
        this.bowData = data;
        this.direction = dir;
        this.pierceCount = data.piercingCount; // Preparado para mejoras

        rb = GetComponent<Rigidbody2D>();

        // --- Lógica de Velocidad Mejorada ---
        // 1. Proyectamos la velocidad del jugador sobre la dirección de la flecha.
        //    Esto nos da la contribución de velocidad del jugador en la dirección del disparo.
        float playerSpeedInArrowDirection = Vector2.Dot(initialVelocity, dir);

        // 2. Nos aseguramos de que la velocidad base de la flecha no se vea reducida al disparar hacia atrás.
        //    La velocidad en la dirección del disparo será, como mínimo, la velocidad de la flecha.
        float finalSpeedInArrowDirection = Mathf.Max(bowData.arrowSpeed, playerSpeedInArrowDirection + bowData.arrowSpeed);
        rb.linearVelocity = dir * finalSpeedInArrowDirection;

        startPosition = transform.position;

        // Asegurarse de que el SpriteRenderer esté configurado correctamente
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Projectiles"; // Asigna la capa de ordenación
        }
    }

    void FixedUpdate()
    {
        if (hasHit) return;

        // Sincronizar la rotación del sprite con la dirección de la velocidad
        if (rb.linearVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // Simular "drag" o pérdida de velocidad gradual
        rb.linearVelocity *= 0.99f;

        // Comprobar si ha alcanzado la distancia máxima
        if (Vector3.Distance(startPosition, transform.position) >= bowData.maxRange)
        {
            StickInGround();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Ignorar colisiones con el jugador
        if (other.CompareTag("Player")) return;

        // --- Lógica de Colisión Específica ---

        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Calcular daño (base + crítico)
                bool isCrit = Random.value < bowData.critChance;
                float totalDamage = isCrit ? bowData.baseDamage * bowData.critMultiplier : bowData.baseDamage;

                Debug.Log(isCrit ? $"¡Golpe CRÍTICO! Daño: {totalDamage}" : $"Golpe normal. Daño: {totalDamage}");

                enemyHealth.TakeDamage((int)totalDamage, this.gameObject);

                // Lógica de Piercing (preparada para el futuro)
                if (pierceCount > 0)
                {
                    pierceCount--;
                }
                else
                {
                    hasHit = true;
                    Destroy(gameObject); // Se destruye al impactar
                }
            }
        }
        // Comprobar si el objeto golpeado está en una de las capas de obstáculos
        // La expresión (1 << other.gameObject.layer) crea una máscara de bits para la capa del objeto
        else if (((1 << other.gameObject.layer) & obstacleLayers) != 0)
        {
            StickInGround();
        }
        // Si no es un enemigo ni un obstáculo, la flecha lo ignora y sigue su camino.
    }

    private void StickInGround()
    {
        hasHit = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;

        // Cambiar sprite
        if (spriteRenderer != null && stuckArrowSprite != null)
        {
            spriteRenderer.sprite = stuckArrowSprite;
        }

        // Sonido de impacto en suelo
        if (bowData.groundImpactSound != null)
        {
            AudioSource.PlayClipAtPoint(bowData.groundImpactSound, transform.position);
        }

        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(stuckDuration);
        
        float timer = 0f;
        Color startColor = spriteRenderer.color;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, timer / fadeOutDuration));
            yield return null;
        }

        Destroy(gameObject);
    }
}