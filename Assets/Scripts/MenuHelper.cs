using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHelper : MonoBehaviour
{

    public string sceneName = "Scenes/MainScene";

    private void StartScene()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void StartServer()
    {
        NetTestChooser.StartServer();
        StartScene();
    }

    public void StartClient()
    {
        NetTestChooser.StartClient();
        StartScene();
    }
}
