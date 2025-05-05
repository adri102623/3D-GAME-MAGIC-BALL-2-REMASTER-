using UnityEngine;

public class BarrierCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Destroy(collision.gameObject); // Destruye la pelota
            Debug.Log("Pelota destruida por la barrera");
            // A�ade aqu� m�s l�gica si la necesitas (sonido, restar vidas, etc.)
        }
    }
}