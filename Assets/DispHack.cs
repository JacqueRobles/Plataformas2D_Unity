using UnityEngine;

public class DisparoEnemigo : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject prefabProyectil;
    public float velocidadProyectil = 15f;
    public Transform puntoDisparo;
    public float tiempoRecargaDisparo = 1.5f;
    public float rangoDisparo = 10f;

    private float tiempoUltimoDisparo;
    private Collider2D miCollider;
    private Transform objetivoJugador;

    void Start()
    {
        // Obtener el collider del enemigo
        miCollider = GetComponent<Collider2D>();

        // Si no se asignó un punto de disparo, usar la posición del objeto
        if (puntoDisparo == null)
        {
            puntoDisparo = transform;
        }

        // Buscar al jugador automáticamente (usa el tag "Player")
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            objetivoJugador = jugador.transform;
        }
    }

    void Update()
    {
        if (objetivoJugador != null && Time.time > tiempoUltimoDisparo + tiempoRecargaDisparo)
        {
            float distancia = Vector2.Distance(transform.position, objetivoJugador.position);
            if (distancia <= rangoDisparo)
            {
                DispararHaciaJugador();
            }
        }
    }

    void DispararHaciaJugador()
    {
        tiempoUltimoDisparo = Time.time;

        GameObject proyectil = Instantiate(prefabProyectil, puntoDisparo.position, Quaternion.identity);

        Rigidbody2D rbProyectil = proyectil.GetComponent<Rigidbody2D>();
        if (rbProyectil != null && objetivoJugador != null)
        {
            Vector2 direccion = (objetivoJugador.position - puntoDisparo.position).normalized;
            rbProyectil.linearVelocity = direccion * velocidadProyectil;
        }

        Collider2D proyectilCollider = proyectil.GetComponent<Collider2D>();
        if (proyectilCollider != null && miCollider != null)
        {
            Physics2D.IgnoreCollision(proyectilCollider, miCollider, true);
        }

        Destroy(proyectil, 3f);
    }
}
