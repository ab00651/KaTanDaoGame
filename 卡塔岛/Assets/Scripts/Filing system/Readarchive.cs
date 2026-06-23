using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Readarchive : MonoBehaviour
{
    private Button btn;

    private static string SavePath =>
        System.IO.Path.Combine(Application.persistentDataPath, "save.json");

    void Start()
    {
        btn = GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("Readarchive: 未找到 Button 组件");
            return;
        }

        bool hasSave = System.IO.File.Exists(SavePath) &&
                       new System.IO.FileInfo(SavePath).Length > 0;

        btn.interactable = hasSave;

        if (hasSave)
            btn.onClick.AddListener(OnLoadSaveClicked);
    }

    private void OnLoadSaveClicked()
    {
        MapVisualizer.LoadFromSaveOnStart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
