using UnityEngine;

public class PickupHealth : MonoBehaviour
{
    public int vidas = 1;

    public Material material2vidas;
    public Material material1vida;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        UpdateMaterial();
    }
    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Ball"))
        {
            TocarPelota();
        }
    }

    public void TocarPelota()
    {
        vidas--;
        if (vidas <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            UpdateMaterial();
        }
    }

    void UpdateMaterial()
    {
        if (vidas == 2 && material2vidas != null)
        {
            rend.material = material2vidas;
        }
        else if (vidas == 1 && material1vida != null)
        {
            rend.material = material1vida;
        }
    }
}
