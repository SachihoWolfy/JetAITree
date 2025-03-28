using UnityEngine;

public class PIDController
{
    // Note: This didn't work, and I did research for nothing.
    public float Kp, Ki, Kd;
    private float integral;
    private float previousError;
    private float maxIntegral;

    public PIDController(float Kp, float Ki, float Kd, float maxIntegral = 10f)
    {
        this.Kp = Kp;
        this.Ki = Ki;
        this.Kd = Kd;
        this.maxIntegral = maxIntegral;
    }

    public float Compute(float error)
    {
        integral += error * Time.fixedDeltaTime;
        if (integral > maxIntegral)
            integral = maxIntegral;
        else if (integral < -maxIntegral)
            integral = -maxIntegral;

        float derivative = (error - previousError) / Time.fixedDeltaTime;
        previousError = error;

        return Kp * error + Ki * integral + Kd * derivative;
    }

    public void Reset()
    {
        integral = 0;
        previousError = 0;
    }
}


