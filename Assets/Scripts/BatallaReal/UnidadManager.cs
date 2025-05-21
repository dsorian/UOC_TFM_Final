//De antes de poner el cambio para controlar a los dos jugadores

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;



//PENDIENTE GORDO:

//También deberá llevar el control el UnidadManager si tiene más de 6 unidades y le destruyen 6
//que vuelva a salir una nueva con las siguientes 6 y así hasta destruirla entera.
//
//No está terminado el movimiento de la caballería. Hay que pulirlo y que haga el movimiento+ataque
//cuando las dos variables sean true.

//Se mueve y las unidades siguen el punto que se les ha asignado

public class UnidadManager : MonoBehaviour
{
   // public NavMeshAgent miNavMeshAgent;
    public BatallaManager elBatallaManager;
    public string player; //Guardar a qué player pertenece (Player1 o Player2)
    public GameObject modeloUnidad;
    public int tipoUnidad;
    public int numTotalUnidades=6;  //Total de unidades que tenemos
    public int numUnidadesCombatiendo=6; //Total de unidades en el campo de batalla (entre 0 y 6)
    private Material material1Unidades;
    private Material material2Unidades;
    private int numUnidadVanguardia=0;  //Unidad que va en la vanguardia (la 1 si es player 1 y la 4 si es player 2). La posición del medio en primera línea
    public GameObject[] unidades = null;
    public bool idle = true, andando = false, atacando = false, defendiendo = false, cargandoCatapulta = false, muerto = false;

    public bool cooldownAtaqueActivo;
    public float cooldownAtaque = 1.0f;
    public float cooldownAtaqueActual = 1.0f;
    public float rangoAtaque = 5.0f;

    private bool seleccionada = false;
     //Movimiento
    private float horizontalInputP1,horizontalInputP2;
    private float verticalInputP1,verticalInputP2;
    public float walkSpeed;
    public float fuerzaCatapulta = 0;
    private bool oponenteCPU;

    // Start is called before the first frame update
    void Start()
    {
        if(tipoUnidad == 2){
            walkSpeed +=2.5f;
            cooldownAtaque = 4.0f; 
        }
        if(PlayerPrefs.GetInt("numPlayers") == 1)
            oponenteCPU = true;
        else
            oponenteCPU = false;   
    }

    // Update is called once per frame
    void Update(){
        if(elBatallaManager.tutorialActivo)
            return;
            
        if(muerto)
            return;
        if(!seleccionada){
            PararUnidades();
            return;
        }

        if(cooldownAtaqueActivo){
            Debug.Log("5.- cooldownAtaqueActivo: "+cooldownAtaqueActivo+" cooldownAtaqueActual: "+cooldownAtaqueActual);
            cooldownAtaqueActual -= Time.deltaTime;
            Debug.Log("kk cooldownAtaqueActual: "+cooldownAtaqueActual);
            if(cooldownAtaqueActual < 0.1f){
                Debug.Log("Fin del cooldown del ataque: ");
                cooldownAtaqueActivo = false;
                cooldownAtaqueActual = cooldownAtaque;
            }
        }

        if( !atacando && cargandoCatapulta && tipoUnidad == 0){
            //Hay que seguir cargando la barra de disparo
            if( fuerzaCatapulta < 100)
                fuerzaCatapulta += 30f * Time.deltaTime;
            CargarCatapulta();
        }
        /*Esto hacía que la catapulta de player2 no moviera pero ahora no se para :_(
        if(!cargandoCatapulta && !atacando && tipoUnidad == 0){ //Resetear la barra de disparo
                FinalizarAtaque();
        }
        */

        /*
        ******    **
        *    *   ***
        ******  * **
        *         **
        *         **
        */
        if(player == "Player1"){

            // ATAQUE Player1
                if (Input.GetButtonDown("AttackP1")) {
                    IniciarAtaque();
                }   
                if (Input.GetButtonUp("AttackP1") && tipoUnidad == 0) {
                    Debug.Log("Soltado botón de disparar");
                    DispararCatapulta();
                    cargandoCatapulta = false;
                }

                // DEFENSA Player1
                if (Input.GetButtonDown("DefendP1")) {
                    RealizarDefensa();
                    if (tipoUnidad == 1) {
                        elBatallaManager.elSoundManager.PlayRandomSound(elBatallaManager.elSoundManager.sonidosGritoGolpe, 0.25f, "Batalla");
                    } else if (tipoUnidad == 2) {
                        elBatallaManager.elSoundManager.PlaySound(elBatallaManager.elSoundManager.sonidosCaballos, UnityEngine.Random.Range(0, 3), 0.25f, "Batalla");
                    }
                }
                if (Input.GetButtonUp("DefendP1")) {
                    FinalizarDefensa();
                }

                // MOVIMIENTO Player1
                horizontalInputP1 = Input.GetAxis("HorizontalP1");
                verticalInputP1 = Input.GetAxis("VerticalP1");

                bool isIdle = horizontalInputP1 == 0 && verticalInputP1 == 0;
                bool isMoving = horizontalInputP1 != 0 || verticalInputP1 != 0;

                ManejarSonidoSeleccionado(isMoving);
                ManejarMovimientoTipoUnidad();
        }
        
        if(oponenteCPU)
            return;
        else{
        /*
        ******   *****
        *    *      **
        ******   ***** 
        *        ** 
        *        ***** 
        */
            if(player == "Player2"){

                // ATAQUE Player2
                if (Input.GetButtonDown("AttackP2")) {
                    IniciarAtaque();
                }   
                if (Input.GetButtonUp("AttackP2") && tipoUnidad == 0) {
                    Debug.Log("Soltado botón de disparar");
                    DispararCatapulta();
                    cargandoCatapulta = false;
                }

                // DEFENSA Player2
                if (Input.GetButtonDown("DefendP2")) {
                    RealizarDefensa();
                    if (tipoUnidad == 1) {
                        elBatallaManager.elSoundManager.PlayRandomSound(elBatallaManager.elSoundManager.sonidosGritoGolpe, 0.5f, "Batalla");
                    } else if (tipoUnidad == 2) {
                        elBatallaManager.elSoundManager.PlaySound(elBatallaManager.elSoundManager.sonidosCaballos, UnityEngine.Random.Range(0, 3), 0.5f, "Batalla");
                    }
                }
                if (Input.GetButtonUp("DefendP2")) {
                    FinalizarDefensa();
                }

                // MOVIMIENTO Player2
                horizontalInputP2 = Input.GetAxis("HorizontalP2");
                verticalInputP2 = Input.GetAxis("VerticalP2");

                bool isIdle = horizontalInputP2 == 0 && verticalInputP2 == 0;
                bool isMoving = horizontalInputP2 != 0 || verticalInputP2 != 0;

                ManejarSonidoSeleccionado(isMoving);
                ManejarMovimientoTipoUnidad();
            }
        }
    }

