using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusHud : MonoBehaviour
{
    private AIAircraft manueverAI;
    private TargetSelectionTree targetAI;

    public Image manueverImage;
    public Image targetImage;

    public Color redColor = Color.red;
    public Color blueColor = Color.blue;

    public Sprite evadeSprite;
    public Sprite dogfightSprite;
    public Sprite strafingSprite;

    public Sprite keepTargetSprite;
    public Sprite pursuerSprite;
    public Sprite allyDefendSprite;
    public Sprite bestSprite;
    public Sprite straferSprite;
    public Sprite groundSprite;
    // Start is called before the first frame update
    void Start()
    {
        manueverAI = GetComponent<AIAircraft>();
        targetAI = GetComponent<TargetSelectionTree>();
        if(manueverAI.team == Team.Red)
        {
            manueverImage.color = redColor;
            targetImage.color = redColor;
        }
        else
        {
            manueverImage.color = blueColor;
            targetImage.color = blueColor;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateManueverHUD();
        UpdateTargetHUD();
    }

    private void UpdateManueverHUD()
    {
        switch (manueverAI.m_status)
        {
            case ManueverStatus.Evading:
                manueverImage.sprite = evadeSprite;
                break;
            case ManueverStatus.Dogfighting:
                manueverImage.sprite = dogfightSprite;
                break;
            case ManueverStatus.Strafing:
                manueverImage.sprite = strafingSprite;
                break;
        }
    }
    private void UpdateTargetHUD()
    {
        switch (targetAI.status)
        {
            case TargetingStatus.KeepTarget:
                targetImage.sprite = keepTargetSprite;
                break;
            case TargetingStatus.Pursuer:
                targetImage.sprite = pursuerSprite;
                break;
            case TargetingStatus.AllyDefense:
                targetImage.sprite = allyDefendSprite;
                break;
            case TargetingStatus.Best:
                targetImage.sprite = bestSprite;
                break;
            case TargetingStatus.Strafers:
                targetImage.sprite = straferSprite;
                break;
            case TargetingStatus.Ground:
                targetImage.sprite = groundSprite;
                break;
        }
    }
}
public enum ManueverStatus
{
    Evading,
    Dogfighting,
    Strafing
}
public enum TargetingStatus
{
    KeepTarget,
    Pursuer,
    AllyDefense,
    Best,
    Strafers,
    Ground
}