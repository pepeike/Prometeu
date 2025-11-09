using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This struct holds the networked state for a single player's character choice.
/// It is made INetworkSerializable so it can be used in a NetworkList.
/// </summary>
public struct PlayerCharacterChoice : INetworkSerializable, IEquatable<PlayerCharacterChoice> {
    public ulong ClientId;
    // Character ID: 1 for Character A, 2 for Character B, 0 for unselected
    public int CharacterId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
    }

    public bool Equals(PlayerCharacterChoice other) {
        return ClientId == other.ClientId && CharacterId == other.CharacterId;
    }

    public override bool Equals(object obj) {
        return obj is PlayerCharacterChoice other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(ClientId, CharacterId);
    }
}

/// <summary>
/// Manages character selection logic, exclusivity, and scene transition for the Host.
/// Attach this script to a GameObject in your Character Select Scene.
/// </summary>
public class CharacterSelectManager : NetworkBehaviour {
    // --- UI References (Assign in Inspector) ---
    [Header("UI Elements")]
    [SerializeField] private Button selectCharacterAButton;
    [SerializeField] private Button selectCharacterBButton;
    [SerializeField] private Button proceedButton;
    [SerializeField] private TMP_Text statusText;

    // --- Configuration ---
    [Header("Scene and Prefabs")]
    // The scene to load after selection is complete. (Host only)
    [SerializeField] private string gameSceneName = "GameScene";

    // --- Networked State ---
    // A list to track the choices of all connected players.
    private NetworkList<PlayerCharacterChoice> playerChoices;

    // A dictionary to map ClientId to their chosen character for easy lookup
    private readonly Dictionary<ulong, int> clientChoices = new Dictionary<ulong, int>();

    // --- Static Data for Spawning (Will be used after scene load) ---
    // Since we can't easily pass complex data through the scene load, 
    // we use a static dictionary to store the final choices before loading.
    public static Dictionary<ulong, int> FinalCharacterChoices { get; private set; } = new Dictionary<ulong, int>();


    private void Awake() {
        // Initialize the networked list
        playerChoices = new NetworkList<PlayerCharacterChoice>();
    }

    public override void OnNetworkSpawn() {
        // Subscribe to changes in the playerChoices list
        playerChoices.OnListChanged += OnCharacterListChanged;

        // Add current player to the list if they are not already there
        if (IsServer) {
            // The Server/Host needs to initialize its own entry on connection
            foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
                AddClientEntry(clientId);
            }

            // Listen for new client connections to add their entry
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            // Listen for client disconnections to remove their entry
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        SetupUI();

        // If a client joins late, we need to manually sync their UI once
        if (!IsServer) {
            UpdateLocalUI();
        }
    }

