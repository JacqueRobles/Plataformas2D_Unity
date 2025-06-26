using UnityEngine;
using UnityEngine.InputSystem;

public class DisparoScript : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject prefabProyectil;
    public float velocidadProyectil = 15f;
    public Transform puntoDisparo;
    public float tiempoRecargaDisparo = 0.5f;
    
    private movimiento2D controlMovimiento;
    private float tiempoUltimoDisparo;
    private Collider2D miCollider;
    
    void Start()
    {
        // Obtener referencia al script de movimiento
        controlMovimiento = GetComponent<movimiento2D>();
        
        // Obtener el collider del personaje
        miCollider = GetComponent<Collider2D>();
        
        // Si no se asignó un punto de disparo, usar la posición del objeto
        if (puntoDisparo == null)
        {
            puntoDisparo = transform;
        }
    }
    
    void Update()
    {
        // Disparar con la tecla espacio
        if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.time > tiempoUltimoDisparo + tiempoRecargaDisparo)
        {
            Disparar();
        }
    }
    
    void Disparar()
    {
        if (prefabProyectil != null && controlMovimiento != null)
        {
            tiempoUltimoDisparo = Time.time;
            
            // Obtener dirección del movimiento
            float direccion = controlMovimiento.ObtenerDireccion();
            
            // Crear proyectil
            GameObject proyectil = Instantiate(prefabProyectil, puntoDisparo.position, Quaternion.identity);
            
            // Añadir velocidad en la dirección adecuada
            Rigidbody2D rbProyectil = proyectil.GetComponent<Rigidbody2D>();
            if (rbProyectil != null)
            {
                rbProyectil.linearVelocity = new Vector2(direccion * velocidadProyectil, 0);
            }
            
            // Ignorar colisión entre el proyectil y el jugador
            Collider2D proyectilCollider = proyectil.GetComponent<Collider2D>();
            if (proyectilCollider != null && miCollider != null)
            {
                Physics2D.IgnoreCollision(proyectilCollider, miCollider, true);
            }
            
            // Destruir después de 3 segundos por seguridad
            Destroy(proyectil, 3f);
        }
    }
} 