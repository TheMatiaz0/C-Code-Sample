# Code Sample (03/2025)

This code sample is from [Telegraphist 1920: Beats of War](https://store.steampowered.com/app/2584510), a passion project I'm working on as **Lead Developer** alongside two other programmers since **April 2023**. 

This sample covers around **60%** of the game’s total codebase, with approx. **80-90%** authored by me. I’ve been deeply involved in architecture, gameplay, and system-level design, tackling optimization challenges and implementing core mechanics.

In addition to core development, I’ve been responsible for leading code reviews, ensuring that code contributions across the team meet high standards for readability, efficiency, and maintainability. The review process allowed me to refactor and improve shared components, catch potential bugs early, and align our coding practices across the team. This experience has deepened my understanding of large-scale codebase management and the intricacies of long-term maintainability.

Highlights:
- [Router](Runtime/Helpers/Router) - an abstract Stack navigation, inspired from web development, 
- [Provider](Runtime/Helpers/Provider) - Dependency Injection pattern, 
- [Lifecycle](Runtime/Lifecycle) - Singleton pattern, 
- [Events](Runtime/Events) - Observer pattern, 
- [Scene Management](Runtime/Helpers/SceneManagement) - custom-made implementation,
- [Level Store](Runtime/Helpers/LevelStore) - Facade pattern,
- [Timeline](Runtime/LevelEditor/Timeline) - Mediator pattern.

## Project tree

```bash
│   README.md
│
├───Editor                             # Custom Unity Editor tools to streamline development
│       MissingAssetsWindow.cs              # Editor tool to locate missing assets in project hierarchy
│
├───Runtime                            # Files used in the built game
│   ├───Events                             # Events based on the Observer pattern
│   │       ...
│   │
│   ├───FX                                 # Handles visual and audio effects
│   │       ...
│   │
│   ├───Gameplay                           # Core gameplay logic, including interactions and state management
│   │   │   ...
│   │   ├───QTE                            # System managing quick-time events (QTEs) for timed player inputs
│   │   │   ├───BrokenTelegraph                # A QTE content simulating a broken telegraph
│   │   │   ├───Frequency                      # A QTE content related to frequency changes
│   │   │   └───Sequence                       # A QTE logic involving sequences of multiple timed tiles
│   │   ├───Scoring                        # Manages player performance scoring logic
│   │   └───Tiles                          # Visual render for core game tiles
│   │
│   ├───Helpers                          # Helper functions and generic API utilities
│   │   ├───LevelStore                       # Interfaces for managing level storage, including player-made and built-in levels
│   │   ├───Provider                         # Implements dependency injection pattern across project
│   │   ├───Router                           # UI and scene routing stack, inspired by web router implementations
│   │   ├───SceneManagement                  # Custom scene loading and management utilities
│   │   └───Video                            # Manages video playback and subtitle display in scenes
│   │
│   ├───Input                             # Manages input system, handling different types of user input across gameplay and UI
│   │       ...
│   │
│   ├───LevelEditor                       # Tools and scripts for Level Editor
│   │   ├───Tiles                            # Editable tiles specifically for the Level Editor
│   │   └───Timeline                         # Manages the timeline for level editing
│   │       ├───Grid                            # Grid rendering for timeline structure
│   │       ├───Tiles                           # Timeline-specific tiles for arranging level elements
│   │       └───Tools                           # Interactive tools for selecting and modifying timeline elements
│   │
│   ├───Lifecycle                         # Singleton pattern implementations for managing object lifecycle control
│   │       ...
│   │
│   ├───Structures                        # Core data structures and enums used across game components
│   │       ...
│   │
│   └───UI                                # UI components, including menus, notifications, and interaction handlers
│       ├───Buttons                           # Button elements with various in-game functionalities
│       ├───Notifications                     # Notification UI elements for player feedback
│       ├───Pauseable                         # Interfaces for pause functionality
│       └───Skippable                         # Interfaces for skip functionality in cutscenes or timed interactions
│
└───Utils                               # Miscellaneous utility scripts
        ...
```