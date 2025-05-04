using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProyectilCatapulta : MonoBehaviour
{
    public Unidad miCatapulta;
    public bool disparado = false; //Para saber si el proyectil ha sido disparado o no.

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
        //Para que no mate unidades cuando esté en la catapulta
        if( disparado == false){
            return;
        }
        if( other.tag == "Catapulta" || other.tag == "Soldado" || other.tag == "Caballeria"){
            Debug.Log("Proyectil chocado con: "+other.tag);
            other.GetComponent<Unidad>().Morir();
        }
        if( other.tag == "Terrain" || other.tag == "TargetProyectil"){
            gameObject.GetComponent<ParticleSystem>().Play();
            Debug.Log("Proyectil Impactado con: "+other.tag+" Me reinicio!");
        }
        miCatapulta.miUnidadManager.elBatallaManager.elSoundManager.PlayRandomSound(miCatapulta.miUnidadManager.elBatallaManager.elSoundManager.sonidosExplosion, 0.8f,"Batalla");
        disparado = false;
    }
}
