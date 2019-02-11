using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapTouchMover : MonoBehaviour
{
    private static Vector3 REFERENCE;

    private Vector3 calcVec = Vector3.zero;

    public Camera camera;

    private float camHeight;
    private float camWidth;
    private Vector2 deltaDrag;
    public MapLoader mapLoader;


    public GameObject mapPlane;
    private Vector3 movement;
    private float planeHeight;
    private float planeWidth;
    private Vector3 prevPos;

    private Touch touch;

    public bool touchFreeze;
    private Vector3 worldDragDelta;
    private Vector3 worldDragEnd;

    private Vector3 worldDragStart;

    private void Awake()
    {
    }

    // Use this for initialization
    private void Start()
    {
        camHeight = 2f * camera.orthographicSize;
        camWidth = camHeight * camera.aspect;
        //http://answers.unity3d.com/questions/230190/how-to-get-the-width-and-height-of-a-orthographic.html
        Debug.LogFormat("width={0}, height={1}, rect={2}", camWidth, camHeight, camera.rect);

        Debug.LogFormat("aabb:{0}", mapPlane.GetComponent<MeshCollider>().bounds);
        var bounds = mapPlane.GetComponent<MeshCollider>().bounds;

        planeHeight = bounds.size.z;
        planeWidth = bounds.size.x;

        REFERENCE.x = Math.Abs(camWidth / 2f - planeWidth / 2f);
        REFERENCE.z = Math.Abs(camHeight / 2f - planeHeight / 2f);

        Debug.LogFormat("Refernece={0}", REFERENCE);

        prevPos = Vector3.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        if (touchFreeze)
            return;
        if (EventSystem.current.currentSelectedGameObject != null)
            return;


        // Single/Double detection by: https://forum.unity3d.com/threads/single-tap-double-tap-script.83794/#post-1713096
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                //deltaDrag = Vector2.zero;

                worldDragDelta = Vector3.zero;

                worldDragStart = ConvertTouchToWorld(touch.position);
                Debug.LogFormat("touch={0}, worldDragStart={1}", touch.position, worldDragStart);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                worldDragEnd = ConvertTouchToWorld(touch.position);

                worldDragDelta = worldDragEnd - worldDragStart; // Inverted, since plane is 180deg rotated
                worldDragDelta *= -1;
                //deltaDrag = touch.deltaPosition;


                prevPos.Set(transform.position.x, transform.position.y, transform.position.z);

                movement = Clamp(worldDragDelta + prevPos);

                mapPlane.transform.Translate(movement - prevPos);


                Debug.LogFormat("Old={0},new={1}, worldDrag={2}, movment={3}", prevPos, transform.position,
                    worldDragDelta, movement);

                if (movement != worldDragDelta + prevPos)
                {
                    // Clamped
                    Debug.Log("Reloading");
                    var deltaLon =
                        transform.position.x * MapLoader.Y_SCALE * 0.5f * -1f; // -1, flipped plane, 0.5f cause reasons
                    Debug.LogFormat("Pos={0}", transform.position);
                    var deltaLat = transform.position.z * MapLoader.X_SCALE * 0.5f * -1f;

                    mapLoader.ReloadAndCenter(deltaLat / 100f, deltaLon / 100f); // 100f zoom factor, related to scale
                    touchFreeze = true;
                }

                //Debug.LogFormat("Pos Clamped={0}", transform.position);

                worldDragStart = ConvertTouchToWorld(touch.position);

                CloseToBorder(transform.position);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                worldDragEnd = ConvertTouchToWorld(touch.position);

//                deltaDrag = Vector2.zero;
                worldDragDelta = Vector3.zero;
            }
        }
    }

    private Vector3 ConvertTouchToWorld(Vector2 touch)
    {
        calcVec.Set(touch.x, touch.y, 0);
        return camera.ScreenToWorldPoint(calcVec);
    }

    private bool CloseToBorder(Vector3 position)
    {
        /*
         * moving finger
         *  up: -z
         *  down: +z
         *  right: -x
         *  left: +y
         */

        if (position.z <= camHeight / 2f * -1)
            Debug.Log("-z border reached");
        if (position.z >= camHeight / 2f)
            Debug.Log("z border reached");
        return false;
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        return Vector3.ClampMagnitude(position, REFERENCE.magnitude);
        /*
        position.x = position.x >= 3.9f ? 3.9f : position.x;
        position.x = position.x <= -3.9f ? -3.9f : position.x;

        position.z = position.z >= 3.1f ? 3.1f : position.z;
        position.z = position.z <= -3.1f ? -3.1f : position.z;
        */
    }


    private Vector3 Clamp(Vector3 vec)
    {
        float x = 0;
        float y = 0;
        float z = 0;


        if (Math.Abs(vec.x) > REFERENCE.x)
            x = REFERENCE.x * (vec.x < 0 ? -1f : 1f);
        else
            x = vec.x;

        if (Math.Abs(vec.z) > REFERENCE.z)
            z = REFERENCE.z * (vec.z < 0 ? -1f : 1f);
        else
            z = vec.z;
        var ret = new Vector3(x, y, z);
        Debug.LogFormat(":Clamp in={0}, ref={1}, out={2}", vec, REFERENCE, ret);
        return ret;
    }
}