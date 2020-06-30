using UnityEngine;

public class Move : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        Vector3 move = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * 
            new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        transform.position += move * .05f;
    }
}