    private void ManejarSonidoSeleccionado(bool isMoving) {
        if(player == "Player1"){
            if (isMoving) {
                if (!elBatallaManager.elSoundManager.UnidadSeleccionadaP1Source.isPlaying) {
                    elBatallaManager.elSoundManager.PlayMusic(elBatallaManager.elSoundManager.musicaUnidadSeleccionada[tipoUnidad], true, 0.5f, "UnidadSeleccionadaP1Source");
                }
            } else {
                if (elBatallaManager.elSoundManager.UnidadSeleccionadaP1Source.isPlaying) {
                    elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP1Source");
                }
            }
        }else{
            if (isMoving) {
                if (!elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying) {
                    elBatallaManager.elSoundManager.PlayMusic(elBatallaManager.elSoundManager.musicaUnidadSeleccionada[tipoUnidad], true, 0.5f, "UnidadSeleccionadaP2Source");
                }
            } else {
                if (elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying) {
                    elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
                }
            }
        }
    }


    private void ManejarMovimientoTipoUnidad() {
        if(player == "Player1"){
            switch (tipoUnidad) {
                case 0: // Catapulta
                case 1: // Infantería
                    if (horizontalInputP1 == 0 && verticalInputP1 == 0 && !atacando && !defendiendo && !cargandoCatapulta) {
                        andando = false;
                        idle = true;
                        PararUnidades();
                    } else if (!cargandoCatapulta) { // Evitar movimiento si está cargando
                        andando = true;
                        idle = false;
                        MoverCatapultaOInfanteria(); // Método dedicado para mover estas unidades
                    }
                    break;
                case 2: // Caballería
                    if (horizontalInputP1 == 0 && verticalInputP1 == 0 && !atacando && !defendiendo) {
                        andando = false;
                        idle = true;
                        PararUnidades();
                    } else {
                        andando = true;
                        idle = false;
                        MoverCaballeria();
                    }
                    break;
                default:
                    Debug.LogError("UnidadManager: Error. tipo de unidad no contemplado.");
                    break;
            }
        }else{
            switch (tipoUnidad) {
            case 0: // Catapulta
            case 1: // Infantería
                if (horizontalInputP2 == 0 && verticalInputP2 == 0 && !atacando && !defendiendo && !cargandoCatapulta) {
                    andando = false;
                    idle = true;
                    PararUnidades();
                } else if (!cargandoCatapulta) { // Evitar movimiento si está cargando
                    andando = true;
                    idle = false;
                    MoverCatapultaOInfanteria(); // Método dedicado para mover estas unidades
                }
                break;
            case 2: // Caballería
                if (horizontalInputP2 == 0 && verticalInputP2 == 0 && !atacando && !defendiendo) {
                    andando = false;
                    idle = true;
                    PararUnidades();
                } else {
                    andando = true;
                    idle = false;
                    MoverCaballeria();
                }
                break;
            default:
                Debug.LogError("UnidadManager: Error. tipo de unidad no contemplado.");
                break;
        }
        }
    }


