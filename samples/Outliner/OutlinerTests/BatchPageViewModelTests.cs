using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Outliner;
using Outliner.Services;

namespace OutlinerTests
{
    /// <summary>
    /// VM-level tests for BatchPageViewModel. No mocks; we set folder state
    /// directly to whatever the FolderPicker would have produced and assert
    /// what the VM does with it.
    /// </summary>
    [TestClass]
    public class BatchPageViewModelTests
    {
        private static BatchPageViewModel BuildVm(OutlinerPreferences? prefs = null)
        {
            // OutlineRunner isn't exercised by these VM tests, but the parameterless
            // constructor would pull dependencies from Ioc that may not be present in
            // a no-API-key test environment. Pass null prefs path through real types.
            return new BatchPageViewModel(
                runner: null!,
                prefs: prefs ?? new OutlinerPreferences(),
                prefsService: new PreferencesService());
        }

        [TestMethod]
        public void NewVm_NoLastFolder_HasEmptyItems()
        {
            var vm = BuildVm();
            Assert.AreEqual(0, vm.Items.Count);
            Assert.IsFalse(vm.RunBatchCommand.CanExecute(null));
        }

        [TestMethod]
        public void PopulateItems_FromFolderWithSupportedFiles_LoadsThem()
        {
            var tmp = Path.Combine(Path.GetTempPath(), $"outliner-batch-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tmp);
            File.WriteAllText(Path.Combine(tmp, "one.txt"),  "first prose");
            File.WriteAllText(Path.Combine(tmp, "two.docx"), "second prose");
            File.WriteAllText(Path.Combine(tmp, "skip.md"),  "unsupported format");

            var vm = BuildVm();
            vm.InputFolder = tmp;
            vm.PopulateItems();

            Assert.AreEqual(2, vm.Items.Count, "Only .txt and .docx should be picked up.");
            Assert.IsTrue(vm.RunBatchCommand.CanExecute(null));

            Directory.Delete(tmp, recursive: true);
        }

        [TestMethod]
        public void IsRunning_True_DisablesRunBatchCommand()
        {
            var tmp = Path.Combine(Path.GetTempPath(), $"outliner-batch-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tmp);
            File.WriteAllText(Path.Combine(tmp, "x.txt"), "prose");

            var vm = BuildVm();
            vm.InputFolder = tmp;
            vm.PopulateItems();
            Assert.IsTrue(vm.RunBatchCommand.CanExecute(null));

            vm.IsRunning = true;
            Assert.IsFalse(vm.RunBatchCommand.CanExecute(null));

            Directory.Delete(tmp, recursive: true);
        }

        [TestMethod]
        public void NewVm_LastFolderRestoredFromPreferences()
        {
            var tmp = Path.Combine(Path.GetTempPath(), $"outliner-batch-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tmp);
            File.WriteAllText(Path.Combine(tmp, "a.docx"), "prose");

            var prefs = new OutlinerPreferences { LastBatchInputFolder = tmp };
            var vm = BuildVm(prefs);

            Assert.AreEqual(tmp, vm.InputFolder);
            Assert.AreEqual(Path.Combine(tmp, "Outlines"), vm.OutputFolder);
            Assert.AreEqual(1, vm.Items.Count);

            Directory.Delete(tmp, recursive: true);
        }
    }
}
