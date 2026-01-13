using Emgu.CV;

namespace Domain.Interfaces
{
    public interface IHeadPoseService
    {
        // Estimación simple de yaw/pitch con ojos y nariz; devuleve grados aproximados 
        (double pitch, double yaw) EstimatePose(Mat faceRoi);
    }
}
