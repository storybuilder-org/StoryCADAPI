using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using StoryCADLib.Models;
using StoryCADLib.Services.API;

namespace Outliner.Services
{
    /// <summary>
    /// Service responsible for Phase 2: Building StoryCAD outlines from analyzed prose.
    /// Takes the structured data from the LLM analysis and constructs a .stbx outline
    /// using the StoryCAD API.
    /// </summary>
    public class OutlineBuilder
    {
        private readonly StoryCADApi _api;
        private Guid _rootGuid;
        private readonly Dictionary<string, Guid> _guidMapping;

        public OutlineBuilder(StoryCADApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _guidMapping = new Dictionary<string, Guid>();
        }

        /// <summary>
        /// Builds a complete StoryCAD outline from the LLM's single-pass response.
        /// Processing order: Overview → Characters → Settings → Scenes → Problems
        /// </summary>
        /// <param name="response">The analyzed story structure from the LLM</param>
        /// <param name="outputPath">Path where the .stbx file will be saved</param>
        /// <returns>True if outline was created successfully</returns>
        public async Task<bool> BuildOutlineFromResponse(OnePassResponse response, string outputPath)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            try
            {
                // Phase 1: Create the root story overview
                if (!await CreateStoryOverview(response.StoryOverview))
                    return false;

                // Phase 2: Add all story elements in dependency order
                // Characters first as they're referenced by scenes and problems
                if (response.Characters != null)
                    AddCharacters(response.Characters);

                // Settings next as scenes reference them
                if (response.Settings != null)
                    AddSettings(response.Settings);

                // Problems before scenes (problems can exist independently)
                if (response.Problems != null)
                    AddProblems(response.Problems);

                // Scenes last as they can reference characters, settings, and problems
                if (response.Scenes != null)
                    AddScenes(response.Scenes);

                // Save the outline to disk
                return await SaveOutline(outputPath);
            }
            catch (Exception ex)
            {
                // Log error but attempt to save partial outline
                Console.WriteLine($"Error building outline: {ex.Message}");
                await SaveOutline(outputPath); // Try to save what we have
                return false;
            }
        }

        /// <summary>
        /// Creates the root story overview element.
        /// This must succeed for the outline to be valid.
        /// </summary>
        private async Task<bool> CreateStoryOverview(StoryOverviewElement? overview)
        {
            var title = overview?.Title ?? "Working Title";
            var author = overview?.Author ?? "Unknown Author";

            var result = await _api.CreateEmptyOutline(title, author, "0");
            if (result == null || !result.IsSuccess)
                return false;

            _rootGuid = result.Payload.FirstOrDefault();
            if (_rootGuid == Guid.Empty)
                return false;

            // Update the story overview. Title is the element's Name (already
            // set by CreateEmptyOutline); OverviewModel has no Title property,
            // so passing Title would fail and bail the whole properties update.
            if (overview != null)
            {
                var props = new Dictionary<string, object>();
                AddIfPresent(props, "Author",       overview.Author);
                AddIfPresent(props, "Premise",      overview.Premise);
                AddIfPresent(props, "StoryType",    overview.StoryType);
                AddIfPresent(props, "StoryGenre",   overview.StoryGenre);
                AddIfPresent(props, "StoryProblem", overview.StoryProblem);
                AddIfPresent(props, "Concept",      overview.Concept);

                if (props.Count > 0)
                {
                    _api.UpdateElementProperties(_rootGuid, props);
                }
            }

            return true;
        }

        /// <summary>
        /// Adds all characters to the outline.
        /// Uses LLM-provided GUIDs to maintain cross-references.
        /// </summary>
        private void AddCharacters(List<CharacterElement> characters)
        {
            foreach (var character in characters)
            {
                var props = BuildCharacterProperties(character);
                var result = _api.AddElement(
                    StoryItemType.Character,
                    _rootGuid.ToString(),
                    character.Name ?? "Unnamed Character",
                    props,
                    character.Guid);  // Use the LLM-provided GUID

                if (result != null && result.IsSuccess)
                {
                    // Map the LLM-generated GUID to itself (since we're using it as override)
                    _guidMapping[character.Guid] = Guid.Parse(character.Guid);
                }
            }
        }

