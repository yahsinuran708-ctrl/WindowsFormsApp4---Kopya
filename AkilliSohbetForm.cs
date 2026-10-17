using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class AkilliSohbetForm : Form
    {
        private readonly TextBox txtInput = new TextBox();
        private readonly Button btnSend = new Button();
        private readonly RichTextBox rtbChat = new RichTextBox();

        private ChatbotQueryEngine _engine;

        public AkilliSohbetForm()
        {
            InitializeComponent();
            BuildMinimalUi();
        }

        private void AkilliSohbetForm_Load(object sender, EventArgs e)
        {
            _engine = new ChatbotQueryEngine(Db.ConnectionString);
            rtbChat.AppendText("Sohbete başlamak için soru yazın. Örn: 'barkod 123 stok nedir'\n");
        }

        private void BuildMinimalUi()
        {
            Text = "Akıllı Sohbet";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 900;
            Height = 600;

            rtbChat.Dock = DockStyle.Top;
            rtbChat.Height = 420;
            rtbChat.ReadOnly = true;
            rtbChat.Font = new Font("Segoe UI", 10);

            txtInput.Location = new Point(12, 430);
            txtInput.Width = 780;

            btnSend.Text = "Gönder";
            btnSend.Location = new Point(800, 428);
            btnSend.Width = 80;
            btnSend.Click += btnSend_Click;

            Controls.Add(rtbChat);
            Controls.Add(txtInput);
            Controls.Add(btnSend);

            Load += AkilliSohbetForm_Load;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var q = txtInput.Text;
            if (string.IsNullOrWhiteSpace(q))
                return;

            rtbChat.AppendText("\nSiz: " + q + "\n");
            txtInput.Clear();

            var ans = _engine == null ? "Motor başlatılamadı." : _engine.Ask(q);
            rtbChat.AppendText("\nBot: " + ans + "\n");
        }

        private void AkilliSohbetForm_Load_1(object sender, EventArgs e)
        {

        }
    }
}

