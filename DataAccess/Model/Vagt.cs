using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Model
{
    public class Vagt
    {
        private int _id;
        private DateTime _dato;
        private bool _ædru;
        private bool _frigivet;
        private List<Medarbejder> _medarbejdere;

        public Vagt(int id, DateTime dato, bool ædru, bool frigivet)
        {
            this._id = id;
            this._dato = dato;
            this._ædru = ædru;
            this._frigivet = frigivet;
        }

        public int Id { get => _id; set => _id = value; }
        public DateTime Dato { get => _dato; set => _dato = value; }
        public bool Ædru { get => _ædru; set => _ædru = value; }
        public bool Frigivet { get => _frigivet; set => _frigivet = value; }

        public List<Medarbejder> Medarbejdere { get => _medarbejdere; set => _medarbejdere = value; }
    }
}
