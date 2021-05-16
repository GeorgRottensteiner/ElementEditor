using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace C64Studio.Types
{
  public class FileChunk
  {
    public const System.UInt16    SOLUTION              = 0x0400;
    public const System.UInt16    SOLUTION_INFO         = 0x0401;
    public const System.UInt16    SOLUTION_PROJECT      = 0x0402;

    public const System.UInt16    PROJECT               = 0x1000;
    public const System.UInt16    PROJECT_ELEMENT       = 0x1001;
    public const System.UInt16    PROJECT_ELEMENT_DATA  = 0x1002;
    public const System.UInt16    PROJECT_ELEMENT_DISPLAY_DATA  = 0x1003;
    public const System.UInt16    PROJECT_ELEMENT_PER_CONFIG_SETTING = 0x1004;
    public const System.UInt16    PROJECT_CONFIG        = 0x1100;
    public const System.UInt16    PROJECT_WATCH_ENTRY   = 0x1101;

    public const System.UInt16    CHARSET_SCREEN_INFO   = 0x1200;
    public const System.UInt16    SCREEN_CHAR_DATA      = 0x1300;
    public const System.UInt16    SCREEN_COLOR_DATA     = 0x1301;
    public const System.UInt16    GRAPHIC_SCREEN_INFO   = 0x1310;
    public const System.UInt16    GRAPHIC_DATA          = 0x1311;   // uint width, uint height, uint image type, uint palette entry count, byte r,g,b, uint data size, data

    public const System.UInt16    MAP_PROJECT_INFO      = 0x1320;
    public const System.UInt16    MAP_PROJECT_DATA      = 0x1321;
    public const System.UInt16    MAP_TILE              = 0x1322;
    public const System.UInt16    MAP                   = 0x1324;
    public const System.UInt16    MAP_INFO              = 0x1325;
    public const System.UInt16    MAP_DATA              = 0x1326;
    public const System.UInt16    MAP_EXTRA_DATA        = 0x1327;
    public const System.UInt16    MAP_CHARSET           = 0x1328;
    public const System.UInt16    MAP_EXTRA_DATA_TEXT   = 0x1329;   // replaces MAP_EXTRA_DATA

    public const System.UInt16    SPRITESET_LAYER       = 0x1400;
    public const System.UInt16    SPRITESET_LAYER_ENTRY = 0x1401;

    public const System.UInt16    MULTICOLOR_DATA       = 0x1500;
    public const System.UInt16    CHARSET_DATA          = 0x1501;   // multicolor-data und binary data

    public const System.UInt16    SETTINGS_TOOL         = 0x2000;
    public const System.UInt16    SETTINGS_ACCELERATOR  = 0x2001;
    public const System.UInt16    SETTINGS_SOUND        = 0x2002;
    public const System.UInt16    SETTINGS_WINDOW       = 0x2003;
    public const System.UInt16    SETTINGS_TABS         = 0x2004;
    public const System.UInt16    SETTINGS_FONT         = 0x2005;
    public const System.UInt16    SETTINGS_SYNTAX_COLORING = 0x2006;
    public const System.UInt16    SETTINGS_UI           = 0x2007;
    public const System.UInt16    SETTINGS_DEFAULTS     = 0x2008;
    public const System.UInt16    SETTINGS_FIND_REPLACE = 0x2009;
    public const System.UInt16    SETTINGS_IGNORED_WARNINGS = 0x200A;
    public const System.UInt16    SETTINGS_LAYOUT       = 0x200B;   // do not use anymore!
    public const System.UInt16    SETTINGS_PANEL_DISPLAY_DETAILS = 0x200C;
    public const System.UInt16    SETTINGS_DPS_LAYOUT   = 0x200D;
  }



  public class ConstantData
  {
    public static System.Drawing.Color[]      m_Colors = new System.Drawing.Color[17];
    public static System.Drawing.Brush[]      m_ColorBrushes = new System.Drawing.Brush[17];
    public static uint[]                      m_ColorValues = new uint[17];



    static ConstantData()
    {
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
