// Script mejorado y generado con IA //
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDWeaponSelector : MonoBehaviour
{
    // 1. Estructura para asociar cada Enum con sus elementos de UI
    [System.Serializable]
    public struct WeaponUISlot
    {
        public string weaponName;                  // Nombre para identificar en Inspector
        public GunshipTurret.EstadoTorreta modoArma;  
        public Image backgroundImage;              // La caja roja de fondo
        public TextMeshProUGUI textMesh;           
    }

    [Header("Referencias")]
    public GunshipTurret turret; // Tu script donde está 'estadoActual'
    public WeaponUISlot[] weaponSlots; // Array con cada arma (105mm, 40mm, 25mm)

    [Header("Estilos")]
    public Color activeTextColor = Color.white;
    public Color inactiveTextColor = new Color(1f, 1f, 1f, 0.4f); // Blanco semi-transparente
    public Color activeBackgroundColor = new Color(0.8f, 0.1f, 0.1f, 0.8f); // Rojo militar

    private GunshipTurret.EstadoTorreta lastState;

    void Start()
    {
        if (turret != null)
        {
            lastState = turret.estadoActual;
            UpdateWeaponUI();
        }
    }

    void Update()
    {
        if (turret == null) return;

        // Solo actualizamos la UI cuando el estado cambia (para ahorrar rendimiento)
        if (turret.estadoActual != lastState)
        {
            lastState = turret.estadoActual;
            UpdateWeaponUI();
        }
    }

    private void UpdateWeaponUI()
    {
        foreach (var slot in weaponSlots)
        {
            // Comprobamos si este slot coincide con el estadoEnum actual de la torreta
            bool isActive = (slot.modoArma == turret.estadoActual);

            // 1. Encender / Apagar el fondo rojo
            if (slot.backgroundImage != null)
            {
                slot.backgroundImage.enabled = isActive;
                slot.backgroundImage.color = activeBackgroundColor;
            }

            // 2. Ajustar brillo/alfa del texto
            if (slot.textMesh != null)
            {
                slot.textMesh.color = isActive ? activeTextColor : inactiveTextColor;
                slot.textMesh.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
            }
        }
    }
}
