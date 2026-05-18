using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StoryCADLib.Models;
using StoryCADLib.Services.API;

namespace StoryCADCritter
{
    /// <summary>
    /// "The Last Lighthouse Keeper" — the public-domain-safe demo outline the
    /// sample falls back to when run with no .stbx argument. Lives in its own
    /// file so the integration test the issue calls for can reuse it.
    /// </summary>
    internal static class LighthouseKeeperFixture
    {
        public const string Title  = "The Last Lighthouse Keeper";
        public const string Author = "Demo Author";

        /// <summary>
        /// Builds the fixture outline against the given API instance. Returns
        /// the Guid of the Story Overview node so callers can save the
        /// outline (or otherwise navigate it) by hand.
        /// </summary>
        public static async Task<Guid> BuildAsync(StoryCADApi api)
        {
            var createResult = await api.CreateEmptyOutline(Title, Author, "0");
            if (!createResult.IsSuccess)
                throw new InvalidOperationException($"Couldn't create demo outline: {createResult.ErrorMessage}");

            var overviewGuid = createResult.Payload[0];
            var overviewKey = overviewGuid.ToString();

            api.UpdateElementProperties(overviewGuid, new Dictionary<string, object>
            {
                { "Premise", "A woman returns to the lighthouse where her father died alone and discovers the developer who wants it gone has more to lose than she does." },
                { "StoryProblem", "Can Elara save the lighthouse — and forgive herself — before the council vote?" }
            });

            // Characters
            var elara = api.AddElement(StoryItemType.Character, overviewKey, "Elara Marsh",
                new Dictionary<string, object>
                {
                    { "Role",      "Protagonist" },
                    { "Age",       "34" },
                    { "Flaw",      "Cannot trust anyone since her father's betrayal" },
                    { "BackStory", "Grew up in the lighthouse, left for the city, returned after her father's death" }
                });

            var silas = api.AddElement(StoryItemType.Character, overviewKey, "Silas Croft",
                new Dictionary<string, object>
                {
                    { "Role",  "Antagonist" },
                    { "Age",   "50" },
                    { "Flaw",  "Greed disguised as civic duty" }
                });

            var marina = api.AddElement(StoryItemType.Character, overviewKey, "Marina Torres",
                new Dictionary<string, object>
                {
                    { "Role", "Supporting" },
                    { "Age",  "29" }
                });

            // Problems
            var prob1 = api.AddElement(StoryItemType.Problem, overviewKey, "Save the Lighthouse");
            api.UpdateElementProperties(prob1.Payload, new Dictionary<string, object>
            {
                { "Protagonist", elara.Payload.ToString() },
                { "Antagonist",  silas.Payload.ToString() },
                { "ProtGoal",    "Preserve the lighthouse and her father's legacy" },
                { "ProtMotive",  "Guilt over leaving and her father dying alone" },
                { "AntagGoal",   "Demolish the lighthouse for a resort development" },
                { "AntagMotive", "Profit and political ambition" },
                { "Theme",       "Legacy vs. Progress" },
                { "Premise",     "A woman must confront her past to save her father's lighthouse from demolition" }
            });

            var prob2 = api.AddElement(StoryItemType.Problem, overviewKey, "Trust Again");
            api.UpdateElementProperties(prob2.Payload, new Dictionary<string, object>
            {
                { "Protagonist", elara.Payload.ToString() },
                { "ProtGoal",    "Learn to rely on others" },
                { "ProtMotive",  "Loneliness and the realization she cannot fight alone" }
            });

            // Settings
            var lighthouse = api.AddElement(StoryItemType.Setting, overviewKey, "Marsh Point Lighthouse",
                new Dictionary<string, object>
                {
                    { "Locale", "Remote coastal headland" },
                    { "Season", "Late autumn" },
                    { "Period", "Present day" }
                });

            var townHall = api.AddElement(StoryItemType.Setting, overviewKey, "Harborview Town Hall",
                new Dictionary<string, object> { { "Locale", "Small fishing town" } });

            var cafe = api.AddElement(StoryItemType.Setting, overviewKey, "The Anchor Cafe",
                new Dictionary<string, object> { { "Locale", "Small fishing town waterfront" } });

            // Scenes
            var scene1 = api.AddElement(StoryItemType.Scene, overviewKey, "Homecoming");
            api.UpdateElementProperties(scene1.Payload, new Dictionary<string, object>
            {
                { "Setting",     lighthouse.Payload.ToString() },
                { "Protagonist", elara.Payload.ToString() },
                { "Outcome",     "Elara discovers the demolition notice" }
            });
            api.AddCastMember(scene1.Payload, elara.Payload);

            var scene2 = api.AddElement(StoryItemType.Scene, overviewKey, "The Town Meeting");
            api.UpdateElementProperties(scene2.Payload, new Dictionary<string, object>
            {
                { "Setting",     townHall.Payload.ToString() },
                { "Protagonist", elara.Payload.ToString() },
                { "Antagonist",  silas.Payload.ToString() },
                { "Outcome",     "Silas reveals the council vote is in two weeks" }
            });
            api.AddCastMember(scene2.Payload, elara.Payload);
            api.AddCastMember(scene2.Payload, silas.Payload);

            var scene3 = api.AddElement(StoryItemType.Scene, overviewKey, "An Unlikely Ally");
            api.UpdateElementProperties(scene3.Payload, new Dictionary<string, object>
            {
                { "Setting",     cafe.Payload.ToString() },
                { "Protagonist", elara.Payload.ToString() },
                { "Outcome",     "Marina offers to help with the historical preservation filing" }
            });
            api.AddCastMember(scene3.Payload, elara.Payload);
            api.AddCastMember(scene3.Payload, marina.Payload);

            var scene4 = api.AddElement(StoryItemType.Scene, overviewKey, "Father's Secret");
            api.UpdateElementProperties(scene4.Payload, new Dictionary<string, object>
            {
                { "Setting",     lighthouse.Payload.ToString() },
                { "Protagonist", elara.Payload.ToString() },
                { "Outcome",     "Elara finds her father's journal revealing why he stayed" }
            });
            api.AddCastMember(scene4.Payload, elara.Payload);

            var scene5 = api.AddElement(StoryItemType.Scene, overviewKey, "The Council Vote");
            api.UpdateElementProperties(scene5.Payload, new Dictionary<string, object>
            {
                { "Setting",     townHall.Payload.ToString() },
                { "Protagonist", elara.Payload.ToString() },
                { "Antagonist",  silas.Payload.ToString() },
                { "Outcome",     "The vote is tied; Elara must make a personal appeal" }
            });
            api.AddCastMember(scene5.Payload, elara.Payload);
            api.AddCastMember(scene5.Payload, silas.Payload);
            api.AddCastMember(scene5.Payload, marina.Payload);

            return overviewGuid;
        }
    }
}
