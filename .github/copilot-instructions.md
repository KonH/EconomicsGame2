# EconomicsGame2 Coding Guidelines

You are an AI assistant helping with the EconomicsGame2 Unity project. Please follow these guidelines when generating code:

## General Guidelines

### Tabs vs Spaces
- Use tabs for indentation

### Namespaces
- Group related classes under meaningful namespaces (e.g., `Prototype`)
- Use namespace structure to separate different aspects of the game
- Place namespaces in given order, without spaces between them:
```
using System; // All system namespaces
using UnityEngine; // All Unity namespaces
using ThirdParty; // All third-party libraries
using EconomicsGame.*; // All project-specific namespaces
```

### Script Creation and File Management
- **Use `manage_script` command only for creating new C# scripts with empty content**
- **Do NOT use `manage_script` for updating existing scripts** - use normal file editing instead
- After creating empty script with `manage_script`, add the actual script content using normal file editing
- All scripts should follow the established code style and naming conventions
- Unity will automatically generate corresponding `.meta` files for scripts created via `manage_script`
- Meta files contain Unity's asset metadata and GUIDs and should be committed to version control

### Asset Creation
- **Asset files (.asset) are human user responsibility**
- Do not attempt to create ScriptableObject assets programmatically
- Focus on script implementation and system logic
- Human users will create assets using Unity Editor menus or manual creation

## Naming Conventions

### Classes and Types
- Use PascalCase for class names and type names (e.g., `PlainClass`)
- Mark classes as `sealed` when inheritance is not planned
- Namespace all classes (e.g., `namespace Prototype { ... }`)
- **ALWAYS place opening braces on the same line** for class and method declarations (e.g., `class MyClass {`)

### Fields and Properties
- Use explicit `private` access modifier for private members
- Public static fields use PascalCase (e.g., `PublicStaticField`)
- Public static properties use PascalCase (e.g., `PublicStaticProperty`)
- Private static fields use underscore prefix with camelCase (e.g., `_privateStaticField`)
- Private static properties use PascalCase (e.g., `PrivateStaticProperty`)
- Public fields that should be serialized in Unity Inspector use camelCase without underscore (e.g., `publicFieldSerialized`)
- Public properties use PascalCase (e.g., `PublicProperty`)
- Private fields always use underscore prefix with camelCase (e.g., `_privateField`)
- The underscore prefix does not affect Unity serialization; use `[SerializeField]` for private fields that should appear in the Inspector.
- Private fields that need to be serialized in the Unity Inspector must be marked with `[SerializeField]` (e.g., `[SerializeField] private int _mySerializedField;`).
- Private properties use PascalCase (e.g., `PrivateProperty`)

### Methods and Functions
- Use PascalCase for public methods
- Use PascalCase for Unity lifecycle methods (e.g., `Start`, `Update`)
- **ALWAYS place opening braces on the same line** for method declarations (e.g., `void MyMethod() {`)

## Code Organization

### Class Members Ordering
1. Static fields and properties
2. Nested types
3. Public instance fields and properties
4. Private instance fields and properties
5. Unity lifecycle methods (e.g., `Awake`, `Start`, `Update`)
6. Public methods
7. Private methods

## Unity-Specific Guidelines

### MonoBehaviour Pattern
- Extend `MonoBehaviour` for Unity components
- Implement Unity lifecycle methods as needed (`Awake`, `Start`, `Update`, etc.)
- Keep `Update` methods efficient or empty when not needed

### Unity Object Null Checks
- Use implicit bool conversion when checking Unity objects (`if (!obj)`) instead of explicit null comparison (`if (obj == null)`)
- Unity overrides the bool conversion operator for UnityEngine.Object to properly handle destroyed objects
- Always check if Unity objects are valid before accessing them using this pattern
- When caching references to Unity objects, validate them before usage

## Control Structures
- **ALWAYS use standard bracing style with braces on the same line** as the statement
- Format conditional statements with consistent indentation:
```csharp
if (condition) {
    // code
} else {
    // code
}
```
- Invert conditions to reduce nesting it it is make sense and do not make code less readable
- Add empty lines between switch case blocks for better readability:
```csharp
switch (value) {
    case Type1:
        // code
        break;

    case Type2:
        // code
        break;

    default:
        // code
        break;
}
```


## Variable Declarations
- ALWAYS use the `var` keyword for local variables when the type is obvious from the right side of the assignment:
```csharp
var currentPosition = transform.position;
var deltaPosition = currentPosition - lastPosition;
var index = 0;
var count = myList.Count;
```

## General Practices

- Keep methods short and focused on a single responsibility
- Use descriptive names that clearly indicate purpose
- Add comments for complex logic or non-obvious implementations
- Use Debug.Log statements for temporary debugging, include meaningful context
- Avoid public fields unless they need to be serialized in the Inspector
- Prefer properties over direct field access for public APIs
- Use explicit `private` access modifier for private members
- Do not use explicit `this` keyword unless necessary for clarity
- Do not add obvious comments (e.g., `// If that, then this`), add comments only when necessary to explain really complex logic or completely unclear implementations
- Never use standard or XML comments anywhere at all, they are not useful in this project

## Nullable Reference Types

