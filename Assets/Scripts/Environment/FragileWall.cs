using UnityEngine;
using System;

/// <summary>
/// Pared frágil que puede ser destruida por el jugador en furia, proyectiles de torretas o explosiones
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FragileWall : MonoBehaviour
{
    [Header("Configuración de Destrucción")]
    public bool puedeSerDestruidaPorFuria = true;
    public bool puedeSerDestruidaPorProyectiles = true;
    public bool puedeSerDestruidaPorExplosiones = true;
    
    [Header("Efectos Visuales")]
    public GameObject efectoDestruccion;
    public float tiempoEfecto = 2f;
    
    [Header("Audio")]
    public AudioClip sonidoDestruccion;
    
    [Header("Recompensas (Opcional)")]
    public GameObject itemRecompensa;
    public float probabilidadRecompensa = 0.3f;
    
    // Componentes
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private AudioSource audioSource;
    
    // Estado
    public bool EstaDestruida { get; private set; } = false;
    
    // Eventos
    public static event Action<FragileWall> OnParedDestruida;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Configurar como pared frágil
        gameObject.tag = "FragileWall";
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (EstaDestruida) return;
        
        // Destrucción por jugador en furia
        if (puedeSerDestruidaPorFuria && other.CompareTag("PlayerFury"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.EstaEnFuria)
            {
                Destruir("Furia del Jugador");
            }
        }
        
        // Destrucción por proyectiles
        if (puedeSerDestruidaPorProyectiles && other.CompareTag("Projectile"))
        {
            TurretProjectile proyectil = other.GetComponent<TurretProjectile>();
            if (proyectil != null)
            {
                Destruir("Proyectil de Torreta");
                proyectil.Explotar(); // El proyectil también se destruye
            }
        }
        
        // Destrucción por explosiones
        if (puedeSerDestruidaPorExplosiones && other.CompareTag("Explosion"))
        {
            Destruir("Explosión");
        }
    }
    
    /// <summary>
    /// Destruye la pared frágil con efectos opcionales
    /// </summary>
    /// <param name="causa">Causa de la destrucción para debug</param>
    public void Destruir(string causa = "Desconocida")
    {
        if (EstaDestruida) return;
        
        EstaDestruida = true;
        
        Debug.Log($"Pared frágil destruida por: {causa}");
        
        // Reproducir sonido
        ReproducirSonidoDestruccion();
        
        // Crear efecto visual
        CrearEfectoDestruccion();
        
        // Generar recompensa (si aplica)
        GenerarRecompensa();
        
        // Notificar evento
        OnParedDestruida?.Invoke(this);
        
        // Desactivar componentes visuales y de colisión
        DesactivarPared();
        
        // Destruir el objeto después del efecto
        Destroy(gameObject, tiempoEfecto);
    }
    
    void ReproducirSonidoDestruccion()
    {
        if (sonidoDestruccion != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(sonidoDestruccion);
            }
            else
            {
                // Usar AudioSource temporal si no hay uno asignado
                AudioSource.PlayClipAtPoint(sonidoDestruccion, transform.position);
            }
        }
    }
    
    void CrearEfectoDestruccion()
    {
        if (efectoDestruccion != null)
        {
            GameObject efecto = Instantiate(efectoDestruccion, transform.position, transform.rotation);
            
            // Destruir el efecto después del tiempo especificado
            Destroy(efecto, tiempoEfecto);
        }
        else
        {
            // Efecto básico con partículas simples
            CrearEfectoBasico();
        }
    }
    
    void CrearEfectoBasico()
    {
        // Crear fragmentos básicos usando el sprite actual
        for (int i = 0; i < 4; i++)
        {
            GameObject fragmento = new GameObject("Fragmento");
            fragmento.transform.position = transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f;
            
            SpriteRenderer fragmentoSprite = fragmento.AddComponent<SpriteRenderer>();
            fragmentoSprite.sprite = spriteRenderer.sprite;
            fragmentoSprite.color = spriteRenderer.color;
            
            // Hacer fragmentos más pequeños
            fragmento.transform.localScale = Vector3.one * 0.3f;
            
            // Añadir física para que caigan
            Rigidbody2D rbFragmento = fragmento.AddComponent<Rigidbody2D>();
            rbFragmento.linearVelocity = UnityEngine.Random.insideUnitCircle * 3f + Vector2.up * 2f;
            rbFragmento.angularVelocity = UnityEngine.Random.Range(-360f, 360f);
            
            // Destruir fragmentos después de un tiempo
            Destroy(fragmento, 2f);
        }
    }
    
    void GenerarRecompensa()
    {
        if (itemRecompensa != null && UnityEngine.Random.value <= probabilidadRecompensa)
        {
            Vector3 posicionRecompensa = transform.position + Vector3.up * 0.5f;
            Instantiate(itemRecompensa, posicionRecompensa, Quaternion.identity);
        }
    }
    
    void DesactivarPared()
    {
        // Desactivar renderizado
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
            
        // Desactivar colisión
        if (col != null)
            col.enabled = false;
    }
    
    /// <summary>
    /// Método público para destruir la pared desde scripts externos
    /// </summary>
    public void DestruirPorScript()
    {
        Destruir("Script Externo");
    }
    
    /// <summary>
    /// Verifica si la pared puede ser destruida por un tipo específico de fuente
    /// </summary>
    /// <param name="tipoFuente">Tipo de fuente de destrucción</param>
    /// <returns>True si puede ser destruida</returns>
    public bool PuedeSerDestruida(TipoDestruccion tipoFuente)
    {
        if (EstaDestruida) return false;
        
        switch (tipoFuente)
        {
            case TipoDestruccion.Furia:
                return puedeSerDestruidaPorFuria;
            case TipoDestruccion.Proyectil:
                return puedeSerDestruidaPorProyectiles;
            case TipoDestruccion.Explosion:
                return puedeSerDestruidaPorExplosiones;
            default:
                return false;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Mostrar área de efecto de destrucción
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Mostrar iconos según qué puede destruir esta pared
        Gizmos.color = Color.yellow;
        Vector3 offset = Vector3.up * (transform.localScale.y * 0.6f);
        
        if (puedeSerDestruidaPorFuria)
        {
            Gizmos.DrawWireSphere(transform.position + offset, 0.1f);
        }
        
        if (puedeSerDestruidaPorProyectiles)
        {
            Gizmos.DrawWireCube(transform.position + offset + Vector3.right * 0.3f, Vector3.one * 0.1f);
        }
        
        if (puedeSerDestruidaPorExplosiones)
        {
            Gizmos.DrawWireSphere(transform.position + offset + Vector3.left * 0.3f, 0.15f);
        }
    }
}

public enum TipoDestruccion
{
    Furia,
    Proyectil,
    Explosion
} 