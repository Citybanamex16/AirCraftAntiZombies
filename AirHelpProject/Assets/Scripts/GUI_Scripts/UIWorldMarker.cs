// == Script base generado con IA == //
using UnityEngine;

public class UIWorldMarker : MonoBehaviour
{
    private Transform target3D;
    private Collider targetCollider;
    private Camera mainCamera;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

   [Header("Configuración de Visibilidad por Zoom")]
    public float baseMaxDistance = 250f; // Distancia máxima en metros SIN ZOOM
    public float defaultFOV = 60f;       // FOV por defecto de tu cámara principal

    [Header("Ajuste de Caja 2D")]
    public Vector2 minBoxSize = new Vector2(15f, 15f); // Tamaño mínimo para que no desaparezca si está lejísimos
    public float padding = 5f;

    private Vector3[] worldCorners = new Vector3[8];


    public void Setup(Transform target, Camera cam)
    {
        target3D = target;
        mainCamera = cam;
        rectTransform = GetComponent<RectTransform>();

        targetCollider = target.GetComponent<Collider>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void LateUpdate()
    {
        // CRITERIO 1: Si el Zombie volvió al Pool (se desactivó) o fue destruido
        if (target3D == null || !target3D.gameObject.activeInHierarchy)
        {
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false); // Apagamos el marcador para reutilizarlo
            return;
        }

        // 2. Calcular la distancia de corte adaptada al Zoom actual
        float currentFOV = mainCamera.fieldOfView;
        // Evitamos división por cero por seguridad
        float zoomFactor = defaultFOV / Mathf.Max(currentFOV, 1.0f);
        float dynamicMaxDistance = baseMaxDistance * zoomFactor;

        // 3. Proyectar posición 3D a la Pantalla 2D
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target3D.position);
        float distanceToTarget = screenPos.z;

        // 4. Validar criterios de visibilidad
        bool inFrontOfCamera = distanceToTarget > 0;
        bool inDynamicRange = distanceToTarget <= dynamicMaxDistance;

        if (inFrontOfCamera && inDynamicRange)
        {
            // 1. Si tenemos collider, calculamos el Bounding Box dinámico
            if (targetCollider != null)
            {
                UpdateBoundingBox2D();
            }
            else
            {
                // Si no tiene collider, simplemente lo posicionamos en el punto pivot
                rectTransform.position = screenPos;
            }

            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = 0f; // Oculto por estar lejos o a la espalda
        }
    }



    // == Funcion generada con IA == //
    private void UpdateBoundingBox2D()
    {
        Bounds bounds = targetCollider.bounds;
        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;

        // 8 esquinas del cubo en 3D
        worldCorners[0] = new Vector3(c.x + e.x, c.y + e.y, c.z + e.z);
        worldCorners[1] = new Vector3(c.x + e.x, c.y + e.y, c.z - e.z);
        worldCorners[2] = new Vector3(c.x + e.x, c.y - e.y, c.z + e.z);
        worldCorners[3] = new Vector3(c.x + e.x, c.y - e.y, c.z - e.z);
        worldCorners[4] = new Vector3(c.x - e.x, c.y + e.y, c.z + e.z);
        worldCorners[5] = new Vector3(c.x - e.x, c.y + e.y, c.z - e.z);
        worldCorners[6] = new Vector3(c.x - e.x, c.y - e.y, c.z + e.z);
        worldCorners[7] = new Vector3(c.x - e.x, c.y - e.y, c.z - e.z);

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        // Proyectamos las 8 esquinas a la pantalla y buscamos los bordes extremos
        for (int i = 0; i < 8; i++)
        {
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldCorners[i]);
            if (screenPoint.x < minX) minX = screenPoint.x;
            if (screenPoint.x > maxX) maxX = screenPoint.x;
            if (screenPoint.y < minY) minY = screenPoint.y;
            if (screenPoint.y > maxY) maxY = screenPoint.y;
        }

        // Ancho y Alto en píxeles + padding
        float width = Mathf.Max((maxX - minX) + padding * 2f, minBoxSize.x);
        float height = Mathf.Max((maxY - minY) + padding * 2f, minBoxSize.y);

        // Centro 2D del rectángulo
        Vector2 centerScreenPoint = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);

        // Aplicar al RectTransform de la UI
        rectTransform.position = centerScreenPoint;
        rectTransform.sizeDelta = new Vector2(width, height);
    }

}