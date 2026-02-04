# OperationResult\<T\> Class

A result wrapper that encapsulates success/failure state and payload for all API operations.

```csharp
namespace StoryCADLib.Services.API

public class OperationResult<T>
```

## Overview

All StoryCADLib API methods return `OperationResult<T>` instead of throwing exceptions. This pattern ensures:

- **Safe consumption** - No unexpected exceptions for external callers
- **Explicit error handling** - Errors are communicated through properties
- **Consistent interface** - All methods follow the same result pattern

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsSuccess` | bool | True if the operation succeeded |
| `Payload` | T | The result data (only valid if IsSuccess is true) |
| `ErrorMessage` | string | Description of what went wrong (only set if IsSuccess is false) |

## Usage Pattern

Always check `IsSuccess` before accessing `Payload`:

```csharp
var result = await api.CreateEmptyOutline("My Story", "Author", "0");

if (result.IsSuccess)
{
    // Safe to use Payload
    var guids = result.Payload;
    Console.WriteLine($"Created {guids.Count} elements");
}
else
{
    // Handle error
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

## Static Factory Methods

### Success

Creates a successful result with a payload.

```csharp
public static OperationResult<T> Success(T payload)
```

**Example:**
```csharp
return OperationResult<List<Guid>>.Success(elementGuids);
```

---

### Failure

Creates a failed result with an error message.

```csharp
public static OperationResult<T> Failure(string errorMessage)
```

**Example:**
```csharp
return OperationResult<bool>.Failure("Element not found");
```

---

### SafeExecuteAsync

Wraps an async operation, catching exceptions and converting them to failure results.

```csharp
public static async Task<OperationResult<T>> SafeExecuteAsync(Task<T> operation)
```

**Example:**
```csharp
var result = await OperationResult<StoryModel>.SafeExecuteAsync(
    outlineService.CreateModel(name, author, templateIndex));
```

## Best Practices

### Do: Check IsSuccess First

```csharp
// Good
var result = api.GetStoryElement(guid);
if (result.IsSuccess)
{
    var element = result.Payload;
    // Use element...
}
```

### Don't: Access Payload Without Checking

```csharp
// Bad - Payload may be null/default if operation failed
var element = api.GetStoryElement(guid).Payload;
```

### Do: Propagate Errors

```csharp
public OperationResult<bool> MyOperation()
{
    var result = api.GetStoryElement(guid);
    if (!result.IsSuccess)
    {
        return OperationResult<bool>.Failure(result.ErrorMessage);
    }

    // Continue with operation...
    return OperationResult<bool>.Success(true);
}
```

### Do: Provide Meaningful Error Messages

Error messages should help diagnose the problem:

```csharp
// Good
return OperationResult<T>.Failure(
    $"Element with GUID {guid} not found in current model");

// Not helpful
return OperationResult<T>.Failure("Error");
```

## Common Return Types

| Method | Returns |
|--------|---------|
| `CreateEmptyOutline` | `OperationResult<List<Guid>>` |
| `OpenOutline` | `OperationResult<bool>` |
| `WriteOutline` | `OperationResult<string>` |
| `GetAllElements` | `OperationResult<ObservableCollection<StoryElement>>` |
| `GetStoryElement` | `OperationResult<StoryElement>` |
| `AddElement` | `OperationResult<Guid>` |
| `UpdateElementProperty` | `OperationResult<StoryElement>` |
| `DeleteElement` | `OperationResult<bool>` |
| `SearchForText` | `OperationResult<List<Dictionary<string, object>>>` |
