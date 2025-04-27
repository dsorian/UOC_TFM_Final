using UnityEngine;
using System.Collections;


//De este hilo: https://discussions.unity.com/t/throw-an-object-along-a-parabola/490479/2
public class ThrowSimulation : MonoBehaviour
{
    public Transform target=null;
    public float firingAngle = 45.0f;
    public float gravity = 9.8f;

//    public GameObject projectile;      
    public Transform projectileOrigin;
    public bool disparar = false;
    public GameObject miCatapulta;

    void Awake()
    {
        //myTransform = transform;      
        //firingAngle += UnityEngine.Random.Range(0,6);
    }

    void Start()
    {           
        StartCoroutine(SimulateProjectile());
    }


    IEnumerator SimulateProjectile()
    {
        while(true){
            
                // Short delay added before Projectile is thrown
                yield return new WaitForSeconds(0.5f);

                // Move projectile to the position of throwing object + add some offset if needed.
                /*projectile.*/transform.position = projectileOrigin.position + new Vector3(0, 0.0f, 0);
                
                // Calculate distance to target
                float target_Distance = Vector3.Distance(/*projectile.*/transform.position, target.position);

                // Calculate the velocity needed to throw the object to the target at specified angle.
                float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);

                // Extract the X  Y componenent of the velocity
                float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
                float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

                // Calculate flight time.
                float flightDuration = target_Distance / Vx;
        
                // Rotate projectile to face the target.
                if ( (target.position - /*projectile.*/transform.position) != Vector3.zero)
                    /*projectile.*/transform.rotation = Quaternion.LookRotation(target.position - /*projectile.*/transform.position);
            if(disparar){    
                float elapse_time = 0;
                while (elapse_time < flightDuration)
                {
                    //Aplicamos la rotación a la roca
                    /*projectile.*/transform.GetChild(0).transform.Rotate(0.25f,0.25f,0.5f);
                    /*projectile.*/transform.Translate(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);

                    elapse_time += Time.deltaTime;

                    yield return null;
                }
                //Para no volver a disparar
                disparar = false;
                target.position = projectileOrigin.position;
                /*projectile.*/transform.SetParent(miCatapulta.transform);
                /*projectile.*/GetComponent<ParticleSystem>().Stop();
                
            }
        }
    }   
}