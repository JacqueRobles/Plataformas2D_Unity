using UnityEngine; 
 
public class EnemigoIA : MonoBehaviour 
{ 
    public float velocidad = 2f; 
     
    private Transform jugador; 
    private Rigidbody2D rb; 
     
    void Start() 
    { 
        rb = GetComponent<Rigidbody2D>(); 
        jugador = GameObject.Find("Jugador").transform; 
    } 
     
    void FixedUpdate() 
    { 
        if (jugador != null) 
        { 
            Vector2 direccion = (jugador.position - transform.position).normalized; 
            rb.MovePosition(rb.position + direccion * velocidad * Time.fixedDeltaTime); 
        } 
    } 
} 