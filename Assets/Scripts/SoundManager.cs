using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource mapaSource;
    public AudioSource batallaSource;
    public AudioSource UnidadSeleccionadaP1Source;
    public AudioSource UnidadSeleccionadaP2Source;

    [Header("Audio Clips")]
    public AudioClip[] musicaMapa; //Música del mapa
    public AudioClip[] musicaBatalla; //Música de la batalla
    public AudioClip[] musicaUnidadSeleccionada; //Música de la unidad seleccionada
    public AudioClip[] sonidosMenu; //Sonidos del menú
    public AudioClip[] sonidosEspada; //Sonidos de golpe de espada
    public AudioClip[] sonidosGritoGolpe; //Sonidos de gritos de soldados
    public AudioClip[] sonidosMuerte; //Sonidos de muerte de soldados
    public AudioClip[] sonidosCaballos; //Sonidos de caballos
    public AudioClip[] sonidosCatapulta; //Sonidos de catapulta
    public AudioClip[] sonidosExplosion; //Sonidos de explosiones

    /*Volver a poner el volumen 0.02, de momento lo pongo fuerte para testear */

    [Range(0f, 1f)] public float musicaVolume = 0.05f;
    [Range(0f, 1f)] public float efectosVolume = 0.5f;
    // Start is called before the first frame update
    void Start(){
        PlayMusic(musicaMapa[0],true,musicaVolume,"Mapa");
    }

    /// Cambia el AudioListener activo según la cámara seleccionada.
    public void SetActiveCamera(Camera camera)
    {
        // Desactiva todos los AudioListeners
        foreach (var listener in FindObjectsOfType<AudioListener>())
        {
            listener.enabled = false;
        }

        // Activa el AudioListener de la cámara seleccionada
        if (camera != null)
        {
            var audioListener = camera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }
            else
            {
                Debug.LogWarning("La cámara seleccionada no tiene un AudioListener.");
            }
        }
    }


    /// Reproduce un sonido específico de un array.
    public void PlaySound(AudioClip[] soundArray, int index, float volume , string origen = "Mapa")
    {
        if( origen == "Mapa"){
            if (soundArray != null && index >= 0 && index < soundArray.Length)
            {
                mapaSource.PlayOneShot(soundArray[index], volume * efectosVolume);
            }
            else
            {
                Debug.LogWarning("PlaySound: Índice fuera de rango o array nulo.");
            }
        }else if( origen == "Batalla"){
            if (soundArray != null && index >= 0 && index < soundArray.Length)
            {
                batallaSource.PlayOneShot(soundArray[index], volume * efectosVolume);
            }
            else
            {
                Debug.LogWarning("PlaySound: Índice fuera de rango o array nulo.");
            }
        }
    }


    /// Reproduce un sonido aleatorio de un array.
    public void PlayRandomSound(AudioClip[] soundArray, float volume, string origen = "Mapa")
    {
        if( origen == "Mapa"){
            if (soundArray != null && soundArray.Length > 0)
            {
                int randomIndex = Random.Range(0, soundArray.Length);
                UnidadSeleccionadaP1Source.PlayOneShot(soundArray[randomIndex], volume * efectosVolume);
            }
            else
            {
                Debug.LogWarning("PlayRandomSound: Array vacío o nulo.");
            }
        }else if( origen == "Batalla"){
            if (soundArray != null && soundArray.Length > 0)
            {
                int randomIndex = Random.Range(0, soundArray.Length);
                batallaSource.PlayOneShot(soundArray[randomIndex], volume * efectosVolume);
            }
            else
            {
                Debug.LogWarning("PlayRandomSound: Array vacío o nulo.");
            }
        }
    }


    /// Reproduce música de fondo.
    public void PlayMusic(AudioClip musicClip, bool loop, float volume, string origen = "Mapa")
    {
        if( origen == "Mapa"){
            if (musicClip != null)
            {
                mapaSource.clip = musicClip;
                mapaSource.volume = volume * musicaVolume;
                mapaSource.loop = loop;
                mapaSource.Play();
            }
            else
            {
                Debug.LogWarning("PlayMusic: Clip de música nulo.");
            }
        }else if( origen == "Batalla"){
            if (musicClip != null)
            {
                batallaSource.clip = musicClip;
                batallaSource.volume = volume * musicaVolume;
                batallaSource.loop = loop;
                batallaSource.Play();
            }
            else
            {
                Debug.LogWarning("PlayMusic: Clip de música nulo.");
            }
        }else if( origen == "UnidadSeleccionadaP1Source"){
            //Para los sonidos de la batalla
            if (musicClip != null)
            {
                UnidadSeleccionadaP1Source.clip = musicClip;
                UnidadSeleccionadaP1Source.volume = volume * efectosVolume;
                UnidadSeleccionadaP1Source.loop = loop;
                UnidadSeleccionadaP1Source.Play();
            }
            else
            {
                Debug.LogWarning("PlayMusic: Clip de música nulo.");
            }
        }else if( origen == "UnidadSeleccionadaP2Source"){
            if (musicClip != null)
            {
                UnidadSeleccionadaP2Source.clip = musicClip;
                UnidadSeleccionadaP2Source.volume = volume * efectosVolume;
                UnidadSeleccionadaP2Source.loop = loop;
                UnidadSeleccionadaP2Source.Play();
            }
            else
            {
                Debug.LogWarning("PlayMusic: Clip de música nulo.");
            }
        }
    }


    /// Detiene la música de fondo.
    public void StopMusic(string origen = "Mapa"){
        if( origen == "Mapa"){
            mapaSource.Stop();
        }else if( origen == "Batalla"){
            batallaSource.Stop();
        }else if( origen == "UnidadSeleccionadaP1Source"){
            UnidadSeleccionadaP1Source.Stop();
        }else if( origen == "UnidadSeleccionadaP2Source"){
            UnidadSeleccionadaP2Source.Stop();
        }
    }
    
    /// Detiene todos los sonidos.
    public void StopAllSounds(string origen = "Mapa")
    {
        if( origen == "Mapa"){
            mapaSource.Stop();
        }else if( origen == "Batalla"){
            batallaSource.Stop();
        }else if( origen == "UnidadSeleccionadaP1Source"){
            UnidadSeleccionadaP1Source.Stop();
        }else if( origen == "UnidadSeleccionadaP2Source"){
            UnidadSeleccionadaP2Source.Stop();
        }
    }
    
    /// Ajusta el volumen global de la música.
    public void SetGlobalMusicVolume(float volume){
        musicaVolume = Mathf.Clamp01(volume);
                
        mapaSource.volume = musicaVolume;
        batallaSource.volume = musicaVolume;
    }

    /// Ajusta el volumen global de los efectos de sonido.
    public void SetGlobalSoundVolume(float volume){
        efectosVolume = Mathf.Clamp01(volume);
        
        UnidadSeleccionadaP1Source.volume = efectosVolume;
        UnidadSeleccionadaP2Source.volume = efectosVolume;
    }

}