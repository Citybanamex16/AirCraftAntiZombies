using System.Collections.Generic;
using UnityEngine;

public class HitParticlePool : MonoBehaviour
{
    public static HitParticlePool Instance { get; private set; }

    [Header("Configuración del Pool")]
    public GameObject particlePrefab;
    public int initialPoolSize = 50;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Pre-instanciar las partículas al iniciar el juego
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(particlePrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    // Método para obtener una partícula del pool
    public void SpawnHitParticle(Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            // Auto-escalamiento en caso de que se acaben
            obj = Instantiate(particlePrefab, transform);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        // Reiniciar y reproducir el ParticleSystem
        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Clear();
            ps.Play();
        }
    }

    // Regresar la partícula al pool (llamada por la particula)
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
