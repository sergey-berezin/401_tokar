using GeneticProgrammingLib;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
public class App {
    protected class myHandler { 
        public bool flag = false;
        public AsyncEvolution evolution; 
        public myHandler (AsyncEvolution evo) => evolution = evo; 

        public void runHandler(object? sender, ConsoleCancelEventArgs args) 
        { 
            flag = true;
            args.Cancel = true;
        }
    }
    public class SortState {
        public int l, r, idx;
        public int[] arr, tmp;
        public Task[] tasks;
        public IComparer<int> cmp;
        public SortState(int l, int r, int idx, int[] arr, Task[] tasks, IComparer<int> cmp) {
            this.l = l;
            this.r = r;
            this.idx = idx;
            this.arr = arr;
            this.tasks = tasks;
            this.cmp = cmp;
            this.tmp = new int[arr.Length];
        }
    }
    public static void merge (SortState state) {
        int r = state.r, l = state.l;
        int[] arr = state.arr; Task[] tasks = state.tasks;
        IComparer<int> cmp = state.cmp;
        int[] tmp = state.tmp;
        int m = (r + l) / 2;
        int idx = l;
        int i = l;
        int j = m;
        while (i < m && j < r) {
            if (cmp.Compare(arr[i] , arr[j]) == -1) {
                tmp[idx] = arr[i];
                i++;
            }
            else {
                tmp[idx] = arr[j];
                j++;
            }
            idx++;
        }
        int s = idx;
        for (; i < m; ++i) {
            arr[idx] = arr[i];
            idx++;
        }
        for (; j < r; ++j) {
            arr[idx] = arr[j];
            idx++;
        }
        for (int k = 0; k < s - l; ++k) {
            arr[l + k] = tmp[l + k];
        }
    }
    public static void merge_sort(object ?args) {
        SortState state = (SortState)args;
        int l = state.l, r = state.r, idx = state.idx;
        if (r - l < 2) return;
        int[] arr = state.arr; Task[] tasks = state.tasks;
        IComparer<int> cmp = state.cmp;
        int m = (r + l) / 2;
        tasks[2 * idx] = Task.Factory.StartNew(merge_sort, new SortState(l, m, 2 * idx, arr, tasks, cmp));
        tasks[2 * idx + 1] = Task.Factory.StartNew(merge_sort, new SortState(m, r, 2 * idx + 1, arr, tasks, cmp));

        // merge_sort( new SortState(l, m, 2 * idx, arr, tasks, cmp));
        // merge_sort( new SortState(m, r, 2 * idx + 1, arr, tasks, cmp));
        Task.WaitAll(tasks[2 * idx], tasks[2 * idx + 1]);
        merge(state);
    }
    public static void MergeSort(int[] arr, IComparer<int> cmp) {
        Task[] tasks = new Task[5 * arr.Length];
        merge_sort(new SortState(0, arr.Length, 1, arr, tasks, cmp));
    }
    public class IntComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x < y) return -1;
            if (x == y) return 0;
            return 1;
        }
    }
    public class ParallelQuickSort
{
    public static void Sort(int[] array)
    {
        ParallelQuickSortRecursive(array, 0, array.Length - 1);
    }

    private static void ParallelQuickSortRecursive(int[] array, int left, int right)
    {
        const int threshold = 1000; // Порог для последовательной сортировки

        if (left >= right)
            return;

        if (right - left < threshold)
        {
            Array.Sort(array, left, right - left + 1); // Маленькие подмассивы сортируются последовательно
            return;
        }

        int pivot = Partition(array, left, right);

        // Рекурсивная параллельная сортировка для левой и правой частей
        Parallel.Invoke(
            () => ParallelQuickSortRecursive(array, left, pivot - 1),
            () => ParallelQuickSortRecursive(array, pivot + 1, right)
        );
    }

    private static int Partition(int[] array, int left, int right)
    {
        int pivot = array[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (array[j] <= pivot)
            {
                i++;
                Swap(array, i, j);
            }
        }

        Swap(array, i + 1, right);
        return i + 1;
    }