        /// <summary>
        /// Builds property dictionary for a character element.
        /// Uses only fields the StoryCAD CharacterModel actually has. Name is
        /// set by AddElement separately and should not be in the dictionary.
        /// CharacterSketch (LLM-only) is folded into Notes.
        /// Relationships is a string from the LLM; the StoryCAD field is
        /// RelationshipList (a List), which requires AddCollectionEntry — not
        /// settable via UpdateElementProperties — so we surface the LLM string
        /// in Notes for now.
        /// </summary>
        private Dictionary<string, object> BuildCharacterProperties(CharacterElement character)
        {
            var props = new Dictionary<string, object>();
            AddIfPresent(props, "Role",       character.Role);
            AddIfPresent(props, "Age",        character.Age);
            AddIfPresent(props, "Sex",        character.Sex);
            AddIfPresent(props, "Eyes",       character.Eyes);
            AddIfPresent(props, "Hair",       character.Hair);
            AddIfPresent(props, "Weight",     character.Weight);
            AddIfPresent(props, "Health",     character.Health);
            AddIfPresent(props, "PhysNotes",  character.PhysNotes);
            AddIfPresent(props, "Appearance", character.Appearance);
            AddIfPresent(props, "Ethnic",     character.Ethnic);
            AddIfPresent(props, "Religion",   character.Religion);
            AddIfPresent(props, "Education",  character.Education);
            AddIfPresent(props, "Focus",      character.Focus);
            AddIfPresent(props, "PsychNotes", character.PsychNotes);
            AddIfPresent(props, "Flaw",       character.Flaw);
            AddIfPresent(props, "BackStory",  character.BackStory);

            var sketchAndRelationships = string.Join("\n\n",
                new[] { character.CharacterSketch, character.Relationships }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
            if (!string.IsNullOrWhiteSpace(sketchAndRelationships))
                props["Notes"] = sketchAndRelationships;

            return props;
        }

        private static void AddIfPresent(Dictionary<string, object> props, string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                props[key] = value;
        }

        /// <summary>
        /// Adds all settings to the outline.
        /// Uses LLM-provided GUIDs to maintain cross-references.
        /// </summary>
        private void AddSettings(List<SettingElement> settings)
        {
            foreach (var setting in settings)
            {
                var props = BuildSettingProperties(setting);
                var result = _api.AddElement(
                    StoryItemType.Setting,
                    _rootGuid.ToString(),
                    setting.Name ?? "Unnamed Setting",
                    props,
                    setting.Guid);  // Use the LLM-provided GUID

                if (result != null && result.IsSuccess)
                {
                    // Map the LLM-generated GUID to itself
                    _guidMapping[setting.Guid] = Guid.Parse(setting.Guid);
                }
            }
        }

        /// <summary>
        /// Builds property dictionary for a setting element.
        /// SettingModel has no Summary field; the LLM's summary is folded into Notes.
        /// </summary>
        private Dictionary<string, object> BuildSettingProperties(SettingElement setting)
        {
            var props = new Dictionary<string, object>();
            AddIfPresent(props, "Locale",     setting.Locale);
            AddIfPresent(props, "Season",     setting.Season);
            AddIfPresent(props, "Period",     setting.Period);
            AddIfPresent(props, "Lighting",   setting.Lighting);
            AddIfPresent(props, "Sights",     setting.Sights);
            AddIfPresent(props, "Sounds",     setting.Sounds);
            AddIfPresent(props, "Touch",      setting.Touch);
            AddIfPresent(props, "SmellTaste", setting.SmellTaste);
            AddIfPresent(props, "Notes",      setting.Summary);
            return props;
        }

        /// <summary>
        /// Adds all problems to the outline.
        /// Character references use LLM-provided GUIDs directly.
        /// </summary>
        private void AddProblems(List<ProblemElement> problems)
        {
            foreach (var problem in problems)
            {
                var props = BuildProblemProperties(problem);

                // Add protagonist and antagonist references directly
                if (!string.IsNullOrEmpty(problem.Protagonist))
                    props["Protagonist"] = problem.Protagonist;
                if (!string.IsNullOrEmpty(problem.Antagonist))
                    props["Antagonist"] = problem.Antagonist;

                var result = _api.AddElement(
                    StoryItemType.Problem,
                    _rootGuid.ToString(),
                    problem.Name ?? "Unnamed Problem",
                    props,
                    problem.Guid);  // Use the LLM-provided GUID

                if (result != null && result.IsSuccess)
                {
                    _guidMapping[problem.Guid] = Guid.Parse(problem.Guid);
                }
            }
        }

        /// <summary>
        /// Builds property dictionary for a problem element.
        /// ProblemModel has no StoryQuestion or Significance fields; the LLM's
        /// StoryQuestion is folded into Notes.
        /// </summary>
        private Dictionary<string, object> BuildProblemProperties(ProblemElement problem)
        {
            var props = new Dictionary<string, object>();
            AddIfPresent(props, "ProblemType",      problem.ProblemType);
            AddIfPresent(props, "ConflictType",     problem.ConflictType);
            AddIfPresent(props, "ProblemCategory",  problem.ProblemCategory);
            AddIfPresent(props, "Subject",          problem.Subject);
            AddIfPresent(props, "ProblemSource",    problem.ProblemSource);
            AddIfPresent(props, "ProtGoal",         problem.ProtGoal);
            AddIfPresent(props, "ProtMotive",       problem.ProtMotive);
            AddIfPresent(props, "ProtConflict",     problem.ProtConflict);
            AddIfPresent(props, "AntagGoal",        problem.AntagGoal);
            AddIfPresent(props, "AntagMotive",      problem.AntagMotive);
            AddIfPresent(props, "AntagConflict",    problem.AntagConflict);
            AddIfPresent(props, "Outcome",          problem.Outcome);
            AddIfPresent(props, "Method",           problem.Method);
            AddIfPresent(props, "Theme",            problem.Theme);
            AddIfPresent(props, "Notes",            problem.StoryQuestion);
            return props;
        }


        /// <summary>
        /// Adds all scenes to the outline.
        /// CastMembers is List<Guid> on SceneModel and must be populated via
        /// AddCollectionEntry rather than UpdateElementProperties.
        /// </summary>
        private void AddScenes(List<SceneElement> scenes)
        {
            foreach (var scene in scenes)
            {
                var props = BuildSceneProperties(scene);

                AddIfPresent(props, "Protagonist",        scene.Protagonist);
                AddIfPresent(props, "Antagonist",         scene.Antagonist);
                AddIfPresent(props, "ViewpointCharacter", scene.ViewpointCharacter);
                AddIfPresent(props, "Setting",            scene.Setting);

                var result = _api.AddElement(
                    StoryItemType.Scene,
                    _rootGuid.ToString(),
                    scene.Name ?? "Unnamed Scene",
                    props,
                    scene.Guid);

                if (result != null && result.IsSuccess)
                {
                    _guidMapping[scene.Guid] = Guid.Parse(scene.Guid);

                    if (scene.Cast != null)
                    {
                        foreach (var castGuid in scene.Cast)
                        {
                            if (!string.IsNullOrWhiteSpace(castGuid))
                                _api.AddCollectionEntry(Guid.Parse(scene.Guid), "CastMembers", castGuid);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds property dictionary for a scene element.
        /// SceneModel uses ProtagGoal (not ProtGoal); has no Significance or
        /// AntagMotive fields, so those LLM values are folded into Notes.
        /// </summary>
        private Dictionary<string, object> BuildSceneProperties(SceneElement scene)
        {
            var props = new Dictionary<string, object>();
            AddIfPresent(props, "Description", scene.Description);
            AddIfPresent(props, "ProtagGoal",  scene.ProtGoal);
            AddIfPresent(props, "AntagGoal",   scene.AntagGoal);
            AddIfPresent(props, "Outcome",     scene.Outcome);

            var notes = string.Join("\n\n",
                new[] { scene.Significance, scene.AntagMotive }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
            if (!string.IsNullOrWhiteSpace(notes))
                props["Notes"] = notes;

            return props;
        }


        /// <summary>
        /// Saves the completed outline to disk.
        /// </summary>
        private async Task<bool> SaveOutline(string outputPath)
        {
            try
            {
                var result = await _api.WriteOutline(outputPath);
                return result != null && result.IsSuccess;
            }
            catch
            {
                return false;
            }
        }
    }
}