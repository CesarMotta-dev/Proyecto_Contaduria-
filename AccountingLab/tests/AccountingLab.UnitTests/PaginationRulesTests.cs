using AccountingLab.Application.Contracts;
using AccountingLab.Application.Services;
using AccountingLab.Domain.Model;
using Xunit;

namespace AccountingLab.UnitTests;

public sealed class PaginationRulesTests
{
    [Fact]
    public void Default_page_is_valid()
    {
        Assert.Null(Record.Exception(() => CatalogService.ValidatePage(new ListQuery())));
    }

    [Fact]
    public void Maximum_page_size_is_valid()
    {
        Assert.Null(Record.Exception(() => CatalogService.ValidatePage(new(Page: 2, PageSize: 100))));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Page_number_must_start_at_one(int page)
    {
        Assert.Throws<BusinessRuleException>(() => CatalogService.ValidatePage(new(Page: page)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Page_size_must_be_between_one_and_one_hundred(int size)
    {
        Assert.Throws<BusinessRuleException>(() => CatalogService.ValidatePage(new(PageSize: size)));
    }

    [Fact]
    public void Single_day_filter_is_valid()
    {
        Assert.Null(Record.Exception(() => CatalogService.ValidatePage(new(From: new(2026, 1, 1), To: new(2026, 1, 1)))));
    }

    [Fact]
    public void Inverted_date_filter_is_rejected()
    {
        Assert.Throws<BusinessRuleException>(() => CatalogService.ValidatePage(new(From: new(2026, 2, 1), To: new(2026, 1, 1))));
    }

    [Fact]
    public void Optional_date_boundary_can_remain_unspecified()
    {
        Assert.Null(Record.Exception(() => CatalogService.ValidatePage(new(From: new(2026, 1, 1)))));
    }
}

