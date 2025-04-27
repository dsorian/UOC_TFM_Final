using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class Estado : ScriptableObject
{
    public int numEstado;
    public string nombreEstado;
    public int tileCapital;
    public List<Vector2Int> coordsTiles;   //Coordenadas x,y de las tiles del estado
    public List<int> tipoCeldas;            //Tipo de las celdas
    //Si un estado es vecino de éste o no 
    public bool[] estadosVecinos = {false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false};
    
    //¿Habrá que poner el listado de tiles para poder encontrarlas dada sus coordenadas x-y?
    //No, con x-y tenemos la tile en MapaReino.elGrid = y*gridsize.y+x nos da la tile que es
    //private List<HexTile> tilesEstado;

    public Estado()
    {
        numEstado = 0;
        nombreEstado = "Estado "+numEstado;
        tileCapital = -1;
        coordsTiles = new List<Vector2Int>();
        tipoCeldas = new List<int>();
    }
    public void SetEstado(int numEstado,string nombreEstado, int tileCapital, List<Vector2Int> coordsTiles,List<int> tipoCeldas){
        this.numEstado = numEstado;
        this.nombreEstado = nombreEstado;
        this.tileCapital = tileCapital;
        this.coordsTiles = coordsTiles;
        this.tipoCeldas = tipoCeldas;
    }

    public void AnyadirTile(HexTile tile){
//        Debug.Log("Añadiendo tile: "+tile.name+" al estado: "+numEstado);
        tile.numEstado = numEstado;
        coordsTiles.Add(new Vector2Int(tile.coordenada.x,tile.coordenada.y));
        tipoCeldas.Insert(coordsTiles.IndexOf(tile.coordenada),tile.tipoCelda);
    }

    public void QuitarTile(HexTile tile){
        //Debug.Log("Quitando tile: "+tile.name+" del estado: "+numEstado+" la coordenada del tipocelda es: "+coordsTiles.IndexOf(tile.coordenada));
        tipoCeldas.RemoveAt(coordsTiles.IndexOf(tile.coordenada));
        coordsTiles.RemoveAt(coordsTiles.IndexOf(tile.coordenada));
    }

    public void SetCapital(HexTile tile){
        tileCapital = coordsTiles.IndexOf(tile.coordenada);
    }

    public Vector2Int GetCoordsCapital(){
        //Debug.Log("GetCoordsCapital del estado: "+numEstado);
        if(tileCapital == -1)
            return new Vector2Int(0,0);
        else 
            return coordsTiles[tileCapital];
    }

    public void SetMaterialCelda(HexTile tile, int tipoCelda){
        //Debug.Log("SetMaterialCelda: celda: "+tile.name+" tipo: "+tile.tipoCelda+" le quiero poner: "+tipoCelda);
        tipoCeldas[coordsTiles.IndexOf(tile.coordenada)] = tipoCelda;
        tile.SetMaterial(tile.materiales[tipoCelda]);
    }

    public int GetMaterialCelda(HexTile tile){
        return tipoCeldas[coordsTiles.IndexOf(tile.coordenada)];
    }

    public void SetEstadoVecino(int vecino,bool valor){
        estadosVecinos[vecino] = valor;
    }

    public bool GetEstadoVecino(int vecino){
        return estadosVecinos[vecino];
    }

    public bool EsVecino(int estado){
        return estadosVecinos[estado];
    }
    
    public void MostrarEstado(){
        Debug.Log("Mostrando el estado: "+numEstado);
        Debug.Log("         >Nombre Estado: "+nombreEstado);
        Debug.Log("          >Capital: "+tileCapital);
        for(int i=0; i<coordsTiles.Count;i++){
            Debug.Log("         >Tile: "+i+" coordenadas: "+coordsTiles[i].x+"-"+ coordsTiles[i].y);
        }
        for(int i=0; i<tipoCeldas.Count;i++){
            Debug.Log("         >TipoCelda: "+i+" Tipo: "+tipoCeldas[i]);
        }
        Debug.Log("Estados vecinos: "+estadosVecinos);
    }
}
