using UnityEngine;
using System.Collections;

/// <summary>
/// Power-up que aumenta la velocidad de movimiento del jugador temporalmente
/// </summary>
public class PowerUpVelocidad : PowerUpBase
{
    [Header("Configuración de Velocidad")]
    public float multiplicadorVelocidad = 1.5f;
    public Color colorEfecto = Color.cyan;
    
    protected override void AplicarEfecto(PlayerController player)
    {
        // Iniciar corrutina para aplicar el efecto
        StartCoroutine(EfectoVelocidad(player));
        
        Debug.Log($"Power-up de Velocidad aplicado! Multiplicador: {multiplicadorVelocidad}x por {duracionEfecto} segundos");
    }
    
    IEnumerator EfectoVelocidad(PlayerController player)
    {
        // Guardar velocidad original
        float velocidadOriginal = player.velocidadMovimiento;
        Color colorOriginal = player.GetComponent<SpriteRenderer>().color;
        
        // Aplicar efecto
        player.velocidadMovimiento *= multiplicadorVelocidad;
        
        // Efecto visual temporal
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            StartCoroutine(EfectoColorPulsante(playerSprite, colorEfecto));
        }
        
        // Esperar duración del efecto
        yield return new WaitForSeconds(duracionEfecto);
        
        // Restaurar velocidad original
        player.velocidadMovimiento = velocidadOriginal;
        
        // Restaurar color original
        if (playerSprite != null)
        {
            playerSprite.color = colorOriginal;
        }
        
        Debug.Log("Efecto de velocidad terminado");
    }
    
    IEnumerator EfectoColorPulsante(SpriteRenderer sprite, Color colorEfecto)
    {
        Color colorOriginal = sprite.color;
        float tiempoTranscurrido = 0f;
        
        while (tiempoTranscurrido < duracionEfecto)
        {
            // Crear efecto pulsante
            float alpha = Mathf.PingPong(Time.time * 2f, 1f);
            Color colorMezcla = Color.Lerp(colorOriginal, colorEfecto, alpha * 0.3f);
            sprite.color = colorMezcla;
            
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }
        
        // Restaurar color original
        sprite.color = colorOriginal;
    }
} 