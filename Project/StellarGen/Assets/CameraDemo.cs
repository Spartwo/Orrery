using SystemGen;
using UnityEngine;
using Universe;

public class CameraDemo : MonoBehaviour
{

    public CameraMovement cameraController;
    bool isDemoMode = false;
    public SystemManager systemManager;
    private float toggleInterval = 600.0f;
    private float toggleTimer = 0f;

    void Update()
    {
        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftControl))
        {
            isDemoMode = !isDemoMode;
            Logger.Log(GetType().Name, "Enabling Demo Mode");
        }

        if (isDemoMode)
        {
            cameraController.yaw -= 15f / 7500 * 1 / Time.unscaledDeltaTime;


            toggleTimer += Time.deltaTime;
            if (toggleTimer >= toggleInterval)
            {
                toggleTimer = 0f;
                systemManager.useLogScaling = !systemManager.useLogScaling;
                Logger.Log(GetType().Name, $"Demo toggled useLogScaling to {systemManager.useLogScaling}");
            }
        }
    }
}
