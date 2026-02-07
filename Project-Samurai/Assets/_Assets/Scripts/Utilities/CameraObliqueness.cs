using UnityEngine;

public class CameraObliqueness : MonoBehaviour
{
    public float horizObl;
    public float vertObl;

    [ContextMenu("Set obliqueness")]
    public void SetObliqueness()
    {
        Matrix4x4 mat = Camera.main.projectionMatrix;
        mat[0, 2] = horizObl;
        mat[1, 2] = vertObl;
        Camera.main.projectionMatrix = mat;
    }
}
