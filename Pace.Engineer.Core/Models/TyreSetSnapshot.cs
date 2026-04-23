namespace Pace.Engineer.Core.Models;

public sealed class TyreSetSnapshot
{
    public TyreSnapshot FrontLeft { get; init; } = new();
    public TyreSnapshot FrontRight { get; init; } = new();
    public TyreSnapshot RearLeft { get; init; } = new();
    public TyreSnapshot RearRight { get; init; } = new();
}