using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/*
    Esto es de soldado.cs del strategyGame01
*/
public class Unidad : MonoBehaviour
{
    public NavMeshAgent miNavMeshAgent;
    public Animator miAnimator;
    public string player; //Guardar a qué player pertenece (Player1 o Player2)
    public Transform targetFollowed; //Objeto de la UnidadManager que debe seguir
    public GameObject objetivo = null; //Objetivo al que vamos a atacar

    public UnidadManager miUnidadManager;
    public int numeroUnidad;
    public string[] animaciones;
    public ThrowSimulation elThrowSimulation;  //Para controlar el disparo del proyectil (si es catapulta)

    public bool idle = true, andando = false, atacando = false, defendiendo = false, muerto = false, cargandoCatapulta = false;

    public GameObject unitSelector;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if ( muerto ){
            //Debug.Log("EStoy muerto, me debería haber muerto...");
            return;
        }

        //Para que la caballería pueda atacar mientras mueve
        if( atacando && miUnidadManager.tipoUnidad == 2 ){
            miNavMeshAgent.isStopped = false;
            setMyDestination(targetFollowed);
        }

        if (Input.GetKeyUp(KeyCode.X)){
            Debug.Log("Atacando!!!!!");
            idle = false;
            atacando = true;
            defendiendo = false;
            muerto = false;
            EstablecerAnimaciones();
            //Destroy(objetivo);
            if(objetivo != null){
                Debug.Log("Destruyendo a : "+ objetivo.name);
                objetivo.GetComponent<Unidad>().Morir();
            }
        }

    }

    public void setMyDestination(Transform dest){
        if( muerto)
            return;
        targetFollowed = dest;
        //transform.LookAt(new Vector3(100,0,0));
        miNavMeshAgent.SetDestination(targetFollowed.transform.position);
    }

    public void Parar(){
        if( muerto){
            return;
        }
        idle = true;
        andando = false;
        atacando = false;
        cargandoCatapulta = false;
        defendiendo = false;
        muerto = false;
        EstablecerAnimaciones();
        miNavMeshAgent.SetDestination(transform.position);
        miNavMeshAgent.isStopped = true;
    }

    public void Andar(){
        if( muerto)
            return;
        miNavMeshAgent.isStopped = false;
        setMyDestination(targetFollowed);
        idle = false;
        andando = true;
        atacando = false;
        cargandoCatapulta = false;
        defendiendo = false;
        muerto = false;
        EstablecerAnimaciones();
    }


    public void continuarAtacandoCaballeria(){
        if( muerto)
            return;
        Debug.Log("Siguiendo Atacando caballería");
        miNavMeshAgent.isStopped = false;
        setMyDestination(targetFollowed);
        idle = false;
        atacando = true;
        cargandoCatapulta = false;
        defendiendo = false;
        muerto = false;
    }
    public void IniAtacar(){
        if( muerto)
            return;
        Debug.Log("ZAS! Golpeando!");
        miNavMeshAgent.isStopped = true;
        idle = false;
        andando = false;
        atacando = true;
        cargandoCatapulta = false;
        defendiendo = false;
        muerto = false;
        EstablecerAnimaciones();
    }

    public void IniAtacarCaballeria(){
        if( muerto)
            return;
        Debug.Log("Unidad: Atacando caballería");
        miNavMeshAgent.isStopped = false;
        setMyDestination(targetFollowed);
        idle = false;
        atacando = true;
        andando = true;
        cargandoCatapulta = false;
        defendiendo = false;
        muerto = false;
        EstablecerAnimaciones();
    }

    public void FinAtacarCaballeria(){
        if( muerto)
            return;
        if( objetivo != null && Vector3.Distance(transform.position,objetivo.transform.position) < 5.5f){
            Debug.Log("ZASCA!!! Soy caballero de "+player+", he golpeado a: "+objetivo.GetComponent<Unidad>().tag +" que es de "+objetivo.GetComponent<Unidad>().player+" Distancia: "+Vector3.Distance(transform.position,objetivo.transform.position));
            miUnidadManager.elBatallaManager.elSoundManager.PlayRandomSound(miUnidadManager.elBatallaManager.elSoundManager.sonidosMuerte, 0.8f,"Batalla");            
            objetivo.GetComponent<Unidad>().Morir();
        }
        miUnidadManager.atacando = false;
        miUnidadManager.andando = false;
        Parar();
    }

    public void FinAtacar(){
        if( muerto)
            return;
        Debug.Log("Se acabó el ataque. De: "+player);
        //Si golpeo y mi objetivo está a menos de ?? me lo cargo

        if( objetivo != null && Vector3.Distance(transform.position,objetivo.transform.position) < 5.5f){
            Debug.Log("ZASCA!!! Soy "+player+", he golpeado a: "+objetivo.GetComponent<Unidad>().tag +" que es de "+objetivo.GetComponent<Unidad>().player+" Distancia: "+Vector3.Distance(transform.position,objetivo.transform.position));
            if( !objetivo.GetComponent<Unidad>().defendiendo )
                objetivo.GetComponent<Unidad>().Morir();
        }
        miUnidadManager.atacando = false;
        Parar();
    }

    public void Defender(){
        if( muerto)
            return;
        miNavMeshAgent.isStopped = true;
        idle = false;
        andando = false;
        atacando = false;
        cargandoCatapulta = false;
        defendiendo = true;
        muerto = false;
        EstablecerAnimaciones();
    }

    public void Morir(){
        Debug.Log("Unidad.Morir(): Unidad"+miUnidadManager.tipoUnidad+" de "+player+" ha muerto.");
        if(miUnidadManager.tipoUnidad == 0){
            if(elThrowSimulation != null){
                elThrowSimulation.DestruirProyectil();
            }
        }
        miUnidadManager.elBatallaManager.elSoundManager.PlayRandomSound(miUnidadManager.elBatallaManager.elSoundManager.sonidosMuerte, 0.8f,"Batalla");
        miNavMeshAgent.isStopped = true;
        idle = false;
        andando = false;
        atacando = false;
        cargandoCatapulta = false;
        defendiendo = false;
        muerto = true;
        EstablecerAnimaciones();
    }

    public void CargarProyectil(){
        if( muerto)
            return;
 
        if( this.gameObject.transform.childCount < 12)//El proyectil no está listo
            return;

        Debug.Log("CargarProyectil: Soy la catapulta del player: "+player+" estoy cargando.");
        
        idle = false;
        atacando = false;
        cargandoCatapulta = true;
        defendiendo = false;
        muerto = false;
        EstablecerAnimaciones();
    }
    public void DispararProyectil(float fuerzaDisparo){
        if( muerto)
            return;
 //       Debug.Log("Esta unidad tiene estos hijos: "+this.gameObject.transform.childCount);
        if( this.gameObject.transform.childCount < 13)//El proyectil no está listo
            return;
        //Para que la catapulta no se dispare a sí misma
        if( fuerzaDisparo < 20) 
            fuerzaDisparo = 20f;
        if(player == "Player2"){
            fuerzaDisparo = -fuerzaDisparo;
        }
        Debug.Log("DispararProyectil:INI. Soy la catapulta del player: "+player);
        Debug.Log("DispararProyectil: "+fuerzaDisparo);
        elThrowSimulation.target.transform.position += new Vector3(fuerzaDisparo,-7,0);
        elThrowSimulation.disparar = true;
        //Para que el proyectil sepa si ha sido disparado y pueda matar unidades al colisionar
        transform.GetChild(12).GetComponent<ProyectilCatapulta>().disparado = true;
        transform.GetChild(12).GetComponent<ProyectilCatapulta>().transform.SetParent(null);
        
        idle = false;
        atacando = true;
        cargandoCatapulta = false;
        defendiendo = false;
        muerto = false;
        Debug.Log("Catapultando! animación play!");
        EstablecerAnimaciones();
//        Debug.Log("Proyetil disparado. Ahora habría que llamar a una corrutina para que reinicie el target al origen de la catapulta y ésta no pueda disparar mientras no llega el proyectil.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if( other.tag == "Soldado" || other.tag == "Caballeria" || other.tag == "Catapulta"){
            if( other.gameObject.GetComponent<Unidad>().player != player){
//                Debug.Log("Soy: "+gameObject.name+" de "+player+" he colisionado con: "+other.gameObject.name+" que es del: "+other.gameObject.GetComponent<Unidad>().player);
                objetivo = other.gameObject;
            }
        }
        if( other.tag == "Limite")
            Morir();
    }

    //¿Esto sobraría?
    //Aunque salga, me quedo con el objetivo que ha entrado y cuando golpee, si la 
    //distancia es menor de ?? me lo cargo.
    private void OnTriggerExit(Collider other){
        /*
        if( other.tag == "Soldado" || other.tag == "Caballeria" || other.tag == "Catapulta"){
            if( other.gameObject.GetComponent<Unidad>().player != player){
                Debug.Log("Soy: "+gameObject.name+" de "+player+" ha salido de mi area de acción: "+other.gameObject.name+" que es del: "+other.gameObject.GetComponent<Unidad>().player);
                objetivo = null;
            }
        }
        */
    }

    public bool EstoyMuerto(){
        return muerto;
    }

    private void EstablecerAnimaciones(){//bool idle, bool andando, bool atacando, bool defendiendo, bool muerto, bool cargandoCata = false){
        miAnimator.SetBool("idle", idle);
        miAnimator.SetBool("andando", andando);
        miAnimator.SetBool("atacando", atacando);
        miAnimator.SetBool("defendiendo", defendiendo);
        miAnimator.SetBool("muerto", muerto);
        miAnimator.SetBool("cargando",cargandoCatapulta);
    }

    public float DistanciaToObjetivo(){
        if( objetivo == null )
            return 1000f;
//        Debug.Log("Alguien tiene enemigo a mano-->>"+Vector3.Distance(transform.position, objetivo.transform.position));
        return Vector3.Distance(transform.position, objetivo.transform.position);
    }

    public string GetEstado(){
        return "idle: "+ idle+" andando: "+ andando+" atacando: "+atacando+" defendiendo: "+defendiendo+" muerto: "+muerto;
    }

    public void ResaltarUnidad(){
        unitSelector.SetActive(true);
    }

    public void NoResaltarUnidad(){
        unitSelector.SetActive(false);
    }
}
