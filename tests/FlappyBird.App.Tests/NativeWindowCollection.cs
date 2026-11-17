namespace FlappyBird.App.Tests;

// Creating WinForms windows initializes process-wide DPI/font state. Do not change
// that state concurrently with exact off-screen renderer comparisons.
[CollectionDefinition("Native windows", DisableParallelization = true)]
public sealed class NativeWindowCollection;
