using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeStar
{
    class Player
    {
        private string _name;
        public string Name { get => _name; set => _name = value; }

        private int _number;
        public int Number { get => _number; set => _number = value; }

        public enum Teams
        {
            Instinct,
            Valor,
            Mystic
        }
        private Teams _team;
        public Teams Team { get => _team; set => _team = value; }
        

    }
}
