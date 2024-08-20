using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Camera cam;
    [SerializeField] LayerMask layer;
    [SerializeField] GameManager gameManager;
    public static bool touchCheck = true;
    void Start()
    {
        cam = Camera.main;
    }
    void Update()
    {
        PlacePawnWithRay();
    }
    private void PlacePawnWithRay()
    {

        if (touchCheck)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    Ray ray = cam.ScreenPointToRay(touch.position);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer))
                    {
                        if (hit.collider != null)
                        {
                            gameManager.PlacePlayerPawn(hit.collider.GetComponentInParent<GridCell>());
                            gameManager.CalculateDistancePercentage(gameManager.targetGrid, hit.collider.GetComponentInParent<GridCell>());

                            touch.phase = TouchPhase.Ended;
                        }
                    }
                }
            }
        }
    }
}
