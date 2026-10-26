using TMPro;
using UnityEngine;
using Vuforia;

/// <summary>
/// Muestra en pantalla el estado del tracking del dispositivo y si se está detectando el piso,
/// para poder diagnosticar en el celular por qué no aparece el indicador.
/// </summary>
public class GroundPlaneStatusUI : MonoBehaviour
{
    [SerializeField] private PlaneFinderBehaviour _planeFinder;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private float _floorTimeout = 0.5f;

    private TargetStatus _deviceStatus = TargetStatus.NotObserved;
    private float _lastFloorHitTime = -1f;

    private void Start()
    {
        if (_planeFinder == null)
            _planeFinder = FindAnyObjectByType<PlaneFinderBehaviour>();

        if (_planeFinder != null)
            _planeFinder.OnAutomaticHitTest.AddListener(HandleAutomaticHitTest);

        VuforiaApplication.Instance.OnVuforiaStarted += HandleVuforiaStarted;
        if (VuforiaApplication.Instance.IsRunning)
            HandleVuforiaStarted();
    }

    private void OnDestroy()
    {
        if (_planeFinder != null)
            _planeFinder.OnAutomaticHitTest.RemoveListener(HandleAutomaticHitTest);

        VuforiaApplication.Instance.OnVuforiaStarted -= HandleVuforiaStarted;
        if (VuforiaBehaviour.Instance != null && VuforiaBehaviour.Instance.DevicePoseBehaviour != null)
            VuforiaBehaviour.Instance.DevicePoseBehaviour.OnTargetStatusChanged -= HandleDeviceStatusChanged;
    }

    private void HandleVuforiaStarted()
    {
        DevicePoseBehaviour devicePose = VuforiaBehaviour.Instance.DevicePoseBehaviour;
        if (devicePose == null)
        {
            _label.text = "Falta Device Pose Behaviour en la ARCamera";
            return;
        }

        devicePose.OnTargetStatusChanged -= HandleDeviceStatusChanged;
        devicePose.OnTargetStatusChanged += HandleDeviceStatusChanged;
        _deviceStatus = devicePose.TargetStatus;
    }

    private void HandleDeviceStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        _deviceStatus = status;
    }

    private void HandleAutomaticHitTest(HitTestResult result)
    {
        if (result != null)
            _lastFloorHitTime = Time.time;
    }

    private void Update()
    {
        if (_label == null)
            return;

        bool floorDetected = _lastFloorHitTime >= 0f && Time.time - _lastFloorHitTime < _floorTimeout;
        string device = $"Tracking: {_deviceStatus.Status} ({_deviceStatus.StatusInfo})";

        if (floorDetected)
            _label.text = $"{device}\nPiso detectado: tocá para colocar";
        else if (_deviceStatus.Status == Status.TRACKED)
            _label.text = $"{device}\nApuntá al piso y mové el celu despacio";
        else
            _label.text = $"{device}\nMové el celu despacio para iniciar el tracking";
    }
}
