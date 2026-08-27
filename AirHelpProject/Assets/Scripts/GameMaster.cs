using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    // Start is called before the first frame update

    [Header("Container References")]
    public GameObject enemyContainer;
    public GameObject targetContainer;

    [Header("Managers Reference")]
    public SpawnManager SpawnManagerObject;



    void Start()
    {
        if(enemyContainer == null || targetContainer == null){
            Debug.LogError("No containers sett in GM");
        }

        if(SpawnManagerObject == null){
            Debug.LogError("No SpawnManager Detectado");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
