using UnityEngine;

public class SpriteIcon : VisualOnlyFeature
{

  void Update()
  {
    // Get the current object's X and Z Euler angles
    float currentX = transform.eulerAngles.x;
    float currentZ = transform.eulerAngles.z;

    // Get the target object's Y Euler angle
    float targetY = Player.Instance.transform.eulerAngles.y + 180f;

    // Create a new Vector3 with the combined Euler angles
    Vector3 newEulerRotation = new Vector3(currentX, targetY, currentZ);

    // Assign the new rotation using Quaternion.Euler to convert the Vector3 back to a Quaternion
    transform.rotation = Quaternion.Euler(newEulerRotation);
  }
}