    private void MoverCatapultaOInfanteria() {
        if(player == "Player1"){
            // Mueve la unidad en función de las entradas del jugador
            if (tipoUnidad == 0) { // Catapulta: solo se mueve en el eje Z (adelante y atrás)
                transform.Translate(Vector3.forward * Time.deltaTime * verticalInputP1 * walkSpeed, Space.World);
            } else { // Infantería: se mueve en ambos ejes (X e Z)
                transform.Translate(Vector3.forward * Time.deltaTime * verticalInputP1 * walkSpeed, Space.World);
                transform.Translate(Vector3.right * Time.deltaTime * horizontalInputP1 * walkSpeed, Space.World);
            }

            // Si la unidad está defendiendo, realiza la defensa
            if (defendiendo) {
                RealizarDefensa();
            }

            // Si la unidad está andando pero no atacando, mueve las unidades asociadas
            if (andando && !atacando) {
                MoverUnidades();
            }
        }else{
            // Mueve la unidad en función de las entradas del jugador
            if (tipoUnidad == 0) { // Catapulta: solo se mueve en el eje Z (adelante y atrás)
                transform.Translate(Vector3.forward * Time.deltaTime * verticalInputP2 * walkSpeed, Space.World);
            } else { // Infantería: se mueve en ambos ejes (X e Z)
                transform.Translate(Vector3.forward * Time.deltaTime * verticalInputP2 * walkSpeed, Space.World);
                transform.Translate(Vector3.right * Time.deltaTime * horizontalInputP2 * walkSpeed, Space.World);
            }

            // Si la unidad está defendiendo, realiza la defensa
            if (defendiendo) {
                RealizarDefensa();
            }

            // Si la unidad está andando pero no atacando, mueve las unidades asociadas
            if (andando && !atacando) {
                MoverUnidades();
            }
        }
    }

private void MoverCaballeria() {

    // Si la caballería está defendiendo, se detiene y realiza la defensa
    if (defendiendo) {
        andando = false;
        idle = false;
        PararUnidades(); // Detiene el movimiento
        RealizarDefensa(); // Ejecuta la lógica de defensa
        return; // Salimos del método para evitar que continúe moviéndose
    }

    if(player == "Player1"){
        // Movimiento de la caballería
        transform.Translate(Vector3.forward * Time.deltaTime * verticalInputP1 * walkSpeed, Space.World);
        transform.Translate(Vector3.right * Time.deltaTime * horizontalInputP1 * walkSpeed, Space.World);
    }else{
        // Movimiento de la caballería
        transform.Translate(Vector3.forward * Time.deltaTime * verticalInputP2 * walkSpeed, Space.World);
        transform.Translate(Vector3.right * Time.deltaTime * horizontalInputP2 * walkSpeed, Space.World);
    }
    // Si la caballería está andando pero no atacando, mueve las unidades asociadas
    if (andando && !atacando) {
        MoverUnidades();
    }
}




    //Antiguo CrearUnidades cuando sólo tenía un material que aplicar. Al modelar tengo por un lado el caballo y por otro el 
    //jinete y son dos materiales y lo mismo para la catapulta. Lo conservo por si los fusionara en el futuro
    public void CrearUnidadesOLD(GameObject laUnidad,int num,Material elMaterial,int numPlayer,bool seleccionada, int tipoUnidad){
        numTotalUnidades = num;
        modeloUnidad = laUnidad;
        player = "Player"+numPlayer;
        material1Unidades = elMaterial;
        unidades = new GameObject[6];

        if( numTotalUnidades>=6)
            numUnidadesCombatiendo = 6;
        else
            numUnidadesCombatiendo = numTotalUnidades;
        for( int i=0; i<numUnidadesCombatiendo;i++){
            //Como la catapulta tiene otra orientación le pongo una rotación diferente
            int anguloRotacion = 0;
            if( tipoUnidad == 0)
                anguloRotacion = 180;
            else
                anguloRotacion = 90;
            unidades[i] = Instantiate(modeloUnidad,new Vector3(transform.GetChild(i).position.x, transform.GetChild(i).position.y, transform.GetChild(i).position.z), Quaternion.Euler(0,anguloRotacion,0));
            unidades[i].GetComponentInChildren<SkinnedMeshRenderer>().material = material1Unidades;
            unidades[i].GetComponent<Unidad>().setMyDestination(transform.GetChild(i).transform);
            unidades[i].GetComponent<Unidad>().player = "Player"+numPlayer;
            unidades[i].GetComponent<Unidad>().miUnidadManager = this;
            unidades[i].GetComponent<Unidad>().numeroUnidad = i;
            this.seleccionada = seleccionada;
            this.tipoUnidad = tipoUnidad;
            unidades[i].GetComponentInChildren<GestionarAnimaciones>().miUnidadManager = this;
            unidades[i].GetComponentInChildren<GestionarAnimaciones>().miNumUnidad = i;

            if(numPlayer == 2)  //Si es del Player 2 la rotamos para que mire a la izquierda
                unidades[i].transform.Rotate(0,180,0);
            unidades[i].GetComponent<Unidad>().Parar();
        }
        if(numUnidadesCombatiendo > 0)
            ReorganizarUnidades();
    }

