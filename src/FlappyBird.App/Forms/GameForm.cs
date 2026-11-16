using FlappyBird.App.Rendering;
using FlappyBird.Core.Models;

namespace FlappyBird.App.Forms;

/// <summary>Owns the window lifecycle and delegates the static scene to its renderer.</summary>
public sealed class GameForm : Form
{
    private readonly Playfield playfield = new();
    private readonly GameRenderer renderer;

    public GameForm()
    {
        renderer = new GameRenderer(playfield);
        Text = "YMH219 — Flappy Bird";
        ClientSize = new Size(playfield.Width, playfield.Height);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;
        ResizeRedraw = true;
        KeyPreview = true;
        AccessibleName = "Flappy Bird static preview";
        AccessibleDescription = "Ready. Bird and scenery preview. Controls arrive in Stage 02.";
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        renderer.Render(e.Graphics, ClientSize);
    }
}
