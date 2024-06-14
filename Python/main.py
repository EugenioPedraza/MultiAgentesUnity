import os
import numpy as np
import matplotlib.pyplot as plt
import random
import json

class Car:
    def __init__(self, id, pos, speed, direction, color, turn_right):
        self.id = id
        self.pos = pos.astype(float)
        self.speed = speed
        self.direction = direction
        self.stopped = False
        self.passed_light = False
        self.turn_right = turn_right and np.array_equal(direction, np.array([0, 1]))  # Only allow right turns for south to north
        self.initial_direction = direction.copy()
        self.color = color

    def update(self, traffic_light, cars):
        # Check if the car has passed the traffic light
        if not self.passed_light:
            # Check if there is a car in front
            for car in cars:
                if car != self and np.array_equal(car.direction, self.direction):
                    dist = np.linalg.norm(self.pos - car.pos)
                    if 0 < dist < 2 and np.dot(car.pos - self.pos, self.direction) > 0:
                        self.stopped = True
                        break

            if traffic_light.state == "red" and -2 <= np.dot(self.pos, self.direction) <= 0:
                self.stopped = True
            elif traffic_light.state == "green" and self.stopped:
                self.stopped = False

            # Check if the car has passed the traffic light
            if np.dot(self.pos, self.direction) > 0:
                self.passed_light = True

        if not self.stopped:
            self.pos += self.speed * self.direction

        # Make a right turn if the car has decided to turn right and is at the appropriate position
        if self.passed_light and self.turn_right:
            if np.array_equal(self.initial_direction, np.array([0, 1])) and self.pos[0] >= 0:  # South to North
                self.direction = np.array([1, 0])  # Turn East

    def get_state(self):
        return {
            'id': self.id,
            'pos': self.pos.tolist(),
            'direction': self.direction.tolist(),
            'stopped': self.stopped,
            'passed_light': self.passed_light,
            'turn_right': self.turn_right,
            'color': self.color
        }

    def draw(self):
        plt.plot(self.pos[0], self.pos[1], self.color, markersize=8)

class TrafficLight:
    def __init__(self, id, state, green_duration, yellow_duration, red_duration, direction):
        self.id = id
        self.state = state
        self.green_duration = green_duration
        self.yellow_duration = yellow_duration
        self.red_duration = red_duration
        self.timer = 0
        self.direction = direction

    def update(self):
        self.timer += 1
        if self.state == "green" and self.timer >= self.green_duration:
            self.state = "yellow"
            self.timer = 0
        elif self.state == "yellow" and self.timer >= self.yellow_duration:
            self.state = "red"
            self.timer = 0
        elif self.state == "red" and self.timer >= self.red_duration:
            self.state = "green"
            self.timer = 0

    def get_state(self):
        return {
            'id': self.id,
            'state': self.state,
            'timer': self.timer,
            'direction': self.direction.tolist()
        }

    def draw(self, position):
        if self.state == "red":
            color = 'ro'
        elif self.state == "yellow":
            color = 'yo'
        else:
            color = 'go'
        plt.plot(position[0], position[1], color, markersize=12)

def spawn_car(cars, direction, car_id, spawn_probability=0.5):
    if random.random() > spawn_probability:
        return  # Do not spawn a car based on probability
    
    if np.array_equal(direction, np.array([-1, 0])):  # East to West
        pos = np.array([14, 1])
        color = 'bo'
    elif np.array_equal(direction, np.array([0, 1])):  # South to North
        pos = np.array([0, -14])
        color = 'go'
    elif np.array_equal(direction, np.array([1, 0])):  # West to East
        pos = np.array([-14, 0])
        color = 'ro'
    else:  # North to South
        pos = np.array([-1, 14])
        color = 'mo'

    turn_right = random.random() < 0.5 if np.array_equal(direction, np.array([0, 1])) else False  # 50% chance to turn right only for south to north
    car = Car(car_id, pos, 0.2, direction, color, turn_right)
    cars.append(car)

def simulate_intersection(sim_steps, output_directory):
    cars = []
    car_id = 1

    traffic_lights = [
        TrafficLight(1, "green", green_duration=50, yellow_duration=10, red_duration=100, direction=np.array([1, 0])),   # East
        TrafficLight(2, "red", green_duration=50, yellow_duration=10, red_duration=100, direction=np.array([0, 1])),     # South
        TrafficLight(3, "red", green_duration=50, yellow_duration=10, red_duration=100, direction=np.array([-1, 0])),    # West
        TrafficLight(4, "red", green_duration=50, yellow_duration=10, red_duration=100, direction=np.array([0, -1]))     # North
    ]

    current_green_light = 0
    spawn_interval = 10
    spawn_timer = 0

    # List to hold the state of the simulation at each step
    simulation_data = []

    for step in range(sim_steps):
        plt.clf()
        plt.xlim(-15, 15)
        plt.ylim(-15, 15)
        plt.axhline(0, color='black', linewidth=2)
        plt.axvline(0, color='black', linewidth=2)

        # Aparecer carros en cierto intervalo
        spawn_timer += 1
        if spawn_timer >= spawn_interval:
            spawn_timer = 0
            for direction in [np.array([-1, 0]), np.array([0, 1]), np.array([1, 0]), np.array([0, -1])]:
                spawn_car(cars, direction, car_id, spawn_probability=0.2)  # Adjust the spawn probability here
                car_id += 1

        # Actualizar carros
        for car in cars:
            car.update(traffic_lights[np.where(np.all(np.array([light.direction for light in traffic_lights]) == car.direction, axis=1))[0][0]], cars)
            car.draw()

        # Actualizar luces de trafico
        traffic_lights[current_green_light].update()
        if traffic_lights[current_green_light].state == "red":
            current_green_light = (current_green_light + 1) % len(traffic_lights)
            traffic_lights[current_green_light].state = "green"

        # Draw traffic lights
        for light in traffic_lights:
            light.draw(np.array([np.cos(np.radians((light.id - 1) * 90)), np.sin(np.radians((light.id - 1) * 90))]))

        # Record the state at this step
        step_data = {
            'step': step,
            'cars': [car.get_state() for car in cars],
            'traffic_lights': [light.get_state() for light in traffic_lights]
        }
        simulation_data.append(step_data)

        plt.title(f"Simulation Step: {step}")
        plt.grid(True)
        plt.pause(0.001)


    if not os.path.exists(output_directory):
        os.makedirs(output_directory)

    output_file = os.path.join(output_directory, 'simulation_data.json')
    with open(output_file, 'w') as f:
        json.dump(simulation_data, f, indent=4)

    plt.show()


sim_steps = 1000
output_directory = '../Assets/Resources' 
simulate_intersection(sim_steps, output_directory)