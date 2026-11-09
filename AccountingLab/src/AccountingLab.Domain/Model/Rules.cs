namespace AccountingLab.Domain.Model;

public sealed class BusinessRuleException(string message) : Exception(message);
public sealed class ResourceConflictException(string message) : Exception(message);
public sealed class ResourceNotFoundException(string message) : Exception(message);
public sealed class AccessDeniedException(string message) : Exception(message);

public static class MoneyRules
{
    public const decimal Maximum = 999999999999999.9999m;
    public static void Validate(decimal value, bool allowZero = false)
    {
        if (value < 0 || (!allowZero && value == 0) || value > Maximum || ((decimal.GetBits(value)[3] >> 16) & 0xff) > 4)
            throw new BusinessRuleException("El importe debe ser positivo, dentro de DECIMAL(19,4) y con hasta cuatro decimales.");
    }
}

public static class JournalRules
{
    public static void Validate(JournalEntry entry)
    {
        if (entry.OperationId == Guid.Empty || string.IsNullOrWhiteSpace(entry.Reference) || entry.Date == default)
            throw new BusinessRuleException("La operación, referencia y fecha son obligatorias.");
        if (entry.Lines.Count < 2) throw new BusinessRuleException("El asiento debe tener al menos dos líneas.");
        foreach (var line in entry.Lines)
        {
            if (line.AccountId == Guid.Empty || (line.Debit > 0) == (line.Credit > 0))
                throw new BusinessRuleException("Cada línea requiere una cuenta y un único débito o crédito positivo.");
            MoneyRules.Validate(line.Debit, true);
            MoneyRules.Validate(line.Credit, true);
        }
        if (entry.Lines.Sum(x => x.Debit) != entry.Lines.Sum(x => x.Credit))
            throw new BusinessRuleException("La suma de débitos debe ser igual a la suma de créditos.");
    }
}

public static class BasicDataRules
{
    public static void Required(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 200)
            throw new BusinessRuleException($"{field} es obligatorio y admite hasta 200 caracteres.");
    }
    public static void Validate(ITenantEntity value)
    {
        switch (value)
        {
            case Account x:
                Required(x.Name, "Nombre"); Required(x.Code, "Código");
                if (x.ParentId == x.Id) throw new BusinessRuleException("Una cuenta no puede agruparse en sí misma.");
                break;
            case AccountingPeriod x:
                Required(x.Name, "Nombre");
                if (x.StartDate == default || x.EndDate < x.StartDate || x.Status != "Open")
                    throw new BusinessRuleException("El periodo requiere fechas válidas y debe crearse abierto.");
                break;
            case ThirdParty x:
                Required(x.Name, "Nombre"); Required(x.Document, "Documento"); break;
            case CostCenter x:
                Required(x.Name, "Nombre"); Required(x.Code, "Código"); break;
            case TaxRule x:
                Required(x.Name, "Nombre");
                if (x.Rate < 0 || x.Rate > 1 || ((decimal.GetBits(x.Rate)[3] >> 16) & 0xff) > 6 || x.ValidFrom == default || x.ValidTo < x.ValidFrom)
                    throw new BusinessRuleException("La tasa ficticia debe estar entre 0 y 1, con hasta seis decimales y vigencia válida.");
                break;
            case Obligation x:
                Required(x.Description, "Descripción"); MoneyRules.Validate(x.Amount);
                if (x.ThirdPartyId == Guid.Empty || x.DueDate == default || x.Status != "Open" || x.Kind is not ("Receivable" or "Payable"))
                    throw new BusinessRuleException("La obligación requiere tercero, vencimiento y estado abierto.");
                break;
            case PaymentDraft x:
                Required(x.Reference, "Referencia"); MoneyRules.Validate(x.Amount);
                if (x.ThirdPartyId == Guid.Empty || x.Status != "Draft" || x.Direction is not ("Payment" or "Receipt"))
                    throw new BusinessRuleException("El pago o cobro se registra únicamente como borrador con tercero válido.");
                break;
            case InvoiceDraft x:
                Required(x.Number, "Número"); MoneyRules.Validate(x.Amount);
                if (x.ThirdPartyId == Guid.Empty || x.IssueDate == default || x.DueDate < x.IssueDate || x.Status != "Draft")
                    throw new BusinessRuleException("La factura básica debe ser un borrador con fechas válidas.");
                break;
            case BankStatement x:
                Required(x.Reference, "Referencia"); MoneyRules.Validate(x.Amount);
                if (x.BookingDate == default) throw new BusinessRuleException("La fecha bancaria es obligatoria.");
                break;
            case ReconciliationDraft x:
                Required(x.Name, "Nombre");
                if (x.StatementDate == default || x.Status != "Draft") throw new BusinessRuleException("La sesión debe ser un borrador fechado.");
                break;
            case BudgetDraft x:
                Required(x.Name, "Nombre"); MoneyRules.Validate(x.Amount, true);
                if (x.Year < 2000 || x.Year > 2200 || x.Status != "Draft") throw new BusinessRuleException("El presupuesto debe ser un borrador con año válido.");
                break;
            case ApprovalDraft x:
                Required(x.Reference, "Referencia"); MoneyRules.Validate(x.Amount);
                if (x.Status != "Draft") throw new BusinessRuleException("La solicitud se registra como borrador.");
                break;
            default: throw new BusinessRuleException("Este recurso es de solo lectura.");
        }
    }
}
