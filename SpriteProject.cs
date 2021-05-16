using System;
using System.Collections.Generic;
using System.Text;

namespace C64Studio.Formats
{
  public class SpriteProject
  {
    public class SpriteData
    {
      public GR.Memory.ByteBuffer       Data = new GR.Memory.ByteBuffer( 63 );
      public bool                       Multicolor = false;
      public int                        Color = 1;
      public GR.Image.MemoryImage       Image = null;


      public SpriteData()
      {
        Image = new GR.Image.MemoryImage( 24, 21, System.Drawing.Imaging.PixelFormat.Format8bppIndexed );
      }


      public SpriteData Clone()
      {
        SpriteData copy = new SpriteData();

        copy.Multicolor = Multicolor;
        copy.Color = Color;
        copy.Data = new GR.Memory.ByteBuffer( Data );
        copy.Image = new GR.Image.MemoryImage( Image );
        return copy;
      }
    }



    public class LayerSprite
    {
      public int        X = 0;
      public int        Y = 0;
      public int        Color = 0;
      public int        Index = 0;
    }

    public class Layer
    {
      public List<LayerSprite> Sprites = new List<LayerSprite>();
    }



    public List<SpriteData>      Sprites = new List<SpriteData>( 256 );
    public List<Layer>           SpriteLayers = new List<Layer>();

    public int            BackgroundColor = 0;
    public int            MultiColor1 = 0;
    public int            MultiColor2 = 0;

    public string         Name = "";
    public string         ExportFilename = "";

    public uint           UsedSprites = 0;
    public int            StartIndex = 0;
    public int            NumSprites = 256;
    public bool           ShowGrid = false;



