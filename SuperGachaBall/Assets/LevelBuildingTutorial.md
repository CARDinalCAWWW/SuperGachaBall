SuperGachaBall - Level Creation Guide 🛠️
Welcome to the SuperGachaBall Level Editor Guide! This document deals with everything you need to know to create, build, and playtest your own custom levels.

1. Getting Started 💾
Step 1: Open the Project
Open the project in Unity (ensure you are on the correct branch).
Navigate to the Scenes folder (Assets/Scenes).
Step 2: Create Your Scene
Don't start from scratch! Use our template to make sure you have the Player, UI, and Camera setups ready to go.

Find the scene named "Test World" (or the current main level).
Press Ctrl + D to duplicate it.
Rename your new scene (e.g., Level_MyName).
Double-click to open it.
2. Building the Level (ProBuilder) 🧱
We use ProBuilder to create the map geometry. It's like modeling inside Unity.

Open ProBuilder: Go to Tools > ProBuilder > ProBuilder Window.
New Shape: Click the "New Shape" button (cube icon) in the ProBuilder window.
Draw/Resize:
Click and drag to shape your floor/ramp/wall.
Use the Face Selection tool (top of editor view) to grab faces and extend them.
Colliders:
ProBuilder objects automatically have Mesh Colliders.
Important: If you make a "moving platform" or something complex, ensure the collider is set up correctly. For static floors, the default is fine.
💡 Design Tips
Ramps: You can select an edge and drag it down to make a ramp.
Colors: You can vertex paint or drag materials onto specific faces to color-code the "track" vs "scenery."
3. Adding Gameplay Elements 🎮
A. The Player (Start Point)
Find the object named "Player" (the ball) in the Hierarchy.
Move it to where you want the level to start.
Note: The camera will automatically follow it.
B. The Kill Floor (Death Logic)
Click on the Player object.
Find the RespawnSystem component in the Inspector.
Respawn Height: Default is -35. Any ball that falls below Y = -35 will die and respawn.
If you build a very vertical level (falling down), lower this number (e.g., to -100).
C. Collectibles (Points & Juice) 💎
Use the Prefab!

Go to your Project Folder (where the assets are).
Find the "Collectible" prefab (blue cube icon).
Drag and drop it into your scene as many times as you want!
Customizing:
You can select a specific collectible and change its Score Popup Text (e.g., make a special one say "+500").
You can change the color/material if you want variety.
D. The Goal (Winning) 🏁
Use the Prefab!

Find the "Goal" prefab in your Project Folder.
Drag it into your scene at the end of the track.
Resize it: You can use the Scale tool (R key) to make the goal zone bigger or smaller.
Settings (Optional):
Finish Slow Mo Scale: Adjust how much time slows down on win.
4. Testing Your Level ▶️
Press the Play button at the top.
Roll around!
WASD / Left Stick: Move
Mouse / Right Stick: Rotate Camera
Space / South Button: Jump
Click / West Button: Dash
Check the Console (Window > General > Console) for any red errors.
Checklist Before Publishing:
 Can you complete the level?
 Do all collectibles work?
 Does falling off the map kill you properly?
 Does the goal screen appear with your Rank?
Happy Building! 🏗️