# Lighthouse Project Instructions

This document provides an overview of major scripts and instructions for editing dialogues.

## Project Root
`C:\Users\maxim\Desktop\UnityProj\Lighthouse\lighthouse`

## Major Scripts

### Core Architecture
- **GameState.cs** - Central state management system. Use `GameState.Set(key, value)` and `GameState.Get<T>(key, defaultValue)` for persistent game data.
- **MessageBus.cs** - Pub/sub event system for decoupled communication between components. Subscribe with `MessageBus.Instance.Subscribe(topic, callback)`.
- **Singleton.cs** - Base class for singleton MonoBehaviours (automatically handles instance management and DontDestroyOnLoad).
- **SaveManager.cs** - PlayerPrefs-based save/load system using JSON serialization.

### Managers
- **CutsceneManager.cs** - Handles cutscene playback with fade effects, animations, and skip support.
- **DialogueManager.cs** - Shows/hides dialogue boxes via MessageBus. Use `DialogueManager.ShowDialogue(dialogueObject)`.
- **TaskManager.cs** - Quest/task system with MessageBus integration for adding/completing tasks.
- **Navigation.cs** - Scene transitions with fade-to-black effects.
- **AudioManager.cs** - Camera-following audio singleton for BGM and SFX.

### Game Logic
- **InteractableObject.cs** - Base class for clickable objects with hover states and UnityEvents.
- **MiscObjectClick.cs** - Scene-specific interaction handling and dialogue triggering.
- **ChoiceManager.cs** - Multi-choice dialogue system with consequence handling.
- **LHMinigame.cs** - Lighthouse repair minigame logic.

### UI Components
- **UIDialogueBox.cs** - Dialogue UI component with text display and animation.
- **UITaskTracker.cs** - Task list UI display component.

## Editing Dialogues

### Location
All dialogue ScriptableObjects are located at:
```
Assets/Resources/ScriptableObjects/Dialogues/
```

### Dialogue Format
Dialogues are YAML files with the following structure:
```yaml
text:
  - Line 1 of dialogue
  - Line 2 of dialogue
  - Line 3 of dialogue
```

Each line (marked with `- `) represents a separate text box in the dialogue system.

### Quick Reference
To change dialogue text:
1. Navigate to `Assets/Resources/ScriptableObjects/Dialogues/`
2. Find the dialogue file (e.g., `kitchen/sleep_all_day.asset`)
3. Edit the lines under the `text:` section
4. Each `- ` entry is a new dialogue box
5. Use `##` for paragraph breaks within a line
6. Use `#` for inline pauses/line breaks

### Example Edit
Original:
```yaml
text:
  - Bad Camborne, sleeping all day.
  - No fish for you.
```

Changed to:
```yaml
text:
  - "He hasn't moved, how tired is he?"
  - He better wake up soon for his night shift.
```

### Finding Dialogue Text
All dialogue text is cataloged in:
```
DIALOGUES.txt (at project root)
```

This file contains all dialogue text organized by file path for easy searching.

## Important Notes

1. **Dialogues use YAML format** - Be careful with indentation and special characters
2. **Quotes in text** - Use double quotes if the line contains apostrophes or special characters
3. **Multi-line dialogues** - Each `- ` creates a new dialogue box that requires a click to advance
4. **MessageBus topics** - Common topics: `"PlayCutscene"`, `"ShowDialogue"`, `"AddTaskString"`, `"CompleteTask"`
5. **GameState keys** - Common keys: `"day"`, `"hungry"`, `"ate_breakfast"`, `"lighthouse_fixed"`, `"do_burial"`

## Scene-Specific Scripts Location
```
Assets/Scripts/sceneScripts/
```

Contains logic for specific scenes (Kitchen, Bedroom, Lighthouse, etc.)

## One-off Scripts Location
```
Assets/Scripts/oneoffs/
```

Contains single-purpose scripts that may be attached to specific GameObjects.

## Common Code Patterns

### Show a Dialogue
```csharp
DialogueManager.ShowDialogue(Resources.Load<Dialogue>("ScriptableObjects/Dialogues/kitchen/example"));
```

### Publish a MessageBus Event
```csharp
// Simple event
MessageBus.Instance.Publish("CompleteTask", "task_id");

// Event with callback (for cutscenes)
MessageBus.Instance.Publish("PlayCutscene", "CutsceneName", true, (Action)(() =>
{
    // Callback code here
}), true);
```

### Check GameState
```csharp
if (GameState.Get<bool>("hungry", false)) 
{
    // Player is hungry
}

// Set a value
GameState.Set("ate_breakfast", true);
```

### Subscribe to MessageBus
```csharp
private MessageBus.SubscriptionHandle handle;

void Start()
{
    handle = MessageBus.Instance.Subscribe("TopicName", OnMessageReceived, this);
}

void OnDestroy()
{
    handle?.Unsubscribe();
}
```

## Project Structure Overview

```
Assets/
├── Scripts/
│   ├── Core/              (GameState, MessageBus, Singleton)
│   ├── Managers/          (Dialogue, Task, Audio, Navigation)
│   ├── UI/               (UIDialogueBox, UITaskTracker)
│   ├── sceneScripts/     (Scene-specific logic)
│   └── oneoffs/          (Single-purpose scripts)
├── Resources/
│   ├── ScriptableObjects/
│   │   ├── Dialogues/    (All dialogue assets)
│   │   └── Tasks/        (All task assets)
│   └── Prefabs/          (UI prefabs, etc.)
├── Art/
│   ├── Backgrounds/      (Scene backgrounds)
│   └── UI/              (UI sprites)
└── Scenes/               (Unity scenes)
```
