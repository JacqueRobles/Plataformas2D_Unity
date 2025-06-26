using UnityEngine;

public class ObjetoDestructibleloco : MonoBehaviour
{
    [Header("Configuración")]
    public int vidaMaxima = 100;
    public bool destruirAlMorir = true;
    
    private int vidaActual;
    
    void Start()
    {
        vidaActual = vidaMaxima;
    }
    
    public void RecibirDanio(int cantidad)
    {
        vidaActual -= cantidad;
        Debug.Log($"{gameObject.name} recibió {cantidad} de daño. Vida: {vidaActual}/{vidaMaxima}");
        
        if (vidaActual <= 0)
        {
            Morir();
        }
    }
    
    void Morir()
    {
        Debug.Log($"{gameObject.name} destruido!");
        
        if (destruirAlMorir)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}