using FlappyBird.Core.Models;

namespace FlappyBird.App.Forms;

/// <summary>The application shell. Rendering and input are added in later stages.</summary>
public sealed class GameForm : Form
{
    public GameForm()
    {
        var playfield = new Playfield();
        Text = "YMH219 — Flappy Bird";
        ClientSize = new Size(playfield.Width, playfield.Height);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;
        KeyPreview = true;

        Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Flappy Bird\nYMH219 — Nesne Tabanlı Programlama\n\n"
                + "Project foundation ready.\nGameplay will be added in the next stages.",
        });
    }
}
