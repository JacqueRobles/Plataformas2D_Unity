using UnityEngine; 
 
public class ObjetoDestructible : MonoBehaviour 
{ 
    void OnTriggerEnter2D(Collider2D other) 
    { 
        if (other.CompareTag("Espada")) 
        { 
            Destroy(gameObject); 
        } 
    } 
} 