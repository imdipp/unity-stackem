using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class CubeScript : MonoBehaviour
{
    // ==== SOUND ====
    [SerializeField] private AudioClip placementFx;
    [SerializeField] private AudioClip growFx;
    [SerializeField] private AudioClip perfectFx;
    [SerializeField] private AudioClip impactFx;
    // ===============
    public GameObject markers;
    public MasterScript masterScript;

    public ParticleSystem particleSystem;
    public ParticleSystem specialParticleSystem;
    public Vector3 lastPos;
    public Vector3 lastScale;
    private float time = 0;
    [SerializeField] public float speed;
    private bool goingToEnd = true;
    float growSpeed = 1f;
    const float GROWTH_VALUE = .35f;
    const int MAX_TO_GROW = 1;
    const float DURATION_OF_GROW = .65f;
    public bool moving = true;
    public bool onX;
    const float MARGIN = 0.155f;
    public Color color;

    public bool isSpecial = false;

    private Vector3 localScale;
    private Vector3 position;
    [SerializeField] private InputActionReference place;
    void Start()
    {
        localScale = transform.localScale;
        position = transform.position;
        gameObject.GetComponent<Renderer>().material.color = color;
    }
    void OnEnable()
    {
        //CoyoteJump type of function 
        StartCoroutine(BufferToAllowInput(.2f));
    }
    void OnDisable()
    {
        place.action.started -= placeAction;
    }

    bool isClickingUI()
    {
        if (Touchscreen.current == null) return false;
        Vector2 min = new Vector2(50, 1000);
        Vector2 max = new Vector2(200, 3000);
        Vector2 pointer = Touchscreen.current.primaryTouch.position.ReadValue();

        return
             pointer.x >= min.x &&
             pointer.x <= max.x &&
             pointer.y >= min.y &&
             pointer.y <= max.y;
    }
    private void placeAction(InputAction.CallbackContext obj)
    {
        if (!isClickingUI())
        {
            checkPlacement();
        }
    }

    void Update()
    {
        loop();
    }

    void loop()
    {
        if (moving)
        {
            time += Time.deltaTime * speed;
            if (time < 1 && moving)
            {
                transform.position = Vector3.Lerp(
                    goingToEnd ? markers.transform.GetChild(1).transform.position : markers.transform.GetChild(0).transform.position,
                    goingToEnd ? markers.transform.GetChild(0).transform.position : markers.transform.GetChild(1).transform.position,
                    time
                );
            }
            else
            {
                goingToEnd = !goingToEnd;
                time = 0;
            }
        }
    }
    void gameOver()
    {
        var rb = gameObject.AddComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(0, -2, 0);
        masterScript.gameOver();
    }

    IEnumerator BufferToAllowInput(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        place.action.started += placeAction;
    }

    IEnumerator GrowCubeRoutine(Vector3 targetScale, Vector3 targetPos, float duration)
    {
        yield return new WaitForEndOfFrame();

        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration * growSpeed;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            yield return null;
        }
    }

    void checkPlacement()
    {
        moving = false;
        float delta = onX ? transform.position.x - lastPos.x : transform.position.z - lastPos.z;
        if (onX ? Mathf.Abs(delta) > lastScale.x : Mathf.Abs(delta) > lastScale.z)
        {
            gameOver();
            return;
        }
        if (Mathf.Abs(delta) < MARGIN)
        {
            onPerfect();
        }
        else
        {
            onCut(delta);
            masterScript.next(transform.localScale, transform.position, false, isSpecial);
        }
        this.enabled = false;
    }

    void onPerfect()
    {
        if (isSpecial)
        {
            specialParticleSystem.Play();
        }
        else
        {
            particleSystem.startSize = particleSystem.startSize + (transform.localScale.x * .5f);
            particleSystem.Play();
        }
        SoundMaster.instance.playOnPerfectAudio(perfectFx, transform, masterScript.perfectCounter);
        transform.position = new Vector3(lastPos.x, transform.position.y, lastPos.z);
        masterScript.perfectCounter += 1;
        if (masterScript.perfectCounter > MAX_TO_GROW)
        {
            SoundMaster.instance.playAudio(growFx, transform);
            var targetScale = onX ? transform.localScale + new Vector3(GROWTH_VALUE, 0, GROWTH_VALUE) : transform.localScale + new Vector3(GROWTH_VALUE, 0, GROWTH_VALUE);
            Vector3 growth = targetScale - transform.localScale;
            float offsetX = growth.x * 0.5f;
            float offsetZ = growth.z * 0.5f;
            var targetPos = new Vector3(transform.localPosition.x + offsetX, transform.position.y, transform.localPosition.z + offsetZ);
            StartCoroutine(GrowCubeRoutine(targetScale, targetPos, DURATION_OF_GROW));
            masterScript.next(targetScale, targetPos, true, isSpecial);
        }
        else
        {
            masterScript.next(transform.localScale, transform.position, true, isSpecial);
        }
    }

    void calculateNewSize(float delta)
    {
        float overlap = onX ? transform.localScale.x - Mathf.Abs(delta) : transform.localScale.z - Mathf.Abs(delta);
        transform.localScale = onX ?
        new Vector3(
            Mathf.Abs(overlap),
            transform.localScale.y,
            transform.localScale.z
        )
        :
        new Vector3(
            transform.localScale.x,
            transform.localScale.y,
            Mathf.Abs(overlap)
        );
    }

    void alignPos(float delta)
    {
        transform.position = onX ?
        new Vector3(
            lastPos.x + delta / 2,
            transform.position.y,
            lastPos.z
        )
        :
        new Vector3(
            lastPos.x,
            transform.position.y,
            lastPos.z + delta / 2
        );
    }


    void onCut(float delta)
    {
        masterScript.perfectCounter = 0;
        SoundMaster.instance.playAudio(placementFx, transform);
        calculateNewSize(delta);
        alignPos(delta);
        spawnExtra(delta);
    }

    void spawnExtra(float delta)
    {
        Vector3 spawnExcessSize = onX ?
            new Vector3(Mathf.Abs(delta), transform.localScale.y, transform.localScale.z)
            : new Vector3(transform.localScale.x, transform.localScale.y, Mathf.Abs(delta));
        Vector3 spawnExcessPos = onX ?
            new Vector3(transform.position.x + (transform.localScale.x / 2 + Math.Abs(delta) / 2) * Math.Sign(delta), transform.position.y, transform.position.z)
            :
            new Vector3(transform.position.x, transform.position.y, transform.position.z + (transform.localScale.z / 2 + Math.Abs(delta) / 2) * Math.Sign(delta));
        var extra = GameObject.CreatePrimitive(PrimitiveType.Cube);
        extra.transform.localScale = spawnExcessSize;
        extra.transform.localPosition = spawnExcessPos;
        Material mat = new Material(GetComponent<Renderer>().material);
        mat.color = color;
        extra.GetComponent<Renderer>().material = mat;
        applyForceToExtra(extra.AddComponent<Rigidbody>(), spawnExcessSize, spawnExcessPos, delta);
        Destroy(extra, 5f);
    }


    public void applyForceToExtra(Rigidbody extra, Vector3 size, Vector3 pos, float delta)
    {
        const float AMOUNT_OF_FORCE = 1f;
        if (onX)
        {
            extra.GetComponent<Rigidbody>()
                .AddForceAtPosition(
                    new Vector3(size.x + AMOUNT_OF_FORCE, 0, 0),
                    new Vector3(0, pos.y * Math.Sign(delta), 0)
                );
        }
        else
        {
            extra.GetComponent<Rigidbody>()
                .AddForceAtPosition(
                    new Vector3(0, 0, size.z * AMOUNT_OF_FORCE),
                    new Vector3(0, pos.y, 0)
                );
        }
    }
}
