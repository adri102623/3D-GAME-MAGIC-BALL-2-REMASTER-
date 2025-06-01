using UnityEngine;
using System.Collections;

public class CameraIntroMove : MonoBehaviour
{
    public Transform nave; // Asigna aquí la nave (player) desde el inspector
    public Transform cameraTarget; // Un empty en la posición final deseada de la cámara
    public float duration = 2f; // Segundos que dura la animación

    [HideInInspector] public bool introFinished = false; // Indica si la intro ha acabat

    void Start()
    {
        if (nave != null && cameraTarget != null)
            StartCoroutine(MoveCameraIntro());
    }

    IEnumerator MoveCameraIntro()
    {
        Vector3 startPos = nave.position;
        Quaternion startRot = transform.rotation;
        Vector3 endPos = cameraTarget.position;
        Quaternion endRot = cameraTarget.rotation;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float curveT = Mathf.SmoothStep(0, 1, t);

            // Interpolació de posició
            transform.position = Vector3.Lerp(startPos, endPos, curveT);

            // Interpolació de rotació, però forçant X = 25
            Vector3 euler = Quaternion.Slerp(startRot, endRot, curveT).eulerAngles;
            euler.x = 25f;
            transform.rotation = Quaternion.Euler(euler);

            yield return null;
        }
        // Assegura la posició i rotació final exacta
        transform.position = endPos;
        Vector3 finalEuler = cameraTarget.rotation.eulerAngles;
        finalEuler.x = 25f;
        transform.rotation = Quaternion.Euler(finalEuler);

        introFinished = true;
    }
}