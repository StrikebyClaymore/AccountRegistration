using System;

namespace Core.Scripts
{
    [Serializable]
    public class KeyItem
    {
        private const string EmptyKey = "No Key";
        public string Key { get; set; }
        public bool IsEmpty => Key == EmptyKey;
    }
}