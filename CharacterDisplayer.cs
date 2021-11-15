using RetroDevStudio;
using System;
using System.Collections.Generic;
using System.Text;



namespace C64Studio.Displayer
{
  public class CharacterDisplayer
  {
    public static void DisplayHiResChar( GR.Memory.ByteBuffer Data, Palette Palette, int BGColor, int CharColor, GR.Image.IImage TargetImage, int X, int Y )
    {
      // single color
      int colorIndex = 0;
      for ( int j = 0; j < 8; ++j )
      {
        for ( int i = 0; i < 8; ++i )
        {
          if ( ( Data.ByteAt( j ) & ( 1 << ( 7 - i ) ) ) != 0 )
          {
            colorIndex = CharColor;
          }
          else
          {
            colorIndex = BGColor;
          }
          uint color = Palette.ColorValues[colorIndex];
          TargetImage.SetPixel( X + i, Y + j, color );
        }
      }
    }



    public static void DisplayMultiColorChar( GR.Memory.ByteBuffer Data, Palette Palette, int BGColor, int MColor1, int MColor2, int CharColor, GR.Image.IImage TargetImage, int X, int Y )
    {
      // multicolor
      if ( CharColor < 8 )
      {
        DisplayHiResChar( Data, Palette, BGColor, CharColor, TargetImage, X, Y );
        return;
      }

      int charColor = CharColor - 8;

      for ( int j = 0; j < 8; ++j )
      {
        for ( int i = 0; i < 4; ++i )
        {
          int pixelValue = ( Data.ByteAt( j ) & ( 3 << ( ( 3 - i ) * 2 ) ) ) >> ( ( 3 - i ) * 2 );

          switch ( pixelValue )
          {
            case 0:
              pixelValue = BGColor;
              break;
            case 1:
              pixelValue = MColor1;
              break;
            case 2:
              pixelValue = MColor2;
              break;
            case 3:
              pixelValue = charColor;
              break;
          }
          uint color = Palette.ColorValues[pixelValue];
          TargetImage.SetPixel( X + i * 2, Y + j, color );
          TargetImage.SetPixel( X + i * 2 + 1, Y + j, color );
        }
      }
    }



    public static void DisplayVC20Char( GR.Memory.ByteBuffer Data, Palette Palette, int BGColor, int MColor1, int MColor2, int CharColor, GR.Image.IImage TargetImage, int X, int Y )
    {
      // multicolor
      if ( CharColor < 8 )
      {
        DisplayHiResChar( Data, Palette, BGColor, CharColor, TargetImage, X, Y );
        return;
      }

      int charColor = CharColor - 8;

      for ( int j = 0; j < 8; ++j )
      {
        for ( int i = 0; i < 4; ++i )
        {
          int pixelValue = ( Data.ByteAt( j ) & ( 3 << ( ( 3 - i ) * 2 ) ) ) >> ( ( 3 - i ) * 2 );

          switch ( pixelValue )
          {
            case 0:
              pixelValue = BGColor;
              break;
            case 1:
              // border color(!)
              pixelValue = MColor1;
              break;
            case 2:
              pixelValue = charColor;
              break;
            case 3:
              pixelValue = MColor2;
              break;
          }
          uint  color = Palette.ColorValues[pixelValue];

          TargetImage.SetPixel( X + i * 2, Y + j, color );
          TargetImage.SetPixel( X + i * 2 + 1, Y + j, color );
        }
      }
    }



    public static void DisplayMega65FCMChar( GR.Memory.ByteBuffer Data, Palette Palette, int BGColor, int CharColor, GR.Image.IImage TargetImage, int X, int Y )
    {
      for ( int j = 0; j < 8; ++j )
      {
        for ( int i = 0; i < 8; ++i )
        {
          int colorIndex = Data.ByteAt( i + j * 8 );
          uint color = Palette.ColorValues[colorIndex];
          TargetImage.SetPixel( X + i, Y + j, color );
        }
      }
    }



    public static void DisplayChar( Formats.CharsetProject Charset, int CharIndex, GR.Image.IImage TargetImage, int X, int Y )
    {
      Formats.CharData Char = Charset.Characters[CharIndex];

      DisplayChar( Charset, CharIndex, TargetImage, X, Y, Char.Tile.CustomColor );
    }



    public static void DisplayChar( Formats.CharsetProject Charset, int CharIndex, GR.Image.IImage TargetImage, int X, int Y, int AlternativeColor )
    {
      DisplayChar( Charset, CharIndex, TargetImage, X, Y, AlternativeColor, Charset.Colors.BackgroundColor, Charset.Colors.MultiColor1, Charset.Colors.MultiColor2, Charset.Colors.BGColor4 );
    }




    public static void DisplayChar( Formats.CharsetProject Charset, int CharIndex, GR.Image.IImage TargetImage, int X, int Y, int AlternativeColor, int AltBGColor, int AltMColor1, int AltMColor2, int AltBGColor4 )
    {
      Formats.CharData Char = Charset.Characters[CharIndex];

      DisplayChar( Charset, CharIndex, TargetImage, X, Y, AlternativeColor, AltBGColor, AltMColor1, AltMColor2, AltBGColor4, Charset.Mode );
    }



    public static void DisplayChar( Formats.CharsetProject Charset, int CharIndex, GR.Image.IImage TargetImage, int X, int Y, int AlternativeColor, int AltBGColor, int AltMColor1, int AltMColor2, int AltBGColor4, TextCharMode AlternativeMode )
    {
      Formats.CharData Char = Charset.Characters[CharIndex];

      if ( AlternativeMode == TextCharMode.COMMODORE_ECM )
      {
        // ECM
        Formats.CharData origChar = Charset.Characters[CharIndex % 64];

        int bgColor = AltBGColor;
        switch ( CharIndex / 64 )
        {
          case 1:
            bgColor = AltMColor1;
            break;
          case 2:
            bgColor = AltMColor2;
            break;
          case 3:
            bgColor = AltBGColor4;
            break;
        }
        DisplayHiResChar( origChar.Tile.Data, Charset.Colors.Palette, bgColor, AlternativeColor, TargetImage, X, Y );
      }
      else if ( AlternativeMode == TextCharMode.COMMODORE_MULTICOLOR )
      {
        DisplayMultiColorChar( Char.Tile.Data, Charset.Colors.Palette, AltBGColor, AltMColor1, AltMColor2, AlternativeColor, TargetImage, X, Y );
      }
      else if ( AlternativeMode == TextCharMode.COMMODORE_HIRES )
      {
        DisplayHiResChar( Char.Tile.Data, Charset.Colors.Palette, AltBGColor, AlternativeColor, TargetImage, X, Y );
      }
      else if ( ( AlternativeMode == TextCharMode.MEGA65_FCM )
      ||        ( AlternativeMode == TextCharMode.MEGA65_FCM_16BIT ) )
      {
        DisplayMega65FCMChar( Char.Tile.Data, Charset.Colors.Palette, AltBGColor, AlternativeColor, TargetImage, X, Y );
      }
      else if ( AlternativeMode == TextCharMode.VIC20 )
      {
        DisplayVC20Char( Char.Tile.Data, Charset.Colors.Palette, AltBGColor, AltMColor1, AltMColor2, AlternativeColor, TargetImage, X, Y );
      }
      else
      {
        Debug.Log( "DisplayChar #2 unsupported mode " + AlternativeMode );
      }
    }

  }
}
