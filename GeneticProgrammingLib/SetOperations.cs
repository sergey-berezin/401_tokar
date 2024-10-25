namespace GeneticProgrammingLib;
public class SetOperations<T>
{
    public static T[] Difference(T[] arr1, T[] arr2, IComparer<T> cmp)
    {
        List<T> result = new List<T>();
        int i = 0, j = 0;

        while (i < arr1.Length) {
            if (j >= arr2.Length || cmp.Compare(arr1[i], arr2[j]) == -1) {
                result.Add(arr1[i]); 
                i++;
            }
            else if (cmp.Compare(arr1[i], arr2[j]) == 1)
                j++;
            else {
                i++;
                j++;
            }
        }

        return result.ToArray();
    }
    public static T[] Union(T[] arr1, T[] arr2, IComparer<T> cmp)
    {
        List<T> result = new List<T>();
        int i = 0, j = 0;

        while (i < arr1.Length && j < arr2.Length) {
            if (cmp.Compare(arr1[i], arr2[j]) == -1) {
                result.Add(arr1[i]);
                i++;
            }
            else if (cmp.Compare(arr1[i], arr2[j]) == 1) {
                result.Add(arr2[j]);
                j++;
            }
            else {
                result.Add(arr1[i]);
                i++;
                j++;
            }
        }

        while (i < arr1.Length) {
            result.Add(arr1[i]);
            i++;
        }

        while (j < arr2.Length) {
            result.Add(arr2[j]);
            j++;
        }

        return result.ToArray();
    }
}