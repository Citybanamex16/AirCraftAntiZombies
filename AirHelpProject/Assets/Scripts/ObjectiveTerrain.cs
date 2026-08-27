using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveTerrain : MonoBehaviour
{

    private Terrain terrenoNativo;

    // Awake() es el equivalemte de enter_tree() de Godot
    void Awake()
    {
        // 2. Buscamos el componente nativo 'Terrain' en este mismo GameObject
        // Esto es el equivalente en Godot a hacer: terrenoNativo = $Terrain o get_node("Terrain")
        terrenoNativo = GetComponent<Terrain>();

        if (terrenoNativo == null)
        {
            Debug.LogError("¡Objeto no Terrain Detectado! Devolviendo position como centro");
        }
    }



    //=== Funciones Publicas ===//
    public Vector3 get_center(){

        if (terrenoNativo == null) return transform.position;

        // 3. ¡Ahora sí! Accedemos al terreno nativo y a su propiedad 'terrainData'
        Vector3 pos = transform.position;
        Vector3 size = terrenoNativo.terrainData.size;

        float centroX = pos.x + (size.x/2f);
        float centroY = pos.y;
        float centroZ = pos.z + (size.z/2f);

        return new Vector3(centroX,centroY,centroZ);
    }


}
