using UnityEngine;

public class CamaraSeguimientoloca : MonoBehaviour
{
    public Transform jugador;
    public Vector3 offset = new Vector3(0, 0, -10);
    
    void Start()
    {
        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                jugador = playerObj.transform;
        }
    }
    
    void LateUpdate()
    {
        if (jugador != null)
            transform.position = jugador.position + offset;
    }
}