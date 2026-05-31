using UnityEngine;

// Marca a los obstaculos estaticos (como los autos) que el jugador SI puede saltar.
// Le ajusta el collider para que no sea tan alto y se pueda esquivar saltando.
public class VehicleObstacle : MonoBehaviour
{
    public bool isVehicle = true; // si es un auto o no (por si lo necesitamos despues)

    private void Start()
    {
        // dejamos el collider mas bajo para que el salto alcance a pasarlo
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            // el salto permite pasar cosas de hasta 1.4 de alto mas o menos
            boxCollider.size = new Vector3(boxCollider.size.x, Mathf.Min(boxCollider.size.y, 1.4f), boxCollider.size.z);
            boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.size.y / 2f, boxCollider.center.z);
        }
        else
        {
            // por si usa capsula en vez de caja
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                capsuleCollider.height = Mathf.Min(capsuleCollider.height, 1.4f);
                capsuleCollider.center = new Vector3(capsuleCollider.center.x, capsuleCollider.height / 2f, capsuleCollider.center.z);
            }
        }
    }
}
