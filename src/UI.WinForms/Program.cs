using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Domain.Services;
using Domain.Interfaces;
using Infrastructure;


namespace UI.WinForms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        var services = new ServiceCollection();

        string assets = Path.Combine(AppContext.BaseDirectory, "assets");
        // Servicios de detección de rostro (Emgu HaarCascade)
        services.AddSingleton<IFaceDetectionService>(sp =>
            new EmguFaceDetectionService(Path.Combine(assets, "haarcascade_frontalface_default.xml")));
        /*
        services.AddSingleton<IEyeTrackingService>(sp =>
            new EmguEyeTrackingService(Path.Combine(assets, "haarcascade_eye.xml")));
        
        services.AddSingleton<IHeadPoseService>(sp =>
            new EmguHeadPoseService(Path.Combine(assets, "haarcascade_eye.xml"),
                                    Path.Combine(assets, "haarcascade_mcs_nose.xml")));
        */

        // Servicios de detección de rostro (Emgu HaarCascade)
        services.AddSingleton<IFaceDetectionService>(sp =>
            new EmguFaceDetectionService(Path.Combine(assets, "haarcascade_frontalface_default.xml")));



        // Servicio de landmarks (Dlib 68 puntos)
        services.AddSingleton<DlibLandmarkService>(sp =>
            new DlibLandmarkService(Path.Combine(assets, "shape_predictor_68_face_landmarks.dat")));

        // Servicio de parpadeo (EAR con landmarks)
        services.AddSingleton<IEyeTrackingService, EmguEyeTrackingService>();

        // Servicio de movimiento de cabeza (SolvePnP con landmarks)
        services.AddSingleton<IHeadPoseService, EmguHeadPoseService>();

        // Servicio de textura de piel (LBP + máscara YCrCb)
        services.AddSingleton<ISkinTextureService, EmguSkinTextureService>();

        // Orquestador de liveness
        services.AddSingleton<LivenessOrchestrator>();


        using var provider = services.BuildServiceProvider();

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(provider.GetRequiredService<LivenessOrchestrator>()));

    }
}