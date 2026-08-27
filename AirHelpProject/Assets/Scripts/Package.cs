using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Package : MonoBehaviour
{
    [Header("Canal de Eventos")]
    public TargetEventChannel targetChannel;
    public GameMaster gameMaster;

    [Header("Físicas de Lanzamiento")]
    public float speed = 20.0f;
    public float fuerzaCaida = 5.0f;

    [Header("Vars de Package")]
    public float maxHp = 200f;
    private float hp = 200.0f;

    private bool haImpactado = false;
    private Rigidbody rb;

    private bool isDestroyed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Aplicamos un impulso inicial combinando el avance de la nave con una fuerza hacia abajo
        Vector3 velocidadInicial = (transform.forward * speed) + (Vector3.down * fuerzaCaida);
        rb.velocity = velocidadInicial;

        hp = maxHp;

        gameMaster.OnPackageSuccess += OnSucces;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Detecta impacto con el terreno
        if (!haImpactado && collision.gameObject.CompareTag("Ground"))
        {
            haImpactado = true;

            rb.velocity = Vector3.zero;
            rb.isKinematic = true; // Se congela firme en el suelo para los zombies

            if (targetChannel != null)
            {
                print("Setted and ready to go - Paquete en el terreno");
                targetChannel.RaiseEvent(this.transform);
                
            }
        }
    }


    public void hurted(float dp){
    hp -= dp;
    print($"[{GetInstanceID()}] PAQUETE DAÑADO, VIDA: {hp} | isDestroyed={isDestroyed} | frame={Time.frameCount}");
    
    if(hp <= 0 && !isDestroyed){
        isDestroyed = true;
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;   // 🛡️ Fuera del juego YA, antes de cualquier otra cosa
        
        if (gameMaster != null) gameMaster.OnPackageDestroyed();
        die();
        }
    }

    void die(){
        if (targetChannel != null) targetChannel.RaiseEvent(null);
        Destroy(gameObject);
    }

    void OnSucces(){
        print("Package, signal received from Game Master");
        die();

    }
}
