using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class UIMaster : MonoBehaviour
{
    public static UIMaster instance;
    [SerializeField] private AudioClip uiFx;
    [SerializeField] private CanvasGroup startUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private CanvasGroup scoreUI;
    [SerializeField] private GameObject gameOverScore;

    [SerializeField] private CanvasGroup gameOverUi;
    [SerializeField] private float fadeInDuration = 1;
    [SerializeField] private float scoreGoingUpDuration = 1.5f;

    [SerializeField] private AudioListener cameraAudioListener;

    [SerializeField] private bool muted = false;
    [SerializeField] private Button muteButton;
    [SerializeField] private Sprite notMuted, onMuted;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        scoreUI.alpha = 0;
        scoreUI.alpha = 0;
        startUI.alpha = 1;
        gameOverUi.enabled = false;
    }

    public void gameStart()
    {
        startUI.alpha = 0;
        scoreUI.alpha = 1;
    }

    public void gameOver(int finalScore)
    {
        gameOverUi.enabled = true;
        scoreUI.alpha = 0;
        StartCoroutine(uiFadeIn(gameOverUi, gameOverUi.alpha, 1, fadeInDuration));
        StartCoroutine(scoreGoingUp(gameOverScore.GetComponent<Text>(), finalScore, scoreGoingUpDuration));
    }

    public void scoreUp(int score)
    {
        scoreUI.GetComponent<Text>().text = score.ToString();
    }

    private IEnumerator uiFadeIn(CanvasGroup ui, float start, float end, float duration)
    {
        gameOverUI.SetActive(true);
        float time = 0.0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            ui.alpha = Mathf.Lerp(start, end, time / duration);
            yield return null;
        }
        ui.alpha = end;
    }

    public IEnumerator scoreGoUp(int finalScore)
    {
        Text ui = scoreUI.GetComponent<Text>();
        float time = 0.0f;
        float duration = 1f;
        while (time < duration)
        {
            time += Time.deltaTime;
            ui.text = Mathf.RoundToInt(Mathf.Lerp(System.Convert.ToInt32(ui.text), finalScore, time / duration)).ToString();
            yield return null;
        }
        ui.text = finalScore.ToString();
    }

    private IEnumerator scoreGoingUp(Text score, float finalScore, float duration)
    {
        float time = 0.0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            score.text = Mathf.RoundToInt(Mathf.Lerp(0, finalScore, time / duration)).ToString();
            yield return null;
        }
        score.text = finalScore.ToString();
    }

    public void muteButtonLogic()
    {
        muted = !muted;
        if (muted)
        {
            muteButton.image.sprite = onMuted;
        }
        else
        {
            muteButton.image.sprite = notMuted;
        }
        cameraAudioListener.enabled = !muted;
    }

}
