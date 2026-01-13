using Domain.Interfaces;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;


namespace Infrastructure
{
    public class EmguSkinTextureService: ISkinTextureService
    {
        public bool ValidateSkinTexture(Mat faceRoi, float minSkinAreaRatio = 0.25f, double lbpUniformityMin = 0.35)
        {
            using var ycrcb = new Mat();
            CvInvoke.CvtColor(faceRoi, ycrcb, Emgu.CV.CvEnum.ColorConversion.Bgr2YCrCb);

            // Umbrales estándar (ajusta según iluminación)
            // Cr: 135–180, Cb: 85–135
            using var skinMask = new Mat();
            CvInvoke.InRange(
                ycrcb,
                new ScalarArray(new MCvScalar(0, 135, 85)),
                new ScalarArray(new MCvScalar(255, 180, 135)),
                skinMask
            );

            double skinPixels = CvInvoke.CountNonZero(skinMask);
            double totalPixels = faceRoi.Width * faceRoi.Height;
            if (skinPixels / totalPixels < minSkinAreaRatio) return false;

            // LBP en gris sobre regiones de piel
            using var gray = new Mat();
            CvInvoke.CvtColor(faceRoi, gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

            // LBP manual
            using var lbp = ComputeLBP(gray, skinMask);

            // --- CORRECCIÓN DEL HISTOGRAMA (2026) ---
            using Mat hist = new Mat();
            int[] binsCount = { 256 };
            float[] ranges = { 0, 256 };
            int[] channels = { 0 };

            // Usamos VectorOfMat para pasar la imagen LBP
            using (var vLbp = new Emgu.CV.Util.VectorOfMat(lbp))
            {
                CvInvoke.CalcHist(vLbp, channels, skinMask, hist, binsCount, ranges, false);
            }

            // --- ACCESO A DATOS DEL HISTOGRAMA ---
            // En versiones modernas, copiamos el Mat a un array de una sola dimensión
            float[] histData = new float[256];
            hist.CopyTo(histData);

            double sum = 0;
            foreach (var b in histData) sum += b;

            if (sum == 0) return false; // Evitar división por cero

            int lowBins = 0;
            double threshold = (sum / 256.0) * 0.5;
            foreach (var b in histData)
            {
                if (b < threshold) lowBins++;
            }

            double uniformity = 1.0 - (lowBins / 256.0);
            return uniformity >= lbpUniformityMin;
        }

        private static Mat ComputeLBP(Mat gray, Mat mask)
        {
            var lbp = new Mat(gray.Size, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
            unsafe
            {
                using var g = gray.ToImage<Gray, byte>();
                using var m = mask.ToImage<Gray, byte>();
                var l = lbp.ToImage<Gray, byte>();

                for (int y = 1; y < g.Height - 1; y++)
                {
                    for (int x = 1; x < g.Width - 1; x++)
                    {
                        if (m.Data[y, x, 0] == 0)
                        {
                            l.Data[y, x, 0] = 0;
                            continue;
                        }

                        byte c = g.Data[y, x, 0];
                        int code = 0;
                        code |= (g.Data[y - 1, x - 1, 0] >= c ? 1 : 0) << 7;
                        code |= (g.Data[y - 1, x, 0] >= c ? 1 : 0) << 6;
                        code |= (g.Data[y - 1, x + 1, 0] >= c ? 1 : 0) << 5;
                        code |= (g.Data[y, x + 1, 0] >= c ? 1 : 0) << 4;
                        code |= (g.Data[y + 1, x + 1, 0] >= c ? 1 : 0) << 3;
                        code |= (g.Data[y + 1, x, 0] >= c ? 1 : 0) << 2;
                        code |= (g.Data[y + 1, x - 1, 0] >= c ? 1 : 0) << 1;
                        code |= (g.Data[y, x - 1, 0] >= c ? 1 : 0) << 0;
                        l.Data[y, x, 0] = (byte)code;
                    }
                }
            }
            return lbp;
        }

    }
}
