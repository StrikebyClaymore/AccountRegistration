using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Scripts
{
    public class PhoneNumberInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        private const string PhoneNumberFormat = @"^(\+7|8)\s?(\(\d{1,4}\)|\d{1,4})\s?\d{1,5}[-]?\d{1,6}$";

        public bool TryGetPhoneNumber(out PhoneData phoneData)
        {
            phoneData = null;
            var inputNumber = inputField.text;
            Match match = Regex.Match(inputNumber, PhoneNumberFormat);
            if (match.Success)
            {
                string countryCode = match.Groups[1].Value;
                string operatorCode = match.Groups[2].Value;
                string phoneNumber = match.Value;
                phoneData = new PhoneData()
                {
                    Country = countryCode,
                    Operator = operatorCode,
                    Number = phoneNumber
                };
                return true;

            }
            Debug.LogError("Invalid phone number format");
            return false;
        }
    }
}