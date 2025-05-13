using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EstadoAndando : IEstadoUnidad
{
    AI_CombateReal fsmAIController;
    private float tiempoActualDecision = 0.15f; //Para contar el tiempo que ha pasado desde la última decisión
    private float tiempoDecision = 0.3f;  //El agente tomará una decisión cada tiempoActualDecision segundos
    private float tiempoActualAndando = 0.15f;  //Para contar el tiempo que lleva andando y cambiar a otra unidad o realizar otra acción
    private float tiempoAndando = 2.5f; //Andará sin parar este tiempo


    public EstadoAndando(AI_CombateReal laAI){
        fsmAIController = laAI;
    }


    public void ActualizaEstado()
    {
        if( ! fsmAIController.combateRealActivo )
            return;
        tiempoActualDecision += Time.deltaTime;
        tiempoActualAndando += Time.deltaTime;
        //Cada tiempoDecision comprueba si hay que hacer alguna otra acción (el destino se ha movido, o lo que sea)
        if( tiempoActualDecision > tiempoDecision ){
            tiempoActualDecision = 0;
            float distanciaObjetivo;
            Debug.Log("IA Real: 1.-EstadoAndando de: "+fsmAIController.player+": Toca decidir. unidadControlada: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2);
            //CATAPULTA
            if(fsmAIController.elBatallaManager.unidadSeleccionadaP2 == 0 ){
                //Si tengo el objetivo a tiro, disparo 
                //Vector3 destinoCatapulta = new Vector3(fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position.x,fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position.y,fsmAIController.unidadesManagerP1[fsmAIController.numUnidadObjetivo].transform.position.z);

                distanciaObjetivo = Vector3.Distance(fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position,fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().GetVanguardia().transform.position);
                Debug.Log("IA Real: EstadoAndando:    CATAPULTA: La distancia con el objetivo es: "+distanciaObjetivo);
                Debug.Log("IA Real: EstadoAndando: "+fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position+"\n           "+fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().GetVanguardia().transform.position);
                if( distanciaObjetivo < 4.2f ){
                    Debug.Log("IA Real: 2.-EstadoAndando: de: "+fsmAIController.player+"    CATAPULTA: La distancia con el objetivo es: "+distanciaObjetivo+" Le ataco!!!!");
                    AEstadoAtacando();
                    return;
                }else   
                    Debug.Log("IA Real: 3.-EstadoAndando de: "+fsmAIController.player+"Estoy lejos, a: "+distanciaObjetivo+", no ataco.");
            }else{
                //RESTO DE UNIDADES
                //Cuando se movía unidadmanager poco a poco
                //distanciaObjetivo = Vector3.Distance(fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position, fsmAIController.unidadesManagerP1[fsmAIController.numUnidadObjetivo].transform.position);
                //if( distanciaObjetivo < 3.5f && fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().AlgunaUnidadTieneObjetivoCerca()){
                if( fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().IAPuedeAtacar()){
                    Debug.Log("IA Real: 4.-EstadoAndando de: "+fsmAIController.player+":    Unidad: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2+" Hay unidades que pueden atacar.  Le ataco!!!!");
                    AEstadoAtacando();
                }else{
                    Debug.Log("IA Real: 5.-EstadoAndando de: "+fsmAIController.player+": No hay unidades que puedan atacar. No hago nada.");
                }
            }
        }
        if(tiempoActualAndando > tiempoAndando){//Llevo el tiempo máximo andando, me paro y que mueva otro
            Debug.Log("IA Real: 6.-EstadoAndando de: "+fsmAIController.player+":    EstadoAndando, ya he andado suficiente, me voy a elegir: tiempos: "+tiempoActualAndando +" - "+ tiempoAndando+" UnidadControlada: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2);
            AEstadoEligiendo();
        }else{
            Debug.Log("IA Real: 7.-EstadoAndando de: "+fsmAIController.player+":    EstadoAndando, Aún no he andado suficiente. Sigo moviéndome. tiempoActualAndando: "+tiempoActualAndando +" tiempoAndando: "+ tiempoAndando+" UnidadControlada: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2);
            Vector3 vectorConObjetivo;  //Guarda la dirección hacia el objetivo
            if( fsmAIController.elBatallaManager.unidadSeleccionadaP2 == 0 ){     //Unidad es CATAPULTA
                int unidadObjetivo;
                //Vemos a qué objetivo disparar
                if( !fsmAIController.unidadesManagerP1[1].GetComponent<UnidadManager>().muerto){
                    unidadObjetivo = 1;
                }else if( !fsmAIController.unidadesManagerP1[2].GetComponent<UnidadManager>().muerto){
                    unidadObjetivo = 2;
                }else
                    unidadObjetivo = 0;
                Vector3 destinoCatapulta = new Vector3(fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position.x,fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position.y,fsmAIController.unidadesManagerP1[unidadObjetivo].transform.position.z);
                vectorConObjetivo = destinoCatapulta;//-fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position;
                Debug.Log("IA Real: 8.-EstadoAndando de: "+fsmAIController.player+": El vectorConObjetivo: "+vectorConObjetivo);
            }else{
                //Si queremos ir avanzando poco a poco en dirección al objetivo
                //vectorConObjetivo = fsmAIController.unidadesManagerP1[fsmAIController.numUnidadObjetivo].transform.position-fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position;
                //El objetivo es directamente la unidad que queremos atacar + el rango de ataque para no ponerse encima justo, sino un poco a la derecha
                vectorConObjetivo = fsmAIController.unidadesManagerP1[fsmAIController.numUnidadObjetivo].transform.position + new Vector3(fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().rangoAtaque-1,0,0);// + new Vector3(3,0,0);
                Debug.Log("IA Real: 9.-EstadoAndando de: "+fsmAIController.player+": vectorConObjetivo:"+vectorConObjetivo);
            }
            //Para que no se quede justo en el sitio sino un poco antes lo muevo 6 unidades a la derecha y se quedarán las vanguardias mirando
            fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].transform.position = vectorConObjetivo;
            fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().MoverUnidades();
            Debug.Log("IA Real: 10.-EstadoAndando de: "+fsmAIController.player+" la unidad: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2+" Nos estamos moviendo a vectorConObjetivo:"+vectorConObjetivo);
        }
    }

    public void AEstadoAndando(){
        Debug.Log("IA Real: 11.-EstadoAndando: A EstadoAndando...");
    }

    public void AEstadoEligiendo(){
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
            fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        Debug.Log("IA Real: 12.-EstadoAndando: A EstadoEligiendo... que ya he andado suficiente.");
        fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().PararUnidades();
        //Para que no anden siempre el mismo tiempo
        tiempoActualAndando = 0 + UnityEngine.Random.Range(0.0f,0.8f);
        fsmAIController.estadoActual = fsmAIController.estadoEligiendo;
    }

    public void AEstadoAtacando(){
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
            fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");

        Debug.Log("IA Real: 13.-EstadoAndando. Unidad: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2+" ... de estadoAndando a aEstadoAtacando. Inicio el ataque!!!");
        fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().PararUnidades();
        fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().IniciarAtaque();
        fsmAIController.estadoActual = fsmAIController.estadoAtacando;
    }

    public void AEstadoDefendiendo(){
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
            fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        Debug.Log("IA Real: 14.-Unidad: ... de estadoAndando a estadodefendiendo");
    }
    public void AEstadoDerrotado(){
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
            fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        Debug.Log("IA Real: 15.-EstadoDerrotado");
    }
}