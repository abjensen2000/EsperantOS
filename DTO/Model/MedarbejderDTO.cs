using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Model
{
    public class MedarbejderDTO
    {
        private int _id;
        private string _name;
        private bool bestyrelsesmedlem;
        private List<VagtDTO> vagter;

        public MedarbejderDTO(int id, string name, bool bestyrelsesmedlem)
        {
            _id = id;
            _name = name;
            this.bestyrelsesmedlem = bestyrelsesmedlem;
        }

        public int Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public bool Bestyrelsesmedlem { get => bestyrelsesmedlem; set => bestyrelsesmedlem = value; }
        public List<VagtDTO> Vagter { get => vagter; set => vagter = value; }

    }
}
