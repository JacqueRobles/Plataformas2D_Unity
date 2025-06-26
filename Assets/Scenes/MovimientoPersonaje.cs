using UnityEngine; 
using UnityEngine.InputSystem; 
public class MovimientoPersonaje : MonoBehaviour 
{ 
    public float velocidad = 5f; 
     
    private Rigidbody2D rb; 
    private Vector2 movimiento; 
     
    void Start() 
    { 
        rb = GetComponent<Rigidbody2D>(); 
    } 
     
    void Update() 
    { 
        movimiento = Vector2.zero; 
         
        if (Keyboard.current.wKey.isPressed) movimiento.y = 1; 
        if (Keyboard.current.sKey.isPressed) movimiento.y = -1; 
        if (Keyboard.current.aKey.isPressed) movimiento.x = -1; 
        if (Keyboard.current.dKey.isPressed) movimiento.x = 1; 
    } 
     
    void FixedUpdate() 
    { 
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime); 
    } 
}