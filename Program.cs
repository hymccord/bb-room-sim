using KaimiraGames;

namespace bbsurvivor;

internal class Program
{
    private static async Task Main(string[] args)
    {
        int numRuns = 100_000;
        var runner = new RoomRunner(
            numRuns,
            new IRoomStrategy[]
            {
                new NoobStrat(),
                new BailOnLower(),
                new CoastTilExtremeOrBetter(),
                new CoastTilUltimate(),
            },
            new IRoomDistributionProvider[]
            {
                new DefaultRoomDistribution(),
                new GreatHallRoomDistribution(),
            }
        );

        await runner.SimulateAsync();
    }
}

class RoomRunner
{
    private readonly IRoomStrategy[] _roomStrategies;
    private readonly IEnumerable<IRoomDistributionProvider> _roomDistributionProviders;
    private int _numRuns;

    public RoomRunner(int numRuns, IRoomStrategy[] roomStrategies, IRoomDistributionProvider[] roomDistributionProviders)
    {
        _numRuns = numRuns;
        _roomStrategies = roomStrategies;
        _roomDistributionProviders = roomDistributionProviders;
    }

    public async Task SimulateAsync()
    {
        var tasks = new List<Task>();
        var writers = new List<StrategyWriter>();

        foreach (IRoomDistributionProvider distribution in _roomDistributionProviders)
        {
            var writer = new StrategyWriter(distribution.Name);
            writers.Add(writer);

            var rooms = distribution.GetRoomDistributions();
            writer.WriteResult("Default Distribution",
                rooms.GetWeightOf(RoomLevel.Standard),
                rooms.GetWeightOf(RoomLevel.Super),
                rooms.GetWeightOf(RoomLevel.Extreme),
                rooms.GetWeightOf(RoomLevel.Ultimate)
            );

            foreach (IRoomStrategy strategy in _roomStrategies)
            {
                tasks.Add(Task.Run(async () => await RunStrategy(writer, distribution, strategy)));
            }
        }

        await Task.WhenAll(tasks);
        foreach (var writer in writers)
        {
            writer.Dispose();
        }
    }

    private async Task RunStrategy(StrategyWriter writer, IRoomDistributionProvider roomDistributionProvider, IRoomStrategy strategy)
    {
        
        var roomCounts = new Dictionary<RoomLevel, int>
        {
            {RoomLevel.Standard, 0},
            {RoomLevel.Super, 0},
            {RoomLevel.Extreme, 0},
            {RoomLevel.Ultimate, 0},
        };
        WeightedList<RoomLevel> roomDistributions = roomDistributionProvider.GetRoomDistributions();

        for (int i = 1; i <= _numRuns; i++)
        {
            RoomLevel currentRoom = roomDistributions.Next();
            RoomLevel nextRoom = roomDistributions.Next();
            roomCounts[currentRoom]++;

            while (strategy.ShouldAdvance(currentRoom, nextRoom))
            {
                currentRoom = nextRoom;
                nextRoom = roomDistributions.Next();

                roomCounts[nextRoom]++;
            }
        }

        writer.WriteResult(strategy.Name,
            roomCounts[RoomLevel.Standard],
            roomCounts[RoomLevel.Super],
            roomCounts[RoomLevel.Extreme],
            roomCounts[RoomLevel.Ultimate]);

        await Console.Out.WriteLineAsync(
            $"""
            Done. Distribution: '{roomDistributionProvider.Name}', Strategy: '{strategy.Name}'");
            {string.Format("{0},{1},{2},{3},{4}", strategy.Name,
            roomCounts[RoomLevel.Standard],
            roomCounts[RoomLevel.Super],
            roomCounts[RoomLevel.Extreme],
            roomCounts[RoomLevel.Ultimate])}
            """
        );
    }

    private class StrategyWriter : IDisposable
    {
        private StreamWriter _stream;

        public StrategyWriter(string name)
        {
            _stream = new StreamWriter($"{name.Replace(" ", "")}.csv", append: false);
            _stream.WriteLine("Name,Standard,Super,Extreme,Ultimate");
        }

        public void WriteResult(string name, int standardCount, int superCount, int extremeCount, int ultimateCount)
        {
            _stream.WriteLine($"{name},{standardCount},{superCount},{extremeCount},{ultimateCount}");
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
