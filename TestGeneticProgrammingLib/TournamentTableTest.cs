namespace Test.GeneticProgrammingLib;

public class TournamentTableTest
{
    [Fact]
    public void RankTest()
    {
        int [,] t = {{1, 2, 1, 2},
                    {1, 1, 2, 2},
                    {1, 2, 2, 1},};
        TournamentTable table = new TournamentTable(t);
        table.Rank.Should().Be((3, 1));
    }
    [Fact]
    public void MutateTest() {
        int [,] t = {{1, 2, 1, 2},
                    {1, 1, 2, 2},
                    {1, 2, 2, 1},};
        TournamentTable table = new TournamentTable(t);
        table = table.Mutate(3);
        table.R.Should().Be(3);
        table.N.Should().Be(4);
        table.Table.Count().Should().Be(table.R);
        foreach (var round in table.Table) {
            round.Count().Should().Be(table.N / 2);
            foreach (var court in round) {
                court.Key.Should().BeLessThan(3);
                court.Value.Count().Should().Be(2);
                foreach (var player in court.Value) {
                    player.Should().BeLessThan(table.N);
                }
            }
        }
    }
    [Fact]
    public void CrossoverTest() {
        int [,] t1 = {{1, 2, 1, 2},
                    {1, 1, 2, 2},
                    {1, 2, 2, 1},};
        int [,] t2 = {{0, 3, 0, 3},
                    {0, 0, 3, 3},
                    {0, 3, 3, 0},};
        TournamentTable table1 = new TournamentTable(t1);
        TournamentTable table2 = new TournamentTable(t2);
        TournamentTable table = table1.Crossover(table2);
        table.R.Should().Be(3);
        table.N.Should().Be(4);
        table.Table.Count().Should().Be(table.R);
        SortedSet<int> courts = new SortedSet<int>();
        foreach (var round in table.Table) {
            round.Count().Should().Be(table.N / 2);
            foreach (var court in round) {
                courts.Add(court.Key);
                court.Key.Should().BeLessThan(4);
                court.Value.Count().Should().Be(2);
                foreach (var player in court.Value) {
                    player.Should().BeLessThan(table.N);
                }
            }
        }
        courts.Should().BeEquivalentTo(new SortedSet<int>() {0, 1, 2, 3});
    }
}