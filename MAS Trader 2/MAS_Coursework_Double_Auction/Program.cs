﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ActressMas;
using MAS_Algo_Trader;

namespace MAS_Coursework_Double_Auction
{
    //Version 2

    public class Program
    {
        public static List<string> stockList = new List<string>();
        public static int numTAA = 3;
        public static int numFAA = 1;
        public static int numStocks = 0;
        public static int turns = 0;

        
        
        

        static void Main(string[] args)
        {

            
            string[] fileArray = Directory.GetFiles(@"/Users/roancreed/Desktop/University/FourthYear/Dissertation/Test_Algo_trader/MAS_Coursework_Double_Auction/Data/", "*.csv");

            foreach (string fileName in fileArray)
            {
                stockList.Add(Path.GetFileNameWithoutExtension(fileName));
            }
            

            //stockList.AddRange(new string[] { "AAPL", "NFLX", "QQQ", "SPY", "TSLA" });

            numStocks = stockList.Count;


            

            if(numFAA > numTAA)
            { turns = numFAA;}
            else
            {turns = numTAA;}

            Console.WriteLine("Starting Agents");

            //Start environment
            var env = new EnvironmentMas();

            //var dataAgent = new DataAgent();
            //env.Add(dataAgent, $"dataAgent");


            var TAA1 = new TAA1();
            env.Add(TAA1, $"TAA1");
            var TAA2 = new TAA2();
            env.Add(TAA2, $"TAA2");
            var TAA3 = new TAA3();
            env.Add(TAA3, $"TAA3");

            
            //var FAA1 = new FAA1();
            //env.Add(FAA1, $"FAA1");
            




            var coordinatorAgent = new CoordinatorAgent();
            env.Add(coordinatorAgent, "coordinatorAgent");

            env.Start();

            //Once shutting down
            Console.WriteLine("Program shutting down");

        }
    }




    
}

