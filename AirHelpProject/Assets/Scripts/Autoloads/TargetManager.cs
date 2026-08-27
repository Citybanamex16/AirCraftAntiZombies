using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetEventChannel", menuName = "Events/Target Event Channel")]
public class TargetEventChannel : ScriptableObject

//Autoload de Objetivos de Zombies

{
    // Evento C# al que se suscribirán los zombies
    public event Action<Transform> OnTargetChanged;

    // Guarda el objetivo actual en memoria
    public Transform CurrentTarget { get; private set; }

    // Método que llamará el paquete al impactar el suelo
    public void RaiseEvent(Transform newTarget)
    {
        
        CurrentTarget = newTarget;
        
        // Notifica a todos los escuchas (zombies)
        OnTargetChanged?.Invoke(newTarget);
    }

    // Opcional: Para limpiar la referencia al reiniciar el juego
    private void OnEnable()
    {
        CurrentTarget = null;
    }
}
