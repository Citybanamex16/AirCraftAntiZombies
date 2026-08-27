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

    [Header("Referencias Pool")]
    public EnemyPoolManager enemyPoolManager;

    void Start()
    {
        agente = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if(agente != null){
            agente.speed = speed;

            //Como vamos a usar Raynolds, que solo nos calcule el Path
            agente.updatePosition = false; 
            agente.updateRotation = true;
        }
        else{
            Debug.LogError("Componente Agente no encontrado en Zombie");
        }
    }

    // Update is called once per frame
    void Update()
    {
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
    void OnEnable(){
        if (targetChannel != null)
        {
            // Nos suscribimos al evento
            targetChannel.OnTargetChanged += OnNewTargetReceived;

            // Si ya existía un objetivo activo al momento de nacer, nos asignamos a él
            if (targetChannel.CurrentTarget != null)
            {
                OnNewTargetReceived(targetChannel.CurrentTarget);
            }
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
            print("¡New target received");
            setObjective(newTarget.position);
        }
    }


    // ==== Pool Functions === //

    [Header("Pool & Spawn Vars")]
    public float maxHp = 100.0f;


    public void Spawn(Vector3 spawnPosition){
        // 1. Restaurar vida
        hp = maxHp;

        // 2. Teleportar posición física
        transform.position = spawnPosition;

        // 3. Activar el GameObject
        gameObject.SetActive(true);

        // 4. Sincronizar y reiniciar el NavMeshAgent
        if (agente != null)
        {
            agente.enabled = true;
            agente.Warp(spawnPosition); // Teleporta el agente sin romper la malla
            agente.ResetPath();          // Limpia la ruta anterior
            agente.nextPosition = spawnPosition;
        }

        // 5. Asignar objetivo si el canal ya tiene uno activo al momento de renacer
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
            enemyPoolManager.ReturnZombieToPool(this);
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
