using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestionarAnimaciones : MonoBehaviour
{

    public UnidadManager miUnidadManager;
    public int miNumUnidad;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FinalizarAtaque(){
        if(miUnidadManager !=null)
            miUnidadManager.FinalizarAtaque();
    }
/*    
    public void GolpearCaballeria(){
        if(miUnidadManager != null)
            miUnidadManager.GolpearCaballeria();
    }
*/

/*
    public void FinalizarDefensa(){
        miUnidadManager.FinalizarDefensa();
    }
*/
    public void EliminarUnidad(){
        Debug.Log("GestionarAnimaciones.cs. Eliminando la unidad minNumUnidad");
        if(miUnidadManager != null)
            miUnidadManager.EliminarUnidad(miNumUnidad);
        else
            Destroy(this.gameObject);
    }
}
