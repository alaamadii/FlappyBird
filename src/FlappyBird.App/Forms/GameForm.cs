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
    private GameState presentedState = GameState.Ready;
    private readonly Button restartButton = new()
    {
        Text = "New game",
        AccessibleName = "New game",
        AccessibleDescription = "Return to Ready. Press Space after releasing it to start a new run.",
        BackColor = Color.FromArgb(37, 86, 76),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        UseVisualStyleBackColor = false,
        Cursor = Cursors.Hand,
        Visible = false,
    };

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
        AccessibleName = "Flappy Bird";
        AccessibleDescription = "Ready. Press Space to start. Release and press Space again to flap.";
        restartButton.FlatAppearance.BorderSize = 0;
        restartButton.Click += (_, _) => RestartGame();
        Controls.Add(restartButton);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        renderer.Render(e.Graphics, ClientSize, engine.Bird, engine.State, engine.Pipes.Pairs, engine.Score);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space)
        {
            e.SuppressKeyPress = true;
            if (active && input.Press() && engine.State != GameState.GameOver)
            {
                if (engine.State == GameState.Ready)
                {
                    loop.Reset();
                    clock.Restart();
                }

                engine.Flap();
                SynchronizePresentation();
                Invalidate();
            }
        }
        else if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            RestartGame();
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

    protected override bool ProcessKeyPreview(ref Message message)
    {
        // Keep a focused native button from treating Space as a restart click.
        if (message.WParam == (IntPtr)Keys.Space && (message.Msg == 0x0100 || message.Msg == 0x0101))
        {
            var key = new KeyEventArgs(Keys.Space);
            if (message.Msg == 0x0100) OnKeyDown(key);
            else OnKeyUp(key);
            return true;
        }

        return base.ProcessKeyPreview(ref message);
    }

    protected override bool ProcessCmdKey(ref Message message, Keys keyData)
    {
        if ((keyData & Keys.KeyCode) == Keys.Enter)
        {
            RestartGame();
            return true;
        }

        return base.ProcessCmdKey(ref message, keyData);
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (restartButton is not null && playfield is not null)
        {
            restartButton.Bounds = SceneViewport.Create(playfield, ClientSize).Map(SceneViewport.RestartButton(playfield));
        }
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
        clock.Reset();
        if (engine.State != GameState.GameOver)
        {
            clock.Start();
            timer.Start();
        }
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
            SynchronizePresentation();
            if (engine.State != GameState.GameOver) clock.Restart();
            return;
        }

        var elapsed = clock.Elapsed.TotalSeconds;
        clock.Restart();
        if (loop.Advance(elapsed, engine.Update) > 0)
        {
            SynchronizePresentation();
            Invalidate();
        }
    }

    private void SynchronizePresentation()
    {
        if (presentedState == engine.State) return;
        presentedState = engine.State;
        restartButton.Visible = engine.State == GameState.GameOver;
        if (engine.State == GameState.GameOver)
        {
            timer.Stop();
            clock.Reset();
            loop.Reset();
            input.Resume(input.IsHeld || SpaceHeld());
            AccessibleDescription = $"Game Over. Final score {engine.Score}. Press Enter or click New game to return to Ready.";
        }
        else
        {
            AccessibleDescription = engine.State == GameState.Ready
                ? "Ready. Release Space, then press it to start a new run."
                : "Running. Release and press Space to flap. Avoid pipes and ground. Score is shown at the top.";
        }

        Invalidate();
    }

    private void RestartGame()
    {
        if (!active || engine.State != GameState.GameOver) return;
        var held = input.IsHeld || SpaceHeld();
        engine.Reset();
        input.Resume(held);
        loop.Reset();
        clock.Restart();
        SynchronizePresentation();
        ActiveControl = null;
        Focus();
        timer.Start();
    }

    private static bool SpaceHeld() => (GetAsyncKeyState((int)Keys.Space) & 0x8000) != 0;

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
