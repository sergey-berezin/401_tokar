using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticLibViewModel
{
    public interface IUIServices
    {
        void ErrorReport(string message);
        void ShowTable(Dictionary<int, SortedSet<int>>[] table, int num_courts);
    }
}
