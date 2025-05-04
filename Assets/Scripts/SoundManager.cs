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

/*Volver a poner el volumen, de momento lo pongo fuerte para testear */

    [Range(0f, 1f)] public float globalVolume = 1.0f;

    // Start is called before the first frame update
    void Start(){
        PlayMusic(musicaMapa[0],true,0.02f,"Mapa");
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
    public void PlaySound(AudioClip[] soundArray, int index, float volume = 1.0f, string origen = "Mapa")
    {
        if( origen == "Mapa"){
            if (soundArray != null && index >= 0 && index < soundArray.Length)
            {
                mapaSource.PlayOneShot(soundArray[index], volume * globalVolume);
            }
            else
            {
                Debug.LogWarning("PlaySound: Índice fuera de rango o array nulo.");
            }
        }else if( origen == "Batalla"){
            if (soundArray != null && index >= 0 && index < soundArray.Length)
            {
                batallaSource.PlayOneShot(soundArray[index], volume * globalVolume);
            }
            else
            {
                Debug.LogWarning("PlaySound: Índice fuera de rango o array nulo.");
            }
        }
    }


    /// Reproduce un sonido aleatorio de un array.
    public void PlayRandomSound(AudioClip[] soundArray, float volume = 1.0f, string origen = "Mapa")
    {
        if( origen == "Mapa"){
            if (soundArray != null && soundArray.Length > 0)
            {
                int randomIndex = Random.Range(0, soundArray.Length);
                UnidadSeleccionadaP1Source.PlayOneShot(soundArray[randomIndex], volume * globalVolume);
            }
            else
            {
                Debug.LogWarning("PlayRandomSound: Array vacío o nulo.");
            }
        }else if( origen == "Batalla"){
            if (soundArray != null && soundArray.Length > 0)
            {
                int randomIndex = Random.Range(0, soundArray.Length);
                batallaSource.PlayOneShot(soundArray[randomIndex], volume * globalVolume);
            }
            else
            {
                Debug.LogWarning("PlayRandomSound: Array vacío o nulo.");
            }
        }
    }


    /// Reproduce música de fondo.
    public void PlayMusic(AudioClip musicClip, bool loop = true, float volume = 1.0f, string origen = "Mapa")
    {
        if( origen == "Mapa"){
            if (musicClip != null)
            {
                mapaSource.clip = musicClip;
                mapaSource.volume = volume * globalVolume;
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
                batallaSource.volume = volume * globalVolume;
                batallaSource.loop = loop;
                batallaSource.Play();
            }
            else
            {
                Debug.LogWarning("PlayMusic: Clip de música nulo.");
            }
        }else if( origen == "UnidadSeleccionadaP1Source"){
            if (musicClip != null)
            {
                UnidadSeleccionadaP1Source.clip = musicClip;
                UnidadSeleccionadaP1Source.volume = volume * globalVolume;
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
                UnidadSeleccionadaP2Source.volume = volume * globalVolume;
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
    
    /// Ajusta el volumen global.
    public void SetGlobalVolume(float volume, string origen = "Mapa")
    {
        if( origen == "Mapa"){
            globalVolume = Mathf.Clamp01(volume);
            mapaSource.volume = globalVolume;
        }else if( origen == "Batalla"){
            globalVolume = Mathf.Clamp01(volume);
            batallaSource.volume = globalVolume;
        }else if( origen == "UnidadSeleccionadaP1Source"){
            globalVolume = Mathf.Clamp01(volume);
            UnidadSeleccionadaP1Source.volume = globalVolume;
        }else if( origen == "UnidadSeleccionadaP2Source"){
            globalVolume = Mathf.Clamp01(volume);
            UnidadSeleccionadaP2Source.volume = globalVolume;
        }
    }
}