using System.Collections.Generic;
using UnityEngine;

public class TrailRendererGradientCopier : MonoBehaviour
{
    public TrailRenderer sourceTrail;
    public List<TrailRenderer> targetTrails;

    void Start()
    {
        if (sourceTrail == null || targetTrails == null || targetTrails.Count == 0)
        {
            Debug.LogWarning("Source TrailRenderer or Target TrailRenderers not assigned.");
            return;
        }

        CopyGradientToTargets();
    }

    public void CopyGradientToTargets()
    {
        Gradient colorGradient = sourceTrail.colorGradient;
        AnimationCurve widthCurve = sourceTrail.widthCurve;

        foreach (TrailRenderer trail in targetTrails)
        {
            if (trail != null)
            {
                trail.colorGradient = colorGradient;
                trail.widthCurve = widthCurve;
            }
        }
    }
}
