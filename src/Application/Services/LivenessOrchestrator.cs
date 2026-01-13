using Domain.Interfaces;
using Emgu.CV;
using System;
using System.Drawing;

namespace Domain.Services;

public class LivenessOrchestrator
{
    private readonly IFaceDetectionService _face;
    private readonly IEyeTrackingService _eyes;
    private readonly IHeadPoseService _pose;
    private readonly ISkinTextureService _skin;

    public LivenessOrchestrator(IFaceDetectionService face, IEyeTrackingService eyes, IHeadPoseService pose, ISkinTextureService skin)
    {
        _face = face;
        _eyes = eyes;
        _pose = pose;
        _skin = skin;
    }

    // Regla simple: rostro detectado + parpadeo + textura válida + movimiento de cabeza (yaw/pitch > umbral)
    public (bool isLive, Rectangle faceRect, (double pitch, double yaw) head) EvaluateFrame(Mat frame,
        double yawMinDeg = 8, double pitchMinDeg = 8)
    {
        var faceRect = _face.DetectPrimaryFace(frame);
        if (faceRect == Rectangle.Empty)
            return (false, Rectangle.Empty, (0, 0));

        using var faceRoi = new Mat(frame, faceRect);

        bool blink = _eyes.DetectBlink(faceRoi);
        var (pitch, yaw) = _pose.EstimatePose(faceRoi);
        bool skinOk = _skin.ValidateSkinTexture(faceRoi);

        bool live = blink && skinOk && (Math.Abs(yaw) >= yawMinDeg || Math.Abs(pitch) >= pitchMinDeg);
        return (live, faceRect, (pitch, yaw));
    }



}
