using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

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
    public GameObject videoRawImage;  //Objeto con la rawImage del vídeo
    public VideoPlayer elVideoPlayer; //Para reproducir la introel vídeo de la intro

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        //Limitar FPS: Application.targetFrameRate = 60
        Application.targetFrameRate = 60;
        // Suscribirse al evento de fin de vídeo
        if (elVideoPlayer != null)
            elVideoPlayer.loopPointReached += OnVideoFinished;
    }

    // Update is called once per frame
    void Update()
    {
        //Para pasar a la pantalla del trailer y grabarlo
        /*
        if (Input.GetKeyUp(KeyCode.T)){
            PlayerPrefs.SetInt("tutorialActivo", 0);
            SceneManager.LoadScene("Trailer", LoadSceneMode.Single);
        }
        */
    }

    public void EmpezarJuego()
    {
        UnityEngine.Debug.Log("Splash activo ini: " + menuSplashScreen.activeSelf);

        audioSource.PlayOneShot(clickBoton);

        menuSplashScreen.SetActive(false);
        menuJugadores.SetActive(true);
        menuTipoCombate.SetActive(false);
        UnityEngine.Debug.Log("Splash activo fin: " + menuSplashScreen.activeSelf);
    }


    public void Modo1Player()
    {
        audioSource.PlayOneShot(clickBoton);
        numPlayers = 1;
        menuSplashScreen.SetActive(false);
        menuJugadores.SetActive(false);
        menuTipoCombate.SetActive(true);
    }

    public void Modo2Player()
    {
        audioSource.PlayOneShot(clickBoton);
        numPlayers = 2;
        menuSplashScreen.SetActive(false);
        menuJugadores.SetActive(false);
        menuTipoCombate.SetActive(true);
    }

    public void IniciarJuegoTutorial()
    {
        UnityEngine.Debug.Log("Clickado iniciar juego tutorial.");
        audioSource.PlayOneShot(clickBoton);
        modoTurnos = 1;
        PlayerPrefs.SetInt("modoTurnos", modoTurnos);
        PlayerPrefs.SetInt("numPlayers", 1);
        PlayerPrefs.SetInt("tutorialActivo", 1);
        StartCoroutine(esperarCortinilla());
    }

    public void IniciarJuegoTurnos()
    {
        UnityEngine.Debug.Log("Clickado iniciar juego turnos.");
        audioSource.PlayOneShot(clickBoton);
        modoTurnos = 1;
        PlayerPrefs.SetInt("modoTurnos", modoTurnos);
        PlayerPrefs.SetInt("numPlayers", numPlayers);
        PlayerPrefs.SetInt("tutorialActivo", 0);
        StartCoroutine(esperarCortinilla());
    }

    public void IniciarJuegoAccion()
    {
        UnityEngine.Debug.Log("Clickado iniciar juego acción.");
        audioSource.PlayOneShot(clickBoton);
        modoTurnos = 0;
        PlayerPrefs.SetInt("modoTurnos", modoTurnos);
        PlayerPrefs.SetInt("numPlayers", numPlayers);
        PlayerPrefs.SetInt("tutorialActivo", 0);
        StartCoroutine(esperarCortinilla());
    }

    public IEnumerator esperarCortinilla()
    {
        laCortinilla.cortinaCerrada = false;
        while (laCortinilla.cortinaCerrada == false)
        {
            laCortinilla.PlayCloseCurtainAnimation(false);
            yield return new WaitForSeconds(2f);  //Esperamos 2 segundos para comprobar si la cortinilla está cerrada
        }
        SceneManager.LoadScene("Mapa", LoadSceneMode.Single);
    }

    public void VolverInicio()
    {
        audioSource.PlayOneShot(clickBoton);
        menuSplashScreen.SetActive(true);
        menuJugadores.SetActive(false);
        menuTipoCombate.SetActive(false);
    }

    public void VolverPlayers()
    {
        audioSource.PlayOneShot(clickBoton);
        menuSplashScreen.SetActive(false);
        menuJugadores.SetActive(true);
        menuTipoCombate.SetActive(false);
    }

    public void QuitGame()
    {
        audioSource.PlayOneShot(clickBoton);
        Application.Quit();
    }

    public void ReproducirIntro()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // Llamamos a JavaScript
            PlayVideoInWebGL();
        }
        else
        {
            // Reproducimos con Unity VideoPlayer
            PlayVideoInStandalone();
        }

        /*
        audioSource.Pause();
        elVideoIntro.SetActive(true);
        videoPlayer.Play();
        */
    }

    public void PararIntro()
    {
        audioSource.Play();
        elVideoPlayer.Stop();
        videoRawImage.GetComponent<RawImage>().texture = null;
        videoRawImage.SetActive(false);
    }

    //Para cerrar el vídeo cuando acabe
    private void OnVideoFinished(UnityEngine.Video.VideoPlayer vp){
        PararIntro();
    }

    void PlayVideoInStandalone()
    {
        /*
                unityVideoPlayer.Prepare();
                unityVideoPlayer.prepareCompleted += OnVideoPrepared;
                */
        UnityEngine.Debug.Log("Reproduciendo video en Standalone");
        audioSource.Pause();
        videoRawImage.SetActive(true);
        elVideoPlayer.Play();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
     ///   videoImage.texture = vp.texture;
        vp.Play();
    }

    void PlayVideoInWebGL(){
        UnityEngine.Debug.Log("Reproduciendo video en WebGL");

        #if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalCall(webJsFunctionName);
#endif
    }
}
