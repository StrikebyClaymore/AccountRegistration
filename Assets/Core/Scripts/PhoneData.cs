using System;

namespace Core.Scripts
{
    [Serializable]
    public class PhoneData
    {
        public string FullNumber => Country + "(" + Operator + ")" + Number;
        public string Country { get; set; }
        public string Operator { get; set; }
        public string Number { get; set; }
    }
}