namespace FlappyBird.App.Input;

/// <summary>Turns key messages into press edges, including held keys across focus changes.</summary>
public sealed class FlapInput
{
    private bool isDown;
    private bool waitingForRelease;

    public bool Press()
    {
        if (isDown || waitingForRelease)
        {
            return false;
        }

        isDown = true;
        return true;
    }

    public void Release()
    {
        isDown = false;
        waitingForRelease = false;
    }

    public void Suspend()
    {
        isDown = false;
        waitingForRelease = true;
    }

    public void Resume(bool spaceHeld)
    {
        isDown = false;
        waitingForRelease = spaceHeld;
    }
}
