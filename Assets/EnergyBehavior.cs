using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBehavior : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _explosionFX;
    [SerializeField]
    private EnergyScales _energyScale;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.EnergyCollected(_energyScale);
            Instantiate(_explosionFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
