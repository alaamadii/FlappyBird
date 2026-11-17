using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using FlappyBird.App.Forms;
using FlappyBird.App.Input;
using FlappyBird.App.Rendering;
using FlappyBird.App.Services;
using FlappyBird.App.Timing;
using FlappyBird.Core.Models;
using FlappyBird.Core.Services;

namespace FlappyBird.App.Tests;

[Collection("Native windows")]
public sealed class GameFormRulesTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr window, uint message, IntPtr key, IntPtr detail);

    [Fact]
    public void ButtonAndEnterRestartRepeatedlyWithoutReplacingTimerOrPipes() => RunOnSta((form, engine, button) =>
    {
        var timer = Field<System.Windows.Forms.Timer>(form, "timer");
        var pairs = engine.Pipes.Pairs.ToArray();
        for (var run = 0; run < 12; run++)
        {
            Lose(form, engine);
            Assert.True(button.Visible);
            Assert.False(timer.Enabled);
            Assert.Equal(SceneViewport.Create(new Playfield(), form.ClientSize)
                .Map(SceneViewport.RestartButton(new Playfield())), button.Bounds);
            if (run % 2 == 0) button.PerformClick();
            else { button.Focus(); Key(button, Keys.Enter, down: true); }
            Assert.Equal(GameState.Ready, engine.State);
            Assert.Equal(0, engine.Score);
            Assert.False(button.Visible);
            Assert.True(timer.Enabled);
            Assert.Equal(520, engine.Pipes.Pairs[0].X);
            Assert.All(engine.Pipes.Pairs, pair => Assert.Contains(pair, pairs));
            Assert.Same(timer, Field<System.Windows.Forms.Timer>(form, "timer"));
        }
    });

    [Fact]
    public void FocusedButtonDoesNotRestartOnSpaceAndHeldSpaceCannotStartResetRun() => RunOnSta((form, engine, button) =>
    {
        Lose(form, engine);
        button.Focus();
        Key(button, Keys.Space, down: true);
        Key(button, Keys.Space, down: false);
        Assert.Equal(GameState.GameOver, engine.State);
        Key(button, Keys.Space, down: true);
        Key(form, Keys.Enter, down: true);
        Assert.Equal(GameState.Ready, engine.State);
        Key(form, Keys.Space, down: true);
        Assert.Equal(GameState.Ready, engine.State);
        Key(form, Keys.Space, down: false);
        Key(form, Keys.Space, down: true);
        Assert.Equal(GameState.Running, engine.State);
        Assert.Equal(-320, engine.Bird.VerticalVelocity);
    });

    [Fact]
    public void FocusReturnPreservesGameOverAndRestartClearsTimingDebt() => RunOnSta((form, engine, button) =>
    {
        Lose(form, engine);
        Invoke(form, "OnDeactivate", EventArgs.Empty);
        Invoke(form, "OnActivated", EventArgs.Empty);
        Assert.Equal(GameState.GameOver, engine.State);
        Assert.False(Field<System.Windows.Forms.Timer>(form, "timer").Enabled);
        var loop = Field<FixedStepLoop>(form, "loop");
        loop.Advance(1d / 120, _ => throw new InvalidOperationException());
        button.PerformClick();
        Assert.Equal(0, loop.Advance(1d / 120, _ => throw new InvalidOperationException()));
        Assert.Equal(GameState.Ready, engine.State);
        Assert.False(Field<FlapInput>(form, "input").IsHeld);
    });

    private static void Lose(GameForm form, GameEngine engine)
    {
        engine.Flap();
        engine.Update(1);
        engine.Update(1);
        Invoke(form, "OnTimerTick", null, EventArgs.Empty);
        Assert.Equal(GameState.GameOver, engine.State);
    }

    [Fact]
    public void CompletedBestSavesOnceSurvivesRestartAndLoadsInAFreshWindow()
    {
        var store = new TestHighScoreStore();
        RunOnSta((form, engine, button) =>
        {
            ScoreThenLose(form, engine);
            var best = Field<HighScoreService>(form, "highScores");
            Assert.Equal(1, best.BestScore);
            Assert.Equal(1, store.Saves);
            for (var repaint = 0; repaint < 8; repaint++)
            {
                using var image = new System.Drawing.Bitmap(form.ClientSize.Width, form.ClientSize.Height);
                form.DrawToBitmap(image, form.ClientRectangle);
                Invoke(form, "OnTimerTick", null, EventArgs.Empty);
                Invoke(form, "OnDeactivate", EventArgs.Empty);
                Invoke(form, "OnActivated", EventArgs.Empty);
            }
            Assert.Equal(1, store.Saves);
            button.PerformClick();
            Assert.Equal(GameState.Ready, engine.State);
            Assert.Equal(0, engine.Score);
            Assert.Equal(1, best.BestScore);
            Assert.Contains("Best score 1", form.AccessibleDescription);
            Lose(form, engine); // Lower run must not rewrite the file.
            Assert.Equal(1, store.Saves);
        }, store);
        RunOnSta((form, engine, button) =>
        {
            Assert.Equal(GameState.Ready, engine.State);
            Assert.Equal(1, Field<HighScoreService>(form, "highScores").BestScore);
            Assert.Equal(1, store.Saves);
        }, store);
        Assert.Equal(2, store.Loads);
    }

    [Fact]
    public void StorageFailureDoesNotBlockLossRestartOrPlayAndRetriesOnlyNextCompletedRun()
    {
        var store = new TestHighScoreStore
        {
            LoadFailure = new IOException("test read failure"),
            SaveFailure = new UnauthorizedAccessException("test write failure"),
        };
        RunOnSta((form, engine, button) =>
        {
            Assert.Contains("Saved best score unavailable.", form.AccessibleDescription);
            ScoreThenLose(form, engine);
            Assert.Contains("Best score couldn't be saved.", form.AccessibleDescription);
            Assert.Equal(1, store.Saves);
            Invoke(form, "OnTimerTick", null, EventArgs.Empty);
            Assert.Equal(1, store.Saves);
            button.PerformClick();
            Assert.Equal(GameState.Ready, engine.State);
            Assert.Equal(1, Field<HighScoreService>(form, "highScores").BestScore);
            Assert.Equal(1, store.Saves);
            store.SaveFailure = null;
            Lose(form, engine);
            Assert.Equal(2, store.Saves);
            Assert.Equal(1, store.Value);
            Assert.Null(Field<HighScoreService>(form, "highScores").StatusMessage);
        }, store);
    }

    [Fact]
    public void PauseDescriptionAndRestartButtonScaleWithTheSameViewport() => RunOnSta((form, engine, button) =>
    {
        Key(form, Keys.Space, down: true);
        Key(form, Keys.Space, down: false);
        Invoke(form, "OnDeactivate", EventArgs.Empty);
        Assert.Contains("Paused", form.AccessibleDescription);
        Assert.False(Field<System.Windows.Forms.Timer>(form, "timer").Enabled);
        Invoke(form, "OnActivated", EventArgs.Empty);
        Assert.Contains("Running", form.AccessibleDescription);
        Lose(form, engine);
        foreach (var scale in new[] { 1f, 1.25f, 1.5f, 1f })
        {
            form.ClientSize = new System.Drawing.Size((int)(480 * scale), (int)(640 * scale));
            var viewport = SceneViewport.Create(new Playfield(), form.ClientSize);
            Assert.Equal(viewport.Map(SceneViewport.RestartButton(new Playfield())), button.Bounds);
            Assert.Equal(16 * scale, button.Font.Size);
            Assert.Equal(System.Drawing.GraphicsUnit.Pixel, button.Font.Unit);
            Assert.True(button.Font.Bold);
        }
        button.Focus();
        Assert.True(button.Focused);
        Assert.Equal(System.Drawing.Color.FromArgb(255, 207, 70), button.FlatAppearance.BorderColor);
        Key(button, Keys.Enter, down: true);
        Assert.Equal(GameState.Ready, engine.State);
        Assert.False(button.Visible);
        Assert.Equal(System.Drawing.Color.FromArgb(37, 86, 76), button.FlatAppearance.BorderColor);
    });

    private static void ScoreThenLose(GameForm form, GameEngine engine)
    {
        engine.Flap();
        engine.Pipes.Pairs[0].Reset(engine.Bird.X - engine.Pipes.Pairs[0].Width - 1, 190);
        engine.Update(1d / 60);
        Assert.Equal(1, engine.Score);
        engine.Pipes.Pairs[1].Reset(engine.Bird.X + engine.Bird.Width, 300);
        engine.Update(1d / 60);
        Invoke(form, "OnTimerTick", null, EventArgs.Empty);
        Assert.Equal(GameState.GameOver, engine.State);
    }

    private static void Key(Control control, Keys key, bool down) =>
        SendMessage(control.Handle, down ? 0x0100u : 0x0101u, (IntPtr)key, IntPtr.Zero);

    private static T Field<T>(GameForm form, string name) => (T)typeof(GameForm).GetField(name, Flags)!.GetValue(form)!;
    private static void Invoke(GameForm form, string name, params object?[] args) =>
        typeof(GameForm).GetMethod(name, Flags)!.Invoke(form, args);

    private static void RunOnSta(Action<GameForm, GameEngine, Button> action, IHighScoreStore? store = null)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new GameForm(store ?? new TestHighScoreStore())
                {
                    Opacity = 0,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.Manual,
                    Location = new System.Drawing.Point(-20000, -20000),
                };
                form.Show();
                Invoke(form, "OnActivated", EventArgs.Empty);
                action(form, Field<GameEngine>(form, "engine"), Field<Button>(form, "restartButton"));
                form.Close();
            }
            catch (Exception exception) { failure = exception; }
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "Invisible Form test did not finish.");
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
