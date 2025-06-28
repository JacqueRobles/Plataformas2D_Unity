using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Controlador principal del jugador que maneja movimiento, habilidades y estados
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 8f;
    public float fuerzaSalto = 16f;
    public float gravedadPersonalizada = 25f;
    public float velocidadCaida = 20f;
    
    [Header("Salto en Paredes")]
    public float fuerzaSaltoParedes = 14f;
    public float tiempoSaltoParedes = 0.15f;
    public LayerMask capasParedes = -1;
    
    [Header("Dash")]
    public float fuerzaDash = 24f;
    public float duracionDash = 0.2f;
    public float cooldownDash = 1f;
    
    [Header("Estado de Furia")]
    public float duracionFuria = 5f;
    public float cooldownFuria = 10f;
    public Color colorFuria = Color.red;
    
    [Header("Detección de Suelo")]
    public Transform verificadorSuelo;
    public float radioVerificacionSuelo = 0.2f;
    public LayerMask capasSuelo = -1;
    
    // Componentes
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;
    
    // Estados
    public bool EstaEnSuelo { get; private set; }
    public bool EstaTocandoPared { get; private set; }
    public bool EstaEnFuria { get; private set; }
    public bool PuedeDash { get; private set; } = true;
    public bool PuedeFuria { get; private set; } = true;
    
    // Dashes adicionales para power-ups
    private int dashesAdicionales = 0;
    
    // Controles
    private Vector2 inputMovimiento;
    private bool inputSalto;
    private bool inputDash;
    private bool inputFuria;
    private bool inputAgacharse;
    
    // Timers
    private float timerSaltoParedes;
    private float timerDash;
    private float timerCooldownDash;
    private float timerFuria;
    private float timerCooldownFuria;
    
    // Direcciones
    private int direccionPared;
    private bool mirandoDerecha = true;
    
    // Eventos
    public static event Action OnJugadorMuerto;
    public static event Action OnDashRealizado;
    public static event Action OnSaltoRealizado;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colorOriginal = spriteRenderer.color;
        
        // Configurar física personalizada
        rb.gravityScale = 0f; // Usaremos gravedad personalizada
    }
    
    void Start()
    {
        // Suscribirse a eventos del GameManager
        GameManager.OnFuriaActivada += ActivarFuria;
        GameManager.OnFuriaDesactivada += DesactivarFuria;
    }
    
    void Update()
    {
        LeerInputs();
        VerificarSuelo();
        VerificarParedes();
        ActualizarTimers();
        ManejarEstados();
    }
    
    void FixedUpdate()
    {
        AplicarMovimiento();
        AplicarGravedad();
    }
    
    void LeerInputs()
    {
        // Usando el nuevo Input System
        inputMovimiento.x = 0;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            inputMovimiento.x = -1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            inputMovimiento.x = 1;
            
        inputSalto = Keyboard.current.spaceKey.wasPressedThisFrame;
        inputDash = Keyboard.current.leftShiftKey.wasPressedThisFrame;
        inputFuria = Keyboard.current.fKey.wasPressedThisFrame;
        inputAgacharse = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
    }
    
    void VerificarSuelo()
    {
        EstaEnSuelo = Physics2D.OverlapCircle(verificadorSuelo.position, radioVerificacionSuelo, capasSuelo);
    }
    
    void VerificarParedes()
    {
        // Verificar pared derecha
        RaycastHit2D hitDerecha = Physics2D.Raycast(transform.position, Vector2.right, 0.7f, capasParedes);
        // Verificar pared izquierda
        RaycastHit2D hitIzquierda = Physics2D.Raycast(transform.position, Vector2.left, 0.7f, capasParedes);
        
        if (hitDerecha.collider != null)
        {
            EstaTocandoPared = true;
            direccionPared = 1;
        }
        else if (hitIzquierda.collider != null)
        {
            EstaTocandoPared = true;
            direccionPared = -1;
        }
        else
        {
            EstaTocandoPared = false;
        }
    }
    
    void ActualizarTimers()
    {
        // Timer salto en paredes
        if (timerSaltoParedes > 0)
            timerSaltoParedes -= Time.deltaTime;
            
        // Timer dash
        if (timerDash > 0)
            timerDash -= Time.deltaTime;
            
        // Timer cooldown dash
        if (timerCooldownDash > 0)
        {
            timerCooldownDash -= Time.deltaTime;
            if (timerCooldownDash <= 0)
                PuedeDash = true;
        }
        
        // Timer furia
        if (timerFuria > 0)
        {
            timerFuria -= Time.deltaTime;
            if (timerFuria <= 0)
            {
                DesactivarFuria();
            }
        }
        
        // Timer cooldown furia
        if (timerCooldownFuria > 0)
        {
            timerCooldownFuria -= Time.deltaTime;
            if (timerCooldownFuria <= 0)
                PuedeFuria = true;
        }
    }
    
    void ManejarEstados()
    {
        // Manejar salto
        if (inputSalto)
        {
            if (EstaEnSuelo)
            {
                Saltar();
            }
            else if (EstaTocandoPared && timerSaltoParedes <= 0)
            {
                SaltarEnPared();
            }
        }
        
        // Manejar dash
        if (inputDash && (PuedeDash || dashesAdicionales > 0) && timerDash <= 0)
        {
            RealizarDash();
        }
        
        // Manejar furia
        if (inputFuria && PuedeFuria && !EstaEnFuria)
        {
            GameManager.Instance.ActivarFuria();
        }
        
        // Manejar agacharse
        ManejarAgacharse();
    }
    
    void AplicarMovimiento()
    {
        // Solo aplicar movimiento horizontal si no estamos en dash
        if (timerDash <= 0)
        {
            float velocidadX = inputMovimiento.x * velocidadMovimiento;
            rb.linearVelocity = new Vector2(velocidadX, rb.linearVelocity.y);
            
            // Voltear sprite
            if (inputMovimiento.x > 0 && !mirandoDerecha)
                Voltear();
            else if (inputMovimiento.x < 0 && mirandoDerecha)
                Voltear();
        }
    }
    
    void AplicarGravedad()
    {
        if (!EstaEnSuelo && timerDash <= 0)
        {
            float gravedad = rb.linearVelocity.y > 0 ? gravedadPersonalizada : gravedadPersonalizada * 1.5f;
            rb.linearVelocity += Vector2.down * gravedad * Time.fixedDeltaTime;
            
            // Limitar velocidad de caída
            if (rb.linearVelocity.y < -velocidadCaida)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -velocidadCaida);
        }
    }
    
    void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        OnSaltoRealizado?.Invoke();
    }
    
    void SaltarEnPared()
    {
        // Saltar en dirección opuesta a la pared
        Vector2 fuerzaSalto = new Vector2(-direccionPared * fuerzaSaltoParedes * 0.7f, fuerzaSaltoParedes);
        rb.linearVelocity = fuerzaSalto;
        
        timerSaltoParedes = tiempoSaltoParedes;
        OnSaltoRealizado?.Invoke();
    }
    
    void RealizarDash()
    {
        Vector2 direccionDash = inputMovimiento.x != 0 ? new Vector2(inputMovimiento.x, 0) : 
                               mirandoDerecha ? Vector2.right : Vector2.left;
        
        rb.linearVelocity = direccionDash * fuerzaDash;
        
        timerDash = duracionDash;
        
        // Usar dash adicional si está disponible, sino usar cooldown normal
        if (dashesAdicionales > 0)
        {
            dashesAdicionales--;
        }
        else
        {
            timerCooldownDash = cooldownDash;
            PuedeDash = false;
        }
        
        OnDashRealizado?.Invoke();
    }
    
    void ManejarAgacharse()
    {
        if (inputAgacharse)
        {
            // Reducir collider para agacharse
            col.size = new Vector2(col.size.x, 1f);
            col.offset = new Vector2(col.offset.x, -0.5f);
        }
        else
        {
            // Restaurar collider normal
            col.size = new Vector2(col.size.x, 2f);
            col.offset = new Vector2(col.offset.x, 0f);
        }
    }
    
    void ActivarFuria()
    {
        EstaEnFuria = true;
        timerFuria = duracionFuria;
        timerCooldownFuria = cooldownFuria;
        PuedeFuria = false;
        
        // Cambiar color visual
        spriteRenderer.color = colorFuria;
        
        // Cambiar tag para interacción con paredes rompibles
        gameObject.tag = "PlayerFury";
    }
    
    void DesactivarFuria()
    {
        EstaEnFuria = false;
        
        // Restaurar color original
        spriteRenderer.color = colorOriginal;
        
        // Restaurar tag normal
        gameObject.tag = "Player";
        
        GameManager.Instance.DesactivarFuria();
    }
    
    void Voltear()
    {
        mirandoDerecha = !mirandoDerecha;
        transform.Rotate(0, 180, 0);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Detectar objetos mortales
        if (other.CompareTag("Deadly"))
        {
            Morir();
        }
        
        // Detectar meta del nivel
        if (other.CompareTag("Goal"))
        {
            // El LevelManager manejará la lógica de completar nivel
        }
    }
    
    void Morir()
    {
        OnJugadorMuerto?.Invoke();
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualizar verificador de suelo
        if (verificadorSuelo != null)
        {
            Gizmos.color = EstaEnSuelo ? Color.green : Color.red;
            Gizmos.DrawWireSphere(verificadorSuelo.position, radioVerificacionSuelo);
        }
        
        // Visualizar detección de paredes
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector2.right * 0.7f);
        Gizmos.DrawRay(transform.position, Vector2.left * 0.7f);
    }
    
    void OnDestroy()
    {
        // Desuscribirse de eventos
        GameManager.OnFuriaActivada -= ActivarFuria;
        GameManager.OnFuriaDesactivada -= DesactivarFuria;
    }
    
    // Métodos públicos para power-ups
    
    /// <summary>
    /// Otorga un dash adicional que se puede usar sin cooldown
    /// </summary>
    public void OtorgarDashAdicional()
    {
        dashesAdicionales++;
    }
    
    /// <summary>
    /// Extiende la duración actual del estado de furia
    /// </summary>
    /// <param name="tiempoExtra">Tiempo adicional en segundos</param>
    public void ExtenderFuria(float tiempoExtra)
    {
        if (EstaEnFuria)
        {
            timerFuria += tiempoExtra;
        }
    }
    
    /// <summary>
    /// Reduce el cooldown restante de furia
    /// </summary>
    /// <param name="reduccion">Tiempo a reducir en segundos</param>
    public void ReducirCooldownFuria(float reduccion)
    {
        timerCooldownFuria = Mathf.Max(0, timerCooldownFuria - reduccion);
        if (timerCooldownFuria <= 0)
        {
            PuedeFuria = true;
        }
    }
} 