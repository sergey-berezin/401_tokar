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
    public static void Main(string[] Args) {
        AsyncEvolution evolution = new AsyncEvolution(40, 60, 30, 0.5, 0.5, 0.8, 2500, 2500);
        int i = 1;
        myHandler handler = new myHandler(evolution);
        Console.CancelKeyPress += new ConsoleCancelEventHandler(handler.runHandler);  
        while (i-- != 0 && handler.flag == false) {
            evolution.Step();
            Console.WriteLine($" {evolution.Epoch} {evolution.BestRank()}");
        }
    }
}

