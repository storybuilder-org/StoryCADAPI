using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoryCADLib.Services.IoC;

namespace StoryCADCritterTests;

[TestClass]
public static class TestSetup
{
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        BootStrapper.Initialise(headless: true);
    }
}
