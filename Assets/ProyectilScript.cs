using UnityEngine;

public class ProyectilScript : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si el proyectil golpea un objeto eliminable, lo destruye
        if (collision.gameObject.GetComponent<ObjetoEliminable>() != null)
        {
            Destroy(collision.gameObject);
        }
        
        // Destruir el proyectil al impactar con cualquier cosa
        Destroy(gameObject);
    }
} 