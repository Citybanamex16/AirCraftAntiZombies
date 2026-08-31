using UnityEngine;

public class GroundHit : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // Si las partículas terminaron su ciclo, destruimos el GameObject
        if (ps != null && !ps.IsAlive())
        {
            HitParticlePool.Instance.ReturnToPool(gameObject);
        }
    }
}