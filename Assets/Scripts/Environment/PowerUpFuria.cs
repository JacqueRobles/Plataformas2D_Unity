using UnityEngine;

/// <summary>
/// Power-up que extiende la duración del estado de furia del jugador
/// </summary>
public class PowerUpFuria : PowerUpBase
{
    [Header("Configuración de Furia")]
    public float tiempoExtraFuria = 3f;
    public bool activaFuriaInmediatamente = false;
    
    protected override void AplicarEfecto(PlayerController player)
    {
        if (activaFuriaInmediatamente && !player.EstaEnFuria)
        {
            // Activar furia inmediatamente
            GameManager.Instance.ActivarFuria();
            Debug.Log($"Power-up de Furia: Estado activado inmediatamente");
        }
        else if (player.EstaEnFuria)
        {
            // Extender duración actual
            ExtenderDuracionFuria(player);
            Debug.Log($"Power-up de Furia: Duración extendida en {tiempoExtraFuria} segundos");
        }
        else
        {
            // Reducir cooldown para próximo uso
            ReducirCooldownFuria(player);
            Debug.Log($"Power-up de Furia: Cooldown reducido");
        }
    }
    
    void ExtenderDuracionFuria(PlayerController player)
    {
        // Acceder al timer de furia del jugador y extenderlo
        // Nota: Esto requiere que PlayerController exponga métodos para modificar la furia
        if (player.EstaEnFuria)
        {
            // Llamar método público en PlayerController para extender furia
            player.ExtenderFuria(tiempoExtraFuria);
        }
    }
    
    void ReducirCooldownFuria(PlayerController player)
    {
        // Reducir el cooldown de furia para que pueda usarse antes
        player.ReducirCooldownFuria(tiempoExtraFuria);
    }
} 