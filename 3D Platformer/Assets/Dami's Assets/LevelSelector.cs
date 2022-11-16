using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public int level;
    public Text levelText;
    // Start is called before the first frame update
    void Start()
    {
        levelText.text = level.ToString();
    }

    // Update is called once per frame
    public void OpenScene()
    {
        SceneManager.LoadScene("Level " + level.ToString());
    }
}
