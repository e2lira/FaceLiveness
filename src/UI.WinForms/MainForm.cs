using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Domain.Services;


namespace UI.WinForms;

public partial class MainForm : Form
{
    private readonly LivenessOrchestrator _orchestrator;
    private readonly VideoCapture _capture;

    private PictureBox _picture;
    private Label _status;


    public MainForm(LivenessOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
        _capture = new VideoCapture(0); // cámara por defecto

        InitializeComponent();
        SetupUi();

        Application.Idle += OnIdle;
    }

    private void SetupUi()
    {
        Text = "Face Liveness Demo (Emgu CV)";
        Width = 900; Height = 600;

        _picture = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
        _status = new Label { Dock = DockStyle.Top, Height = 24, TextAlign = ContentAlignment.MiddleLeft };

        Controls.Add(_picture);
        Controls.Add(_status);
    }

    private void OnIdle(object? sender, EventArgs e)
    {
        using var frame = _capture.QueryFrame();
        if (frame is null) return;

        var (live, faceRect, head) = _orchestrator.EvaluateFrame(frame);
        //var bmp = frame.Bitmap;
        // Convertir Mat -> Image<Bgr, byte> -> Bitmap y dibujar sobre él.
        using var img = frame.ToImage<Bgr, byte>();
        using var bmpTemp = img.ToBitmap(); // Bitmap temporal sobre el que dibujamos


        using var g = Graphics.FromImage(bmpTemp);
        if (faceRect != Rectangle.Empty)
        {
            g.DrawRectangle(new Pen(live ? Color.Lime : Color.Red, 2), faceRect);
            g.DrawString($"Pitch: {head.pitch:F1}°, Yaw: {head.yaw:F1}°",
                SystemFonts.DefaultFont, Brushes.Yellow, faceRect.X, faceRect.Y - 20);
        }

        _status.Text = live ? "LIVE" : "FAKE / NO-LIVE";
        _status.ForeColor = live ? Color.LimeGreen : Color.OrangeRed;
        _picture.Image?.Dispose();
        _picture.Image = (Bitmap)bmpTemp.Clone();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        Application.Idle -= OnIdle;
        _capture?.Dispose();
    }

}
