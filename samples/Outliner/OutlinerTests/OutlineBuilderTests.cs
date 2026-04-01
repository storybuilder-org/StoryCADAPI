using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Outliner;
using Outliner.Services;
using StoryCAD.Models;
using StoryCAD.Services.API;

namespace OutlinerTests
{
    [TestClass]
    public class OutlineBuilderTests
    {
        private Mock<StoryCADApi> _mockApi;
        private OutlineBuilder _outlineBuilder;
        private OnePassResponse _testResponse;
        private string _testOutputPath;

        [TestInitialize]
        public void Setup()
        {
            _mockApi = new Mock<StoryCADApi>();
            _outlineBuilder = new OutlineBuilder(_mockApi.Object);
            _testOutputPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "test_outline.stbx");

            // Create a sample response for testing
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
                        Guid = "char-guid-1",
                        Name = "Hero",
                        Role = "Protagonist",
                        CharacterSketch = "A brave hero"
                    }
                },
                Settings = new List<SettingElement>
                {
                    new SettingElement
                    {
                        Guid = "setting-guid-1",
                        Name = "Castle",
                        Summary = "A medieval castle"
                    }
                },
                Scenes = new List<SceneElement>
                {
                    new SceneElement
                    {
                        Guid = "scene-guid-1",
                        Name = "Opening Scene",
                        Description = "The story begins",
                        Protagonist = "char-guid-1",
                        Setting = "setting-guid-1",
                        Cast = new List<string> { "char-guid-1" }
                    }
                },
                Problems = new List<ProblemElement>
                {
                    new ProblemElement
                    {
                        Guid = "problem-guid-1",
                        Name = "Main Conflict",
                        StoryQuestion = "Will the hero succeed?",
                        Protagonist = "char-guid-1"
                    }
                }
            };
        }

        [TestMethod]
        public void Constructor_NullApi_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new OutlineBuilder(null));
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_NullResponse_ThrowsArgumentNullException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await _outlineBuilder.BuildOutlineFromResponse(null, _testOutputPath));
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_ValidResponse_CreatesOutline()
        {
            // Arrange
            var rootGuid = Guid.NewGuid();
            var charGuid = Guid.NewGuid();

            _mockApi.Setup(x => x.CreateEmptyOutline(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<List<Guid>>
                {
                    IsSuccess = true,
                    Payload = new List<Guid> { rootGuid }
                });

            _mockApi.Setup(x => x.AddElement(
                It.IsAny<StoryItemType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()))
                .Returns(new OperationResult<Guid>
                {
                    IsSuccess = true,
                    Payload = charGuid
                });

            _mockApi.Setup(x => x.WriteOutline(It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<string>
                {
                    IsSuccess = true,
                    Payload = _testOutputPath
                });

            // Act
            var result = await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            // Assert
            Assert.IsTrue(result);
            _mockApi.Verify(x => x.CreateEmptyOutline("Test Story", "Test Author", "0"), Times.Once);
            _mockApi.Verify(x => x.WriteOutline(_testOutputPath), Times.Once);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_CreateOverviewFails_ReturnsFalse()
        {
            // Arrange
            _mockApi.Setup(x => x.CreateEmptyOutline(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<List<Guid>>
                {
                    IsSuccess = false
                });

            // Act
            var result = await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            // Assert
            Assert.IsFalse(result);
            _mockApi.Verify(x => x.CreateEmptyOutline(It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            // Should not proceed to add elements
            _mockApi.Verify(x => x.AddElement(
                It.IsAny<StoryItemType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_EmptyRootGuid_ReturnsFalse()
        {
            // Arrange
            _mockApi.Setup(x => x.CreateEmptyOutline(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<List<Guid>>
                {
                    IsSuccess = true,
                    Payload = new List<Guid> { Guid.Empty }
                });

            // Act
            var result = await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_NullOverview_UsesDefaults()
        {
            // Arrange
            _testResponse.StoryOverview = null;
            var rootGuid = Guid.NewGuid();

            _mockApi.Setup(x => x.CreateEmptyOutline(
                "Working Title",
                "Unknown Author",
                "0"))
                .ReturnsAsync(new OperationResult<List<Guid>>
                {
                    IsSuccess = true,
                    Payload = new List<Guid> { rootGuid }
                });

            _mockApi.Setup(x => x.WriteOutline(It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<string>
                {
                    IsSuccess = true,
                    Payload = _testOutputPath
                });

            // Act
            var result = await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            // Assert
            Assert.IsTrue(result);
            _mockApi.Verify(x => x.CreateEmptyOutline("Working Title", "Unknown Author", "0"), Times.Once);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_WithPremise_UpdatesOverview()
        {
            // Arrange
            var rootGuid = Guid.NewGuid();
            _testResponse.StoryOverview.Premise = "An epic tale of adventure";

            _mockApi.Setup(x => x.CreateEmptyOutline(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<List<Guid>>
                {
                    IsSuccess = true,
                    Payload = new List<Guid> { rootGuid }
                });

            _mockApi.Setup(x => x.WriteOutline(It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<string>
                {
                    IsSuccess = true,
                    Payload = _testOutputPath
                });

            // Act
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            // Assert
            _mockApi.Verify(x => x.UpdateElementProperties(
                rootGuid,
                It.Is<Dictionary<string, object>>(d =>
                    d.ContainsKey("Premise") &&
                    d["Premise"].ToString() == "An epic tale of adventure")),
                Times.Once);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_SaveFails_ReturnsFalse()
        {
            // Arrange
            var rootGuid = Guid.NewGuid();

            _mockApi.Setup(x => x.CreateEmptyOutline(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<List<Guid>>
                {
                    IsSuccess = true,
                    Payload = new List<Guid> { rootGuid }
                });

            _mockApi.Setup(x => x.WriteOutline(It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<string>
                {
                    IsSuccess = false
                });

            // Act
            var result = await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            // Assert
            Assert.IsFalse(result);
            _mockApi.Verify(x => x.WriteOutline(_testOutputPath), Times.Once);
        }

        [TestMethod]
        public async Task BuildOutlineFromResponse_AddsCharactersWithLLMGuids()
        {
            // Arrange
            var rootGuid = Guid.NewGuid();

            _mockApi.Setup(x => x.CreateEmptyOutline(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<List<Guid>>
                {
                    IsSuccess = true,
                    Payload = new List<Guid> { rootGuid }
                });

            string capturedGuidOverride = null;
            _mockApi.Setup(x => x.AddElement(
                StoryItemType.Character,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()))
                .Callback<StoryItemType, string, string, Dictionary<string, object>, string>(
                    (type, parent, name, props, guidOverride) =>
                    {
                        if (type == StoryItemType.Character)
                            capturedGuidOverride = guidOverride;
                    })
                .Returns(new OperationResult<Guid>
                {
                    IsSuccess = true,
                    Payload = Guid.NewGuid()
                });

            _mockApi.Setup(x => x.WriteOutline(It.IsAny<string>()))
                .ReturnsAsync(new OperationResult<string>
                {
                    IsSuccess = true,
                    Payload = _testOutputPath
                });

            // Act
            await _outlineBuilder.BuildOutlineFromResponse(_testResponse, _testOutputPath);

            // Assert
            Assert.AreEqual("char-guid-1", capturedGuidOverride);
        }
    }
}