    public void CrearUnidades(GameObject laUnidad, int num, Material elMaterial1, Material elMaterial2, int numPlayer, bool seleccionada, int tipoUnidad)
    {
        numTotalUnidades = num;
        modeloUnidad = laUnidad;
        player = "Player" + numPlayer;
        material1Unidades = elMaterial1;
        material2Unidades = elMaterial2;
        unidades = new GameObject[6];

        if (numTotalUnidades >= 6)
            numUnidadesCombatiendo = 6;
        else
            numUnidadesCombatiendo = numTotalUnidades;
        for (int i = 0; i < numUnidadesCombatiendo; i++)
        {
            //Como la catapulta tiene otra orientación le pongo una rotación diferente
            int anguloRotacion = 0;
            if (tipoUnidad == 0)
                anguloRotacion = 180;
            else
                anguloRotacion = 90;

            unidades[i] = Instantiate(modeloUnidad, new Vector3(transform.GetChild(i).position.x, transform.GetChild(i).position.y, transform.GetChild(i).position.z), Quaternion.Euler(0, anguloRotacion, 0));

            unidades[i].GetComponentInChildren<SkinnedMeshRenderer>().material = material1Unidades;
            //Hasta que se solucione: Los soldados y la catapulta tienen distinto material
            if (tipoUnidad == 0)
            {
                unidades[i].transform.Find("Catapulta").GetComponent<SkinnedMeshRenderer>().material = material2Unidades;
            }
            if (tipoUnidad == 2)
            {
                unidades[i].transform.Find("Soldado").GetComponent<SkinnedMeshRenderer>().material = material1Unidades;
                unidades[i].transform.Find("Caballo").GetComponent<SkinnedMeshRenderer>().material = material2Unidades;
            }

            unidades[i].GetComponent<Unidad>().setMyDestination(transform.GetChild(i).transform);
            unidades[i].GetComponent<Unidad>().player = "Player" + numPlayer;
            unidades[i].GetComponent<Unidad>().miUnidadManager = this;
            unidades[i].GetComponent<Unidad>().numeroUnidad = i;
            this.seleccionada = seleccionada;
            this.tipoUnidad = tipoUnidad;
            unidades[i].GetComponentInChildren<GestionarAnimaciones>().miUnidadManager = this;
            unidades[i].GetComponentInChildren<GestionarAnimaciones>().miNumUnidad = i;

            if (numPlayer == 2)  //Si es del Player 2 la rotamos para que mire a la izquierda
                unidades[i].transform.Rotate(0, 180, 0);
            unidades[i].GetComponent<Unidad>().Parar();
        }
        if (numUnidadesCombatiendo > 0)
            ReorganizarUnidades();
        if (numUnidadesCombatiendo == 1)
        {
            if (player == "Player1")
                unidades[0].transform.position = transform.GetChild(1).transform.position;
            else
                unidades[0].transform.position = transform.GetChild(4).transform.position;            
        }
    }
    public void AddRemainingUnits(){
        //Creamos las unidades que quedan
            
        Debug.Log("Añado las unidades que quedan pendientes:");

        if( numTotalUnidades>=6)
            numUnidadesCombatiendo = 6;
        else
            numUnidadesCombatiendo = numTotalUnidades;
                    int anguloRotacion = 0;
        //Como la catapulta tiene otra orientación le pongo una rotación diferente
        if( tipoUnidad == 0)
            anguloRotacion = 180;
        else
            anguloRotacion = 90;
        for( int i=0; i<numUnidadesCombatiendo;i++){
            unidades[i] = Instantiate(modeloUnidad,new Vector3(transform.GetChild(i).position.x, transform.GetChild(i).position.y, transform.GetChild(i).position.z), Quaternion.Euler(0,anguloRotacion,0));
//if(tipoUnidad == 2 )
//    Debug.Log("Poniendo skin al caballero de "+player + " - " + material1Unidades.name + " - " + material2Unidades);
            unidades[i].GetComponentInChildren<SkinnedMeshRenderer>().material = material1Unidades;
            //Hasta que se solucione: Los soldados y la catapulta tienen distinto material
            if(tipoUnidad == 0){
                unidades[i].transform.Find("Catapulta").GetComponent<SkinnedMeshRenderer>().material = material2Unidades;
            }
            if(tipoUnidad == 2){
Debug.Log("Poniendo skin al caballero de "+player + " - " + material1Unidades.name + " - " + material2Unidades);
                unidades[i].transform.Find("Soldado").GetComponent<SkinnedMeshRenderer>().material = material1Unidades;
                unidades[i].transform.Find("Caballo").GetComponent<SkinnedMeshRenderer>().material = material2Unidades;
            }

            unidades[i].GetComponent<Unidad>().setMyDestination(transform.GetChild(i).transform);
            unidades[i].GetComponent<Unidad>().player = player;
            unidades[i].GetComponent<Unidad>().miUnidadManager = this;
            unidades[i].GetComponent<Unidad>().numeroUnidad = i;
            this.seleccionada = false;
            unidades[i].GetComponentInChildren<GestionarAnimaciones>().miUnidadManager = this;
            unidades[i].GetComponentInChildren<GestionarAnimaciones>().miNumUnidad = i;

            if(player == "Player2")  //Si es del Player 2 la rotamos para que mire a la izquierda
                unidades[i].transform.Rotate(0,180,0);
            unidades[i].GetComponent<Unidad>().Parar();
        }
        if(numUnidadesCombatiendo > 0)
            ReorganizarUnidades();
    }

