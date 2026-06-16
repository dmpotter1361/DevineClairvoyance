using System.Speech.Synthesis;

namespace DevineClairvoyance;

/// <summary>
/// The three-card reading game (formerly Form3). Click the stack to draw three
/// random cards into the Current Situation / Challenge / Advice slots, then click
/// a slot to read its meaning, with optional text-to-speech.
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
        ClientSize = new Size(1008, 768);
        FormBorderStyle = FormBorderStyle.Fixed3D;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Font;

        // Read-out pane, hidden until all three cards are drawn.
        _meaning.Location = new Point(12, 12);
        _meaning.Size = new Size(984, 416);
        _meaning.BackColor = Color.White;
        _meaning.Font = new Font("Segoe Print", 14.25f);
        _meaning.Multiline = true;
        _meaning.ReadOnly = true;
        _meaning.ScrollBars = ScrollBars.Horizontal;
        _meaning.Visible = false;

        // The three spread slots live inside a panel.
        var panel = new Panel
        {
            Location = new Point(189, 434),
            Size = new Size(639, 322),
            BackColor = Color.Transparent,
            BorderStyle = BorderStyle.FixedSingle,
        };
        for (int i = 0; i < 3; i++)
        {
            var slot = new PictureBox
            {
                Location = new Point(6 + i * 213, 7),
                Size = new Size(192, 306),
                BackgroundImage = SlotArt[i],
                BackgroundImageLayout = ImageLayout.Stretch,
                BorderStyle = BorderStyle.Fixed3D,
                SizeMode = PictureBoxSizeMode.StretchImage,
                TabStop = false,
            };
            int index = i;
            slot.Click += (_, _) => ShowSlot(index);
            _slots[i] = slot;
            panel.Controls.Add(slot);
        }

        // The draw pile.
        _stack.Location = new Point(727, 12);
        _stack.Size = new Size(269, 416);
        _stack.BackColor = Color.Transparent;
        _stack.BackgroundImage = Assets.CardStack;
        _stack.BackgroundImageLayout = ImageLayout.Stretch;
        _stack.SizeMode = PictureBoxSizeMode.StretchImage;
        _stack.TabStop = false;
        _stack.Click += OnDraw;

        _instruction.Location = new Point(12, 529);
        _instruction.Size = new Size(171, 144);
        _instruction.BackColor = Color.Black;
        _instruction.ForeColor = Color.Yellow;
        _instruction.BorderStyle = BorderStyle.Fixed3D;
        _instruction.FlatStyle = FlatStyle.Popup;
        _instruction.Font = new Font("Segoe Print", 14.25f);
        _instruction.TextAlign = ContentAlignment.MiddleCenter;
        _instruction.Text = "Select Card from Stack of cards.";
        _instruction.Click += (_, _) => SpeakIfEnabled(_instruction.Text);

        _shortPhrase.Location = new Point(834, 443);
        _shortPhrase.Size = new Size(162, 306);
        _shortPhrase.BackColor = Color.Black;
        _shortPhrase.ForeColor = Color.Yellow;
        _shortPhrase.BorderStyle = BorderStyle.Fixed3D;
        _shortPhrase.FlatStyle = FlatStyle.Popup;
        _shortPhrase.Font = new Font("Segoe Print", 9.75f);
        _shortPhrase.TextAlign = ContentAlignment.MiddleCenter;
        _shortPhrase.Click += (_, _) => SpeakIfEnabled(_shortPhrase.Text);

        _speak.Location = new Point(12, 676);
        _speak.Size = new Size(171, 40);
        _speak.BackColor = Color.Transparent;
        _speak.ForeColor = Color.Yellow;
        _speak.Font = new Font("Segoe Print", 15.75f);
        _speak.Text = "Text2Voice";
        _speak.TextAlign = ContentAlignment.MiddleCenter;
        _speak.Checked = true;

        Controls.Add(_speak);
        Controls.Add(_meaning);
        Controls.Add(_shortPhrase);
        Controls.Add(_instruction);
        Controls.Add(_stack);
        Controls.Add(panel);
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
