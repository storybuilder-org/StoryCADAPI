using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Outliner
{
    /// <summary>
    /// Response structure for single-pass outline analysis
    /// </summary>
    public class OnePassResponse
    {
        [JsonPropertyName("story_overview")]
        public StoryOverviewElement? StoryOverview { get; set; }

        [JsonPropertyName("characters")]
        public List<CharacterElement>? Characters { get; set; }

        [JsonPropertyName("settings")]
        public List<SettingElement>? Settings { get; set; }

        [JsonPropertyName("scenes")]
        public List<SceneElement>? Scenes { get; set; }

        [JsonPropertyName("problems")]
        public List<ProblemElement>? Problems { get; set; }
    }

    public class StoryOverviewElement
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("premise")]
        public string? Premise { get; set; }
    }

    public class CharacterElement
    {
        [JsonPropertyName("guid")]
        public string Guid { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("character_sketch")]
        public string? CharacterSketch { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("age")]
        public string? Age { get; set; }

        [JsonPropertyName("sex")]
        public string? Sex { get; set; }

        [JsonPropertyName("eyes")]
        public string? Eyes { get; set; }

        [JsonPropertyName("hair")]
        public string? Hair { get; set; }

        [JsonPropertyName("weight")]
        public string? Weight { get; set; }

        [JsonPropertyName("health")]
        public string? Health { get; set; }

        [JsonPropertyName("phys_notes")]
        public string? PhysNotes { get; set; }

        [JsonPropertyName("appearance")]
        public string? Appearance { get; set; }

        [JsonPropertyName("ethnic")]
        public string? Ethnic { get; set; }

        [JsonPropertyName("religion")]
        public string? Religion { get; set; }

        [JsonPropertyName("education")]
        public string? Education { get; set; }

        [JsonPropertyName("focus")]
        public string? Focus { get; set; }

        [JsonPropertyName("psych_notes")]
        public string? PsychNotes { get; set; }

        [JsonPropertyName("flaw")]
        public string? Flaw { get; set; }

        [JsonPropertyName("backstory")]
        public string? BackStory { get; set; }

        [JsonPropertyName("relationships")]
        public string? Relationships { get; set; }
    }

    public class SettingElement
    {
        [JsonPropertyName("guid")]
        public string Guid { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("season")]
        public string? Season { get; set; }

        [JsonPropertyName("period")]
        public string? Period { get; set; }

        [JsonPropertyName("lighting")]
        public string? Lighting { get; set; }

        [JsonPropertyName("sights")]
        public string? Sights { get; set; }

        [JsonPropertyName("sounds")]
        public string? Sounds { get; set; }

        [JsonPropertyName("touch")]
        public string? Touch { get; set; }

        [JsonPropertyName("smell_taste")]
        public string? SmellTaste { get; set; }
    }

    public class SceneElement
    {
        [JsonPropertyName("guid")]
        public string Guid { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("protagonist")]
        public string? Protagonist { get; set; }

        [JsonPropertyName("prot_goal")]
        public string? ProtGoal { get; set; }

        [JsonPropertyName("significance")]
        public string? Significance { get; set; }

        [JsonPropertyName("antagonist")]
        public string? Antagonist { get; set; }

        [JsonPropertyName("antag_goal")]
        public string? AntagGoal { get; set; }

        [JsonPropertyName("antag_motive")]
        public string? AntagMotive { get; set; }

        [JsonPropertyName("outcome")]
        public string? Outcome { get; set; }

        [JsonPropertyName("viewpoint_character")]
        public string? ViewpointCharacter { get; set; }

        [JsonPropertyName("setting")]
        public string? Setting { get; set; }

        [JsonPropertyName("cast")]
        public List<string>? Cast { get; set; }
    }

    public class ProblemElement
    {
        [JsonPropertyName("guid")]
        public string Guid { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("story_question")]
        public string? StoryQuestion { get; set; }

        [JsonPropertyName("problem_type")]
        public string? ProblemType { get; set; }

        [JsonPropertyName("conflict_type")]
        public string? ConflictType { get; set; }

        [JsonPropertyName("problem_category")]
        public string? ProblemCategory { get; set; }

        [JsonPropertyName("problem_source")]
        public string? ProblemSource { get; set; }

        [JsonPropertyName("protagonist")]
        public string? Protagonist { get; set; }

        [JsonPropertyName("prot_goal")]
        public string? ProtGoal { get; set; }

        [JsonPropertyName("significance")]
        public string? Significance { get; set; }

        [JsonPropertyName("antagonist")]
        public string? Antagonist { get; set; }

        [JsonPropertyName("antag_goal")]
        public string? AntagGoal { get; set; }

        [JsonPropertyName("antag_motive")]
        public string? AntagMotive { get; set; }

        [JsonPropertyName("outcome")]
        public string? Outcome { get; set; }
    }
}