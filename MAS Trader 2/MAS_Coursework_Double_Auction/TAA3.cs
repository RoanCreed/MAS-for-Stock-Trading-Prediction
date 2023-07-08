using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using ActressMas;
using MAS_Algo_Trader;

namespace MAS_Coursework_Double_Auction
{
    public class TAA3 : Agent
    {
        public TAA3()
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
                        Console.WriteLine($"{Name} - Initialising RSI calculation for: " + parameters[0]);
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

            var resultTA = HandleRSI(stock, startDate);
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

            if (probability >= 0)
            { signal = 1; }
            else
            { signal = -1; }


            return (probability, signal);
        }

        private (double, int) HandleRSI(string stock, DateTime startDate)
        {
            InputData dat = new InputData();
            List<InputData> d = new List<InputData>();
            d = dat.LoadData(stock);
            Console.WriteLine($"{Name} - calculating the relative strength index from {startDate}...");

            int period = 50;

            int index = d.FindIndex(d => d.date == startDate);
            int p = index;

            double sumGain = 0;
            double sumLoss = 0;

            for (int i = period; i > 0; i--)
            {
                double priceChange = d[p].close - d[p - 1].close;

                if (priceChange >= 0)
                {
                    sumGain += priceChange;
                }
                else
                {
                    sumLoss += Math.Abs(priceChange);
                }
                p--;
            }

            double averageGain = sumGain / period;
            double averageLoss = sumLoss / period;
            p = index;

            for (int i = period; i > 0; i--)
            {
                double priceChange = d[p].close - d[p-1].close;

                if (priceChange >= 0)
                {
                    averageGain = (averageGain * (period - 1) + priceChange) / period;
                    averageLoss = (averageLoss * (period - 1)) / period;
                }
                else
                {
                    averageGain = (averageGain * (period - 1)) / period;
                    averageLoss = (averageLoss * (period - 1) + Math.Abs(priceChange)) / period;
                }
                p--;
            }

            double rs = averageGain / averageLoss;
            double rsi = 100 - (100 / (1 + rs));

            double probability = 0;
            int signal = 0;

            if (rsi > 70)   //If high then stock is valued as overbought so should sell
            {
                Console.WriteLine($"{Name} - Stock: {stock} has high RSI strength - Current RSI: {rsi} - Overbought");
                probability = 1;
                signal = 0;
            }
            else if (rsi < 30)  //If high then stock is valued as oversold so should buy
            {
                Console.WriteLine($"{Name} - Stock: {stock} has low RSI strength - Current RSI: {rsi} - Oversold");
                probability = 1;
                signal = 1;
            }
            else
            {
                Console.WriteLine($"{Name} - Stock: {stock} RSI has no imbalance - Current RSI: {rsi}");
                probability = 0;
                signal = 0;
            }

            return (probability, signal);
        }
    }
}

