using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rios : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if( other.tag == "Soldado" || other.tag == "Caballeria" || other.tag == "Catapulta"){
            Debug.Log("Soy el agua me ha tocado: "+other.gameObject.name+" se muere.");
            other.GetComponent<Unidad>().Morir();
        }
    }
}
