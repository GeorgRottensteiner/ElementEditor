using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ElementEditor
{
  static class Program
  {
    /// <summary>
    /// Der Haupteinstiegspunkt für die Anwendung.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetHighDpiMode(HighDpiMode.SystemAware);
      Application.SetCompatibleTextRenderingDefault( false );
      Application.Run( new FormMain() );
    }
  }
}
