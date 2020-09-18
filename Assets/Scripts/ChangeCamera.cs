using UnityEngine;
using UnityEngine.UI;

public class ChangeCamera : MonoBehaviour
{
    public Button changeCamBut;
    int currentCamera = 0;

    Camera[] allCameras;
    void Start()
    {
        allCameras = new Camera[Camera.allCamerasCount];
        Camera.GetAllCameras(allCameras);

        changeCamBut.onClick.AddListener(ChangeActiveCamera);
        ChangeActiveCamera();
    }

    void ChangeActiveCamera()
    {
        foreach(var cam in allCameras)
        {
            cam.enabled = false;
        }
        allCameras[currentCamera].enabled = true;
        
        currentCamera++;
        if (currentCamera > allCameras.Length - 1) currentCamera = 0;
    }
}
