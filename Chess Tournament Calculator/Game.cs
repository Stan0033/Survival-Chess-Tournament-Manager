using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Tournament_Calculator
{
    public  class Game
    {
        public string Player_White { get; set; }
        public string Player_Black { get; set; }
        public string? PGN { get; set; }
        public double Result { get; set; }
        public string End_Reason { get; set; }
        // 3fold repetition, 50-move rule,agreement, stalemate
        
        // timeout, checkmate, resignation, decided by jusge
        public Game()
        {
            Player_White = "";
            Player_Black = "";
            PGN = "";
            Result = -1;
            End_Reason = "";
        }
        public Game(string white, string black, string PGN_,   double result, string endReason)
        {
            Player_White = white;
            Player_Black = black;
            PGN = PGN_;
            Result = result;
            End_Reason = endReason;
            
        }
        public override   string ToString()
        {
            string score = string.Empty;
            
            if (Result == 1)
            {
                score = "1-0";
            }
            if (Result == 0)
            {
                score = "0-1";
            }
            if (Result == 0.5)
            {
                score = "1/2-1/2";
            }
            return $"{Player_White} - {Player_Black} {score} by {End_Reason}";
        }
    }
}
