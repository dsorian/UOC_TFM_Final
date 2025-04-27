using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShootBar : MonoBehaviour
{
    void LateUpdate(){
        //Para mirar a cámara si el personaje gira, pero no lo hará. Lo comento
//        transform.parent.transform.LookAt(transform.position + Camera.main.transform.forward);
    }
    public Slider slider;
    public void SetMaxForce( float force){
        slider.maxValue = force;
        slider.value = force;
    }
    
    public void SetForce( float force ){
        slider.value = force;
        if(slider.value < 0)
            slider.value = 0;
        if(slider.value > slider.maxValue )
            slider.value = slider.maxValue;
    }

    public void ResetForce( ){
        slider.value = 0;
    }
}
