# Floating Text Feature for Rope Breaks

## Overview
This feature adds visual feedback when the rope breaks during gameplay, showing floating text messages at specific locations. Two types of messages are shown:

1. "Rope Broke!" at the hook position when the rope breaks
2. "Stress Limit!" near the player position when stress causes the break

## Setup Instructions

### Automatic Setup (Recommended)
1. Open the Unity Editor
2. Go to Tools > Setup > Copy FloatingTextPrefab to Resources
3. Click "OK" when the success message appears

### Manual Setup
If the automatic setup doesn't work:
1. In the Unity Project window, find the `FloatingTextPrefab` in the `Assets/Prefabs` folder
2. Create a `Resources` folder under `Assets` if it doesn't already exist
3. Copy the `FloatingTextPrefab` from `Assets/Prefabs` to `Assets/Resources`
4. Ensure the prefab name remains "FloatingTextPrefab"

## How It Works
- When the rope breaks due to stress, the `RopeManager` calls `BreakRope()` which shows the "Stress Limit!" text
- The `PlayerController` handles the rope break with `OnRopeBreak()`, showing the "Rope Broke!" text
- Both use the `FloatingText` system to create animated text that moves upward and fades out

## Customization
You can customize the floating text appearance by:
1. Modifying the `FloatingTextPrefab` in the Resources folder
2. Changing color, font size, and animation parameters
3. Adjusting the messages in `PlayerController.cs` and `RopeManager.cs`

## Troubleshooting
If floating text doesn't appear:
- Check that the `FloatingTextPrefab` exists in the `Resources` folder
- Verify that the main canvas is properly set up in your scene
- Ensure the main camera is tagged as "MainCamera"
- Check console for any error messages related to FloatingText 