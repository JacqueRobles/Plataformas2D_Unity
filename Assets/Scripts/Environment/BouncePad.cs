using UnityEngine;
using System;

/// <summary>
/// Trampolín que impulsa al jugador con fuerza configurable
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BouncePad : MonoBehaviour
{
    [Header("Configuración de Impulso")]
    public float fuerzaImpulso = 20f;
    public Vector2 direccionImpulso = Vector2.up;
    public bool normalizarDireccion = true;
    
    [Header("Limitaciones")]
    public float cooldownUso = 0.5f;
    public bool soloAfectaJugador = true;
    public LayerMask capasAfectadas = -1;
    
    [Header("Efectos Visuales")]
    public Animator animatorTrampolines;
    public string triggerAnimacion = "Bounce";
    public GameObject efectoImpulso;
    public float duracionEfecto = 1f;
    
    [Header("Audio")]
    public AudioClip sonidoImpulso;
    [Range(0f, 1f)]
    public float volumenSonido = 0.7f;
    
    [Header("Feedback Visual")]
    public float escalaMaxima = 1.2f;
    public float duracionEscala = 0.2f;
    
    // Componentes
    private AudioSource audioSource;
    private Vector3 escalaOriginal;
    
    // Estado
    private float ultimoUso;
    public bool PuedeUsarse => Time.time >= ultimoUso + cooldownUso;
    
    // Eventos
    public static event Action<BouncePad, GameObject> OnTrampolinUsado;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        escalaOriginal = transform.localScale;
        
        // Normalizar dirección si está habilitado
        if (normalizarDireccion)
        {
            direccionImpulso = direccionImpulso.normalized;
        }
        
        // Configurar como trigger
        GetComponent<Collider2D>().isTrigger = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!PuedeUsarse) return;
        
        // Verificar si el objeto puede ser afectado
        if (!PuedeAfectar(other.gameObject)) return;
        
        // Aplicar impulso
        AplicarImpulso(other.gameObject);
    }
    
    bool PuedeAfectar(GameObject objeto)
    {
        // Verificar si solo afecta al jugador
        if (soloAfectaJugador)
        {
            return objeto.CompareTag("Player") || objeto.CompareTag("PlayerFury");
        }
        
        // Verificar capas afectadas
        int capaObjeto = 1 << objeto.layer;
        return (capasAfectadas.value & capaObjeto) != 0;
    }
    
    void AplicarImpulso(GameObject objeto)
    {
        Rigidbody2D rb = objeto.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        
        // Calcular fuerza de impulso
        Vector2 fuerzaFinal = direccionImpulso * fuerzaImpulso;
        
        // Aplicar fuerza
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Resetear velocidad Y para impulso consistente
        rb.AddForce(fuerzaFinal, ForceMode2D.Impulse);
        
        // Actualizar tiempo de último uso
        ultimoUso = Time.time;
        
        // Activar efectos
        ActivarEfectos();
        
        // Notificar evento
        OnTrampolinUsado?.Invoke(this, objeto);
        
        Debug.Log($"Trampolín impulsa a {objeto.name} con fuerza {fuerzaFinal}");
    }
    
    void ActivarEfectos()
    {
        // Reproducir animación
        if (animatorTrampolines != null && !string.IsNullOrEmpty(triggerAnimacion))
        {
            animatorTrampolines.SetTrigger(triggerAnimacion);
        }
        
        // Reproducir sonido
        ReproducirSonido();
        
        // Crear efecto visual
        CrearEfectoVisual();
        
        // Feedback de escala
        StartCoroutine(EfectoEscala());
    }
    
    void ReproducirSonido()
    {
        if (sonidoImpulso != null)
        {
            if (audioSource != null)
            {
                audioSource.volume = volumenSonido;
                audioSource.PlayOneShot(sonidoImpulso);
            }
            else
            {
                AudioSource.PlayClipAtPoint(sonidoImpulso, transform.position, volumenSonido);
            }
        }
    }
    
    void CrearEfectoVisual()
    {
        if (efectoImpulso != null)
        {
            GameObject efecto = Instantiate(efectoImpulso, transform.position, 
                                          Quaternion.LookRotation(Vector3.forward, direccionImpulso));
            
            // Destruir efecto después del tiempo especificado
            Destroy(efecto, duracionEfecto);
        }
    }
    
    System.Collections.IEnumerator EfectoEscala()
    {
        float tiempoTranscurrido = 0f;
        
        // Escalar hacia arriba
        while (tiempoTranscurrido < duracionEscala / 2f)
        {
            float progreso = tiempoTranscurrido / (duracionEscala / 2f);
            float escalaActual = Mathf.Lerp(1f, escalaMaxima, progreso);
            transform.localScale = escalaOriginal * escalaActual;
            
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }
        
        tiempoTranscurrido = 0f;
        
        // Escalar hacia abajo
        while (tiempoTranscurrido < duracionEscala / 2f)
        {
            float progreso = tiempoTranscurrido / (duracionEscala / 2f);
            float escalaActual = Mathf.Lerp(escalaMaxima, 1f, progreso);
            transform.localScale = escalaOriginal * escalaActual;
            
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }
        
        // Asegurar escala original
        transform.localScale = escalaOriginal;
    }
    
    /// <summary>
    /// Método público para activar el trampolín manualmente
    /// </summary>
    /// <param name="objetivo">GameObject a impulsar</param>
    public void ActivarManualmente(GameObject objetivo)
    {
        if (PuedeUsarse && PuedeAfectar(objetivo))
        {
            AplicarImpulso(objetivo);
        }
    }
    
    /// <summary>
    /// Configurar la dirección del impulso dinámicamente
    /// </summary>
    /// <param name="nuevaDireccion">Nueva dirección del impulso</param>
    public void ConfigurarDireccion(Vector2 nuevaDireccion)
    {
        direccionImpulso = normalizarDireccion ? nuevaDireccion.normalized : nuevaDireccion;
    }
    
    /// <summary>
    /// Configurar la fuerza del impulso dinámicamente
    /// </summary>
    /// <param name="nuevaFuerza">Nueva fuerza del impulso</param>
    public void ConfigurarFuerza(float nuevaFuerza)
    {
        fuerzaImpulso = Mathf.Max(0f, nuevaFuerza);
    }
    
    void OnDrawGizmosSelected()
    {
        // Mostrar dirección y fuerza del impulso
        Gizmos.color = Color.green;
        Vector3 inicio = transform.position;
        Vector3 fin = inicio + (Vector3)(direccionImpulso * (fuerzaImpulso * 0.1f));
        
        Gizmos.DrawLine(inicio, fin);
        Gizmos.DrawWireSphere(fin, 0.2f);
        
        // Mostrar área de efecto
        Gizmos.color = Color.yellow;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, 
                              col.bounds.size);
        }
        
        // Mostrar información de cooldown
        if (Application.isPlaying)
        {
            Gizmos.color = PuedeUsarse ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
        }
    }
    
    void OnDrawGizmos()
    {
        // Mostrar dirección básica siempre
        Gizmos.color = Color.cyan;
        Vector3 direccion = (Vector3)(direccionImpulso * 2f);
        Gizmos.DrawRay(transform.position, direccion);
    }
} 