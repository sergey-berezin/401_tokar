using System.Diagnostics;
using System;
namespace GeneticProgrammingLib {
    public class AsyncEvolution {
        public class AsyncTournamentTableComparer : IComparer<AsyncTournamentTable> {
            public int Compare(AsyncTournamentTable? x, AsyncTournamentTable? y) {
                if (Object.ReferenceEquals(x, y)) {
                    return 0;
                }
                if (x.Rank().Item1 > y.Rank().Item1) return 1;
                if (x.Rank().Item1 < y.Rank().Item1) return -1;
                if (x.Rank().Item2 > y.Rank().Item2) return 1;
                if (x.Rank().Item2 < y.Rank().Item2) return -1;
                if (x.GetHashCode() < y.GetHashCode()) return -1;
                return 1;
            }
        }
        public class ProbabilitiesComparer : IComparer<long> {
            public int Compare(long x, long y) {
                if (x < y) return -1;
                return 1;
            }
        }
        public AsyncTournamentTable[] Population { get; set; }
        public double EvolutionStrength { get; set; }
        public double MutationRate { get; set; }
        public double CrossoverRate { get; set; }
        public int Rounds { get; set; }
        public int Players { get; set; }
        public int Courts { get; set; }
        public int MaxPopulationSize { get; set; }
        public int Epoch {get; set;}
        public AsyncEvolution() {}
        public AsyncEvolution (
            int rounds,
            int players,
            int courts,
            double evolution_strength, 
            double mutation_rate, 
            double crossover_rate,
            int population_size,
            int max_population_size
            )
        {
            Epoch = 0;
            EvolutionStrength = evolution_strength;
            MutationRate = mutation_rate;
            CrossoverRate = crossover_rate;
            Rounds = rounds;
            Players = players;
            Courts = courts;
            MaxPopulationSize = max_population_size;

            // Stopwatch stopwatch = new Stopwatch();
            // stopwatch.Reset();
            // stopwatch.Start();
            Population = new AsyncTournamentTable[population_size];
            for (int i = 0; i < population_size; ++i) {
                AsyncTournamentTable new_table = new AsyncTournamentTable(Rounds, Players, Courts);
                Population[i] = new_table;
            }
            ParallelMergeSort<AsyncTournamentTable>.Sort(Population, new AsyncTournamentTableComparer());
            // stopwatch.Stop();
            // Console.WriteLine($"POPULATION BUILD TIME: {stopwatch.ElapsedTicks} ticks {stopwatch.ElapsedMilliseconds} ms");
        }
        public void Step() {
            Stopwatch stopwatch = new Stopwatch();
            Task[] tasks = new Task[2 * Population.Length];
            ++Epoch;
            int NumSamplesToCrossover = (int)(CrossoverRate * (double)Population.Length);
            int NumSamplesToMutate = (int)(MutationRate * (double)Population.Length);
            Random rnd = new Random();

            // stopwatch.Reset();
            // stopwatch.Start();

            long[] ProbabilitiesToCrossover = new long[NumSamplesToCrossover * 2];
            long RankSum = Population.Sum(x => x.Rank().Item1);
            for (int k = 0; k < NumSamplesToCrossover * 2; ++k) {
                ProbabilitiesToCrossover[k] = rnd.NextInt64(RankSum);
            }
            ParallelMergeSort<long>.Sort(ProbabilitiesToCrossover, new ProbabilitiesComparer());

            // stopwatch.Stop();
            // Console.WriteLine($"PROBABILITIES GENERATION TIME: {stopwatch.ElapsedTicks} ticks {stopwatch.ElapsedMilliseconds} ms");

            var ProbabilitiesToCrossoverEnumerator = ProbabilitiesToCrossover.GetEnumerator();

            ProbabilitiesToCrossoverEnumerator.MoveNext();
            var PopulationEnumerator = Population.GetEnumerator();
            AsyncTournamentTable[] SamplesToCrossover = new AsyncTournamentTable[NumSamplesToCrossover * 2];
            int idx = 0;
            long CumulativeRank = 0;
            bool f = true;
            while (PopulationEnumerator.MoveNext() && f) {
                CumulativeRank += ((AsyncTournamentTable)PopulationEnumerator.Current).Rank().Item1;
                while ((long)ProbabilitiesToCrossoverEnumerator.Current <= CumulativeRank) {
                    SamplesToCrossover[idx++] = ((AsyncTournamentTable)PopulationEnumerator.Current);
                    if (!(f = ProbabilitiesToCrossoverEnumerator.MoveNext())) break;
                }
            } 
            Random.Shared.Shuffle(SamplesToCrossover);
            AsyncTournamentTable[] NewSamples = new AsyncTournamentTable[NumSamplesToCrossover + Population.Length];
            
            int i = 0;

            // stopwatch.Reset();
            // stopwatch.Start();

            for (i = 0; i < NumSamplesToCrossover; ++i) {
                tasks[i] = Task.Factory.StartNew((j) => {
                    int i = (int)j;
                    AsyncTournamentTable new_sample = SamplesToCrossover[2 * i].Crossover(SamplesToCrossover[2 * i + 1]);
                    NewSamples[i]  = new_sample;
                }, i);
            }
            foreach (var sample in Population) {
                if (rnd.NextDouble() < MutationRate) {
                    tasks[i] = Task.Factory.StartNew((j) => {
                        int i = (int)j;
                        AsyncTournamentTable new_sample = sample.Mutate(Courts);
                        NewSamples[i] = new_sample;
                    }, i);
                    ++i;
                }
            }
            Task.WaitAll(tasks.Where(x => x != null).ToArray());
            Array.Resize(ref NewSamples, i);

            // stopwatch.Stop();
            // Console.WriteLine($"MUTATION TIME: {stopwatch.ElapsedTicks} ticks {stopwatch.ElapsedMilliseconds} ms");

            // stopwatch.Reset();
            // stopwatch.Start();

            ParallelMergeSort<AsyncTournamentTable>.Sort(NewSamples, new AsyncTournamentTableComparer());
            Population = SetOperations<AsyncTournamentTable>.Union(Population, NewSamples, new AsyncTournamentTableComparer());

            // stopwatch.Stop();
            // Console.WriteLine($"SORT+UNION TIME: {stopwatch.ElapsedTicks} ticks {stopwatch.ElapsedMilliseconds} ms");
            int NumSamplesToRemove = (Population.Length - MaxPopulationSize);
            AsyncTournamentTable[] SamplesToRemove = new AsyncTournamentTable[NumSamplesToRemove];
            int j = 0;
            foreach (var sample in Population) {
                if (++j > NumSamplesToRemove) break;
                SamplesToRemove[j - 1] = sample;
            }

            Population = SetOperations<AsyncTournamentTable>.Difference(Population, SamplesToRemove, new AsyncTournamentTableComparer());
            
        }
        public (int, int) BestRank() {
            return Population.Last().Rank();
        }
        public Dictionary<int, SortedSet<int>>[] BestTable()
        {
            return Population.Last().Table;
        }
    }
}
