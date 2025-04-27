using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private int modoTurnos = 1;  //1=true combate por turnos 0=false combate real
    private int numPlayers = 1; 
    public GameObject menuSplashScreen;
    public GameObject menuJugadores;
    public GameObject menuTipoCombate;
    //SONIDOS
    public AudioSource audioSource;
    public AudioClip clickBoton;
    public CurtainAnimator laCortinilla;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EmpezarJuego(){
        UnityEngine.Debug.Log("Splash activo ini: "+menuSplashScreen.activeSelf);

        audioSource.PlayOneShot(clickBoton);

        menuSplashScreen.SetActive(false);
        menuJugadores.SetActive(true);
        menuTipoCombate.SetActive(false);
        UnityEngine.Debug.Log("Splash activo fin: "+menuSplashScreen.activeSelf);
    }


    public void Modo1Player(){
        audioSource.PlayOneShot(clickBoton);
        numPlayers = 1;
        menuSplashScreen.SetActive(false);
        menuJugadores.SetActive(false);
        menuTipoCombate.SetActive(true);
    }

    public void Modo2Player(){
        audioSource.PlayOneShot(clickBoton);
        numPlayers = 2;
        menuSplashScreen.SetActive(false);
        menuJugadores.SetActive(false);
        menuTipoCombate.SetActive(true);
    }

    public void IniciarJuegoTutorial(){
        UnityEngine.Debug.Log("Clickado iniciar juego tutorial.");
        audioSource.PlayOneShot(clickBoton);
        modoTurnos = 1;
        PlayerPrefs.SetInt("modoTurnos", modoTurnos);
        PlayerPrefs.SetInt("numPlayers", 1);
        PlayerPrefs.SetInt("tutorialActivo", 1);
        StartCoroutine(esperarCortinilla());
    }

    public void IniciarJuegoTurnos(){
        UnityEngine.Debug.Log("Clickado iniciar juego turnos.");
        audioSource.PlayOneShot(clickBoton);
        modoTurnos = 1;
        PlayerPrefs.SetInt("modoTurnos", modoTurnos);
        PlayerPrefs.SetInt("numPlayers", numPlayers);
        PlayerPrefs.SetInt("tutorialActivo", 0);
        StartCoroutine(esperarCortinilla());
    }

    public void IniciarJuegoAccion(){
        UnityEngine.Debug.Log("Clickado iniciar juego acción.");
        audioSource.PlayOneShot(clickBoton);
        modoTurnos = 0;
        PlayerPrefs.SetInt("modoTurnos", modoTurnos);
        PlayerPrefs.SetInt("numPlayers", numPlayers);
        PlayerPrefs.SetInt("tutorialActivo", 0);
        StartCoroutine(esperarCortinilla());
    }

    public IEnumerator esperarCortinilla(){
        laCortinilla.cortinaCerrada = false;
        while( laCortinilla.cortinaCerrada == false ){
            laCortinilla.PlayCloseCurtainAnimation(false);
            yield return new WaitForSeconds(2f);  //Esperamos 2 segundos para comprobar si la cortinilla está cerrada
        }
        SceneManager.LoadScene("Mapa", LoadSceneMode.Single);
    }

    public void VolverInicio(){
        audioSource.PlayOneShot(clickBoton);
        menuSplashScreen.SetActive(true);
        menuJugadores.SetActive(false);
        menuTipoCombate.SetActive(false);
    }

    public void VolverPlayers(){
        audioSource.PlayOneShot(clickBoton);
        menuSplashScreen.SetActive(false);
        menuJugadores.SetActive(true);
        menuTipoCombate.SetActive(false);
    }

    public void QuitGame(){
        audioSource.PlayOneShot(clickBoton);
        Application.Quit();
    }
}
