using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.forward; // Eje de rotación (por defecto, eje Z)
    public float rotationSpeed = 90f; // Velocidad de rotación en grados por segundo
    public float maxAngle = 160f; // Máximo ángulo de rotación (media vuelta)

    private float currentAngle = 60f; // Ángulo acumulado de rotación
    private bool movingForward = true; // Dirección del movimiento (true = adelante, false = regreso)

    void Update()
    {
        // Calcular el cambio de ángulo basado en la velocidad y el tiempo
        float angleChange = rotationSpeed * Time.deltaTime;

        if (movingForward)
        {
            // Rotar hacia adelante hasta llegar a maxAngle
            currentAngle += angleChange;
            transform.Rotate(rotationAxis * angleChange, Space.Self);

            if (currentAngle >= maxAngle)
            {
                currentAngle = maxAngle;
                movingForward = false; // Cambiar dirección
            }
        }
        else
        {
            // Rotar hacia atrás hasta llegar a 0
            currentAngle -= angleChange;
            transform.Rotate(rotationAxis * -angleChange, Space.Self);

            if (currentAngle <= 0f)
            {
                currentAngle = 0f;
                movingForward = true; // Cambiar dirección
            }
        }
    }
}