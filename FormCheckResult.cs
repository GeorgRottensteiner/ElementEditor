using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ElementEditor
{
  public partial class FormCheckResult : Form
  {
    public FormCheckResult()
    {
      InitializeComponent();
    }



    private void btnClose_Click( object sender, EventArgs e )
    {
      Hide();
    }



    public void ShowText( string Info )
    {
      editCheckResult.Text = Info;
    }



    private void FormCheckResult_FormClosing( object sender, FormClosingEventArgs e )
    {
      e.Cancel = true;
      Hide();
    }
  }
}
