using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    Vector3 topOffset = new Vector3(0f, 8f, -6.5f);
    Vector3 midOffset = new Vector3(0f, 4.5f, -7f);
    Vector3 botOffset = new Vector3(0f, 2f, -4.8f);
    Vector3 curOffset, curOrigin;
    Vector2 bounds = new Vector2(2.5f,1f);
    float playerR = 0.2f;
    Quaternion topRot = Quaternion.Euler(45f, 0f, 0f);
    Quaternion midRot = Quaternion.Euler(22f, 0f, 0f);
    Quaternion botRot = Quaternion.Euler(10f, 0f, 0f);
    int state;//0 low, 1 mid, 2 top
    public Material[] curveMats;
    public AnimationCurve curve;
    bool moving = false;
    void Start(){
        curOffset = midOffset;
        curOrigin = player.position;
        state = 1;
    }
    void Update(){
        Vector3 playerOffset = player.position - curOrigin;//how far the player is from being centered
        Vector3 target = curOrigin;//the "origin" is where the player would be if the camera were centered on them
        target.y = player.position.y;
        //bounds check
        if (Mathf.Abs(playerOffset.x) + playerR > bounds.x)
            target.x += Mathf.Sign(playerOffset.x) * (Mathf.Abs(playerOffset.x) + playerR - bounds.x);
        if (Mathf.Abs(playerOffset.z) + playerR > bounds.y)
            target.z += Mathf.Sign(playerOffset.z) * (Mathf.Abs(playerOffset.z) + playerR - bounds.y);
        //move camera
        curOrigin = Vector3.Lerp(curOrigin, target, 0.1f);
        transform.position = curOrigin + curOffset;
        //view switch
        if(Input.GetKeyDown(KeyCode.E))
            ChangeView(true);
        else if (Input.GetKeyDown(KeyCode.Q))
            ChangeView(false);
    }

    void ChangeView(bool up){
        if ((state == 0 && !up ) || (state == 2 && up) || moving)
            return;//bound check
        state += up? 1: -1;
        switch(state){//start camera transition
            case 0:
                StartCoroutine(CameraShift(botOffset,botRot,95,1f));
                break;
            case 1:
                StartCoroutine(CameraShift(midOffset,midRot,90,1f));
                break;
            default:
                StartCoroutine(CameraShift(topOffset,topRot,60,1f));
                break;
        }
    }
    IEnumerator CameraShift(Vector3 offset, Quaternion rot, float amount, float moveTime){
        moving = true;
        float startTime = Time.time;
        float perc = 0;
        Vector3 startOffset = curOffset;
        Quaternion startRot = transform.rotation;
        float startAmount = curveMats[0].GetFloat("Vector1_A1626516");
        while(perc < 1){//lerp between positions, rotations, and shader values
            perc = (Time.time - startTime) / moveTime;
            curOffset = Vector3.Lerp(startOffset, offset, perc);
            transform.rotation = Quaternion.Lerp(startRot, rot, perc);
            foreach(Material mat in curveMats)
                mat.SetFloat("Vector1_A1626516", Mathf.Lerp(startAmount, amount, perc));
            yield return null;
        }
        //make sure it got to the destination
        curOffset = offset;
        transform.rotation = rot;
        foreach(Material mat in curveMats)
            mat.SetFloat("Vector1_A1626516", amount);
        moving = false;
    }
}
