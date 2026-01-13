using System;
using System.Collections.Generic;
using System.Drawing;
using Domain.Interfaces;
using DlibDotNet;
using Emgu.CV;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;


namespace Infrastructure
{
    public class DlibLandmarkService
    {
        private readonly FrontalFaceDetector _detector;
        private readonly ShapePredictor _shapePredictor;

        public DlibLandmarkService(string predictorPath)
        {
            if (!System.IO.File.Exists(predictorPath))
                throw new ArgumentException($"No se encontró el archivo de predictor en {predictorPath}");

            _detector = Dlib.GetFrontalFaceDetector();
            _shapePredictor = ShapePredictor.Deserialize(predictorPath);
        }

        /// <summary>
        /// Obtiene los landmarks (68 puntos) de un rostro detectado en un frame.
        /// </summary>
        public FullObjectDetection GetLandmarks(Mat frame, Rectangle faceRect)
        {
            // Convertir Mat de Emgu a imagen de Dlib
            using var dlibImg = ToDlibImage(frame);

            // Convertir rectángulo de Emgu a rectángulo de Dlib
            var rect = new DlibDotNet.Rectangle(faceRect.X, faceRect.Y, faceRect.Right, faceRect.Bottom);

            // Detectar landmarks
            var shape = _shapePredictor.Detect(dlibImg, rect);
            return shape;
        }

        /// <summary>
        /// Convierte un FullObjectDetection (68 puntos) a una lista de Point.
        /// </summary>
        public List<Point> ToPoints(FullObjectDetection shape)
        {
            var points = new List<Point>();
            for (int i = 0; i < shape.Parts; i++)
            {
                var part = shape.GetPart((uint)i);
                points.Add(new Point(part.X, part.Y));
            }
            return points;
        }

        private Array2D<RgbPixel> ToDlibImage(Mat frame)
        {
            // Convertir Mat a Bitmap
            using var bmp = frame.ToBitmap();
            var byteArray = bmp.ToByteArray();
            return Dlib.LoadImageData<RgbPixel>(
                byteArray,
                (uint)bmp.Height,
                (uint)bmp.Width,
                (uint)(bmp.Width * 3)
            );
        }
    }

    public static class BitmapExtensions
    {
        public static byte[] ToByteArray(this Bitmap bmp)
        {
            using var ms = new System.IO.MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            return ms.ToArray();
        }
    }

}
