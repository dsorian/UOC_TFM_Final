using System.Collections;
using UnityEngine;

public class CurtainAnimator : MonoBehaviour
{
    public RectTransform[] panels; // Los paneles que compondrán la cortinilla
    public Vector2[] startPositions; // Posiciones iniciales (fuera de pantalla)
    public Vector2[] endPositions; // Posiciones finales (cubriendo la pantalla)
    public float moveDuration = 1f; // Duración del movimiento
    public float displayDuration = 2f; // Tiempo que permanece el mensaje visible
    public GameObject targetContent; // El contenido que se mostrará después de la cortinilla
    public bool cortinaCerrada = false;

    private void Start()
    {
        // Nos aseguramos de que los paneles comiencen en las posiciones iniciales
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].anchoredPosition = startPositions[i];
        }
        //PlayCurtainAnimation();
    }

    public void PlayFullCurtainAnimation()
    {
        StartCoroutine(CurtainSequence());
    }

    public void PlayOpenCurtainAnimation(bool mostrarContenido)
    {
        StartCoroutine(AbrirCortina(mostrarContenido));
    }

    public void PlayCloseCurtainAnimation(bool mostrarContenido)
    {
        StartCoroutine(CerrarCortina(mostrarContenido));
    }

    private IEnumerator CurtainSequence()
    {
        // Fase 1: Mover los paneles desde los bordes hacia las posiciones finales
        yield return StartCoroutine(MovePanels(startPositions, endPositions));

        cortinaCerrada = true;
        // Aquí puedes realizar cambios en los objetos de la escena
        if (targetContent != null)
        {
            targetContent.SetActive(true);
        }

        // Fase 2: Mostrar el mensaje durante el tiempo especificado
        yield return new WaitForSeconds(displayDuration);
        
        cortinaCerrada = false;
        // Fase 3: Retirar los paneles hacia las posiciones iniciales
        yield return StartCoroutine(MovePanels(endPositions, startPositions));
        targetContent.SetActive(false);
    }

    private IEnumerator CerrarCortina(bool mostrarContenido)
    {
        cortinaCerrada = false;
        // Mover los paneles desde las posiciones iniciales hacia las posiciones finales
        yield return StartCoroutine(MovePanels(startPositions, endPositions));
        cortinaCerrada = true;

        // Controlar la visibilidad del targetContent
        if (targetContent != null)
        {
            targetContent.SetActive(mostrarContenido);
        }
    }

    private IEnumerator AbrirCortina(bool mostrarContenido)
    {
        // Controlar la visibilidad del targetContent antes de abrir la cortina
        if (targetContent != null)
        {
            targetContent.SetActive(mostrarContenido);
        }
        cortinaCerrada = true;
        // Mover los paneles desde las posiciones finales hacia las posiciones iniciales
        yield return StartCoroutine(MovePanels(endPositions, startPositions));
        cortinaCerrada = false;
    }

    private IEnumerator MovePanels(Vector2[] fromPositions, Vector2[] toPositions)
    {
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;

            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].anchoredPosition = Vector2.Lerp(fromPositions[i], toPositions[i], elapsedTime / moveDuration);
            }

            yield return null;
        }
        // Nos aseguramos de que estén exactamente en las posiciones finales
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].anchoredPosition = toPositions[i];
        }
    }
}
