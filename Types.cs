using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

public class CharsetProjectInfo
{
  public string   Filename = "";
  public bool     Multicolor = true;
};



namespace RetroDevStudio
{
  public partial class ConstantData
  {
    public static System.Drawing.Color[]      m_Colors = new System.Drawing.Color[17];
    public static System.Drawing.Brush[]      m_ColorBrushes = new System.Drawing.Brush[17];
    public static uint[]                      m_ColorValues = new uint[17];

    public static Palette                     Palette;



    static ConstantData()
    {
      Palette = RetroDevStudio.ConstantData.PaletteC64();

      m_ColorValues[0] = 0xff000000;
      m_ColorValues[1] = 0xffffffff;
      m_ColorValues[2] = 0xff8B4131;
      m_ColorValues[3] = 0xff7BBDC5;
      m_ColorValues[4] = 0xff8B41AC;
      m_ColorValues[5] = 0xff6AAC41;
      m_ColorValues[6] = 0xff3931A4;
      m_ColorValues[7] = 0xffD5DE73;
      m_ColorValues[8] = 0xff945A20;
      m_ColorValues[9] = 0xff5A4100;
      m_ColorValues[10] = 0xffBD736A;
      m_ColorValues[11] = 0xff525252;
      m_ColorValues[12] = 0xff838383;
      m_ColorValues[13] = 0xffACEE8B;
      m_ColorValues[14] = 0xff7B73DE;
      m_ColorValues[15] = 0xffACACAC;
      m_ColorValues[16] = 0xff80ff80;
      for ( int i = 0; i < 16; ++i )
      {
        m_Colors[i] = GR.Color.Helper.FromARGB( m_ColorValues[i] );
        m_ColorBrushes[i] = new System.Drawing.SolidBrush( m_Colors[i] );
      }

    }
  }
}
