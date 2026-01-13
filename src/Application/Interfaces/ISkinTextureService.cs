using Emgu.CV;


namespace Domain.Interfaces
{
    public interface ISkinTextureService
    {
        // Valida textura de piel usando LBP + máscara de piel
        bool ValidateSkinTexture(Mat faceRoi, float minSkinAreaRatio = 0.25f, double lbpUniformityMin = 0.35);
    }
}
