# EconomicsGame2 Coding Guidelines

You are an AI assistant helping with the EconomicsGame2 Unity project. Please follow these guidelines when generating code:

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

### Tabs vs Spaces
- Use tabs for indentation

### Namespaces
- Group related classes under meaningful namespaces (e.g., `Prototype`)
- Use namespace structure to separate different aspects of the game

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
- Use the `var` keyword for local variables whenever possible:
```csharp
var currentPosition = transform.position;
var deltaPosition = currentPosition - lastPosition;
```

## General Practices

- Keep methods short and focused on a single responsibility
- Use descriptive names that clearly indicate purpose
- Add comments for complex logic or non-obvious implementations
- Use Debug.Log statements for temporary debugging, include meaningful context
- Avoid public fields unless they need to be serialized in the Inspector
- Prefer properties over direct field access for public APIs
- Do not use explicit 'private' keyword for private members
- Do not use explicit `this` keyword unless necessary for clarity
- Do not add obvious comments (e.g., `// If that, then this`)