    public void EliminarUnidad(int laUnidad){
        Debug.Log("UnidadManager.EliminarUnidad()");
        numTotalUnidades--;
        numUnidadesCombatiendo--;
        Destroy(unidades[laUnidad]);
        unidades[laUnidad] = null;
        if(numUnidadesCombatiendo == 0){
            Debug.Log("OMG! era la última unidad. La restituyo a su posición original y si quedan unidades las creo.");
            if( tipoUnidad == 0 ){
                if (player == "Player1"){
                    transform.position = new Vector3(elBatallaManager.posIniCatapultaP1.x,78,elBatallaManager.posIniCatapultaP1.y);
                }else{
                    transform.position = new Vector3(elBatallaManager.posIniCatapultaP2.x,78,elBatallaManager.posIniCatapultaP2.y);
                }
            }else if( tipoUnidad == 1 ){
                if (player == "Player1"){
                    transform.position = new Vector3(elBatallaManager.posIniInfanteriaP1.x,78,elBatallaManager.posIniInfanteriaP1.y);
                }else{
                    transform.position = new Vector3(elBatallaManager.posIniInfanteriaP2.x,78,elBatallaManager.posIniInfanteriaP2.y);
                }
            }else if( tipoUnidad == 2 ){
                if (player == "Player1"){
                    transform.position = new Vector3(elBatallaManager.posIniCaballeriaP1.x,78,elBatallaManager.posIniCaballeriaP1.y);
                }else{
                    transform.position = new Vector3(elBatallaManager.posIniCaballeriaP2.x,78,elBatallaManager.posIniCaballeriaP2.y);
                }
            }
            
            if( numTotalUnidades <=0 ){
                Debug.Log("Todas las unidades destruidas.");
                muerto = true;
            }
            else{
                Debug.Log("Las 6 unidades destruidas pero quedan más. Las creo.");
                AddRemainingUnits();
            }

            if( elBatallaManager.EsFinBatalla()){
                return;
            }
            if(seleccionada){
                if(player == "Player1")
                    elBatallaManager.SiguienteUnidadManagerP1();
                else
                    elBatallaManager.SiguienteUnidadManagerP2();
            }
        }
        ReorganizarUnidades();
    }

    public void EliminarTodasUnidades(){
        foreach(GameObject laUnidad in unidades){
            Destroy(laUnidad);
        }
        numTotalUnidades = 0;
        numUnidadesCombatiendo = 0;
    }

    public void MoverUnidades(){
        for(int i=0; i<unidades.Length;i++){
            if( unidades[i] != null &&  ! unidades[i].GetComponent<Unidad>().EstoyMuerto())
                unidades[i].GetComponent<Unidad>().Andar();
        }
        idle = false;
        andando =true;
    }

    public void PararUnidades(){
        Debug.Log("UnidadManager: PararUnidades() de: "+player);
        for(int i=0; i<unidades.Length;i++){
            if(unidades[i] != null )// &&  ! unidades[i].GetComponent<Unidad>().miNavMeshAgent.hasPath)
                unidades[i].GetComponent<Unidad>().Parar();
        }
        idle = true;
        andando = false;
    }

