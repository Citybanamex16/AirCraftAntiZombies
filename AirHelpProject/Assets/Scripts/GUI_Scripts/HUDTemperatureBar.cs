using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDTemperatureBar : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image fillImage;
    public TextMeshProUGUI tempText;

    [Header("Referencia al Arma")]
    public GunshipTurret turret; // Referencia a la Torreta

    [Header("Configuración Visual")]
    //Colores de Barra
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color overloadColor = Color.red;

    void Update()
    {
        if (turret == null || fillImage == null){
            Debug.LogError("Temp UI Error: Referencias no validas");
            return;
        } 

        // 1. Calcular el ratio de temperatura (0.0 a 1.0) con un clamp como en Godot
        float heatRatio = Mathf.Clamp01(turret.getTemp() / turret.getMaxTemp());

        // 2. Modificar el llenado de la barra (equivalente a un Value en TextureProgressBar)
        fillImage.fillAmount = heatRatio;

        // 3. Cambiar color según el nivel de peligro
        if (turret.getOverload())
        {
            fillImage.color = overloadColor;
            if (tempText != null) tempText.text = "TEMP: OVERHEAT!";
        }
        else if (heatRatio > 0.75f)
        {
            fillImage.color = warningColor;
            if (tempText != null) tempText.text = $"TEMP: {(heatRatio * 100f):F0}%";
        }
        else
        {
            fillImage.color = normalColor;
            if (tempText != null) tempText.text = $"TEMP: {(heatRatio * 100f):F0}%";
        }
    }
}
