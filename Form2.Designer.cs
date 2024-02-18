namespace OpentkGraphics
{
    partial class Form2
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
            components = new System.ComponentModel.Container();
            label1 = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            textBox1 = new TextBox();
            label2 = new Label();
            label3 = new Label();
            timer2 = new System.Windows.Forms.Timer(components);
            timer3 = new System.Windows.Forms.Timer(components);
            trackBar1 = new TrackBar();
            label4 = new Label();
            groupBox1 = new GroupBox();
            label6 = new Label();
            label5 = new Label();
            checkBox1 = new CheckBox();
            label7 = new Label();
            label8 = new Label();
            checkBox2 = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(292, 245);
            label1.Name = "label1";
            label1.Size = new Size(27, 15);
            label1.TabIndex = 0;
            label1.Text = "Log";
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 1;
            timer1.Tick += timer1_Tick;
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            textBox1.Location = new Point(292, 22);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = ScrollBars.Both;
            textBox1.Size = new Size(286, 220);
            textBox1.TabIndex = 1;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(292, 269);
            label2.Name = "label2";
            label2.Size = new Size(23, 15);
            label2.TabIndex = 2;
            label2.Text = "fps";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(292, 286);
            label3.Name = "label3";
            label3.Size = new Size(29, 15);
            label3.TabIndex = 3;
            label3.Text = "fp5s";
            // 
            // timer2
            // 
            timer2.Enabled = true;
            timer2.Interval = 5000;
            timer2.Tick += timer2_Tick;
            // 
            // timer3
            // 
            timer3.Enabled = true;
            timer3.Interval = 60;
            timer3.Tick += timer3_Tick;
            // 
            // trackBar1
            // 
            trackBar1.Location = new Point(3, 376);
            trackBar1.Maximum = 165;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(241, 45);
            trackBar1.SmallChange = 5;
            trackBar1.TabIndex = 4;
            trackBar1.Value = 60;
            trackBar1.Scroll += trackBar1_Scroll;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 358);
            label4.Name = "label4";
            label4.Size = new Size(123, 15);
            label4.TabIndex = 5;
            label4.Text = "Maximum Frame Rate";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label3);
            groupBox1.Location = new Point(12, 7);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(584, 327);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Stats";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(292, 309);
            label6.Name = "label6";
            label6.Size = new Size(217, 15);
            label6.TabIndex = 4;
            label6.Text = "BLACK - FPS;   BLUE - FRAMETIME*1000";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(250, 376);
            label5.Name = "label5";
            label5.Size = new Size(19, 15);
            label5.TabIndex = 7;
            label5.Text = "60";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(275, 376);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(63, 19);
            checkBox1.TabIndex = 8;
            checkBox1.Text = "V-Sync";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(335, 337);
            label7.Name = "label7";
            label7.Size = new Size(74, 15);
            label7.TabIndex = 9;
            label7.Text = "Bad Shaders:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(12, 337);
            label8.Name = "label8";
            label8.Size = new Size(38, 15);
            label8.TabIndex = 10;
            label8.Text = "label8";
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(275, 396);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(79, 19);
            checkBox2.TabIndex = 11;
            checkBox2.Text = "Fullscreen";
            checkBox2.UseVisualStyleBackColor = true;
            checkBox2.CheckedChanged += checkBox2_CheckedChanged;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(608, 427);
            Controls.Add(checkBox2);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(checkBox1);
            Controls.Add(label5);
            Controls.Add(groupBox1);
            Controls.Add(label4);
            Controls.Add(trackBar1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "Form2";
            Text = "Form2";
            Load += Form2_Load;
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private System.Windows.Forms.Timer timer1;
        private TextBox textBox1;
        private Label label2;
        private Label label3;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Timer timer3;
        private TrackBar trackBar1;
        private Label label4;
        private GroupBox groupBox1;
        private Label label5;
        private Label label6;
        private CheckBox checkBox1;
        private Label label7;
        private Label label8;
        private CheckBox checkBox2;
    }
}