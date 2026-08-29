// == Script base generado con IA == //
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDMarkerManager : MonoBehaviour
{
    public static HUDMarkerManager Instance { get; private set; }

    [Header("Referencias")]
    public Camera mainCamera;
    public RectTransform markerContainer;
    public LayerMask groundLayer;

    [Header("Prefabs de Marcadores")]
    public UIWorldMarker zombieMarkerPrefab;  // Cuadro Rojo
    public UIWorldMarker packageMarkerPrefab; // Cuadro Verde

    [Header("Referencias de UI")]
    public TextMeshProUGUI coordinatesText;
    public TextMeshProUGUI rangeText;

    
    


    // Pool de marcadores para no instanciar  
    private List<UIWorldMarker> markerPool = new List<UIWorldMarker>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (mainCamera == null) mainCamera = Camera.main;
    }


    void Start(){
    //Inicializamos Marcadores
    PrewarmPool(zombieMarkerPrefab, 50);
    PrewarmPool(packageMarkerPrefab, 3);
    }

    private void PrewarmPool(UIWorldMarker prefab, int count){
    for (int i = 0; i < count; i++)
        {
            UIWorldMarker newMarker = Instantiate(prefab, markerContainer);
            newMarker.name = prefab.name;
            newMarker.gameObject.SetActive(false);
            markerPool.Add(newMarker);
        }
    }





    // Método para vincular un marcador a un Zombie o Paquete al aparecer
    public void TrackObject(Transform target, bool isPackage)
    {
        UIWorldMarker marker = GetFreeMarker(isPackage ? packageMarkerPrefab : zombieMarkerPrefab);
        marker.transform.SetParent(markerContainer, false);
        marker.gameObject.SetActive(true);
        marker.Setup(target, mainCamera);
    }

    private UIWorldMarker GetFreeMarker(UIWorldMarker prefabToUse)
    {
        foreach (var marker in markerPool)
        {
            if (!marker.gameObject.activeInHierarchy && marker.name.Contains(prefabToUse.name))
            {
                return marker;
            }
        }

        // Si no hay libres, instanciamos uno nuevo
        UIWorldMarker newMarker = Instantiate(prefabToUse, markerContainer);
        newMarker.name = prefabToUse.name;
        markerPool.Add(newMarker);
        return newMarker;
    }


    //Sistema de Telemetría en tiempo real // 

    void Update()
    {
        if (mainCamera == null) return;

        // 1. Simular coordenadas militares que reaccionan a la rotación de la cámara
        float yaw = mainCamera.transform.eulerAngles.y;
        float pitch = mainCamera.transform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f; // Convertir de 0..360 a -180..180

        if (coordinatesText != null)
        {
            coordinatesText.text = $"AZ: {yaw:000.0}°\nEL: {pitch:00.0}°";
        }

        // 2. Calcular la distancia real en metros al punto donde apunta la mirada (Raycast)
        if (rangeText != null)
        {
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, 2000f, groundLayer))
            {
                float distance = hit.distance;
                rangeText.text = $"RNG: {distance:F0} M";
            }
            else
            {
                rangeText.text = "RNG: --- M";
            }
        }
    }

}
