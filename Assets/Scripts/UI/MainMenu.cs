using System;
using NetWorking.Client;
using NetWorking.Client.Services;
using NetWorking.Host;
using TMPro;
using Unity.Services.Lobbies;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Text queueStatusText;
        [SerializeField] private TMP_Text queueTimerText;
        [SerializeField] private TMP_Text findMatchButtonText;
        [SerializeField] private TMP_InputField joinCodeField;
        [SerializeField] private Toggle teamToggle;
        [SerializeField] private Toggle privateToggle;

        private bool isMatchmaking;
        private bool isCanceling;
        private bool isBusy;
        private float timeInQueue;
        
        private void Start()
        {
            if (ClientSingleton.Instance == null) { return; }
            
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            
            queueStatusText.text = string.Empty;
            queueTimerText.text = string.Empty;
        }

        private void Update()
        {
            if (isMatchmaking)
            {
                timeInQueue += Time.deltaTime;
                TimeSpan ts = TimeSpan.FromSeconds(timeInQueue);
                queueTimerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
            }
        }

        public async void FindMatchPressed()
        {
            if (isCanceling) {return;}
            
            if (isMatchmaking)
            {
                queueTimerText.text = "Canceling...";
                isCanceling = true;
                await ClientSingleton.Instance.GameManager.CancelMatchmaking();
                isCanceling = false;
                isMatchmaking = false;
                isBusy = false;
                findMatchButtonText.text = "Find Match";
                queueStatusText.text = string.Empty;
                queueTimerText.text = string.Empty;
                return;
            }

            if (isBusy) { return; }
            
            ClientSingleton.Instance.GameManager.MatchmakeAsync(teamToggle.isOn, OnMatchMade);
            findMatchButtonText.text = "Cancel";
            queueStatusText.text = "Searching...";
            timeInQueue = 0f;
            isMatchmaking = true;
            isBusy = true;
        }

        private void OnMatchMade(MatchmakerPollingResult result)
        {
            switch (result)
            {
                case MatchmakerPollingResult.Success:
                    queueStatusText.text = "Connecting...";
                    break;
                case MatchmakerPollingResult.TicketCreationError:
                    queueStatusText.text = "Ticket creation error";
                    break;
                case MatchmakerPollingResult.TicketCancellationError:
                    queueStatusText.text = "Ticket cancellation error";
                    break;
                case MatchmakerPollingResult.TicketRetrievalError:
                    queueStatusText.text = "ticket retrieval error";
                    break;
                case MatchmakerPollingResult.MatchAssignmentError:
                    queueStatusText.text = "match assignment error";
                    break;
            }
        }

        public async void StartHost()
        {
            if (isBusy) { return; }
            
            isBusy = true;
            
            await HostSingleton.Instance.GameManager.StartHostAsync(privateToggle.isOn);
            
            isBusy = false;
        }

        public async void StartClient()
        {
            if (isBusy) { return; }
            
            isBusy = true;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
            
            isBusy = false;
        }
        
        public async void JoinAsync(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            if (isBusy) {return; }
        
            isBusy = true;
        
            try
            {
                Unity.Services.Lobbies.Models.Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
                string joinCode = joiningLobby.Data["JoinCode"].Value;

                await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            isBusy = false;
        }
    
    }
}
