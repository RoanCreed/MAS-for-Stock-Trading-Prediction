using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using ActressMas;
using MAS_Algo_Trader;

namespace MAS_Coursework_Double_Auction
{
    public class TAA2 : Agent
    {

        public TAA2()
        {

        }

        public override void Setup()
        {
            Send("coordinatorAgent", $"registerTAA");
        }

        public override void Act(Message message)   //Program enters here after it receives a message
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out List<string> parameters);  //parsing the data coming from other agents into the action and a list of parameters
                switch (action)
                {
                    case "start":
                        Console.WriteLine($"{Name} - Initialising OBV calcluation for: " + parameters[0]);
                        HandleAnalysis(parameters[0], Convert.ToDateTime(parameters[1]));
                        break;

                    case "stop":
                        Stop();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void HandleAnalysis(string stock, DateTime startDate)
        {

            var resultFA = HandleFA(stock);
            double FAProbabilty = resultFA.Item1;
            int FASignal = resultFA.Item2;

            var resultTA = HandleOBV(stock, startDate);
            double TAProbabilty = resultTA.Item1;
            int TASignal = resultTA.Item2;

            //Weight 7/3 for TA
            double FAWeight = 0.3;
            double TAWeight = 0.7;

            double combinedTAFASignal = (TASignal * TAWeight) + (FASignal * FAWeight);
            double combinedTAFAProbabilty = 0;

            if (combinedTAFASignal == 1 || combinedTAFASignal > 0.5)
            {
                combinedTAFAProbabilty = (FAProbabilty * FAWeight) + (TAProbabilty * TAWeight);
                string content = $"taaResult {combinedTAFAProbabilty} {stock} 1";  //Probability, stock name, signal
                Send("coordinatorAgent", content);
            }
            else if (combinedTAFASignal <= 0.5)
            {
                combinedTAFAProbabilty = (FAProbabilty * FAWeight) + (TAProbabilty * TAWeight);
                string content = $"taaResult {combinedTAFAProbabilty} {stock} 0";  //Probability, stock name, signal
                Send("coordinatorAgent", content);
            }
        }

        private (double, int) HandleFA(string stock)
        {
            double probability = 0;
            int signal = 0;
            Random random = new Random();
            probability = random.NextDouble();

            if (probability >0.5)
            { signal = 1; }
            else
            { signal = -1; }


            return (probability, signal);
        }



        private (double, int) HandleOBV(string stock, DateTime startDate)
        {
            InputData dat = new InputData();
            List<InputData> d = new List<InputData>();
            d = dat.LoadData(stock);
            Console.WriteLine($"{this.Name} - calculating the on balance volume...");

            int period = 50;
            int index = d.FindIndex(d => d.date == startDate);
            int p = index;

            double currentOBV = 0;
            double previousOBV = 0;
            double previousClosePrice = 0;
            double dayBeforeOBV = 0;

            for (int i = period; i > 0; i--)
            {
                dayBeforeOBV = previousOBV;
                p--;
                if (d[p].close > previousClosePrice)
                {
                    currentOBV += d[p].volume;
                }
                else if (d[p].close < previousClosePrice)
                {
                    currentOBV -= d[p].volume;
                }
                previousClosePrice = d[p].close;
                previousOBV = currentOBV;
            }

            double probability = 0;
            int signal = 0;

            if(currentOBV > dayBeforeOBV)
            {
                probability = 1;
                signal = 1;

                Console.WriteLine($"{Name} - Stock: {stock} is on an uptrend - Current OBV: {currentOBV} - Previous OBV: {dayBeforeOBV}");
            }
            else if(currentOBV < dayBeforeOBV)
            {
                probability = 1;
                signal = 0;
                Console.WriteLine($"{Name} - Stock: {stock} is on a downtrend - Current OBV: {currentOBV} - Previous OBV: {dayBeforeOBV}");
            }
            else
            {
                probability = 0;
                signal = 0;
                Console.WriteLine($"{Name} - Stock: {stock} has no change - Current OBV: {currentOBV} - Previous OBV: {dayBeforeOBV}");
            }

            
            return (probability, signal);
        }
    }
}

