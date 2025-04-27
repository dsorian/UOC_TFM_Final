using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(SphereCollider))]
public class HexTile : MonoBehaviour
{

    private Mesh m_mesh;
    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;

    private List<Face> m_faces;

    public Material materialCelda;
    public int tipoCelda; //Si es 0=Neutral(Negro) 1=Agua 2=Hierba 3=Roca 4=Arena...?
    public Material[] materiales; //indice=tipoCelda 0 = materialNeutral, materialAgua,materialHierba,materialRoca,materialArena;
    public int numEstado;
    public float innersize = 1.0f;
    public float outerSize = 2.0f;
    public float height = 0.29f;
    public bool isFlatTopped;   //Para elegir cómo representar el hexágono (girado o no)
    public string nombre = "Hex";   //Según el nombre indicará el tipo de loseta
    //coordenada y offseCoordinate guardan lo mismo, habría que eliminar una de las dos (la que menos se use y cambiarla a la otra)
    public Vector2Int coordenada;   //Fila y columna de la casilla en la matriz

    public Vector2Int offsetCoordinate; //Para la navegación (coordenadas en la matriz de celdas) (no sé si sobra y vale coordenada sólo BUSCAR DONDE SE USA)
    public Vector3Int cubeCoordinate; //Para la navegación
    public List<HexTile> neighbours;
    public bool[] fronterasPintadas = {false,false,false,false,false,false};   //Para saber si ha pintado sus fronteras
    //public GameObject decoradoCelda;   //Para poner el asset de árbol, montaña, etc...

    public struct Face{
        public List<Vector3> vertices { get; private set;}
        public List<int> triangles {get; private set; }
        public List<Vector2> uvs { get; private set; }

