using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP08Composite;

public sealed record AccountHierarchy(string Code, bool AllowsPosting, decimal OwnBalance, IReadOnlyList<AccountHierarchy> Children);
public interface IAccountTreeChallenge { decimal Total(AccountHierarchy account); }

// --- COMPOSITE (Componente / Nodo de árbol contable) ---
public interface IAccountTreeNode
{
    decimal CalculateTotal();
}

public sealed class AccountHierarchyNode(AccountHierarchy account) : IAccountTreeNode
{
    private readonly AccountHierarchy _account = account ?? throw new ArgumentNullException(nameof(account));

    public decimal CalculateTotal()
    {
        var hasChildren = _account.Children is { Count: > 0 } && _account.Children.Any(c => c is not null);

        if (_account.AllowsPosting && hasChildren)
        {
            throw new ArgumentException(
                $"Account '{_account.Code}' allows posting and cannot contain child accounts.",
                nameof(account));
        }

        decimal childrenTotal = 0m;
        if (_account.Children is not null)
        {
            foreach (var child in _account.Children)
            {
                if (child is not null)
                {
                    childrenTotal += new AccountHierarchyNode(child).CalculateTotal();
                }
            }
        }

        return _account.OwnBalance + childrenTotal;
    }
}

public sealed class AccountTreeChallenge : IAccountTreeChallenge
{
    public decimal Total(AccountHierarchy account)
    {
        ArgumentNullException.ThrowIfNull(account);

        IAccountTreeNode rootNode = new AccountHierarchyNode(account);
        return rootNode.CalculateTotal();
    }
}