using UnityEngine;

public class CarController : MonoBehaviour
{
    public Vector3 direction;
    public bool stopped;
    public bool turnRight;
    private bool passedLight;
    private Vector3 initialDirection;
    private Vector3 carScale = new Vector3(15f, 15f, 15f); // Adjust the scale as needed

    void Start()
    {
        initialDirection = direction;
        passedLight = false;
        transform.localScale = carScale; // Set initial scale
    }

    void Update()
    {
        
    }

    public void UpdateCarState(Vector3 newPosition, Vector3 newDirection, bool isStopped, bool hasPassedLight, bool shouldTurnRight)
    {
        transform.position = newPosition;
        direction = newDirection;
        stopped = isStopped;
        passedLight = hasPassedLight;
        turnRight = shouldTurnRight;

        
        if (direction == Vector3.right)
        {
            transform.rotation = Quaternion.Euler(-100.7f, 104.19f, -14.3f); 
        }
        else if (direction == Vector3.forward)
        {
            transform.rotation = Quaternion.Euler(-97.68f, 2.35f, 0); 
        }
        else if (direction == Vector3.left)
        {
            transform.rotation = Quaternion.Euler(-96.07f, -94.81f, 5.92f); 
        }
        else if (direction == Vector3.back)
        {
            transform.rotation = Quaternion.Euler(-88.49f, 0.22f, -180.6f); 
        }

        transform.localScale = carScale; 
    }

    private void MakeRightTurn()
    {
        if (initialDirection == Vector3.forward && transform.position.z >= 0) // South to North
        {
            direction = Vector3.right; // Turn East
        }
    }

    public void CheckTrafficLight(TrafficLightController trafficLight)
    {
        if (!passedLight)
        {
            if (trafficLight.GetCurrentState() == "red" && Vector3.Dot(transform.position, direction) <= 1)
            {
                if (!stopped)
                {
                    Debug.Log($"Car {gameObject.name} is stopping at red light.");
                }
                stopped = true;
            }
            else if (trafficLight.GetCurrentState() == "green" && stopped)
            {
                Debug.Log($"Car {gameObject.name} is moving at green light.");
                stopped = false;
            }

            if (Vector3.Dot(transform.position, direction) > 1)
            {
                passedLight = true;
            }
        }
    }
}
