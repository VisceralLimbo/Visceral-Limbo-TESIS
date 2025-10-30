using UnityEngine;
using System.Collections;

public class CameraFollowSword : MonoBehaviour
{
    [Header("Configuración del Efecto")]
    [Tooltip("Máxima cantidad de rotación o desplazamiento (ejes X, Y, Z).")]
    public Vector3 hitStrength = new Vector3(3f, 3f, 5f); // Más control, con un pequeño 'roll' (Z)

    [Tooltip("Duración total del efecto (ida y vuelta).")]
    public float totalDuration = 0.25f;

    [Tooltip("Curva para suavizar el movimiento de la cámara.")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Quaternion initialRot; // Rotación original de la cámara
    private Coroutine currentHitCoroutine; // Para controlar la corutina y detenerla si hay un nuevo golpe

    void Start()
    {
        // Almacena la rotación local inicial de la cámara.
        initialRot = transform.localRotation;
    }

    /// <summary>
    /// Inicia el efecto de seguimiento de la cámara para un golpe específico.
    /// </summary>
    /// <param name="dir">Vector de dirección del ataque (ej: Vector3.left, Vector3.up).</param>
    public void DoHitEffect(Vector3 dir)
    {
        // Si ya hay un golpe en curso, lo detiene inmediatamente para empezar el nuevo.
        if (currentHitCoroutine != null)
        {
            StopCoroutine(currentHitCoroutine);
            // Opcional: Reiniciar la rotación para evitar acumulación si el ataque es muy rápido
            transform.localRotation = initialRot;
        }

        // Inicia la nueva corutina de efecto
        currentHitCoroutine = StartCoroutine(HitEffectCoroutine(dir));
    }

    private IEnumerator HitEffectCoroutine(Vector3 dir)
    {
        float timer = 0f;

        // rotacion a la q voy a ir
        // se inclina la cam en direccion opuesta

        // El 'roll' (Z) es el que simula la inclinación de la cámara.
        float roll = -dir.x * hitStrength.z; // Si golpea a la derecha (dir.x=-1), roll es positivo.

        // El 'pitch' (X) simula el movimiento arriba/abajo (golpe arriba, mira abajo).
        float pitch = -dir.y * hitStrength.x; // Si golpea hacia arriba (dir.y=1), pitch es negativo.

        // El 'yaw' (Y) es menos común para espadas, pero puede ser un pequeño balanceo lateral.
        float yaw = dir.x * hitStrength.y;

        Quaternion targetRot = initialRot * Quaternion.Euler(pitch, yaw, roll);

        Quaternion startRot = transform.localRotation;

        // ida
        // mitad de la duracion es para hacer la rotacion la otra para volver
        float halfDuration = totalDuration / 2f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / halfDuration);
            // suavizado de la curva
            float curveT = easeCurve.Evaluate(t);

            transform.localRotation = Quaternion.Slerp(startRot, targetRot, curveT);
            yield return null;
        }

        // vuelta a la rotacion original
        timer = 0f;
        startRot = transform.localRotation; // rotacion actual punto de partida

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / halfDuration);
            // curva de suavizado
            float curveT = easeCurve.Evaluate(t);

            // vuelvo al inicial
            transform.localRotation = Quaternion.Slerp(startRot, initialRot, curveT);
            yield return null;
        }

        // rotacion vuelve al inicio
        transform.localRotation = initialRot;
        currentHitCoroutine = null;
    }
}
