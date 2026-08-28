using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    // Start is called before the first frame update

    // === Vars Publicas === //
    

    [Header("Configuración de Flocking (Reynolds)")]
    public float radioSeparacion = 2.0f;
    public float pesoSeparacion = 3.0f;
    public LayerMask capaZombies; // Asignar la Layer "Zombie" en Unity
    private Collider[] vecinosDetectados = new Collider[10]; // Buffer para no generar Garbage Collection

    
    [Header("Navegation Vars")]
    public float speed = 10.0f;
    private UnityEngine.AI.NavMeshAgent agente;
    private Vector3 TargetPos;

    [Header("Canal de Eventos")]
    public TargetEventChannel targetChannel;

    [Header("Enemy vars")]
    public float hp = 100.0f;
    public float maxHp = 100.0f;
    public float damage = 25.0f;

    [Header("Referencias Pool")]
    public EnemyPoolManager enemyPoolManager;


    void Awake()
    {
        // Usamos Awake para garantizar que la referencia 'agente' nunca sea null al reactivar del Pool
        agente = GetComponent<NavMeshAgent>();
        if (agente != null)
        {
            agente.speed = speed;
            agente.updatePosition = false;
            agente.updateRotation = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        //Si no estamos activos no ejecutamos nada
        if (agente == null || !agente.enabled || !agente.isOnNavMesh) return;


        // 1. Obtener la velocidad base del NavMesh (Pathfinding)
        Vector3 velocidadNavMesh = agente.desiredVelocity;

        // 2. Calcular la fuerza de Separación contra otros zombies cercanos
        Vector3 fuerzaSeparacion = CalcularSeparacion(); // (Mini motor de Raynolds)

        // 3. Vector Final = Dirección NavMesh + Repulsión de Vecinos
        Vector3 movimientoFinal = velocidadNavMesh + (fuerzaSeparacion * pesoSeparacion);

        // 4. Mover físicamente al Zombie
        transform.position += movimientoFinal * Time.deltaTime;

        // 5. Sincronizar 
        agente.nextPosition = transform.position;
    }



    public void hurted(float dp){
        hp -= dp;
        print("Hurted: " + hp);

        if(hp <= 0){
            Despawn();
        }
    }



    public void setObjective(Vector3 TargetPosition){

        if(agente != null){
            TargetPos = TargetPosition;
            agente.SetDestination(TargetPosition);
        }
        
    }


    // ==== Mini motor de Raynolds === //

    private Vector3 CalcularSeparacion(){
        // Inicializamos
        Vector3 flujoRepulsion = Vector3.zero;

        // Busca vecinos cercanos sin instanciar arreglos dinámicos
        //OverlapSphereNonAlloc detecta colisionadores en un radio especifico y los guarda sin generar basura en memoria
        // OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, int layerMask)
        int cantidadVecinos = Physics.OverlapSphereNonAlloc(transform.position, radioSeparacion, vecinosDetectados, capaZombies);

        int vecinosValidos = 0;

        for (int i = 0; i < cantidadVecinos; i++)
        {
            Transform vecino = vecinosDetectados[i].transform;

            // Evitar calcular la distancia contra sí mismo
            if (vecino != transform)
            {
                Vector3 direccionAlejamiento = transform.position - vecino.position;
                float distancia = direccionAlejamiento.magnitude; //Le sacamos el lenght

                if (distancia > 0.001f) // Prevenir división por cero
                {
                    // Mientras más cerca esté el vecino, mayor es la fuerza de repulsión
                    flujoRepulsion += direccionAlejamiento.normalized / distancia;
                    vecinosValidos++;
                }
            }
        }

        if (vecinosValidos > 0)
        {
            flujoRepulsion /= vecinosValidos; // Promedio vectorial
        }

        return flujoRepulsion;
    }



    // === Funciones de Target === //

    // == Regla 2: Mientras viva escucho nuevos targets ==
    void OnEnable(){
        if (targetChannel != null)
        {
            // Nos suscribimos al evento
            targetChannel.OnTargetChanged += OnNewTargetReceived;
        }
    }

    void OnDisable(){
        if (targetChannel != null)
        {
            // Desuscripción obligatoria para evitar Memory Leaks
            targetChannel.OnTargetChanged -= OnNewTargetReceived;
        }
    }

    private void OnNewTargetReceived(Transform newTarget){
        if (newTarget != null){
            // Llamamos a nuestro método de movimiento del NavMesh
            //print("¡New target received");
            setObjective(newTarget.position);
        } else{
            StopMoving();
        }
    }

    public void StopMoving()
    {
        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.ResetPath(); // Borra la ruta hacia la caja muerta
        }
    }




     private void OnTriggerEnter(Collider other){
        //print("¡Algo Detectado! " + other.gameObject.tag);

        // Puedes comprobar una etiqueta (tag) específica
        if (other.CompareTag("Package")){
            //print("¡Piso Tocado!");
            Package componentePackage = other.GetComponent<Package>();

            if (componentePackage != null){
            componentePackage.hurted(damage);
                }
            else{
                Debug.LogError("Componente Script sin encontrar");
            }

            Despawn();
        }
        
    }



    // ==== Pool Functions === //


   // === REGLA 1: Nacer desde el Pool y tomar el objetivo activo de la BD ===
    public void Spawn(Vector3 spawnPosition)
    {
        // 1. Restaurar vida
        hp = maxHp;

        // 2. Activar GameObject PRIMERO
        gameObject.SetActive(true);

        // 3. Posicionar y Sincronizar NavMeshAgent
        if (agente != null)
        {
            agente.enabled = true;
            transform.position = spawnPosition;
            agente.Warp(spawnPosition); // Coloca al agente en la malla en esa coordenada
            agente.ResetPath();
        }

        // 4. Consultar objetivo activo en la BD
        if (targetChannel != null && targetChannel.CurrentTarget != null)
        {
            setObjective(targetChannel.CurrentTarget.position);
        }
    }


    // Método de desactivación para devolver al Pool
    public void Despawn()
    {
        if (agente != null && agente.isOnNavMesh)
        {
            agente.ResetPath();
            agente.enabled = false; // Desactivar agente evita que procese en background
        }

        //Caso A: Tenemos referencia
        if (enemyPoolManager != null)
        {
            EnemyPoolManager.Instance.ReturnZombieToPool(this);
        } //Caso B: No tenemos ref, la buscamos
        else if (EnemyPoolManager.Instance != null)
        { 
            EnemyPoolManager.Instance.ReturnZombieToPool(this);
        }//Caso C: No tenemos ref y no encontramos, nos desactivamos
        else
        {   
            Debug.LogError("Perdida de Pool, enemigo no encontro Pool a regresar");
            gameObject.SetActive(false);
        }
    }

}
