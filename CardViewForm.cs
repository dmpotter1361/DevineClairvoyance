namespace DevineClairvoyance;

/// <summary>
/// Displays a single card's art (formerly Form2). The window keeps the card's
/// portrait aspect ratio (width ≈ 60% of height) as it is resized.
/// </summary>
public sealed class CardViewForm : Form
{
    public CardViewForm(string cardName)
    {
        Text = cardName;
        Icon = Assets.AppIcon;
        BackgroundImage = Assets.Image(cardName);
        BackgroundImageLayout = ImageLayout.Stretch;
        AutoScaleMode = AutoScaleMode.Font;
        ShowInTaskbar = false;
        Width = 400;
        Height = 666;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        // Lock width to the card image's aspect ratio.
        Width = (int)(Height * 0.6006);
    }
}
