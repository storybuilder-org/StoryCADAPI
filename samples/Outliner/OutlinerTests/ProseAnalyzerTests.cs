using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Outliner.Services;

namespace OutlinerTests
{
    [TestClass]
    public class ProseAnalyzerTests
    {
        private Kernel _kernel;
        private Mock<IChatCompletionService> _mockChatService;
        private ProseAnalyzer _proseAnalyzer;

        [TestInitialize]
        public void Setup()
        {
            _kernel = Kernel.CreateBuilder().Build();
            _mockChatService = new Mock<IChatCompletionService>();
            _proseAnalyzer = new ProseAnalyzer(_kernel, _mockChatService.Object);
        }

        [TestMethod]
        public void Constructor_NullKernel_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new ProseAnalyzer(null, _mockChatService.Object));
        }

        [TestMethod]
        public void Constructor_NullChatService_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new ProseAnalyzer(_kernel, null));
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
            var text = "This is a test text with forty characters.";
            var count = _proseAnalyzer.EstimateTokenCount(text);
            Assert.IsTrue(count >= 9 && count <= 11);
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
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
                await _proseAnalyzer.AnalyzeProse(""));
        }

        [TestMethod]
        public async Task AnalyzeProse_NullText_ThrowsArgumentException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
                await _proseAnalyzer.AnalyzeProse(null));
        }

        [TestMethod]
        public async Task AnalyzeProse_TextTooLong_ThrowsInvalidOperationException()
        {
            var veryLongText = new string('a', 5000000);
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
                await _proseAnalyzer.AnalyzeProse(veryLongText));
        }
    }
}
