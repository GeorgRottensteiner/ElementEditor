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
  public partial class FormManageCharsets : Form
  {
    FormMain main = null;


    public FormManageCharsets( FormMain Main )
    {
      main = Main;
      InitializeComponent();

      foreach ( ElementEditor.Types.CharsetProjectInfo charset in main.m_Project.CharsetProjects )
      {
        string shortName = System.IO.Path.GetFileNameWithoutExtension( charset.Filename );
        listCharsets.Items.Add( shortName );
      }
    }



    private void listCharsets_SelectedIndexChanged( object sender, EventArgs e )
    {
      btnRefreshCharset.Enabled = ( listCharsets.SelectedIndex != -1 );
      btnChangeCharset.Enabled = ( listCharsets.SelectedIndex != -1 );
      btnRemoveCharset.Enabled = ( ( listCharsets.SelectedIndex != -1 ) && ( listCharsets.Items.Count >= 2 ) );
    }



    private void btnAddCharset_Click( object sender, EventArgs e )
    {
      OpenFileDialog openFile = new OpenFileDialog();

      openFile.Title = "Open charset project";
      openFile.Filter = "Charset Project Files|*.charsetproject";

      if ( openFile.ShowDialog() == DialogResult.OK )
      {
        Types.CharsetProject charSet = main.OpenCharsetProject( openFile.FileName );
        if ( charSet != null )
        {
          charSet.Name = openFile.FileName;
          string shortName = System.IO.Path.GetFileNameWithoutExtension( openFile.FileName );
          main.m_Project.Charsets.Add( charSet );

          Types.CharsetProjectInfo info = new ElementEditor.Types.CharsetProjectInfo();
          info.Filename = charSet.Name;
          info.Multicolor = true;
          main.m_Project.CharsetProjects.Add( info );
          main.comboScreenCharset.Items.Add( shortName );
          main.comboElementCharset.Items.Add( shortName );

          listCharsets.Items.Add( shortName );
        }
      }
    }



    private void btnOK_Click( object sender, EventArgs e )
    {
      DialogResult = DialogResult.OK;
      Close();
    }



    private void btnRefreshCharset_Click( object sender, EventArgs e )
    {
      int charsetIndex = listCharsets.SelectedIndex;
      if ( charsetIndex != -1 )
      {
        Types.CharsetProject charSet = main.OpenCharsetProject( main.m_Project.CharsetProjects[charsetIndex].Filename );
        main.m_Project.Charsets[charsetIndex] = charSet;
        main.SetActiveElementCharset( main.m_Project.Charsets[main.comboElementCharset.SelectedIndex],
                                      main.m_Project.Charsets[main.comboElementCharset.SelectedIndex].MultiColor1,
                                      main.m_Project.Charsets[main.comboElementCharset.SelectedIndex].MultiColor2,
                                      main.m_Project.CharsetProjects[main.comboElementCharset.SelectedIndex].Multicolor );
      }
    }



    private void btnChangeCharset_Click( object sender, EventArgs e )
    {
      int charsetIndex = listCharsets.SelectedIndex;
      if ( charsetIndex != -1 )
      {
        OpenFileDialog openFile = new OpenFileDialog();

        openFile.Title = "Open charset project";
        openFile.Filter = "Charset Project Files|*.charsetproject";

        if ( openFile.ShowDialog() == DialogResult.OK )
        {
          Types.CharsetProject charSet = main.OpenCharsetProject( openFile.FileName );
          if ( charSet != null )
          {
            charSet.Name = openFile.FileName;
            string shortName = System.IO.Path.GetFileNameWithoutExtension( openFile.FileName );

            main.m_Project.Charsets[charsetIndex] = charSet;
            main.m_Project.CharsetProjects[charsetIndex].Filename = openFile.FileName;
            main.comboElementCharset.Items[charsetIndex] = shortName;
            listCharsets.Items[charsetIndex] = shortName;
            main.Modified = true;
            main.SetActiveElementCharset( main.m_Project.Charsets[main.comboElementCharset.SelectedIndex],
                                          main.m_Project.Charsets[main.comboElementCharset.SelectedIndex].MultiColor1,
                                          main.m_Project.Charsets[main.comboElementCharset.SelectedIndex].MultiColor2, 
                                          main.m_Project.CharsetProjects[main.comboElementCharset.SelectedIndex].Multicolor );
          }
        }
      }
    }



    private void btnRemoveCharset_Click( object sender, EventArgs e )
    {
      int charsetIndex = listCharsets.SelectedIndex;
      if ( charsetIndex != -1 )
      {
        main.m_Project.CharsetProjects.RemoveAt( charsetIndex );
        main.m_Project.Charsets.RemoveAt( charsetIndex );

        listCharsets.Items.RemoveAt( charsetIndex );
        main.Modified = true;
      }
    }

  }
}
