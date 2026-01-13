using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV;
using System.Drawing;

namespace Domain.Interfaces
{
    public interface IFaceDetectionService
    {
        Rectangle DetectPrimaryFace(Mat frame);
        Rectangle[] DetectFaces(Mat frame);
    }
}
