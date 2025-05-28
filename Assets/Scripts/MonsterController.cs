using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    private Animator animator;
    private bool isDead = false;

    [Header("Animation Settings")]
    [Tooltip("Tiempo mínimo en Idle antes de atacar")]
    public float minIdleTime = 2f;
    [Tooltip("Tiempo máximo en Idle antes de atacar")]
    public float maxIdleTime = 4f;
    [Tooltip("Tiempo adicional único para este monster (para mayor desincronización)")]
    [Range(0f, 5f)]
    public float additionalUniqueDelay = 0f;
    public float attackCooldown = 1f;

    [Tooltip("Retraso inicial aleatorio para desincronizar prefabs")]
    public float maxInitialDelay = 2f;

    [Header("Parameter Names")]
    public string attackParameter = "Attack";
    public string idleParameter = "Idle";
    public string dieParameter = "Die";

    [Header("Fade Settings")]
    public float fadeDuration = 10f;
    public float delayBeforeFade = 2f;

    private float timer = 0f;
    private bool isAttacking = false;
    private float currentIdleTime; // Tiempo aleatorio actual para este ciclo
    private bool hasStartedAttacking = false; // Para controlar el retraso inicial

    // Para almacenar los materiales originales
    private Material[][] originalMaterials;
    private Renderer[] renderers;

    void Awake()
    {
        // additionalUniqueDelay = Random.Range(0f, 5f);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        // Guardar referencia a todos los renderers
        renderers = GetComponentsInChildren<Renderer>();
        // Guardar los materiales originales para restaurarlos si es necesario
        StoreOriginalMaterials();
        // Verificar que los parámetros existen al inicio
        VerifyParameters();
        // Generar el primer tiempo aleatorio para Idle
        GenerateRandomIdleTime();
        // Iniciar con un retraso aleatorio para desincronizar los prefabs
        StartCoroutine(InitialRandomDelay());
    }

    // Coroutine para retraso inicial aleatorio
    IEnumerator InitialRandomDelay()
    {
        float initialDelay = Random.Range(0f, maxInitialDelay);
        Debug.Log($"{gameObject.name} esperará {initialDelay:F2} segundos antes de empezar su ciclo de ataque");

        yield return new WaitForSeconds(initialDelay);
        hasStartedAttacking = true;
        timer = 0f;
    }

    // Genera un nuevo tiempo aleatorio para estar en Idle, añadiendo el tiempo único adicional
    void GenerateRandomIdleTime()
    {
        // Tiempo base aleatorio + tiempo único adicional configurado para este monster específico
        currentIdleTime = Random.Range(minIdleTime, maxIdleTime) + additionalUniqueDelay;
        Debug.Log($"{gameObject.name} esperará {currentIdleTime:F2} segundos antes de atacar (incluye {additionalUniqueDelay:F2}s adicionales únicos)");
    }

    void StoreOriginalMaterials()
    {
        // Almacenar los materiales originales para restaurarlos después si es necesario
        originalMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].sharedMaterials;
            originalMaterials[i] = new Material[mats.Length];
            // Guardar una referencia (no copia) a cada material original
            for (int j = 0; j < mats.Length; j++)
            {
                originalMaterials[i][j] = mats[j];
            }
        }

        Debug.Log($"Almacenados {renderers.Length} renderers con sus materiales originales");
    }

    void VerifyParameters()
    {
        AnimatorControllerParameter[] parameters = animator.parameters;
        bool foundAttack = false;
        bool foundIdle = false;
        bool foundDie = false;

        foreach (var param in parameters)
        {
            if (param.name == attackParameter) foundAttack = true;
            if (param.name == idleParameter) foundIdle = true;
            if (param.name == dieParameter) foundDie = true;
        }

        if (!foundAttack) Debug.LogWarning($"El parámetro '{attackParameter}' no existe en el Animator. Debes agregarlo.");
        if (!foundIdle) Debug.LogWarning($"El parámetro '{idleParameter}' no existe en el Animator. Debes agregarlo.");
        if (!foundDie) Debug.LogWarning($"El parámetro '{dieParameter}' no existe en el Animator. Debes agregarlo.");
    }

    void Update()
    {
        if (isDead || !hasStartedAttacking) return;

        timer += Time.deltaTime;

        if (!isAttacking && timer >= currentIdleTime)
        {
            animator.SetTrigger(attackParameter);
            isAttacking = true;
            timer = 0f;
        }
        else if (isAttacking && timer >= attackCooldown)
        {
            animator.SetTrigger(idleParameter);
            isAttacking = false;
            timer = 0f;

            GenerateRandomIdleTime();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ball"))
        {
            Die();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Ball"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger(dieParameter);

        StartCoroutine(DisappearAfterAnimation());
    }

    IEnumerator DisappearAfterAnimation()
    {
        // Esperar a que termine la animación de muerte
        yield return new WaitForSeconds(delayBeforeFade);

        CreateTransparentMaterialCopies();

        yield return StartCoroutine(FadeOutAllRenderers(fadeDuration));
        gameObject.SetActive(false);
    }

    void CreateTransparentMaterialCopies()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] currentMats = renderers[i].sharedMaterials;
            Material[] newMats = new Material[currentMats.Length];

            for (int j = 0; j < currentMats.Length; j++)
            {
                Material newMat = new Material(currentMats[j]);
                newMats[j] = newMat;

                // Configurar para transparencia según el tipo de shader
                if (newMat.shader.name.Contains("Universal Render Pipeline/Lit"))
                {
                    // Configuración para URP
                    newMat.SetFloat("_Surface", 1); // 1 = Transparent
                    newMat.SetFloat("_Blend", 0);   // 0 = Alpha blend
                    newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    newMat.SetInt("_ZWrite", 0);
                    newMat.renderQueue = 3000;
                }
                else
                {
                    // Configuración para Standard y otros
                    newMat.SetFloat("_Mode", 2); // 2 = Fade mode
                    newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    newMat.SetInt("_ZWrite", 0);
                    newMat.DisableKeyword("_ALPHATEST_ON");
                    newMat.EnableKeyword("_ALPHABLEND_ON");
                    newMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    newMat.renderQueue = 3000;
                }
            }

            renderers[i].materials = newMats;
        }
    }

    IEnumerator FadeOutAllRenderers(float duration)
    {
        if (renderers.Length == 0)
        {
            yield break;
        }

        // Almacenar colores originales para cada material
        Color[][] originalColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;
            originalColors[i] = new Color[materials.Length];

            for (int j = 0; j < materials.Length; j++)
            {
                // Obtener el color original según el tipo de shader
                if (materials[j].HasProperty("_BaseColor")) // URP
                {
                    originalColors[i][j] = materials[j].GetColor("_BaseColor");
                }
                else if (materials[j].HasProperty("_Color")) // Standard
                {
                    originalColors[i][j] = materials[j].GetColor("_Color");
                }
                else
                {
                    originalColors[i][j] = materials[j].color;
                }
            }
        }

        float elapsed = 0f;

        // Hacer el fade out gradualmente
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            float alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(normalizedTime, 0.7f));

            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].materials;

                for (int j = 0; j < materials.Length; j++)
                {
                    Color originalColor = originalColors[i][j];
                    Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

                    if (materials[j].HasProperty("_BaseColor")) // URP
                    {
                        materials[j].SetColor("_BaseColor", newColor);
                    }
                    else if (materials[j].HasProperty("_Color")) // Standard
                    {
                        materials[j].SetColor("_Color", newColor);
                    }
                    else
                    {
                        materials[j].color = newColor;
                    }
                }
            }

            if (Mathf.FloorToInt(normalizedTime * 10) != Mathf.FloorToInt((elapsed - Time.deltaTime) / duration * 10))
            {
                Debug.Log($"Fade progress: {normalizedTime:P0}, Alpha: {alpha:F2}");
            }

            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;

            for (int j = 0; j < materials.Length; j++)
            {
                Color originalColor = originalColors[i][j];
                Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

                if (materials[j].HasProperty("_BaseColor")) // URP
                {
                    materials[j].SetColor("_BaseColor", transparentColor);
                }
                else if (materials[j].HasProperty("_Color")) // Standard
                {
                    materials[j].SetColor("_Color", transparentColor);
                }
                else
                {
                    materials[j].color = transparentColor;
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
    }
}