using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLand : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject model;
    [SerializeField] private ParticleSystem ps;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            rb.useGravity = true;
        } else if (other.gameObject.CompareTag("PaintTile"))
        {
            ps.Play();
            other.gameObject.GetComponent<PaintTile>().StartTempEnable(3);
            model.SetActive(false);
        }
    }
}
