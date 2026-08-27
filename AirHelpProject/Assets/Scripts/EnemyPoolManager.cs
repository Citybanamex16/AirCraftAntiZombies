using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [Header("Pool Config")]
    public int EnemyPoolSize = 50;
    public Zombie zombiePrefab; 
    // Zombies inactivos actualmente en la isla
    private Queue<Zombie> EnemyPoolQueue = new Queue<Zombie>();
    // Zombies activos actualmente en la isla, se supone que HashSet es super rapido 
    private HashSet<Zombie> activeZombies = new HashSet<Zombie>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        for (int i = 0; i < EnemyPoolSize; i++)
        {
            // Instantiate devuelve directamente el tipo 'Zombie'
            Zombie nuevoZombie = Instantiate(zombiePrefab, transform);
            nuevoZombie.gameObject.SetActive(false);
            
            EnemyPoolQueue.Enqueue(nuevoZombie);
        }

        print("EnemyPool Inicializado con: " + EnemyPoolQueue.Count + " Zombies");
    }

    // Pide un zombie al pool y lo posiciona en la isla
    public Zombie GetZombie()
    {
        Zombie zombieAActivar;

        if (EnemyPoolQueue.Count > 0)
        {
            zombieAActivar = EnemyPoolQueue.Dequeue();
        }
        else
        {
            // AutoEscalamiento, Si la cola se vacía, instanciamos uno nuevo
            zombieAActivar = Instantiate(zombiePrefab, transform);
        }
        //Registrams en Hash de activos
        activeZombies.Add(zombieAActivar);

        return zombieAActivar;
    }

    

    // Método para devolver al zombie a la Queue cuando muere
    public void ReturnZombieToPool(Zombie zombie)
    {
        activeZombies.Remove(zombie);
        zombie.gameObject.SetActive(false);
        EnemyPoolQueue.Enqueue(zombie);
    }

    public int getActiveZombiesCount(){
        return activeZombies.Count;
    }
}
