using UnityEngine;
using System.Collections;

public class LevelPresentation : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera presentationCamera;
    public float rotationSpeed = 30f; // Grados por segundo
    public float presentationDuration = 5f; // Duración en segundos
    public float cameraDistance = 20f; // Distancia de la cámara al centro
    public float cameraHeight = 10f; // Altura de la cámara

    [Header("Level Center")]
    public Transform levelCenter; // Centro del nivel para rotar alrededor

    [Header("UI")]
    public GameObject gameplayUI; // UI del gameplay (a ocultar durante presentación)
    public GameObject presentationUI; // UI de presentación (opcional)

    private bool isPresentationActive = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    void Start()
    {
        // Auto-asignar la cámara principal si no está asignada
        if (presentationCamera == null)
        {
            presentationCamera = Camera.main;
            if (presentationCamera == null)
            {
                // Buscar cualquier cámara en la escena
                presentationCamera = FindFirstObjectByType<Camera>();
            }
        }

        // Si no se asigna levelCenter, usar el centro del mundo (0, 0, 0)
        if (levelCenter == null)
        {
            GameObject centerObject = new GameObject("LevelCenter");
            levelCenter = centerObject.transform;
            levelCenter.position = Vector3.zero; // Centro en 0, 0, 0
        }

        // Guardar posición original de la cámara
        if (presentationCamera != null)
        {
            originalCameraPosition = presentationCamera.transform.position;
            originalCameraRotation = presentationCamera.transform.rotation;
        }
    }

    public void StartPresentation()
    {
        if (isPresentationActive || presentationCamera == null) return;

        StartCoroutine(PresentationCoroutine());
    }

    private IEnumerator PresentationCoroutine()
    {
        isPresentationActive = true;

        // Desactivar UI de gameplay
        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        // Activar UI de presentación
        if (presentationUI != null)
            presentationUI.SetActive(true);

        // Desactivar controles del jugador
        DisablePlayerControls(true);

        // Configurar cámara para presentación
        SetupPresentationCamera();

        // Rotar durante el tiempo especificado
        float elapsedTime = 0f;
        float totalRotation = 30f; // Solo 30 grados como prueba

        while (elapsedTime < presentationDuration)
        {
            // Calcular el ángulo actual basado en el tiempo transcurrido
            float currentAngle = (elapsedTime / presentationDuration) * totalRotation;
            Vector3 position = CalculateCameraPosition(currentAngle);

            presentationCamera.transform.position = position;
            presentationCamera.transform.LookAt(levelCenter.position);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Finalizar presentación
        EndPresentation();
    }

    private void SetupPresentationCamera()
    {
        if (presentationCamera == null || levelCenter == null) return;

        // Posicionar cámara inicial en la posición original del nivel
        presentationCamera.transform.position = new Vector3(0, 90, 140);
        presentationCamera.transform.LookAt(levelCenter.position);
    }

    private Vector3 CalculateCameraPosition(float angle)
    {
        if (levelCenter == null) return originalCameraPosition;

        // Convertir ángulo a radianes
        float radians = angle * Mathf.Deg2Rad;

        // Calcular nueva posición rotando alrededor del centro (0, 0, 0)
        Vector3 originalPos = new Vector3(0, 90, 140);

        // Rotar solo en el eje Y (horizontal)
        float newX = originalPos.x * Mathf.Cos(radians) - originalPos.z * Mathf.Sin(radians);
        float newZ = originalPos.x * Mathf.Sin(radians) + originalPos.z * Mathf.Cos(radians);

        return new Vector3(newX, originalPos.y, newZ);
    }

    private void EndPresentation()
    {
        isPresentationActive = false;

        // Restaurar posición original de la cámara
        if (presentationCamera != null)
        {
            presentationCamera.transform.position = originalCameraPosition;
            presentationCamera.transform.rotation = originalCameraRotation;
        }

        // Reactivar UI de gameplay
        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        // Desactivar UI de presentación
        if (presentationUI != null)
            presentationUI.SetActive(false);

        // Reactivar controles del jugador
        DisablePlayerControls(false);
    }

    private void DisablePlayerControls(bool disable)
    {
        // Buscar y desactivar el jugador
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.enabled = !disable;
        }

        // Buscar y desactivar la pelota
        Ball ball = FindFirstObjectByType<Ball>();
        if (ball != null)
        {
            ball.enabled = !disable;

            // Detener la pelota durante la presentación
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                if (disable)
                {
                    ballRb.linearVelocity = Vector3.zero;
                    ballRb.angularVelocity = Vector3.zero;
                    ballRb.isKinematic = true; // Hacer la pelota kinematic durante presentación
                }
                else
                {
                    ballRb.isKinematic = false; // Restaurar física
                                                // Reiniciar velocidad de la pelota usando el método actualizado
                    ball.ResetBall();
                }
            }
        }
    }

    public void SkipPresentation()
    {
        if (isPresentationActive)
        {
            StopAllCoroutines();
            EndPresentation();
        }
    }
}