# Blackboard

Key-value storage system for Unity.

## Usage

```csharp
using AiInGames.Blackboard;

// Create a blackboard asset (Create > AI > Blackboard)
public Blackboard blackboard;

// Set and get values
blackboard.SetValue("Health", 100);
int health = blackboard.GetValue<int>("Health");

// Subscribe to changes
blackboard.Subscribe("Health", () =>
{
    int health = blackboard.GetValue<int>("Health");
    Debug.Log($"Health: {health}");
});
```

## Supported Types

Primitives: int, float, bool, string
Unity: Vector3, GameObject, Transform
Collections: List<T> for all types

## Debug Window

```csharp
using AiInGames.Blackboard.Editor;

// Open debug window for a specific blackboard
BlackboardDebugWindow.ShowWindow(myBlackboard);
```

[Full documentation](https://github.com/AI-In-Games/Blackboard)
