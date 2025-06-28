using UnityEngine; 
 
public class GeneradorEnemigos : MonoBehaviour 
{ 
    public GameObject enemigoPrefab; 
    public int cantidadMaxima = 5; 
    public float radio = 8f; 
    public float tiempoGeneracion = 5f; 
     
    private Transform jugador; 
    private float tiempoUltimo = 0f; 
     
    void Start() 
    { 
        jugador = GameObject.Find("Jugador").transform; 
         
        for (int i = 0; i < cantidadMaxima / 2; i++) 
        { 
            GenerarEnemigo(); 
        } 
    } 
     
    void Update() 
    { 
        if (Time.time - tiempoUltimo >= tiempoGeneracion) 
        { 
            if (FindObjectsByType<EnemigoIA>(FindObjectsSortMode.None).Length < cantidadMaxima) 
            { 
                GenerarEnemigo(); 
                tiempoUltimo = Time.time; 
            } 
        } 
    } 
     
    void GenerarEnemigo() 
    { 
        if (enemigoPrefab != null && jugador != null) 
        { 
            float angulo = Random.Range(0f, 2f * Mathf.PI); 
            Vector2 posicion = (Vector2)jugador.position + new Vector2( 
                Mathf.Cos(angulo) * radio, 
                Mathf.Sin(angulo) * radio 
); 
Instantiate(enemigoPrefab, posicion, Quaternion.identity); 
} 
} 
}