using System;

namespace Match3.Presentation.UI;

public sealed class UiButton(string label, Action onClick)
{
    private readonly Action onClick = onClick ?? throw new ArgumentNullException(nameof(onClick));

    public string Label { get; } = label ?? throw new ArgumentNullException(nameof(label));

    public void Click()
    {
        onClick();
    }
}
