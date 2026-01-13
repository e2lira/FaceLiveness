using Domain.Interfaces;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Infrastructure;

public class EmguFaceDetectionService: IFaceDetectionService
{
    private readonly CascadeClassifier _faceCascade;

    public EmguFaceDetectionService(string cascadePath)
    {
        _faceCascade = new CascadeClassifier(cascadePath);
    }

    public Rectangle DetectPrimaryFace(Mat frame)
    {
        var faces = DetectFaces(frame);
        if (faces.Length == 0) return Rectangle.Empty;

        // Seleccionar el rostro más grande como principal
        Rectangle best = faces[0];
        foreach (var f in faces)
            if (f.Width * f.Height > best.Width * best.Height)
                best = f;

        return best;
    }

    public Rectangle[] DetectFaces(Mat frame)
    {
        using var gray = new Mat();
        CvInvoke.CvtColor(frame, gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
        CvInvoke.EqualizeHist(gray, gray);
        var faces = _faceCascade.DetectMultiScale(
            image: gray,
            scaleFactor: 1.1,
            minNeighbors: 5,
            minSize: Size.Empty,
            maxSize: Size.Empty
        );
        return faces;
    }



}
