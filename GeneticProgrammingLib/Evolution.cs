namespace GeneticProgrammingLib {
    public class Evolution {
        public class TournamentTableComparer : IComparer<TournamentTable> {
            public int Compare(TournamentTable? x, TournamentTable? y) {
                if (Object.ReferenceEquals(x, y)) {
                    return 0;
                }
                if (x.Rank.Item1 > y.Rank.Item1) return 1;
                if (x.Rank.Item1 < y.Rank.Item1) return -1;
                if (x.Rank.Item2 > y.Rank.Item2) return 1;
                if (x.Rank.Item2 < y.Rank.Item2) return -1;
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
        public SortedSet<TournamentTable> Population = new SortedSet<TournamentTable>(new TournamentTableComparer());
        protected double EvolutionStrength;
        protected double MutationRate;
        protected double CrossoverRate;
        protected int Rounds;
        protected int Players;
        protected int Courts;
        protected int MaxPopulationSize;
        public int Epoch {get; protected set;}
        protected Evolution() {}
        public Evolution (
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
            for (int i = 0; i < population_size; ++i) {
                TournamentTable new_table = new TournamentTable(Rounds, Players, Courts);
                Population.Add(new_table);
            }
        }
        public void Step() {
            ++Epoch;
            int NumSamplesToCrossover = (int)(CrossoverRate * (double)Population.Count);
            int NumSamplesToMutate = (int)(MutationRate * (double)Population.Count);
            Random rnd = new Random();
            SortedSet<long> ProbabilitiesToCrossover = new SortedSet<long>(new ProbabilitiesComparer());
            long RankSum = Population.Sum(x => x.Rank.Item1);
            for (int i = 0; i < NumSamplesToCrossover * 2; ++i) {
                ProbabilitiesToCrossover.Add(rnd.NextInt64(RankSum));
            }
            var ProbabilitiesToCrossoverEnumerator = ProbabilitiesToCrossover.GetEnumerator();
            ProbabilitiesToCrossoverEnumerator.MoveNext();
            var PopulationEnumerator = Population.GetEnumerator();
            TournamentTable[] SamplesToCrossover = new TournamentTable[NumSamplesToCrossover * 2];
            int idx = 0;
            long CumulativeRank = 0;
            bool f = true;
            while (PopulationEnumerator.MoveNext() && f) {
                CumulativeRank += PopulationEnumerator.Current.Rank.Item1;
                while (ProbabilitiesToCrossoverEnumerator.Current <= CumulativeRank) {
                    SamplesToCrossover[idx++] = PopulationEnumerator.Current;
                    if (!(f = ProbabilitiesToCrossoverEnumerator.MoveNext())) break;
                }
            } 
            Random.Shared.Shuffle(SamplesToCrossover);
            SortedSet<TournamentTable> NewSamples = new SortedSet<TournamentTable>(new TournamentTableComparer());
            for (int i = 0; i < NumSamplesToCrossover; ++i) {
                TournamentTable new_sample = SamplesToCrossover[2 * i].Crossover(SamplesToCrossover[2 * i + 1]);
                NewSamples.Add(new_sample);
            }
            SortedSet<TournamentTable> MutatedSamples = new SortedSet<TournamentTable>(new TournamentTableComparer());
            foreach (var sample in Population) {
                if (rnd.NextDouble() < MutationRate) {
                    TournamentTable new_sample = sample.Mutate(Courts);
                    NewSamples.Add(new_sample);
                    MutatedSamples.Add(sample);
                }
            }
            // Population.ExceptWith(MutatedSamples);
            Population.UnionWith(NewSamples);
            // foreach (var sample in Population) {
            //     if ((double)sample.Rank.Item1 / (double)RankSum.Item1 * str)
            // }
            SortedSet<TournamentTable> SamplesToRemove = new SortedSet<TournamentTable>(new TournamentTableComparer());
            int NumSamplesToRemove = (Population.Count - MaxPopulationSize);
            int j = 0;
            foreach (var sample in Population) {
                if (++j > NumSamplesToRemove) break;
                SamplesToRemove.Add(sample);
            }
            Population.ExceptWith(SamplesToRemove);
            
        }
        public (int, int) BestRank() {
            return Population.Last().Rank;
        }
    }
}
