using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AI_Turnos_SistemaReglas : MonoBehaviour{
    // Variables para el estado del juego
    public int soyPlayer = 2;
    public TileManager elTileManager;
    public MapaReino elMapaReino;
    public BatallaManager elBatallaManager;
    public bool turnoIAterminado = true;  //Ya ha jugado y no le toca a la IA

    //Borrar estas variables de ejemplo
    public int enemyHealth;
    public int playerHealth;
    public int enemyResources;
    public int playerResources;

    // Método para que la IA decida qué hacer en su turno
    public IEnumerator PerformAIActions()
    {
        //para cada ejército mío que puedo mover
        //Si puedo mover y ejército tiene enemigo adyacente y es igual o menos fuerte   (sacar un valor + un random)
            //Debug.Log("IA Soldado: AttackPlayer();");
        //si no si tengo aliado adyacente
            //Unir a ese ejército
        //si no si hay un estado enemigo vacío
            //Ocupar ese estado
        //si no
            //Ocupar un estado neutral
        Debug.Log("IA Soldado: PeformAIActions!!!!!");
        while( true){
            //Debug.Log("IA Soldado: AI_Turnos_SistemaReglas: Estoy funcionando...");
            //Obtenemos de qué tipo son los vecinos
            List<int> enemigosOcupados;
            List<int> enemigosDesOcupados;
            List<int> aliadosOcupados;
            List<int> aliadosDesOcupados;
            List<int> neutrales;
            if( turnoIAterminado || elTileManager.combateActivo )
                yield return new WaitForSeconds(1.0f);
            else{
                Debug.Log("IA Soldado: Inicio de tirada.*****************************************************************************");
                enemigosOcupados = new List<int>();
                enemigosDesOcupados = new List<int>();
                aliadosOcupados = new List<int>();
                aliadosDesOcupados = new List<int>();
                neutrales = new List<int>();
                //El player2 será siempre la IA mientras no lo cambie
                Debug.Log("IA Soldado: Soldados que tengo: "+elTileManager.jugadores[2].ejercitos.Count+" voy a ver mis opciones");
                for( int i=0; i< elTileManager.jugadores[2].ejercitos.Count;i++){
                    Debug.Log("IA soldado: Viendo si muevo el ejército: "+elTileManager.jugadores[2].ejercitos[i].GetComponent<Ejercito>().indiceEjercito+" del estado: "+elTileManager.jugadores[2].ejercitos[i].GetComponent<Ejercito>().currentTile.GetComponent<HexTile>().numEstado+" ===========INICIO ACCIÓN DE Soldado "+i+"================================");
                    elTileManager.GetComponent<TileManager>().OnSelectTile(elTileManager.jugadores[2].ejercitos[i].GetComponent<Ejercito>().currentTile);
                    if( ! elTileManager.jugadores[2].ejercitos[i].GetComponent<Ejercito>().haMovido){
                        //Obtenemos los vecinos, de quien son y si están ocupados o no
                        //0=no es vecino. Otro valor sí es vecino y además:
                        // 1=es neutral 2=Del enemigo y está libre 3=Del enemigo ocupada 4=Aliada libre 5=Aliada ocupada
                        int numEstado = elTileManager.jugadores[2].ejercitos[i].GetComponent<Ejercito>().currentTile.GetComponent<HexTile>().numEstado;
                        bool[] vecinos = elMapaReino.GetComponent<MapaReino>().listaEstados[numEstado].estadosVecinos;

                        enemigosOcupados.Clear();
                        enemigosDesOcupados.Clear();
                        aliadosOcupados.Clear();
                        aliadosDesOcupados.Clear();
                        neutrales.Clear();
                        for(int j=1 ; j<vecinos.Length; j++){  //j=1 para no coger el mar, montañas, etc
                            if( vecinos[j] == true ){ //false=no es vecino, lo ignoramos
                                Capital capitalVecina = elMapaReino.GetComponent<MapaReino>().capitalesEstados[j].GetComponent<Capital>();
                                if( capitalVecina.propietario == 0 ){ //Vecino neutral
                                    neutrales.Add(j);
                                    //Debug.Log("IA Soldado:  El estado: "+j+" es neutral.");
                                }else{
                                    if( capitalVecina.propietario != soyPlayer ){ //Vecino enemigo
                                        if( elMapaReino.GetComponent<MapaReino>().capitalesEstados[j].GetComponent<Capital>().ejercitoOcupante != null)
                                            enemigosOcupados.Add(j);
                                        else
                                            enemigosDesOcupados.Add(j);
                                        //Debug.Log("IA soldado: El estado: "+j+" es enemigo.");
                                    }
                                    if( capitalVecina.propietario == soyPlayer){  //Vecino aliado
                                        if( elMapaReino.GetComponent<MapaReino>().capitalesEstados[j].GetComponent<Capital>().ejercitoOcupante != null)
                                            aliadosOcupados.Add(j);
                                        else
                                            aliadosDesOcupados.Add(j);
                                        //Debug.Log("IA Soldado:  El estado: "+j+" es aliado.");
                                    }
                                }
                            }
                        }

                        int poderMiEjercito = elTileManager.jugadores[2].ejercitos[i].GetComponent<Ejercito>().GetPoder();
                        HexTile celdaDestinoAtaque = null;
                        HexTile celdaDestinoOcupacion = null;
                        int poderEnemigo = 1000;
                        int poderAux;
                        Ejercito ejercitoEnemigo = null;
                        int estadoDestinoOcupar = -1;
                        //PENSAR CÓMO ELEGIR NO ATACAR AL ENEMIGO Y CONQUISTAR UN ESTADO O **HACERLO RANDOM** creo que Random y au
                        Debug.Log("IA Soldado: Tengo enemigos ocupados: "+enemigosOcupados.Count+" y desocupados: "+enemigosDesOcupados.Count);
                        if( enemigosOcupados.Count > 0 || enemigosDesOcupados.Count > 0 ){//Enemigos cerca
                            //Elijo el enemigo más débil o igual al mío
                            foreach(int estadoEnemigo in enemigosOcupados){
                                Debug.Log("IA soldado: Calculando ataque al estado: "+estadoEnemigo+" Total enemigos ocupados: "+enemigosOcupados.Count);
                                ejercitoEnemigo = elMapaReino.GetComponent<MapaReino>().capitalesEstados[estadoEnemigo].GetComponent<Capital>().ejercitoOcupante.GetComponent<Ejercito>();
                                poderAux = ejercitoEnemigo.GetPoder();
                                if( poderAux < poderEnemigo ){
                                    poderEnemigo = poderAux;
                                    celdaDestinoAtaque = ejercitoEnemigo.currentTile;
                                }
                            }
                            //Elijo un enemigo desocupado random
                            if( enemigosDesOcupados.Count > 0){
                                estadoDestinoOcupar = enemigosDesOcupados[UnityEngine.Random.Range(0,enemigosDesOcupados.Count)];
                                Debug.Log("IA soldado: Ocuparé el estado enemigo vacío: "+estadoDestinoOcupar+" Total enemigos desocupados: "+enemigosDesOcupados.Count);
                                //ejercitoEnemigo = elMapaReino.GetComponent<MapaReino>().capitalesEstados[estadoEnemigo].GetComponent<Capital>().ejercitoOcupante.GetComponent<Ejercito>();
                                celdaDestinoOcupacion = elMapaReino.GetComponent<MapaReino>().GetTileCapital(estadoDestinoOcupar);
                            }
                            
                            if( poderEnemigo <= poderMiEjercito){
                                Debug.Log("IA soldado: La celda a atacar es: "+celdaDestinoAtaque.nombre);
                                //Si tengo neutrales o aliados a los que mover no atacaré si los enemigos son más fuertes que yo
                                elTileManager.GetComponent<TileManager>().OnSelectTile(celdaDestinoAtaque);
                            }else if( estadoDestinoOcupar != -1){
                                poderEnemigo=1000;
                                elTileManager.GetComponent<TileManager>().OnSelectTile(celdaDestinoOcupacion);
                            }

                        }
                        Debug.Log("IA soldado: miPoder: "+poderMiEjercito+" poderEnemigo: "+poderEnemigo+" estadoDestinoOcupar: "+estadoDestinoOcupar);
                        if(  estadoDestinoOcupar == -1 ){  //No ha encontrado enemigo igual o más débil ni sin ocupar, moveremos a una no ocupada
                            Debug.Log("IA Soldado: No he encontrado enemigo asequible, voy a moverme a un estado neutral o aliado. Num estados neutrales: "+neutrales.Count+" aliados ocupados: "+aliadosOcupados.Count+" aliados desocupados: "+aliadosDesOcupados.Count);
                            //Si no he atacado ni ocupado nada y tengo un enemigo más fuerte que yo, me muevo a un aliado o neutral
                            //Si hay neutrales moveremos a una random sí o sí (**Se podría poner un random**)
                            if( neutrales.Count > 0){ //Hay neutrales alrededor
                                estadoDestinoOcupar = neutrales[UnityEngine.Random.Range(0,neutrales.Count)];
                                Debug.Log("IA Soldado: Me voy al estado neutral: "+estadoDestinoOcupar);
                            }else{
                                Debug.Log("IA Soldado: No hay neutrales, voy a moverme a un aliado. Ocupado o no ocupado.");
                                if( aliadosOcupados.Count > 0 && poderMiEjercito < 12){  //Si no hemos atacado ni ocupado nada y mi poder es menor de 12 nos movemos a una aliada
                                    estadoDestinoOcupar = aliadosOcupados[UnityEngine.Random.Range(0,aliadosOcupados.Count)];
                                    Debug.Log("IA Soldado: Me voy a un estado aliado ocupado: "+estadoDestinoOcupar);
                                }else if( aliadosDesOcupados.Count > 0 ){
                                    estadoDestinoOcupar = aliadosDesOcupados[UnityEngine.Random.Range(0,aliadosDesOcupados.Count)];
                                    Debug.Log("IA Soldado: Me voy a un estado aliado desocupado: "+estadoDestinoOcupar);
                                }
                            }
                            celdaDestinoOcupacion = elMapaReino.GetComponent<MapaReino>().GetTileCapital(estadoDestinoOcupar);
                            Debug.Log("IA soldado: La celda neutral o aliada de destino es: "+celdaDestinoOcupacion.nombre);
                            elTileManager.GetComponent<TileManager>().OnSelectTile(celdaDestinoOcupacion);
                        }
                        //Si no ha atacado ni ocupado nada paso turno
                        if( ! elTileManager.jugadores[2].ejercitos[i].GetComponent<Ejercito>().haMovido ){
                            elTileManager.jugadores[2].ejercitos[i].GetComponent<Ejercito>().haMovido = true;
                            Debug.Log("IA soldado: NO HE ATACADO NI OCUPADO NADA, PASO TURNO!!!!");
                        }


                        //Para no pasar a la siguiente unidad hasta que la acción actual termine
                        while ( elTileManager.GetComponent<TileManager>().AccionActiva()){
                            Debug.Log("IA soldado: en el while accionactiva");
                            yield return new WaitForSeconds(1.0f);
                        }
                        //Si no, si estoy al 50% de fuerzas unirme a un vecino aliado

                        //Si no, conquistar un vecino neutral



                    }
                }
                yield return new WaitForSeconds(1.0f);
                Debug.Log("IA Soldado:  Se acabó LA TIRADA/////////////////////////////////////////////////////////////");
            }
        }
    }
}


/*
Si vamos ganando ( fuerza enemigos < fuerza aliados) //Prioridad a atacar
    Si tengo un enemigo próximo igual o más débil que yo
        Mover a su celda para atacarlo
    Si no Si tengo una zona enemiga vacía
        Ocuparla
    Si no Si tengo una zona neutral vacía 
        Ocuparla
    Si no Si tengo una zona aliada vacía
        Ocuparla
    Si no Si tengo una zona aliada ocupada
        Ocuparla para unirse
    fin si
si no //Prioridad ocupar

fin si
        
*/