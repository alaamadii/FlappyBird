using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using FlappyBird.App.Forms;
using FlappyBird.App.Input;
using FlappyBird.App.Rendering;
using FlappyBird.App.Timing;
using FlappyBird.Core.Models;
using FlappyBird.Core.Services;

namespace FlappyBird.App.Tests;

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

    private static void Key(Control control, Keys key, bool down) =>
        SendMessage(control.Handle, down ? 0x0100u : 0x0101u, (IntPtr)key, IntPtr.Zero);

    private static T Field<T>(GameForm form, string name) => (T)typeof(GameForm).GetField(name, Flags)!.GetValue(form)!;
    private static void Invoke(GameForm form, string name, params object?[] args) =>
        typeof(GameForm).GetMethod(name, Flags)!.Invoke(form, args);

    private static void RunOnSta(Action<GameForm, GameEngine, Button> action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new GameForm
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
