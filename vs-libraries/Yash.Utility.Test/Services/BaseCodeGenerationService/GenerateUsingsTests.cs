using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Yash.Utility.Models.CodeGeneration;
using Yash.Utility.Models.Configuration;
using Yash.Utility.Services.CodeGeneration.Models;

namespace Yash.Utility.Test.Services.BaseCodeGenerationService
{
    [TestClass]
    public class GenerateUsingsTests : BaseCodeGenerationServiceTestBase
    {
        [TestMethod]
        public void GenerateUsings_WithBasicUsings_ShouldGenerateCorrectFormat()
        {
            // Arrange
            var usings = new List<CodeGenerationUsing>
            {
                new() { Namespace = "System" },
                new() { Namespace = "System.Collections.Generic" }
            };

            // Act
            var result = _service.GenerateUsings(usings);

            // Assert
            result.Should().Contain("using System;");
            result.Should().Contain("using System.Collections.Generic;");
        }

        [TestMethod]
        public void GenerateUsings_WithStaticUsing_ShouldIncludeStaticKeyword()
        {
            // Arrange
            var usings = new List<CodeGenerationUsing>
            {
                new() { Namespace = "System.Math", IsStatic = true }
            };

            // Act
            var result = _service.GenerateUsings(usings);

            // Assert
            result.Should().Contain("using static System.Math;");
        }

        [TestMethod]
        public void GenerateUsings_WithGlobalUsing_ShouldIncludeGlobalKeyword()
        {
            // Arrange
            var usings = new List<CodeGenerationUsing>
            {
                new() { Namespace = "System", IsGlobal = true }
            };

            // Act
            var result = _service.GenerateUsings(usings);

            // Assert
            result.Should().Contain("global using System;");
        }

        [TestMethod]
        public void GenerateUsings_WithAlias_ShouldIncludeAlias()
        {
            // Arrange
            var usings = new List<CodeGenerationUsing>
            {
                new() { Namespace = "System.Collections.Generic", Alias = "SCG" }
            };

            // Act
            var result = _service.GenerateUsings(usings);

            // Assert
            result.Should().Contain("using SCG = System.Collections.Generic;");
        }

        [TestMethod]
        public void GenerateUsings_WithSortingEnabled_ShouldSortUsings()
        {
            // Arrange
            var usings = new List<CodeGenerationUsing>
            {
                new() { Namespace = "System.Linq" },
                new() { Namespace = "System" },
                new() { Namespace = "System.Collections.Generic" }
            };

            // Act
            var result = _service.GenerateUsings(usings, new CodeGenerationOptions { SortUsings = true });

            // Assert
            var lines = result.Split('\n');
            var systemIndex = -1;
            var linqIndex = -1;
            var genericIndex = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("using System;")) systemIndex = i;
                if (lines[i].Contains("using System.Collections.Generic;")) genericIndex = i;
                if (lines[i].Contains("using System.Linq;")) linqIndex = i;
            }

            // System should come before System.Collections.Generic and System.Linq
            systemIndex.Should().BeLessThan(genericIndex);
            systemIndex.Should().BeLessThan(linqIndex);
            genericIndex.Should().BeLessThan(linqIndex);
        }
    }
}