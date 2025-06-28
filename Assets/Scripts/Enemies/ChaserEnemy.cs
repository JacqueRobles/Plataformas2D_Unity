using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// Enemigo perseguidor que sigue al jugador de manera implacable usando NavMesh
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody2D))]
public class ChaserEnemy : MonoBehaviour
{
    [Header("Configuración de Persecución")]
    public float velocidadNormal = 3f;
    public float velocidadEscape = 5f;
    public float distanciaMinima = 1.5f;
    public float distanciaDeteccion = 15f;
    
    [Header("Comportamiento")]
    public float tiempoEsperaAlPerder = 2f;
    public float tiempoPatrullaAleatoria = 5f;
    public bool puedeRomperParedesFragiles = false;
    
    [Header("Reacción a Furia")]
    public float multiplicadorVelocidadFuria = 1.5f;
    public float tiempoHuidaFuria = 3f;
    
    [Header("Debug")]
    public bool mostrarDebugInfo = false;
    
    // Componentes
    private NavMeshAgent agent;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform jugador;
    
    // Estados
    public EstadoPerseguidor EstadoActual { get; private set; } = EstadoPerseguidor.Buscando;
    private bool jugadorEnFuria = false;
    private bool mirandoDerecha = true;
    
    // Timers y control
    private float timerPerderJugador;
    private float timerPatrulla;
    private float timerHuidaFuria;
    private Vector3 ultimaPosicionJugador;
    private Vector3 puntoPatrullaObjetivo;
    
