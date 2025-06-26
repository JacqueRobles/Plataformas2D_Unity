using UnityEngine;

public class EnemigoIAloco : MonoBehaviour
{
    public enum EstadoEnemigo { Patrullaje, Persecucion, Evasion }

    [Header("Configuración")]
    public float velocidad = 2f;
    public float velocidadEvasion = 4f;
    public float rangoDeteccion = 8f;
    public float rangoDeteccionMisil = 5f;
    public float velocidadPatrulla = 1.5f;
    public float radioPatrulla = 2f;
    public LayerMask capaMisiles = -1;

    private Transform jugador;
    private Rigidbody2D rb;

    private bool evadiendoMisil = false;
    private Vector2 direccionEvasion;
    private float tiempoInicioEvasion;
    private EstadoEnemigo estadoActual = EstadoEnemigo.Patrullaje;

    private float anguloPatrulla = 0f;
    private Vector2 centroPatrulla;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
            jugador = jugadorObj.transform;

        centroPatrulla = rb.position; // Start circular patrol at initial position
    }

    void FixedUpdate()
    {
        if (jugador == null) return;

        DetectarMisiles();
        ActualizarEstado();
        EjecutarComportamiento();
    }

    void DetectarMisiles()
    {
        Collider2D[] misiles = Physics2D.OverlapCircleAll(transform.position, rangoDeteccionMisil, capaMisiles);

        foreach (Collider2D misilCollider in misiles)
        {
            MisilTeledirigido misil = misilCollider.GetComponent<MisilTeledirigido>();
            if (misil != null)
            {
                Vector2 direccionMisil = misil.GetComponent<Rigidbody2D>().linearVelocity.normalized;
                Vector2 direccionAMi = (transform.position - misil.transform.position).normalized;

                if (Vector2.Dot(direccionMisil, direccionAMi) > 0.3f)
                {
                    IniciarEvasion(misil.transform.position);
                    break;
                }
            }
        }

        if (evadiendoMisil && Time.time > tiempoInicioEvasion + 2f)
        {
            evadiendoMisil = false;
        }
    }

    void IniciarEvasion(Vector3 posicionMisil)
    {
        if (!evadiendoMisil)
        {
            evadiendoMisil = true;
            tiempoInicioEvasion = Time.time;
            estadoActual = EstadoEnemigo.Evasion;

            Vector2 direccionMisil = (transform.position - posicionMisil).normalized;
            direccionEvasion = Vector2.Perpendicular(direccionMisil);
            if (Random.value > 0.5f)
                direccionEvasion = -direccionEvasion;
        }
    }

    void ActualizarEstado()
    {
        if (evadiendoMisil)
        {
            estadoActual = EstadoEnemigo.Evasion;
            return;
        }

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
        if (distanciaAlJugador <= rangoDeteccion)
            estadoActual = EstadoEnemigo.Persecucion;
        else
            estadoActual = EstadoEnemigo.Patrullaje;
    }

    void EjecutarComportamiento()
    {
        Vector2 direccionMovimiento;
        float velocidadMovimiento;

        switch (estadoActual)
        {
            case EstadoEnemigo.Evasion:
                direccionMovimiento = direccionEvasion;
                velocidadMovimiento = velocidadEvasion;
                break;

            case EstadoEnemigo.Persecucion:
                direccionMovimiento = (jugador.position - transform.position).normalized;
                velocidadMovimiento = velocidad;
                break;

            case EstadoEnemigo.Patrullaje:
                anguloPatrulla += velocidadPatrulla * Time.fixedDeltaTime;
                Vector2 nuevaPosicion = centroPatrulla + new Vector2(Mathf.Cos(anguloPatrulla), Mathf.Sin(anguloPatrulla)) * radioPatrulla;
                direccionMovimiento = (nuevaPosicion - rb.position).normalized;
                velocidadMovimiento = velocidadPatrulla;
                break;

            default:
                return;
        }

        rb.MovePosition(rb.position + direccionMovimiento * velocidadMovimiento * Time.fixedDeltaTime);

        float angulo = Mathf.Atan2(direccionMovimiento.y, direccionMovimiento.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angulo - 90f, Vector3.forward);
    }
}
