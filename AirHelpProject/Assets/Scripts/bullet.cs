using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class bullet : MonoBehaviour
{

    [Header("bullet vars")]
    public float speed = 120.0f;
    public float damage = 100.0f;
    private float damageToPackage = 0.0f;
    public AudioClip groundHitSound;

    [Header("Efectos Visuales")]
    public GameObject groundHitParticlePrefab;

    // === Object Pooling Vars == //
    public event Action<bullet> onBulletDeactivated;

    private Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        damageToPackage = damage * 0.20f;
    }

    // Update is called once per frame
    void Update()
    {
        //Esta funcion ya calcula matemáticamente la 
        //dirección "hacia adelante" del objeto basándose en su rotación actual
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other){
        //print("¡Algo Detectado! " + other.gameObject.tag);



        // Puedes comprobar una etiqueta (tag) específica
        if (other.CompareTag("Ground")){
            //print("¡Piso Tocado!");

            // 1. Instanciar la partícula en la posición exacta del impacto
            if (groundHitParticlePrefab != null)
            {
                HitParticlePool.Instance.SpawnHitParticle(transform.position, Quaternion.identity);
            }
            // Crea un AudioSource 3D en el punto del impacto, reproduce el clip y se autodestruye solo
            AudioSource.PlayClipAtPoint(groundHitSound, transform.position, 0.9f);



            deactivate();
        }
        else if(other.CompareTag("Zombie")){
            //print("Enemy detected");

            //1. Conseguir el componente script
            Zombie componenteZombie = other.GetComponent<Zombie>();

            if (componenteZombie != null)
                {
            
            componenteZombie.hurted(damage);
                }
            else{
                Debug.LogError("Componente Script sin encontrar");
            }

            deactivate();
        }
        else if (other.CompareTag("Package")){
            Package componentePackage = other.GetComponent<Package>();

            if (componentePackage != null){
            componentePackage.hurted(damageToPackage);
                }
            else{
                Debug.LogError("Componente Script sin encontrar");
            }
            deactivate();
        }
    }


    // === Object Pooling Functions == //


    public void activate(Vector3 spawnPosition, Quaternion spawnRotation){
        //Setteamos Pos
        transform.position = spawnPosition;
        //print("Bullet spawning in: " + spawnPosition);
        transform.rotation = spawnRotation;

        // 2. Receteamos fuerzas de RgidBody (aunque usamos Kinetic pero bueno)
        if (rb != null)
        {
            rb.velocity = Vector3.zero; // Resetea velocidad lineal
            rb.angularVelocity = Vector3.zero; // Resetea rotación física
        }

        // 3. Encendemos el interruptor
        gameObject.SetActive(true);
    }
    
    public void deactivate(){
        gameObject.SetActive(false);
        //¿Alguien esta escuchando? Sí si manda señal.
        onBulletDeactivated?.Invoke(this);
    }


}
