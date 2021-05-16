﻿namespace ElementEditor
{
  partial class FormCheckResult
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose( bool disposing )
    {
      if ( disposing && ( components != null ) )
      {
        components.Dispose();
      }
      base.Dispose( disposing );
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.editCheckResult = new System.Windows.Forms.TextBox();
      this.btnClose = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // editCheckResult
      // 
      this.editCheckResult.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( ( ( System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom )
                  | System.Windows.Forms.AnchorStyles.Left )
                  | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.editCheckResult.Location = new System.Drawing.Point( 12, 12 );
      this.editCheckResult.Multiline = true;
      this.editCheckResult.Name = "editCheckResult";
      this.editCheckResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.editCheckResult.Size = new System.Drawing.Size( 768, 506 );
      this.editCheckResult.TabIndex = 0;
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ( (System.Windows.Forms.AnchorStyles)( ( System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right ) ) );
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnClose.Location = new System.Drawing.Point( 705, 524 );
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size( 75, 23 );
      this.btnClose.TabIndex = 1;
      this.btnClose.Text = "Close";
      this.btnClose.UseVisualStyleBackColor = true;
      this.btnClose.Click += new System.EventHandler( this.btnClose_Click );
      // 
      // FormCheckResult
      // 
      this.AcceptButton = this.btnClose;
      this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnClose;
      this.ClientSize = new System.Drawing.Size( 792, 557 );
      this.Controls.Add( this.btnClose );
      this.Controls.Add( this.editCheckResult );
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormCheckResult";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Check Result";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler( this.FormCheckResult_FormClosing );
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox editCheckResult;
    private System.Windows.Forms.Button btnClose;
  }
}