    public override void OnNetworkDespawn() {
        // Clean up subscriptions
        playerChoices.OnListChanged -= OnCharacterListChanged;

        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void SetupUI() {
        selectCharacterAButton.onClick.RemoveAllListeners();
        selectCharacterAButton.onClick.AddListener(() => OnSelectCharacterClicked(1));
        selectCharacterBButton.onClick.RemoveAllListeners();
        selectCharacterBButton.onClick.AddListener(() => OnSelectCharacterClicked(2));

        if (IsHost) {
            proceedButton.gameObject.SetActive(true);
            proceedButton.onClick.RemoveAllListeners();
            proceedButton.onClick.AddListener(CheckAndProceed);
        } else {
            proceedButton.gameObject.SetActive(false);
        }

        statusText.text = "Choose your character";
    }

    private void HandleClientConnected(ulong clientId) {
        AddClientEntry(clientId);
        Debug.Log($"Client {clientId} connected and added to character selection.");
    }

    private void HandleClientDisconnected(ulong clientId) {
        RemoveClientEntry(clientId);
    }

    private void AddClientEntry(ulong clientId) {
        bool exists = false;
        foreach (var choice in playerChoices) {
            if (choice.ClientId == clientId) {
                exists = true;
                break;
            }
        }

        if (!exists) {
            playerChoices.Add(new PlayerCharacterChoice { ClientId = clientId, CharacterId = 0 });
        }
    }

    private void RemoveClientEntry(ulong clientId) {
        for (int i = 0; i < playerChoices.Count; i++) {
            if (playerChoices[i].ClientId == clientId) {
                playerChoices.RemoveAt(i);
                break;
            }
        }
    }

    public void OnSelectCharacterClicked(int characterId) {
        
        SubmitCharacterChoiceServerRpc(characterId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitCharacterChoiceServerRpc(int characterId, ServerRpcParams rpcParams = default) {
        ulong clientId = rpcParams.Receive.SenderClientId;
        int choiceIndex = -1;

        for (int i = 0; i < playerChoices.Count; i++) {
            if (playerChoices[i].ClientId == clientId) {
                choiceIndex = i;
                break;
            }
        }

        if (choiceIndex == -1) {
            Debug.LogError($"Client ID {clientId} not found in playerChoices.");
            return;
        }

        int currentChoiceId = playerChoices[choiceIndex].CharacterId;
        int finalChoiceId = 0;

        if (characterId != currentChoiceId) {
            bool isTaken = false;
            foreach (var choice in playerChoices) {
                if (choice.CharacterId == characterId) {
                    isTaken = true;
                    break;
                }
            }
            if (isTaken) {
                Debug.Log($"Client {clientId} failed to select Character {characterId}: Already taken.");
                return;
            }
            finalChoiceId = characterId;
        }

        PlayerCharacterChoice newChoice = playerChoices[choiceIndex];
        newChoice.CharacterId = finalChoiceId;
        playerChoices[choiceIndex] = newChoice;

        Debug.Log($"Client {clientId} chose Character {finalChoiceId}");

    }

    private void OnCharacterListChanged(NetworkListEvent<PlayerCharacterChoice> changeEvent) {
        UpdateLocalUI();

        if (IsHost) {
            CheckProceedButtonState();
        }
    }

    private void UpdateLocalUI() {
        clientChoices.Clear();
        int localChoice = 0;
        int takenA = 0;
        int takenB = 0;

        foreach (var choice in playerChoices) {
            clientChoices[choice.ClientId] = choice.CharacterId;

            if (choice.CharacterId == 1) takenA++;
            if (choice.CharacterId == 2) takenB++;

            if (choice.ClientId == NetworkManager.Singleton.LocalClientId) {
                localChoice = choice.CharacterId;
            }
        }

        if (localChoice == 1) {
            statusText.text = "You have selected Character A.";
        } else if (localChoice == 2) {
            statusText.text = "You have selected Character B.";
        } else {
            statusText.text = "Choose your character";
        }

        selectCharacterAButton.interactable = (takenA == 0) || (localChoice == 1);
        selectCharacterBButton.interactable = (takenB == 0) || (localChoice == 2);
    }

    private void CheckProceedButtonState() {
        bool noUnselected = false;
        foreach (var choice in playerChoices) {
            if (choice.CharacterId == 0) {
                noUnselected = false;
                break;
            }
            noUnselected = true;
        }
        bool allSelected = playerChoices.Count > 0 && noUnselected;

        if (allSelected) {
            proceedButton.interactable = true;
            statusText.text = "All players have selected their characters. Host can proceed.";
        } else {
            proceedButton.interactable = false;
            statusText.text = "Waiting for all players to select their characters.";
        }
    }

    public void CheckAndProceed() {
        if (!IsHost || !proceedButton.interactable) {
            Debug.LogWarning("Only the Host can proceed when all players have selected.");
            return;
        }

        // Convert NetworkList to a Dictionary without using LINQ's ToList
        FinalCharacterChoices = new Dictionary<ulong, int>();
        foreach (var choice in playerChoices) {
            FinalCharacterChoices[choice.ClientId] = choice.CharacterId;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

}
