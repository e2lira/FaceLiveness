using Domain.Interfaces;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using System;
using System.Drawing;


namespace Infrastructure
{
    public class EmguHeadPoseService: IHeadPoseService
    {
        private readonly DlibLandmarkService _landmarkService;

        public EmguHeadPoseService(DlibLandmarkService landmarkService)
        {
            _landmarkService = landmarkService;
        }

        /*
        private readonly CascadeClassifier _eyeCascade;
        private readonly CascadeClassifier _noseCascade;

       
        public EmguHeadPoseService(string eyeCascadePath, string noseCascadePath)
        {
            _eyeCascade = new CascadeClassifier(eyeCascadePath);
            _noseCascade = new CascadeClassifier(noseCascadePath);
        }
        public (double pitch, double yaw) EstimatePose(Mat faceRoi)
        {
            var eyes = _eyeCascade.DetectMultiScale(faceRoi, 1.1, 3, Size.Empty);
            var noses = _noseCascade.DetectMultiScale(faceRoi, 1.1, 3, Size.Empty);

            if (eyes.Length == 0 || noses.Length == 0) return (0, 0);

            // Tomar dos ojos más grandes
            Array.Sort(eyes, (a, b) => (b.Width * b.Height).CompareTo(a.Width * a.Height));
            Rectangle? leftEye = null;
            Rectangle? rightEye = null;

            foreach (var e in eyes)
            {
                if (leftEye == null) { leftEye = e; continue; }
                if (rightEye == null) { rightEye = e; break; }
            }
            if (leftEye == null || rightEye == null) return (0, 0);

            var nose = noses[0];

            // Centro del ROI
            double cx = faceRoi.Width / 2.0;
            double cy = faceRoi.Height / 2.0;

            // Centros de ojos y nariz
            var l = Center(leftEye.Value);
            var r = Center(rightEye.Value);
            var n = Center(nose);

            // Distancias normalizadas por ancho/alto del rostro
            double dxEye = (r.X - l.X) / faceRoi.Width;      // anchura entre ojos
            double dyEyes = (r.Y - l.Y) / faceRoi.Height;    // diferencia vertical entre ojos
            double nx = (n.X - cx) / faceRoi.Width;          // nariz desviación horizontal
            double ny = (n.Y - cy) / faceRoi.Height;         // nariz desviación vertical

            // Heurísticas simples:
            // - yaw ~ combinación de desviación horizontal de nariz y centrado de ojos
            // - pitch ~ combinación de diferencia vertical entre ojos y nariz
            double yaw = (nx * 90.0) + (dyEyes * 50.0);
            double pitch = (ny * 90.0);

            return (pitch, yaw);
        }
        */
        public (double pitch, double yaw) EstimatePose(Emgu.CV.Mat faceRoi)
        {
            var shape = _landmarkService.GetLandmarks(faceRoi, new Rectangle(0, 0, faceRoi.Width, faceRoi.Height));

            // Puntos 2D de la imagen
           
            var imagePoints = new PointF[]
            {
                ToPointF(shape.GetPart(30)), // nariz
                ToPointF(shape.GetPart(36)), // ojo izq
                ToPointF(shape.GetPart(45)), // ojo der
                ToPointF(shape.GetPart(48)), // boca izq
                ToPointF(shape.GetPart(54))  // boca der
            };
            
            if (shape == null)
                return (0, 0);

            // Índices usados del modelo de 68 puntos de dlib:
            // 30: nariz, 36: ojo izq, 45: ojo der, 48: boca izq, 54: boca der, 8: mentón (añadido)
            int[] idx = { 30, 36, 45, 48, 54, 8 };
            // Verificar que todos los puntos estén disponibles
            try
            {

                // Puntos 3D de referencia (modelo simplificado de cabeza)
                var modelPoints = new MCvPoint3D32f[]
                {
                new MCvPoint3D32f(0,0,0),       // nariz
                new MCvPoint3D32f(-30,-30,-30), // ojo izq
                new MCvPoint3D32f(30,-30,-30),  // ojo der
                new MCvPoint3D32f(-40,40,-30),  // boca izq
                new MCvPoint3D32f(40,40,-30),    // boca
                new MCvPoint3D32f(0, 80, -30)     // mentón (aprox.)
            };

                using var vModel = new VectorOfPoint3D32F(modelPoints);
                using var vImage = new VectorOfPointF(imagePoints);


                Mat cameraMatrix = GetCameraMatrix(faceRoi.Size);
                Mat distCoeffs = new Mat();
                Mat rvec = new Mat(), tvec = new Mat();
                // Usar explícitamente el método Iterative (más tolerante y estable);
                // DLS (DLT) exige >= 6 puntos y es el que lanzó la excepción.
                CvInvoke.SolvePnP(vModel, vImage, cameraMatrix, distCoeffs, rvec, tvec, useExtrinsicGuess: false,
    flags: SolvePnpMethod.Iterative); // <--- El nombre correcto es SolvePnpMethod

                double[] rot = new double[3];
                rvec.CopyTo(rot);;

                double yaw = rot[1] * 57.29577951308232;   // rad → grados
                double pitch = rot[0] * 57.29577951308232;

                return (pitch, yaw);
            }
            catch (ArgumentException)
            {
                // Algún punto no estaba presente; evitar excepción y devolver 0s
                return (0, 0);
            }
            catch (CvException)
            {
                // Fallback: si SolvePnP falla, devolver 0s (o implementar otra heurística)
                return (0, 0);
            }
        }

        private Mat GetCameraMatrix(Size size)
        {
            double focalLength = size.Width;
            PointF center = new PointF(size.Width / 2f, size.Height / 2f);
            var cameraMatrix = new Matrix<double>(3, 3);
            cameraMatrix[0, 0] = focalLength;
            cameraMatrix[1, 1] = focalLength;
            cameraMatrix[0, 2] = center.X;
            cameraMatrix[1, 2] = center.Y;
            cameraMatrix[2, 2] = 1;
            return cameraMatrix.Mat;
        }

        private PointF ToPointF(DlibDotNet.Point p) => new PointF(p.X, p.Y);
    }


}