    public void IniciarAtaque(){
        Debug.Log("UnidadManager de: "+player+" Iniciar Ataque.");
        if(cooldownAtaqueActivo)
            return;
        //Sonidos
        switch ((tipoUnidad))
        {
            case 0:
                elBatallaManager.elSoundManager.PlaySound(elBatallaManager.elSoundManager.sonidosCatapulta,0, 0.5f,"Batalla");
                break;
            case 1:
                elBatallaManager.elSoundManager.PlaySound(elBatallaManager.elSoundManager.sonidosEspada,UnityEngine.Random.Range(0,elBatallaManager.elSoundManager.sonidosEspada.Length), 0.5f,"Batalla");
                elBatallaManager.elSoundManager.PlaySound(elBatallaManager.elSoundManager.sonidosGritoGolpe,UnityEngine.Random.Range(0,elBatallaManager.elSoundManager.sonidosGritoGolpe.Length), 0.5f,"Batalla");
                break;
            case 2:
                Debug.Log("Reproduciendo sonidos caballería.");
                elBatallaManager.elSoundManager.PlaySound(elBatallaManager.elSoundManager.sonidosEspada,UnityEngine.Random.Range(0,elBatallaManager.elSoundManager.sonidosEspada.Length), 0.5f,"Batalla");
                elBatallaManager.elSoundManager.PlaySound(elBatallaManager.elSoundManager.sonidosGritoGolpe,UnityEngine.Random.Range(0,elBatallaManager.elSoundManager.sonidosGritoGolpe.Length), 0.5f,"Batalla");
                if( UnityEngine.Random.Range(0,100) <= 5) //El relincho sólo un 5% de las veces
                    elBatallaManager.elSoundManager.PlaySound(elBatallaManager.elSoundManager.sonidosCaballos,UnityEngine.Random.Range(0,3), 0.5f,"Batalla");
                break;
            default:
                Debug.LogError("UnidadManager: Error. tipo de unidad no contemplado en IniciarAtaque()");
                break;
        }
        for(int i=0; i<unidades.Length;i++){
            if(unidades[i] != null){
                switch(tipoUnidad){
                case 0:
                    //No hacemos nada, pasamos a modo ataque y empezará a cargar el disparo.
                    idle = false;
                    atacando = false;
                    andando = false;
                    cargandoCatapulta = true;
                    fuerzaCatapulta = 10f * Time.deltaTime;
                    CargarCatapulta();
                    break;
                case 1:
                    unidades[i].GetComponent<Unidad>().IniAtacar();
                    andando = false;
                    atacando = true;
                    defendiendo = false;
                    break;
                case 2: 
                    unidades[i].GetComponent<Unidad>().IniAtacarCaballeria();
                    andando = true;
                    atacando = true;
                    defendiendo = false;
                    break;
                default:
                    Debug.LogError("UnidadManager: Caso no contemplado de IniciarAtaque()");
                    break;
                }
            }
        }
        idle = false;
        defendiendo = false;
        
        cooldownAtaqueActivo = true;
        cooldownAtaqueActual = cooldownAtaque;
    }
    public void FinalizarAtaque(){
        Debug.Log("UnidadManager de: "+player+": FinalizarAtaque.");
        for(int i=0; i<unidades.Length;i++){
            if(unidades[i] != null){
                switch(tipoUnidad){
                case 0:
                    unidades[i].GetComponent<Unidad>().FinAtacar();
                    unidades[i].GetComponentInChildren<ShootBar>().ResetForce();
                    fuerzaCatapulta = 0;
                    break;
                case 1:
                    unidades[i].GetComponent<Unidad>().FinAtacar();
                    break;
                case 2: 
                    unidades[i].GetComponent<Unidad>().FinAtacarCaballeria();
                    break;
                default:
                    Debug.LogError("UnidadManager: Caso no contemplado de FinalizarAtaque()");
                    break;
                }
            }
        }
        atacando = false;
        //Debug.Log("4.- kk finalizando ataque: Pongo cooldownAtaqueActivo a: "+cooldownAtaqueActivo+" e inicio el cooldown: "+cooldownAtaqueActual);
    }

    public void RealizarDefensa(){
        for(int i=0; i<unidades.Length;i++){
            if(unidades[i] != null)
                unidades[i].GetComponent<Unidad>().Defender();
        }
        idle = false;
        andando = false;
        atacando = false;
        defendiendo = true;
    }

    public void FinalizarDefensa(){
        for(int i=0; i<unidades.Length;i++){
            if(unidades[i] != null)
                unidades[i].GetComponent<Unidad>().Parar();
        }
        defendiendo = false;
    }

    public void CargarCatapulta(){
        for(int i=0; i<unidades.Length;i++){
            if(unidades[i] != null){
                unidades[i].GetComponentInChildren<ShootBar>().SetForce(fuerzaCatapulta);
                unidades[i].GetComponent<Unidad>().CargarProyectil();
            }
        }
        idle = false;
        andando = false;
        atacando = false;
        cargandoCatapulta = true;
        defendiendo = false;
    }

    public void DispararCatapulta(){
        elBatallaManager.elSoundManager.PlaySound(elBatallaManager.elSoundManager.sonidosCatapulta,1, 0.5f,"Batalla");
        for(int i=0; i<unidades.Length;i++){
            if( unidades[i] != null ){
                unidades[i].GetComponentInChildren<ShootBar>().SetForce(0);
                unidades[i].GetComponent<Unidad>().DispararProyectil(fuerzaCatapulta);
            }
        }
        fuerzaCatapulta = 0;
        
        idle = false;
        andando = false;
        atacando = true;
        cargandoCatapulta = false;
        defendiendo = false;
    }

