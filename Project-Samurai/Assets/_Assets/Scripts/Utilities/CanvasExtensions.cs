using UnityEngine;

public static class CanvasExtensions
{
    private static Vector2 WorldToCanvasPosition(Canvas canvas, Camera worldCamera, Vector3 worldPosition)
    {
        Vector2 viewportPoint = worldCamera.WorldToViewportPoint(worldPosition);

        var rootCanvasTransform = (canvas.isRootCanvas ? canvas.transform : canvas.rootCanvas.transform) as RectTransform;
        var rootCanvasSize = rootCanvasTransform!.rect.size;

        var rootCoord = (viewportPoint - rootCanvasTransform.pivot) * rootCanvasSize;
        if (canvas.isRootCanvas)
            return rootCoord;

        var rootToWorldPos = rootCanvasTransform.TransformPoint(rootCoord);
        return canvas.transform.InverseTransformPoint(rootToWorldPos);
    }
}
