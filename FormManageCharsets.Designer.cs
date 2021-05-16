namespace ElementEditor
{
  partial class FormManageCharsets
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
      this.listCharsets = new System.Windows.Forms.ListBox();
      this.label1 = new System.Windows.Forms.Label();
      this.btnAddCharset = new System.Windows.Forms.Button();
      this.btnRemoveCharset = new System.Windows.Forms.Button();
      this.btnRefreshCharset = new System.Windows.Forms.Button();
      this.btnChangeCharset = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // listCharsets
      // 
      this.listCharsets.FormattingEnabled = true;
      this.listCharsets.Location = new System.Drawing.Point( 12, 25 );
      this.listCharsets.Name = "listCharsets";
      this.listCharsets.Size = new System.Drawing.Size( 193, 225 );
      this.listCharsets.TabIndex = 0;
      this.listCharsets.SelectedIndexChanged += new System.EventHandler( this.listCharsets_SelectedIndexChanged );
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point( 12, 9 );
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size( 87, 13 );
      this.label1.TabIndex = 1;
      this.label1.Text = "Loaded Charsets";
      // 
      // btnAddCharset
      // 
      this.btnAddCharset.Location = new System.Drawing.Point( 250, 25 );
      this.btnAddCharset.Name = "btnAddCharset";
      this.btnAddCharset.Size = new System.Drawing.Size( 75, 23 );
      this.btnAddCharset.TabIndex = 2;
      this.btnAddCharset.Text = "Add...";
      this.btnAddCharset.UseVisualStyleBackColor = true;
      this.btnAddCharset.Click += new System.EventHandler( this.btnAddCharset_Click );
      // 
      // btnRemoveCharset
      // 
      this.btnRemoveCharset.Enabled = false;
      this.btnRemoveCharset.Location = new System.Drawing.Point( 412, 54 );
      this.btnRemoveCharset.Name = "btnRemoveCharset";
      this.btnRemoveCharset.Size = new System.Drawing.Size( 75, 23 );
      this.btnRemoveCharset.TabIndex = 2;
      this.btnRemoveCharset.Text = "Remove";
      this.btnRemoveCharset.UseVisualStyleBackColor = true;
      this.btnRemoveCharset.Click += new System.EventHandler( this.btnRemoveCharset_Click );
      // 
      // btnRefreshCharset
      // 
      this.btnRefreshCharset.Enabled = false;
      this.btnRefreshCharset.Location = new System.Drawing.Point( 250, 54 );
      this.btnRefreshCharset.Name = "btnRefreshCharset";
      this.btnRefreshCharset.Size = new System.Drawing.Size( 75, 23 );
      this.btnRefreshCharset.TabIndex = 2;
      this.btnRefreshCharset.Text = "Refresh";
      this.btnRefreshCharset.UseVisualStyleBackColor = true;
      this.btnRefreshCharset.Click += new System.EventHandler( this.btnRefreshCharset_Click );
      // 
      // btnChangeCharset
      // 
      this.btnChangeCharset.Enabled = false;
      this.btnChangeCharset.Location = new System.Drawing.Point( 331, 54 );
      this.btnChangeCharset.Name = "btnChangeCharset";
      this.btnChangeCharset.Size = new System.Drawing.Size( 75, 23 );
      this.btnChangeCharset.TabIndex = 2;
      this.btnChangeCharset.Text = "Change...";
      this.btnChangeCharset.UseVisualStyleBackColor = true;
      this.btnChangeCharset.Click += new System.EventHandler( this.btnChangeCharset_Click );
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point( 412, 227 );
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size( 75, 23 );
      this.btnOK.TabIndex = 2;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler( this.btnOK_Click );
      // 
      // FormManageCharsets
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size( 497, 262 );
      this.Controls.Add( this.btnRefreshCharset );
      this.Controls.Add( this.btnChangeCharset );
      this.Controls.Add( this.btnRemoveCharset );
      this.Controls.Add( this.btnOK );
      this.Controls.Add( this.btnAddCharset );
      this.Controls.Add( this.label1 );
      this.Controls.Add( this.listCharsets );
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormManageCharsets";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Manage Charsets";
      this.ResumeLayout( false );
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListBox listCharsets;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnAddCharset;
    private System.Windows.Forms.Button btnRemoveCharset;
    private System.Windows.Forms.Button btnRefreshCharset;
    private System.Windows.Forms.Button btnChangeCharset;
    private System.Windows.Forms.Button btnOK;
  }
}