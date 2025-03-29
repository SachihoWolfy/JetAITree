using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class InfoCanvasController : MonoBehaviour
{
    private AIAircraft aircraft;
    private List<AIAircraft> topThree = new List<AIAircraft>();
    private List<AIAircraft> allAircraft = new List<AIAircraft>();
    private CameraBehavior cameraBehavior;
    private ManueverStatus m_status;
    private TargetingStatus t_status;

    [Header("BlueColors")]
    public Color blueTextDefault;
    public Color blueTextSelected;
    public Color blueTextFailure;

    [Header("BlueColors")]
    public Color redTextDefault;
    public Color redTextSelected;
    public Color redTextFailure;

    [Header("UI Elements")]
    public List<StatusUIElement> maneuverStatusList = new List<StatusUIElement>();
    public List<StatusUIElement> targetStatusList = new List<StatusUIElement>();
    public List<TMP_Text> otherText = new List<TMP_Text>();
    public List<Image> otherImages = new List<Image>();

    [Header("Score")]
    public TMP_Text blueScoreText;
    public TMP_Text redScoreText;
    private int scoreBlue = 0;
    private int scoreRed = 0;

    [Header("Spectating")]
    public TMP_Text nameText;
    public Image pilotImage;

    public LeaderBoardButton[] topThreeButtons;

    // Start is called before the first frame update
    void Start()
    {
        cameraBehavior = FindObjectOfType<CameraBehavior>();
        aircraft = cameraBehavior.curTarget;
    }

    // Update is called once per frame
    void Update()
    {
        aircraft = cameraBehavior.curTarget;
        m_status = aircraft.m_status;
        t_status = aircraft.t_status;
        UpdateTargetTree();
        UpdateManueverTree();
        UpdateSpectate();
    }
    public void UpdateTopThreeList()
    {
        if(allAircraft.Count == 0)
        {
            allAircraft.Clear();
            allAircraft.AddRange(FindObjectsOfType<AIAircraft>());
        }
        topThree.Clear();
        topThree.AddRange(allAircraft);
        topThree.Sort((a, b) => b.kills.CompareTo(a.kills));
        topThree = topThree.Take(3).ToList();
        UpdateLeaderBoardButtons();
    }

    void UpdateLeaderBoardButtons()
    {
        int index = 0;
        foreach(LeaderBoardButton button in topThreeButtons)
        {
            if (topThree.Count > index)
            {
                button.aircraftID.text = topThree[index].aircraftID;
                button.killText.text = topThree[index].kills.ToString();
                if (topThree[index].team == Team.Red)
                {
                    button.pilotImage.color = redScoreText.color;
                    button.aircraftID.color = redScoreText.color;
                    button.killText.color = redScoreText.color;
                    button.buttonImage.color = redScoreText.color;
                }
                else
                {
                    button.pilotImage.color = blueScoreText.color;
                    button.aircraftID.color = blueScoreText.color;
                    button.killText.color = blueScoreText.color;
                    button.buttonImage.color = blueScoreText.color;
                }
            }
            else if(allAircraft.Count > index)
            {
                button.aircraftID.text = allAircraft[index].aircraftID;
                button.killText.text = allAircraft[index].kills.ToString();
                if (allAircraft[index].team == Team.Red)
                {
                    button.pilotImage.color = redScoreText.color;
                    button.aircraftID.color = redScoreText.color;
                    button.killText.color = redScoreText.color;
                    button.buttonImage.color = redScoreText.color;
                }
                else
                {
                    button.pilotImage.color = blueScoreText.color;
                    button.aircraftID.color = blueScoreText.color;
                    button.killText.color = blueScoreText.color;
                    button.buttonImage.color = blueScoreText.color;
                }
            }
            index++;
        }
    }

    public void OnLeaderboardButtonPress(int index)
    {
        cameraBehavior.ChangeTarget(topThree[index]);
    }

    void UpdateTargetTree()
    {
        int targetIndex = 0;
        switch (t_status)
        {
            case TargetingStatus.KeepTarget:
                targetIndex = 0;
                break;
            case TargetingStatus.Pursuer:
                targetIndex = 1;
                break;
            case TargetingStatus.AllyDefense:
                targetIndex = 2;
                break;
            case TargetingStatus.Best:
                targetIndex = 3;
                break;
            case TargetingStatus.Strafers:
                targetIndex = 4;
                break;
            case TargetingStatus.Ground:
                targetIndex = 5;
                break;
        }
        UpdateVisuals(targetStatusList, targetIndex);
    }
    void UpdateManueverTree()
    {
        int targetIndex = 0;
        switch (m_status)
        {
            case ManueverStatus.Evading:
                targetIndex = 0;
                break;
            case ManueverStatus.Dogfighting:
                targetIndex = 1;
                break;
            case ManueverStatus.Strafing:
                targetIndex = 2;
                break;
        }
        UpdateVisuals(maneuverStatusList, targetIndex);
    }

    void UpdateVisuals(List<StatusUIElement> selectedList, int targetIndex)
    {
        if(aircraft.team == Team.Red)
        {
            foreach(TMP_Text text in otherText)
            {
                text.color = redTextDefault;
            }
            foreach(Image image in otherImages)
            {
                image.color = redTextDefault;
            }
        }
        else
        {
            foreach (TMP_Text text in otherText)
            {
                text.color = blueTextDefault;
            }
            foreach (Image image in otherImages)
            {
                image.color = blueTextDefault;
            }
        }
            int curIndex = 0;
        foreach (StatusUIElement ui in selectedList)
        {
            if (aircraft.team == Team.Red)
            {
                if (curIndex < targetIndex)
                {
                    ui.infoText.color = redTextFailure;
                    ui.actionText.color = redTextFailure;
                    ui.image.color = redTextFailure;
                }
                if (curIndex == targetIndex)
                {
                    ui.infoText.color = redTextSelected;
                    ui.actionText.color = redTextSelected;
                    ui.image.color = redTextSelected;
                }
                if (curIndex > targetIndex)
                {
                    ui.infoText.color = redTextDefault;
                    ui.actionText.color = redTextDefault;
                    ui.image.color = redTextDefault;
                }
            }
            else
            {
                if (curIndex < targetIndex)
                {
                    ui.infoText.color = blueTextFailure;
                    ui.actionText.color = blueTextFailure;
                    ui.image.color = blueTextFailure;
                }
                if (curIndex == targetIndex)
                {
                    ui.infoText.color = blueTextSelected;
                    ui.actionText.color = blueTextSelected;
                    ui.image.color = blueTextSelected;
                }
                if (curIndex > targetIndex)
                {
                    ui.infoText.color = blueTextDefault;
                    ui.actionText.color = blueTextDefault;
                    ui.image.color = blueTextDefault;
                }
            }
            curIndex++;
        }
    }
    public void AddKill(Team team)
    {
        if(team == Team.Red)
        {
            scoreBlue++;
        }
        else
        {
            scoreRed++;
        }
        UpdateScore();
    }
    void UpdateScore()
    {
        blueScoreText.text = scoreBlue.ToString();
        redScoreText.text = scoreRed.ToString();
    }
    void UpdateSpectate()
    {
        nameText.text = aircraft.aircraftID;
        if (aircraft.team == Team.Red)
        {
            nameText.color = redScoreText.color;
            pilotImage.color = redScoreText.color;
        }
        else
        {
            nameText.color = blueScoreText.color;
            pilotImage.color = blueScoreText.color;
        }
    }
}

[System.Serializable]
public class StatusUIElement
{
    public TMP_Text infoText;
    public TMP_Text actionText;
    public Image image;
}
[System.Serializable]
public class LeaderBoardButton
{
    public TMP_Text killText;
    public TMP_Text aircraftID;
    public Image pilotImage;
    public Image buttonImage;
}