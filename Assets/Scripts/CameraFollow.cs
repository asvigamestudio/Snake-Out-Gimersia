using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 6, -10);
    public float positionSmooth = 5f;
    public float rotationSmooth = 2f;

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        // Smooth posisi
        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref currentVelocity, 1f / positionSmooth);

        // Smooth rotasi (sedikit delay)
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmooth * Time.deltaTime);
    }
}
