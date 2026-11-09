using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AccountingLab.Domain.Challenges.DP22Visitor;

namespace AccountingLab.Api.Transport;

public static class AnalysisJsonTypes
{
    public static void Configure(JsonTypeInfo info)
    {
        if (info.Type != typeof(IAccountingAnalysisDocument)) return;
        info.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "documentKind",
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            DerivedTypes =
            {
                new JsonDerivedType(typeof(InvoiceAnalysisDocument), "invoice"),
                new JsonDerivedType(typeof(PaymentAnalysisDocument), "payment"),
                new JsonDerivedType(typeof(JournalAnalysisDocument), "journal")
            }
        };
    }
}
