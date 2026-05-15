using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Outliner;
using Outliner.Services;
using StoryCADLib.Models;
using StoryCADLib.Services.API;

namespace OutlinerTests
{
    [TestClass]
    public class OutlineBuilderTests
    {
        private StoryCADApi _api;
        private OutlineBuilder _outlineBuilder;
        private OnePassResponse _testResponse;
        private string _testOutputPath;

        [TestInitialize]
        public void Setup()
        {
            _api = Ioc.Default.GetRequiredService<StoryCADApi>();
            _outlineBuilder = new OutlineBuilder(_api);
            _testOutputPath = Path.Combine(Path.GetTempPath(), $"test_outline_{Guid.NewGuid()}.stbx");

            _testResponse = new OnePassResponse
            {
                StoryOverview = new StoryOverviewElement
                {
                    Title = "Test Story",
                    Author = "Test Author",
                    Premise = "A test premise"
                },
                Characters = new List<CharacterElement>
                {
                    new CharacterElement
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Name = "Hero",
                        Role = "Protagonist",
                        CharacterSketch = "A brave hero"
                    }
                },
                Settings = new List<SettingElement>
                {
                    new SettingElement
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Name = "Castle",
                        Summary = "A medieval castle"
                    }
                },
                Scenes = new List<SceneElement>
                {
                    new SceneElement
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Name = "Opening Scene",
                        Description = "The story begins",
                        Cast = new List<string>()
                    }
                },
                Problems = new List<ProblemElement>
                {
                    new ProblemElement
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Name = "Main Conflict",
                        StoryQuestion = "Will the hero succeed?"
                    }
                }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (File.Exists(_testOutputPath)) File.Delete(_testOutputPath); }
            catch { }
        }

        [TestMethod]
        public void Constructor_NullApi_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new OutlineBuilder(null));
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_NullResponse_ThrowsArgumentNullException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
                await _outlineBuilder.BuildOutlineFromResponse(null, _testOutputPath));
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_ValidResponse_CreatesOutlineFile()
        {
            var result = await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(_testOutputPath));
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_ValidResponse_ContainsCharacter()
        {
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var elements = _api.GetElementsByType(StoryItemType.Character);
            Assert.IsTrue(elements.IsSuccess);
            Assert.IsTrue(elements.Payload.Count > 0);

            var hero = elements.Payload.Find(e => e.Name == "Hero");
            Assert.IsNotNull(hero, "Character 'Hero' should exist in outline");
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_ValidResponse_ContainsSetting()
        {
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var elements = _api.GetElementsByType(StoryItemType.Setting);
            Assert.IsTrue(elements.IsSuccess);

            var castle = elements.Payload.Find(e => e.Name == "Castle");
            Assert.IsNotNull(castle, "Setting 'Castle' should exist in outline");
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_ValidResponse_ContainsScene()
        {
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var elements = _api.GetElementsByType(StoryItemType.Scene);
            Assert.IsTrue(elements.IsSuccess);

            var scene = elements.Payload.Find(e => e.Name == "Opening Scene");
            Assert.IsNotNull(scene, "Scene 'Opening Scene' should exist in outline");
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_ValidResponse_ContainsProblem()
        {
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var elements = _api.GetElementsByType(StoryItemType.Problem);
            Assert.IsTrue(elements.IsSuccess);

            var problem = elements.Payload.Find(e => e.Name == "Main Conflict");
            Assert.IsNotNull(problem, "Problem 'Main Conflict' should exist in outline");
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_NullOverview_UsesDefaults()
        {
            _testResponse.StoryOverview = null;

            var result = await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(_testOutputPath));
        }

        // Issue #15 — the four labelled fields on Problem/Character/Setting/Scene pages
        // ("Story Question", "Character Sketch", "Setting Summary", "Scene Sketch")
        // all bind to StoryElement.Description on the base class. The OnePassResponse
        // fields character_sketch / summary / story_question must land there.

        [TestMethod]
        public async Task BuildOutlineFromResponse_PopulatesCharacterDescription()
        {
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var hero = _api.GetElementsByType(StoryItemType.Character).Payload.Find(e => e.Name == "Hero");
            Assert.IsNotNull(hero);
            Assert.AreEqual("A brave hero", hero.Description);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_PopulatesSettingDescription()
        {
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var castle = _api.GetElementsByType(StoryItemType.Setting).Payload.Find(e => e.Name == "Castle");
            Assert.IsNotNull(castle);
            Assert.AreEqual("A medieval castle", castle.Description);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_PopulatesProblemDescription()
        {
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var problem = _api.GetElementsByType(StoryItemType.Problem).Payload.Find(e => e.Name == "Main Conflict");
            Assert.IsNotNull(problem);
            Assert.AreEqual("Will the hero succeed?", problem.Description);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_PopulatesSceneDescription()
        {
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var scene = _api.GetElementsByType(StoryItemType.Scene).Payload.Find(e => e.Name == "Opening Scene");
            Assert.IsNotNull(scene);
            Assert.AreEqual("The story begins", scene.Description);
        }

        // Issue #15 — non-Description property round-trips. A failure on a key near the
        // end of OutlineBuilder's property dict is a tell that an earlier key threw and
        // short-circuited the rest (UpdateElementProperty uses reflection and bails on
        // any name that doesn't exist on the model).

        [TestMethod]
        public async Task BuildOutlineFromResponse_PopulatesCharacterAge()
        {
            _testResponse.Characters[0].Age = "30";
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var hero = _api.GetElementsByType(StoryItemType.Character).Payload.Find(e => e.Name == "Hero") as CharacterModel;
            Assert.IsNotNull(hero);
            Assert.AreEqual("30", hero.Age);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_PopulatesSettingLocale()
        {
            _testResponse.Settings[0].Locale = "Castle hall";
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var castle = _api.GetElementsByType(StoryItemType.Setting).Payload.Find(e => e.Name == "Castle") as SettingModel;
            Assert.IsNotNull(castle);
            Assert.AreEqual("Castle hall", castle.Locale);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_PopulatesProblemOutcome()
        {
            _testResponse.Problems[0].Outcome = "Hero wins";
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var problem = _api.GetElementsByType(StoryItemType.Problem).Payload.Find(e => e.Name == "Main Conflict") as ProblemModel;
            Assert.IsNotNull(problem);
            Assert.AreEqual("Hero wins", problem.Outcome);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_PopulatesSceneProtagonistGoal()
        {
            _testResponse.Scenes[0].ProtGoal = "Win the battle";
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var scene = _api.GetElementsByType(StoryItemType.Scene).Payload.Find(e => e.Name == "Opening Scene") as SceneModel;
            Assert.IsNotNull(scene);
            Assert.AreEqual("Win the battle", scene.ProtagGoal);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_PopulatesSceneOutcome()
        {
            _testResponse.Scenes[0].Outcome = "Battle won";
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            var scene = _api.GetElementsByType(StoryItemType.Scene).Payload.Find(e => e.Name == "Opening Scene") as SceneModel;
            Assert.IsNotNull(scene);
            Assert.AreEqual("Battle won", scene.Outcome);
        }
    }
}
