using UnityEngine; 
using UnityEngine.InputSystem; 
 
public class SistemaAtaque : MonoBehaviour 
{ 
    public GameObject espada; 
    public float duracion = 0.5f; 
    public float velocidadGiro = 720f; 
     
    private bool atacando = false; 
    private float tiempo = 0f; 
    private Collider2D espadaCollider; 
     
    void Start() 
    { 
        espadaCollider = espada.GetComponent<Collider2D>(); 
        espadaCollider.enabled = false; 
    } 
     
    void Update() 
    { 
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !atacando) 
        { 
            IniciarAtaque(); 
        } 
         
        if (atacando) 
        { 
            ActualizarAtaque(); 
        } 
    } 
     
    void IniciarAtaque() 
    { 
        atacando = true; 
        tiempo = 0f; 
        espadaCollider.enabled = true; 
    } 
     
    void ActualizarAtaque() 
    { 
        tiempo += Time.deltaTime; 
        espada.transform.Rotate(0, 0, velocidadGiro * Time.deltaTime); 
         
        if (tiempo >= duracion) 
        { 
            TerminarAtaque(); 
        } 
    } 
     
    void TerminarAtaque() 
    { 
        atacando = false; 
        tiempo = 0f; 
        espadaCollider.enabled = false; 
        espada.transform.rotation = Quaternion.identity; 
    } 
}