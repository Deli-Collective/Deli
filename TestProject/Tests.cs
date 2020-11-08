using System;
using System.IO;
using H3ModLoader;
using NUnit.Framework;

namespace TestProject
{
    [TestFixture]
    public class Tests
    {
        private const string ModArchive = "SampleModArchive.zip";

        [OneTimeSetUp]
        public void Setup()
        {
            TypeLoaders.ScanAssemblies();
        }
        
        [Test(Author = "nrgill28", Description = "Tests the ModInfo load method")]
        public void TestModInfoLoad()
        {
            // Get the mod archive
            var modInfo = ModInfo.FromFile(ModArchive);
            // We really only need to test for one value here since they're all deserialized with Newtonsoft.
            Assert.True(modInfo.Guid == "nrgill28.sample_mod");
        }

        [Test(Author = "nrgill28", Description = "Tests the string Type Loader method")]
        public void TestTypeLoaderString()
        {
            // Get the mod archive
            var modInfo = ModInfo.FromFile(ModArchive);
            
            // Get the string resource
            var resource = modInfo.GetResource<string>("Resources/ExampleResource.txt");
            
            // Assert
            Assert.True(resource == "This is a sample resource containing some text data!");
        }
    }
}