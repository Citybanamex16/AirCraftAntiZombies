using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameMaster : MonoBehaviour
{

    public static GameMaster Instance { get; private set; }

    public event Action OnPackageSuccess;

    [Header("Player Vars")]
    public int lives = 3;
    public int PackagesToDefend = 3;
    private int currentDefendedPackages = 0;
    //private float score = 0.0f;

    [Header("References")]
    public SpawnManager spawnManager;
    public GunshipTurret gunshipTurret;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Busca automáticamente el SpawnManager en la escena
        if (spawnManager == null)
        {
            spawnManager = FindObjectOfType<SpawnManager>();
        }

        // Busca automáticamente la torreta/nave en la escena
        if (gunshipTurret == null)
        {
            gunshipTurret = FindObjectOfType<GunshipTurret>(); 
        }
    }

    void Start()
    {
        lives = 3;
    }

    

    private bool isProcessingLoss = false;



    // === Funciones === ///

   public void OnPackageDestroyed(){
        // Previene que múltiples zombies ejecuten esta función más de una vez por paquete
        if (isProcessingLoss) return;
        isProcessingLoss = true;

        lives -= 1;
        print("Vida perdida, te quedan: " + lives);
        if(lives <= 0){
            GameOver();
        }
        else{
            resetGameplay();
        }
        
        
        
        

        // Permitir procesar el siguiente paquete tras un breve lapso
        Invoke(nameof(ResetLossFlag), 0.5f);
        }


   private void resetGameplay()
    {
        print("Resetting GamePlay");

        if (spawnManager == null) 
            Debug.LogError("¡Falta asignar SpawnManager en GameMaster!");
            
        if (gunshipTurret == null) 
            Debug.LogError("¡Falta asignar GunshipTurret (o la clase no coincide) en GameMaster!");

        if (spawnManager != null && gunshipTurret != null)
        {
            spawnManager.reset();
            gunshipTurret.reset();
        } 
    }
        

    private void ResetLossFlag()
    {
        isProcessingLoss = false;
    }

    //Called by SpawnManager when stock is out and no enemies in game
    public void OnPackageDefended(){
        print("Game Master: received Package Defended");
        currentDefendedPackages += 1;
        print("Game Master: Emitting signal OnPackageSuccess");
        OnPackageSuccess?.Invoke();

        if(currentDefendedPackages >= PackagesToDefend){
            Win();
        } else{
            resetGameplay();
        }
    }

    void GameOver(){
        print("GAME OVER");
    }

    void Win(){
        print("¡You Won!");
    }

}