    public bool TodosIdle(){
        bool todosIdle = true;
        for(int i=0; i<unidades.Length;i++){
            if( unidades[i] != null && ! unidades[i].GetComponent<Unidad>().idle ){
                todosIdle = false;
            }
        }
        return todosIdle;
    }

    public bool AlgunaUnidadTieneObjetivoCerca(){
        bool tieneObjetivoCerca = false;
        for( int i=0; i<unidades.Length; i++){
            if( unidades[i] != null && unidades[i].GetComponent<Unidad>().DistanciaToObjetivo() <= rangoAtaque){
                tieneObjetivoCerca = true;
            }
        }
        return tieneObjetivoCerca;
    }

    public string GetInfoUnidades(){
        string laInfo = "";
        for( int i = 0 ; i < unidades.Length; i++){
            if( unidades[i] != null){
                laInfo += "Unidad "+i+":"+ unidades[i].GetComponent<Unidad>().GetEstado()+"\n";
            }
        }
        return laInfo;
    }

    public bool IAPuedeAtacar(){
        bool puedeAtacarAlguien = false;
        foreach(GameObject laUnidad in unidades){
            if( laUnidad != null && laUnidad.GetComponent<Unidad>().objetivo !=null && laUnidad.GetComponent<Unidad>().DistanciaToObjetivo()< rangoAtaque){
                puedeAtacarAlguien = true;
            }
        }
        Debug.Log("UnidadManager.cs: IAPuedeAtacar() de: "+player+" puede atacar?-> "+puedeAtacarAlguien);
        return puedeAtacarAlguien;
    }

    //Para que cuando la UnidadManager pierde unidades se coloque una tropa en la posición
    //PosUnidad5 (4) o PosUnidad2 (1) para que siempre haya una tropa en el medio de la formación
    private void ReorganizarUnidades(){
        bool tengoVanguardia = false;
        int numUltimo = 0;
        int numVanguardia = 1;
        if( player == "Player2")
            numVanguardia = 4;
        
        for(int i=0 ; i<unidades.Length ; i++){
            if( unidades[i] != null ){
                numUltimo = i;
                if(unidades[i].GetComponent<Unidad>().targetFollowed == transform.GetChild(numVanguardia).transform){
                    Debug.Log("Sí tengo Vanguardia. Player: "+player);
                    tengoVanguardia = true;
                }else
                    Debug.Log("No tengo vanguardia. Player: "+player);
            }
        }
        //Como me he quedado sin unidad en la vanguardia, le pongo ese destino a la última unidad que he visitado
        if( ! tengoVanguardia){
            //            Debug.Log("No tengo Vanguardia. Muevo la unidad: "+numUltimo+" a: "+numVanguardia);
            if (unidades[numUltimo] != null){
                unidades[numUltimo].GetComponent<Unidad>().setMyDestination(transform.GetChild(numVanguardia).transform);
            }
            numUnidadVanguardia = numUltimo;
            
        }
    }

    //Devuelve la unidad que va en vanguardia (para comparar distancias con el objetivo de la IA)
    public GameObject GetVanguardia(){
        return unidades[numUnidadVanguardia];
    }

    public void SeleccionarUnidad(bool seleccionada){
//        Debug.Log("RESALTANDO UNIDADMANAGER. seleccionada: "+seleccionada+". "+player);
        this.seleccionada = seleccionada;

        if( seleccionada ){
            if( ! oponenteCPU ){
//                Debug.Log("Resaltando: en el if the SeleccionarUnidad() "+player);
                StartCoroutine(ReproducirResaltado());
            }else if( player == "Player1" ){
//                Debug.Log("Resaltando: en el if the SeleccionarUnidad() "+player);
                StartCoroutine(ReproducirResaltado());
            }
        }
    }

    IEnumerator ReproducirResaltado(){
//        Debug.Log("Resaltando unidad!!!!");
        for(int i=0 ; i<unidades.Length ; i++){
            if(unidades[i] != null && seleccionada)
                unidades[i].GetComponent<Unidad>().ResaltarUnidad();
        }
        yield return new WaitForSeconds(0.25f);
        for(int i=0 ; i<unidades.Length ; i++){
            if(unidades[i] != null)
                unidades[i].GetComponent<Unidad>().NoResaltarUnidad();
        }
        yield return new WaitForSeconds(0.25f);
        for(int i=0 ; i<unidades.Length ; i++){
            if(unidades[i] != null && seleccionada)
                unidades[i].GetComponent<Unidad>().ResaltarUnidad();
        }
        yield return new WaitForSeconds(0.25f);
        for(int i=0 ; i<unidades.Length ; i++){
            if(unidades[i] != null)
                unidades[i].GetComponent<Unidad>().NoResaltarUnidad();
        }
        Debug.Log("Fin de resaltando unidad");
    }
}




