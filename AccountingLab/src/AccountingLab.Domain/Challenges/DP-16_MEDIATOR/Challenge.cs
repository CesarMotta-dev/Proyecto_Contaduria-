using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP16Mediator;

public sealed record InvoiceProcessingRequest(Guid InvoiceId, decimal Amount, bool IsValid, IReadOnlyList<string> Participants);
public sealed record InvoiceProcessingResult(Guid InvoiceId, bool Published, IReadOnlyList<string> ParticipantsInvoked);
public interface IInvoiceCoordinationChallenge { InvoiceProcessingResult Process(InvoiceProcessingRequest request); }

// --- MEDIATOR ABSTRACTION ---
internal interface IInvoiceProcessingMediator
{
    void RegisterParticipant(IInvoiceParticipant participant);
    InvoiceProcessingResult Coordinate(InvoiceProcessingRequest request);
}

// --- COLLEAGUE ABSTRACTION ---
internal interface IInvoiceParticipant
{
    string Name { get; }
    bool IsValidationStep { get; }
    void Handle(InvoiceProcessingRequest request, IInvoiceProcessingMediator mediator);
}

// --- CONCRETE COLLEAGUE ---
internal sealed class NamedInvoiceParticipant : IInvoiceParticipant
{
    public string Name { get; }
    public bool IsValidationStep { get; }

    public NamedInvoiceParticipant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Participant name cannot be null or whitespace.", nameof(name));
        }

        Name = name.Trim();
        IsValidationStep = Name.StartsWith("valid", StringComparison.OrdinalIgnoreCase);
    }

    public void Handle(InvoiceProcessingRequest request, IInvoiceProcessingMediator mediator)
    {
        // Cada colega ejecuta su responsabilidad coordinado exclusivamente a través del mediador
    }
}

// --- CONCRETE MEDIATOR ---
internal sealed class InvoiceProcessingMediator : IInvoiceProcessingMediator
{
    private readonly List<IInvoiceParticipant> _participants = new();
    private readonly HashSet<string> _registeredNames = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterParticipant(IInvoiceParticipant participant)
    {
        ArgumentNullException.ThrowIfNull(participant);

        if (!_registeredNames.Add(participant.Name))
        {
            throw new ArgumentException(
                $"Duplicate participant registration '{participant.Name}' is not allowed.",
                nameof(participant));
        }

        _participants.Add(participant);
    }

    public InvoiceProcessingResult Coordinate(InvoiceProcessingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.InvoiceId == Guid.Empty)
        {
            throw new ArgumentException("InvoiceId cannot be empty.", nameof(request));
        }

        var invoked = new List<string>(_participants.Count);

        if (!request.IsValid || request.Amount <= 0m)
        {
            foreach (var participant in _participants.Where(p => p.IsValidationStep))
            {
                participant.Handle(request, this);
                invoked.Add(participant.Name);
            }

            return new InvoiceProcessingResult(
                InvoiceId: request.InvoiceId,
                Published: false,
                ParticipantsInvoked: invoked.AsReadOnly());
        }

        foreach (var participant in _participants)
        {
            participant.Handle(request, this);
            invoked.Add(participant.Name);
        }

        return new InvoiceProcessingResult(
            InvoiceId: request.InvoiceId,
            Published: true,
            ParticipantsInvoked: invoked.AsReadOnly());
    }
}

// --- ENTRYPOINT ---
public sealed class InvoiceCoordinationChallenge : IInvoiceCoordinationChallenge
{
    public InvoiceProcessingResult Process(InvoiceProcessingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        IInvoiceProcessingMediator mediator = new InvoiceProcessingMediator();

        if (request.Participants is not null)
        {
            foreach (var participantName in request.Participants)
            {
                mediator.RegisterParticipant(new NamedInvoiceParticipant(participantName));
            }
        }

        return mediator.Coordinate(request);
    }
}