    private static void Swap(int[] array, int i, int j)
    {
        int temp = array[i];
        array[i] = array[j];
        array[j] = temp;
    }
}
public class ParallelMergeSort<T>
{
    public class Comparer : IComparer<T> {
        Func<T, T, int> cmp;
        public Comparer(Func<T, T, int> cmp) {
            this.cmp = cmp;
        }
        public int Compare(T? x, T? y) => cmp(x, y);
    }
    public static void Sort(T[] array, Func<T, T, int> cmp)
    {
        T[] tempArray = new T[array.Length];
        ParallelMergeSortRecursive(array, tempArray, 0, array.Length, new Comparer(cmp));
    }

    public static void Sort(T[] array, IComparer<T>cmp)
    {
        T[] tempArray = new T[array.Length];
        ParallelMergeSortRecursive(array, tempArray, 0, array.Length, cmp);
    }


    private static void ParallelMergeSortRecursive(T[] array, T[] tempArray, int left, int right, IComparer<T> cmp)
    {
        const int threshold = 1000000;
        if (right - left < 2)
            return;
        if (right - left < threshold)
        {
            Array.Sort(array, left, right - left, cmp);
            return;
        }
        int middle = (left + right) / 2;
        Parallel.Invoke(
            () => ParallelMergeSortRecursive(array, tempArray, left, middle, cmp),
            () => ParallelMergeSortRecursive(array, tempArray, middle, right, cmp)
        );
        Merge(array, tempArray, left, middle, right, cmp);
    }

    private static void Merge(T[] array, T[] tempArray, int left, int middle, int right, IComparer<T> cmp)
    {
        int i = left;
        int j = middle;
        int k = left;
        while (i < middle && j < right)
        {
            if (cmp.Compare(array[i], array[j]) != 1)
                tempArray[k++] = array[i++];
            else
                tempArray[k++] = array[j++];
        }
        while (i < middle)
            tempArray[k++] = array[i++];
        while (j < right)
            tempArray[k++] = array[j++];
        for (i = left; i < right; i++)
        {
            array[i] = tempArray[i];
        }
    }
}
    public static void Main(string[] Args) {
        AsyncEvolution evolution = new AsyncEvolution(40, 60, 30, 0.5, 0.5, 0.8, 2500, 2500);
        int i = 1;
        myHandler handler = new myHandler(evolution);
        Console.CancelKeyPress += new ConsoleCancelEventHandler(handler.runHandler);  
        while (i-- != 0 && handler.flag == false) {
            evolution.Step();
            Console.WriteLine($" {evolution.Epoch} {evolution.BestRank()}");
        }
        // Stopwatch stopwatch = new Stopwatch();

        // int n = 100000000;
        // int[] Rounds = Enumerable.Range(0, n).ToArray();
        // Random.Shared.Shuffle(Rounds);
        // int[] Rounds2 = new int[n]; Array.Copy(Rounds, Rounds2, n);
        // int[] Rounds3 = new int[n]; Array.Copy(Rounds, Rounds3, n);

        // stopwatch.Reset();
        // stopwatch.Start();
        // ParallelMergeSort<int>.Sort(Rounds, new IntComparer());
        // stopwatch.Stop();
        // Console.WriteLine($"ParallelMergeSort TIME: {stopwatch.ElapsedTicks} ticks {stopwatch.ElapsedMilliseconds} ms");

        // stopwatch.Reset();
        // stopwatch.Start();
        // ParallelQuickSort.Sort(Rounds3);
        // stopwatch.Stop();
        // Console.WriteLine($"ParallelQuickSort TIME: {stopwatch.ElapsedTicks} ticks {stopwatch.ElapsedMilliseconds} ms");
            
        // stopwatch.Reset();
        // stopwatch.Start();
        // Array.Sort(Rounds2, new IntComparer());
        // stopwatch.Stop();
        // Console.WriteLine($"Sort TIME: {stopwatch.ElapsedTicks} ticks {stopwatch.ElapsedMilliseconds} ms");
            
        // foreach (var a in Rounds) {
        //     Console.WriteLine(a);
        // }
        // i = 0; int N = evolution.Population.Last().N;
        // foreach (var court in evolution.Population.Last().RawTable) { 
        //     Console.Write(court.ToString() + " "); 
        //     if (++i % N == 0) {
        //         Console.WriteLine(); 
        //     }
        // } 
    }
}

