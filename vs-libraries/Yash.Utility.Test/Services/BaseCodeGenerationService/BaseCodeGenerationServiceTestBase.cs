using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yash.Utility.Services.Interfaces;

namespace Yash.Utility.Test.Services.BaseCodeGenerationService
{
    [TestClass]
    public abstract class BaseCodeGenerationServiceTestBase
    {
        protected TestableCodeGeneration_service _service;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _service = new TestableCodeGeneration_service();
        }
    }

    // Testable concrete implementation for testing the abstract base class
    public class TestableCodeGeneration_service : Utility.Services.CodeGeneration.BaseCodeGenerationService
    {
        // This class is used to test the abstract BaseCodeGenerationService
        // by providing a concrete implementation for testing purposes
    }
}