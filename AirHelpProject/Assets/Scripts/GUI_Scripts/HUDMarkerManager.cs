// == Script base generado con IA == //
using System.Collections.Generic;
using UnityEngine;

public class HUDMarkerManager : MonoBehaviour
{
    public static HUDMarkerManager Instance { get; private set; }

    [Header("Referencias")]
    public Camera mainCamera;
    public RectTransform markerContainer;

    [Header("Prefabs de Marcadores")]
    public UIWorldMarker zombieMarkerPrefab;  // Cuadro Rojo
    public UIWorldMarker packageMarkerPrefab; // Cuadro Verde

    // Pool de marcadores para no instanciar masivamente en UI
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
}
