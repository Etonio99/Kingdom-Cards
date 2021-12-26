using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public bool canPan = false;
    [HideInInspector]
    public bool finPanEffect = true;
    private Vector2 panMoveLoc;
    public bool followPlayer = true;
    public float panSpeed = 0.1f;
    public Vector2 clamps = new Vector2(5f, 10f);

    void Update ()
    {
        if (canPan && finPanEffect)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector2 _touchDeltaPosition = Input.GetTouch(0).deltaPosition;
                transform.Translate(-_touchDeltaPosition.x * panSpeed * Time.deltaTime, -_touchDeltaPosition.y * panSpeed * Time.deltaTime, 0f);
            }
        }
        else if (canPan && !finPanEffect)
        {
            StartCoroutine(PanEffect());
            canPan = false;
        }
        else if (followPlayer)
        {
            float _interpolation = 5f * Time.deltaTime;
            Vector3 _newPos = Vector3.Lerp(transform.position, new Vector3(BoardManagerScript.instance.player.position.x, BoardManagerScript.instance.player.position.y - 2f, -10f), _interpolation);
            _newPos.z = -10f;
            transform.position = _newPos;
        }

        if (!finPanEffect)
        {
            float _interpolation = 5f * Time.deltaTime;
            Vector3 _newPos = Vector3.Lerp(transform.position, panMoveLoc, _interpolation);
            transform.position = _newPos;
        }

        Vector3 _pos = transform.position;
        _pos.x = Mathf.Clamp(transform.position.x, -clamps.x, clamps.x);
        _pos.y = Mathf.Clamp(transform.position.y, -clamps.y, clamps.y);
        transform.position = _pos;
    }

    IEnumerator PanEffect ()
    {
        BoardManagerScript.instance.SetUsableUI(false);
        finPanEffect = false;
        panMoveLoc = new Vector3(transform.position.x, transform.position.y - 1f, -10f);
        yield return new WaitForSeconds(0.5f);
        BoardManagerScript.instance.SetUsableUI(true);
        finPanEffect = true;
        canPan = true;
    }
}