---
layout: default
title: Models.StoryWorld
parent: API Reference
nav_order: 6
---

# Models.StoryWorld

Reference for the `StoryCADLib.Models.StoryWorld` namespace.

{: .fs-6 .fw-300 }

## CultureEntry

*Class*: `StoryCADLib.Models.StoryWorld.CultureEntry`

Entry for the Cultures list tab.
Represents a culture, milieu, or social environment.
For Consensus Reality stories, each entry is a milieu (e.g., Wall Street, police precinct).

```csharp
public class CultureEntry
```

### Constructors

#### CultureEntry()

```csharp
public CultureEntry()
```

### Properties

#### Name

```csharp
[JsonInclude]
[JsonPropertyName("Name")]
public string Name { get; set; }
```

**Type** `string`

#### Values

```csharp
[JsonInclude]
[JsonPropertyName("Values")]
public string Values { get; set; }
```

**Type** `string`

#### Customs

```csharp
[JsonInclude]
[JsonPropertyName("Customs")]
public string Customs { get; set; }
```

**Type** `string`

#### Taboos

```csharp
[JsonInclude]
[JsonPropertyName("Taboos")]
public string Taboos { get; set; }
```

**Type** `string`

#### Art

```csharp
[JsonInclude]
[JsonPropertyName("Art")]
public string Art { get; set; }
```

**Type** `string`

#### DailyLife

```csharp
[JsonInclude]
[JsonPropertyName("DailyLife")]
public string DailyLife { get; set; }
```

**Type** `string`

#### Entertainment

```csharp
[JsonInclude]
[JsonPropertyName("Entertainment")]
public string Entertainment { get; set; }
```

**Type** `string`

---

## GovernmentEntry

*Class*: `StoryCADLib.Models.StoryWorld.GovernmentEntry`

Entry for the Governments list tab.
Represents a government, faction, or power structure.

```csharp
public class GovernmentEntry
```

### Constructors

#### GovernmentEntry()

```csharp
public GovernmentEntry()
```

### Properties

#### Name

```csharp
[JsonInclude]
[JsonPropertyName("Name")]
public string Name { get; set; }
```

**Type** `string`

#### Type

```csharp
[JsonInclude]
[JsonPropertyName("Type")]
public string Type { get; set; }
```

**Type** `string`

#### PowerStructures

```csharp
[JsonInclude]
[JsonPropertyName("PowerStructures")]
public string PowerStructures { get; set; }
```

**Type** `string`

#### Laws

```csharp
[JsonInclude]
[JsonPropertyName("Laws")]
public string Laws { get; set; }
```

**Type** `string`

#### ClassStructure

```csharp
[JsonInclude]
[JsonPropertyName("ClassStructure")]
public string ClassStructure { get; set; }
```

**Type** `string`

#### ForeignRelations

```csharp
[JsonInclude]
[JsonPropertyName("ForeignRelations")]
public string ForeignRelations { get; set; }
```

**Type** `string`

---

## PhysicalWorldEntry

*Class*: `StoryCADLib.Models.StoryWorld.PhysicalWorldEntry`

Entry for the Physical World list tab.
Represents a world, planet, or realm in multi-world stories.

```csharp
public class PhysicalWorldEntry
```

### Constructors

#### PhysicalWorldEntry()

```csharp
public PhysicalWorldEntry()
```

### Properties

#### Name

```csharp
[JsonInclude]
[JsonPropertyName("Name")]
public string Name { get; set; }
```

**Type** `string`

#### Geography

```csharp
[JsonInclude]
[JsonPropertyName("Geography")]
public string Geography { get; set; }
```

**Type** `string`

#### Climate

```csharp
[JsonInclude]
[JsonPropertyName("Climate")]
public string Climate { get; set; }
```

**Type** `string`

#### NaturalResources

```csharp
[JsonInclude]
[JsonPropertyName("NaturalResources")]
public string NaturalResources { get; set; }
```

**Type** `string`

#### Flora

```csharp
[JsonInclude]
[JsonPropertyName("Flora")]
public string Flora { get; set; }
```

**Type** `string`

#### Fauna

```csharp
[JsonInclude]
[JsonPropertyName("Fauna")]
public string Fauna { get; set; }
```

**Type** `string`

#### Astronomy

```csharp
[JsonInclude]
[JsonPropertyName("Astronomy")]
public string Astronomy { get; set; }
```

**Type** `string`

---

## ReligionEntry

*Class*: `StoryCADLib.Models.StoryWorld.ReligionEntry`

Entry for the Religions list tab.
Represents a religion or belief system.

```csharp
public class ReligionEntry
```

### Constructors

#### ReligionEntry()

```csharp
public ReligionEntry()
```

### Properties

#### Name

```csharp
[JsonInclude]
[JsonPropertyName("Name")]
public string Name { get; set; }
```

**Type** `string`

#### Deities

```csharp
[JsonInclude]
[JsonPropertyName("Deities")]
public string Deities { get; set; }
```

**Type** `string`

#### Beliefs

```csharp
[JsonInclude]
[JsonPropertyName("Beliefs")]
public string Beliefs { get; set; }
```

**Type** `string`

#### Practices

```csharp
[JsonInclude]
[JsonPropertyName("Practices")]
public string Practices { get; set; }
```

**Type** `string`

#### Organizations

```csharp
[JsonInclude]
[JsonPropertyName("Organizations")]
public string Organizations { get; set; }
```

**Type** `string`

#### CreationMyths

```csharp
[JsonInclude]
[JsonPropertyName("CreationMyths")]
public string CreationMyths { get; set; }
```

**Type** `string`

---

## SpeciesEntry

*Class*: `StoryCADLib.Models.StoryWorld.SpeciesEntry`

Entry for the People/Species list tab.
Represents a species, race, or people group.

```csharp
public class SpeciesEntry
```

### Constructors

#### SpeciesEntry()

```csharp
public SpeciesEntry()
```

### Properties

#### Name

```csharp
[JsonInclude]
[JsonPropertyName("Name")]
public string Name { get; set; }
```

**Type** `string`

#### PhysicalTraits

```csharp
[JsonInclude]
[JsonPropertyName("PhysicalTraits")]
public string PhysicalTraits { get; set; }
```

**Type** `string`

#### Lifespan

```csharp
[JsonInclude]
[JsonPropertyName("Lifespan")]
public string Lifespan { get; set; }
```

**Type** `string`

#### Origins

```csharp
[JsonInclude]
[JsonPropertyName("Origins")]
public string Origins { get; set; }
```

**Type** `string`

#### SocialStructure

```csharp
[JsonInclude]
[JsonPropertyName("SocialStructure")]
public string SocialStructure { get; set; }
```

**Type** `string`

#### Diversity

```csharp
[JsonInclude]
[JsonPropertyName("Diversity")]
public string Diversity { get; set; }
```

**Type** `string`
