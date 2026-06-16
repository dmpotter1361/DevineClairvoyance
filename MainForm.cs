using System.Speech.Synthesis;

namespace DevineClairvoyance;

/// <summary>
/// Main window (formerly Form1): a TreeView of all 78 cards grouped by suit,
/// a meaning pane, and buttons to view the card art, open the 3-card spread,
/// and read the meaning aloud.
/// </summary>
public sealed class MainForm : Form
{
    private readonly SpeechSynthesizer _synth = new();
    private readonly TreeView _tree = new();
    private readonly TextBox _meaning = new();
    private readonly Button _seeCard = new();
    private readonly Button _spread = new();
    private readonly Button _play = new();
    private readonly ToolTip _tips = new();

    private string _currentCard = string.Empty;

    private static readonly Font UiFont = new("Segoe Print", 15.75f);

    public MainForm()
    {
        Text = $"Devine Clairvoyance {ProductVersion}";
        Icon = Assets.AppIcon;
        BackgroundImage = Assets.Background;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(834, 561);
        MinimumSize = new Size(850, 600);
        MaximumSize = new Size(2048, 2048);
        DoubleBuffered = true;
        SizeGripStyle = SizeGripStyle.Show;
        AutoScaleMode = AutoScaleMode.Font;

        // Card tree, grouped by suit.
        _tree.Location = new Point(12, 12);
        _tree.Size = new Size(274, 481);
        _tree.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        _tree.BackColor = Color.SteelBlue;
        _tree.ForeColor = Color.Black;
        _tree.BorderStyle = BorderStyle.FixedSingle;
        _tree.Font = UiFont;
        _tree.FullRowSelect = true;
        _tree.Nodes.Add(SuitNode("Major Arcana", CardData.MajorArcana));
        _tree.Nodes.Add(SuitNode("Cups", CardData.Cups));
        _tree.Nodes.Add(SuitNode("Pentacles", CardData.Pentacles));
        _tree.Nodes.Add(SuitNode("Swords", CardData.Swords));
        _tree.Nodes.Add(SuitNode("Wands", CardData.Wands));
        _tree.AfterSelect += OnCardSelected;
        _tips.SetToolTip(_tree, "Select either an explanation of the suits or\ndrill down further for specific card meanings.");

        // Meaning pane.
        _meaning.Location = new Point(302, 12);
        _meaning.Size = new Size(520, 481);
        _meaning.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _meaning.BackColor = Color.White;
        _meaning.BorderStyle = BorderStyle.FixedSingle;
        _meaning.Font = UiFont;
        _meaning.Multiline = true;
        _meaning.ReadOnly = true;
        _meaning.ScrollBars = ScrollBars.Both;

        // Buttons.
        _spread.Text = "3 Card Spread";
        _spread.Location = new Point(12, 501);
        _spread.Size = new Size(189, 52);
        _spread.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        _spread.BackColor = Color.Transparent;
        _spread.Font = UiFont;
        _spread.Click += (_, _) => new SpreadForm().Show();
        _tips.SetToolTip(_spread, "Try a simple three-card spread to see your current situation,\nthe challenges you may face, and advice from spirit.");

        _play.Text = "Play";
        _play.Location = new Point(541, 501);
        _play.Size = new Size(135, 52);
        _play.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _play.Font = UiFont;
        _play.Click += OnPlay;
        _tips.SetToolTip(_play, "Read the information using text-to-speech.");

        _seeCard.Text = "See Card";
        _seeCard.Location = new Point(682, 501);
        _seeCard.Size = new Size(136, 52);
        _seeCard.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _seeCard.BackColor = Color.Transparent;
        _seeCard.Font = new Font("Segoe Print", 18f);
        _seeCard.Enabled = false;
        _seeCard.Click += OnSeeCard;
        _tips.SetToolTip(_seeCard, "See an example of this tarot card.");

        Controls.Add(_tree);
        Controls.Add(_meaning);
        Controls.Add(_spread);
        Controls.Add(_play);
        Controls.Add(_seeCard);
    }

    private static TreeNode SuitNode(string suit, IEnumerable<string> cards)
    {
        var node = new TreeNode(suit);
        foreach (var card in cards)
            node.Nodes.Add(card);
        return node;
    }

    private void OnCardSelected(object? sender, TreeViewEventArgs e)
    {
        var name = e.Node?.Text ?? string.Empty;
        _currentCard = name;
        _meaning.Text = CardData.GetMeaning(name);
        _meaning.SelectionStart = 0;
        _meaning.ScrollToCaret();
        // Suit (parent) nodes have no single card image to show.
        _seeCard.Enabled = e.Node?.Parent is not null;
    }

    private void OnSeeCard(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentCard))
            return;
        new CardViewForm(_currentCard).Show();
    }

    private void OnPlay(object? sender, EventArgs e)
    {
        if (_play.Text == "Play")
        {
            _synth.SpeakAsync(_meaning.Text);
            _play.Text = "Stop";
        }
        else
        {
            _synth.SpeakAsyncCancelAll();
            _play.Text = "Play";
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _synth.SpeakAsyncCancelAll();
        _synth.Dispose();
        base.OnFormClosed(e);
    }
}
