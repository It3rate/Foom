
namespace MathDemo;

	partial class CoreForm
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CoreForm));
        corePanel = new Panel();
        lbText = new Label();
        label1 = new Label();
        corePanel.SuspendLayout();
        SuspendLayout();
        // 
        // corePanel
        // 
        corePanel.AutoSize = true;
        corePanel.BackColor = Color.FromArgb(64, 64, 64);
        corePanel.Controls.Add(lbText);
        corePanel.Controls.Add(label1);
        corePanel.Dock = DockStyle.Fill;
        corePanel.Location = new Point(0, 0);
        corePanel.Margin = new Padding(2);
        corePanel.Name = "corePanel";
        corePanel.Size = new Size(867, 554);
        corePanel.TabIndex = 0;
        // 
        // lbText
        // 
        lbText.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        lbText.AutoSize = true;
        lbText.BackColor = SystemColors.Window;
        lbText.Font = new Font("Trebuchet MS", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
        lbText.ForeColor = Color.FromArgb(0, 0, 64);
        lbText.Location = new Point(750, 533);
        lbText.Margin = new Padding(2, 0, 2, 0);
        lbText.Name = "lbText";
        lbText.Size = new Size(109, 20);
        lbText.TabIndex = 3;
        lbText.Text = "(2i+4) * (-3i-2)";
        // 
        // label1
        // 
        label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        label1.AutoSize = true;
        label1.BackColor = SystemColors.Window;
        label1.Font = new Font("Segoe UI", 6F, FontStyle.Regular, GraphicsUnit.Point, 0);
        label1.Location = new Point(0, 419);
        label1.Margin = new Padding(2, 0, 2, 0);
        label1.Name = "label1";
        label1.Size = new Size(149, 165);
        label1.TabIndex = 2;
        label1.Text = resources.GetString("label1.Text");
        // 
        // CoreForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(64, 64, 64);
        ClientSize = new Size(867, 554);
        Controls.Add(corePanel);
        Margin = new Padding(2);
        Name = "CoreForm";
        Text = "Foom";
        corePanel.ResumeLayout(false);
        corePanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Panel corePanel;
    private System.Windows.Forms.Label lbText;
    private System.Windows.Forms.Label label1;
}