/*
Backup del update para el input del player 2
            if(player == "Player2"){

                //ATAQUE Player2
                if(Input.GetButtonDown("AttackP2")){
                    IniciarAtaque();
                }
                if(Input.GetButtonUp("AttackP2") && tipoUnidad == 0){
                    Debug.Log("Soltado botón de disparar");
                    DispararCatapulta();
                    cargandoCatapulta = false;
                    //FinalizarAtaque();
                }

                //DEFENSA Player2
                if(Input.GetButtonDown("DefendP2")){
                    RealizarDefensa();
                    if(tipoUnidad == 1)
                        elBatallaManager.elSoundManager.PlayRandomSound(elBatallaManager.elSoundManager.sonidosGritoGolpe,0.5f,"Batalla");
                    if(tipoUnidad == 2)
                        elBatallaManager.elSoundManager.PlaySound(elBatallaManager.elSoundManager.sonidosCaballos,UnityEngine.Random.Range(0, 3),0.5f,"Batalla");
                }
                if(Input.GetButtonUp("DefendP2")){
                    FinalizarDefensa();
                }

                //MOVIMIENTO Player2
                //Leemos la entrada del teclado para P2
                horizontalInputP2 = Input.GetAxis("HorizontalP2");
                verticalInputP2 = Input.GetAxis("VerticalP2");
                if ( horizontalInputP2 == 0 && verticalInputP2 == 0){
                    if( elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying)
                        elBatallaManager.elSoundManager.StopMusic("UnidadSeleccionadaP2Source");
                }else{
                    if( ! elBatallaManager.elSoundManager.UnidadSeleccionadaP2Source.isPlaying){
                        elBatallaManager.elSoundManager.PlayMusic(elBatallaManager.elSoundManager.musicaUnidadSeleccionada[tipoUnidad],true,0.5f,"UnidadSeleccionadaP2Source");
                    }
                }
                switch(tipoUnidad){
                case 0: //Catapulta e infantería no pueden mover y atacar al mismo tiempo 
                case 1:
                    if( horizontalInputP2==0 && verticalInputP2==0 && !atacando && !defendiendo && !cargandoCatapulta){
                        andando = false;
                        idle = true;
                        PararUnidades();
                    }else{
                        
                        andando = true;
                        idle = false;
                    }
                    break;
                case 2: //Caballería sí puede atacar y mover
                    //Ver qué hago aquí
                    if( horizontalInputP2==0 && verticalInputP2==0 && !atacando && !defendiendo){
                        andando = false;
                        idle = true;
                        PararUnidades();
                    }else{
                        andando = true;
                        idle = false;
                    }
                    if( horizontalInputP2 != 0 || verticalInputP2 !=0){
                        Debug.Log("prueba: movimiento: "+horizontalInputP2+"-"+verticalInputP2);
                        //Poner el movimiento con una sola línea:
                        //transform.Translate(new Vector3(horizontalInputP2, verticalInputP2, 0) * walkSpeed * Time.deltaTime,Space.Self);
                        transform.Translate(Vector3.forward * Time.deltaTime * verticalInputP2 * walkSpeed, Space.World);
                        transform.Translate(Vector3.right * Time.deltaTime * horizontalInputP2 * walkSpeed, Space.World);
    //                    if( andando && atacando)
                            //Debug.Log("Llamar a MoverAtacar() cuando la haya creado?? NO, hay que hacerlo como el andar");
    //                    else{
                            
                            //if(  atacando )
                                //IniciarAtaque();
                            if(defendiendo){
                                RealizarDefensa();
                                elBatallaManager.elSoundManager.PlayRandomSound(elBatallaManager.elSoundManager.sonidosCaballos,0.5f,"Batalla");
                            }
                            if(andando && !atacando){
                                MoverUnidades();
                            }
    //                    }
                    }
                    break;
                default:
                    Debug.LogError("UnidadManager: Error. tipo de unidad no contemplado.");
                    break;
                }

                if( !atacando && !cargandoCatapulta && !defendiendo && tipoUnidad != 2 ){
                    //Leemos la entrada del teclado para P2
                    //horizontalInputP2 = Input.GetAxis("HorizontalP2");
                    //verticalInputP2 = Input.GetAxis("VerticalP2");

                    if( horizontalInputP2 != 0 || verticalInputP2 !=0){
                    //Poner el movimiento con una sola línea:
                    //transform.Translate(new Vector3(horizontalInput, verticalInput, 0) * moveSpeed * Time.deltaTime);
                        transform.Translate(Vector3.forward * Time.deltaTime * verticalInputP2 * walkSpeed);
                        if( tipoUnidad != 0 )
                            transform.Translate(Vector3.right * Time.deltaTime * horizontalInputP2 * walkSpeed);
                        MoverUnidades();
                    }
                }
            }

*/