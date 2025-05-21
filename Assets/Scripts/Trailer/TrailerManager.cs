using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TrailerManager : MonoBehaviour
{
    public TrailerCamera trailerCamera;
    private string[] textosTrailer = {
        "In an age long forgotten... when the world was still young and the wind whispered the names of kings long dead...\nThere lay a realm divided by greed, bound by steel, and shrouded in silence.",
        "A fragile peace held the land… a truce forged not with words, but with blood. These were days of shadow… where the will of men was carved by the edge of a blade.",
        "Now, as old alliances crumble and the drums of war echo once more… you must rise.\nLead thy warriors through bitter frost and burning fields. Command thy hosts with wisdom… and strike with the fury of steel.",
        "Each choice shall shape thy fate.\nEach battle shall test thy mettle.\nWill thine arm be steady? Will thy banner rule the continent?",
        "The time has come… to conquer, or be forgotten. The hegemonic wars have begun.",
        "Move your armies wisely across the map, and lead them through the field of battle unto victory",
        "Only the wisest and most skilled shall seize absolute power. Are you ready to accept the challenge?"
    };
    public bool grabandoTrailer = false; //Para grabar el trailer para controlar los eventos del mismo
    private int estadoTrailer = 0; //Para el trailer, para saber en qué parte del trailer estamos
    private TileManager elTileManager;


    // Start is called before the first frame update
    void Start()
    {
        elTileManager = gameObject.GetComponent<TileManager>();
        elTileManager.grabandoTrailer = true;
        elTileManager.contadorTurnos = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //Para las acciones del trailer
        if (grabandoTrailer)
        {
            if (Input.GetKeyUp(KeyCode.T))
            {
                Debug.Log("T pulsada: contadorTurnos="+elTileManager.contadorTurnos);
                switch (estadoTrailer)
                {
                    case 0:
                        //Mostramos el primer texto y nada más
                        elTileManager.elCanvasUI_Mapa.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = textosTrailer[estadoTrailer];
                        break;
                    case 1:
                        //Clonamos dos ejércitos de cada jugador en el mapa
                        elTileManager.currentPlayer = 2;
                        elTileManager.contadorTurnos = 2;
                        elTileManager.elCanvasUI_Mapa.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = textosTrailer[estadoTrailer];
                        //Player 2 estado 3 y 6
                        elTileManager.AddEjercito(elTileManager.currentPlayer);
                        elTileManager.ejercitoSeleccionado = elTileManager.jugadores[2].ejercitos[0];
                        elTileManager.numTropaNueva = elTileManager.jugadores[2].ejercitos.Count - 1;
                        elTileManager.ColocarEjercito(elTileManager.elMapaReino.GetComponent<MapaReino>().GetTileCapital(3));
                        elTileManager.AddEjercito(elTileManager.currentPlayer);
                        elTileManager.ejercitoSeleccionado = elTileManager.jugadores[2].ejercitos[1];
                        elTileManager.numTropaNueva = elTileManager.jugadores[2].ejercitos.Count - 1;
                        elTileManager.ColocarEjercito(elTileManager.elMapaReino.GetComponent<MapaReino>().GetTileCapital(6));
                        //
                        elTileManager.currentPlayer = 1;
                        elTileManager.contadorTurnos = 1;
                        elTileManager.elCanvasUI_Mapa.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = textosTrailer[estadoTrailer];
                        //Player 1 estado 9 y 10
                        elTileManager.AddEjercito(elTileManager.currentPlayer);
                        elTileManager.ejercitoSeleccionado = elTileManager.jugadores[1].ejercitos[0];
                        elTileManager.numTropaNueva = elTileManager.jugadores[1].ejercitos.Count - 1;
                        elTileManager.ColocarEjercito(elTileManager.elMapaReino.GetComponent<MapaReino>().GetTileCapital(9));
                        elTileManager.AddEjercito(elTileManager.currentPlayer);
                        elTileManager.ejercitoSeleccionado = elTileManager.jugadores[1].ejercitos[1];
                        elTileManager.numTropaNueva = elTileManager.jugadores[1].ejercitos.Count - 1;
                        elTileManager.ColocarEjercito(elTileManager.elMapaReino.GetComponent<MapaReino>().GetTileCapital(10));

                        break;
                    case 2:
                        //El player 1 ocupa un estado
                        elTileManager.contadorTurnos = 1;
                        elTileManager.currentPlayer = 1;
                        elTileManager.elCanvasUI_Mapa.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = textosTrailer[estadoTrailer];
                        //elTileManager.numEstadoAnterior = 10;
                        //elTileManager.numEstadoActual = 8;
                        elTileManager.estadoSeleccionado = false;
                        elTileManager.OnSelectTile(elTileManager.elMapaReino.GetComponent<MapaReino>().GetTileCapital(10));
                        elTileManager.estadoSeleccionado = true;
                        elTileManager.ejercitoSeleccionado = elTileManager.jugadores[1].ejercitos[1];
                        elTileManager.OnSelectTile(elTileManager.elMapaReino.GetComponent<MapaReino>().GetTileCapital(8));
                        break;
                    case 3:
                        //El player 2 ataca al player 1 en automático
                        elTileManager.contadorTurnos = 2;
                        elTileManager.currentPlayer = 2;
                        PlayerPrefs.SetInt("modoTurnos", 1);
                        PlayerPrefs.SetInt("numPlayers", 2);
                        elTileManager.modoTurnos = true;
                        elTileManager.oponenteCPU = false;
                        elTileManager.elCanvasUI_Mapa.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = textosTrailer[estadoTrailer];
                        elTileManager.estadoSeleccionado = false;
                        elTileManager.OnSelectTile(elTileManager.elMapaReino.GetComponent<MapaReino>().GetTileCapital(3));
                        elTileManager.estadoSeleccionado = true;
                        elTileManager.ejercitoSeleccionado = elTileManager.jugadores[2].ejercitos[0];
                        elTileManager.OnSelectTile(elTileManager.elMapaReino.GetComponent<MapaReino>().GetTileCapital(9));
                        break;
                    case 4:
                        //El player 1 ataca al player 2 en manual
                        elTileManager.elCanvasUI_Mapa.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = textosTrailer[estadoTrailer];
                        elTileManager.contadorTurnos = 1;
                        elTileManager.currentPlayer = 1;
                        PlayerPrefs.SetInt("modoTurnos", 2);
                        PlayerPrefs.SetInt("numPlayers", 1);
                        elTileManager.modoTurnos = false;
                        elTileManager.oponenteCPU = true;
                        elTileManager.la_AI_Turnos.turnoIAterminado = true;
                        elTileManager.la_AI_Turnos.gameObject.SetActive(true);
                        elTileManager.la_AI_Real.gameObject.SetActive(true);
                        StartCoroutine(elTileManager.la_AI_Turnos.PerformAIActions());
                        elTileManager.elCanvasUI_Mapa.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = textosTrailer[estadoTrailer];
                        elTileManager.estadoSeleccionado = false;
                        elTileManager.OnSelectTile(elTileManager.elMapaReino.GetComponent<MapaReino>().GetTileCapital(8));
                        elTileManager.estadoSeleccionado = true;
                        if( elTileManager.jugadores[1].ejercitos.Count == 2)
                            elTileManager.ejercitoSeleccionado = elTileManager.jugadores[1].ejercitos[1];
                        else
                            elTileManager.ejercitoSeleccionado = elTileManager.jugadores[1].ejercitos[0];
                        elTileManager.OnSelectTile(elTileManager.elMapaReino.GetComponent<MapaReino>().GetTileCapital(6));
                        break;
                    case 5:
                        elTileManager.elCanvasUI_Batalla.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = textosTrailer[estadoTrailer];
                        break;
                    case 6:
                        elTileManager.elCanvasUI_Batalla.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = textosTrailer[estadoTrailer];
                        break;
                    default:
                        Debug.Log("ERROR en el trailer, estado no controlado");
                        break;
                }
                estadoTrailer++;
            }
        }
    }
}
