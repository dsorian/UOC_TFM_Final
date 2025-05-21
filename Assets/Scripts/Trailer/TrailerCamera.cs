using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerCamera : MonoBehaviour
{
    [System.Serializable]
    public class CameraPoint
    {
        public Transform positionPoint; // Punto al que se moverá la cámara
        public Transform targetPoint; // Punto al que la cámara mirará
        public float pauseDuration = 0f; // Tiempo que la cámara permanecerá en este punto
    }

    public List<CameraPoint> cameraPoints; // Lista de puntos de la cámara
    public float moveSpeed = 1f; // Velocidad de movimiento de la cámara
    public float rotationSpeed = 2f; // Velocidad de rotación de la cámara

    private int currentPointIndex = 0; // Índice del punto actual
    private bool isPaused = false; // Indica si la cámara está en pausa
    private float pauseTimer = 0f; // Temporizador para la pausa

    void Start()
    {
        if (cameraPoints.Count > 0)
        {
            // Asegurarse de que la cámara comience en el primer punto
            transform.position = cameraPoints[0].positionPoint.position;
            if (cameraPoints[0].targetPoint != null)
            {
                transform.LookAt(cameraPoints[0].targetPoint);
            }
        }
    }

    void Update()
    {
        if (cameraPoints.Count == 0) return;

        if (isPaused)
        {
            // Gestionar la pausa
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
                MoveToNextPoint();
            }
            return;
        }

        // Mover la cámara hacia el punto objetivo
        CameraPoint currentPoint = cameraPoints[currentPointIndex];
        transform.position = Vector3.MoveTowards(transform.position, currentPoint.positionPoint.position, moveSpeed * Time.deltaTime);

        // Suavizar la rotación hacia el siguiente punto o el objetivo
        Transform lookTarget = currentPoint.targetPoint != null ? currentPoint.targetPoint : currentPoint.positionPoint;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Verificar si la cámara ha alcanzado el punto actual
        if (Vector3.Distance(transform.position, currentPoint.positionPoint.position) < 0.1f)
        {
            if (currentPoint.pauseDuration > 0f)
            {
                // Iniciar pausa
                isPaused = true;
                pauseTimer = currentPoint.pauseDuration;
            }
            else
            {
                // Pasar al siguiente punto
                MoveToNextPoint();
            }
        }
    }

    private void MoveToNextPoint()
    {
        currentPointIndex++;
        if (currentPointIndex >= cameraPoints.Count)
        {
            currentPointIndex = 0; // Reiniciar al primer punto (opcional, para bucle)
        }
        Debug.Log("TrailerCamera: MoveToNextPoint currentPointIndex: " + currentPointIndex+" Mirando: "+cameraPoints[currentPointIndex].targetPoint.name);
    }
}