    // Configuración NavMesh para 2D
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configurar NavMeshAgent para 2D
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = velocidadNormal;
    }
    
    void Start()
    {
        // Buscar al jugador
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
        
        // Suscribirse a eventos
        GameManager.OnFuriaActivada += ReaccionarAFuria;
        GameManager.OnFuriaDesactivada += FinalizarReaccionFuria;
        
        InicializarPatrulla();
    }
    
    void Update()
    {
        if (jugador == null) return;
        
        ActualizarEstado();
        EjecutarComportamiento();
        ActualizarVisuales();
        
        if (mostrarDebugInfo)
            MostrarDebugInfo();
    }
    
    void ActualizarEstado()
    {
        float distanciaAlJugador = Vector3.Distance(transform.position, jugador.position);
        
        // Verificar si puede ver al jugador (sin obstáculos)
        bool puedeVerJugador = PuedeVerJugador();
        
        switch (EstadoActual)
        {
            case EstadoPerseguidor.Buscando:
                if (puedeVerJugador && distanciaAlJugador <= distanciaDeteccion)
                {
                    CambiarEstado(EstadoPerseguidor.Persiguiendo);
                }
                break;
                
            case EstadoPerseguidor.Persiguiendo:
                if (jugadorEnFuria)
                {
                    CambiarEstado(EstadoPerseguidor.Huyendo);
                }
                else if (!puedeVerJugador)
                {
                    ultimaPosicionJugador = jugador.position;
                    timerPerderJugador = tiempoEsperaAlPerder;
                    CambiarEstado(EstadoPerseguidor.BuscandoUltimaPosicion);
                }
                break;
                
            case EstadoPerseguidor.BuscandoUltimaPosicion:
                timerPerderJugador -= Time.deltaTime;
                
                if (puedeVerJugador && distanciaAlJugador <= distanciaDeteccion)
                {
                    CambiarEstado(EstadoPerseguidor.Persiguiendo);
                }
                else if (timerPerderJugador <= 0)
                {
                    CambiarEstado(EstadoPerseguidor.Patrullando);
                }
                break;
                
            case EstadoPerseguidor.Patrullando:
                timerPatrulla -= Time.deltaTime;
                
                if (puedeVerJugador && distanciaAlJugador <= distanciaDeteccion)
                {
                    CambiarEstado(EstadoPerseguidor.Persiguiendo);
                }
                else if (timerPatrulla <= 0)
                {
                    GenerarNuevoPuntoPatrulla();
                }
                break;
                
            case EstadoPerseguidor.Huyendo:
                timerHuidaFuria -= Time.deltaTime;
                
                if (!jugadorEnFuria || timerHuidaFuria <= 0)
                {
                    CambiarEstado(EstadoPerseguidor.Persiguiendo);
                }
                break;
        }
    }
    
    void EjecutarComportamiento()
    {
        switch (EstadoActual)
        {
            case EstadoPerseguidor.Buscando:
                // Movimiento aleatorio lento
                if (!agent.hasPath)
                {
                    GenerarNuevoPuntoPatrulla();
                }
                break;
                
            case EstadoPerseguidor.Persiguiendo:
                PerseguirJugador();
                break;
                
            case EstadoPerseguidor.BuscandoUltimaPosicion:
                agent.SetDestination(ultimaPosicionJugador);
                break;
                
            case EstadoPerseguidor.Patrullando:
                if (!agent.hasPath)
                {
                    GenerarNuevoPuntoPatrulla();
                }
                break;
                
            case EstadoPerseguidor.Huyendo:
                HuirDelJugador();
                break;
        }
    }
    
    void PerseguirJugador()
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);
        
        // Mantener distancia mínima para no "atrapar" inmediatamente al jugador
        if (distancia > distanciaMinima)
        {
            agent.SetDestination(jugador.position);
            agent.speed = velocidadNormal;
        }
        else
        {
            agent.ResetPath();
        }
    }
    
    void HuirDelJugador()
    {
        // Calcular dirección opuesta al jugador
        Vector3 direccionHuida = (transform.position - jugador.position).normalized;
        Vector3 puntoHuida = transform.position + direccionHuida * 5f;
        
        agent.SetDestination(puntoHuida);
        agent.speed = velocidadEscape;
    }
    
    void GenerarNuevoPuntoPatrulla()
    {
        // Generar punto aleatorio dentro de un radio
        Vector3 puntoAleatorio = transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 8f;
        
        // Verificar que el punto sea válido en el NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(puntoAleatorio, out hit, 8f, NavMesh.AllAreas))
        {
            puntoPatrullaObjetivo = hit.position;
            agent.SetDestination(puntoPatrullaObjetivo);
            timerPatrulla = tiempoPatrullaAleatoria;
        }
    }
    
    bool PuedeVerJugador()
    {
        if (jugador == null) return false;
        
        Vector3 direccionAlJugador = jugador.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionAlJugador.normalized, 
                                           direccionAlJugador.magnitude, LayerMask.GetMask("Walls"));
        
        // Si no hay obstáculos, puede ver al jugador
        return hit.collider == null;
    }
    
    void CambiarEstado(EstadoPerseguidor nuevoEstado)
    {
        if (EstadoActual == nuevoEstado) return;
        
        EstadoActual = nuevoEstado;
        
        if (mostrarDebugInfo)
            Debug.Log($"Perseguidor cambió a estado: {nuevoEstado}");
    }
    
    void ReaccionarAFuria()
    {
        jugadorEnFuria = true;
        timerHuidaFuria = tiempoHuidaFuria;
        
        // Aumentar velocidad y cambiar comportamiento
        agent.speed = velocidadNormal * multiplicadorVelocidadFuria;
        
        if (EstadoActual == EstadoPerseguidor.Persiguiendo)
        {
            CambiarEstado(EstadoPerseguidor.Huyendo);
        }
    }
    
    void FinalizarReaccionFuria()
    {
        jugadorEnFuria = false;
        agent.speed = velocidadNormal;
    }
    
    void ActualizarVisuales()
    {
        // Voltear sprite según dirección de movimiento
        if (agent.hasPath && agent.velocity.x != 0)
        {
            bool deberíaMirarDerecha = agent.velocity.x > 0;
            if (deberíaMirarDerecha != mirandoDerecha)
            {
                mirandoDerecha = deberíaMirarDerecha;
                transform.Rotate(0, 180, 0);
            }
        }
        
        // Cambiar color según estado
        Color colorEstado = Color.white;
        switch (EstadoActual)
        {
            case EstadoPerseguidor.Persiguiendo:
                colorEstado = Color.red;
                break;
            case EstadoPerseguidor.Huyendo:
                colorEstado = Color.blue;
                break;
            case EstadoPerseguidor.Buscando:
                colorEstado = Color.yellow;
                break;
            case EstadoPerseguidor.Patrullando:
                colorEstado = Color.green;
                break;
        }
        
        spriteRenderer.color = colorEstado;
    }
    
    void InicializarPatrulla()
    {
        GenerarNuevoPuntoPatrulla();
        CambiarEstado(EstadoPerseguidor.Buscando);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Si toca al jugador y no está en furia, el jugador muere
        if (other.CompareTag("Player") && !jugadorEnFuria)
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // El jugador morirá por el trigger "Deadly" que debería tener este enemigo
            }
        }
        
        // Romper paredes frágiles si está habilitado
        if (puedeRomperParedesFragiles && other.CompareTag("FragileWall"))
        {
            FragileWall paredFragil = other.GetComponent<FragileWall>();
            if (paredFragil != null)
            {
                paredFragil.Destruir();
            }
        }
    }
    
    void MostrarDebugInfo()
    {
        Debug.Log($"Estado: {EstadoActual}, Velocidad: {agent.speed:F1}, " +
                 $"Distancia al jugador: {(jugador ? Vector3.Distance(transform.position, jugador.position) : -1):F1}");
    }
    
    void OnDrawGizmosSelected()
    {
        // Mostrar rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
        
        // Mostrar distancia mínima
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaMinima);
        
        // Mostrar destino actual
        if (Application.isPlaying && agent != null && agent.hasPath)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, agent.destination);
            Gizmos.DrawWireSphere(agent.destination, 0.5f);
        }
        
        // Mostrar línea de visión al jugador
        if (Application.isPlaying && jugador != null)
        {
            Gizmos.color = PuedeVerJugador() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, jugador.position);
        }
    }
    
    void OnDestroy()
    {
        // Desuscribirse de eventos
        GameManager.OnFuriaActivada -= ReaccionarAFuria;
        GameManager.OnFuriaDesactivada -= FinalizarReaccionFuria;
    }
}

public enum EstadoPerseguidor
{
    Buscando,
    Persiguiendo,
    BuscandoUltimaPosicion,
    Patrullando,
    Huyendo
} 