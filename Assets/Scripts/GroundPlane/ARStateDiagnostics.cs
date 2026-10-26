using UnityEngine;
using Vuforia;

public class ARStateDiagnostics : MonoBehaviour
{
    private void Start()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += HandleVuforiaStarted;
        DevicePoseBehaviour devicePose = Object.FindAnyObjectByType<DevicePoseBehaviour>();
        if (devicePose != null)
        {
            devicePose.OnTargetStatusChanged += HandlePoseStatus;
        }
        else
        {
            Debug.LogError("[AR Diagnostic] No se encontró DevicePoseBehaviour. El Device Tracker está apagado.");
        }
    }

    private void HandleVuforiaStarted()
    {
        Debug.Log("<color=green>[AR Diagnostic] Vuforia iniciado correctamente.</color>");
    }

    private void HandlePoseStatus(ObserverBehaviour behaviour, TargetStatus status)
    {
        Debug.Log($"<color=yellow>[AR Diagnostic] Estado del Tracker: {status.Status} | Detalle: {status.StatusInfo}</color>");
    }
}