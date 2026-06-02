namespace Domain.Common;

public abstract class LongEntity
{
    public long Id { get; protected set; }

    protected LongEntity() { }
}
