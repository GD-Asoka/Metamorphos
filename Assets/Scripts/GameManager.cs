using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject Canvas, PauseMenu, ControlsMenu, overlay;
    private bool canWin = false;
    [HideInInspector]public int enemiesKilled, blueFlames, redFlames, druidPowers;
    public int enemyLimit, blueFlameCondition, redFlameCondition, druidPowerLimit;
    public TextMeshProUGUI enemiesKilledText, blueFlamesText, redFlamesText, druidPowersText;
    public Image key;
    public bool keyCollected;
    public AudioSource audioSource;
    public AudioClip[] bgm, pHurt, eHurt, eAttack, confused, singing, bored, tree, vine, bird, fish;
    public enum Player_VFX
    {
        Hurt,
        Sing, 
        Bored, 
        Tree, 
        Vine,
        Bird,
        Fish,
    }
    public Player_VFX playerVFX;
    public enum Enemy_VFX
    {
        Hurt, 
        Confused,
        Attack,
    }
    public Enemy_VFX enemyVFX;

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
        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        Pause();
        InvokeRepeating(nameof(CheckState), 0.1f, 0.1f); 
        var torches = FindObjectsOfType<Torch>();
        if (torches == null)
            return;
        blueFlames = 0;
        redFlames = 0;
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
        UpdateBGM();
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
    public void UpdateBGM()
    {
        var rand = Random.Range(0, bgm.Length);
        audioSource.clip = bgm[rand];
        audioSource.Play();
    }
    bool playerCanPlay, enemyCanPlay;
    public void PlayPlayerVFX(Player_VFX vfx)
    {
        if (!playerCanPlay)
            return;
        switch (vfx)
        {
            case Player_VFX.Hurt:
                StartCoroutine(PlayerVFX(pHurt[Random.Range(0, pHurt.Length)]));
                break;
            case Player_VFX.Sing:
                StartCoroutine(PlayerVFX(singing[Random.Range(0, singing.Length)]));
                break;
            case Player_VFX.Bored:
                StartCoroutine(PlayerVFX(bored[Random.Range(0, bored.Length)]));
                break;
            case Player_VFX.Tree:
                StartCoroutine(PlayerVFX(tree[Random.Range(0, tree.Length)]));
                break;
            case Player_VFX.Vine:
                StartCoroutine(PlayerVFX(vine[Random.Range(0, vine.Length)]));
                break;
            case Player_VFX.Bird:
                StartCoroutine(PlayerVFX(bird[Random.Range(0, bird.Length)]));
                break;
            case Player_VFX.Fish:
                StartCoroutine(PlayerVFX(fish[Random.Range(0, fish.Length)]));
                break;
        }
    }
    private IEnumerator PlayerVFX(AudioClip audioClip)
    {
        playerCanPlay = false;
        audioSource.PlayOneShot(audioClip);
        yield return new WaitForSeconds(1f);
        playerCanPlay = true;
    }
    public void PlayEnemyVFX(Enemy_VFX vfx)
    {
        if (!enemyCanPlay)
            return;
        switch(vfx)
        {
            case Enemy_VFX.Hurt:
                StartCoroutine(EnemyVFX(eHurt[Random.Range(0, eHurt.Length)]));
                break;
            case Enemy_VFX.Confused:
                StartCoroutine(EnemyVFX(confused[Random.Range(0, confused.Length)]));
                break;
            case Enemy_VFX.Attack:
                StartCoroutine(EnemyVFX(eAttack[Random.Range(0, eAttack.Length)]));
                break;
        }
    }
    private IEnumerator EnemyVFX(AudioClip audioClip)
    {
        enemyCanPlay = false;
        audioSource.PlayOneShot(audioClip);
        yield return new WaitForSeconds(1f);
        enemyCanPlay = true;
    }
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
