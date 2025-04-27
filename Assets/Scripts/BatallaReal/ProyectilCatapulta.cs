using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProyectilCatapulta : MonoBehaviour
{
    public Unidad miCatapulta;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<ParticleSystem>().Stop();
        miCatapulta = transform.parent.gameObject.GetComponent<Unidad>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if( other.tag == "Catapulta" || other.tag == "Soldado" || other.tag == "Caballeria"){
            Debug.Log("Proyectil chocado con: "+other.tag);
            other.GetComponent<Unidad>().Morir();
        }
        if( other.tag == "Terrain"){
            gameObject.GetComponent<ParticleSystem>().Play();
            Debug.Log("Proyectil Impactado con Terrain. Me reinicio! FALTA");
        }
        miCatapulta.miUnidadManager.elBatallaManager.elSoundManager.PlayRandomSound(miCatapulta.miUnidadManager.elBatallaManager.elSoundManager.sonidosExplosion, 0.8f,"Batalla");
    }
}
