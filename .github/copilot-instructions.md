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

## Naming Conventions

### Classes and Types
- Use PascalCase for class names and type names (e.g., `PlainClass`)
- Mark classes as `sealed` when inheritance is not planned
- Namespace all classes (e.g., `namespace Prototype { ... }`)
- Place opening braces on the same line for class and method declarations (e.g., `class MyClass {`)

### Fields and Properties
- Public static fields use PascalCase (e.g., `PublicStaticField`)
- Public static properties use PascalCase (e.g., `PublicStaticProperty`)
- Private static fields use underscore prefix with camelCase (e.g., `_privateStaticField`)
- Private static properties use PascalCase (e.g., `PrivateStaticProperty`)
- Public fields that should be serialized in Unity Inspector use camelCase without underscore (e.g., `publicFieldSerialized`)
- Public properties use PascalCase (e.g., `PublicProperty`)
- Private fields use underscore prefix with camelCase (e.g., `_privateField`)
- Private properties use PascalCase (e.g., `PrivateProperty`)
- Private fields that should be serialized in Unity Inspector use underscore prefix with camelCase (e.g., `_privateFieldSerialized`)

### Methods and Functions
- Use PascalCase for public methods
- Use PascalCase for Unity lifecycle methods (e.g., `Start`, `Update`)
- Place opening braces on the same line for method declarations (e.g., `void MyMethod() {`)

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
- Use standard bracing style with braces on the same line as the statement
- Format conditional statements with consistent indentation:
```csharp
if (condition) {
    // code
} else {
    // code
}
```
- Invert conditions to reduce nesting it it is make sense and do not make code less readable


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
- Do not use explicit 'private' keyword for private members (omit access modifier for private members)
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

[SerializeField] object? serializedMember;

if (!this.Validate(serializedMember)) {
    return;
}
```
- For fields in ScriptableObjects and classes under Configs use another approach:
```csharp
using Common;

[SerializeField] object? serializedMember;

SerializedMember => this.ValidateOrThrow(serializedMember);
```
- For tests you should use `!`, it is okay:
```csharp
World _world = null!;
```

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

### Data Structures
- Use appropriate data structures for performance-critical operations
- Consider memory usage and allocation patterns in frequently executed code
