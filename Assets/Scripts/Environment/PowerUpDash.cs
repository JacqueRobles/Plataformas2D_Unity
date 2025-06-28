using UnityEngine;
using System.Collections;

/// <summary>
/// Power-up que otorga dashes adicionales o reduce el cooldown del dash
/// </summary>
public class PowerUpDash : PowerUpBase
{
    [Header("Configuración de Dash")]
    public int dashesAdicionales = 1;
    public float reduccionCooldown = 0.5f; // Multiplicador del cooldown (0.5 = 50% más rápido)
    public bool aplicaAmbosEfectos = false;
    
    protected override void AplicarEfecto(PlayerController player)
    {
        if (aplicaAmbosEfectos)
        {
            // Aplicar ambos efectos
            StartCoroutine(EfectoDashCompleto(player));
        }
        else
        {
            // Solo dashes adicionales
            StartCoroutine(EfectoDashesAdicionales(player));
        }
        
        Debug.Log($"Power-up de Dash aplicado! Dashes adicionales: {dashesAdicionales}");
    }
    
    IEnumerator EfectoDashesAdicionales(PlayerController player)
    {
        // Otorgar dashes adicionales inmediatamente
        for (int i = 0; i < dashesAdicionales; i++)
        {
            player.OtorgarDashAdicional();
        }
        
        yield return null; // Este power-up es instantáneo
    }
    
    IEnumerator EfectoDashCompleto(PlayerController player)
    {
        // Guardar cooldown original
        float cooldownOriginal = player.cooldownDash;
        
        // Aplicar dashes adicionales
        for (int i = 0; i < dashesAdicionales; i++)
        {
            player.OtorgarDashAdicional();
        }
        
        // Reducir cooldown
        player.cooldownDash *= reduccionCooldown;
        
        // Esperar duración del efecto
        yield return new WaitForSeconds(duracionEfecto);
        
        // Restaurar cooldown original
        player.cooldownDash = cooldownOriginal;
        
        Debug.Log("Efecto de dash mejorado terminado");
    }
} 