using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV;
using System.Drawing;

namespace Domain.Interfaces
{
    public interface IEyeTrackingService
    {
        // Devuelve true si se detecta un parpadeo en el frame dado o ventana temporal
        bool DetectBlink(Mat frame, double earThreshold = 0.22, int minFramesBelow = 2);
    }
}
