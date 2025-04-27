using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capital : MonoBehaviour
{
    public int propietario = 0; //0=Nadie, 1=Player1, 2=Player2
    public GameObject ejercitoOcupante = null;
    public Material materialNeutral;
    public Material materialEjercito1;
    public Material materialEjercito2;
    public GameObject cubo1,cubo2; //Que forman la capital, para cambiarles el material
    public bool seleccionada = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivarCapital(Color color){
        seleccionada = true;
        //Ahora seleccionamos el estado, no la capital. No hace falta
        //transform.GetComponent<Light>().enabled = true;
        //transform.GetComponent<Light>().color = color;
    }

    public void DesactivarCapital(){
        seleccionada = false;
        //Ahora seleccionamos el estado, no la capital. No hace falta
        //transform.GetComponent<Light>().enabled = false;
    }

    public void SetPropietario(int elPropietario){
        this.propietario = elPropietario;
        if(propietario == 0){
            cubo1.GetComponent<MeshRenderer>().material = materialNeutral;
            cubo2.GetComponent<MeshRenderer>().material = materialNeutral;
        }
        if(propietario == 1){
            cubo1.GetComponent<MeshRenderer>().material = materialEjercito1;
            cubo2.GetComponent<MeshRenderer>().material = materialEjercito1;
        }
        if(propietario == 2){
            cubo1.GetComponent<MeshRenderer>().material = materialEjercito2;
            cubo2.GetComponent<MeshRenderer>().material = materialEjercito2;
        }
    }

    public int GetPropietario(){
        return propietario;
    }
    public void OcuparCapital(GameObject ocupante){
        propietario = ocupante.GetComponent<Ejercito>().numPlayer;
        ejercitoOcupante = ocupante;
        SetPropietario(propietario);
    }


    public void OcuparCapital(int aux,GameObject objeto){
        //Para que no den errores los backups que he hecho de TileManager.cs
    }
    public void DesOcuparCapital(int numPlayer, GameObject ocupante){
        this.ejercitoOcupante = null;
    }

    public GameObject GetEjercitoOcupante(){
        return ejercitoOcupante;
    }

    public void SetEjercitoOcupante(GameObject ocupante, int jugador){
        ejercitoOcupante = ocupante;
        this.propietario = jugador;
    }
}
