using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Outliner;
using Outliner.Services;

namespace OutlinerTests
{
    [TestClass]
    public class ProseAnalyzerTests
    {
        private Mock<Kernel> _mockKernel;
        private Mock<IChatCompletionService> _mockChatService;
        private ProseAnalyzer _proseAnalyzer;

        [TestInitialize]
        public void Setup()
        {
            _mockKernel = new Mock<Kernel>();
            _mockChatService = new Mock<IChatCompletionService>();
            _proseAnalyzer = new ProseAnalyzer(_mockKernel.Object, _mockChatService.Object);
        }

        [TestMethod]
        public void Constructor_NullKernel_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ProseAnalyzer(null, _mockChatService.Object));
        }

        [TestMethod]
        public void Constructor_NullChatService_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ProseAnalyzer(_mockKernel.Object, null));
        }

        [TestMethod]
        public void ValidateProseLength_EmptyText_ReturnsFalse()
        {
            var result = _proseAnalyzer.ValidateProseLength("");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateProseLength_NullText_ReturnsFalse()
        {
            var result = _proseAnalyzer.ValidateProseLength(null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateProseLength_ShortText_ReturnsTrue()
        {
            var shortText = "This is a short story about a cat.";
            var result = _proseAnalyzer.ValidateProseLength(shortText);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidateProseLength_VeryLongText_ReturnsFalse()
        {
            // Create text that exceeds token limit (roughly 4 million characters)
            var veryLongText = new string('a', 5000000);
            var result = _proseAnalyzer.ValidateProseLength(veryLongText);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EstimateTokenCount_EmptyText_ReturnsZero()
        {
            var count = _proseAnalyzer.EstimateTokenCount("");
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void EstimateTokenCount_SimpleText_ReturnsExpectedEstimate()
        {
            // 40 characters should estimate to approximately 10 tokens (40/4)
            var text = "This is a test text with forty characters.";
            var count = _proseAnalyzer.EstimateTokenCount(text);
            Assert.IsTrue(count >= 9 && count <= 11); // Allow for rounding
        }

        [TestMethod]
        public void LoadSystemPrompt_NoCustomPath_ReturnsDefaultPrompt()
        {
            var prompt = _proseAnalyzer.LoadSystemPrompt();
            Assert.IsNotNull(prompt);
            Assert.IsTrue(prompt.Contains("One-Pass Analysis System Prompt"));
        }

        [TestMethod]
        public void LoadSystemPrompt_InvalidPath_ReturnsDefaultPrompt()
        {
            var prompt = _proseAnalyzer.LoadSystemPrompt("/invalid/path/to/prompt.md");
            Assert.IsNotNull(prompt);
            Assert.IsTrue(prompt.Contains("One-Pass Analysis System Prompt"));
        }

        [TestMethod]
        public async Task AnalyzeProse_EmptyText_ThrowsArgumentException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await _proseAnalyzer.AnalyzeProse(""));
        }

        [TestMethod]
        public async Task AnalyzeProse_NullText_ThrowsArgumentException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await _proseAnalyzer.AnalyzeProse(null));
        }

        [TestMethod]
        public async Task AnalyzeProse_TextTooLong_ThrowsInvalidOperationException()
        {
            var veryLongText = new string('a', 5000000);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                await _proseAnalyzer.AnalyzeProse(veryLongText));
        }

        [TestMethod]
        public async Task AnalyzeProse_ValidResponse_ReturnsOnePassResponse()
        {
            // Arrange
            var proseText = "A short story about a brave knight.";
            var mockResponse = @"{
                ""story_overview"": {
                    ""title"": ""The Brave Knight"",
                    ""author"": ""Test Author"",
                    ""premise"": ""A knight faces challenges""
                },
                ""characters"": [{
                    ""guid"": ""550e8400-e29b-41d4-a716-446655440000"",
                    ""name"": ""Sir Knight"",
                    ""role"": ""Protagonist""
                }],
                ""settings"": [],
                ""scenes"": [],
                ""problems"": []
            }";

            var mockChatContent = Mock.Of<ChatMessageContent>(c => c.Content == mockResponse);
            _mockChatService.Setup(x => x.GetChatMessageContentAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockChatContent);

            // Act
            var result = await _proseAnalyzer.AnalyzeProse(proseText);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.StoryOverview);
            Assert.AreEqual("The Brave Knight", result.StoryOverview.Title);
            Assert.AreEqual("Test Author", result.StoryOverview.Author);
            Assert.IsNotNull(result.Characters);
            Assert.AreEqual(1, result.Characters.Count);
            Assert.AreEqual("Sir Knight", result.Characters[0].Name);
        }

        [TestMethod]
        public async Task AnalyzeProse_EmptyLLMResponse_ThrowsInvalidOperationException()
        {
            // Arrange
            var proseText = "A short story.";
            var mockChatContent = Mock.Of<ChatMessageContent>(c => c.Content == "");
            _mockChatService.Setup(x => x.GetChatMessageContentAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockChatContent);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                await _proseAnalyzer.AnalyzeProse(proseText));
        }

        [TestMethod]
        public async Task AnalyzeProse_InvalidJSON_ThrowsInvalidOperationException()
        {
            // Arrange
            var proseText = "A short story.";
            var mockChatContent = Mock.Of<ChatMessageContent>(c => c.Content == "Not valid JSON");
            _mockChatService.Setup(x => x.GetChatMessageContentAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockChatContent);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                await _proseAnalyzer.AnalyzeProse(proseText));
        }

        [TestMethod]
        public async Task AnalyzeProse_MissingStoryOverview_ThrowsInvalidOperationException()
        {
            // Arrange
            var proseText = "A short story.";
            var mockResponse = @"{
                ""characters"": [],
                ""settings"": [],
                ""scenes"": [],
                ""problems"": []
            }";

            var mockChatContent = Mock.Of<ChatMessageContent>(c => c.Content == mockResponse);
            _mockChatService.Setup(x => x.GetChatMessageContentAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockChatContent);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                await _proseAnalyzer.AnalyzeProse(proseText));
        }

        [TestMethod]
        public async Task AnalyzeProse_AutoGeneratesGUIDs_WhenMissing()
        {
            // Arrange
            var proseText = "A short story.";
            var mockResponse = @"{
                ""story_overview"": {
                    ""title"": ""Test"",
                    ""author"": ""Author""
                },
                ""characters"": [{
                    ""name"": ""Character Without GUID""
                }],
                ""settings"": [],
                ""scenes"": [],
                ""problems"": []
            }";

            var mockChatContent = Mock.Of<ChatMessageContent>(c => c.Content == mockResponse);
            _mockChatService.Setup(x => x.GetChatMessageContentAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockChatContent);

            // Act
            var result = await _proseAnalyzer.AnalyzeProse(proseText);

            // Assert
            Assert.IsNotNull(result.Characters[0].Guid);
            Assert.IsTrue(Guid.TryParse(result.Characters[0].Guid, out _));
        }
    }
}