# Blackboard

A simple key-value storage system for Unity, designed for AI and game state management.

## What is this?

Blackboard provides a flexible way to store and share data between different parts of your game. Think of it as a shared memory system where components can read, write, and observe changes to named values.

Common use cases include:
- AI decision making (storing perceived threats, goals, etc.)
- Behavior trees (sharing context between nodes)
- Game state management (player stats, world state)
- Event-driven systems (observe value changes)

## Features

- Store primitive types (int, float, bool, string) and Unity types (Vector3, GameObject, Transform)
- List support for all types
- Parent blackboard inheritance for hierarchical lookups
- Change notifications via Subscribe/Unsubscribe
- Custom Unity inspector with full undo/redo support
- Runtime debug window for inspecting and editing values during play mode

## Installation

1. Open Unity Package Manager (Window > Package Manager)
2. Click the **+** button in the top-left corner
3. Select **Add package from git URL**
4. Enter: `https://github.com/AI-In-Games/Blackboard.git?path=/UnityPackage`
5. Click **Add**

## Basic Usage

```csharp
using AiInGames.Blackboard;

// Create a blackboard asset in Unity (Create > AI > Blackboard)
public Blackboard blackboard;

// Set values
blackboard.SetValue("Health", 100);
blackboard.SetValue("IsAlive", true);

// Get values
int health = blackboard.GetValue<int>("Health");
bool isAlive = blackboard.GetValue<bool>("IsAlive");

// Subscribe to changes
blackboard.Subscribe("Health", () =>
{
    int health = blackboard.GetValue<int>("Health");
    Debug.Log($"Health changed to: {health}");
});
```

## Runtime Debugging

Open the debug window programmatically to inspect and modify values during play mode:

```csharp
using AiInGames.Blackboard.Editor;

// Open debug window for a specific blackboard
BlackboardDebugWindow.ShowWindow(myBlackboard);
```

## Documentation

- [Changelog](UnityPackage/CHANGELOG.md) - Version history and changes
- [Contributing](CONTRIBUTING.md) - How to contribute

## License

This project is licensed under the MIT License.
