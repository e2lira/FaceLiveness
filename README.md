# FaceLivenessSolution

Proyecto en **C# (.NET 8)** con **Emgu CV** y **DlibDotNet** para reconocimiento facial y detección de rostro vivo (*liveness detection*).  
Incluye:
- Detección de rostro (HaarCascade).
- Landmarks faciales (68 puntos con dlib).
- Detección de parpadeo (Eye Aspect Ratio - EAR).
- Estimación de movimiento de cabeza (SolvePnP).
- Validación de textura de piel (LBP + máscara YCrCb).
- Arquitectura limpia con DI.
- UI WinForms para pruebas en vivo.

---

## 🚀 Requisitos

- Visual Studio 2022+ o Rider.
- .NET 8 (puedes usar 6/7).
- Paquetes NuGet:
  - `Emgu.CV`
  - `Emgu.CV.Bitmap`
  - `Emgu.CV.runtime.windows`
  - `DlibDotNet`
  - `Microsoft.Extensions.DependencyInjection`

---

## 📂 Estructura

flowchart TD
    subgraph UI["UI Layer (WinForms / MAUI)"]
        A[MainForm / MainPage] -->|captura frame| B[LivenessOrchestrator]
    end

    subgraph Application["Application Layer"]
        B --> C1[IFaceDetectionService]
        B --> C2[IEyeTrackingService]
        B --> C3[IHeadPoseService]
        B --> C4[ISkinTextureService]
    end

    subgraph Infrastructure["Infrastructure Layer"]
        C1 --> D1[EmguFaceDetectionService]
        C2 --> D2[EmguEyeTrackingService]
        C3 --> D3[EmguHeadPoseService]
        C4 --> D4[EmguSkinTextureService]
        D2 --> D5[DlibLandmarkService]
        D3 --> D5
    end

    subgraph Assets["Assets (Modelos y Cascadas)"]
        D1 --> E1[haarcascade_frontalface_default.xml]
        D2 --> E2[shape_predictor_68_face_landmarks.dat]
        D3 --> E2
        D4 --> E3[LBP + máscara YCrCb]
    end



    flowchart TD
    A[Captura de frame desde cámara] --> B[Detección de rostro con Emgu HaarCascade]
    B -->|rostro encontrado| C[Obtención de landmarks 68 puntos con Dlib]
    C --> D[Detección de parpadeo con EAR]
    C --> E[Estimación de movimiento de cabeza con SolvePnP]
    B --> F[Validación de textura de piel con LBP + máscara YCrCb]

    D --> G[LivenessOrchestrator]
    E --> G
    F --> G

    G -->|combina resultados| H{¿Rostro vivo?}
    H -->|Sí| I[Resultado: LIVE ✅]
    H -->|No| J[Resultado: FAKE / NO-LIVE ❌]




---

👉 Con este README ya tienes todo lo necesario: **explicación, instalación, ejecución, arquitectura y flujo visual**.  

¿Quieres que te prepare también un **ejemplo de captura de pantalla simulada** (mockup) para mostrar en el README cómo se ve la UI con el rectángulo verde/rojo y el texto de LIVE/FAKE?
