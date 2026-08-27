using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    [Header("Referencias")]
    public EnemyPoolManager enemyPool;

    [Header("Canal de Eventos")]
    public TargetEventChannel targetChannel;

    [Header("Spawn Vars")]
    public float spawnTime = 0.5f;
    private float currentTime = 0.0f;
    public bool isSpawning = false;

    [Header("Objetivo Actual")]
    private Transform currentTarget;

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
        if (currentTime <= 0)
        {
            Spawn();
            currentTime = spawnTime;
        }
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

        if (zombieToSpawn == null) return;

        // 2. Pendiente: Algoritmo para calcular las coordenadas de origen en la isla
        Vector3 spawnPosition = GetSpawnPosition();

        //3. Activar y Spawnear al Zombie
        zombieToSpawn.Spawn(spawnPosition);

    }


    // Algoritmo de Coordenadas de Spawn (Pendiente de implementar)
    private Vector3 GetSpawnPosition()
    {
        // Por ahora devuelve la posición del SpawnManager como fallback
        return transform.position;
    }
}
