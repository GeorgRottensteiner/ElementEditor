﻿using GR.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetroDevStudio.Types
{
  public class GraphicTile
  {
    public ColorSettings        Colors = new ColorSettings();
    public GraphicTileMode      Mode = GraphicTileMode.COMMODORE_HIRES;
    public int                  TransparentColorIndex = -1;
    public int                  Width = 8;
    public int                  Height = 8;
    public int                  CustomColor = -1;
    public ByteBuffer           Data = new ByteBuffer( 8 );
    public GR.Image.MemoryImage Image = new GR.Image.MemoryImage( 8, 8, System.Drawing.Imaging.PixelFormat.Format8bppIndexed );



    public GraphicTile()
    {
    }



    public GraphicTile( GraphicTile OtherTile )
    {
      Width                 = OtherTile.Width;
      Height                = OtherTile.Height;
      Mode                  = OtherTile.Mode;
      Colors                = OtherTile.Colors;
      CustomColor           = OtherTile.CustomColor;
      TransparentColorIndex = OtherTile.TransparentColorIndex;
      Data                  = new ByteBuffer( OtherTile.Data );
      Image                 = new GR.Image.MemoryImage( OtherTile.Image );
    }



    public GraphicTile( int Width, int Height, GraphicTileMode Mode, ColorSettings Color )
    {
      this.Width = Width;
      this.Height = Height;
      this.Mode = Mode;
      Colors = Color;

      Data  = new ByteBuffer( (uint)Lookup.NumBytes( Width, Height, Mode ) );
      Image = new GR.Image.MemoryImage( Width, Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed );
    }



    public bool SetPixel( int X, int Y, ColorType Color )
    {
      return SetPixel( X, Y, (int)Color );
    }



    public bool SetPixel( int X, int Y, int Color )
    {
      switch ( Mode )
      {
        case GraphicTileMode.COMMODORE_ECM:
        case GraphicTileMode.COMMODORE_HIRES:
          {
            int   byteValue = Data.ByteAt( Y );
            if ( ( byteValue & ( 1 << ( 7 - X ) ) ) == 0 )
            {
              if ( Color != 0 )
              {
                Data.SetU8At( Y, (byte)( byteValue | ( 1 << ( 7 - X ) ) ) );
                return true;
              }
            }
            else if ( Color == 0  )
            {
              Data.SetU8At( Y, (byte)( byteValue & ~( 1 << ( 7 - X ) ) ) );
              return true;
            }
          }
          break;
        case GraphicTileMode.COMMODORE_MULTICOLOR:
          if ( CustomColor >= 8 )
          {
            // mc mode
            X = X / 2;
            X = 3 - X;

            byte newByte = (byte)( Data.ByteAt( Y ) & ~( 3 << ( 2 * X ) ) );

            int     replacementBytes = 0;

            switch ( (ColorType)Color )
            {
              case ColorType.CUSTOM_COLOR:
                replacementBytes = 3;
                break;
              case ColorType.MULTICOLOR_1:
                replacementBytes = 1;
                break;
              case ColorType.MULTICOLOR_2:
                replacementBytes = 2;
                break;
            }
            newByte |= (byte)( replacementBytes << ( 2 * X ) );

            if ( Data.ByteAt( Y ) != newByte )
            {
              Data.SetU8At( Y, newByte );
              return true;
            }
            break;
          }
          goto case GraphicTileMode.COMMODORE_HIRES;
        case GraphicTileMode.MEGA65_FCM_16_COLORS:
          {
            int     bytePos = X / 2 + Y * ( ( Width + 1 ) / 2 );
            byte pixelValue = Data.ByteAt( bytePos );
            if ( ( X % 2 ) == 1 )
            {
              if ( ( pixelValue >> 4 ) != Color )
              {
                pixelValue &= 0x0f;
                pixelValue |= (byte)( Color << 4 );

                Data.SetU8At( bytePos, pixelValue );
                return true;
              }
            }
            else
            {
              if ( ( pixelValue & 0x0f ) != Color )
              {
                pixelValue &= 0xf0;
                pixelValue |= (byte)Color;

                Data.SetU8At( bytePos, pixelValue );
                return true;
              }
            }
            return false;
          }
        case GraphicTileMode.MEGA65_FCM_256_COLORS:
          if ( Data.ByteAt( X + Y * 8 ) != Color )
          {
            Data.SetU8At( X + Y * 8, (byte)Color );
            return true;
          }
          break;
        default:
          Debug.Log( "GraphicTile.SetPixel, unsupported mode " + Mode );
          break;
      }
      return false;
    }



    public int GetPixel( int X, int Y )
    {
      switch ( Mode )
      {
        case GraphicTileMode.COMMODORE_ECM:
        case GraphicTileMode.COMMODORE_HIRES:
          if ( ( Data.ByteAt( Y * ( ( Width + 7 ) / 8 ) ) & ( 1 << ( 7 - ( X % 8 ) ) ) ) != 0 )
          {
            return (int)ColorType.CUSTOM_COLOR;
          }
          return (int)ColorType.BACKGROUND;
        case GraphicTileMode.COMMODORE_MULTICOLOR:
          if ( CustomColor >= 8 )
          {
            // multi color
            X = X / 2;
            X = 3 - X;

            int   bitPattern = Data.ByteAt( Y ) & ( 3 << ( 2 * X ) );
            bitPattern >>= X * 2;

            switch ( bitPattern )
            {
              case 0x00:
                return (int)ColorType.BACKGROUND;
              case 0x01:
                return (int)ColorType.MULTICOLOR_1;
              case 0x02:
                return (int)ColorType.MULTICOLOR_2;
              case 0x03:
              default:
                return (int)ColorType.CUSTOM_COLOR;
            }
          }
          goto case GraphicTileMode.COMMODORE_HIRES;
        case GraphicTileMode.MEGA65_FCM_16_COLORS:
          if ( ( X % 2 ) == 0 )
          {
            return Data.ByteAt( X / 2 + Y * ( ( Width + 1 ) / 2 ) ) & 0x0f;
          }
          return Data.ByteAt( X / 2 + Y * ( ( Width + 1 ) / 2 ) ) >> 4;
        case GraphicTileMode.MEGA65_FCM_256_COLORS:
          return Data.ByteAt( X + Y * Width );
        default:
          Debug.Log( "GraphicTile.GetPixel, unsupported mode " + Mode );
          return 0;
      }
    }



    public int MapColor( uint Color )
    {
      return 0;
    }



    public int MapPixelColor( int X, int Y, GraphicTile TargetTile )
    {
      int     pixelValue = GetPixel( X, Y );
      if ( Mode == TargetTile.Mode )
      {
        return pixelValue;
      }
      // now things are getting funny
      uint  pixelColor = GetColorFromValue( pixelValue );

      var potentialColors = new List<uint>();
      switch ( TargetTile.Mode )
      {
        case GraphicTileMode.COMMODORE_HIRES:
          potentialColors.Add( Colors.Palette.ColorValues[Colors.BackgroundColor] );

          // TODO - variable color!
          potentialColors.Add( Colors.Palette.ColorValues[CustomColor] );
          break;
        case GraphicTileMode.COMMODORE_MULTICOLOR:
          potentialColors.Add( Colors.Palette.ColorValues[Colors.BackgroundColor] );
          potentialColors.Add( Colors.Palette.ColorValues[Colors.MultiColor1] );
          potentialColors.Add( Colors.Palette.ColorValues[Colors.MultiColor2] );

          // TODO - variable color!
          potentialColors.Add( Colors.Palette.ColorValues[CustomColor] );
          break;
        case GraphicTileMode.COMMODORE_ECM:
          potentialColors.Add( Colors.Palette.ColorValues[Colors.BackgroundColor] );
          potentialColors.Add( Colors.Palette.ColorValues[Colors.BGColor4] );

          // TODO - variable colors!
          potentialColors.Add( Colors.Palette.ColorValues[CustomColor] );
          break;
        case GraphicTileMode.MEGA65_FCM_16_COLORS:
        case GraphicTileMode.MEGA65_FCM_256_COLORS:
          potentialColors.AddRange( Colors.Palette.ColorValues );
          break;
      }

      int bestMatch = FindClosestEntryInPalette( pixelColor, potentialColors );

      return bestMatch;
    }



    private int FindClosestEntryInPalette( uint PixelColor, IEnumerable<uint> PotentialColors )
    {
      int   index = 0;
      foreach ( var color in PotentialColors )
      {
        if ( PixelColor == color )
        {
          return index;
        }
        ++index;
      }

      // TODO - find best match
      return (int)ColorType.BACKGROUND;
    }



    private uint GetColorFromValue( int PixelValue )
    {
      if ( Lookup.HasCustomPalette( Mode ) )
      {
        return Colors.Palette.ColorValues[PixelValue];
      }
      switch ( (ColorType)PixelValue )
      {
        case ColorType.BACKGROUND:
          return Colors.Palette.ColorValues[Colors.BackgroundColor];
        case ColorType.MULTICOLOR_1:
          return Colors.Palette.ColorValues[Colors.MultiColor1];
        case ColorType.MULTICOLOR_2:
          return Colors.Palette.ColorValues[Colors.MultiColor2];
        case ColorType.CUSTOM_COLOR:
          return Colors.Palette.ColorValues[CustomColor];
        case ColorType.BGCOLOR4:
          return Colors.Palette.ColorValues[Colors.BGColor4];
      }
      return 0;
    }



  }
}
