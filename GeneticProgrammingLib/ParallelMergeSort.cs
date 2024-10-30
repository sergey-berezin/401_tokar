namespace GeneticProgrammingLib;
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
        const int threshold = 1000;
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

public class ParallelMergeSort
{
    public static void Sort(IComparable[] array)
    {
        IComparable[] tempArray = new IComparable[array.Length];
        ParallelMergeSortRecursive(array, tempArray, 0, array.Length);
    }
    private static void ParallelMergeSortRecursive(IComparable[] array, IComparable[] tempArray, int left, int right)
    {
        const int threshold = 1000;
        if (right - left < 2)
            return;
        if (right - left < threshold)
        {
            Array.Sort(array, left, right - left);
            return;
        }
        int middle = (left + right) / 2;
        Parallel.Invoke(
            () => ParallelMergeSortRecursive(array, tempArray, left, middle),
            () => ParallelMergeSortRecursive(array, tempArray, middle, right)
        );
        Merge(array, tempArray, left, middle, right);
    }

    private static void Merge(IComparable[] array, IComparable[] tempArray, int left, int middle, int right)
    {
        int i = left;
        int j = middle;
        int k = left;
        while (i < middle && j < right)
        {
            if (array[i].CompareTo(array[j]) != 1)
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