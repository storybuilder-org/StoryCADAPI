using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Outliner
{
    /// <summary>
    /// Response structure for single-pass outline analysis.
    /// Inner properties match the LLM's camelCase JSON via case-insensitive deserialization;
    /// only the top-level story_overview key needs an explicit attribute.
    /// </summary>
    public class OnePassResponse
    {
        [JsonPropertyName("story_overview")]
        public StoryOverviewElement? StoryOverview { get; set; }

        public List<CharacterElement>? Characters { get; set; }

        public List<SettingElement>? Settings { get; set; }

        public List<SceneElement>? Scenes { get; set; }

        public List<ProblemElement>? Problems { get; set; }
    }

    public class StoryOverviewElement
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? StoryIdea { get; set; }
        public string? Viewpoint { get; set; }
        public string? ViewpointCharacter { get; set; }
        public string? Premise { get; set; }
        public string? StoryType { get; set; }
        public string? StoryGenre { get; set; }
        public string? StoryProblem { get; set; }
        public string? Concept { get; set; }
    }

    public class CharacterElement
    {
        public string Guid { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? CharacterSketch { get; set; }
        public string? Role { get; set; }
        public string? StoryRole { get; set; }
        public string? Age { get; set; }
        public string? Sex { get; set; }
        public string? Eyes { get; set; }
        public string? Hair { get; set; }
        public string? Weight { get; set; }
        public string? Health { get; set; }
        public string? PhysNotes { get; set; }
        public string? Appearance { get; set; }
        public string? Ethnic { get; set; }
        public string? Religion { get; set; }
        public string? Education { get; set; }
        public string? Focus { get; set; }
        public string? PsychNotes { get; set; }
        public string? Flaw { get; set; }
        public string? BackStory { get; set; }
        public string? Relationships { get; set; }
    }

    public class SettingElement
    {
        public string Guid { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Summary { get; set; }
        public string? Locale { get; set; }
        public string? Season { get; set; }
        public string? Period { get; set; }
        public string? Lighting { get; set; }
        public string? Sights { get; set; }
        public string? Sounds { get; set; }
        public string? Touch { get; set; }
        public string? SmellTaste { get; set; }
    }

    public class SceneElement
    {
        public string Guid { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Protagonist { get; set; }
        public string? ProtGoal { get; set; }
        public string? Significance { get; set; }
        public string? Antagonist { get; set; }
        public string? AntagGoal { get; set; }
        public string? AntagMotive { get; set; }
        public string? Outcome { get; set; }
        public string? ViewpointCharacter { get; set; }
        public string? Setting { get; set; }
        public List<string>? Cast { get; set; }
        public List<string>? ScenePurpose { get; set; }
    }

    public class ProblemElement
    {
        public string Guid { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? StoryQuestion { get; set; }
        public string? ProblemType { get; set; }
        public string? ConflictType { get; set; }
        public string? ProblemCategory { get; set; }
        public string? ProblemSource { get; set; }
        public string? Subject { get; set; }
        public string? Theme { get; set; }
        public string? Method { get; set; }
        public string? Protagonist { get; set; }
        public string? ProtGoal { get; set; }
        public string? ProtMotive { get; set; }
        public string? ProtConflict { get; set; }
        public string? Significance { get; set; }
        public string? Antagonist { get; set; }
        public string? AntagGoal { get; set; }
        public string? AntagMotive { get; set; }
        public string? AntagConflict { get; set; }
        public string? Outcome { get; set; }
        public string? Premise { get; set; }
    }
}
