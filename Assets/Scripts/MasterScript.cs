using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class MasterScript : MonoBehaviour
{
    public static MasterScript instance;
    // ==== SOUND ====
    [SerializeField] private AudioClip gameOverFx;
    [SerializeField] private AudioClip UiFx;
    // ===============
    // ======INPUT====
    [SerializeField] private InputActionReference place;
    // ===============
    // ======MISC=====
    [SerializeField] private Camera _cam;
    [SerializeField] CubeScript prefab;
    [SerializeField] CubeScript specialPrefab;
    [SerializeField] private GameObject markers;
    [SerializeField] public UIMaster ui;
    // ===============
    // =====LOGIC=====
    private bool onX = false;
    public int score = -1;
    private bool doUpdateCamY = false;
    private float MOVE_UP_AMOUNT = .5f;
    private Vector3 defaultMarkersTransform;
    public int perfectCounter = 0;
    private bool isGameOver = false;
    private Vector3 cameraEndedPos;
    private Vector3 cameraDefaultPos;
    private Vector3 cameraTargetPos;
    // ===============
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        defaultMarkersTransform = markers.transform.localScale;
        _cam = Camera.main;
        cameraDefaultPos = _cam.transform.position;
        next(prefab.transform.localScale, Vector3.zero, false, false);
        place.action.started += placeAction;
    }


    private void placeAction(InputAction.CallbackContext obj)
    {
        ui.gameStart();
        place.action.started -= placeAction;
    }

    void Update()
    {
        if (doUpdateCamY)
        {
            float step = 2 * Time.deltaTime;
            Vector3 camCurrentPos = _cam.transform.position;
            _cam.transform.position = Vector3.MoveTowards(camCurrentPos, cameraTargetPos, step);
        }
        if (isGameOver && cameraEndedPos.y - cameraDefaultPos.y > 10)
        {
            float step = 2 * Time.deltaTime;
            Vector3 camCurrentPos = _cam.transform.position;
            _cam.transform.position = Vector3.MoveTowards(camCurrentPos, cameraEndedPos, step);
        }
    }

    public void next(Vector3 scale, Vector3 pos, bool wasPerfect, bool wasSpecial)
    {
        onX = !onX;
        markers.transform.rotation = onX ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, -90, 0);
        markers.transform.position = new Vector3(pos.x, pos.y + scale.y, pos.z);
        cameraTargetPos = _cam.transform.position + Vector3.up * MOVE_UP_AMOUNT;
        if (UnityEngine.Random.Range(0f, 1f) < 0.1f)
        {
            setupSpecialObject(scale, pos);
        }
        else
        {
            setupObject(scale, pos);
        }

        doUpdateCamY = true;
        //TODO instead of bool - cubeScript should send a "scoreToAdd" so master doesn't maintain this logic
        if (wasPerfect)
        {
            score = score + 2;
            StartCoroutine(ui.scoreGoUp(score));
        }
        if (wasSpecial)
        {
            score = score + 10;
            StartCoroutine(ui.scoreGoUp(score));
        }
        else
        {
            score++;
        }
        ui.scoreUp(score);
    }

    public void restartGame()
    {
        SceneManager.LoadScene("Game");
    }

    void setupSpecialObject(Vector3 scale, Vector3 pos)
    {
        var nextObject = Instantiate(specialPrefab, transform);
        var currentObject = nextObject.GetComponent<CubeScript>();
        currentObject.transform.localScale = scale;
        currentObject.markers = markers;
        currentObject.isSpecial = true;
        currentObject.speed = 1f + (perfectCounter * .05f);
        currentObject.onX = onX;
        currentObject.masterScript = this;
        currentObject.lastPos = pos;
        currentObject.lastScale = scale;
    }

    void setupObject(Vector3 scale, Vector3 pos)
    {
        var nextObject = Instantiate(prefab, transform);
        var currentObject = nextObject.GetComponent<CubeScript>();
        currentObject.color = new Color(UnityEngine.Random.Range(0F, 1F), UnityEngine.Random.Range(0, 1F), UnityEngine.Random.Range(0, 1F));
        currentObject.transform.localScale = scale;
        currentObject.markers = markers;
        currentObject.speed = .8f + (perfectCounter * .05f);
        currentObject.onX = onX;
        currentObject.masterScript = this;
        currentObject.lastPos = pos;
        currentObject.lastScale = scale;
    }

    public void gameOver()
    {
        isGameOver = true;
        cameraEndedPos = _cam.transform.position;
        _cam.transform.position = cameraDefaultPos;
        SoundMaster.instance.playGameOverAudio(gameOverFx, transform);
        ui.gameOver(score);
    }

    public void interruptPerfect()
    {
        perfectCounter = 0;
    }
}


