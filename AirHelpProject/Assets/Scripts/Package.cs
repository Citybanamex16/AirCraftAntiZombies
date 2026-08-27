using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Package : MonoBehaviour
{
    [Header("Canal de Eventos")]
    public TargetEventChannel targetChannel;

    [Header("Físicas de Lanzamiento")]
    public float speed = 20.0f;
    public float fuerzaCaida = 5.0f;

    private bool haImpactado = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Aplicamos un impulso inicial combinando el avance de la nave con una fuerza hacia abajo
        Vector3 velocidadInicial = (transform.forward * speed) + (Vector3.down * fuerzaCaida);
        rb.velocity = velocidadInicial;
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
}