    public SpriteProject()
    {
      for ( int i = 0; i < 256; ++i )
      {
        Sprites.Add( new SpriteData() );

        for ( int j = 0; j < 16; ++j )
        {
          Sprites[i].Image.SetPaletteColor( j, (byte)( ( Types.ConstantData.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( Types.ConstantData.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( Types.ConstantData.m_ColorValues[j] & 0xff ) );
        }
      }
    }



    public GR.Memory.ByteBuffer SaveToBuffer()
    {
      GR.Memory.ByteBuffer projectFile = new GR.Memory.ByteBuffer();

      // version
      projectFile.AppendU32( 1 );
      projectFile.AppendI32( Sprites.Count );
      // Name
      projectFile.AppendString( Name );
      for ( int i = 0; i < Sprites.Count; ++i )
      {
        projectFile.AppendI32( Sprites[i].Color );
      }
      for ( int i = 0; i < Sprites.Count; ++i )
      {
        projectFile.AppendU8( Sprites[i].Multicolor ? (byte)1 : (byte)0 );
      }
      projectFile.AppendI32( BackgroundColor );
      projectFile.AppendI32( MultiColor1 );
      projectFile.AppendI32( MultiColor2 );
      // generic MC
      projectFile.AppendU32( 0 );
      for ( int i = 0; i < Sprites.Count; ++i )
      {
        projectFile.Append( Sprites[i].Data );
        projectFile.AppendU8( (byte)Sprites[i].Color );
      }
      projectFile.AppendU32( UsedSprites );

      // export name
      projectFile.AppendString( ExportFilename );

      // exportpath
      projectFile.AppendString( "" );

      // desc
      for ( int i = 0; i < Sprites.Count; ++i )
      {
        projectFile.AppendString( "" );
      }

      // testbed
      projectFile.AppendI32( 0 );


      foreach ( var layer in SpriteLayers )
      {
        GR.IO.FileChunk   chunkLayer = new GR.IO.FileChunk( Types.FileChunk.SPRITESET_LAYER );

        foreach ( var sprite in layer.Sprites )
        {
          GR.IO.FileChunk   chunkLayerSprite = new GR.IO.FileChunk( Types.FileChunk.SPRITESET_LAYER_ENTRY );
          chunkLayerSprite.AppendI32( sprite.Index );
          chunkLayerSprite.AppendU8( (byte)sprite.Color );
          chunkLayerSprite.AppendI32( sprite.X );
          chunkLayerSprite.AppendI32( sprite.Y );

          chunkLayer.Append( chunkLayerSprite.ToBuffer() );
        }
        projectFile.Append( chunkLayer.ToBuffer() );
      }
      /*
      int spriteTestCount = memIn.ReadInt32();
      for ( int i = 0; i < spriteTestCount; ++i )
      {
        int spriteIndex = memIn.ReadInt32();
        byte spriteColor = memIn.ReadUInt8();
        bool spriteMultiColor = ( memIn.ReadUInt8() != 0 );
        int spriteX = memIn.ReadInt32();
        int spriteY = memIn.ReadInt32();
      }*/
      return projectFile;
    }



    public bool ReadFromBuffer( GR.Memory.ByteBuffer DataIn )
    {
      if ( DataIn == null )
      {
        return false;
      }
      SpriteLayers.Clear();

      GR.IO.MemoryReader memIn = DataIn.MemoryReader();

      uint     Version = memIn.ReadUInt32();
      int       numSprites = 256;
      if ( Version >= 1 )
      {
        numSprites = memIn.ReadInt32();
      }
      Sprites = new List<SpriteData>();
      for ( int i = 0; i < numSprites; ++i )
      {
        Sprites.Add( new SpriteData() );
        for ( int j = 0; j < 16; ++j )
        {
          Sprites[i].Image.SetPaletteColor( j, (byte)( ( Types.ConstantData.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( Types.ConstantData.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( Types.ConstantData.m_ColorValues[j] & 0xff ) );
        }
      }

      string name = memIn.ReadString();
      for ( int i = 0; i < numSprites; ++i )
      {
        Sprites[i].Color = memIn.ReadInt32();
      }
      for ( int i = 0; i < numSprites; ++i )
      {
        Sprites[i].Multicolor = ( memIn.ReadUInt8() != 0 );
      }
      BackgroundColor = memIn.ReadInt32();
      MultiColor1 = memIn.ReadInt32();
      MultiColor2 = memIn.ReadInt32();

      bool genericMultiColor = ( memIn.ReadUInt32() != 0 );
      for ( int i = 0; i < numSprites; ++i )
      {
        GR.Memory.ByteBuffer tempBuffer = new GR.Memory.ByteBuffer();

        memIn.ReadBlock( tempBuffer, 64 );
        tempBuffer.CopyTo( Sprites[i].Data, 0, 63 );
      }

      UsedSprites = memIn.ReadUInt32();

      ExportFilename = memIn.ReadString();
      string exportPathSpriteFile = memIn.ReadString();
      for ( int i = 0; i < numSprites; ++i )
      {
        string desc = memIn.ReadString();
      }
      int     spriteTestCount = memIn.ReadInt32();
      for ( int i = 0; i < spriteTestCount; ++i )
      {
        int spriteIndex = memIn.ReadInt32();
        byte spriteColor = memIn.ReadUInt8();
        bool spriteMultiColor = ( memIn.ReadUInt8() != 0 );
        int spriteX = memIn.ReadInt32();
        int spriteY = memIn.ReadInt32();
      }

      GR.IO.FileChunk   chunk = new GR.IO.FileChunk();

      while ( chunk.ReadFromStream( memIn ) )
      {
        switch ( chunk.Type )
        {
          case Types.FileChunk.SPRITESET_LAYER:
            {
              Layer  layer = new Layer();

              SpriteLayers.Add( layer );

              var    chunkReader = chunk.MemoryReader();

              GR.IO.FileChunk   subChunk = new GR.IO.FileChunk();

              while ( subChunk.ReadFromStream( chunkReader ) )
              {
                var    subChunkReader = subChunk.MemoryReader();

                if ( subChunk.Type == Types.FileChunk.SPRITESET_LAYER_ENTRY )
                {
                  LayerSprite sprite = new LayerSprite();

                  sprite.Index = subChunkReader.ReadInt32();
                  sprite.Color = subChunkReader.ReadUInt8();
                  sprite.X = subChunkReader.ReadInt32();
                  sprite.Y = subChunkReader.ReadInt32();

                  layer.Sprites.Add( sprite );
                }
              }
            }
            break;
        }
      }
      return true;
    }

  }
}
