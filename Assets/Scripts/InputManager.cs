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
                // Ýlk dokunma olayýný al

                Touch touch = Input.GetTouch(0);

                // Dokunma baþladýysa
                if (touch.phase == TouchPhase.Began)
                {

                    Ray ray = cam.ScreenPointToRay(touch.position);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer))
                    {
                        if (hit.collider != null)
                        {
                            gameManager.PlacePawn(hit.collider.GetComponentInParent<GridCell>());
                            gameManager.CalculateColorSimilarity(gameManager.targetGrid.GridCellColor, gameManager.selectedGrid.GridCellColor);
                            touch.phase = TouchPhase.Ended;
                        }
                    }
                }
            }
        }
    }
}
