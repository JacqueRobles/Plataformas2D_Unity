using UnityEngine;

/// <summary>
/// Proyectil disparado por las torretas que puede destruir paredes frágiles
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class TurretProjectile : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 10f;
    public float tiempoVida = 5f;
    public int danio = 1;
    
    [Header("Destrucción")]
    public bool destruyeParedesFragiles = true;
    public bool afectaJugador = false; // Por defecto no daña al jugador
    public LayerMask capasDestruccion = -1;
    
    [Header("Efectos")]
    public GameObject efectoExplosion;
    public AudioClip sonidoExplosion;
    public float radioExplosion = 1f;
    
    // Componentes
    private Rigidbody2D rb;
    private AudioSource audioSource;
    
    // Estado
    private bool haExplotado = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        
        // Configurar como proyectil
        gameObject.tag = "Projectile";
        
        // Configurar física
        rb.gravityScale = 0f; // Sin gravedad para trayectoria recta
    }
    
    void Start()
    {
        // Autodestruir después del tiempo de vida
        Destroy(gameObject, tiempoVida);
    }
    
    /// <summary>
    /// Configurar el proyectil con velocidad y dirección específicas
    /// </summary>
    /// <param name="velocidadProyectil">Velocidad del proyectil</param>
    /// <param name="direccion">Dirección normalizada del movimiento</param>
    public void ConfigurarProyectil(float velocidadProyectil, Vector2 direccion)
    {
        velocidad = velocidadProyectil;
        rb.linearVelocity = direccion.normalized * velocidad;
        
        // Rotar el proyectil hacia la dirección de movimiento
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (haExplotado) return;
        
        // Verificar colisión con jugador
        if (other.CompareTag("Player") || other.CompareTag("PlayerFury"))
        {
            if (afectaJugador)
            {
                // El jugador morirá por contacto, el PlayerController maneja esto
                Explotar();
            }
            return; // No explotar si no afecta al jugador
        }
        
        // Verificar colisión con paredes normales
        if (other.CompareTag("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            Explotar();
            return;
        }
        
        // Verificar colisión con paredes frágiles
        if (destruyeParedesFragiles && other.CompareTag("FragileWall"))
        {
            FragileWall paredFragil = other.GetComponent<FragileWall>();
            if (paredFragil != null && !paredFragil.EstaDestruida)
            {
                paredFragil.Destruir("Proyectil de Torreta");
            }
            Explotar();
            return;
        }
        
        // Verificar otras capas de destrucción
        int capaObjeto = 1 << other.gameObject.layer;
        if ((capasDestruccion.value & capaObjeto) != 0)
        {
            // Intentar destruir objeto
            DestruirObjeto(other.gameObject);
            Explotar();
        }
    }
    
    void DestruirObjeto(GameObject objeto)
    {
        // Verificar si el objeto tiene componente destructible
        ObjetoDestructible1 destructible = objeto.GetComponent<ObjetoDestructible1>();
        if (destructible != null)
        {
            destructible.RecibirDanio(danio);
        }
        else
        {
            // Si no tiene componente destructible, intentar destruir directamente
            Destroy(objeto);
        }
    }
    
    public void Explotar()
    {
        if (haExplotado) return;
        haExplotado = true;
        
        // Crear efecto de explosión
        CrearEfectoExplosion();
        
        // Reproducir sonido
        ReproducirSonidoExplosion();
        
        // Crear área de explosión para afectar objetos cercanos
        CrearAreaExplosion();
        
        // Destruir el proyectil
        Destroy(gameObject, 0.1f); // Pequeño delay para que se reproduzcan los efectos
    }
    
    void CrearEfectoExplosion()
    {
        if (efectoExplosion != null)
        {
            GameObject efecto = Instantiate(efectoExplosion, transform.position, Quaternion.identity);
            Destroy(efecto, 3f);
        }
        else
        {
            // Efecto básico con partículas
            CrearEfectoBasico();
        }
    }
    
    void CrearEfectoBasico()
    {
        // Crear pequeñas partículas de explosión
        for (int i = 0; i < 6; i++)
        {
            GameObject particula = new GameObject("Particula");
            particula.transform.position = transform.position;
            
            SpriteRenderer sprite = particula.AddComponent<SpriteRenderer>();
            sprite.color = new Color(1f, 0.5f, 0f, 1f); // Color naranja
            sprite.sprite = Resources.Load<Sprite>("Sprites/DefaultSprite"); // Sprite básico
            
            particula.transform.localScale = Vector3.one * 0.2f;
            
            Rigidbody2D rbParticula = particula.AddComponent<Rigidbody2D>();
            rbParticula.linearVelocity = UnityEngine.Random.insideUnitCircle * 5f;
            rbParticula.angularVelocity = UnityEngine.Random.Range(-360f, 360f);
            
            Destroy(particula, 1f);
        }
    }
    
    void ReproducirSonidoExplosion()
    {
        if (sonidoExplosion != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(sonidoExplosion);
            }
            else
            {
                AudioSource.PlayClipAtPoint(sonidoExplosion, transform.position);
            }
        }
    }
    
    void CrearAreaExplosion()
    {
        if (radioExplosion <= 0) return;
        
        // Crear trigger temporal para área de explosión
        GameObject areaExplosion = new GameObject("AreaExplosion");
        areaExplosion.transform.position = transform.position;
        areaExplosion.tag = "Explosion";
        
        CircleCollider2D colliderExplosion = areaExplosion.AddComponent<CircleCollider2D>();
        colliderExplosion.radius = radioExplosion;
        colliderExplosion.isTrigger = true;
        
        // Destruir área de explosión después de un frame
        Destroy(areaExplosion, 0.1f);
        
        // Buscar objetos en el área de explosión
        Collider2D[] objetosAfectados = Physics2D.OverlapCircleAll(transform.position, radioExplosion);
        
        foreach (Collider2D objeto in objetosAfectados)
        {
            // Destruir paredes frágiles en el área
            if (objeto.CompareTag("FragileWall"))
            {
                FragileWall paredFragil = objeto.GetComponent<FragileWall>();
                if (paredFragil != null && !paredFragil.EstaDestruida && 
                    paredFragil.PuedeSerDestruida(TipoDestruccion.Explosion))
                {
                    paredFragil.Destruir("Explosión de Proyectil");
                }
            }
            
            // Afectar otros objetos destructibles
            ObjetoDestructible1 destructible = objeto.GetComponent<ObjetoDestructible1>();
            if (destructible != null)
            {
                destructible.RecibirDanio(danio);
            }
        }
    }
    
    /// <summary>
    /// Configurar si el proyectil debe afectar al jugador
    /// </summary>
    /// <param name="debeAfectar">True si debe dañar al jugador</param>
    public void ConfigurarAfectaJugador(bool debeAfectar)
    {
        afectaJugador = debeAfectar;
    }
    
    /// <summary>
    /// Configurar el daño del proyectil
    /// </summary>
    /// <param name="nuevoDanio">Nuevo valor de daño</param>
    public void ConfigurarDanio(int nuevoDanio)
    {
        danio = Mathf.Max(0, nuevoDanio);
    }
    
    void OnDrawGizmosSelected()
    {
        // Mostrar radio de explosión
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
        
        // Mostrar dirección de movimiento
        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 2f);
        }
    }
} 