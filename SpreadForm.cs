using System.Speech.Synthesis;

namespace DevineClairvoyance;

/// <summary>
/// The three-card reading game (formerly Form3). Click the stack to draw three
/// random cards into the Current Situation / Challenge / Advice slots, then click
/// a slot to read its meaning, with optional text-to-speech.
///
/// Built with layout panels and wrapping auto-size labels so the long instruction
/// text and controls adapt to the user's font and display-scaling settings instead
/// of clipping at fixed pixel sizes.
/// </summary>
public sealed class SpreadForm : Form
{
    private readonly SpeechSynthesizer _synth = new();
    private readonly Random _rng = new();

    private readonly PictureBox _stack = new();
    private readonly PictureBox[] _slots = new PictureBox[3];
    private readonly Label _instruction = new();
    private readonly Label _shortPhrase = new();
    private readonly TextBox _meaning = new();
    private readonly CheckBox _speak = new();

    private readonly string[] _picked = new string[3];
    private int _pickedCount;

    private static readonly Image[] SlotArt =
    {
        Assets.CurrentSituation, Assets.Challenge, Assets.Advice,
    };

    private static readonly string[] SlotPrompt =
    {
        "Interpret your Current situation.",
        "Interpret the Challenge you may be going though.",
        "Interpret Advice from the Spirit.",
    };

    public SpreadForm()
    {
        Text = "Situation Spread";
        Icon = Assets.AppIcon;
        BackgroundImage = Assets.Background;
        BackgroundImageLayout = ImageLayout.Stretch;
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1000, 720);
        MinimumSize = new Size(760, 560);

        var bodyFont = AppFonts.Script(14.25f);

        // Read-out pane, hidden until all three cards are drawn.
        _meaning.Dock = DockStyle.Fill;
        _meaning.Margin = new Padding(8);
        _meaning.BackColor = Color.White;
        _meaning.Font = bodyFont;
        _meaning.Multiline = true;
        _meaning.ReadOnly = true;
        _meaning.ScrollBars = ScrollBars.Vertical;
        _meaning.Visible = false;

        // The three spread slots.
        for (int i = 0; i < 3; i++)
        {
            var slot = new PictureBox
            {
                Size = new Size(150, 240),
                Margin = new Padding(6, 0, 6, 0),
                BackgroundImage = SlotArt[i],
                BackgroundImageLayout = ImageLayout.Stretch,
                BorderStyle = BorderStyle.Fixed3D,
                SizeMode = PictureBoxSizeMode.StretchImage,
                TabStop = false,
            };
            int index = i;
            slot.Click += (_, _) => ShowSlot(index);
            _slots[i] = slot;
        }

        // The draw pile.
        _stack.Size = new Size(200, 300);
        _stack.Margin = new Padding(0, 0, 0, 8);
        _stack.BackColor = Color.Transparent;
        _stack.BackgroundImage = Assets.CardStack;
        _stack.BackgroundImageLayout = ImageLayout.Stretch;
        _stack.SizeMode = PictureBoxSizeMode.StretchImage;
        _stack.TabStop = false;
        _stack.Cursor = Cursors.Hand;
        _stack.Click += OnDraw;

        ConfigureBlackLabel(_instruction, AppFonts.Script(14.25f), 220);
        _instruction.Text = "Select Card from Stack of cards.";
        _instruction.Click += (_, _) => SpeakIfEnabled(_instruction.Text);

        ConfigureBlackLabel(_shortPhrase, AppFonts.Script(10.5f), 200);
        _shortPhrase.Click += (_, _) => SpeakIfEnabled(_shortPhrase.Text);

        _speak.AutoSize = true;
        _speak.BackColor = Color.Transparent;
        _speak.ForeColor = Color.Yellow;
        _speak.Font = AppFonts.Script(15.75f);
        _speak.Text = "Text2Voice";
        _speak.Checked = true;
        _speak.Margin = new Padding(0, 12, 0, 0);

