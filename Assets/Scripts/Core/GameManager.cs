using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Gestor principal del juego que controla estados, eventos y configuración global
/// </summary>
public class GameManager : MonoBehaviour
{
    public float tiempoLimiteNivel = 300f; // 5 minutos por nivel
    public int vidasIniciales = 3;
    public bool modoDebug = false;
    
    // Singleton
    public static GameManager Instance { get; private set; }
    
    // Estado del juego
    public GameState EstadoActual { get; private set; } = GameState.Menu;
    public int VidasActuales { get; private set; }
    public float TiempoRestante { get; private set; }
    public int NivelActual { get; private set; } = 1;
    
    // Eventos globales
    public static event Action<GameState> OnCambioEstado;
    public static event Action<int> OnCambioVidas;
    public static event Action<float> OnCambioTiempo;
    public static event Action OnGameOver;
    public static event Action OnNivelCompletado;
    public static event Action OnJugadorMuerto;
    public static event Action OnFuriaActivada;
    public static event Action OnFuriaDesactivada;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InicializarJuego();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        if (EstadoActual == GameState.Jugando)
        {
            ActualizarTiempo();
        }
    }
    
    void InicializarJuego()
    {
        VidasActuales = vidasIniciales;
        TiempoRestante = tiempoLimiteNivel;
        
        // Suscribirse a eventos
        PlayerController.OnJugadorMuerto += ManejarMuerteJugador;
        LevelManager.OnNivelCompletado += ManejarNivelCompletado;
    }
    
    void ActualizarTiempo()
    {
        TiempoRestante -= Time.deltaTime;
        OnCambioTiempo?.Invoke(TiempoRestante);
        
        if (TiempoRestante <= 0)
        {
            GameOver();
        }
    }
    
    public void CambiarEstado(GameState nuevoEstado)
    {
        EstadoActual = nuevoEstado;
        OnCambioEstado?.Invoke(nuevoEstado);
        
        Debug.Log($"Estado cambiado a: {nuevoEstado}");
    }
    
    public void IniciarJuego()
    {
        CambiarEstado(GameState.Jugando);
        TiempoRestante = tiempoLimiteNivel;
    }
    
    public void PausarJuego()
    {
        Time.timeScale = 0f;
        CambiarEstado(GameState.Pausado);
    }
    
    public void ReanudarJuego()
    {
        Time.timeScale = 1f;
        CambiarEstado(GameState.Jugando);
    }
    
    void ManejarMuerteJugador()
    {
        VidasActuales--;
        OnCambioVidas?.Invoke(VidasActuales);
        OnJugadorMuerto?.Invoke();
        
        if (VidasActuales <= 0)
        {
            GameOver();
        }
        else
        {
            // Reiniciar nivel
            ReiniciarNivel();
        }
    }
    
    void ManejarNivelCompletado()
    {
        OnNivelCompletado?.Invoke();
        NivelActual++;
        
        // Cargar siguiente nivel o mostrar victoria
        if (NivelActual <= 3) // Asumiendo 3 niveles
        {
            CargarSiguienteNivel();
        }
        else
        {
            Victoria();
        }
    }
    
    public void GameOver()
    {
        CambiarEstado(GameState.GameOver);
        OnGameOver?.Invoke();
        Time.timeScale = 0f;
    }
    
    void Victoria()
    {
        CambiarEstado(GameState.Victoria);
        Time.timeScale = 0f;
    }
    
    void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    void CargarSiguienteNivel()
    {
        TiempoRestante = tiempoLimiteNivel;
        SceneManager.LoadScene($"Nivel{NivelActual}");
    }
    
    public void ActivarFuria()
    {
        OnFuriaActivada?.Invoke();
    }
    
    public void DesactivarFuria()
    {
        OnFuriaDesactivada?.Invoke();
    }
    
    void OnDestroy()
    {
        // Desuscribirse de eventos
        PlayerController.OnJugadorMuerto -= ManejarMuerteJugador;
        LevelManager.OnNivelCompletado -= ManejarNivelCompletado;
    }
}

public enum GameState
{
    Menu,
    Jugando,
    Pausado,
    GameOver,
    Victoria
} 