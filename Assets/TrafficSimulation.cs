using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;

public class TrafficSimulation : MonoBehaviour
{
    public GameObject carPrefab;
    public GameObject[] trafficLights; // Assign your empty GameObjects in the Unity inspector
    public TextAsset jsonData;
    private List<CarController> cars = new List<CarController>();
    private List<TrafficLightController> trafficLightControllers = new List<TrafficLightController>();

    public class CarData
    {
        public int id;
        public float[] pos;
        public int[] direction;
        public bool stopped;
        public bool passed_light;
        public bool turn_right;
        public string color;
    }

    public class TrafficLightData
    {
        public int id;
        public string state;
        public int timer;
        public int[] direction;
    }

    public class StepData
    {
        public int step;
        public List<CarData> cars;
        public List<TrafficLightData> traffic_lights;
    }

    private List<StepData> simulationData;
    private int currentStep = 0;

    void Start()
    {
        try
        {
            simulationData = JsonConvert.DeserializeObject<List<StepData>>(jsonData.text);
            InitializeTrafficLights();
            StartCoroutine(Simulate());
        }
        catch (JsonSerializationException e)
        {
            Debug.LogError("Error deserializing JSON: " + e.Message);
        }
    }

    IEnumerator Simulate()
    {
        while (currentStep < simulationData.Count)
        {
            UpdateSimulationStep(currentStep);
            currentStep++;
            yield return new WaitForSeconds(0.1f); // Adjust the wait time as needed
        }
    }

    void InitializeTrafficLights()
    {
        foreach (var lightObject in trafficLights)
        {
            TrafficLightController lightController = lightObject.GetComponent<TrafficLightController>();
            trafficLightControllers.Add(lightController);
        }
    }

    void UpdateSimulationStep(int step)
    {
        var stepData = simulationData[step];

        // Update traffic lights
        foreach (var lightData in stepData.traffic_lights)
        {
            TrafficLightController light = trafficLightControllers.Find(t => t.gameObject.name == "TrafficLight" + lightData.id);
            if (light != null)
            {
                light.UpdateTrafficLightState(lightData.state);
                Debug.Log($"TrafficLight {lightData.id} is {lightData.state} at step {step}.");
            }
        }

        // Update and spawn cars
        foreach (var carData in stepData.cars)
        {
            Vector3 position = new Vector3(carData.pos[0], 0, carData.pos[1]);
            Vector3 direction = new Vector3(carData.direction[0], 0, carData.direction[1]);

            CarController car = cars.Find(c => c.gameObject.name == "Car" + carData.id);
            if (car == null)
            {
                GameObject carObject = Instantiate(carPrefab, position, Quaternion.identity);
                carObject.name = "Car" + carData.id;
                car = carObject.GetComponent<CarController>();
                cars.Add(car);
                Debug.Log($"Car {carData.id} spawned at position {position} with direction {direction} at step {step}.");
            }

            car.UpdateCarState(position, direction, carData.stopped, carData.passed_light, carData.turn_right);

            // Check traffic light state for each car
            TrafficLightController trafficLight = GetRelevantTrafficLight(direction);
            if (trafficLight != null)
            {
                car.CheckTrafficLight(trafficLight);
            }
        }
    }

    TrafficLightController GetRelevantTrafficLight(Vector3 direction)
    {
        foreach (var lightController in trafficLightControllers)
        {
            if (Vector3.Dot(direction, new Vector3(lightController.transform.position.x, 0, lightController.transform.position.z)) > 0)
            {
                return lightController;
            }
        }
        return null;
    }
}