        public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs){
            this.vertices = vertices;
            this.triangles = triangles;
            this.uvs = uvs;
        }
    }

    private void Awake(){
     //   Debug.Log("Awake");
        m_meshFilter = GetComponent<MeshFilter>();
        m_meshRenderer = GetComponent<MeshRenderer>();

        m_mesh = new Mesh();
        m_mesh.name = nombre;

        m_meshFilter.mesh = m_mesh;
        m_meshRenderer.material = materialCelda;

        //GetComponent<BoxCollider>().size = new Vector3(2.0f,1.75f,1.25f);
        //GetComponent<BoxCollider>().isTrigger = true;
        GetComponent<SphereCollider>().radius = 1.5f;
        GetComponent<SphereCollider>().isTrigger = true;
    }

    private void OnEnable(){
//        DrawMesh();
    }

    //Para evitar warnings en el editor, quitar cuando se buildee el juego
    public void OnValidate(){
        if(Application.isPlaying){
//            DrawMesh();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if( m_meshRenderer.material != materialCelda )
            m_meshRenderer.material = materialCelda;
        DrawMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawMesh(){
        DrawFaces();
        CombineFaces();
    }

    protected void DrawFaces(){
        m_faces = new List<Face>();

        //Top faces
        for (int point = 0; point < 6 ; point++){
            m_faces.Add(CreateFace(innersize, outerSize, height / 2f, height / 2f, point));
        }

        //Botton faces
        for (int point = 0; point < 6; point++){
            m_faces.Add(CreateFace(innersize, outerSize, -height / 2f, -height / 2f, point, true));
        }

        //Outer faces
        for (int point = 0; point < 6; point++){
            m_faces.Add(CreateFace(outerSize, outerSize, height / 2f, -height / 2f, point, true));
        }
    }

    protected void CombineFaces(){
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < m_faces.Count; i++){
            //Add the vertices
            vertices.AddRange(m_faces[i].vertices);
            uvs.AddRange(m_faces[i].uvs);

            //Offset the triangles
            int offset = (4 * i);
            foreach (int triangle in m_faces[i].triangles){
                tris.Add(triangle + offset);
            }
        }

        m_mesh.vertices = vertices.ToArray();
        m_mesh.triangles = tris.ToArray();
        m_mesh.uv = uvs.ToArray();
        m_mesh.RecalculateNormals();
    }

    protected Face CreateFace(float innerRad, float outerRad, float heightA, float heightB, int point, bool reverse = false){
        //Filtramos el índice del punto para que la última cara conecte con la primera
        Vector3 pointA = GetPoint(innerRad, heightB, point);
        Vector3 pointB = GetPoint(innerRad, heightB, (point<5) ? point + 1 : 0);
        Vector3 pointC = GetPoint(outerRad, heightA, (point < 5) ? point + 1 : 0);
        Vector3 pointD = GetPoint(outerRad,heightA, point);

        List<Vector3> vertices = new List<Vector3>(){ pointA, pointB, pointC, pointD };
        List<int> triangles = new List<int>() {0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new List<Vector2>() { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1),new Vector2(0,1)};
        if( reverse){
            vertices.Reverse();
        }

        return new Face(vertices, triangles, uvs);
    }

    protected Vector3 GetPoint(float size, float height, int index){
        float angle_deg = isFlatTopped ? 60 * index: 60*index-30; //Restamos 30 grados para girarlo o no según isFlatTopped
        float angle_rad = Mathf.PI /180f * angle_deg;
        return new Vector3((size * Mathf.Cos(angle_rad)),height, size*Mathf.Sin(angle_rad));
    }

    public void MouseOverMe(){
        Debug.Log("Ratón sobre mí. Soy "+name+ " provincia tipo:"+numEstado);
    }

    public void OnHighlightTile(){
        TileManager.instance.OnHighlightTile(this);
    }

    public void OnSelectTile(){
        if( TileManager.instance.contadorTurnos%2 == 0 && TileManager.instance.oponenteCPU)
            return;
        TileManager.instance.OnSelectTile(this);
        foreach(HexTile vecino in neighbours){
            if( vecino.numEstado == numEstado){
                //Destroy(vecino.gameObject);
            }
        }
    }

    public void PintarMisFronteras(GameObject unaFrontera, GameObject unaPlaya){
        //List<Vector3> points = new List<Vector3>();
        int numVecino = 0;
        if (numEstado==-1 || numEstado == 0)
            return;
        foreach(Transform child in transform){
            if(child.name == "UnaFrontera(Clone)")
                Destroy(child.gameObject);
        }

        for(int i=0; i<fronterasPintadas.Length; i++){
            fronterasPintadas[i] = false;
        }
        foreach ( HexTile vecino in neighbours){
            if (vecino.numEstado != numEstado && vecino.IsfronteraPintada(numVecino) == false){
                //Para que esté a ras pos Y -0.48   Pero no se ve en la pantalla(cambiar material a ver)
                //Rotación: 0:30 grados 1:90 grados y sumar 60 a siguientes
                GameObject elObjeto;
                if(vecino.numEstado == 0 || numEstado == 0) //Límite con el mar, pintamos una playa
                    elObjeto = unaPlaya;
                else
                    elObjeto = unaFrontera;
                GameObject nuevoObjeto = Instantiate(elObjeto, Vector3.Lerp(vecino.transform.position, transform.position, 0.5f) + new Vector3(0, -0.48f, 0),elObjeto.transform.rotation);
                nuevoObjeto.transform.Rotate(new Vector3( 0,elObjeto.transform.rotation.y+(numVecino*60),0));
                nuevoObjeto.transform.SetParent(this.transform);
                fronterasPintadas[numVecino]=true;
            }
            numVecino++;
        }
    }

    //Devolveremos si elVecino tiene ya pintada la frontera para saber
    //si debemos pintar la nuestra y así no duplicar
    private bool IsfronteraPintada(int elVecino){
        bool respuesta = false;
        switch(elVecino){
            case 0: 
                respuesta = fronterasPintadas[3];
                break;
            case 1:
                respuesta = fronterasPintadas[4];
                break;
            case 2:
                respuesta = fronterasPintadas[5];
                break;
            case 3:
                respuesta = fronterasPintadas[0];
                break;
            case 4:
                respuesta = fronterasPintadas[1];
                break;
            case 5:
                respuesta = fronterasPintadas[2];
                break;
            default:
                respuesta = true;
                break;
        }
        return respuesta;
    }

    public void SetMaterial(Material elMaterial){
        materialCelda = elMaterial;
        gameObject.GetComponent<MeshRenderer>().material = elMaterial;
    }

    public void ResaltarCelda(Material elMaterial){
        gameObject.GetComponent<MeshRenderer>().material = elMaterial;
    }
  
    public void OnDrawGizmosSelected(){
/*
        foreach( HexTile neighbour in neighbours ){
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, neighbour.transform.position);
        }*/
    }
}
