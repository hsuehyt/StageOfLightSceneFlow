# StageOfLightSceneFlow

A lightweight Unity scene-flow solution for immersive projection rooms/arena software.
Keep a **persistent master scene** for system I/O and fades, let **content scenes** stay artist-friendly for preview, and avoid duplicate “stage” devices at runtime.

---

## What’s inside

* **GameFlowManager** – the central, persistent scene transition controller (fade → load additive → set active → unload others). Lives in the master scene.&#x20;
* **SceneAdvanceProxy** – a tiny bridge in each content scene. Your Timeline (or any event) calls this to move to the next scene; the proxy forwards to GameFlowManager.
* **Do not Destroy** – a keep-alive helper (or simply `DontDestroyOnLoad`) on master-scene systems that must persist (e.g., comms with the arena/projection system).
* **StageOfLightSingleton** – ensures only one **StageOfLightPrefab** survives at runtime; duplicate preview copies in additively-loaded scenes auto-remove (artists can still preview in-editor).
* **StageOfLightPrefab** – your device/bridge object that talks to the arena/projection system.

---

## Typical scene layout

* **SceneMaster** (bootstrap; stays loaded)

  * `GameFlow` (with **GameFlowManager** + UI **CanvasGroup** fader) &#x20;
  * `StageOfLight` (the **real** StageOfLightPrefab with **Do not Destroy** and **StageOfLightSingleton**)
  * Optional: global AudioMixer, EventSystem, diagnostics, etc.

* **scene01, scene02, …** (artist content)

  * Local `PlayableDirector` + Timeline (animation/content)
  * `SceneAdvance` (with **SceneAdvanceProxy** + Signal Receiver)
  * A **preview** copy of **StageOfLightPrefab** (so artists see exact behavior in-editor). At runtime, this duplicate self-removes via **StageOfLightSingleton**.

---

## How it works (runtime flow)

1. You start from **SceneMaster**.

   * **GameFlowManager** marks itself persistent and remembers the bootstrap scene name. It controls fades and scene transitions.&#x20;

2. You move into **scene01** (however you like—button, Timeline, etc.).

   * The master’s **StageOfLightPrefab** persists; any new copies loaded from scene01 detect an existing instance and destroy themselves (no double connections to the arena system).

3. At the end of **scene01**, the Timeline triggers **SceneAdvanceProxy.Next()**.

   * The proxy calls `GameFlowManager.Next(nextScene)` (you pick the `nextScene` in the Inspector).

4. **GameFlowManager** does the handoff:

   * Fade to black → `LoadSceneAsync(nextScene, Additive)` → `SetActiveScene(nextScene)` → unload everything **except** the target scene, the master (bootstrap) scene, and the `DontDestroyOnLoad` scene → fade in.&#x20;

Result: smooth transitions, single authoritative StageOfLight at runtime, and artist scenes remain fully previewable in isolation.

---

## Setup (step-by-step)

1. **SceneMaster**

   * Add a full-screen **Canvas** with a black Image and a **CanvasGroup** (alpha 0).
   * Add `GameFlowManager` and drag the CanvasGroup into its `fader` field. Set `fadeTime` if desired.&#x20;
   * Place **StageOfLightPrefab** here with **Do not Destroy** + **StageOfLightSingleton**.

2. **Build Settings**

   * Add **SceneMaster**, **scene01**, **scene02**, … to **Scenes In Build** (names must match what you’ll call).

3. **scene01 / scene02 …**

   * Add your Timeline content.
   * Create a `SceneAdvance` GameObject → add **SceneAdvanceProxy** → set **Next Scene** to the exact next scene name (e.g., `scene02`).
   * Add a **Signal Receiver** to `SceneAdvance`. At the end of your Timeline, add a **Signal Emitter** and bind it to call `SceneAdvanceProxy.Next()`.
   * Keep a **preview** StageOfLightPrefab in the scene (artists can see it in-editor). It will auto-remove at runtime thanks to **StageOfLightSingleton**.

---

## Authoring & preview for artists

* Artists can open **scene01** directly and press Play to preview.
* They’ll see the local StageOfLightPrefab behave for alignment/testing.
* In the full app flow (starting in **SceneMaster**), duplicates won’t persist—only the master’s StageOfLight stays.

---

## Notes on the arena/projection system

* The master **StageOfLightPrefab** is your single runtime point of truth to talk to the arena/projection software (sockets/OSC/serial/etc.).
* Keeping it in **SceneMaster** avoids reconnects and keeps state stable across scene changes.
* Preview copies in content scenes are for layout/feedback only; the singleton pattern guarantees you won’t double-open connections in production.

---

## Alternative trigger (no Signals)

If you prefer code over signals, add this to the content scene’s **PlayableDirector**:

* When the Timeline **stops**, it calls `GameFlowManager.Next(nextScene)`.
* You still configure `nextScene` in the **SceneAdvanceProxy** (so producers can change order without code).

---

## Troubleshooting

* **Black stays up / no fade back**

  * Ensure `GameFlowManager` has a valid **CanvasGroup** assigned.&#x20;
* **Signal errors like “SerializedObject… is null”**

  * A Signal Receiver entry lost its **Object** reference. Rebind or remove the broken row.
* **Didn’t advance to next scene**

  * Check that the **Signal Emitter** is on an **unmuted** track, the Receiver calls `SceneAdvanceProxy.Next()`, and the **scene name matches** **Scenes In Build**.
* **Duplicate StageOfLight at runtime**

  * Confirm the singleton script is on the **prefab asset**, not just one scene instance, so all copies self-manage.

---

## Why this pattern

* **Reliable**: One place handles fades and scene lifecycles.&#x20;
* **Artist-friendly**: Scenes remain standalone and previewable.
* **Production-safe**: Exactly one StageOfLight talks to the arena system; no duplicate connections.

---

## At a glance (responsibilities)

* **GameFlowManager**

  * Fade UI, additive load, set active scene, unload others, keep **SceneMaster** alive.&#x20;
* **SceneAdvanceProxy**

  * Exposes “Next Scene” in Inspector; provides a `Next()` method for Timeline Signals / script hooks.
* **Do not Destroy**

  * Ensures critical objects (e.g., StageOfLight in SceneMaster) survive scene loads.
* **StageOfLightSingleton**

  * Kills duplicate StageOfLight instances when content scenes are additively loaded.
* **StageOfLightPrefab**

  * The device/comms bridge used **for real** in SceneMaster and **for preview** in content scenes.
