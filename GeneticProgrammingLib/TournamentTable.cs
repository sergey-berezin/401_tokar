namespace GeneticProgrammingLib {
    public class TournamentTable {
        public Dictionary<int, SortedSet<int>>[] Table {get; private set;}
        public int[,] RawTable {
            get {
                int[,] result = new int[R, N];
                for (int i = 0; i < R; ++i)
                    foreach (var pair in Table[i])
                        foreach (var player in pair.Value)
                            result[i, player] = pair.Key;
                return result;
            }
        }
        public int R {get; private set;}
        public int N {get; private set;}
        private (int, int)? rank = null;
        public (int, int) Rank {
            get {
                if (rank is null) {
                    
                    HashSet<int>[] Courts = new HashSet<int>[N];
                    SortedSet<int>[] Opponents = new SortedSet<int>[N];
                    for (int i = 0; i < N; ++i) {
                        Courts[i] = new HashSet<int>();
                        Opponents[i] = new SortedSet<int>();
                    }
                    
                    for (int i = 0; i < R; ++i) {
                        foreach (var court in Table[i]) {
                            foreach (int participant in court.Value) {
                                Opponents[participant].UnionWith(court.Value);
                                Courts[participant].Add(court.Key);
                            }
                        }
                    }
                    rank = (Opponents.Min(x => x.Count) - 1, Courts.Min(x => x.Count));
                }
                return ((int, int)) rank;
            }
        }
        public TournamentTable(int[,] table) {
            R = table.GetUpperBound(0) + 1;
            N = table.GetUpperBound(1) + 1;
            Table = new Dictionary<int, SortedSet<int>>[R];
            for (int i = 0; i < R; ++i) {
                Table[i] = new Dictionary<int, SortedSet<int>>(N / 2 + 1);
                for (int j = 0; j < N; ++j) {
                    if (!Table[i].TryAdd(table[i, j], new SortedSet<int>() {j})) {
                        Table[i][table[i, j]].Add(j);
                    }
                } 
            }
        }
        private TournamentTable(Dictionary<int, SortedSet<int>>[] table) {
            Table = table;
            R = table.Count();
            N = table[0].Count() * 2;
        }
        public TournamentTable(int r, int n, int K) {
            R = r; N = n;
            int[] Courts = Enumerable.Range(0, K).ToArray();
            int[] Players = Enumerable.Range(0, N).ToArray();
            Table = new Dictionary<int, SortedSet<int>>[R];
            for (int i = 0; i < R; ++i) {
                Table[i] = new Dictionary<int, SortedSet<int>>();
                Random.Shared.Shuffle(Courts);
                Random.Shared.Shuffle(Players);
                for (int j = 0; j < N / 2; ++j) {
                    Table[i].Add(Courts[j], new SortedSet<int>() { Players[2 * j], Players[2 * j + 1] });
                }
            }
        }
        public TournamentTable Mutate(int K, int rounds=1, int pairs=1) {
            Random rnd = new Random();
            Dictionary<int, SortedSet<int>>[] new_table = new Dictionary<int, SortedSet<int>>[R];
            int[] Rounds = Enumerable.Range(0, R).ToArray();
            int[] Courts = Enumerable.Range(0, N >> 1).ToArray();
            Random.Shared.Shuffle(Rounds);
            for (int i = 0; i < rounds; ++i) {
                Random.Shared.Shuffle(Courts);
                HashSet<int> CourtSet = new HashSet<int>(pairs);
                for (int j = 0; j < pairs; j++) {
                    CourtSet.Add(Courts[j]);
                }
                int[] players = new int[pairs << 1];
                int n = 0, count = 0;
                new_table[i] = new Dictionary<int, SortedSet<int>>();
                foreach (var pair in Table[Rounds[i]]) {
                    if (CourtSet.Count() != 0 && CourtSet.Contains(n)) {
                        foreach (var p in pair.Value.AsEnumerable()) {
                            players[count] = p;
                            ++count;
                        }
                        CourtSet.Remove(n);
                    } else {
                        new_table[i].Add(pair.Key, new SortedSet<int>(pair.Value.AsEnumerable<int>()));
                    }
                    ++n;
                }
                Random.Shared.Shuffle(players);
                for (int j = 0; j < pairs; ++j) {
                    int court = rnd.Next(K);
                    while (new_table[i].ContainsKey(court)) {
                        court = rnd.Next(K);
                    }
                    new_table[i].Add(court, new SortedSet<int>() {players[2 * j], players[2 * j + 1]});
                }
            }
            for (int i = rounds; i < R; ++i) {
                new_table[i] = new Dictionary<int, SortedSet<int>>();
                foreach (var pair in Table[Rounds[i]]) {
                    new_table[i].Add(pair.Key, new SortedSet<int>(pair.Value.AsEnumerable<int>()));
                }
            }
            return new TournamentTable(new_table);
        }
        public TournamentTable Crossover(TournamentTable other) {
            Random rnd = new Random();
            Dictionary<int, SortedSet<int>>[] new_table = new Dictionary<int, SortedSet<int>>[R];
            int[] RoundsThis = Enumerable.Range(0, R).ToArray();
            int[] RoundsOther = Enumerable.Range(0, R).ToArray();
            Random.Shared.Shuffle(RoundsThis);
            Random.Shared.Shuffle(RoundsOther);
            for (int i = 0; i < R / 2; ++i) {
                new_table[i] = new Dictionary<int, SortedSet<int>>();
                foreach (var pair in Table[RoundsThis[i]]) {
                    new_table[i].Add(pair.Key, new SortedSet<int>(pair.Value.AsEnumerable<int>()));
                }
            }
            for (int i = R / 2; i < R; ++i) {
                new_table[i] = new Dictionary<int, SortedSet<int>>();
                foreach (var pair in other.Table[RoundsOther[i]]) {
                    new_table[i].Add(pair.Key, new SortedSet<int>(pair.Value.AsEnumerable<int>()));
                }
            }
            return new TournamentTable(new_table);
        }
    }
}
