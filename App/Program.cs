using GeneticProgrammingLib;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
public class App {
    protected class myHandler { 
        public bool flag = false;
        public Evolution evolution; 
        public myHandler (Evolution evo) => evolution = evo; 

        public void runHandler(object? sender, ConsoleCancelEventArgs args) 
        { 
            flag = true;
            args.Cancel = true;
        }
    }
    public static void Main(string[] Args) {
        Evolution evolution = new Evolution(4, 6, 3, 0.5, 0.5, 0.8, 25, 25);
        int i = -1;
        myHandler handler = new myHandler(evolution);
        Console.CancelKeyPress += new ConsoleCancelEventHandler(handler.runHandler);  
        while (i-- != 0 && handler.flag == false) {
            evolution.Step();
            Console.WriteLine($" {evolution.Epoch} {evolution.BestRank()}");
        }
        i = 0; int N = evolution.Population.Last().N;
        foreach (var court in evolution.Population.Last().RawTable) { 
            Console.Write(court.ToString() + " "); 
            if (++i % N == 0) {
                Console.WriteLine(); 
            }
        } 
    }
}

