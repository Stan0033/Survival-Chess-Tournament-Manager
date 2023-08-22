using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_Tournament_Calculator
{
    public class Player
    {
       public string Name { get; set; }
        public double FIDE_Rating { get; set; }
       public   bool LastColor_White { get; set; }
        public bool Eliminated { get; set; }
        public bool Disqualified { get; set; }
        public List<Game> Games { get; set; }

        //---------------------------------------------
        public Player() { Eliminated = false; Games = new List<Game>(); }
        public Player(string name) { Name = name; Eliminated = false; Games = new List<Game>(); }
        public Player(string name, double FIDERating) { Name = name; FIDE_Rating = FIDERating; Eliminated = false; Games = new List<Game>(); }
        public Player(string name, bool PlaysWhite) { Name = name; LastColor_White = PlaysWhite; Eliminated = false; Games = new List<Game>(); }

    }
}
