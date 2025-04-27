using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongCameraMover : MonoBehaviour
{
    public Transform pointA; // Primer punto al que se moverá la cámara
    public Transform pointB; // Segundo punto al que se moverá la cámara
    public Transform targetPoint; // Punto fijo al que la cámara mirará
    public float moveSpeed = 1f; // Velocidad de movimiento de la cámara

    private Transform currentTarget; // El punto hacia el que la cámara se está moviendo

    void Start()
    {
        // Inicia el movimiento hacia el primer punto
        currentTarget = pointA;
    }

    void Update()
    {
        // Mueve la cámara hacia el punto objetivo
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);

        // Cambia el objetivo si la cámara ha alcanzado el punto actual
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA; // Cambiar entre pointA y pointB
        }

        // Hacer que la cámara mire al punto fijo
        if (targetPoint != null)
        {
            transform.LookAt(targetPoint);
        }
    }
}