        Controls.Add(BuildLayout());
    }

    private Control BuildLayout()
    {
        // Left column: instruction text over the Text2Voice toggle.
        var left = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            FlowDirection = FlowDirection.TopDown,
            Anchor = AnchorStyles.Top,
            Margin = new Padding(8),
        };
        left.Controls.Add(_instruction);
        left.Controls.Add(_speak);

        // Centre column: the three card slots in a fixed 3-column row (a
        // TableLayoutPanel, not a FlowLayoutPanel, so the cards never wrap).
        var slotRow = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            ColumnCount = 3,
            RowCount = 1,
            Anchor = AnchorStyles.None, // centred in the cell
            Margin = new Padding(8),
        };
        for (int i = 0; i < 3; i++)
            slotRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        slotRow.Controls.Add(_slots[0], 0, 0);
        slotRow.Controls.Add(_slots[1], 1, 0);
        slotRow.Controls.Add(_slots[2], 2, 0);

        // Right column: the draw pile over the short-phrase readout.
        var right = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            FlowDirection = FlowDirection.TopDown,
            Anchor = AnchorStyles.Top,
            Margin = new Padding(8),
        };
        right.Controls.Add(_stack);
        right.Controls.Add(_shortPhrase);

        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 3,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
        };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bottom.Controls.Add(left, 0, 0);
        bottom.Controls.Add(slotRow, 1, 0);
        bottom.Controls.Add(right, 2, 0);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 1,
            RowCount = 2,
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(_meaning, 0, 0);
        root.Controls.Add(bottom, 0, 1);
        return root;
    }

    // A black panel-style label with yellow text that wraps and grows to fit.
    private static void ConfigureBlackLabel(Label label, Font font, int maxWidth)
    {
        label.AutoSize = true;
        label.MaximumSize = new Size(maxWidth, 0);
        label.MinimumSize = new Size(maxWidth, 0);
        label.BackColor = Color.Black;
        label.ForeColor = Color.Yellow;
        label.BorderStyle = BorderStyle.Fixed3D;
        label.Font = font;
        label.Padding = new Padding(8);
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.Cursor = Cursors.Hand;
    }

    private void OnDraw(object? sender, EventArgs e)
    {
        if (_pickedCount >= 3)
            return;

        var card = DrawCard();
        _picked[_pickedCount] = card;
        _slots[_pickedCount].BackgroundImage = Assets.Image(card);
        _slots[_pickedCount].BorderStyle = BorderStyle.FixedSingle;
        _pickedCount++;

        if (_pickedCount < 3)
        {
            _instruction.Text = _pickedCount == 1
                ? "Select your second card."
                : "Select your Third card.";
        }
        else
        {
            _instruction.Text = "Select cards on right to show meanings.";
            _stack.Visible = false;
            _meaning.Text = "Select Card from below for meaning.";
            _meaning.Visible = true;
        }
    }

    private string DrawCard()
    {
        // The original drew with replacement; keep that behaviour.
        var deck = CardData.AllCards.ToArray();
        return deck[_rng.Next(deck.Length)];
    }

    private void ShowSlot(int index)
    {
        if (_pickedCount < 3)
            return;

        var card = _picked[index];
        _instruction.Text = SlotPrompt[index];
        _meaning.Text = CardData.GetMeaning(card);
        _meaning.SelectionStart = 0;
        _meaning.ScrollToCaret();
        _shortPhrase.Text = CardData.GetShortTerm(card);

        _synth.SpeakAsyncCancelAll();
        SpeakIfEnabled(_meaning.Text);
    }

    private void SpeakIfEnabled(string text)
    {
        if (!_speak.Checked || string.IsNullOrWhiteSpace(text))
            return;
        _synth.SpeakAsyncCancelAll();
        _synth.SpeakAsync(text);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _synth.SpeakAsyncCancelAll();
        _synth.Dispose();
        base.OnFormClosed(e);
    }
}
