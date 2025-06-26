using UnityEngine;
using UnityEngine.InputSystem;

public class movimiento2Dloco : MonoBehaviour
{
    [Header("Configuración de Nave")]
    public float velocidadMaxima = 8f;
    public float aceleracion = 15f;
    public float desaceleracion = 10f;
    public float suavidadRotacion = 5f;
    
    private Rigidbody2D rb;
    private Vector2 velocidadActual;
    private bool estaMoviendo = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.angularDamping = 5f;
        rb.linearDamping = 0f;
    }

    void Update()
    {
        Vector2 movimiento = Vector2.zero;
        
        // WASD y Flechas
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            movimiento.y = 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            movimiento.y = -1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            movimiento.x = 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            movimiento.x = -1;
        
        movimiento = movimiento.normalized;
        estaMoviendo = movimiento.magnitude > 0.1f;
        
        // Actualizar velocidad
        if (estaMoviendo)
        {
            velocidadActual = Vector2.MoveTowards(velocidadActual, movimiento * velocidadMaxima, 
                                                aceleracion * Time.deltaTime);
        }
        else
        {
            velocidadActual = Vector2.MoveTowards(velocidadActual, Vector2.zero, 
                                                desaceleracion * Time.deltaTime);
        }
        
        rb.linearVelocity = velocidadActual;
        
        // Rotación hacia movimiento
        if (velocidadActual.magnitude > 0.1f)
        {
            float angulo = Mathf.Atan2(velocidadActual.y, velocidadActual.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotacionObjetivo = Quaternion.AngleAxis(angulo, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, 
                                                suavidadRotacion * Time.deltaTime);
        }
    }
}