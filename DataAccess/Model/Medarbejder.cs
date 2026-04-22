using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Model
{
    public class Medarbejder
    {
        private int _id;
        private string _name;
        private bool bestyrelsesmedlem;
        private List<Vagt> vagter;
        private string _passwordHash;

        public Medarbejder(int id, string name, bool bestyrelsesmedlem, string passwordHash)
        {
            _id = id;
            _name = name;
            this.bestyrelsesmedlem = bestyrelsesmedlem;
            _passwordHash = passwordHash;
        }

        public int Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public bool Bestyrelsesmedlem { get => bestyrelsesmedlem; set => bestyrelsesmedlem = value; }
        public List<Vagt> Vagter { get => vagter; set => vagter = value; }
        public string PasswordHash { get => _passwordHash; set => _passwordHash = value; }
    }
}
