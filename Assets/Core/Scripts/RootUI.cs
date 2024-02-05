using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Scripts
{
    public class RootUI : MonoBehaviour
    {
        [SerializeField] private Canvas GetIDScreen;
        [SerializeField] private Canvas RegisterScreen;
        public enum EScreens
        {
            GetID = 0,
            Register = 1,
        }
        
        [SerializeField] private Button _getIDButton;
        [SerializeField] private TextMeshProUGUI _messageMesh;
        [SerializeField] private PhoneNumberInput _phoneNumberInput;
        [SerializeField] private Button _registrationButton;
        [SerializeField] private GameObject _registeredScrollView;
        [SerializeField] private Transform _registeredContainer;
        [SerializeField] private GameObject _registeredScrollItemPrefab;
        public event Action OnGetIDPressed;
        public event Action<PhoneData> OnRegistrationPressed;
        
        public void Initialize()
        {
            _getIDButton.onClick.AddListener(GetIdPressed);
            _registrationButton.onClick.AddListener(RegistrationPressed);
            ShowScreen(EScreens.GetID);
        }

        public void ShowScreen(EScreens screen)
        {
            DisableScreens();
            switch (screen)
            {
                case EScreens.GetID:
                    GetIDScreen.enabled = true;
                    break;
                case EScreens.Register:
                    RegisterScreen.enabled = true;
                    break;
            }
        }

        public void ShowMessage(string text)
        {
            _messageMesh.text = text;
        }
        
        private void DisableScreens()
        {
            GetIDScreen.enabled = false;
            RegisterScreen.enabled = false;
        }

        public void ShowRegistered()
        {
            _registeredScrollView.SetActive(true);
        }
        
        public void AddNewRegistered()
        {
            Instantiate(_registeredScrollItemPrefab, _registeredContainer);
        }
        
        private void GetIdPressed() => OnGetIDPressed?.Invoke();

        private void RegistrationPressed()
        {
            if (_phoneNumberInput.TryGetPhoneNumber(out var phoneData))
            {
                OnRegistrationPressed?.Invoke(phoneData);
                return;
            }

            ShowMessage("Номер телефона введён неверно");
        }
    }
}