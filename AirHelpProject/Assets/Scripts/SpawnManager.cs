using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    [Header("Referencias")]
    public EnemyPoolManager enemyPool;
    private GameMaster gameManager;

    [Header("Canal de Eventos")]
    public TargetEventChannel targetChannel;

    [Header("Spawn Vars")]
    public float spawnTime = 0.5f;
    private float currentTime = 0.0f;
    public bool isSpawning = false;

    

    [Header("Objetivo Actual")]
    private Transform currentTarget;

    [Header("Enemy Horde Vars")]
    public int enemy_stock_max = 25;
    private int enemy_stock;
    private int enemies_in_game;
    public int max_enemies_in_game = 10;

    private bool emptyStock = false;

    [Header("Configuración de Manchas / Clusters")]
    public float ringRadius = 200f; // Radio del anillo
    public float clusterArcAngle = 35f; // Ventana de apertura de la mancha (en grados)
    public int minZombiesPerCluster = 15; // Mínimo por mancha
    public int maxZombiesPerCluster = 50; // Máximo por mancha
    public float depthJitter = 4f; // Variación de profundidad para dar volumen

    // Variables internas de control
    private float currentClusterAngle;
    private int zombiesRemainingInCluster = 0;



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

    void Start(){
        gameManager = GameMaster.Instance;

        if(gameManager != null){
            print("Package: valid Game reference: " + gameManager);
        }
        else{
            Debug.LogError("SpawnManager: error reference, No Game Master reference");
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
        Vector3 spawnPosition = GetClusterSpawnPosition(currentTarget.position);

        //3. Activar y Spawnear al Zombie
        zombieToSpawn.Spawn(spawnPosition);



        // Actualizar Variables
        enemy_stock -= 1;

        checkStock();

    }

private Vector3 GetClusterSpawnPosition(Vector3 centerPosition){
    // 1. Si la mancha actual se agotó, elegimos un nuevo ángulo base en el anillo
    if (zombiesRemainingInCluster <= 0)
    {
        currentClusterAngle = Random.Range(0f, 360f);
        zombiesRemainingInCluster = Random.Range(minZombiesPerCluster, maxZombiesPerCluster + 1);
    }

    // 2. Aplicamos un pequeño Strafe/Desviación angular dentro del arco de la mancha
    float offsetAngle = Random.Range(-clusterArcAngle / 2f, clusterArcAngle / 2f);
    float finalAngle = (currentClusterAngle + offsetAngle) * Mathf.Deg2Rad;

    // 3. Agregamos variación de profundidad (radio) para dar grosor a la horda
    float finalRadius = ringRadius + Random.Range(-depthJitter, depthJitter);

    // 4. Calculamos la posición final X, Z
    float x = centerPosition.x + Mathf.Cos(finalAngle) * finalRadius;
    float z = centerPosition.z + Mathf.Sin(finalAngle) * finalRadius;

    zombiesRemainingInCluster--;

    return new Vector3(x, centerPosition.y, z);
    }
}
