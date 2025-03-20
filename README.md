# Gold Miner Game

A simple Gold Miner game implementation in Unity using basic shapes and mechanics.

## Setup Instructions

### 1. Create the Scene

1. Open Unity and create a new scene
2. Set up an orthographic camera

### 2. Create Prefabs

#### Hook Prefab
1. Create a new empty GameObject and name it "HookPrefab"
2. Add a Sphere mesh to it (scale it down to about 0.3)
3. Add the `Hook.cs` script to it
4. Set its tag to "Hook"
5. Add a Sphere Collider component and set "Is Trigger" to true
6. Drag it to the Prefabs folder to create a prefab

#### Gold Prefab
1. Create a new GameObject with a Sphere mesh and name it "GoldPrefab"
2. Scale it to about 1.0
3. Add a Sphere Collider component
4. Add the `Gold.cs` script to it
5. Drag it to the Prefabs folder to create a prefab

#### Rock Prefab
1. Create a new GameObject with a Cube mesh and name it "RockPrefab"
2. Scale it to about 1.0
3. Add a Box Collider component
4. Add the `Rock.cs` script to it
5. Drag it to the Prefabs folder to create a prefab

### 3. Create Game Objects

#### Player
1. Create a new empty GameObject and name it "Player"
2. Position it at the top center of the screen (e.g., 0, 4, 0)
3. Add a Cylinder mesh to represent the miner (scale it appropriately)
4. Add the `PlayerController.cs` script to it
5. Add a Line Renderer component for the rope
   - Set the Width to something small like 0.1
   - Set the Positions Size to 2
   - Set Material to a default line material
6. Assign the HookPrefab to the "Hook Prefab" field in the PlayerController component

#### Game Manager
1. Create a new empty GameObject and name it "GameManager"
2. Add the `GameManager.cs` script to it
3. Assign the GoldPrefab to the "Gold Prefab" field
4. Assign the RockPrefab to the "Rock Prefab" field
5. Adjust the spawn settings as needed

#### UI
1. Create a Canvas for the UI
2. Add a Text - TextMeshPro element for the score
3. Add a Text - TextMeshPro element for the time
4. Create a Game Over panel with a Text element (set it inactive initially)
5. Add the `UIManager.cs` script to the Canvas
6. Assign the UI elements to the appropriate fields in the UIManager component

### 4. Play the Game

1. Press Play to test the game
2. Left-click to deploy the hook when it's swinging
3. Collect gold and rocks to earn points
4. Try to get the highest score before time runs out

## Game Controls

- **Left Mouse Button**: Deploy the hook to grab objects

## Game Mechanics

- The hook swings back and forth automatically
- Left-click to deploy the hook
- The hook will grab the first collectible it touches
- Gold is worth more points but is heavier (pulls back slower)
- Rocks are worth fewer points but are lighter (pulls back faster)
- Larger objects are worth more points but are heavier
