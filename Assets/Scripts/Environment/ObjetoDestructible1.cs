using UnityEngine;
using System;

/// <summary>
/// Componente base para objetos que pueden ser destruidos o dañados
/// </summary>
public class ObjetoDestructible1 : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int vidaMaxima = 1;
    public bool regeneraVida = false;
    public float velocidadRegeneracion = 1f;
    
    [Header("Resistencias")]
    public bool resisteFuria = false;
    public bool resisteProyectiles = false;
    public bool resisteExplosiones = false;
    
    [Header("Efectos")]
    public GameObject efectoDestruccion;
    public GameObject efectoDanio;
    public AudioClip sonidoDestruccion;
    public AudioClip sonidoDanio;
    
    [Header("Recompensas")]
    public GameObject[] posiblesRecompensas;
    public float probabilidadRecompensa = 0.5f;
    
    // Estado
    public int VidaActual { get; private set; }
    public bool EstaDestruido { get; private set; } = false;
    public float PorcentajeVida => vidaMaxima > 0 ? (float)VidaActual / vidaMaxima : 0f;
    
    // Componentes
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private AudioSource audioSource;
    
    // Regeneración
    private float tiempoUltimoDanio;
    private float tiempoEsperaRegeneracion = 3f;
    
    // Eventos
    public static event Action<ObjetoDestructible1> OnObjetoDestruido;
    public static event Action<ObjetoDestructible1, int> OnObjetoDaniado;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        VidaActual = vidaMaxima;
    }
    
    void Update()
    {
        if (regeneraVida && !EstaDestruido && VidaActual < vidaMaxima)
        {
            ManejarRegeneracion();
        }
    }
    
    void ManejarRegeneracion()
    {
        if (Time.time >= tiempoUltimoDanio + tiempoEsperaRegeneracion)
        {
            float vidaRegenerada = velocidadRegeneracion * Time.deltaTime;
            float nuevaVida = Mathf.Min(VidaActual + vidaRegenerada, vidaMaxima);
            
            if (nuevaVida > VidaActual)
            {
                VidaActual = Mathf.RoundToInt(nuevaVida);
                ActualizarVisuales();
            }
        }
    }
    
    /// <summary>
    /// Aplicar daño al objeto
    /// </summary>
    /// <param name="cantidad">Cantidad de daño</param>
    /// <param name="tipoFuente">Tipo de fuente del daño</param>
    public void RecibirDanio(int cantidad, TipoDanio tipoFuente = TipoDanio.Generico)
    {
        if (EstaDestruido) return;
        
        // Verificar resistencias
        if (EsResistente(tipoFuente))
        {
            Debug.Log($"{gameObject.name} es resistente a {tipoFuente}");
            return;
        }
        
        // Aplicar daño
        VidaActual = Mathf.Max(0, VidaActual - cantidad);
        tiempoUltimoDanio = Time.time;
        
        // Efectos de daño
        CrearEfectoDanio();
        ReproducirSonido(sonidoDanio);
        
        // Notificar evento
        OnObjetoDaniado?.Invoke(this, cantidad);
        
        // Actualizar visuales
        ActualizarVisuales();
        
        Debug.Log($"{gameObject.name} recibió {cantidad} de daño. Vida: {VidaActual}/{vidaMaxima}");
        
        // Verificar si debe ser destruido
        if (VidaActual <= 0)
        {
            Destruir();
        }
    }
    
    bool EsResistente(TipoDanio tipoFuente)
    {
        switch (tipoFuente)
        {
            case TipoDanio.Furia:
                return resisteFuria;
            case TipoDanio.Proyectil:
                return resisteProyectiles;
            case TipoDanio.Explosion:
                return resisteExplosiones;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Destruir el objeto inmediatamente
    /// </summary>
    public void Destruir()
    {
        if (EstaDestruido) return;
        
        EstaDestruido = true;
        
        // Efectos de destrucción
        CrearEfectoDestruccion();
        ReproducirSonido(sonidoDestruccion);
        
        // Generar recompensas
        GenerarRecompensas();
        
        // Notificar evento
        OnObjetoDestruido?.Invoke(this);
        
        Debug.Log($"{gameObject.name} ha sido destruido");
        
        // Destruir el objeto
        Destroy(gameObject, 0.1f); // Pequeño delay para efectos
    }
    
    /// <summary>
    /// Curar el objeto
    /// </summary>
    /// <param name="cantidad">Cantidad de vida a restaurar</param>
    public void Curar(int cantidad)
    {
        if (EstaDestruido) return;
        
        int vidaAnterior = VidaActual;
        VidaActual = Mathf.Min(vidaMaxima, VidaActual + cantidad);
        
        if (VidaActual > vidaAnterior)
        {
            ActualizarVisuales();
            Debug.Log($"{gameObject.name} curado por {VidaActual - vidaAnterior}. Vida: {VidaActual}/{vidaMaxima}");
        }
    }
    
    /// <summary>
    /// Restaurar vida completa
    /// </summary>
    public void CurarCompleto()
    {
        Curar(vidaMaxima);
    }
    
    void CrearEfectoDanio()
    {
        if (efectoDanio != null)
        {
            GameObject efecto = Instantiate(efectoDanio, transform.position, Quaternion.identity);
            Destroy(efecto, 2f);
        }
    }
    
    void CrearEfectoDestruccion()
    {
        if (efectoDestruccion != null)
        {
            GameObject efecto = Instantiate(efectoDestruccion, transform.position, Quaternion.identity);
            Destroy(efecto, 3f);
        }
        else
        {
            // Efecto básico de destrucción
            CrearEfectoDestruccionBasico();
        }
    }
    
    void CrearEfectoDestruccionBasico()
    {
        // Crear fragmentos del sprite actual
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject fragmento = new GameObject("Fragmento");
                fragmento.transform.position = transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f;
                
                SpriteRenderer fragmentoSprite = fragmento.AddComponent<SpriteRenderer>();
                fragmentoSprite.sprite = spriteRenderer.sprite;
                fragmentoSprite.color = spriteRenderer.color;
                fragmento.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.2f, 0.4f);
                
                Rigidbody2D rbFragmento = fragmento.AddComponent<Rigidbody2D>();
                rbFragmento.linearVelocity = UnityEngine.Random.insideUnitCircle * 4f + Vector2.up * 3f;
                rbFragmento.angularVelocity = UnityEngine.Random.Range(-180f, 180f);
                
                Destroy(fragmento, 2f);
            }
        }
    }
    
    void GenerarRecompensas()
    {
        if (posiblesRecompensas.Length == 0) return;
        
        if (UnityEngine.Random.value <= probabilidadRecompensa)
        {
            GameObject recompensaElegida = posiblesRecompensas[UnityEngine.Random.Range(0, posiblesRecompensas.Length)];
            Vector3 posicionRecompensa = transform.position + Vector3.up * 0.5f;
            Instantiate(recompensaElegida, posicionRecompensa, Quaternion.identity);
        }
    }
    
    void ReproducirSonido(AudioClip clip)
    {
        if (clip != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
    }
    
    void ActualizarVisuales()
    {
        if (spriteRenderer != null)
        {
            // Cambiar transparencia basada en la vida
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(0.3f, 1f, PorcentajeVida);
            spriteRenderer.color = color;
            
            // Efecto de parpadeo cuando está dañado
            if (PorcentajeVida < 0.5f)
            {
                StartCoroutine(EfectoParpadeo());
            }
        }
    }
    
    System.Collections.IEnumerator EfectoParpadeo()
    {
        if (spriteRenderer == null) yield break;
        
        Color colorOriginal = spriteRenderer.color;
        Color colorDanio = Color.red;
        
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = colorDanio;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = colorOriginal;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Daño por jugador en furia
        if (other.CompareTag("PlayerFury"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.EstaEnFuria)
            {
                RecibirDanio(1, TipoDanio.Furia);
            }
        }
        
        // Daño por proyectiles
        if (other.CompareTag("Projectile"))
        {
            TurretProjectile proyectil = other.GetComponent<TurretProjectile>();
            if (proyectil != null)
            {
                RecibirDanio(1, TipoDanio.Proyectil);
            }
        }
        
        // Daño por explosiones
        if (other.CompareTag("Explosion"))
        {
            RecibirDanio(2, TipoDanio.Explosion);
        }
    }
    
    /// <summary>
    /// Configurar resistencias del objeto
    /// </summary>
    /// <param name="furia">Resistente a furia</param>
    /// <param name="proyectiles">Resistente a proyectiles</param>
    /// <param name="explosiones">Resistente a explosiones</param>
    public void ConfigurarResistencias(bool furia, bool proyectiles, bool explosiones)
    {
        resisteFuria = furia;
        resisteProyectiles = proyectiles;
        resisteExplosiones = explosiones;
    }
    
    void OnDrawGizmosSelected()
    {
        // Mostrar información de vida
        if (Application.isPlaying)
        {
            Gizmos.color = Color.Lerp(Color.red, Color.green, PorcentajeVida);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }
        
        // Mostrar resistencias
        Vector3 offset = Vector3.up * 1.5f;
        if (resisteFuria)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + offset, 0.1f);
        }
        if (resisteProyectiles)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + offset + Vector3.right * 0.3f, Vector3.one * 0.1f);
        }
        if (resisteExplosiones)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Color naranja
            Gizmos.DrawWireSphere(transform.position + offset + Vector3.left * 0.3f, 0.15f);
        }
    }
}

public enum TipoDanio
{
    Generico,
    Furia,
    Proyectil,
    Explosion
} 