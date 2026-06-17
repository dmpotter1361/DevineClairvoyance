using System.Speech.Synthesis;

namespace DevineClairvoyance;

/// <summary>
/// Main window (formerly Form1): a TreeView of all 78 cards grouped by suit,
/// a meaning pane, and buttons to view the card art, open the 3-card spread,
/// and read the meaning aloud.
///
/// Laid out with TableLayoutPanel/FlowLayoutPanel and auto-sizing buttons so it
/// survives different system fonts and display-scaling settings (controls grow to
/// fit their text instead of clipping at fixed pixel sizes).
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

    public MainForm()
    {
        Text = $"Devine Clairvoyance {ProductVersion}";
        Icon = Assets.AppIcon;
        BackgroundImage = Assets.Background;
        BackgroundImageLayout = ImageLayout.Stretch;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(860, 600);
        MinimumSize = new Size(700, 480);
        DoubleBuffered = true;
        SizeGripStyle = SizeGripStyle.Show;

        var bodyFont = AppFonts.Script(15.75f);

        // Card tree, grouped by suit.
        _tree.Dock = DockStyle.Fill;
        _tree.Margin = new Padding(8, 8, 4, 8);
        _tree.BackColor = Color.SteelBlue;
        _tree.ForeColor = Color.Black;
        _tree.BorderStyle = BorderStyle.FixedSingle;
        _tree.Font = bodyFont;
        _tree.FullRowSelect = true;
        _tree.Nodes.Add(SuitNode("Major Arcana", CardData.MajorArcana));
        _tree.Nodes.Add(SuitNode("Cups", CardData.Cups));
        _tree.Nodes.Add(SuitNode("Pentacles", CardData.Pentacles));
        _tree.Nodes.Add(SuitNode("Swords", CardData.Swords));
        _tree.Nodes.Add(SuitNode("Wands", CardData.Wands));
        _tree.AfterSelect += OnCardSelected;
        _tips.SetToolTip(_tree, "Select either an explanation of the suits or\ndrill down further for specific card meanings.");

        // Meaning pane.
        _meaning.Dock = DockStyle.Fill;
        _meaning.Margin = new Padding(4, 8, 8, 8);
        _meaning.BackColor = Color.White;
        _meaning.BorderStyle = BorderStyle.FixedSingle;
        _meaning.Font = bodyFont;
        _meaning.Multiline = true;
        _meaning.ReadOnly = true;
        _meaning.ScrollBars = ScrollBars.Both;

        // Buttons — auto-size to their text so they never clip.
        ConfigureButton(_spread, "3 Card Spread", bodyFont);
        _spread.Anchor = AnchorStyles.Left;
        _spread.Click += (_, _) => new SpreadForm().Show();
        _tips.SetToolTip(_spread, "Try a simple three-card spread to see your current situation,\nthe challenges you may face, and advice from spirit.");

        ConfigureButton(_play, "Play", bodyFont);
        _play.Click += OnPlay;
        _tips.SetToolTip(_play, "Read the information using text-to-speech.");

        ConfigureButton(_seeCard, "See Card", AppFonts.Script(18f));
        _seeCard.Enabled = false;
        _seeCard.Click += OnSeeCard;
        _tips.SetToolTip(_seeCard, "See an example of this tarot card.");

        Controls.Add(BuildLayout());
    }

    private Control BuildLayout()
    {
        // Tree | meaning, split by percentage so both scale with the window.
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        content.Controls.Add(_tree, 0, 0);
        content.Controls.Add(_meaning, 1, 0);

        // Right-aligned group of Play + See Card.
        var rightButtons = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0),
            FlowDirection = FlowDirection.LeftToRight,
        };
        rightButtons.Controls.Add(_play);
        rightButtons.Controls.Add(_seeCard);

        // Button row: spread on the left, the rest pushed right.
        var buttonRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 0, 8, 6),
        };
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        buttonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        buttonRow.Controls.Add(_spread, 0, 0);
        buttonRow.Controls.Add(rightButtons, 1, 0);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 1,
            RowCount = 2,
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(content, 0, 0);
        root.Controls.Add(buttonRow, 0, 1);
        return root;
    }

    private static void ConfigureButton(Button b, string text, Font font)
    {
        b.Text = text;
        b.Font = font;
        b.AutoSize = true;
        b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        b.Padding = new Padding(12, 4, 12, 4);
        b.Margin = new Padding(6, 4, 0, 4);
        b.UseVisualStyleBackColor = true;
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
