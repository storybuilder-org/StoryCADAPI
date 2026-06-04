---
layout: default
title: "OperationResult&lt;T&gt;"
parent: "API Reference"
nav_order: 2
---

# OperationResult\<T\> Class

A result wrapper that encapsulates success/failure state and payload for all API operations.

<div class="code-tabs" markdown="1">

```csharp
namespace StoryCADLib.Services.API

public class OperationResult<T>
```

```python
# (no direct Python equivalent — the Python binding has no OperationResult wrapper.
#  Methods on the StoryCAD object return their payload directly and raise on error.)
from storycad import StoryCAD
```

</div>

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

<div class="code-tabs" markdown="1">

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

```python
# The Python wrapper raises on error instead of returning a result object.
try:
    elements = sc.create_empty_outline("My Story", "Author", template_index="0")
    print(f"Created {len(elements)} elements")
except Exception as error:
    print(f"Error: {error}")
```

</div>

## Static Factory Methods

### Success

Creates a successful result with a payload.

<div class="code-tabs" markdown="1">

```csharp
public static OperationResult<T> Success(T payload)
```

```python
# (no direct Python equivalent — there is no OperationResult to construct.
#  A successful Python call simply returns the payload directly.)
```

</div>

**Example:**
<div class="code-tabs" markdown="1">

```csharp
return OperationResult<List<Guid>>.Success(elementGuids);
```

```python
# (no direct Python equivalent — Python returns the payload directly instead of
#  wrapping it in a success result.)
return element_guids
```

</div>

---

### Failure

Creates a failed result with an error message.

<div class="code-tabs" markdown="1">

```csharp
public static OperationResult<T> Failure(string errorMessage)
```

```python
# (no direct Python equivalent — there is no failure result to construct.
#  The Python wrapper signals failure by raising an exception.)
```

</div>

**Example:**
<div class="code-tabs" markdown="1">

```csharp
return OperationResult<bool>.Failure("Element not found");
```

```python
# (no direct Python equivalent — Python raises instead of returning a failure result.)
raise Exception("Element not found")
```

</div>

---

### SafeExecuteAsync

Wraps an async operation, catching exceptions and converting them to failure results.

<div class="code-tabs" markdown="1">

```csharp
public static async Task<OperationResult<T>> SafeExecuteAsync(Task<T> operation)
```

```python
# (no direct Python equivalent — there is no SafeExecuteAsync wrapper.
#  Python lets exceptions propagate; callers wrap calls in try/except as needed.)
```

</div>

**Example:**
<div class="code-tabs" markdown="1">

```csharp
var result = await OperationResult<StoryModel>.SafeExecuteAsync(
    outlineService.CreateModel(name, author, templateIndex));
```

```python
# (no direct Python equivalent — Python has no SafeExecuteAsync wrapper.
#  Call the method directly and handle errors with try/except.)
try:
    elements = sc.create_empty_outline(name, author, template_index=template_index)
except Exception as error:
    print(f"Error: {error}")
```

</div>

## Best Practices

### Do: Check IsSuccess First

<div class="code-tabs" markdown="1">

```csharp
// Good
var result = api.GetStoryElement(guid);
if (result.IsSuccess)
{
    var element = result.Payload;
    // Use element...
}
```

```python
# Good - the Python wrapper raises on error, so wrap the call in try/except.
try:
    element = sc.get_element(handle)
    # Use element...
except Exception as error:
    print(f"Error: {error}")
```

</div>

### Don't: Access Payload Without Checking

<div class="code-tabs" markdown="1">

```csharp
// Bad - Payload may be null/default if operation failed
var element = api.GetStoryElement(guid).Payload;
```

```python
# Bad - an unhandled error here raises and crashes the caller.
element = sc.get_element(handle)
```

</div>

### Do: Propagate Errors

<div class="code-tabs" markdown="1">

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

```python
# Python propagates errors by letting the exception bubble up — no result wrapper.
def my_operation():
    element = sc.get_element(handle)  # raises if the element is not found

    # Continue with operation...
    return True
```

</div>

### Do: Provide Meaningful Error Messages

Error messages should help diagnose the problem:

<div class="code-tabs" markdown="1">

```csharp
// Good
return OperationResult<T>.Failure(
    $"Element with GUID {guid} not found in current model");

// Not helpful
return OperationResult<T>.Failure("Error");
```

```python
# Python raises instead of returning a failure result; give the exception a clear message.
# Good
raise Exception(f"Element with GUID {guid} not found in current model")

# Not helpful
raise Exception("Error")
```

</div>

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
