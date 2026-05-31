using UnityEngine;

// Hace que la camara siga al jugador desde arriba y por detras.
// Importante: la camara NO se mueve para los lados (eje X), solo sigue el
// avance del jugador hacia adelante (eje Z). Asi cuando el jugador cambia de
// carril, la camara se queda quieta y se nota el movimiento.
public class CameraFollow : MonoBehaviour
{
    [Header("A quien sigue")]
    public Transform target; // el jugador

    [Header("Posicion de la camara")]
    public float yOffset = 4.5f;   // que tan arriba va la camara
    public float zOffset = -7.5f;  // que tan atras va (negativo = detras)
    public float fixedX = 0f;      // la X queda fija siempre (centro de la calle)
    public float pitchAngle = 18f; // cuanto se inclina la camara hacia abajo

    [Header("Suavizado")]
    public float followSmoothness = 12f; // que tan rapido sigue al jugador

    private void Start()
    {
        // si no asignamos el jugador lo buscamos por el tag
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("[CameraFollow] no encontre al jugador con el tag 'Player'.");
            }
        }

        // inclinamos la camara hacia abajo desde el inicio
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    // usamos LateUpdate (despues de mover al jugador) para que no tiemble la camara
    private void LateUpdate()
    {
        if (target == null) return;

        // la posicion a la que queremos llegar:
        // X fija, Y arriba del jugador, Z detras siguiendo su avance
        Vector3 desiredPosition = new Vector3(
            fixedX,
            target.position.y + yOffset,
            target.position.z + zOffset
        );

        // nos vamos acercando de a poco para que se vea suave
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmoothness * Time.deltaTime
        );

        // mantenemos la camara siempre con la misma inclinacion
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }
}
