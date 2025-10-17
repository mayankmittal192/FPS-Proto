using TMPro;
using Unity.NetCode;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI pingText;
    public TextMeshProUGUI fpsText;
    public RectTransform crosshair;

    private int fps;
    private float deltaTime;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
    }

    void Update()
    {
        // --- FPS ---
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        fps = (int)(1.0f / deltaTime);
        fpsText.text = $"Fps: {fps}";

        // --- Ping ---
        if (ConnectionManager.clientWorld == null || !ConnectionManager.clientWorld.IsCreated)
        {
            if (crosshair.gameObject.activeInHierarchy)
                crosshair.gameObject.SetActive(false);

            return;
        }

        var entityManager = ConnectionManager.clientWorld.EntityManager;

        // Get the client connection entity
        var query = entityManager.CreateEntityQuery(typeof(NetworkId), typeof(NetworkSnapshotAck));

        if (query.IsEmpty)
        {
            pingText.text = "Ping: -- ms";
            return;
        }

        var ack = query.GetSingleton<NetworkSnapshotAck>();
        pingText.text = $"Ping: {(int)ack.EstimatedRTT} ms";

        if (!crosshair.gameObject.activeInHierarchy)
            crosshair.gameObject.SetActive(true);
    }
}
