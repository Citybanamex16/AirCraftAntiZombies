using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirCraft : MonoBehaviour
{

    // ==== public Vars ==== 

    //Refrencia a objetivo
    [Header("Objetivo")]
    public ObjectiveTerrain Objetivo;
    public GunshipTurret Torreta;
    //El script vive como componente dentro de un Game object simple pero necesitas 
    //añadir la clase del script/componente que va a recibir esta referencia
    
    

    //Vars de cono/triangulo de vision (Trigonometria buuuh)
    [Header("Variables de Orbita")]
    public float distanciaObjetivo = 50f; //Hipotenusa (distancia total en diagonal)
    public float anguloVisionGrados = 30f; // Ángulo inclinado desde el suelo hacia el avión
    public float OrbitSpeed = 15.0f; //Grados por segundo
    private float OrbitRadius; //Distancia X en el triangulo calculada en base a la hipotensua y a los angulos de inclinacion



    // === Private Vars ====
    private Vector3 targetCenter = new Vector3(0.0f,0.0f,0.0f);
    private bool positionSetted = false;
    private float anguloActual = 0.0f;



    // Start is called before the first frame update
    void Start()
    {

        if(Objetivo != null){

            targetCenter = Objetivo.get_center();
            

            setAirCraft(targetCenter);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(positionSetted){
            OrbitMovement();
        }

        
    }



    // === AirCraft Functions === // 
    void setAirCraft(Vector3 NewObjectiveCenter){
        // Position the AirCraft to Certain Distance and angle of the center of the objective
        float radianes = anguloVisionGrados * Mathf.Deg2Rad;

        //Usando trigonometria calculamos el cateto opuesto (altura Y) & el cateto adyacente (radio de la orbita)
        float alturaY = distanciaObjetivo * Mathf.Sin(radianes);
        OrbitRadius = distanciaObjetivo * Mathf.Cos(radianes); // Calculamos el radio a partir de la distancia diagonal al objetivo

        //Calcuamos nuestra pos inicial a partir del centro del objetivo
        float posX = NewObjectiveCenter.x + OrbitRadius;
        float posY = NewObjectiveCenter.y + alturaY;
        float posZ = NewObjectiveCenter.z;

        Vector3 newPos = new Vector3(posX, posY, posZ);
        transform.position = newPos;


        if (Torreta != null){
            float yawInicial = -90f; 

            // La cámara se inclina hacia el suelo exactamente los mismos grados del triángulo
            // negativo para ver hacia abajo.
            float pitchInicial = anguloVisionGrados; 

            // Le mandamos la orden a la torreta
            Torreta.InicializarAngulos(yawInicial, pitchInicial);
        }

        

        anguloActual = 0f;
        positionSetted = true;
    }


    void OrbitMovement(){
        //Calculates the Orbital movement every frame

        //Acumular el angulo basado en el tiempo
        anguloActual += OrbitSpeed * Time.deltaTime;

        //Convertir el angulo a RADIANES usando funciones nativas de Unity :)
        float radianes = anguloActual * Mathf.Deg2Rad;

        // X = Centro.x + radio * Cos(angulo)
        // Z = Centro.Z + radio * Sen(angulo)
        float orbitX = targetCenter.x + OrbitRadius * Mathf.Cos(radianes);
        float orbitZ = targetCenter.z + OrbitRadius * Mathf.Sin(radianes);
        float orbitY = transform.position.y;

        Vector3 newOrbitalPos = new Vector3(orbitX,orbitY,orbitZ);

        AirCraftDirection(newOrbitalPos,orbitY);

        transform.position = newOrbitalPos;

        

    }

    void AirCraftDirection(Vector3 currentOrbitalPosition,float orbitY){
        //Utilizamos un truco donde podemos calcular el paso 
        // en la orbita DEL SIGUIENTE frame y voltear a ver para allá
        // Ademas usamos un truco de tangente para saber hacia donde voltear :)

        //Mismo calculo solo 0.1 adelantados
        float radianesFuturos = (anguloActual + 0.1f) * Mathf.Deg2Rad;
        float futuroX = targetCenter.x + OrbitRadius * Mathf.Cos(radianesFuturos);
        float futuroZ = targetCenter.z + OrbitRadius * Mathf.Sin(radianesFuturos);
        Vector3 FutureOrbitalPos = new Vector3(futuroX, orbitY, futuroZ);

        //Usamos restas de vectores para sacar un vector que apunte
        // desde mi pos orbital hacia la futura (A-B da un vector que ve de B hacia A)
        Vector3 OrbitalDirection = (FutureOrbitalPos - currentOrbitalPosition).normalized;

        if (OrbitalDirection != Vector3.zero)
            {
        transform.rotation = Quaternion.LookRotation(OrbitalDirection);
            }
    }

    // === Debug visual AI === //

    private void OnDrawGizmos()
        {
            // Usamos una variable local para previsualizar el centro en el editor
            Vector3 centroVisual = Vector3.zero;
            float radioVisual = OrbitRadius;

            // Si el juego está corriendo, usamos los datos reales calculados
            if (Application.isPlaying)
            {
                centroVisual = targetCenter;
            }
            // Si el juego NO está corriendo (en modo edición), intentamos leer el terreno
            else if (Objetivo != null)
            {
                centroVisual = Objetivo.get_center();
                // Simulamos el cálculo del radio en el editor para verlo antes de dar Play
                float radianesEditor = anguloVisionGrados * Mathf.Deg2Rad;
                radioVisual = distanciaObjetivo * Mathf.Cos(radianesEditor);
            }
            else
            {
                // Si no hay objetivo asignado en el inspector, no dibujamos nada para evitar spam de errores
                return; 
            }

            // 1. Dibuja una línea desde el avión hasta el centro real del objetivo
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, centroVisual);

            // 2. Dibuja la órbita (el círculo en el suelo) centrada en el objetivo
            Gizmos.color = Color.cyan;
            // Colocamos el centro del círculo a la misma altura del avión para que sea fácil de ver
            Vector3 centroCirculoAltitud = new Vector3(centroVisual.x, transform.position.y, centroVisual.z);

            Gizmos.DrawWireSphere(centroCirculoAltitud, radioVisual);
        }


}
