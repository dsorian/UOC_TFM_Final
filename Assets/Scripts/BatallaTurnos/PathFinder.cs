using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static List<HexTile> FindPath(HexTile origin, HexTile destination){

        Dictionary<HexTile, Node> nodesNotEvaluated = new Dictionary<HexTile, Node>();
        Dictionary<HexTile, Node> nodesAlreadyEvaluated = new Dictionary<HexTile, Node>();
        Node startNode = new Node(origin, origin, destination, 0);
        nodesNotEvaluated.Add(origin, startNode);

        bool gotPath = EvaluateNextNode(nodesNotEvaluated, nodesAlreadyEvaluated, origin, destination, out List<HexTile> path);
        
        while (!gotPath){
            gotPath = EvaluateNextNode(nodesNotEvaluated, nodesAlreadyEvaluated, origin, destination, out path);
            //Debug.Log("Nodos del path: "+ path.Count+" nodos no evaluados: "+nodesNotEvaluated.Count()+" nodos ya evaluados: "+ nodesAlreadyEvaluated.Count());
        }
        return path;
    }

    private static bool EvaluateNextNode(Dictionary<HexTile, Node> nodesNotEvaluated, Dictionary<HexTile, Node> nodesEvaluated, HexTile origin, HexTile destination, out List<HexTile> Path){
        Node currentNode = GetCheapestNode(nodesNotEvaluated.Values.ToArray());

        if( currentNode == null){
            Path = new List<HexTile>();
            return false;
        }

        nodesNotEvaluated.Remove(currentNode.target);
        nodesEvaluated.Add(currentNode.target, currentNode);

        Path = new List<HexTile>();

        //If this is our destination then we are done
        if( currentNode.target == destination ){
            Path.Add(currentNode.target);
            while( currentNode.target != origin ){
                Path.Add(currentNode.parent.target);
                currentNode = currentNode.parent;
            }
            return true;
        }
        //Otherwise, add out neighbours to the list and try to traverse them
        List<Node> neighbours = new List<Node>();
        foreach(HexTile tile in currentNode.target.neighbours){
            Node node = new Node(tile, origin, destination, currentNode.GetCost());

            
            //If the tile type isn't something we can traverse, make the cost very high
            if (tile.tipoCelda <= 1 ){  //0=neutral 1=agua
                node.baseCost = 99999999;
                //Continue
            }
            //Para no pasar por estados distintos al origen y destino
            if ( currentNode.target.numEstado != destination.numEstado && currentNode.target.numEstado != origin.numEstado){
                node.baseCost = 99999999;
            }

            neighbours.Add(node);
        } 

        foreach( Node neighbour in neighbours){
            //If the tile has already been evaluated fully we can ignore it
            if ( nodesEvaluated.Keys.Contains(neighbour.target)){ continue;}

            //If the cost is lower, of if the tile isn't in the not evaluated pile...
            if ( neighbour.GetCost() < currentNode.GetCost() || !nodesNotEvaluated.Keys.Contains(neighbour.target)){
                neighbour.SetParent(currentNode);
                if ( !nodesNotEvaluated.Keys.Contains(neighbour.target)){
                    nodesNotEvaluated.Add(neighbour.target, neighbour);
                }
            }
        }
        return false;
    }

    private static Node GetCheapestNode(Node[] nodesNotEvaluated){
        if(nodesNotEvaluated.Length == 0){ return null; }

        Node selectedNode = nodesNotEvaluated[0];

        for (int i = 1; i < nodesNotEvaluated.Length; i++){
            var currentNode = nodesNotEvaluated[i];
            if( currentNode.GetCost() < selectedNode.GetCost()){
                selectedNode = currentNode;
            }else if ( currentNode.GetCost() == selectedNode.GetCost() && currentNode.costToDestination < selectedNode.costToDestination){
                selectedNode = currentNode;
            }
        }
        return selectedNode;
    }
}
