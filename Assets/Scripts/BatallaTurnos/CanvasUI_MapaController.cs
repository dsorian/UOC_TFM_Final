using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



public class CanvasUI_MapaController : MonoBehaviour
{
    public TMP_Text textoOroPlayer1,textoOroPlayer2;
    public TMP_Text textoInfoEjercito;
    public GameObject IndicadorTurnoP1,IndicadorTurnoP2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EstablecerOroPlayers(int oroP1, int oroP2){
        textoOroPlayer1.text = oroP1+"";
        textoOroPlayer2.text = oroP2+"";
    }

    public void MostrarInfoEjercito(string texto){
        textoInfoEjercito.text = texto;
    }

    public void ResaltarTurnoPlayer(int numPlayer){
        if( numPlayer == 1){
            Color colorP1 = IndicadorTurnoP1.GetComponent<Image>().color;
            colorP1.a = 255.0f; // Cambia solo el canal alpha
            IndicadorTurnoP1.GetComponent<Image>().color = colorP1;

            Color colorP2 = IndicadorTurnoP2.GetComponent<Image>().color;
            colorP2.a = 0.0f; // Cambia solo el canal alpha
            IndicadorTurnoP2.GetComponent<Image>().color = colorP2;
        }else{
            if( numPlayer == 2){
            Color colorP1 = IndicadorTurnoP1.GetComponent<Image>().color;
            colorP1.a = 0.0f; // Cambia solo el canal alpha
            IndicadorTurnoP1.GetComponent<Image>().color = colorP1;

            Color colorP2 = IndicadorTurnoP2.GetComponent<Image>().color;
            colorP2.a = 255.0f; // Cambia solo el canal alpha
            IndicadorTurnoP2.GetComponent<Image>().color = colorP2;
        }
        }
    }
}
