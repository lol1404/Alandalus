using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Invulnerabilidad")]
    public float invincibilityTime = 1f;
    private bool isInvincible = false;

    private Rigidbody2D rb;

    [Header("UI - Velas")]
    public Animator[] velaAnimators;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        for (int i = 0; i < velaAnimators.Length; i++)
        {
            if (velaAnimators[i] != null)
            {
                velaAnimators[i].Play("vidaon", 0, Random.Range(0f, 1f));
            }
        }
    }

    // Versión simple (sin knockback externo)
    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0)
            return;

        currentHealth -= damage;
        Debug.Log("Player took damage! Health: " + currentHealth);

        // Apagar vela
        if (currentHealth >= 0 && currentHealth < velaAnimators.Length)
        {
            Animator vela = velaAnimators[currentHealth];
            if (vela != null && vela.gameObject != null)
            {
                vela.SetTrigger("Apagar");
            }
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(DieWithDelay());
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private IEnumerator DieWithDelay()
    {
        Debug.Log("Player murió!");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }
}
