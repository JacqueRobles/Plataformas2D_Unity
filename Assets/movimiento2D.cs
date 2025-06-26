using UnityEngine;
using UnityEngine.InputSystem;

public class movimiento2D : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 5f;
    
    [Header("Configuración de Gravedad")]
    public bool usarGravedad = true;
    public float gravedadPersonalizada = 1f;
    public float fuerzaSalto = 5f;
    
    private Rigidbody2D rb;
    private Vector2 movimiento;
    private float ultimaDireccionHorizontal = 1f; // 1 = derecha, -1 = izquierda
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Configurar gravedad
        if (!usarGravedad)
        {
            rb.gravityScale = 0;
        }
        else
        {
            rb.gravityScale = gravedadPersonalizada;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Capturar entrada del usuario usando el nuevo Input System
        Vector2 inputVector = Vector2.zero;
        
        // Teclas de flecha
        if (Keyboard.current.upArrowKey.isPressed)
            inputVector.y = 1;
        else if (Keyboard.current.downArrowKey.isPressed)
            inputVector.y = -1;
            
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            inputVector.x = 1;
            ultimaDireccionHorizontal = 1f;
        }
        else if (Keyboard.current.leftArrowKey.isPressed)
        {
            inputVector.x = -1;
            ultimaDireccionHorizontal = -1f;
        }
        
        // Teclas WASD
        if (Keyboard.current.wKey.isPressed)
            inputVector.y = 1;
        else if (Keyboard.current.sKey.isPressed)
            inputVector.y = -1;
            
        if (Keyboard.current.dKey.isPressed)
        {
            inputVector.x = 1;
            ultimaDireccionHorizontal = 1f;
        }
        else if (Keyboard.current.aKey.isPressed)
        {
            inputVector.x = -1;
            ultimaDireccionHorizontal = -1f;
        }
        
        // Normalizar el vector de movimiento horizontal (mantener Y para salto)
        if (usarGravedad)
        {
            movimiento.x = inputVector.x;
            
            // Salto
            if (inputVector.y > 0 && EstaEnSuelo())
            {
                rb.AddForce(new Vector2(0, fuerzaSalto), ForceMode2D.Impulse);
            }
        }
        else
        {
            // Modo sin gravedad - movimiento libre
            movimiento = inputVector.normalized;
        }
        
        // Cambiar gravedad con G
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            CambiarGravedad();
        }
    }
    
    void FixedUpdate()
    {
        // Si usamos gravedad, solo aplicamos movimiento horizontal
        if (usarGravedad)
        {
            rb.linearVelocity = new Vector2(movimiento.x * velocidad, rb.linearVelocity.y);
        }
        else
        {
            // Sin gravedad, movimiento completo
            rb.linearVelocity = movimiento * velocidad;
        }
    }
    
    bool EstaEnSuelo()
    {
    float extraHeight = 0.05f;
    RaycastHit2D hit = Physics2D.Raycast(
        new Vector2(transform.position.x, transform.position.y - GetComponent<Collider2D>().bounds.extents.y),
        Vector2.down,
        extraHeight
    );

    return hit.collider != null;
    }

    
    void CambiarGravedad()
    {
        usarGravedad = !usarGravedad;
        
        if (usarGravedad)
        {
            rb.gravityScale = gravedadPersonalizada;
        }
        else
        {
            rb.gravityScale = 0;
        }
    }
    
    // Método público para obtener la última dirección horizontal
    public float ObtenerDireccion()
    {
        return ultimaDireccionHorizontal;
    }
}
