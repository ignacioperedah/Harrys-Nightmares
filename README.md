# Harry's Nightmares 🌙

![Unity](https://img.shields.io/badge/Unity-2020.3.49LTS-white?logo=unity)
![C#](https://img.shields.io/badge/Language-C%23-blue?logo=csharp)
![Status](https://img.shields.io/badge/Status-Refactoring-orange)

**Harry's Nightmares** is a 2D action-adventure game originally developed and published on the Google Play Store when I was 17 years old. 

Now, as a **4th-year Systems Engineering student at UTN FRBA**, I have completed a comprehensive refactoring of the project. I transformed a "legacy" monolithic codebase into a professional, scalable, and high-performance architecture, applying advanced Software Engineering principles and Design Patterns.

## 🚀 The Journey
This project represents my full development lifecycle experience, from initial concept to store publication.
* **Original Release:** Published at age 17. Focus on core mechanics and gameplay.
* **Engineering Phase (Present):** Deep refactoring focused on decoupled architecture, memory optimization, and clean code standards.

## 🛠️ Tech Stack & Skills
* **Engine:** Unity 2020.3.49 LTS.
* **Language:** C# (Advanced OOP, Data Structures).
* **Graphics:** 2D Sprite-based, moving towards custom shader implementations.
* **Tools:** Git for version control.

## 📈 Refactoring Highlights
# 🏗️ Architecture & Design Patterns
* **Event-Driven UI (Observer Pattern):** Total decoupling between game logic and UI using C# Actions. The UIManager reactively updates to score, lives, and state changes emitted by the GameManager.
* **Finite State Machine (FSM):** Implemented a robust FSM to manage global game flow (Menu, Playing, Paused, GameOver) and complex player states, replacing nested conditionals with a scalable state-based logic.
* **Data-Driven Design:** Leveraged ScriptableObjects to externalize player statistics, difficulty curves, and wave definitions, allowing for game balancing without code recompilation.

# ⚡ Performance Optimization
* **Custom Object Pooling:** Implemented a pooling system for projectiles and visual effects (FX) to minimize Instantiate/Destroy calls, significantly reducing Garbage Collector (GC) pressure.
* **Physics Layer Matrix:** Optimized collision detection by migrating from manual Tag checks in Update to a native Layer-based filtering system, delegating interaction logic to Unity's low-level physics engine.
* **Incremental Registry Cleanup:** Developed a stepwise "CleanUp" algorithm for entity registries to prevent memory leaks and maintain stable FPS during high-density enemy waves.

# 🛡️ Robustness & Clean Code
* **Animator Hashing:** Improved animation performance by pre-calculating IDs via Animator.StringToHash, avoiding expensive string lookups in every frame .
* **Centralized Constants:** Established a GameConstants static class to manage all Tags, Layers, and Audio identifiers, eliminating "Magic Strings" and ensuring type safety across the project.
* **Inheritance & Abstraction:** Refactored power-up and entity logic using base classes and method overriding (virtual/override) to enforce the DRY (Don't Repeat Yourself) principle.

## 🕹️ Play Store
[Available here](https://play.google.com/store/apps/details?id=com.Shonik.HarrysNightmares&pli=1)

---
Developed by **Ignacio Agustín Pereda Hernandez**.  
*Engineering Student @ UTN FRBA*.