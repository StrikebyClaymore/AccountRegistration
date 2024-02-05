using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.Scripts
{
    public class ApplicationLogic : MonoBehaviour
    {
        [SerializeField] private RootUI _rootUI;
        private const string Url = "http://45.86.183.61/Test/";
        private const string GetKeyUrl = Url + "GetKey.php";
        private const string CheckUserUrl = Url + "CheckUser.php";
        private const string RegUsersUrl = Url + "RegUsers.php";
        private const string HowManyUrl = Url + "HowMany.php";
        private string _userID;
        private string _secondUserID;
        private PhoneData _userPhoneData;
        private int _registeredUsersCount;
        private const float UpdateRegisteredInterval = 5;

        private void Awake()
        {
            _rootUI.Initialize();
            ConnectActions();
        }

        private void GetID()
        {
            StartCoroutine(SendGetIDRequest());
        }

        private IEnumerator SendGetIDRequest()
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(GetKeyUrl);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string responseText = webRequest.downloadHandler.text;
                Debug.Log("Response: " + responseText);
                ParseID(responseText);
            }
        }

        private void ParseID(string responseText)
        {
            KeyContainer keyContainer = JsonConvert.DeserializeObject<KeyContainer>(responseText);
            foreach (var keyItem in keyContainer.Keys)
            {
                if (keyItem.IsEmpty)
                    continue;
                string inputString = keyItem.Key;
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < inputString.Length; i += 2)
                    result.Append(inputString[i]);
                _userID = result.ToString();
                _secondUserID = result.Remove(result.Length - 1, 1).ToString();
                Debug.Log($"Получен ID: {_userID}");
                _rootUI.ShowMessage($"Получен ID: {_userID}");
                _rootUI.ShowScreen(RootUI.EScreens.Register);
                return;
            }
            _rootUI.ShowMessage("Не удалось получить ID");
        }
        
        private void CheckUser(PhoneData phoneData)
        {
            _userPhoneData = phoneData;
            WWWForm form = new WWWForm();
            form.AddField("ID", _userID);
            form.AddField("Phone", _userPhoneData.FullNumber);
            StartCoroutine(SendCheckUserForm(form));
        }
        
        private IEnumerator SendCheckUserForm(WWWForm form)
        {
            using UnityWebRequest www = UnityWebRequest.Post(CheckUserUrl, form);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("CheckUser form data sent successfully");
                string responseText = www.downloadHandler.text;
                Debug.Log("Server Response: " + responseText);

                if (responseText == "Exist")
                {
                    Debug.Log("Пользователь уже зарегестрирован");
                    _rootUI.ShowMessage("Пользователь уже зарегестрирован");
                    _rootUI.ShowRegistered();
                    StartCoroutine(GetRegisteredUsers());
                }
                else if (responseText == "NoExist")
                {
                    StartCoroutine(RegisterUser());
                }
            }
        }

        private IEnumerator RegisterUser()
        {
            WWWForm form = new WWWForm();
            form.AddField("ID", _secondUserID);
            form.AddField("Country", _userPhoneData.Country);
            form.AddField("Operator", _userPhoneData.Operator);
            form.AddField("Number", _userPhoneData.Number);
            
            using UnityWebRequest www = UnityWebRequest.Post(RegUsersUrl, form);
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("RegisterUser form data sent successfully");
                string responseText = www.downloadHandler.text;
                Debug.Log("Server Response: " + responseText);

                if (responseText == "RegOK")
                {
                    Debug.Log("Пользователь зарегестрирован");
                    _rootUI.ShowMessage("Пользователь зарегестрирован");
                    _rootUI.ShowRegistered();
                    StartCoroutine(GetRegisteredUsers());
                }
            }
        }
        
        private IEnumerator GetRegisteredUsers()
        {
            while (true)
            {
                using UnityWebRequest webRequest = UnityWebRequest.Get(HowManyUrl);
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + webRequest.error);
                }
                else
                {
                    string responseText = webRequest.downloadHandler.text;
                    Debug.Log("Response: " + responseText);
                    ParseUsers(responseText);
                }

                yield return new WaitForSeconds(UpdateRegisteredInterval);
            }
        }
        
        private void ParseUsers(string responseText)
        {
            var oldCount = _registeredUsersCount;
            if (int.TryParse(responseText, out _registeredUsersCount))
            {
                for (int i = 0; i < _registeredUsersCount - oldCount; i++)
                {
                    _rootUI.AddNewRegistered();
                }
            }
        }
        
        private void ConnectActions()
        {
            _rootUI.OnGetIDPressed += GetID;
            _rootUI.OnRegistrationPressed += CheckUser;
        }
    }
}