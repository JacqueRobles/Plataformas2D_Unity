using UnityEngine;
using System;

/// <summary>
/// Clase base para todos los power-ups del juego
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class PowerUpBase : MonoBehaviour
{
    [Header("Configuración Base")]
    public float duracionEfecto = 10f;
    public bool desapareceAlUsarse = true;
    public float tiempoRespawn = 30f;
    
    [Header("Efectos Visuales")]
    public GameObject efectoRecoleccion;
    public float velocidadRotacion = 90f;
    public float amplitudFlotacion = 0.2f;
    public float velocidadFlotacion = 2f;
    
    [Header("Audio")]
    public AudioClip sonidoRecoleccion;
    [Range(0f, 1f)]
    public float volumenSonido = 0.7f;
    
    // Componentes
    protected SpriteRenderer spriteRenderer;
    protected Collider2D col;
    protected AudioSource audioSource;
    
    // Estado
    public bool EstaActivo { get; protected set; } = true;
    protected Vector3 posicionInicial;
    
    // Eventos
    public static event Action<PowerUpBase, PlayerController> OnPowerUpRecolectado;
    
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Configurar como trigger
        col.isTrigger = true;
        
        posicionInicial = transform.position;
    }
    
    protected virtual void Update()
    {
        if (!EstaActivo) return;
        
        // Efectos visuales básicos
        EfectosVisuales();
    }
    
    void EfectosVisuales()
    {
        // Rotación constante
        transform.Rotate(0, 0, velocidadRotacion * Time.deltaTime);
        
        // Flotación vertical
        float nuevaY = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!EstaActivo) return;
        
        if (other.CompareTag("Player") || other.CompareTag("PlayerFury"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                RecolectarPowerUp(player);
            }
        }
    }
    
    void RecolectarPowerUp(PlayerController player)
    {
        // Aplicar efecto específico del power-up
        AplicarEfecto(player);
        
        // Efectos comunes
        ReproducirSonido();
        CrearEfectoVisual();
        
        // Notificar evento
        OnPowerUpRecolectado?.Invoke(this, player);
        
        // Manejar desaparición/respawn
        if (desapareceAlUsarse)
        {
            DesactivarPowerUp();
            
            if (tiempoRespawn > 0)
            {
                Invoke(nameof(ReactivarPowerUp), tiempoRespawn);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    /// <summary>
    /// Método abstracto que debe implementar cada power-up específico
    /// </summary>
    /// <param name="player">El jugador que recolectó el power-up</param>
    protected abstract void AplicarEfecto(PlayerController player);
    
    void ReproducirSonido()
    {
        if (sonidoRecoleccion != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(sonidoRecoleccion, volumenSonido);
            }
            else
            {
                AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position, volumenSonido);
            }
        }
    }
    
    void CrearEfectoVisual()
    {
        if (efectoRecoleccion != null)
        {
            GameObject efecto = Instantiate(efectoRecoleccion, transform.position, Quaternion.identity);
            Destroy(efecto, 2f);
        }
    }
    
    void DesactivarPowerUp()
    {
        EstaActivo = false;
        spriteRenderer.enabled = false;
        col.enabled = false;
    }
    
    void ReactivarPowerUp()
    {
        EstaActivo = true;
        spriteRenderer.enabled = true;
        col.enabled = true;
        transform.position = posicionInicial;
    }
    
    void OnDrawGizmosSelected()
    {
        // Mostrar área de efecto
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // Mostrar información de duración
        if (Application.isPlaying && EstaActivo)
        {
            Gizmos.color = Color.green;
        }
        else if (!EstaActivo)
        {
            Gizmos.color = Color.red;
        }
        
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
    }
} 