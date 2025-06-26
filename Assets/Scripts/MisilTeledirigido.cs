using UnityEngine;

public class MisilTeledirigido : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidadInicial = 8f;
    public float velocidadMaxima = 15f;
    public float fuerzaGiro = 200f;
    public float rangoExplosion = 1.5f;
    public int danioExplosion = 50;
    
    private Transform target;
    private Rigidbody2D rb;
    private float velocidadActual;
    private bool siguiendoTarget = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        velocidadActual = velocidadInicial;
        rb.linearVelocity = transform.up * velocidadActual;
        Destroy(gameObject, 8f);
    }
    
    void FixedUpdate()
    {
        ActualizarMovimiento();
        VerificarImpacto();
    }
    
    public void EstablecerTarget(Transform nuevoTarget)
    {
        target = nuevoTarget;
        siguiendoTarget = true;
    }
    
    void ActualizarMovimiento()
    {
        velocidadActual = Mathf.MoveTowards(velocidadActual, velocidadMaxima, 5f * Time.fixedDeltaTime);
        
        if (siguiendoTarget && target != null)
        {
            Vector2 direccionAlTarget = (target.position - transform.position).normalized;
            Vector2 nuevaDireccion = Vector2.Lerp(rb.linearVelocity.normalized, direccionAlTarget, 
                                                  fuerzaGiro * Time.fixedDeltaTime / rb.linearVelocity.magnitude);
            
            rb.linearVelocity = nuevaDireccion * velocidadActual;
            
            float angulo = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angulo - 90f, Vector3.forward);
        }
    }
    
    void VerificarImpacto()
    {
        if (target != null)
        {
            float distancia = Vector2.Distance(transform.position, target.position);
            if (distancia <= rangoExplosion)
            {
                Explotar();
                return;
            }
        }
    }
    
    void Explotar()
    {
        Collider2D[] objetosAfectados = Physics2D.OverlapCircleAll(transform.position, rangoExplosion);
        
        foreach (Collider2D objeto in objetosAfectados)
        {
            ObjetoDestructibleloco destructible = objeto.GetComponent<ObjetoDestructibleloco>();
            if (destructible != null)
                destructible.RecibirDanio(danioExplosion);
        }
        
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != gameObject.layer)
            Explotar();
    }
}