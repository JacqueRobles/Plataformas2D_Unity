using UnityEngine;
using System.Collections;

/// <summary>
/// Torreta que dispara proyectiles automáticamente hacia el jugador
/// </summary>
public class TurretAI : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject proyectilPrefab;
    public Transform puntoDisparo;
    public float intervalDisparo = 2f;
    public float velocidadProyectil = 10f;
    public int municionTotal = -1; // -1 = infinita
    
    [Header("Detección")]
    public float rangoDeteccion = 12f;
    public float anguloDeteccion = 90f;
    public LayerMask capasObstaculos = -1;
    public bool necesitaLineaVision = true;
    
    [Header("Rotación")]
    public bool puedeRotar = true;
    public float velocidadRotacion = 90f;
    public Transform parteRotatoria; // Parte que rota hacia el jugador
    
    [Header("Estados")]
    public bool activaAlInicio = true;
    public bool disparaAutomaticamente = true;
    
    [Header("Efectos")]
    public GameObject efectoDisparo;
    public AudioClip sonidoDisparo;
    public AudioClip sonidoDeteccion;
    
    // Componentes
    private AudioSource audioSource;
    private Transform jugador;
    
    // Estado
    public bool EstaActiva { get; private set; }
    public bool JugadorEnRango { get; private set; }
    public int MunicionRestante { get; private set; }
    
    // Control de disparo
    private float tiempoUltimoDisparo;
    private Coroutine corrutinaDisparo;
    
    // Rotación
    private Quaternion rotacionObjetivo;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Si no se especifica parte rotatoria, usar el transform principal
        if (parteRotatoria == null)
            parteRotatoria = transform;
            
        // Si no se especifica punto de disparo, usar el centro
        if (puntoDisparo == null)
            puntoDisparo = transform;
    }
    
    void Start()
    {
        // Buscar al jugador
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
        
        // Configurar munición
        MunicionRestante = municionTotal;
        
        // Activar si está configurado
        if (activaAlInicio)
        {
            ActivarTorreta();
        }
        
        rotacionObjetivo = parteRotatoria.rotation;
    }
    
    void Update()
    {
        if (!EstaActiva || jugador == null) return;
        
        // Detectar jugador
        DetectarJugador();
        
        // Rotar hacia el jugador si está en rango
        if (JugadorEnRango && puedeRotar)
        {
            RotarHaciaJugador();
        }
        
        // Disparar automáticamente si está configurado
        if (disparaAutomaticamente && JugadorEnRango && PuedeDisparar())
        {
            Disparar();
        }
    }
    
    void DetectarJugador()
    {
        if (jugador == null) return;
        
        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
        bool estabaDentroRango = JugadorEnRango;
        
        // Verificar distancia
        if (distanciaAlJugador <= rangoDeteccion)
        {
            // Verificar ángulo de detección
            Vector2 direccionAlJugador = (jugador.position - transform.position).normalized;
            float anguloAlJugador = Vector2.Angle(parteRotatoria.up, direccionAlJugador);
            
            if (anguloAlJugador <= anguloDeteccion / 2f)
            {
                // Verificar línea de visión si es necesario
                if (!necesitaLineaVision || TieneLineaVision())
                {
                    JugadorEnRango = true;
                }
                else
                {
                    JugadorEnRango = false;
                }
            }
            else
            {
                JugadorEnRango = false;
            }
        }
        else
        {
            JugadorEnRango = false;
        }
        
        // Reproducir sonido de detección
        if (JugadorEnRango && !estabaDentroRango && sonidoDeteccion != null)
        {
            ReproducirSonido(sonidoDeteccion);
        }
    }
    
    bool TieneLineaVision()
    {
        Vector2 direccion = jugador.position - puntoDisparo.position;
        RaycastHit2D hit = Physics2D.Raycast(puntoDisparo.position, direccion.normalized, 
                                           direccion.magnitude, capasObstaculos);
        
        // Si no hay obstáculos, tiene línea de visión
        return hit.collider == null;
    }
    
    void RotarHaciaJugador()
    {
        Vector2 direccion = (jugador.position - parteRotatoria.position).normalized;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg - 90f;
        
        rotacionObjetivo = Quaternion.AngleAxis(angulo, Vector3.forward);
        parteRotatoria.rotation = Quaternion.RotateTowards(parteRotatoria.rotation, 
                                                          rotacionObjetivo, 
                                                          velocidadRotacion * Time.deltaTime);
    }
    
    bool PuedeDisparar()
    {
        // Verificar cooldown
        if (Time.time < tiempoUltimoDisparo + intervalDisparo)
            return false;
            
        // Verificar munición
        if (municionTotal > 0 && MunicionRestante <= 0)
            return false;
            
        return true;
    }
    
    public void Disparar()
    {
        if (!PuedeDisparar()) return;
        
        // Crear proyectil
        GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, parteRotatoria.rotation);
        
        // Configurar proyectil
        TurretProjectile scriptProyectil = proyectil.GetComponent<TurretProjectile>();
        if (scriptProyectil != null)
        {
            scriptProyectil.ConfigurarProyectil(velocidadProyectil, parteRotatoria.up);
        }
        else
        {
            // Configuración básica si no tiene script específico
            Rigidbody2D rbProyectil = proyectil.GetComponent<Rigidbody2D>();
            if (rbProyectil != null)
            {
                rbProyectil.linearVelocity = parteRotatoria.up * velocidadProyectil;
            }
        }
        
        // Actualizar estado
        tiempoUltimoDisparo = Time.time;
        if (municionTotal > 0)
        {
            MunicionRestante--;
        }
        
        // Efectos
        CrearEfectoDisparo();
        ReproducirSonido(sonidoDisparo);
        
        Debug.Log($"Torreta dispara. Munición restante: {(municionTotal > 0 ? MunicionRestante.ToString() : "∞")}");
    }
    
    void CrearEfectoDisparo()
    {
        if (efectoDisparo != null)
        {
            GameObject efecto = Instantiate(efectoDisparo, puntoDisparo.position, parteRotatoria.rotation);
            Destroy(efecto, 2f);
        }
    }
    
    void ReproducirSonido(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public void ActivarTorreta()
    {
        EstaActiva = true;
        
        if (disparaAutomaticamente)
        {
            IniciarDisparoAutomatico();
        }
    }
    
    public void DesactivarTorreta()
    {
        EstaActiva = false;
        DetenerDisparoAutomatico();
    }
    
    void IniciarDisparoAutomatico()
    {
        if (corrutinaDisparo != null)
        {
            StopCoroutine(corrutinaDisparo);
        }
        
        corrutinaDisparo = StartCoroutine(RutinaDisparoAutomatico());
    }
    
    void DetenerDisparoAutomatico()
    {
        if (corrutinaDisparo != null)
        {
            StopCoroutine(corrutinaDisparo);
            corrutinaDisparo = null;
        }
    }
    
    System.Collections.IEnumerator RutinaDisparoAutomatico()
    {
        while (EstaActiva)
        {
            if (JugadorEnRango && PuedeDisparar())
            {
                Disparar();
            }
            
            yield return new WaitForSeconds(0.1f); // Verificar cada 0.1 segundos
        }
    }
    
    /// <summary>
    /// Recargar munición de la torreta
    /// </summary>
    /// <param name="cantidad">Cantidad a recargar, -1 para llenar completamente</param>
    public void RecargarMunicion(int cantidad = -1)
    {
        if (municionTotal <= 0) return; // Munición infinita
        
        if (cantidad == -1)
        {
            MunicionRestante = municionTotal;
        }
        else
        {
            MunicionRestante = Mathf.Min(MunicionRestante + cantidad, municionTotal);
        }
    }
    
    /// <summary>
    /// Configurar nuevo objetivo para la torreta
    /// </summary>
    /// <param name="nuevoObjetivo">Transform del nuevo objetivo</param>
    public void ConfigurarObjetivo(Transform nuevoObjetivo)
    {
        jugador = nuevoObjetivo;
    }
    
    void OnDrawGizmosSelected()
    {
        // Mostrar rango de detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        
        // Mostrar ángulo de detección
        if (parteRotatoria != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 direccionCentral = parteRotatoria.up;
            Vector3 direccionIzquierda = Quaternion.Euler(0, 0, anguloDeteccion / 2f) * direccionCentral;
            Vector3 direccionDerecha = Quaternion.Euler(0, 0, -anguloDeteccion / 2f) * direccionCentral;
            
            Gizmos.DrawRay(transform.position, direccionIzquierda * rangoDeteccion);
            Gizmos.DrawRay(transform.position, direccionDerecha * rangoDeteccion);
        }
        
        // Mostrar punto de disparo
        if (puntoDisparo != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(puntoDisparo.position, 0.2f);
        }
        
        // Mostrar línea de visión al jugador
        if (Application.isPlaying && jugador != null && JugadorEnRango)
        {
            Gizmos.color = TieneLineaVision() ? Color.green : Color.red;
            Gizmos.DrawLine(puntoDisparo.position, jugador.position);
        }
    }
    
    void OnDrawGizmos()
    {
        // Mostrar dirección de la torreta
        if (parteRotatoria != null)
        {
            Gizmos.color = EstaActiva ? Color.red : Color.gray;
            Gizmos.DrawRay(transform.position, parteRotatoria.up * 2f);
        }
    }
} 