using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Outliner;
using Outliner.Services;

namespace OutlinerTests
{
    /// <summary>
    /// VM-level tests for SettingsPageViewModel. Real OutlinerPreferences and
    /// real PreferencesService — the service is just pointed at a temp file
    /// so we don't write to the user's actual LocalAppData during tests.
    /// </summary>
    [TestClass]
    public class SettingsPageViewModelTests
    {
        private string _tempPrefsPath = string.Empty;
        private PreferencesService _service = null!;

        [TestInitialize]
        public void SetUp()
        {
            _tempPrefsPath = Path.Combine(
                Path.GetTempPath(),
                $"outliner-prefs-{Guid.NewGuid():N}.json");
            _service = new PreferencesService(_tempPrefsPath);
        }

        [TestCleanup]
        public void TearDown()
        {
            if (File.Exists(_tempPrefsPath))
                File.Delete(_tempPrefsPath);
        }

        [TestMethod]
        public void Construction_StartupSingle_ReflectsInTwoFlags()
        {
            var prefs = new OutlinerPreferences { StartupMode = "Single" };
            var vm = new SettingsPageViewModel(prefs, _service);
            Assert.IsTrue(vm.IsStartupSingle);
            Assert.IsFalse(vm.IsStartupBatch);
        }

        [TestMethod]
        public void Construction_StartupBatch_ReflectsInTwoFlags()
        {
            var prefs = new OutlinerPreferences { StartupMode = "Batch" };
            var vm = new SettingsPageViewModel(prefs, _service);
            Assert.IsFalse(vm.IsStartupSingle);
            Assert.IsTrue(vm.IsStartupBatch);
        }

        [TestMethod]
        public void IsStartupBatch_True_UpdatesPrefsAndPersists()
        {
            var prefs = new OutlinerPreferences { StartupMode = "Single" };
            var vm = new SettingsPageViewModel(prefs, _service);

            vm.IsStartupBatch = true;

            Assert.AreEqual("Batch", prefs.StartupMode);
            Assert.IsTrue(File.Exists(_tempPrefsPath), "Prefs file should be written.");

            // Round-trip: reload via a fresh service should restore Batch.
            var reloaded = new PreferencesService(_tempPrefsPath).Load();
            Assert.AreEqual("Batch", reloaded.StartupMode);
        }

        [TestMethod]
        public void IsStartupSingle_True_AfterBatch_UpdatesPrefsBackToSingle()
        {
            var prefs = new OutlinerPreferences { StartupMode = "Batch" };
            var vm = new SettingsPageViewModel(prefs, _service);

            vm.IsStartupSingle = true;

            Assert.AreEqual("Single", prefs.StartupMode);

            var reloaded = new PreferencesService(_tempPrefsPath).Load();
            Assert.AreEqual("Single", reloaded.StartupMode);
        }
    }
}
