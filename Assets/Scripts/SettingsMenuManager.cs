using UnityEngine;
using UnityEngine.UI;

public class OpcionesMenuManager : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicaSlider;
    public Slider efectosSlider;

    [Header("Sound Manager")]
    public SoundManager soundManager; // Referencia al SoundManager

    private void Start()
    {
        Debug.Log("OpcionesMenuManager: Start soundManager.musicaVolume: " + soundManager.musicaVolume);
        // Inicializar los sliders con los valores actuales del SoundManager
        if (soundManager != null)
        {
            musicaSlider.value = soundManager.musicaVolume;
            efectosSlider.value = soundManager.efectosVolume;
        }
        else
        {
            Debug.LogWarning("SoundManager no asignado en SettingsMenuManager.");
        }

        // Añadir listeners para los sliders
        musicaSlider.onValueChanged.AddListener(CambiarVolumenMusica);
        efectosSlider.onValueChanged.AddListener(CambiarVolumenEfectos);
    }

    public void CambiarVolumenMusica(float valor)
    {
        if (soundManager != null)
        {
            //Debug.Log("Cambiando volumen de música a: " + valor);
            soundManager.musicaVolume = valor; // Actualizar el volumen en el SoundManager
            soundManager.SetGlobalMusicVolume(valor); // Aplicar el nuevo volumen global
        }
        else
        {
            Debug.LogWarning("SoundManager no asignado en SettingsMenuManager.");
        }
    }

    public void CambiarVolumenEfectos(float valor)
    {
        if (soundManager != null)
        {
            soundManager.efectosVolume = valor; // Actualizar el volumen en el SoundManager
            soundManager.SetGlobalSoundVolume(valor); // Aplicar el nuevo volumen global
        }
        else
        {
            Debug.LogWarning("SoundManager no asignado en SettingsMenuManager.");
        }
    }
}