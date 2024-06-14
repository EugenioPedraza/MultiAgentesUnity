using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    public GameObject redLight;
    public GameObject yellowLight;
    public GameObject greenLight;

    private Renderer redRenderer;
    private Renderer yellowRenderer;
    private Renderer greenRenderer;

    private Color redOn = new Color(1f, 0f, 0f, 1f); // Standard red
    private Color yellowOn = new Color(1f, 1f, 0f, 1f); // Standard yellow
    private Color greenOn = new Color(0f, 1f, 0f, 1f); // Standard green
    private Color offColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Gray for off state

    private string currentState = "red"; // Initial state

    private void Start()
    {
        redRenderer = redLight.GetComponent<Renderer>();
        yellowRenderer = yellowLight.GetComponent<Renderer>();
        greenRenderer = greenLight.GetComponent<Renderer>();

        // Check if renderers are properly assigned
        if (redRenderer == null || yellowRenderer == null || greenRenderer == null)
        {
            Debug.LogError("One or more Renderer components are not assigned properly.");
            return;
        }

        UpdateTrafficLightState(currentState); // Set initial state
    }

    private void SetLightColors(Color redColor, Color yellowColor, Color greenColor)
    {
        if (redRenderer != null)
        {
            redRenderer.material.color = redColor;
        }
        if (yellowRenderer != null)
        {
            yellowRenderer.material.color = yellowColor;
        }
        if (greenRenderer != null)
        {
            greenRenderer.material.color = greenColor;
        }
    }

    public string GetCurrentState()
    {
        return currentState;
    }

    public void UpdateTrafficLightState(string state)
    {
        currentState = state;
        switch (state)
        {
            case "red":
                SetLightColors(redOn, offColor, offColor);
                break;
            case "yellow":
                SetLightColors(offColor, yellowOn, offColor);
                break;
            case "green":
                SetLightColors(offColor, offColor, greenOn);
                break;
        }
    }
}
