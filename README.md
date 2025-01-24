# Ares Assignment Post-Mortem

## Overview of Project

I built a fairly simple “space” simulation that uses Unity DOTS and Havok Physics. I saw the potential to need to simulate a large number of objects after reading the simulation requirements and this seemed like a good solution and opportunity to learn something new.

The Simulation runs on mobile and PC with various options for controls. The user can toggle on/off spawning of each entity type as well as controlling the rate at which the entities spawn. They can also pause or clear the simulation at any time, adjust the simulation size which changes the boundaries of the simulation to allow the user to simulate more objects, and change the initial velocity given to the entities on spawn.

PC/Mobile: mouse/touch and drag to move the camera

PC: Scroll zoom camera

Mobile: Slider zoom camera

The number of objects able to be simulated depends on the size of the boundaries and the initial velocity (faster objects more collision, more stress on physics engine) 

I was comfortably simulating 10,000 objects on small world and over 50,000 on large world on my PC.

The bottle neck at those numbers was the Havok physics engine not my own coded systems.

### Note: The simulation will still grind to a halt, for me this will happen around 20,000 of the same object in the small simulation. I have implemented code to prevent the scene from going under 10 fps, but it could still crash in a runaway collision scenario.

The low fps at high entities is due to the physics engine, not my own coded systems (if you check profiler)

Upon checking the assignment again, I realized I did not have my asteroids spawning at the collision location, instead spawning randomly in the world. I added an option to have the asteroids spawn at the collision location but there is no system to prevent that object from instantly colliding with the ones that spawned it which instantly results in a runaway collision of infinite spawns. 

This option is disabled by default, meaning collision spawns happen randomly inside of the simulation world.

---

## Process Overview

I initially had some ideas to gamify the simulation but they all somewhat fell flat when required to strictly adhere to the assignment requirements. I did partially implement a “gravity source” that you could place that will pull in the floating asteroids but did not leave it in the final build.

I ended up focusing on creating a robust and performant and large simulation that strictly adhered to the assignment’s requirements. I also wanted to get it running on mobile so I spent some time on getting a passable mobile interface and some touch controls working.

## Largest Hurdles

### Collision

I went back and forth when trying to get collision to work between my entities. Something trivial with GameObjects was something I had to spend quite a bit of time on. 

This was all because the Unity Physics package uses stateless collision. I began to implement my own collision system before decided I did not want to write all my own physics engine, I have done this work before for agent simulation. I was able to find an extension for the Unity Physics package on Unity’s github that provided a solution for stateful collision detection for my Entities.

### Profiler

I had a bunch of issues with my profiler. One day it just stopped working no matter what I did. It also seemed that the mere act of attaching the profiler to my simulation cut my FPS in half. 

At first I saw that it was showing massive frame time spikes regularly. Looking into this it appeared to by Screen Space Ambient Occlusion which was off as far as I could tell. Removing post processing from my scene however fixed this.

I then continued to see some frame time spikes in the profiler but as far as I could tell these were being caused by calls to the profiler itself.

Using the profiler did allow me to find that, even though I was using the entity command buffer to write structural changes, I wasn’t implementing any smoothing, so I was still getting some frame rate spikes.

## Performance

Not something I have really ever needed to think about before, I spent quite a long time on tweaking and adjusting the simulation to increase performance and smooth fps spikes.

On that note I implemented the following:

- Queries for deletion happen multithreaded and store the entities in a `NativeQueue` The deletion is then done on the main thread, deleting either a maximum of 10% of the queue each frame.
    - This system is able to instantly clear the simulation of over 40,000 entities smoothly with no frame loss
- Spawning happens the same way, but all on the main thread. Separate systems check for collisions and handle collisions, the spawner system looks for collided objects and, along with new objects from the spawners themselves, adds these objects to a `NativeQueue` and spawns them 10% a frame.

Further work could be done to make this even better. There is opportunity to multithread more of this by moving work off the main thread into jobs.

## File Overview

**Entity Systems**

- ClearSimulationSystem - allows the User to mark all entities for deletion at once
- HandleCollisionSystem - provides access to stateful collision via buffers
- HandleObjectSystem - keeps the entities in bounds by wrapping them around if they leave the simulation area
- SpawnerSystem - handles spawning objects that were marked after a collision or from the 4 spawners in the scene
- HandleCollisionSystem - checks objects’ stateful collision buffers to mark objects that have collided for deletion or replication
- CleanupSystem - checks objects marked with `Annihilate` or `RequestDuplication` to either delete them, or pass them to the SpawnerSystem to replicate
- CreateSingletonSystem - workaround for my Authoring components in my subscene not appearing in my build, this manually creates the 3 data singletons I am using.

**MonoBehaviour**

- SimulationController - handles user UI input and reading/writing data to the ECS components
- CameraController - basic camera controls for mobile and pc
- MenuToggle - simple script to move the menu for mobile written by chatGPT

### Networking

**Controls**

Due to my recent experience with Mirror, I would likely begin here unless directed otherwise. I would implement simple syncvars for each of the spawners, or implement a simple message handling system. 

In my previous card game I was using Mirror to send card update messages to sync states across game clients that contained small amounts of byte data. 

I could easily replicate this message system to handle player controls.

**Simulation**

Due to the size of my simulation, and the fact that Havok is not a deterministic physics engine and uses floating point numbers, I have come up with the following hypothetical solution to keep the simulation consistent.

Initial spawning would need to be controlled by a host or server - I would imagine a simple host/client p2p setup would be sufficient. The host would relay spawn information to clients. Each client would run the simulation or potentially just a lighter weight interpolation or prediction model. The host would then update each of the clients periodically with corrected simulation information.

I would think to either do this with a % of the entities per frame or spacing out the updates.

## Final Thoughts

I am a bit disappointed I didn’t end up with a more gamified approach. As I stated before, I had some thoughts about this and did start off wanting to build a game involving gravity and black holes and flinging them across the screen with your finger trying to collect certain types of objects or something. However, I ended up deciding to stick to the project specifications rigidly. 

I thought this was a great opportunity to use DOTS since I had never used it before, but was told about it by a professor recently. This choice definitely made trivial things, such as writing a `OnCollisionEnter` function take quite a bit of effort to figure out, but I will walk away with some more Unity knowledge and another tool under my belt. 

It was a fun project, and gave me the opportunity do some work I have never done before and learn some new tools along the way.