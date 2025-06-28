using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Gestor de niveles que controla objetivos, progreso y condiciones de victoria/derrota
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    public string nombreNivel = "Nivel 1";
    public TipoObjetivo tipoObjetivo = TipoObjetivo.AlcanzarMeta;
    public Transform puntoInicio;
    public Transform puntoMeta;
    
    [Header("Objetivos Específicos")]
    public int llaveNecesarias = 0;
    public int enemigosPorEliminar = 0;
    public float tiempoLimiteNivel = 300f; // 5 minutos
    
    [Header("Elementos del Nivel")]
    public List<GameObject> llaves = new List<GameObject>();
    public List<GameObject> enemigos = new List<GameObject>();
    public List<GameObject> puntosControl = new List<GameObject>();
    
    [Header("Spawning")]
    public bool respawnearJugadorAlMorir = true;
    public Transform[] puntosRespawn;
    public float tiempoRespawn = 2f;
    
    [Header("Debug")]
    public bool mostrarDebugInfo = false;
    
    // Estado del nivel
    public int LlavesRecolectadas { get; private set; } = 0;
    public int EnemigosEliminados { get; private set; } = 0;
    public int PuntosControlActivados { get; private set; } = 0;
    public bool NivelCompletado { get; private set; } = false;
    public bool NivelFallado { get; private set; } = false;
    
    // Referencias
    private PlayerController jugador;
    private ChaserEnemy perseguidor;
    
    // Eventos
    public static event Action OnNivelCompletado;
    public static event Action OnNivelFallado;
    public static event Action<int> OnLlaveRecolectada;
    public static event Action<int> OnEnemigoEliminado;
    public static event Action<string> OnObjetivoActualizado;
    
    void Awake()
    {
        // Buscar componentes clave
        jugador = FindFirstObjectByType<PlayerController>();
        perseguidor = FindFirstObjectByType<ChaserEnemy>();
        
        // Si no se especifica punto de inicio, usar posición del jugador
        if (puntoInicio == null && jugador != null)
        {
            puntoInicio = jugador.transform;
        }
    }
    
    void Start()
    {
        InicializarNivel();
        
        // Suscribirse a eventos
        GameManager.OnJugadorMuerto += ManejarMuerteJugador;
        FragileWall.OnParedDestruida += ManejarParedDestruida;
        
        // Buscar automáticamente elementos si las listas están vacías
        if (llaves.Count == 0)
            BuscarLlaves();
        if (enemigos.Count == 0)
            BuscarEnemigos();
        if (puntosControl.Count == 0)
            BuscarPuntosControl();
    }
    
    void Update()
    {
        if (NivelCompletado || NivelFallado) return;
        
        VerificarCondicionesVictoria();
        VerificarCondicionesDerrota();
        
        if (mostrarDebugInfo)
            MostrarEstadoNivel();
    }
    
    void InicializarNivel()
    {
        // Colocar jugador en punto de inicio
        if (jugador != null && puntoInicio != null)
        {
            jugador.transform.position = puntoInicio.position;
        }
        
        // Colocar perseguidor si existe
        if (perseguidor != null && puntoInicio != null)
        {
            Vector3 posicionPerseguidor = puntoInicio.position + Vector3.left * 5f; // 5 unidades a la izquierda
            perseguidor.transform.position = posicionPerseguidor;
        }
        
        // Inicializar contadores
        LlavesRecolectadas = 0;
        EnemigosEliminados = 0;
        PuntosControlActivados = 0;
        
        // Configurar objetivos iniciales
        ConfigurarObjetivos();
        
        Debug.Log($"Nivel iniciado: {nombreNivel}");
        ActualizarObjetivo();
    }
    
    void ConfigurarObjetivos()
    {
        // Configurar llaves
        foreach (GameObject llave in llaves)
        {
            if (llave != null)
            {
                LlaveColeccionable script = llave.GetComponent<LlaveColeccionable>();
                if (script == null)
                {
                    script = llave.AddComponent<LlaveColeccionable>();
                }
                script.OnLlaveRecolectada += RecolectarLlave;
            }
        }
        
        // Configurar enemigos
        foreach (GameObject enemigo in enemigos)
        {
            if (enemigo != null)
            {
                // Agregar evento de muerte si no existe
                ObjetoDestructible destructible = enemigo.GetComponent<ObjetoDestructible>();
                if (destructible == null)
                {
                    destructible = enemigo.AddComponent<ObjetoDestructible>();
                }
                // Suscribirse al evento de muerte (implementar según sea necesario)
            }
        }
        
        // Configurar puntos de control
        foreach (GameObject punto in puntosControl)
        {
            if (punto != null)
            {
                PuntoControl script = punto.GetComponent<PuntoControl>();
                if (script == null)
                {
                    script = punto.AddComponent<PuntoControl>();
                }
                script.OnPuntoActivado += ActivarPuntoControl;
            }
        }
    }
    
    void VerificarCondicionesVictoria()
    {
        bool objetivoCumplido = false;
        
        switch (tipoObjetivo)
        {
            case TipoObjetivo.AlcanzarMeta:
                // Se verifica en OnTriggerEnter del punto meta
                break;
                
            case TipoObjetivo.RecolectarLlaves:
                objetivoCumplido = LlavesRecolectadas >= llaveNecesarias;
                break;
                
            case TipoObjetivo.EliminarEnemigos:
                objetivoCumplido = EnemigosEliminados >= enemigosPorEliminar;
                break;
                
            case TipoObjetivo.ActivarPuntosControl:
                objetivoCumplido = PuntosControlActivados >= puntosControl.Count;
                break;
                
            case TipoObjetivo.Supervivencia:
                // El objetivo se cumple automáticamente si sobrevive el tiempo límite
                objetivoCumplido = GameManager.Instance.TiempoRestante <= 0;
                break;
                
            case TipoObjetivo.Combinado:
                objetivoCumplido = LlavesRecolectadas >= llaveNecesarias && 
                                 EnemigosEliminados >= enemigosPorEliminar;
                break;
        }
        
        if (objetivoCumplido && !NivelCompletado)
        {
            CompletarNivel();
        }
    }
    
    void VerificarCondicionesDerrota()
    {
        // Las condiciones de derrota se manejan principalmente en GameManager
        // Aquí solo verificamos condiciones específicas del nivel
        
        // Ejemplo: Si el perseguidor alcanza al jugador sin que esté en furia
        if (perseguidor != null && jugador != null)
        {
            float distancia = Vector3.Distance(perseguidor.transform.position, jugador.transform.position);
            if (distancia < 1f && !jugador.EstaEnFuria)
            {
                // El jugador será eliminado por el trigger del perseguidor
            }
        }
    }
    
    public void CompletarNivel()
    {
        if (NivelCompletado) return;
        
        NivelCompletado = true;
        OnNivelCompletado?.Invoke();
        
        Debug.Log($"¡Nivel {nombreNivel} completado!");
    }
    
    public void FallarNivel()
    {
        if (NivelFallado) return;
        
        NivelFallado = true;
        OnNivelFallado?.Invoke();
        
        Debug.Log($"Nivel {nombreNivel} fallado.");
    }
    
    void RecolectarLlave()
    {
        LlavesRecolectadas++;
        OnLlaveRecolectada?.Invoke(LlavesRecolectadas);
        ActualizarObjetivo();
        
        Debug.Log($"Llave recolectada: {LlavesRecolectadas}/{llaveNecesarias}");
    }
    
    void EliminarEnemigo()
    {
        EnemigosEliminados++;
        OnEnemigoEliminado?.Invoke(EnemigosEliminados);
        ActualizarObjetivo();
        
        Debug.Log($"Enemigo eliminado: {EnemigosEliminados}/{enemigosPorEliminar}");
    }
    
    void ActivarPuntoControl()
    {
        PuntosControlActivados++;
        ActualizarObjetivo();
        
        Debug.Log($"Punto de control activado: {PuntosControlActivados}/{puntosControl.Count}");
    }
    
    void ManejarMuerteJugador()
    {
        if (respawnearJugadorAlMorir && puntosRespawn.Length > 0)
        {
            StartCoroutine(RespawnearJugador());
        }
        else
        {
            FallarNivel();
        }
    }
    
    void ManejarParedDestruida(FragileWall pared)
    {
        // Lógica adicional cuando se destruye una pared
        // Por ejemplo, abrir nuevos caminos o activar eventos
    }
    
    System.Collections.IEnumerator RespawnearJugador()
    {
        yield return new WaitForSeconds(tiempoRespawn);
        
        if (jugador != null && puntosRespawn.Length > 0)
        {
            // Elegir punto de respawn más cercano o aleatorio
            Transform puntoElegido = puntosRespawn[UnityEngine.Random.Range(0, puntosRespawn.Length)];
            jugador.transform.position = puntoElegido.position;
            
            Debug.Log("Jugador respawneado");
        }
    }
    
    void ActualizarObjetivo()
    {
        string objetivoTexto = "";
        
        switch (tipoObjetivo)
        {
            case TipoObjetivo.AlcanzarMeta:
                objetivoTexto = "Alcanza la meta";
                break;
            case TipoObjetivo.RecolectarLlaves:
                objetivoTexto = $"Recolecta llaves: {LlavesRecolectadas}/{llaveNecesarias}";
                break;
            case TipoObjetivo.EliminarEnemigos:
                objetivoTexto = $"Elimina enemigos: {EnemigosEliminados}/{enemigosPorEliminar}";
                break;
            case TipoObjetivo.ActivarPuntosControl:
                objetivoTexto = $"Activa puntos de control: {PuntosControlActivados}/{puntosControl.Count}";
                break;
            case TipoObjetivo.Supervivencia:
                objetivoTexto = "Sobrevive hasta que termine el tiempo";
                break;
            case TipoObjetivo.Combinado:
                objetivoTexto = $"Llaves: {LlavesRecolectadas}/{llaveNecesarias}, Enemigos: {EnemigosEliminados}/{enemigosPorEliminar}";
                break;
        }
        
        OnObjetivoActualizado?.Invoke(objetivoTexto);
    }
    
    void BuscarLlaves()
    {
        GameObject[] llavesEncontradas = GameObject.FindGameObjectsWithTag("Key");
        llaves.AddRange(llavesEncontradas);
        if (llaveNecesarias == 0)
            llaveNecesarias = llaves.Count;
    }
    
    void BuscarEnemigos()
    {
        GameObject[] enemigosEncontrados = GameObject.FindGameObjectsWithTag("Enemy");
        enemigos.AddRange(enemigosEncontrados);
        if (enemigosPorEliminar == 0)
            enemigosPorEliminar = enemigos.Count;
    }
    
    void BuscarPuntosControl()
    {
        GameObject[] puntosEncontrados = GameObject.FindGameObjectsWithTag("ControlPoint");
        puntosControl.AddRange(puntosEncontrados);
    }
    
    void MostrarEstadoNivel()
    {
        Debug.Log($"Estado del nivel - Llaves: {LlavesRecolectadas}/{llaveNecesarias}, " +
                 $"Enemigos: {EnemigosEliminados}/{enemigosPorEliminar}, " +
                 $"Puntos Control: {PuntosControlActivados}/{puntosControl.Count}");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Si este LevelManager está en el punto meta
        if (other.CompareTag("Player") && tipoObjetivo == TipoObjetivo.AlcanzarMeta)
        {
            CompletarNivel();
        }
    }
    
    void OnDestroy()
    {
        // Desuscribirse de eventos
        GameManager.OnJugadorMuerto -= ManejarMuerteJugador;
        FragileWall.OnParedDestruida -= ManejarParedDestruida;
    }
}

public enum TipoObjetivo
{
    AlcanzarMeta,
    RecolectarLlaves,
    EliminarEnemigos,
    ActivarPuntosControl,
    Supervivencia,
    Combinado
}

/// <summary>
/// Componente simple para llaves coleccionables
/// </summary>
public class LlaveColeccionable : MonoBehaviour
{
    public event Action OnLlaveRecolectada;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerFury"))
        {
            OnLlaveRecolectada?.Invoke();
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// Componente simple para puntos de control
/// </summary>
public class PuntoControl : MonoBehaviour
{
    public event Action OnPuntoActivado;
    private bool activado = false;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!activado && (other.CompareTag("Player") || other.CompareTag("PlayerFury")))
        {
            activado = true;
            OnPuntoActivado?.Invoke();
            
            // Cambiar visual para indicar activación
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
                sprite.color = Color.green;
        }
    }
} 