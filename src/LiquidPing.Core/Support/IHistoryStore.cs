namespace LiquidPing.Core.Support;

public interface IHistoryStore
{
    void Append(Guid hostId, HistoryPoint point);

    IReadOnlyList<HistoryPoint> Get(Guid hostId, TimeSpan window);
}
