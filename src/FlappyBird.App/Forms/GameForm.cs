using System.Diagnostics;
using System.Runtime.InteropServices;
using FlappyBird.App.Input;
using FlappyBird.App.Rendering;
using FlappyBird.App.Timing;
using FlappyBird.Core.Models;
using FlappyBird.Core.Services;

namespace FlappyBird.App.Forms;

/// <summary>Adapts Windows input, focus, and timer signals to the domain and renderer.</summary>
public sealed class GameForm : Form
{
    private readonly Playfield playfield = new();
    private readonly GameRenderer renderer;
    private readonly GameEngine engine;
    private readonly FlapInput input = new();
    private readonly FixedStepLoop loop = new();
    private readonly Stopwatch clock = new();
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 15 };
    private bool active;
    private bool resourcesDisposed;

    public GameForm()
    {
        renderer = new GameRenderer(playfield);
        engine = new GameEngine(playfield, new GameSettings());
        timer.Tick += OnTimerTick;
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
        AccessibleName = "Flappy Bird flight practice";
        AccessibleDescription = "Ready. Press Space to start. Release and press Space again to flap.";
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        renderer.Render(e.Graphics, ClientSize, engine.Bird, engine.State);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space)
        {
            e.SuppressKeyPress = true;
            if (active && input.Press())
            {
                if (engine.State == GameState.Ready)
                {
                    loop.Reset();
                    clock.Restart();
                }

                engine.Flap();
                AccessibleDescription = "Running. Release and press Space to flap. Ground collision is not implemented yet.";
                Invalidate();
            }
        }

        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space)
        {
            e.SuppressKeyPress = true;
            input.Release();
        }

        base.OnKeyUp(e);
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        if (resourcesDisposed)
        {
            return;
        }

        active = true;
        // Read only on activation to reject a key already held when focus returns.
        input.Resume((GetAsyncKeyState((int)Keys.Space) & 0x8000) != 0);
        loop.Reset();
        clock.Restart();
        timer.Start();
    }

    protected override void OnDeactivate(EventArgs e)
    {
        active = false;
        input.Suspend();
        loop.Reset();
        clock.Reset();
        if (!resourcesDisposed)
        {
            timer.Stop();
        }

        base.OnDeactivate(e);
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!active || engine.State != GameState.Running)
        {
            clock.Restart();
            return;
        }

        var elapsed = clock.Elapsed.TotalSeconds;
        clock.Restart();
        if (loop.Advance(elapsed, engine.Update) > 0)
        {
            Invalidate();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !resourcesDisposed)
        {
            resourcesDisposed = true;
            active = false;
            timer.Stop();
            timer.Dispose();
            clock.Stop();
        }

        base.Dispose(disposing);
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);
}
