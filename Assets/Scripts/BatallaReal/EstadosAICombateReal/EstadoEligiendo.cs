using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EstadoEligiendo : IEstadoUnidad
{
    
    AI_CombateReal fsmAIController;
    private float tiempoActualDecision = 0.15f; //Para contar el tiempo que ha pasado desde la última decisión
    private float tiempoDecision = 0.3f;  //El agente tomará una decisión cada tiempoActualDecision segundos

    public EstadoEligiendo(AI_CombateReal laAI){
        fsmAIController = laAI;
    }

    public void ActualizaEstado()
    {
        if( ! fsmAIController.combateRealActivo )
            return;
        Debug.Log("EstadoEligiendo 00: Inicio del bucle ActualizaEstado");
        tiempoActualDecision += Time.deltaTime;
        //Cada tiempoDecision comprueba si hay que hacer alguna otra acción (el destino se ha movido, o lo que sea)
        fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().PararUnidades();
        
        if( tiempoActualDecision > tiempoDecision ){
            tiempoActualDecision = 0;
            Debug.Log("EstadoEligiendo 01 tomando decisión. Unidad controlada: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2);
            fsmAIController.elBatallaManager.unidadSeleccionadaP2 = fsmAIController.elBatallaManager.SiguienteUnidadManagerP2();
            if( fsmAIController.elBatallaManager.unidadSeleccionadaP2 == -1 )
                AEstadoDerrotado();
            Debug.Log("EstadoEligiendo 02: Toca decidir. unidadControlada: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2);
            if( fsmAIController.RestoUnidadesEstanIdle(fsmAIController.elBatallaManager.unidadSeleccionadaP2) ){  //A ver qué nos hará hacer una cosa u otra, de momento siempre echamos a andar
                Debug.Log("EstadoEligiendo 03:    Resto de unidades Idle, me muevo "+fsmAIController.elBatallaManager.unidadSeleccionadaP2+" - "+fsmAIController.numUnidadObjetivo);
                AEstadoAndando();
            }else{
                Debug.Log("EstadoEligiendo 04:    Alguien está haciendo algo. No hago nada.");
            }
        }
    }

    public void AEstadoAndando(){
        Debug.Log("estadoEligiendo: A EstadoAndando... elBatallaManager.unidadSeleccionadaP2: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2);
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
                fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        if(fsmAIController.elBatallaManager.unidadSeleccionadaP2 == 2)
            fsmAIController.elBatallaManager.elSoundManager.PlayRandomSound(fsmAIController.elBatallaManager.elSoundManager.sonidosCaballos,0.5f,"Batalla");
        fsmAIController.elBatallaManager.elSoundManager.PlayMusic(fsmAIController.elBatallaManager.elSoundManager.musicaUnidadSeleccionada[fsmAIController.elBatallaManager.unidadSeleccionadaP2],true,0.5f,"UnidadSeleccionadaP2Source");
        //FALTA: 
        //Si da tiempo: Elegir el objetivo más adecuado (por proximidad, defender a un aliado, etc)
        //Ahora: Cojo la más próxima
        float distancia = 10000;
        float distanciaAux = 10000;
        //Elegimos objetivo más cercano
        for (int i=0; i<fsmAIController.unidadesManagerP1.Length; i++){
            if( ! fsmAIController.unidadesManagerP1[i].GetComponent<UnidadManager>().destruida)
                distanciaAux = Vector3.Distance(fsmAIController.unidadesManagerP1[i].transform.position, fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position);
            if( distanciaAux < distancia ){
                distancia = distanciaAux;
                fsmAIController.numUnidadObjetivo = i;
            }
        }
        fsmAIController.estadoActual = fsmAIController.estadoAndando;
    }

    public void AEstadoEligiendo(){
        Debug.Log("EstadoEligiendo: A EstadoEligiendo...");
    }

    public void AEstadoAtacando(){
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
            fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        Debug.Log("Unidad: ... de estadoEligiendo a aEstadoAtacando.");
    }

    public void AEstadoDefendiendo(){
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
            fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        Debug.Log("Unidad: ... de estadoEligiendo a estadodefendiendo");
    }

    public void AEstadoDerrotado(){
        Debug.Log("AEstadoDerrotado");
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
            fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        fsmAIController.estadoActual = fsmAIController.estadoDerrotado;
    }
}
