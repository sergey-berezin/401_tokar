namespace Test.GeneticProgrammingLib;

public class EvolutionTest
{
    [Fact]
    public void StepTest()
    {
        Evolution evolution = new Evolution(40, 50, 30, 0.5, 0.5, 0.8, 25, 25);
        evolution.Step();
        evolution.Population.Count(x => x == null).Should().Be(0);
    }
}