- Use nullable reference types to indicate that a reference can be null
- Use `?` suffix for nullable types (e.g., `string?`, `GameObject?`)
- Use `null` checks to handle nullable references safely
- Use `??` operator to provide default values for nullable references (e.g., `stringValue ?? "default"`)
- Use `?.` operator to safely access members of nullable references (e.g., `nullableObject?.Method()`)
- Use `!` only when you can't avoid that or inside tests
- Use custom validation method for serialized references in Unity components (in OnValidate method and at start of methods where serialized references are used):
```csharp
using Common;

[SerializeField] private object? _serializedMember;

if (!this.Validate(_serializedMember)) {
    return;
}
```
- For fields in ScriptableObjects and classes under Configs use another approach:
```csharp
using Common;

[SerializeField] private object? _serializedMember;

SerializedMember => this.ValidateOrThrow(_serializedMember);
```
- For tests you should use `!`, it is okay:
```csharp
private World _world = null!;
```
- Don't use string?, use string.Empty instead of null for strings that can be empty
- For collections use empty collections instead of null (e.g., `List<T>()` or `Array.Empty<T>()` instead of `null`)

## Project Specifics

### Scripts Directory Structure

- **Bootstrap** - initialization and entry point
- **Components** - ECS components, plain structs
- **Configs** - configuration, data classes
- **Services** - shared logic, services, accessible from anywhere
- **Systems** - ECS systems, logic that operates on components
- **UnityComponents** - Unity-specific components, MonoBehaviours

## ECS Architecture Guidelines

### Components
- Create small, focused components that store data only
- Use `[Persistent]` attribute for components that need to be saved/loaded
- Components should be simple structs with minimal logic
- Use descriptive names that clearly indicate purpose (e.g., `MovementTargetCell`)
- Do NOT use `[Serializable]` attribute on components - it's not needed for ECS components
- Use `[Persistent]` only for components that need to persist across save/load operations
- Use `[OneFrame]` attribute for event components that should be automatically removed after processing
- Do not add obvious comments about marker components - their purpose is clear from the name

### Systems
- Systems should focus on a single responsibility
- Use clear naming that describes what the system does (e.g., `PathfindingSystem`)
- Define QueryDescription at the class level for system queries
- Register systems in GameLifetimeScope in the order they should execute
- Use dependency injection for required services and settings

### Pathfinding Implementation
- Use A* algorithm for movement pathfinding
- Consider using Manhattan distance for grid-based movement with cardinal directions
- Check cell walkability by querying for obstacles or locked cells
- Process one step at a time for smooth movement visualization
- Add movement target components instead of direct movement components to enable pathfinding
- For keyboard/direct movement, add movement components directly when appropriate

### Services
- Use services for shared functionality across systems
- Inject services where needed rather than creating new instances
- Use services for complex operations that don't fit within the ECS paradigm

### Development Process
- Implement features incrementally with small, testable steps
- Commit changes with descriptive messages after each logical step
- Update TODO.md to track progress on features
- When implementing algorithms like A*, start with a simple version and optimize later

### Full Development Process

#### Planning Phase
1. **User provides general idea** - User describes a feature concept in broad terms
2. **Create plan document** - Assistant creates `docs/wip/FeatureName.md` with sections:
   - **Overview**: High-level description of the feature and its purpose
   - **Tech Spec**: Technical specifications including components, systems, and configuration
   - **Steps To Implement Checklist**: Detailed implementation phases with checkboxes
3. **Iterative refinement** - User provides feedback on each section, assistant updates document
4. **Final plan approval** - User approves the complete plan document

#### Implementation Phase
When working through implementation phases from plan documents:

1. **Complete Phase Steps**: Implement all tasks in the current phase
2. **Wait for Feedback**: After completing a phase, wait for user feedback before proceeding
3. **Mark Completed Steps**: Update the checklist by marking completed steps with [x]
4. **Handle Corrections**: If user feedback indicates issues or changes needed:
   - Add new checklist items with "Fix:" prefix and brief description
   - Example: "- [ ] Fix: Adjust probability calculation in ItemGenerationProcessingSystem"
5. **Proceed to Next Phase**: Only move to the next phase after user approval of current phase
6. **Document Progress**: Update TODO.md when features are completed
7. **Commit Changes**: Commit all changes after completing a phase with descriptive commit message

This workflow ensures iterative development with proper feedback loops and clear tracking of progress and corrections.

### Config, Service and System Registration
- Always register new services in GameLifetimeScope.Configure() method:
  ```csharp
  builder.Register<NewService>(Lifetime.Scoped).AsSelf();
  ```
- Always register new configs in GameLifetimeScope:
  ```csharp
  [SerializeField] private NewConfig? _newConfig;
  this.ValidateOrThrow(_newConfig);
  builder.RegisterInstance(_newConfig).AsSelf();
  ```
- Always register new systems in the ArchApp configuration:
  ```csharp
  c.Add<NewSystem>();
  ```
- Register services and configs before systems in the configuration order

### Config Classes for Testability
> **Rationale:** Unity serialization requires config classes to have a parameterless constructor, which prevents the use of constructor-based initialization for test data. Using a `TestInit` method allows test code to set up config objects with specific values without relying on reflection or breaking Unity's serialization. This approach ensures both testability and compatibility with Unity's asset pipeline.
- Always provide TestInit method for config classes (Serializable/Asset):
  ```csharp
  [Serializable]
  public sealed class MyConfig {
      [SerializeField] private int _priority = 1;
      [SerializeField] private float _value = 1f;

      public void TestInit(int priority, float value) {
          _priority = priority;
          _value = value;
      }

      public int Priority => _priority;
      public float Value => _value;
  }
  ```
- Use parameterless constructor for Unity serialization and full constructor for testing
- Avoid reflection in tests - use constructors instead

### Data Structures
- Use appropriate data structures for performance-critical operations
- Consider memory usage and allocation patterns in frequently executed code
