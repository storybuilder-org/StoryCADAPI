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

            // Update the story overview with all properties from LLM
            if (overview != null)
            {
                var props = new Dictionary<string, object>();

                if (!string.IsNullOrWhiteSpace(overview.Title))
                    props["Title"] = overview.Title;
                if (!string.IsNullOrWhiteSpace(overview.Author))
                    props["Author"] = overview.Author;
                if (!string.IsNullOrWhiteSpace(overview.Premise))
                    props["Premise"] = overview.Premise;

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
        /// </summary>
        private Dictionary<string, object> BuildCharacterProperties(CharacterElement character)
        {
            return new Dictionary<string, object>
            {
                ["Name"] = character.Name ?? "",
                ["CharacterSketch"] = character.CharacterSketch ?? "",
                ["Role"] = character.Role ?? "",
                ["Age"] = character.Age ?? "",
                ["Sex"] = character.Sex ?? "",
                ["Eyes"] = character.Eyes ?? "",
                ["Hair"] = character.Hair ?? "",
                ["Weight"] = character.Weight ?? "",
                ["Health"] = character.Health ?? "",
                ["PhysNotes"] = character.PhysNotes ?? "",
                ["Appearance"] = character.Appearance ?? "",
                ["Ethnic"] = character.Ethnic ?? "",
                ["Religion"] = character.Religion ?? "",
                ["Education"] = character.Education ?? "",
                ["Focus"] = character.Focus ?? "",
                ["PsychNotes"] = character.PsychNotes ?? "",
                ["Flaw"] = character.Flaw ?? "",
                ["BackStory"] = character.BackStory ?? "",
                ["Relationships"] = character.Relationships ?? ""
            };
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
        /// </summary>
        private Dictionary<string, object> BuildSettingProperties(SettingElement setting)
        {
            return new Dictionary<string, object>
            {
                ["Name"] = setting.Name ?? "",
                ["Summary"] = setting.Summary ?? "",
                ["Locale"] = setting.Locale ?? "",
                ["Season"] = setting.Season ?? "",
                ["Period"] = setting.Period ?? "",
                ["Lighting"] = setting.Lighting ?? "",
                ["Sights"] = setting.Sights ?? "",
                ["Sounds"] = setting.Sounds ?? "",
                ["Touch"] = setting.Touch ?? "",
                ["SmellTaste"] = setting.SmellTaste ?? ""
            };
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
        /// </summary>
        private Dictionary<string, object> BuildProblemProperties(ProblemElement problem)
        {
            return new Dictionary<string, object>
            {
                ["Name"] = problem.Name ?? "",
                ["StoryQuestion"] = problem.StoryQuestion ?? "",
                ["ProblemType"] = problem.ProblemType ?? "",
                ["ConflictType"] = problem.ConflictType ?? "",
                ["ProblemCategory"] = problem.ProblemCategory ?? "",
                ["ProblemSource"] = problem.ProblemSource ?? "",
                ["ProtGoal"] = problem.ProtGoal ?? "",
                ["Significance"] = problem.Significance ?? "",
                ["AntagGoal"] = problem.AntagGoal ?? "",
                ["AntagMotive"] = problem.AntagMotive ?? "",
                ["Outcome"] = problem.Outcome ?? ""
            };
        }


        /// <summary>
        /// Adds all scenes to the outline.
        /// Cross-references use LLM-provided GUIDs directly.
        /// </summary>
        private void AddScenes(List<SceneElement> scenes)
        {
            foreach (var scene in scenes)
            {
                var props = BuildSceneProperties(scene);

                // Add all cross-references using LLM GUIDs directly
                if (!string.IsNullOrEmpty(scene.Protagonist))
                    props["Protagonist"] = scene.Protagonist;
                if (!string.IsNullOrEmpty(scene.Antagonist))
                    props["Antagonist"] = scene.Antagonist;
                if (!string.IsNullOrEmpty(scene.ViewpointCharacter))
                    props["ViewpointCharacter"] = scene.ViewpointCharacter;
                if (!string.IsNullOrEmpty(scene.Setting))
                    props["Setting"] = scene.Setting;
                if (scene.Cast != null && scene.Cast.Count > 0)
                    props["Cast"] = scene.Cast;

                var result = _api.AddElement(
                    StoryItemType.Scene,
                    _rootGuid.ToString(),
                    scene.Name ?? "Unnamed Scene",
                    props,
                    scene.Guid);  // Use the LLM-provided GUID

                if (result != null && result.IsSuccess)
                {
                    _guidMapping[scene.Guid] = Guid.Parse(scene.Guid);
                }
            }
        }

        /// <summary>
        /// Builds property dictionary for a scene element.
        /// </summary>
        private Dictionary<string, object> BuildSceneProperties(SceneElement scene)
        {
            return new Dictionary<string, object>
            {
                ["Name"] = scene.Name ?? "",
                ["Description"] = scene.Description ?? "",
                ["ProtGoal"] = scene.ProtGoal ?? "",
                ["Significance"] = scene.Significance ?? "",
                ["AntagGoal"] = scene.AntagGoal ?? "",
                ["AntagMotive"] = scene.AntagMotive ?? "",
                ["Outcome"] = scene.Outcome ?? ""
            };
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