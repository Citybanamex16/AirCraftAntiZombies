using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    [Header("Referencias")]
    public EnemyPoolManager enemyPool;
    public GameMaster gameManager;

    [Header("Canal de Eventos")]
    public TargetEventChannel targetChannel;

    [Header("Spawn Vars")]
    public float spawnTime = 0.5f;
    private float currentTime = 0.0f;
    public bool isSpawning = false;

    public float radioMinimo = 125.0f;
    public float radioMaximo = 150.0f;

    [Header("Objetivo Actual")]
    private Transform currentTarget;

    [Header("Enemy Horde Vars")]
    public int enemy_stock_max = 25;
    private int enemy_stock;
    private int enemies_in_game;
    public int max_enemies_in_game = 10;

    private bool emptyStock = false;



    void OnEnable()
    {
        if (targetChannel != null)
        {
            targetChannel.OnTargetChanged += OnNewTargetReceived;

            // Si ya hay un objetivo registrado al activar la escena
            if (targetChannel.CurrentTarget != null)
            {
                OnNewTargetReceived(targetChannel.CurrentTarget);
            }
        }

        reset();
    }

    void OnDisable()
    {
        if (targetChannel != null)
        {
            targetChannel.OnTargetChanged -= OnNewTargetReceived;
        }
    }

    void Update()
    {
        // Solo spawneamos si la horda está activa
        if (!isSpawning) return;

        currentTime -= Time.deltaTime;

        if(!emptyStock){
            if (currentTime <= 0 && (enemies_in_game < max_enemies_in_game))
            {
            Spawn();
            currentTime = spawnTime;
            }
        }

        if(isSpawning){
            enemies_in_game = enemyPool.getActiveZombiesCount();
            print("Enemies in game: " + enemies_in_game);
            if(emptyStock && enemies_in_game == 0){
                isSpawning = false;
                print("SpawnManager: Package Defended");
                gameManager.OnPackageDefended();
            }
        }


        
    }

    void checkStock(){
        if(enemy_stock  <= 0){
                print("EmptyStock");
                emptyStock = true;
        }
    }

    // function called by Game Master
    public void reset(){
        isSpawning = false;
        enemy_stock = enemy_stock_max;
        emptyStock = false;
        print("Enemy Spawner resetted and ready to got");
    }

    private void OnNewTargetReceived(Transform newTarget)
    {
        currentTarget = newTarget;
        
        // Al recibir un nuevo objetivo (ej. paquete en el suelo), activamos el spawn de la horda
        if (currentTarget != null)
        {
            isSpawning = true;
            print("SpawnManager: Nuevo objetivo recibido. Iniciando horda >:)");
        }
    }

    void Spawn()
    {
        if (enemyPool == null) return;

        // 1. Pedir un zombie inactivo al Pool
        Zombie zombieToSpawn = enemyPool.GetZombie();

        if (zombieToSpawn == null){

            Debug.LogError("SpawnManager: Pool devolvio null, error en spawn");
        }

        // 2. Pendiente: Algoritmo para calcular las coordenadas de origen en la isla
        Vector3 spawnPosition = GetSpawnPosition();

        //3. Activar y Spawnear al Zombie
        zombieToSpawn.Spawn(spawnPosition);



        // Actualizar Variables
        enemy_stock -= 1;

        checkStock();

    }


    // Algoritmo de Coordenadas de Spawn
    private Vector3 GetSpawnPosition()
{
    if (currentTarget == null) return transform.position;

    // Distancias del anillo

    // Dirección aleatoria en 2D (plano XZ)
    Vector2 direccionRandom = Random.insideUnitCircle.normalized;
    float distanciaRandom = Random.Range(radioMinimo, radioMaximo);

    Vector3 puntoGenerado = currentTarget.position + new Vector3(direccionRandom.x, 0, direccionRandom.y) * distanciaRandom;

    // Validar que el punto caiga dentro del NavMesh navegable
    if (NavMesh.SamplePosition(puntoGenerado, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
    {
        return hit.position;
    }

    return currentTarget.position; // Fallback
}
}
