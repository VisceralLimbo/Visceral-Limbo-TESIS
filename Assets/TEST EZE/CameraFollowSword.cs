using UnityEngine;

public class CameraFollowSword : MonoBehaviour
{
    [Header("config")]
    public float rotationAmount;    // rotacion de la cam
    public float returnSpeed;     // velocidad de vuelta a la pos original
    public float duration;       // duracion

    private Quaternion initialRot; //rotacion orig de la cam
    private Quaternion targetRot; //rotacion q va a hacer 
    private float timer; //cuando dura 

    void Start()
    {
        initialRot = transform.localRotation;
    }

    void Update()
    {
        // lerp para q no sea todo tosco
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRot,
            Time.deltaTime * returnSpeed
        );
    }

    public void DoHitEffect(Vector3 dir)
    {
        timer = duration;

        // costo pero si ataco para arriba/abajo la camara se mueve un toque hacia adelante y der/izq lo mismo pero para costados
        Vector3 camMove = new Vector3(-Mathf.Abs(dir.y) * rotationAmount, dir.x * rotationAmount, 0);
        targetRot = initialRot * Quaternion.Euler(camMove);

        // cancel porque se puede pegar seguido en el jueguito
        // y el invoke para llamar al reset 
        CancelInvoke(nameof(ResetRotation));
        Invoke(nameof(ResetRotation), duration);
    }

    private void ResetRotation()
    {
        //vuelve a la pos original
        targetRot = initialRot;
    }
}
