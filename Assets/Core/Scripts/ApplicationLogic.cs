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
        private delegate IEnumerator CoroutineSendRequest(string url, Action<string> callback, WWWForm form);

        private void Awake()
        {
            _rootUI.Initialize();
            ConnectActions();
        }

        private void SendGetIDRequest()
        {
            StartCoroutine(SendRequest(GetKeyUrl, HandleGetIDResult));
        }

        private void HandleGetIDResult(string responseText)
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
        
        private void SendUserRequest(PhoneData phoneData)
        {
            _userPhoneData = phoneData;
            WWWForm form = new WWWForm();
            form.AddField("ID", _userID);
            form.AddField("Phone", _userPhoneData.FullNumber);
            StartCoroutine(SendRequest(CheckUserUrl, HandleUserResult, form));
        }

        private void HandleUserResult(string responseText)
        {
            if (responseText == EResponse.NoExist.ToString())
            {
                WWWForm form = new WWWForm();
                form.AddField("ID", _secondUserID);
                form.AddField("Country", _userPhoneData.Country);
                form.AddField("Operator", _userPhoneData.Operator);
                form.AddField("Number", _userPhoneData.Number);
                StartCoroutine(SendRequest(RegUsersUrl, HandleRegisterUser, form));
            }
            else if (responseText == EResponse.Exist.ToString())
            {
                Debug.Log("Пользователь уже зарегестрирован");
                _rootUI.ShowMessage("Пользователь уже зарегестрирован");
                _rootUI.ShowRegistered();
                StartCoroutine(SendRequest(HowManyUrl, HandleRegisteredList));
            }
        }

        private void HandleRegisterUser(string responseText)
        {
            if (responseText == EResponse.RegOK.ToString())
            {
                Debug.Log("Пользователь зарегестрирован");
                _rootUI.ShowMessage("Пользователь зарегестрирован");
                _rootUI.ShowRegistered();
                StartCoroutine(SendRequest(HowManyUrl, HandleRegisteredList));
            }
        }
        
        private void HandleRegisteredList(string responseText)
        {
            var oldCount = _registeredUsersCount;
            if (int.TryParse(responseText, out _registeredUsersCount))
            {
                for (int i = 0; i < _registeredUsersCount - oldCount; i++)
                {
                    _rootUI.AddNewRegistered();
                }
            }

            StartCoroutine(CoroutineWait(UpdateRegisteredInterval, SendRequest, HowManyUrl));
        }

        private IEnumerator SendRequest(string url, Action<string> callbackAction = null, WWWForm form = null)
        {
            using UnityWebRequest www = UnityWebRequest.Post(url, form);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("Server Response: " + responseText);
                callbackAction?.Invoke(responseText);
            }
        }

        private IEnumerator CoroutineWait(float waitTime, CoroutineSendRequest waitCoroutine, string url, Action<string> callback = null, WWWForm form = null)
        {
            yield return new WaitForSeconds(waitTime);
            StartCoroutine(waitCoroutine(url, callback, form));
        }
        
        private void ConnectActions()
        {
            _rootUI.OnGetIDPressed += SendGetIDRequest;
            _rootUI.OnRegistrationPressed += SendUserRequest;
        }
    }
}