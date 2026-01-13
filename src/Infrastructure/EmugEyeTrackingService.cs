using Domain.Interfaces;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using DlibDotNet;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;


namespace Infrastructure
{
    /// <summary>
    /// Servicio de detección/seguimiento de ojos usando CascadeClassifier de EmguCV.
    /// </summary>
    public class EmguEyeTrackingService : IEyeTrackingService
    {

        private readonly System.Collections.Generic.Queue<double> _earHistory = new();
        private readonly int _maxHistory = 10;
        private readonly DlibLandmarkService _landmarkService;

        public EmguEyeTrackingService(DlibLandmarkService landmarkService)
        {
            _landmarkService = landmarkService;
        }

        /*
        private readonly CascadeClassifier _eyeCascade;

        // Buffers temporales por frame para estimar parpadeo
        private readonly Queue<double> _earHistory = new();
        private readonly int _maxHistory = 10;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="EmguEyeTrackingService"/>.
        /// </summary>
        /// <param name="eyeCascadePath">Ruta al archivo XML del cascade de ojos.</param>
        /// <exception cref="ArgumentException">Si <paramref name="eyeCascadePath"/> es nulo o vacío.</exception>
        public EmguEyeTrackingService(string eyeCascadePath)
        {
            if (string.IsNullOrWhiteSpace(eyeCascadePath))
                throw new ArgumentException("La ruta del cascade de ojos no puede estar vacía.", nameof(eyeCascadePath));

            _eyeCascade = new CascadeClassifier(eyeCascadePath);
        }
        
        public bool DetectBlink(Mat faceRoi, double earThreshold = 0.22, int minFramesBelow = 2)
        {
            var eyes = _eyeCascade.DetectMultiScale(faceRoi, 1.1, 3, Size.Empty);
            if (eyes.Length < 1)
            {
                PushEar(0.0);
                return false;
            }

            // EAR aproximado por ojos detectados
            double earSum = 0;
            int count = 0;
            foreach (var e in eyes)
            {
                double h = e.Height;
                double w = e.Width + 1e-5;
                double earRect = (h / w); // proxy simple; ideal: landmarks 6 puntos por ojo
                earSum += earRect;
                count++;
            }
            double ear = earSum / Math.Max(1, count);

            PushEar(ear);

            // Regla: si EAR cae por debajo del umbral al menos minFramesBelow frames consecutivos y luego sube, parpadeo
            int below = 0;
            foreach (var v in _earHistory)
                if (v < earThreshold) below++;

            bool blink = below >= minFramesBelow && ear >= earThreshold; // recuperación
            return blink;
        }
        */
        public bool DetectBlink(Emgu.CV.Mat faceRoi, double earThreshold = 0.22, int minFramesBelow = 2)
        {
            // Obtener landmarks del rostro
            var shape = _landmarkService.GetLandmarks(faceRoi, new Rectangle(0, 0, faceRoi.Width, faceRoi.Height));

            // Ojo izquierdo (puntos 36–41)
            var leftEye = new Point[]
            {
                ToPoint(shape.GetPart(36)), ToPoint(shape.GetPart(37)),
                ToPoint(shape.GetPart(38)), ToPoint(shape.GetPart(39)),
                ToPoint(shape.GetPart(40)), ToPoint(shape.GetPart(41))
            };

            // Ojo derecho (puntos 42–47)
            var rightEye = new Point[]
            {
                ToPoint(shape.GetPart(42)), ToPoint(shape.GetPart(43)),
                ToPoint(shape.GetPart(44)), ToPoint(shape.GetPart(45)),
                ToPoint(shape.GetPart(46)), ToPoint(shape.GetPart(47))
            };

            double earLeft = CalculateEAR(leftEye);
            double earRight = CalculateEAR(rightEye);
            double ear = (earLeft + earRight) / 2.0;

            PushEar(ear);

            int below = 0;
            foreach (var v in _earHistory)
                if (v < earThreshold) below++;

            bool blink = below >= minFramesBelow && ear >= earThreshold;
            return blink;
        }

        private double CalculateEAR(Point[] eye)
        {
            double vertical1 = Distance(eye[1], eye[5]);
            double vertical2 = Distance(eye[2], eye[4]);
            double horizontal = Distance(eye[0], eye[3]);
            return (vertical1 + vertical2) / (2.0 * horizontal);
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private void PushEar(double v)
        {
            _earHistory.Enqueue(v);
            while (_earHistory.Count > _maxHistory) _earHistory.Dequeue();
        }

        private Point ToPoint(DlibDotNet.Point p) => new Point(p.X, p.Y);
    }


}
