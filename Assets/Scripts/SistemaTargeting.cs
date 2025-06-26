using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class SistemaTargeting : MonoBehaviour
{
    [Header("Configuración")]
    public float rangoDeteccion = 10f;
    public LayerMask capasEnemigos = -1;
    public GameObject indicadorTarget;
    public GameObject prefabMisil;
    public int misilesTotales = 5;
    
    private Transform targetActual;
    private List<Transform> enemigosEnRango = new List<Transform>();
    private int misilesActuales;
    private float tiempoUltimoMisil;
    private Camera camaraJuego;
    private Dictionary<Transform, GameObject> indicadores = new Dictionary<Transform, GameObject>();
    
    void Start()
    {
        misilesActuales = misilesTotales;
        camaraJuego = Camera.main;
        InvokeRepeating(nameof(BuscarEnemigos), 0f, 0.2f);
    }
    
    void Update()
    {
        // TAB para cambiar target
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            CambiarTarget();
        
        // Click izquierdo para misil
        if (Mouse.current.leftButton.wasPressedThisFrame && targetActual != null)
            LanzarMisil();
        
        // Click derecho para targeting manual
        if (Mouse.current.rightButton.wasPressedThisFrame)
            TargetingManual();
            
        ActualizarIndicadores();
    }
    
    void BuscarEnemigos()
    {
        enemigosEnRango.Clear();
        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, rangoDeteccion, capasEnemigos);
        
        foreach (Collider2D enemigo in enemigos)
        {
            if (enemigo.transform != transform)
                enemigosEnRango.Add(enemigo.transform);
        }
        
        enemigosEnRango = enemigosEnRango.OrderBy(t => Vector2.Distance(transform.position, t.position)).ToList();
        
        if (targetActual == null && enemigosEnRango.Count > 0)
            targetActual = enemigosEnRango[0];
    }
    
    void CambiarTarget()
    {
        if (enemigosEnRango.Count == 0) return;
        
        int indice = enemigosEnRango.IndexOf(targetActual);
        indice = (indice + 1) % enemigosEnRango.Count;
        targetActual = enemigosEnRango[indice];
    }
    
    void TargetingManual()
    {
        Vector3 posicionMouse = camaraJuego.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D enemigo = Physics2D.OverlapPoint(posicionMouse, capasEnemigos);
        
        if (enemigo != null && enemigosEnRango.Contains(enemigo.transform))
            targetActual = enemigo.transform;
    }
    
    void LanzarMisil()
    {
        if (misilesActuales <= 0 || Time.time < tiempoUltimoMisil + 2f) return;
        
        tiempoUltimoMisil = Time.time;
        misilesActuales--;
        
        GameObject misil = Instantiate(prefabMisil, transform.position, Quaternion.identity);
        MisilTeledirigido script = misil.GetComponent<MisilTeledirigido>();
        if (script != null)
            script.EstablecerTarget(targetActual);
            
        Debug.Log($"Misil lanzado! Restantes: {misilesActuales}");
    }
    
    void ActualizarIndicadores()
    {
        // Limpiar indicadores obsoletos
        var keysToRemove = indicadores.Keys.Where(key => key == null).ToList();
        foreach (var key in keysToRemove)
        {
            if (indicadores[key] != null) Destroy(indicadores[key]);
            indicadores.Remove(key);
        }
        
        // Crear/actualizar indicadores
        foreach (Transform enemigo in enemigosEnRango)
        {
            if (enemigo == null) continue;
            
            if (!indicadores.ContainsKey(enemigo))
            {
                GameObject indicador = Instantiate(indicadorTarget, enemigo.position, Quaternion.identity);
                indicador.transform.SetParent(enemigo);
                indicador.transform.localPosition = Vector3.up * 0f;
                indicadores[enemigo] = indicador;
            }
            
            SpriteRenderer sprite = indicadores[enemigo].GetComponent<SpriteRenderer>();
            if (sprite != null)
                sprite.color = (enemigo == targetActual) ? Color.red : Color.yellow;
        }
        
        // Remover indicadores fuera de rango
        foreach (Transform enemigo in indicadores.Keys.ToList())
        {
            if (enemigo != null && !enemigosEnRango.Contains(enemigo))
            {
                Destroy(indicadores[enemigo]);
                indicadores.Remove(enemigo);
            }
        }
    }
}