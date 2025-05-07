using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    SOLVED -A veces la IA no mueve nada hasta que le matan la catapulta (en el modo batalla real)
    SOLVED -La catapulta del player 2 no hace la animación de disparo
    -La catapulta del player 2 queda muerta pero la unidadManager sigue como activa y se queda sin mover nada.
    SOLVED -La catapulta da error si pulsas mientras el proyectil está en vuelo (en el throwSimulator)
    -La cortinilla a veces repite el cierre
    -El proyectiCatapulta se queda en el escneario cuando se acaba el combate y no se destruye (hay que destruirlo)

    -Combate:
        -Los caballos se quedan atascados al atacar repetidamente y la animación se queda congelada.
            SOLVED, creo, aumentando el tiempo de cooldown
        -La catapulta al morir a veces no desaparece y sigue en el campo de batalla en idle, pero está muerta y no puede acabar la partida

    SOLVED -Al tener más de una unidad, al respawnear la nueva se ponen mal las skins (en los caballos sale el jinete con armadura azul y el caballo con piel roja)
    SOLVED -En el primer combate la IA no mueve/dispara la catapulta
    SOLVED -Al clonar nuevas unidades se clonan mirando hacia arriba en lugar de a la derecha.
    SOLVER -Al acabar los combates desaparece el escenario y queda feo. Esperar a que se cierren los paneles para hacer el cambio
    SOLVED -No se muestran los 5 de oro al llegar para añadir la unidad. Se queda en 4 y se muestra la unidad.

    Testeo alumnos: 
    -No se indica que tienes más tropas en el modo batalla real. (¿Poner un icono que lo indique?)
    -Cuando cambias muchas veces rápido de unidad no se pueden mover las unidades, se quedan bloqueadas y sólo salen al atacar (Tal vez se tiene un valor raro en la animaciones)
    -Cuando ganas una unidad y la colocas pasas a tener 1 de oro en lugar de 0
    -SOLVED En el modo automático, el botón de fin de turno funciona para la máquina y le puedes hacer pasar sin mover
    -SOLVED Modo automático, no para de sumar oro y no da nuevas unidades
    
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
