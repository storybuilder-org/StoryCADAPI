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
    }
}
