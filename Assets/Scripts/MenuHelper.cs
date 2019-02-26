using UnityEngine;
using UnityEngine.SceneManagement;
using Wheeled.Networking;

internal sealed class MenuHelper : MonoBehaviour
{

    public string sceneName = "Scenes/MainScene";

    public NetworkHostHolder holder;

    private void StartScene()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void StartServer()
    {
        holder.isServer = true;
        StartScene();
    }

    public void StartClient()
    {
        holder.isServer = false;
        StartScene();
    }
}
