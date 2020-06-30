using UnityEngine;

public class Look : MonoBehaviour
{
    float xRotation = 0, yRotation = 0;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        xRotation += Input.GetAxis("Mouse X") * 5;
        yRotation -= Input.GetAxis("Mouse Y") * 5;
        yRotation = Mathf.Clamp(yRotation, -90, 90);
        transform.localRotation = Quaternion.Euler(yRotation, xRotation, 0);
    }
}
