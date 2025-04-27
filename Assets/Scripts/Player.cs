using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector3Int cubeCoordinate;
    public HexTile currentTile;
    public LineRenderer _renderer;

    protected List<HexTile> currentPath;
    public HexTile nextTile;
    protected bool gotPath;
    protected Vector3 targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLineRenderer(List<HexTile> tiles){
        
        if (_renderer == null) { return; }

        List<Vector3> points = new List<Vector3>();
        foreach ( HexTile tile in tiles ){
            points.Add(tile.transform.position + new Vector3(0, 0.5f, 0));
        }
        _renderer.positionCount = points.Count;
        _renderer.SetPositions(points.ToArray());
    }

    public void SetPath(List<HexTile> elPath){
        currentPath = elPath;
        gotPath = true;
    }

    public void HandleMovement(){
        if ( currentPath == null || currentPath.Count <= 1 ){
            nextTile = null;

            if ( currentPath != null && currentPath.Count > 0 ){
                currentTile = currentPath[0];
                nextTile = currentTile;
            }

            gotPath = false;
            UpdateLineRenderer( new List<HexTile>());
        }else{
            currentTile = currentPath[0];

            nextTile = currentPath[1];
            //Nos movemos
            this.transform.position = nextTile.transform.position + new Vector3(0,1f,0);

            //If the next tile is non traversable, stop moving
            /*
            if(nextTile.tileType != HexTileGenerationSettings.TileType.Standard){
                currentPath.Clear();
                HandleMovement();
                return;
            }*/
            //targetPosition = nextTile.transform.position + new Vector3(0,1f,0);
            gotPath = true;
            currentPath.RemoveAt(0);
   //         TileManager.instance.playerPos = nextTile.cubeCoordinate;
            UpdateLineRenderer(currentPath);
        }
    }

    public bool GotCurrentPath(){
        return gotPath;
    }
}
