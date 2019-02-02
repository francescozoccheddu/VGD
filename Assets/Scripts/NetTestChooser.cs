using UnityEngine;
using UnityEngine.SceneManagement;

public class NetTestChooser : MonoBehaviour
{

    public static bool IsServer { get; private set; }

    public string sceneName;

    private void StartScene()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void StartServer()
    {
        StartScene();
        IsServer = true;
    }

    public void StartClient()
    {
        StartScene();
        IsServer = false;
    }

}
