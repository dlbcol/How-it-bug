using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;

public class HowItBugSessionManager : MonoBehaviour
{
    private ISession currentSession;

    private string joinCodeInput = "";
    private string status = "Starting...";
    private bool servicesReady = false;

    public static bool PauseMenuOpen { get; private set; }

    private string lobbyCode = "";

    private async void Start()
    {
        await InitializeServices();
    }

    private void Update()
    {
        // Only allow the pause menu while inside a multiplayer session.
        if (currentSession == null)
        {
            PauseMenuOpen = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenuOpen = !PauseMenuOpen;

            if (PauseMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    private async System.Threading.Tasks.Task InitializeServices()
    {
        try
        {
            status = "Initializing Unity Services...";

            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            servicesReady = true;
            status = "Ready";

            Debug.Log(
                $"UGS initialized. Player ID: {AuthenticationService.Instance.PlayerId}"
            );
        }
        catch (Exception e)
        {
            status = "Failed to initialize Unity Services";
            Debug.LogException(e);
        }
    }

    private async void HostGame()
    {
        if (!servicesReady)
            return;

        try
        {
            status = "Creating session...";

            SessionOptions options = new SessionOptions
            {
                // Host + 3 friends
                MaxPlayers = 4
            }
            .WithRelayNetwork();

            currentSession =
                await MultiplayerService.Instance.CreateSessionAsync(options);

            lobbyCode = currentSession.Code;

            status = $"Hosting! Code: {currentSession.Code}";

            Debug.Log(
                $"HowItBug session created. Join code: {currentSession.Code}"
            );
        }
        catch (Exception e)
        {
            status = "Failed to host";
            Debug.LogException(e);
        }
    }

    private async void JoinGame()
    {
        if (!servicesReady)
            return;

        if (string.IsNullOrWhiteSpace(joinCodeInput))
        {
            status = "Enter a join code.";
            return;
        }

        try
        {
            status = "Joining...";

            string code = joinCodeInput.Trim().ToUpperInvariant();

            currentSession =
                await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            lobbyCode = currentSession.Code;

            status = "Joined!";

            Debug.Log(
                $"Joined session {currentSession.Id}"
            );
        }
        catch (Exception e)
        {
            status = "Failed to join";
            Debug.LogException(e);
        }
    }

    private async void ReconnectGame()
    {
        try
        {
            status = "Checking previous session...";

            var sessionIds =
                await MultiplayerService.Instance
                    .GetJoinedSessionIdsAsync();

            if (sessionIds.Count == 0)
            {
                status = "No session to reconnect to.";
                return;
            }

            status = "Reconnecting...";

            currentSession =
                await MultiplayerService.Instance
                    .ReconnectToSessionAsync(sessionIds[0]);

            lobbyCode = currentSession.Code;

            status = "Reconnected!";

            Debug.Log(
                $"Reconnected to session: {currentSession.Id}"
            );
        }
        catch (SessionException e)
        {
            status = "Failed to reconnect.";
            Debug.LogException(e);
        }
    }

    private async void LeaveGame()
    {
        if (currentSession == null)
            return;

        try
        {
            if (currentSession.IsHost)
            {
                status = "Ending session...";

                await currentSession
                    .AsHost()
                    .DeleteAsync();

                Debug.Log("Host ended the session.");
            }
            else
            {
                status = "Leaving session...";

                await currentSession.LeaveAsync();

                Debug.Log("Left the session.");
            }

            currentSession = null;
            lobbyCode = "";

            PauseMenuOpen = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            status = "Left session";
        }
        catch (Exception e)
        {
            status = "Failed to leave session";
            Debug.LogException(e);
        }
    }

    private void ClosePauseMenu()
    {
        PauseMenuOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnGUI()
    {
        // Not currently in a session:
        // Show the Host / Join / Reconnect menu.
        if (currentSession == null)
        {
            GUILayout.BeginArea(
                new Rect(20, 20, 350, 260),
                GUI.skin.box
            );

            GUILayout.Label("HOW IT BUG - MULTIPLAYER");

            GUILayout.Space(10);

            GUILayout.Label($"Status: {status}");

            GUILayout.Space(15);

            GUI.enabled = servicesReady;

            if (GUILayout.Button("HOST GAME"))
            {
                HostGame();
            }

            GUILayout.Space(10);

            GUILayout.Label("Join Code:");

            joinCodeInput = GUILayout.TextField(joinCodeInput);

            if (GUILayout.Button("JOIN GAME"))
            {
                JoinGame();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("RECONNECT"))
            {
                ReconnectGame();
            }

            GUI.enabled = true;

            GUILayout.EndArea();

            return;
        }

        // Currently inside a session:
        // Always show the lobby code at the top of the screen.
        GUIStyle codeStyle = new GUIStyle(GUI.skin.label);
        codeStyle.alignment = TextAnchor.MiddleCenter;
        codeStyle.fontSize = 20;
        codeStyle.fontStyle = FontStyle.Bold;

        GUI.Label(
            new Rect(
                Screen.width / 2f - 150,
                20,
                300,
                40
            ),
            $"LOBBY CODE: {lobbyCode}",
            codeStyle
        );

        // Only show the menu when Escape has been pressed.
        if (PauseMenuOpen)
        {
            float menuWidth = 300;
            float menuHeight = 170;

            GUILayout.BeginArea(
                new Rect(
                    Screen.width / 2f - menuWidth / 2f,
                    Screen.height / 2f - menuHeight / 2f,
                    menuWidth,
                    menuHeight
                ),
                GUI.skin.box
            );

            GUILayout.Label("PAUSED");

            GUILayout.Space(20);

            if (GUILayout.Button("RESUME"))
            {
                ClosePauseMenu();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("LEAVE GAME"))
            {
                LeaveGame();
            }

            GUILayout.EndArea();
        }
    }
}