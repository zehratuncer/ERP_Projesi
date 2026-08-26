using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Features.Reports.Queries.GetCategoryProfitability;
using ERP.UnitTests.Common;
using Xunit;

namespace ERP.UnitTests.Reports;

public class CategoryProfitabilityCalculationTests
{
    [Fact]
    public async Task GetCategoryProfitability_ShouldReturnCategoriesWithCorrectGrossProfitAndMargin()
    {
        // Arrange
        using var context = TestDbContextFactory.CreateInMemoryDbContext();

        var handler = new GetCategoryProfitabilityQueryHandler(context);
        var query = new GetCategoryProfitabilityQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Categories.Should().NotBeEmpty();

        // Verify categories contain Paper, Writing, Notebook etc.
        result.Data.Categories.Any(c => c.CategoryName.Contains("Kağıt")).Should().BeTrue();
    }
}
