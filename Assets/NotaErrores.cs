using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    -La pantalla de transición a la batalla no muestra los paneles. Salen en negro en la versión web (creo que está solucionado
    cuando muestre las escenas de combate)
    -A veces la IA no mueve nada hasta que le matan una unidad
    -La catapulta del player 2 no hace la animación de disparo
    -La catapulta del player 2 queda muerta pero la unidadManager sigue como activa y se queda sin mover nada.
    -La catapulta da error si pulsas mientras el proyectil está en vuelo (en el throwSimulator)

    -Combate:
        -Los caballos se quedan atascados al atacar repetidamente y la animación se queda congelada.
            SOLVED, creo aumentando el tiempo de cooldown
        -La catapulta al morir a veces no desaparece y sigue en el campo de batalla en idle, pero está muerta y no puede acabar la partida
    

    SOLVED -Al tener más de una unidad, al respawnear la nueva se ponen mal las skins (en los caballos sale el jinete con armadura azul y el caballo con piel roja)
    SOLVED -En el primer combate la IA no mueve/dispara la catapulta
    SOLVED -Al clonar nuevas unidades se clonan mirando hacia arriba en lugar de a la derecha.
    SOLVER -Al acabar los combates desaparece el escenario y queda feo. Esperar a que se cierren los paneles para hacer el cambio
    SOLVED -No se muestran los 5 de oro al llegar para añadir la unidad. Se queda en 4 y se muestra la unidad.
*/

public class NotaErrores : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
