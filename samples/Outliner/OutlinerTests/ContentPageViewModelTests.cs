using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Outliner;
using Outliner.Services;

namespace OutlinerTests
{
    /// <summary>
    /// VM-level tests for ContentPageViewModel — no mocks, no LLM, no UI hosting.
    /// State is set directly to the values that file pickers and successful
    /// outline runs would have produced; observable behaviour is asserted.
    /// </summary>
    [TestClass]
    public class ContentPageViewModelTests
    {
        [TestMethod]
        public void NewVm_FeedbackPanelStartsHidden()
        {
            var vm = new ContentPageViewModel();
            Assert.IsFalse(vm.ShowFeedbackPanel);
            Assert.AreEqual(Visibility.Collapsed, vm.FeedbackVisibility);
        }

        [TestMethod]
        public void ShowFeedbackPanel_True_FlipsVisibilityToVisible()
        {
            var vm = new ContentPageViewModel();
            vm.ShowFeedbackPanel = true;
            Assert.AreEqual(Visibility.Visible, vm.FeedbackVisibility);
        }

        [TestMethod]
        public void CreateOutlineCommand_DisabledWhenStoryTextEmpty()
        {
            var vm = new ContentPageViewModel { StoryText = string.Empty };
            Assert.IsFalse(vm.CreateOutlineCommand.CanExecute(null));
        }

        [TestMethod]
        public void CreateOutlineCommand_DisabledWhileProcessing()
        {
            var vm = new ContentPageViewModel
            {
                StoryText = "some prose",
                IsProcessing = true
            };
            Assert.IsFalse(vm.CreateOutlineCommand.CanExecute(null));
        }

        [TestMethod]
        public void SubmitFeedback_NoState_DoesNothing()
        {
            var vm = new ContentPageViewModel();
            vm.UserFeedback = "ignored";
            // LastResponse and LastOutputPath are null — submit should noop
            vm.SubmitFeedback("thumbs_up");
            Assert.AreEqual(string.Empty, vm.FeedbackStatus);
        }

        [TestMethod]
        public void SubmitFeedback_WithState_WritesRatingFile_HidesPanel()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), $"outliner-test-{System.Guid.NewGuid():N}");
            Directory.CreateDirectory(tmpDir);
            var outputPath = Path.Combine(tmpDir, "story.stbx");

            var vm = new ContentPageViewModel
            {
                ShowFeedbackPanel = true,
                UserFeedback = "great outline"
            };
            // Pre-populate the post-success state that CreateOutlineAsync would set.
            vm.LastResponse = new OnePassResponse
            {
                StoryOverview = new StoryOverviewElement { Title = "T", Author = "A" }
            };
            vm.LastCost = new OutlineCost { ModelId = "gpt-4o" };
            vm.LastOutputPath = outputPath;

            vm.SubmitFeedback("thumbs_up");

            var ratingPath = Path.ChangeExtension(outputPath, ".rating.json");
            Assert.IsTrue(File.Exists(ratingPath), $"Rating file missing: {ratingPath}");
            Assert.IsFalse(vm.ShowFeedbackPanel);
            StringAssert.Contains(vm.FeedbackStatus, "Thanks");

            Directory.Delete(tmpDir, recursive: true);
        }
    }
}
