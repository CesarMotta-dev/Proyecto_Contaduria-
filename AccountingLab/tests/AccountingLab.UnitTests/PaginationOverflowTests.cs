using AccountingLab.Application.Contracts;
using AccountingLab.Application.Services;
using AccountingLab.Domain.Model;
using Xunit;

namespace AccountingLab.UnitTests;

public sealed class PaginationOverflowTests
{
    [Theory]
    [InlineData(int.MaxValue, 100)]
    [InlineData(21474838, 100)]
    public void Rejects_page_whose_offset_exceeds_supported_integer_range(int page, int pageSize)
    {
        Assert.Throws<BusinessRuleException>(() => CatalogService.ValidatePage(new ListQuery(Page: page, PageSize: pageSize)));
    }

    [Fact]
    public void Last_page_with_representable_offset_is_not_rejected_as_overflow()
    {
        Assert.Null(Record.Exception(() => CatalogService.ValidatePage(new ListQuery(Page: 21474837, PageSize: 100))));
    }
}

