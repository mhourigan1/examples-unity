﻿#region

using System.Collections.Generic;
using BrainCloud;
using LitJson;
using UnityEngine;

#endregion

// Achievements are set on the brainCloud Dashboard, under Design | Leaderboard | Leaderboard Configs

public class Leaderboard : GameScene
{
    private readonly List<PlayerInfo> scores = new List<PlayerInfo>();
    private Vector2 _scrollPos;
    private string editablePlayerName = "";

    private bool isEditingPlayerName;

    private void Start()
    {
        gameObject.transform.parent.gameObject.GetComponentInChildren<Camera>().rect = App.ViewportRect;


       

        App.Bc.LeaderboardService.GetGlobalLeaderboardPage("Player_Rating",
            BrainCloudSocialLeaderboard.SortOrder.HIGH_TO_LOW, 0, 10, OnReadLeaderboardData);
    }

    private void OnReadLeaderboardData(string responseData, object cbPostObject)
    {
        scores.Clear();
        
        // Construct our matched players list using response data
        var leaderboardData = JsonMapper.ToObject(responseData)["data"]["leaderboard"];


        foreach (JsonData score in leaderboardData) scores.Add(new PlayerInfo(score));
    }


    private void OnGUI()
    {
        var verticalMargin = 10;


        var profileWindowHeight = Screen.height * 0.20f - verticalMargin * 1.3f;
        var selectorWindowHeight = Screen.height * 0.80f - verticalMargin * 1.3f;


        GUILayout.Window(App.WindowId + 100,
            new Rect(Screen.width / 2 - 150 + App.Offset, verticalMargin, 300, profileWindowHeight),
            OnPlayerInfoWindow, "Profile");


        GUILayout.Window(App.WindowId,
            new Rect(Screen.width / 2 - 150 + App.Offset, Screen.height - selectorWindowHeight - verticalMargin, 300,
                selectorWindowHeight),
            OnPickGameWindow, "Pick Game");
    }

    private void OnPlayerInfoWindow(int windowId)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();


        GUILayout.BeginHorizontal();

        if (!isEditingPlayerName)
        {
            GUILayout.Label(string.Format("PlayerName: {0}", App.PlayerName), GUILayout.MinWidth(200));
            if (GUILayout.Button("Edit", GUILayout.MinWidth(50)))
            {
                editablePlayerName = App.PlayerName;
                isEditingPlayerName = true;
            }
        }
        else
        {
            editablePlayerName = GUILayout.TextField(editablePlayerName, GUILayout.MinWidth(200));
            if (GUILayout.Button("Save", GUILayout.MinWidth(50)))
            {
                App.PlayerName = editablePlayerName;
                isEditingPlayerName = false;

                App.Bc.PlayerStateService.UpdateUserName(App.PlayerName,
                    (response, cbObject) => { },
                    (status, code, error, cbObject) => { Debug.Log("Failed to change Player Name"); });
            }
        }


        GUILayout.EndHorizontal();

        GUILayout.Label(string.Format("PlayerRating: {0}", App.PlayerRating), GUILayout.MinWidth(200));

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("MatchSelect", GUILayout.MinWidth(50))) App.GotoMatchSelectScene(gameObject);

        GUILayout.EndHorizontal();


        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();


        GUILayout.EndHorizontal();
    }

    private void OnPickGameWindow(int windowId)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false);


        GUILayout.Space(10);
        foreach (var score in scores)
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("{0}.  {1} ({2})", score.Rank, score.PlayerName, score.Score));

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();


        if (GUILayout.Button("REFRESH"))
            App.Bc.LeaderboardService.GetGlobalLeaderboardPage("Player_Rating",
                BrainCloudSocialLeaderboard.SortOrder.HIGH_TO_LOW, 0, 10, OnReadLeaderboardData);

        if (GUILayout.Button("LOGOUT"))
            App.Bc.PlayerStateService.Logout((response, cbObject) => { App.GotoLoginScene(gameObject); });

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();


        GUILayout.EndHorizontal();
    }
}