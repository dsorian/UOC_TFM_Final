

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EstadoAtacando : IEstadoUnidad
{
        
    AI_CombateReal fsmAIController;
    private float tiempoActualDecision = 0.25f; //Para contar el tiempo que ha pasado desde la última decisión
    private float tiempoDecision = 0.5f;  //El agente tomará una decisión cada tiempoActualDecision segundos
    private float tiempoActualAtacando = 0.0f;  //Para contar el tiempo que lleva atacando y para en algún momento
    private float tiempoAtacando = 4.5f; //Atacará sin parar este tiempo
    private bool catapultaAtacando = false;  //Para saber si la catapulta está cargando
    private bool catapultaDisparando = false;  //Para saber si la catapulta está disparando
    private bool caballeriaAtacando = false;  //Para saber si la caballería está atacando
    public EstadoAtacando(AI_CombateReal laAI){
        fsmAIController = laAI;
    }

    public void ActualizaEstado()
    {
        tiempoActualDecision += Time.deltaTime;
        //Cada tiempoDecision comprueba si hay que hacer alguna otra acción (el destino se ha movido, o lo que sea)
        tiempoActualAtacando += Time.deltaTime; 
        if( tiempoActualDecision > tiempoDecision ){
            tiempoActualDecision = 0;
            Debug.Log("IA Real: EstadoAtacando. Toca decidir tiempoActualAtacando: "+tiempoActualAtacando+" elBatallaManager.unidadSeleccionadaP2: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2+" catapultaAtacando: "+catapultaAtacando);
            if(catapultaDisparando ){
                AEstadoEligiendo();
            }else{
                if( fsmAIController.elBatallaManager.unidadSeleccionadaP2 == 0){
                    if( !catapultaAtacando ){
                        Debug.Log("IA Real: EstadoAtacando: CATAPULTA: Inicio ataque catapulta.");
                        fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().IniciarAtaque();
                        catapultaAtacando = true;
                    }else{
                        tiempoActualAtacando = 0;
                        if( tiempoActualAtacando > tiempoAtacando){
                            //Como tarda mucho en cargar la dejo cargar todo lo que quiera
                            tiempoActualAtacando = 0;
                            //Debug.Log("IA Real: EstadoAtacando: Ya he atacado bastante, me voy a estado Eligiendo.");
                            //AEstadoEligiendo();
                        }else{
                            float distanciaObjetivo = Vector3.Distance(fsmAIController.unidadesManagerP1[fsmAIController.elBatallaManager.unidadSeleccionadaP1].transform.position,fsmAIController.unidadesManagerP2[0].transform.position);
                            Debug.Log("IA Real: EstadoAtacando: CATAPULTA La catapulta calcula la distancia al objetivo donde debe ir para disparar. distanciaObjetivo: "+distanciaObjetivo+" fuerzaCatapulta: "+fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().fuerzaCatapulta);
                            //El punto de impacto será el calculado +/- 4 unidades para que no sea siempre lo mismo
                            float variacion = (UnityEngine.Random.Range(0,2)*2-1) * UnityEngine.Random.Range(0,4.0f);
                            //Disparo la catapulta y me voy a EstadoEligiendo
                            if( distanciaObjetivo + variacion <= fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().fuerzaCatapulta){
                                Debug.Log("IA Real: EstadoAtacando: Disparando catapulta!!!! y me voy a eligiendo distanciaObjetivo: "+distanciaObjetivo);
                                Debug.Log("IA Real: EstadoAtacando: La catapulta dispara y acaba.");
                                catapultaAtacando = false;
                                fsmAIController.unidadesManagerP2[0].GetComponent<UnidadManager>().DispararCatapulta();
                                catapultaDisparando = true;
                                //fsmAIController.unidadesManagerP2[0].GetComponent<UnidadManager>().FinalizarAtaque();
                            }
                        }

                    }
                }else{
                    Debug.Log("IA Real: EstadoAtacando: tiempoActualAtacando - tiempoAtacando: "+tiempoActualAtacando+" - "+ tiempoAtacando+" unidadcontrolada: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2);
                    if( tiempoActualAtacando > tiempoAtacando){
                        Debug.Log("IA Real: EstadoAtacando: Ya he atacado bastante, me voy a estado Eligiendo.");
                        AEstadoEligiendo();
                    }else{
                        Debug.Log("IA Real: EstadoAtacando:  No he atacado, iniciando ataque. unidadControlada: "+fsmAIController.elBatallaManager.unidadSeleccionadaP2);
                        fsmAIController.unidadesManagerP2[fsmAIController.elBatallaManager.unidadSeleccionadaP2].GetComponent<UnidadManager>().IniciarAtaque();
                        if(fsmAIController.elBatallaManager.unidadSeleccionadaP2 == 2)
                            caballeriaAtacando = true;
                        else
                            caballeriaAtacando = false;}
                }
            }
        }
    }

    public void AEstadoAndando(){
        Debug.Log("IA Real: EstadoAtacando: A EstadoAndando...");
        tiempoActualAtacando = 0;
        catapultaAtacando = false;
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
                fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        if(fsmAIController.elBatallaManager.unidadSeleccionadaP2 == 2)
            fsmAIController.elBatallaManager.elSoundManager.PlayRandomSound(fsmAIController.elBatallaManager.elSoundManager.sonidosCaballos,0.5f,"Batalla");
        fsmAIController.elBatallaManager.elSoundManager.PlayMusic(fsmAIController.elBatallaManager.elSoundManager.musicaUnidadSeleccionada[fsmAIController.elBatallaManager.unidadSeleccionadaP2],true,0.5f,"UnidadSeleccionadaP2Source");

        fsmAIController.estadoActual = fsmAIController.estadoAndando;
    }

    public void AEstadoEligiendo(){
        Debug.Log("IA Real: EstadoAtacando: A EstadoEligiendo...");
        //Estado hay que ponerlo en el Actualizarestado y esperar para que reproduzca la animación de ataque y luego volver aquí
        /*
        if( fsmAIController.elBatallaManager.unidadSeleccionadaP2 == 0){
            Debug.Log("IA Real: EstadoAtacando: La catapulta dispara y acaba.");
            catapultaAtacando = false;
            fsmAIController.unidadesManagerP2[0].GetComponent<UnidadManager>().DispararCatapulta();
            //fsmAIController.unidadesManagerP2[0].GetComponent<UnidadManager>().FinalizarAtaque();
        }
        */
        tiempoActualAtacando = 0;
        catapultaAtacando = false;
        catapultaDisparando = false;
        caballeriaAtacando = false;
        fsmAIController.estadoActual = fsmAIController.estadoEligiendo;
    }

    public void AEstadoAtacando(){
        Debug.Log("IA Real: Unidad: ... de EstadoAtacando a aEstadoAtacando.");
    }

    public void AEstadoDefendiendo(){
        catapultaAtacando = false;
        if( fsmAIController.elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
            fsmAIController.elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
        Debug.Log("IA Real: Unidad: ... de EstadoAtacando a estadodefendiendo");
    }
    public void AEstadoDerrotado(){
        Debug.Log("IA Real: EstadoAtacando: AEstadoDerrotado");
        tiempoActualAtacando = 0;
        catapultaAtacando = false;
        fsmAIController.estadoActual = fsmAIController.estadoDerrotado;
    }
}
