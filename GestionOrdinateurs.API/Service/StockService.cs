namespace GestionOrdinateurs.API.Service;

public class StockService
{
    private readonly object _verrou = new();

    public int StockSSD { get; private set; } = 10;

    public void ConsommerSSD()
    {
        lock (_verrou)
        {
            if (StockSSD <= 0) return;

            var stockActuel = StockSSD;
            Task.Delay(10).Wait();
            StockSSD = stockActuel - 1;
        }
    }
}