namespace WindowsFormsApp4
{
    partial class SatisForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtUrunKodu = new System.Windows.Forms.TextBox();
            this.txtUrunAdi = new System.Windows.Forms.TextBox();
            this.txtAdet = new System.Windows.Forms.TextBox();
            this.txtToplam = new System.Windows.Forms.TextBox();
            this.dtpTarih = new System.Windows.Forms.DateTimePicker();
            this.btnSatis = new System.Windows.Forms.Button();
            this.btnFisOnayla = new System.Windows.Forms.Button();
            this.btnSepetTemizle = new System.Windows.Forms.Button();
            this.lblSepet = new System.Windows.Forms.Label();
            this.dgvSepet = new System.Windows.Forms.DataGridView();
            this.btnListele = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSepet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(111, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ürün Kodu";
            // 
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(111, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Ürün Adı";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(111, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Satış Adeti";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(111, 178);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Satış Tarihi";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(111, 213);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "Toplam Fiyat";
            // 
            // txtUrunKodu
            // 
            this.txtUrunKodu.Location = new System.Drawing.Point(196, 71);
            this.txtUrunKodu.Name = "txtUrunKodu";
            this.txtUrunKodu.Size = new System.Drawing.Size(100, 22);
            this.txtUrunKodu.TabIndex = 5;
            this.txtUrunKodu.TextChanged += new System.EventHandler(this.txtUrunKodu_TextChanged);
            // 
            // txtUrunAdi
            // 
            this.txtUrunAdi.Location = new System.Drawing.Point(196, 105);
            this.txtUrunAdi.Name = "txtUrunAdi";
            this.txtUrunAdi.ReadOnly = true;
            this.txtUrunAdi.Size = new System.Drawing.Size(200, 22);
            this.txtUrunAdi.TabIndex = 6;
            // 
            // txtAdet
            // 
            this.txtAdet.Location = new System.Drawing.Point(196, 139);
            this.txtAdet.Name = "txtAdet";
            this.txtAdet.Size = new System.Drawing.Size(100, 22);
            this.txtAdet.TabIndex = 7;
            // 
            // txtToplam
            // 
            this.txtToplam.Location = new System.Drawing.Point(196, 210);
            this.txtToplam.Name = "txtToplam";
            this.txtToplam.ReadOnly = true;
            this.txtToplam.Size = new System.Drawing.Size(100, 22);
            this.txtToplam.TabIndex = 8;
            // 
            // dtpTarih
            // 
            this.dtpTarih.Location = new System.Drawing.Point(196, 172);
            this.dtpTarih.Name = "dtpTarih";
            this.dtpTarih.Size = new System.Drawing.Size(200, 22);
            this.dtpTarih.TabIndex = 9;
            // 
            // btnSatis
            // 
            this.btnSatis.Location = new System.Drawing.Point(420, 88);
            this.btnSatis.Name = "btnSatis";
            this.btnSatis.Size = new System.Drawing.Size(100, 26);
            this.btnSatis.TabIndex = 10;
            this.btnSatis.Text = "Sepete Ekle";
            this.btnSatis.UseVisualStyleBackColor = true;
            this.btnSatis.Click += new System.EventHandler(this.btnSepeteEkle_Click);
            // 
            // btnFisOnayla
            // 
            this.btnFisOnayla.Location = new System.Drawing.Point(420, 124);
            this.btnFisOnayla.Name = "btnFisOnayla";
            this.btnFisOnayla.Size = new System.Drawing.Size(100, 26);
            this.btnFisOnayla.TabIndex = 13;
            this.btnFisOnayla.Text = "Fişi Onayla";
            this.btnFisOnayla.UseVisualStyleBackColor = true;
            this.btnFisOnayla.Click += new System.EventHandler(this.btnFisOnayla_Click);
            // 
            // btnSepetTemizle
            // 
            this.btnSepetTemizle.Location = new System.Drawing.Point(420, 160);
            this.btnSepetTemizle.Name = "btnSepetTemizle";
            this.btnSepetTemizle.Size = new System.Drawing.Size(100, 26);
            this.btnSepetTemizle.TabIndex = 14;
            this.btnSepetTemizle.Text = "Sepeti Boşalt";
            this.btnSepetTemizle.UseVisualStyleBackColor = true;
            this.btnSepetTemizle.Click += new System.EventHandler(this.btnSepetTemizle_Click);
            // 
            // lblSepet
            // 
            this.lblSepet.AutoSize = true;
            this.lblSepet.Location = new System.Drawing.Point(420, 200);
            this.lblSepet.Name = "lblSepet";
            this.lblSepet.Size = new System.Drawing.Size(44, 16);
            this.lblSepet.TabIndex = 15;
            this.lblSepet.Text = "Sepet";
            // 
            // dgvSepet
            // 
            this.dgvSepet.AllowUserToAddRows = false;
            this.dgvSepet.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSepet.Location = new System.Drawing.Point(420, 222);
            this.dgvSepet.Name = "dgvSepet";
            this.dgvSepet.ReadOnly = true;
            this.dgvSepet.RowHeadersWidth = 51;
            this.dgvSepet.RowTemplate.Height = 24;
            this.dgvSepet.Size = new System.Drawing.Size(360, 120);
            this.dgvSepet.TabIndex = 16;
            // 
            // btnListele
            // 
            this.btnListele.Location = new System.Drawing.Point(538, 88);
            this.btnListele.Name = "btnListele";
            this.btnListele.Size = new System.Drawing.Size(90, 26);
            this.btnListele.TabIndex = 11;
            this.btnListele.Text = "Satışları Listele";
            this.btnListele.UseVisualStyleBackColor = true;
            this.btnListele.Click += new System.EventHandler(this.btnListele_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(24, 355);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(752, 83);
            this.dataGridView1.TabIndex = 12;
            // 
            // SatisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.dgvSepet);
            this.Controls.Add(this.lblSepet);
            this.Controls.Add(this.btnSepetTemizle);
            this.Controls.Add(this.btnFisOnayla);
            this.Controls.Add(this.btnListele);
            this.Controls.Add(this.btnSatis);
            this.Controls.Add(this.dtpTarih);
            this.Controls.Add(this.txtToplam);
            this.Controls.Add(this.txtAdet);
            this.Controls.Add(this.txtUrunAdi);
            this.Controls.Add(this.txtUrunKodu);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "SatisForm";
            this.Text = "Satış (sepet / fiş)";
            this.Load += new System.EventHandler(this.SatisForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSepet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtUrunKodu;
        private System.Windows.Forms.TextBox txtUrunAdi;
        private System.Windows.Forms.TextBox txtAdet;
        private System.Windows.Forms.TextBox txtToplam;
        private System.Windows.Forms.DateTimePicker dtpTarih;
        private System.Windows.Forms.Button btnSatis;
        private System.Windows.Forms.Button btnFisOnayla;
        private System.Windows.Forms.Button btnSepetTemizle;
        private System.Windows.Forms.Label lblSepet;
        private System.Windows.Forms.DataGridView dgvSepet;
        private System.Windows.Forms.Button btnListele;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}