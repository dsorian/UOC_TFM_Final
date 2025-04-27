using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSelectionRaycaster : MonoBehaviour
{
    //Para que se vea bien
    //Posición cámara: 35,40,-80
    //Rotación cámara: 40,0,0

    public Camera _camera;
    private HexTile target;
    Ray ray;

    //para outline effect de chatgpt INI
    public Color highlightColor = Color.yellow;
    public Color defaultColor = Color.white;
    private GameObject lastHighlightedObject = null;
    //FIN

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        //ray = _camera.ScreenPointToRay(InputEvents.current.mousePosition);
        ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit)){
            Transform objectHit = hit.transform;

            //If the object has a selectable component on it, call it.
            if( objectHit.TryGetComponent<HexTile>(out target)){
                target.OnHighlightTile();
                //De momento toco el hexágono para ver que funciona
                target.OnDrawGizmosSelected();
            }

            //Para Outline effect de chatgpt INI
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject != lastHighlightedObject){
                if (lastHighlightedObject != null){
                    HighlightObject(lastHighlightedObject, false);
                }
                HighlightObject(hitObject, true);
                lastHighlightedObject = hitObject;
            }
            //Para Outline effect de chatgpt FIN

        }
        if(Input.GetMouseButtonUp(0)){
            //Debug.Log("boton izquierdo pulsado");
            ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)){
                Transform objectHit = hit.transform;

                //If the object has a selectable component on it, call it.
                if( objectHit.TryGetComponent<HexTile>(out target)){
                    target.OnSelectTile();
                }
            }

            //Para outline effect de chatgpt ini
            if (lastHighlightedObject != null){
                HighlightObject(lastHighlightedObject, false);
                lastHighlightedObject = null;
            }
            //Para outline effect de chatgpt fin
        }
    }

    //Para Outline effect de chatgpt
    void HighlightObject(GameObject obj, bool highlight)
    {
//        Debug.Log("HighlightObject: llamado clickado en: "+obj);
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.SetColor("_OutlineColor", highlight ? highlightColor : defaultColor);
        }
    }
}
