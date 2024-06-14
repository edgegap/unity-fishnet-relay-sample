using System;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.KCP.Edgegap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        [SerializeField] private string _relayProfileToken = "RELAY_PROFILE_TOKEN";
        [SerializeField] private bool _autoDestroySessionOnStop = true;
        [SerializeField] private TMP_InputField _sessionIdInputField;
        [SerializeField] private Button _createSessionButton;
        [SerializeField] private Button _joinSessionButton;
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private EdgegapKcpTransport _transport;

        private bool _waitingForResponse;

        private void Awake()
        {
            _transport.OnClientConnectionState += OnClientConnectionState;
            _transport.OnServerConnectionState += OnServerConnectionState;
            EdgegapRelayService.Initialize(_relayProfileToken);
            _createSessionButton.onClick.AddListener(CreateSession);
            _joinSessionButton.onClick.AddListener(JoinSession);
        }

        private async void CreateSession()
        {
            SetButtonsInteractable(false);
            ApiResponse data = await EdgegapRelayService.CreateSessionAsync(2);
            _sessionIdInputField.text = data.session_id;

            //Convert uint? to uint
            uint sessionAuthorizationToken = data.authorization_token ?? 0;
            uint userAuthorizationToken = data.session_users?[0].authorization_token ?? 0;

            Relay relay = data.relay;
            string address = relay.ip;
            ushort serverPort = relay.ports.server.port;
            ushort clientPort = relay.ports.client.port;
            var relayData = new EdgegapRelayData(address, serverPort, clientPort, userAuthorizationToken, sessionAuthorizationToken);
            _transport.SetEdgegapRelayData(relayData);
            _transport.StartConnection(true);
            _transport.StartConnection(false);
            SetButtonsInteractable(_transport.GetConnectionState(true) == LocalConnectionState.Stopped);
        }

        private async void JoinSession()
        {
            SetButtonsInteractable(false);
            ApiResponse data = await EdgegapRelayService.JoinSessionAsync(_sessionIdInputField.text);
            
            uint sessionAuthorizationToken = data.authorization_token ?? 0;
            uint userAuthorizationToken = data.session_users?[1].authorization_token ?? 0;
            
            Relay relay = data.relay;
            string address = relay.ip;
            ushort serverPort = relay.ports.server.port;
            ushort clientPort = relay.ports.client.port;
            var relayData = new EdgegapRelayData(address, serverPort, clientPort, userAuthorizationToken, sessionAuthorizationToken);
            _transport.SetEdgegapRelayData(relayData);
            _transport.StartConnection(false);
            SetButtonsInteractable(_transport.GetConnectionState(false) == LocalConnectionState.Stopped);
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case LocalConnectionState.Stopped:
                    SetButtonsInteractable(true);
                    if(_autoDestroySessionOnStop && !string.IsNullOrWhiteSpace(_sessionIdInputField.text))
                        EdgegapRelayService.DeleteSessionAsync(_sessionIdInputField.text);
                    break;
                case LocalConnectionState.Starting:
                case LocalConnectionState.Started:
                    SetButtonsInteractable(false);
                    break;
                case LocalConnectionState.Stopping:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case LocalConnectionState.Stopped:
                    SetButtonsInteractable(_transport.GetConnectionState(true) == LocalConnectionState.Stopped);
                    break;
                case LocalConnectionState.Starting:
                case LocalConnectionState.Started:
                    SetButtonsInteractable(false);
                    break;
                case LocalConnectionState.Stopping:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            _createSessionButton.gameObject.SetActive(interactable);
            _joinSessionButton.gameObject.SetActive(interactable);
        }
    }
}