using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject Canvas, PauseMenu, ControlsMenu, overlay;
    private bool canWin = false;
    public int enemiesKilled, blueFlames, redFlames, druidPowers;
    public int enemyLimit, blueFlameCondition, redFlameCondition, druidPowerLimit;
    public TextMeshProUGUI enemiesKilledText, blueFlamesText, redFlamesText, druidPowersText;
    public Image key;
    public bool keyCollected;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Pause();
        InvokeRepeating(nameof(CheckState), 0.1f, 0.1f); 
        var torches = FindObjectsOfType<Torch>();
        if (torches == null)
            return;
        foreach (var t in torches)
        {
            if (t.currentType == Torch.TorchType.Fire)
            {
                redFlames++;
            }
            else
            {
                blueFlames++;
            }
        }
        druidPowers--;
    }
    private void CheckState()
    {
        UpdateUI();
        CheckWin();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
        else if(Input.GetKeyDown(KeyCode.Tab))
        {
            Overlay();
        }
    }
    #region UI
    public void UpdateUI()
    {
        blueFlamesText.text = $"Blue Flames: {blueFlames}/{blueFlameCondition}";
        redFlamesText.text = $"Red Flames: {redFlames}/{redFlameCondition}";
        enemiesKilledText.text = $"Enemies Killed: {enemiesKilled}/{enemyLimit}";
        druidPowersText.text = $"Power Used: {(int)(druidPowers*0.5f)}/{druidPowerLimit}";
        if(enemiesKilled <= enemyLimit)
        {
            enemiesKilledText.color = Color.green;
        }
        else
        {
            enemiesKilledText.color = Color.red;
        }
        if(druidPowers <= druidPowerLimit)
        {
            druidPowersText.color = Color.green;
        }
        else
        {
            druidPowersText.color = Color.red;
        }
        if(blueFlames >= blueFlameCondition)
        {
            blueFlamesText.color = Color.green;
        }
        else
        {
            blueFlamesText.color = Color.red;
        }
        if(redFlames >= redFlameCondition)
        {
            redFlamesText.color = Color.green;
        }
        else
        {
            redFlamesText.color = Color.red;
        }
        var color = key.color;
        if (keyCollected)
        {
            canWin = true;
            key.color = new Color(color.r, color.g, color.b, 1);
        }
        else
        {
            canWin = false;
            key.color = new Color(color.r, color.g, color.b, 0.5f);
        }
    }
    public void Pause()
    {
        PauseMenu.SetActive(!PauseMenu.activeSelf);
        Time.timeScale = PauseMenu.activeSelf ? 0 : 1;
    }
    public void Overlay()
    {
        overlay.SetActive(!overlay.activeSelf);
    }
    public void Play()
    {
        PauseMenu.SetActive(!PauseMenu.activeSelf);
        Time.timeScale = PauseMenu.activeSelf ? 0 : 1;
    }
    public void Controls()
    {
        ControlsMenu.SetActive(true);
        PauseMenu.SetActive(false);
    }
    public void Back()
    {
        ControlsMenu.SetActive(false);
        PauseMenu.SetActive(true);
    }
    #endregion
    #region AUDIO

    #endregion
    #region SCENE MANAGEMENT
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void LoadNextLevel()
    {
        int levels = SceneManager.sceneCountInBuildSettings;
        if(SceneManager.GetActiveScene().buildIndex + 1 < levels)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
    public bool CheckWin()
    {       
        if(blueFlames >= blueFlameCondition && redFlames >= redFlameCondition && keyCollected)
        {            
            canWin = true;
        }
        else
        {
            canWin = false;
        }
        return canWin;
    }
    #endregion
}
