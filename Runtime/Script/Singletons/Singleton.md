```
# Singleton

## Overview

`Singleton` is an abstract class based on `MonoBehaviour` that operates as a single instance throughout the game.
`SingletonManager` automatically discovers and creates all implementations via reflection at game startup, so developers do not need to place them in the scene or create them manually.

---

## Class Structure

### `Singleton` (non-generic, abstract)

The common base for all singletons. Inherits from `MonoBehaviour`.

| Member | Description |
|---|---|
| `Manager` | Reference to the `SingletonManager` that manages this singleton |
| `InitializeAsync()` | Step 1 initialization. Performs initialization that does not depend on other singletons |
| `PostInitializeAsync()` | Step 2 initialization. Performs initialization that requires other singleton instances |
| `StartAsync()` | Step 3 initialization. Runs after all singletons are ready |
| `OnEvent(int eventId)` | Receives events broadcasted by `SingletonManager.DispatchEventAsync()` |

---

### `Singleton<TSingleton>` (generic, abstract)

Base for singletons that do not require data.

```csharp
public sealed class MyManager : Singleton<MyManager>
{
    public override async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Initialization logic
    }
}
```

|Member|Description|
|:---|:---|
|`Instance`|Returns the singleton instance. Throws `InvalidOperationException` if not initialized|
|`TryGetInstance(out TSingleton?)`|Safe access to the instance. Returns `false` without exception if it does not exist|

---

### `Singleton<TSingleton, TData>` (generic, abstract)

Base for singletons that require `ScriptableObject` data set in the `SingletonDefault` asset.

```csharp
public sealed class MyManager : Singleton<MyManager, MyManagerData>
{
    public override async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        var prefab = Data.SomePrefab; // Access configuration data of type TData
    }
}
```

|Member|Description|
|:---|:---|
|`Data`|`TData` instance loaded from the `SingletonDefault` asset. Throws `InvalidOperationException` if not set|

---

## Lifecycle

### Initialization Order

`SingletonManager` runs before the scene loads via `RuntimeInitializeOnLoadMethod(AfterAssembliesLoaded)`.

```
[Runtime Start]
       │
       ▼
SingletonManager.Initialize()
 └─ Discover all Singleton implementations (reflection)
 └─ Call AddComponent() for each type
 └─ Pass Manager/Data via Singleton.ConstructorContext
       │
       ▼
Run InitializeAsync() in parallel for all singletons
       │
       ▼
Run PostInitializeAsync() in parallel for all singletons
       │
       ▼
Run StartAsync() in parallel for all singletons
       │
       ▼
[Initialization Complete — Instance accessible]
```

If other systems need to wait for singleton initialization to complete:

```csharp
await SingletonManager.WaitForInitializeAsync();
```

### CancellationToken

`InitializeAsync`, `PostInitializeAsync`, `StartAsync`, and `OnEvent` all take a `CancellationToken` parameter. This is to conform to async operation standards, and currently, `SingletonManager` does not actually propagate cancellation. However, you should always pass the `CancellationToken` to any internal async operations (`Task`, `ValueTask`, `UniTask`, etc.).

```csharp
public override async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
{
    await SomeAsyncOperation(cancellationToken); // Always pass the token
}
```

### Unity MonoBehaviour Events

`Awake`, `OnEnable`, `Start`, `OnDisable`, and `OnDestroy` are used internally in `Singleton<TSingleton>` for instance registration/unregistration and `CallState` tracking.\
If you override these methods, you **must call `base`**. If omitted, a `Debug.Assert` will occur in `OnDestroy`.

```csharp
protected override void Start()
{
    base.Start(); // Required
    // ...
}

protected override void OnDestroy()
{
    // Cleanup logic
    base.OnDestroy(); // Required — releases Instance and calls Dispose()
}
```

---

## Instance Access Rules

### Access After Initialization

You can access another singleton's `Instance` even during `InitializeAsync` ~ `StartAsync`. However, it is safest to defer access until the `PostInitializeAsync` or `StartAsync` stage.

```csharp
// Recommended: Reference another singleton in PostInitializeAsync
public override ValueTask PostInitializeAsync(CancellationToken cancellationToken = default)
{
    var table = TableManager.Instance; // At this point, TableManager has already completed InitializeAsync
    return default;
}
```

### Safe Access

`Instance` is almost always valid during normal gameplay. However, in edge cases such as PIE (Play In Editor) shutdown or before app initialization, use `TryGetInstance()`. Most code assumes `Instance` exists; use `TryGetInstance()` only in exceptional cases.

```csharp
// Typical case — direct access
MyManager.Instance.DoSomething();

// Exceptional case (PIE shutdown, before app init, etc.) — safe access
if (MyManager.TryGetInstance(out var manager))
{
    manager.DoSomething();
}
```

---

## Data Integration (SingletonData)

Singletons using `Singleton<TSingleton, TData>` require a corresponding `SingletonData` implementation.

### 1. Implement `SingletonData`

```csharp
[CreateAssetMenu]
public class MyManagerData : SingletonData
{
    public GameObject SomePrefab;
}
```

### 2. Register in `SingletonDefault` Asset

- In Project Settings → SingletonDefault, add and configure the data for the type.
- Asset path: `Assets/Settings/SingletonDefault.asset`
- On build, `SingletonPreloadBuildProcessor` automatically includes it in Preloaded Assets.

### 3. Access

```csharp
public override ValueTask InitializeAsync(CancellationToken cancellationToken = default)
{
    var prefab = Data.SomePrefab;
    return default;
}
```

---

## Event Dispatch

To broadcast an event to all singletons:

```csharp
await SingletonManager.DispatchEventAsync(eventId: 42);
```

Receive in each singleton:

```csharp
public override ValueTask OnEvent(int eventId, CancellationToken cancellationToken = default)
{
    if (eventId == 42)
    {
        // Handle
    }
    return default;
}
```

---

## Summary of Rules

|Rule|Description|
|:---|:---|
|No Scene Placement|Singletons are automatically created by `SingletonManager`. Do not manually place them in scenes or prefabs.|
|Always Call `base`|When overriding Unity lifecycle methods, you must call the `base` method to ensure correct internal state tracking.|
|Prefer `Instance`|Assume `Instance` is valid during normal gameplay. Use `TryGetInstance()` only for edge cases like PIE shutdown.|
|Data Required|`Singleton<T, TData>` will throw an exception if the corresponding data is missing in `SingletonDefault`.|
|Pass `CancellationToken`|Always propagate the `CancellationToken` to internal async operations to maintain asynchronous patterns.|
|No duplicate creation|If an instance already exists in `Awake`, a `Debug.Assert` occurs.|

---

## Related Files

|File|Role|
|:---|:---|
|`Singleton.cs`|Defines the base class|
|`SingletonManager.cs`|Manages auto-discovery, creation, and initialization|
|`SingletonDefault.cs`|ScriptableObject for singleton data mapping|
|`SingletonData.cs`|Base class for data ScriptableObjects|
|`Assets/Settings/SingletonDefault.asset`|Actual data configuration asset|
