using System.ComponentModel;

namespace DesktopKnowledge.Forms;

partial class ExamForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

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
        listBox1 = new System.Windows.Forms.ListBox();
        label1 = new System.Windows.Forms.Label();
        label2 = new System.Windows.Forms.Label();
        listBox2 = new System.Windows.Forms.ListBox();
        tabControl1 = new System.Windows.Forms.TabControl();
        tabPage1 = new System.Windows.Forms.TabPage();
        tabPage2 = new System.Windows.Forms.TabPage();
        label3 = new System.Windows.Forms.Label();
        button1 = new System.Windows.Forms.Button();
        button2 = new System.Windows.Forms.Button();
        label4 = new System.Windows.Forms.Label();
        tabControl1.SuspendLayout();
        SuspendLayout();
        // 
        // listBox1
        // 
        listBox1.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
        listBox1.FormattingEnabled = true;
        listBox1.ItemHeight = 24;
        listBox1.Location = new System.Drawing.Point(12, 41);
        listBox1.Name = "listBox1";
        listBox1.Size = new System.Drawing.Size(220, 220);
        listBox1.TabIndex = 0;
        // 
        // label1
        // 
        label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 13.200001F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)134));
        label1.Location = new System.Drawing.Point(12, 9);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(220, 29);
        label1.TabIndex = 1;
        label1.Text = "区域选择";
        label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // label2
        // 
        label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 13.200001F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)134));
        label2.Location = new System.Drawing.Point(12, 264);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(220, 29);
        label2.TabIndex = 2;
        label2.Text = "题目选择";
        label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // listBox2
        // 
        listBox2.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
        listBox2.FormattingEnabled = true;
        listBox2.ItemHeight = 24;
        listBox2.Location = new System.Drawing.Point(12, 296);
        listBox2.Name = "listBox2";
        listBox2.Size = new System.Drawing.Size(220, 340);
        listBox2.TabIndex = 3;
        // 
        // tabControl1
        // 
        tabControl1.Controls.Add(tabPage1);
        tabControl1.Controls.Add(tabPage2);
        tabControl1.Location = new System.Drawing.Point(238, 55);
        tabControl1.Name = "tabControl1";
        tabControl1.SelectedIndex = 0;
        tabControl1.Size = new System.Drawing.Size(768, 583);
        tabControl1.TabIndex = 4;
        // 
        // tabPage1
        // 
        tabPage1.Location = new System.Drawing.Point(4, 29);
        tabPage1.Name = "tabPage1";
        tabPage1.Padding = new System.Windows.Forms.Padding(3);
        tabPage1.Size = new System.Drawing.Size(760, 550);
        tabPage1.TabIndex = 0;
        tabPage1.Text = "tabPage1";
        tabPage1.UseVisualStyleBackColor = true;
        // 
        // tabPage2
        // 
        tabPage2.Location = new System.Drawing.Point(4, 29);
        tabPage2.Name = "tabPage2";
        tabPage2.Padding = new System.Windows.Forms.Padding(3);
        tabPage2.Size = new System.Drawing.Size(421, 371);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "tabPage2";
        tabPage2.UseVisualStyleBackColor = true;
        // 
        // label3
        // 
        label3.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F);
        label3.Location = new System.Drawing.Point(242, 9);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(119, 40);
        label3.TabIndex = 5;
        label3.Text = "答题区域";
        label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // button1
        // 
        button1.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
        button1.Location = new System.Drawing.Point(907, 9);
        button1.Name = "button1";
        button1.Size = new System.Drawing.Size(99, 40);
        button1.TabIndex = 6;
        button1.Text = "提交";
        button1.UseVisualStyleBackColor = true;
        // 
        // button2
        // 
        button2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
        button2.Location = new System.Drawing.Point(802, 9);
        button2.Name = "button2";
        button2.Size = new System.Drawing.Size(99, 40);
        button2.TabIndex = 7;
        button2.Text = "保存";
        button2.UseVisualStyleBackColor = true;
        // 
        // label4
        // 
        label4.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
        label4.Location = new System.Drawing.Point(367, 9);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(429, 40);
        label4.TabIndex = 8;
        label4.Text = "(ExamTitle)";
        label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // ExamForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1018, 650);
        Controls.Add(label4);
        Controls.Add(button2);
        Controls.Add(button1);
        Controls.Add(label3);
        Controls.Add(tabControl1);
        Controls.Add(listBox2);
        Controls.Add(label2);
        Controls.Add(label1);
        Controls.Add(listBox1);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        Text = "Open Knowledge > 测试";
        tabControl1.ResumeLayout(false);
        ResumeLayout(false);
    }

    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Label label4;

    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ListBox listBox2;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button button1;

    private System.Windows.Forms.Label label1;

    private System.Windows.Forms.ListBox listBox1;

    #endregion
}