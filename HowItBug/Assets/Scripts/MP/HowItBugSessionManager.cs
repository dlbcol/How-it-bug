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

    private async void Start()
    {
        await InitializeServices();
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

    private async void LeaveGame()
    {
        if (currentSession == null)
            return;

        try
        {
            await currentSession.LeaveAsync();

            currentSession = null;
            status = "Left session";
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(
            new Rect(20, 20, 350, 250),
            GUI.skin.box
        );

        GUILayout.Label("HOW IT BUG - MULTIPLAYER");

        GUILayout.Space(10);

        GUILayout.Label($"Status: {status}");

        GUILayout.Space(15);

        GUI.enabled = servicesReady && currentSession == null;

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

        GUI.enabled = currentSession != null;

        GUILayout.Space(10);

        if (GUILayout.Button("LEAVE GAME"))
        {
            LeaveGame();
        }

        GUI.enabled = true;

        GUILayout.EndArea();
    }
}