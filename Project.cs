using RetroDevStudio.Formats;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElementEditor
{
  public class Project
  {
    [Flags]
    public enum ChunkType
    {
      GENERAL           = 0,
      ELEMENT           = 1,
      OLD_SCREEN        = 2,
      ELEMENT_OBJECT_DATA = 3,
      ELEMENT_SPAWN_SPOT_DATA = 4,
      OLD_SCREEN_MORE_DATA = 5,
      CHARSET_INFO      = 6,
      OBJECT_TEMPLATE   = 10,
      SCREEN            = 0x2000,
      SCREEN_INFO       = 0x2001,
      SCREEN_ELEMENT    = 0x2002,
      MAP_REGION        = 0x3000,
      MAP_REGION_INFO   = 0x3001,
      MAP_REGION_SCREEN = 0x3002
    };

    public  uint[]                      m_ColorValues = new uint[16];
    private System.Drawing.Color[]      m_Colors = new System.Drawing.Color[16];

    public class Character
    {
      public byte    Char = 0;
      public byte    Color = 0;

      public Character()
      {
        Char = 0;
        Color = 0;
      }



      public Character( byte CharValue, byte Color )
      {
        Char = CharValue;
        this.Color = Color;
      }

      public virtual object Clone()
      {
        return new Character( Char, Color );
      }
    }

    public class Element
    {
      public GR.Game.Layer<Character>       Characters = new GR.Game.Layer<Character>();
      public string                         Name = "";
      public int                            Index = 0;
      public int                            X = 0;
      public int                            Y = 0;
      public int                            CharsetIndex = 0;

      public Element()
      {
        Characters.InvalidTile = new Character();
      }
    }

    public enum ScreenElementType
    {
      LD_ELEMENT,
      LD_ELEMENT_LINE_H,
      LD_ELEMENT_LINE_V,
      LD_LINE_H,
      LD_LINE_V,
      LD_SEARCH_OBJECT,
      LD_LINE_H_ALT,
      LD_LINE_V_ALT,
      LD_AREA,
      LD_OBJECT,
      LD_SPAWN_SPOT,
      LD_ELEMENT_AREA,
      LD_DOOR,
      LD_CLUE,
      LD_SPECIAL
    }

    public class ScreenElement
    {
      public int    Index = 0;
      public int    X = 0;
      public int    Y = 0;
      public int    SearchObjectIndex = 0;
      public int    Repeats = 0;
      public int    Repeats2 = 0;
      public int    Char = 0;
      public int    Color = 0;
      public ScreenElementType    Type = ScreenElementType.LD_ELEMENT;
      public GameObject Object = null;
      public int    TargetX = 0;
      public int    TargetY = 0;
      public int    TargetLevel = 0;
      public int    Flags = 0;


      public ScreenElement()
      {
      }



      public ScreenElement( ScreenElement OtherElement )
      {
        Index = OtherElement.Index;
        X = OtherElement.X;
        Y = OtherElement.Y;
        SearchObjectIndex = OtherElement.SearchObjectIndex;
        Repeats = OtherElement.Repeats;
        Repeats2 = OtherElement.Repeats2;
        Char = OtherElement.Char;
        Color = OtherElement.Color;
        Type = OtherElement.Type;
        TargetX = OtherElement.TargetX;
        TargetY = OtherElement.TargetY;
        TargetLevel = OtherElement.TargetLevel;
        Flags = OtherElement.Flags;

        if ( OtherElement.Object != null )
        {
          Object = new GameObject( OtherElement.Object );
        }
      }
    }

    public class Behaviour
    {
      public int    Value = 0;
      public string Name = "";
      public override string ToString()
      {
        return Name;
      }
    };

    public class ObjectTemplate
    {
      public int    Index = 0;
      public int    StartSprite = 0;
      public string Name = "";
      public GR.Collections.Map<int, Behaviour> Behaviours = new GR.Collections.Map<int, Behaviour>();

      public override string ToString()
      {
        return Name;
      }
        
    };

    public class GameObject
    {
      public enum OptionalType
      {
        ALWAYS_SHOWN = 0,
        HIDDEN_IF_OPTIONAL_SET,
        SHOWN_IF_OPTIONAL_SET
      };

      public int    TemplateIndex = -1;
      public int    X = 0;
      public int    Y = 0;
      public int    Color = 1;
      public int    Speed = 1;
      public int    MoveBorderLeft = 0;
      public int    MoveBorderTop = 0;
      public int    MoveBorderRight = 0;
      public int    MoveBorderBottom = 0;
      public int    Behaviour = 0;
      public int    Data = 0;
      public int    OptionalValue = 0;
      public OptionalType   Optional = OptionalType.ALWAYS_SHOWN;
      public GR.Image.MemoryImage     SpriteImage = null;


      public GameObject()
      {
      }



      public GameObject( GameObject OtherObject )
      {
        TemplateIndex = OtherObject.TemplateIndex;
        X = OtherObject.X;
        Y = OtherObject.Y;
        Color = OtherObject.Color;
        Speed = OtherObject.Speed;
        MoveBorderLeft = OtherObject.MoveBorderLeft;
        MoveBorderRight = OtherObject.MoveBorderRight;
        MoveBorderBottom = OtherObject.MoveBorderBottom;
        MoveBorderTop = OtherObject.MoveBorderTop;
        OptionalValue = OtherObject.OptionalValue;
        Optional = OtherObject.Optional;

        SpriteImage = new GR.Image.MemoryImage( OtherObject.SpriteImage );
      }
    };

    public class Screen
    {
      public List<ScreenElement>    DisplayedElements = new List<ScreenElement>();
      public string             Name = "";
      public string             ExtraData = "";
      public int                Width = 40;
      public int                Height = 25;
      public byte               ConfigByte = 0;
      public byte               WLConfigByte = 0;
      public int                CharsetIndex = 0;
      public int                OverrideMC1 = -1;
      public int                OverrideMC2 = -1;



      public Screen()
      {
      }



      public Screen( Screen OtherScreen )
      {
        Name          = OtherScreen.Name;
        Width         = OtherScreen.Width;
        Height        = OtherScreen.Height;
        ConfigByte    = OtherScreen.ConfigByte;
        WLConfigByte  = OtherScreen.WLConfigByte;
        CharsetIndex  = OtherScreen.CharsetIndex;
        OverrideMC1   = OtherScreen.OverrideMC1;
        OverrideMC2   = OtherScreen.OverrideMC2;

        foreach ( ScreenElement element in OtherScreen.DisplayedElements )
        {
          ScreenElement newElement = new ScreenElement( element );
          DisplayedElements.Add( newElement );
        }
      }



      public override string ToString()
      {
        return Name;
      }
    }



    public class RegionScreenInfo
    {
      public int  ScreenIndex = -1;
      public bool ExitN = false;
      public bool ExitS = false;
      public bool ExitW = false;
      public bool ExitE = false;
    }

    public class Region
    {
      public int          DisplayX = 0;
      public int          DisplayY = 0;
      public List<RegionScreenInfo> Screens = new List<RegionScreenInfo>();
      public bool         Vertical = false;
      public GR.Memory.ByteBuffer   ExtraData = new GR.Memory.ByteBuffer();
    }



    public List<CharsetProject>         Charsets = new List<CharsetProject>();
    public string               OldCharsetProjectFilename = "";
    public string               SpriteProjectFilename = "";
    public string               ExportFilename = "";
    public int                  ExportConstantOffset = 0;
    public string               ExportPrefix = "";
    public List<Element>        Elements = new List<Element>();
    public List<List<GR.Image.MemoryImage>>   CharacterImages = new List<List<GR.Image.MemoryImage>>();
    public List<Screen>         Screens = new List<Screen>();
    public List<ObjectTemplate> ObjectTemplates = new List<ObjectTemplate>();
    public string               ProjectType = "Soulless";
    public List<CharsetProjectInfo> CharsetProjects = new List<CharsetProjectInfo>();
    public byte                 EmptyChar = 0;
    public byte                 EmptyColor = 0;
    public List<Region>         Regions = new List<Region>();



    public Project()
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

      m_Colors[0] = GR.Color.Helper.FromARGB( 0xff000000 );
      m_Colors[1] = GR.Color.Helper.FromARGB( 0xffffffff );
      m_Colors[2] = GR.Color.Helper.FromARGB( 0xff8B4131 );
      m_Colors[3] = GR.Color.Helper.FromARGB( 0xff7BBDC5 );
      m_Colors[4] = GR.Color.Helper.FromARGB( 0xff8B41AC );
      m_Colors[5] = GR.Color.Helper.FromARGB( 0xff6AAC41 );
      m_Colors[6] = GR.Color.Helper.FromARGB( 0xff3931A4 );
      m_Colors[7] = GR.Color.Helper.FromARGB( 0xffD5DE73 );
      m_Colors[8] = GR.Color.Helper.FromARGB( 0xff945A20 );
      m_Colors[9] = GR.Color.Helper.FromARGB( 0xff5A4100 );
      m_Colors[10] = GR.Color.Helper.FromARGB( 0xffBD736A );
      m_Colors[11] = GR.Color.Helper.FromARGB( 0xff525252 );
      m_Colors[12] = GR.Color.Helper.FromARGB( 0xff838383 );
      m_Colors[13] = GR.Color.Helper.FromARGB( 0xffACEE8B );
      m_Colors[14] = GR.Color.Helper.FromARGB( 0xff7B73DE );
      m_Colors[15] = GR.Color.Helper.FromARGB( 0xffACACAC );

      /*
      for ( int i = 0; i < 256; ++i )
      {
        List<GR.Image.MemoryImage>    charactersImages = new List<GR.Image.MemoryImage>();
        for ( int j = 0; j < 16; ++j )
        {
          GR.Image.MemoryImage  charForColor = new GR.Image.MemoryImage( 8, 8, System.Drawing.Imaging.PixelFormat.Format8bppIndexed );

          charactersImages.Add( charForColor );
        }

        CharacterImages.Add( charactersImages );
      }*/
    }



    public bool LoadFromFile( string Filename )
    {
      Elements.Clear();
      ObjectTemplates.Clear();
      this.Screens.Clear();
      Regions.Clear();

      GR.Memory.ByteBuffer    editorFile = GR.IO.File.ReadAllBytes( Filename );
      if ( editorFile == null )
      {
        return false;
      }

      GR.IO.MemoryReader    memIn = editorFile.MemoryReader();

      GR.IO.FileChunk   chunk = new GR.IO.FileChunk();

      while ( chunk.ReadFromStream( memIn ) )
      {
        GR.IO.MemoryReader    memReader = chunk.MemoryReader();
        switch ( chunk.Type )
        {
          case (ushort)ChunkType.GENERAL:
            // general
            {
              OldCharsetProjectFilename = memReader.ReadString();
              ExportFilename = memReader.ReadString();
              ExportPrefix = memReader.ReadString();
              ExportConstantOffset = memReader.ReadInt32();
              SpriteProjectFilename = memReader.ReadString();
              ProjectType = memReader.ReadString();
              EmptyChar = memReader.ReadUInt8();
              EmptyColor = memReader.ReadUInt8();
            }
            break;
          case (ushort)ChunkType.CHARSET_INFO:
            {
              CharsetProjectInfo info = new CharsetProjectInfo();

              info.Filename = memReader.ReadString();
              info.Multicolor = ( memReader.ReadUInt8() == 1 );
              CharsetProjects.Add( info );
            }
            break;
          case (ushort)ChunkType.ELEMENT:
            // element
            {
              Element   element = new Element();

              element.Name = memReader.ReadString();
              element.Index = memReader.ReadInt32();

              int w = memReader.ReadInt32();
              int h = memReader.ReadInt32();

              element.Characters.Resize( w, h );
              for ( int i = 0; i < w; ++i )
              {
                for ( int j = 0; j < h; ++j )
                {
                  element.Characters[i, j].Char = memReader.ReadUInt8();
                  element.Characters[i, j].Color = memReader.ReadUInt8();
                }
              }
              element.CharsetIndex = memReader.ReadInt32();
              //element.CharsetIndex = 0;
              if ( ( CharsetProjects.Count == 0 )
              &&   ( element.CharsetIndex != 0 ) )
              {
                element.CharsetIndex = 0;
              }
              else if ( element.CharsetIndex >= CharsetProjects.Count )
              {
                element.CharsetIndex = 0;
              }
              Elements.Add( element );
            }
            break;
          case (ushort)ChunkType.OLD_SCREEN:
            // screen
            {
              Screen screen = new Screen();

              screen.Name = memReader.ReadString();
              int    numElements = memReader.ReadInt32();

              for ( int i = 0; i < numElements; ++i )
              {
                ScreenElement element = new ScreenElement();
                element.Type = (ScreenElementType)memReader.ReadInt32();
                int     elementIndex = memReader.ReadInt32();
                if ( elementIndex >= Elements.Count )
                {
                  elementIndex = 0;
                }

                element.Index = elementIndex;
                element.X = memReader.ReadInt32();
                element.Y = memReader.ReadInt32();
                element.Repeats = memReader.ReadInt32();
                element.Char = memReader.ReadInt32();
                element.Color = memReader.ReadInt32();
                element.SearchObjectIndex = memReader.ReadInt32();

                screen.DisplayedElements.Add( element );
              }
              // fetch extra object data
              foreach ( ScreenElement element in screen.DisplayedElements )
              {
                if ( element.Type == ScreenElementType.LD_OBJECT )
                {
                  GR.IO.FileChunk chunkObject = new GR.IO.FileChunk();
                  if ( !chunkObject.ReadFromStream( memReader ) )
                  {
                    return false;
                  }
                  element.Object = new GameObject();

                  GR.IO.MemoryReader memObjectReader = chunkObject.MemoryReader();

                  element.Object.TemplateIndex      = memObjectReader.ReadInt32();
                  element.Object.X                  = memObjectReader.ReadInt32();
                  element.Object.Y                  = memObjectReader.ReadInt32();
                  element.Object.Color              = memObjectReader.ReadInt32();
                  element.Object.Speed              = memObjectReader.ReadInt32();
                  element.Object.Behaviour          = memObjectReader.ReadInt32();
                  element.Object.MoveBorderLeft     = memObjectReader.ReadInt32();
                  element.Object.MoveBorderTop      = memObjectReader.ReadInt32();
                  element.Object.MoveBorderRight    = memObjectReader.ReadInt32();
                  element.Object.MoveBorderBottom   = memObjectReader.ReadInt32();
                  element.Object.Data               = memObjectReader.ReadInt32();
                }
                else if ( element.Type == ScreenElementType.LD_SPAWN_SPOT )
                {
                  GR.IO.FileChunk chunkSpawn = new GR.IO.FileChunk();
                  if ( !chunkSpawn.ReadFromStream( memReader ) )
                  {
                    return false;
                  }
                  element.Object = new GameObject();

                  GR.IO.MemoryReader memObjectReader = chunkSpawn.MemoryReader();

                  element.Object.TemplateIndex = memObjectReader.ReadInt32();
                  element.Object.X = memObjectReader.ReadInt32();
                  element.Object.Y = memObjectReader.ReadInt32();
                  element.Repeats = memObjectReader.ReadInt32();
                }
              }
              GR.IO.FileChunk chunkMoreData = new GR.IO.FileChunk();
              while ( chunkMoreData.ReadFromStream( memReader ) )
              {
                GR.IO.MemoryReader moreDataReader = chunkMoreData.MemoryReader();

                switch ( chunkMoreData.Type )
                {
                  case (ushort)ChunkType.OLD_SCREEN_MORE_DATA:
                    screen.Width = moreDataReader.ReadInt32();
                    break;
                }
              }

              Screens.Add( screen );
            }
            break;
          case (ushort)ChunkType.MAP_REGION:
            {
              Region region = new Region();

              GR.IO.FileChunk subChunk = new GR.IO.FileChunk();

              while ( subChunk.ReadFromStream( memReader ) )
              {
                GR.IO.MemoryReader subReader = subChunk.MemoryReader();
                switch ( subChunk.Type )
                {
                  case (ushort)ChunkType.MAP_REGION_INFO:
                    region.DisplayX = subReader.ReadInt32();
                    region.DisplayY = subReader.ReadInt32();
                    region.Vertical = ( subReader.ReadInt32() != 0 );
                    {
                      uint numExtraDataBytes = subReader.ReadUInt32();
                      region.ExtraData = new GR.Memory.ByteBuffer();
                      subReader.ReadBlock( region.ExtraData, numExtraDataBytes );
                    }
                    break;
                  case (ushort)ChunkType.MAP_REGION_SCREEN:
                    {
                      RegionScreenInfo screenInfo = new RegionScreenInfo();

                      screenInfo.ScreenIndex = subReader.ReadInt32();
                      screenInfo.ExitN = ( subReader.ReadInt32() != 0 );
                      screenInfo.ExitS = ( subReader.ReadInt32() != 0 );
                      screenInfo.ExitW = ( subReader.ReadInt32() != 0 );
                      screenInfo.ExitE = ( subReader.ReadInt32() != 0 );

                      region.Screens.Add( screenInfo );
                    }
                    break;
                }
              }
              Regions.Add( region );
            }
            break;
          case (ushort)ChunkType.SCREEN:
            // screen
            {
              Screen screen = new Screen();

              GR.IO.FileChunk subChunk = new GR.IO.FileChunk();

              while ( subChunk.ReadFromStream( memReader ) )
              {
                GR.IO.MemoryReader subReader = subChunk.MemoryReader();
                switch ( subChunk.Type )
                {
                  case (ushort)ChunkType.SCREEN_INFO:
                    screen.Name = subReader.ReadString();
                    screen.Width = subReader.ReadInt32();
                    screen.Height = subReader.ReadInt32();
                    if ( screen.Height == 0 )
                    {
                      screen.Height = 25;
                    }
                    screen.ConfigByte   = subReader.ReadUInt8();
                    screen.CharsetIndex = subReader.ReadInt32();
                    screen.WLConfigByte = subReader.ReadUInt8();
                    screen.ExtraData    = subReader.ReadString();
                    screen.OverrideMC1  = subReader.ReadInt32() - 1;
                    screen.OverrideMC2  = subReader.ReadInt32() - 1;
                    break;
                  case (ushort)ChunkType.SCREEN_ELEMENT:
                    {
                      ScreenElement element = new ScreenElement();
                      element.Type = (ScreenElementType)subReader.ReadInt32();
                      int elementIndex = subReader.ReadInt32();
                      if ( elementIndex >= Elements.Count )
                      {
                        elementIndex = 0;
                      }
                      element.Index = elementIndex;
                      element.X = subReader.ReadInt32();
                      element.Y = subReader.ReadInt32();
                      element.Repeats = subReader.ReadInt32();
                      element.Repeats2 = subReader.ReadInt32();
                      element.Char = subReader.ReadInt32();
                      element.Color = subReader.ReadInt32();
                      element.SearchObjectIndex = subReader.ReadInt32();
                      element.TargetX = subReader.ReadInt32();
                      element.TargetY = subReader.ReadInt32();
                      element.TargetLevel = subReader.ReadInt32();
                      element.Flags = subReader.ReadInt32();

                      if ( ( element.Type == ScreenElementType.LD_OBJECT )
                      ||   ( element.Type == ScreenElementType.LD_SPAWN_SPOT ) )
                      {
                        element.Object = new GameObject();
                      }

                      screen.DisplayedElements.Add( element );
                    }
                    break;
                  case (ushort)ChunkType.ELEMENT_OBJECT_DATA:
                    {
                      ScreenElement screenElement = screen.DisplayedElements[subReader.ReadInt32()];
                      if ( screenElement.Type == ScreenElementType.LD_OBJECT )
                      {
                        screenElement.Object.TemplateIndex = subReader.ReadInt32();
                        screenElement.Object.X = subReader.ReadInt32();
                        screenElement.Object.Y = subReader.ReadInt32();
                        screenElement.Object.Color = subReader.ReadInt32();
                        screenElement.Object.Speed = subReader.ReadInt32();
                        screenElement.Object.Behaviour = subReader.ReadInt32();
                        screenElement.Object.MoveBorderLeft = subReader.ReadInt32();
                        screenElement.Object.MoveBorderTop = subReader.ReadInt32();
                        screenElement.Object.MoveBorderRight = subReader.ReadInt32();
                        screenElement.Object.MoveBorderBottom = subReader.ReadInt32();
                        screenElement.Object.Data = subReader.ReadInt32();
                        screenElement.Object.OptionalValue = subReader.ReadInt32();
                        screenElement.Object.Optional = (GameObject.OptionalType)subReader.ReadInt32();
                      }
                    }
                    break;
                  case (ushort)ChunkType.ELEMENT_SPAWN_SPOT_DATA:
                    {
                      ScreenElement screenElement = screen.DisplayedElements[subReader.ReadInt32()];

                      screenElement.Object.TemplateIndex = subReader.ReadInt32();
                      screenElement.Object.X = subReader.ReadInt32();
                      screenElement.Object.Y = subReader.ReadInt32();
                      screenElement.Repeats = subReader.ReadInt32();
                    }
                    break;
                }
              }
              // fetch extra object data
              Screens.Add( screen );
            }
            break;
          case (ushort)ChunkType.OBJECT_TEMPLATE:
            {
              ObjectTemplate obj = new ObjectTemplate();

              obj.Name = memReader.ReadString();
              obj.StartSprite = memReader.ReadInt32();

              int countBehaviours = memReader.ReadInt32();
              for ( int i = 0; i < countBehaviours; ++i )
              {
                int behaviourIndex = memReader.ReadInt32();
                string behaviourName = memReader.ReadString();

                int   newIndex = obj.Behaviours.Count;
                obj.Behaviours[newIndex] = new Behaviour();
                obj.Behaviours[newIndex].Name = behaviourName;
                obj.Behaviours[newIndex].Value = behaviourIndex;
              }
              obj.Index = memReader.ReadInt32();

              ObjectTemplates.Add( obj );
            }
            break;
        }
      }
      return true;
    }



    public bool SaveToFile( string Filename )
    {
      GR.Memory.ByteBuffer    resultingFile = new GR.Memory.ByteBuffer();

      GR.IO.FileChunk   chunkGeneral = new GR.IO.FileChunk( (ushort)ChunkType.GENERAL );
      chunkGeneral.AppendString( OldCharsetProjectFilename );
      chunkGeneral.AppendString( ExportFilename );
      chunkGeneral.AppendString( ExportPrefix );
      chunkGeneral.AppendI32( ExportConstantOffset );
      chunkGeneral.AppendString( SpriteProjectFilename );
      chunkGeneral.AppendString( ProjectType );
      chunkGeneral.AppendU8( EmptyChar );
      chunkGeneral.AppendU8( EmptyColor );
      resultingFile.Append( chunkGeneral.ToBuffer() );


      int     charSetIndex = 0;
      foreach ( CharsetProjectInfo info in CharsetProjects )
      {
        GR.IO.FileChunk chunkCharset = new GR.IO.FileChunk( (ushort)ChunkType.CHARSET_INFO );
        chunkCharset.AppendString( info.Filename );
        chunkCharset.AppendU8( (byte)( info.Multicolor ? 1 : 0 ) );
        resultingFile.Append( chunkCharset.ToBuffer() );

        ++charSetIndex;
      }

      foreach ( Element element in Elements )
      {
        GR.IO.FileChunk chunkElement = new GR.IO.FileChunk( (ushort)ChunkType.ELEMENT );
        chunkElement.AppendString( element.Name );
        chunkElement.AppendI32( element.Index );
        chunkElement.AppendI32( element.Characters.Width );
        chunkElement.AppendI32( element.Characters.Height );
        for ( int i = 0; i < element.Characters.Width; ++i )
        {
          for ( int j = 0; j < element.Characters.Height; ++j )
          {
            chunkElement.AppendU8( element.Characters[i, j].Char );
            chunkElement.AppendU8( element.Characters[i, j].Color );
          }
        }
        chunkElement.AppendI32( element.CharsetIndex );
        resultingFile.Append( chunkElement.ToBuffer() );
      }

      foreach ( ObjectTemplate obj in ObjectTemplates )
      {
        GR.IO.FileChunk chunkObj = new GR.IO.FileChunk( (ushort)ChunkType.OBJECT_TEMPLATE );
        chunkObj.AppendString( obj.Name );
        chunkObj.AppendI32( obj.StartSprite );
        chunkObj.AppendI32( obj.Behaviours.Count );
        foreach ( KeyValuePair<int,Behaviour> behaviourPair in obj.Behaviours )
        {
          chunkObj.AppendI32( behaviourPair.Value.Value );
          chunkObj.AppendString( behaviourPair.Value.Name );
        }
        chunkObj.AppendI32( obj.Index );
        resultingFile.Append( chunkObj.ToBuffer() );
      }

      foreach ( Screen screen in Screens )
      {
        GR.IO.FileChunk chunkScreen = new GR.IO.FileChunk( (ushort)ChunkType.SCREEN );

        GR.IO.FileChunk chunkScreenInfo = new GR.IO.FileChunk( (ushort)ChunkType.SCREEN_INFO );
        chunkScreenInfo.AppendString( screen.Name );
        chunkScreenInfo.AppendI32( screen.Width );
        chunkScreenInfo.AppendI32( screen.Height );
        chunkScreenInfo.AppendU8( screen.ConfigByte );
        chunkScreenInfo.AppendI32( screen.CharsetIndex );
        chunkScreenInfo.AppendU8( screen.WLConfigByte );
        chunkScreenInfo.AppendString( screen.ExtraData );
        chunkScreenInfo.AppendI32( screen.OverrideMC1 + 1 );
        chunkScreenInfo.AppendI32( screen.OverrideMC2 + 1 );
        chunkScreen.Append( chunkScreenInfo.ToBuffer() );

        foreach ( ScreenElement element in screen.DisplayedElements )
        {
          GR.IO.FileChunk chunkScreenElement = new GR.IO.FileChunk( (ushort)ChunkType.SCREEN_ELEMENT );
          chunkScreenElement.AppendI32( (int)element.Type );
          chunkScreenElement.AppendI32( element.Index );
          chunkScreenElement.AppendI32( element.X );
          chunkScreenElement.AppendI32( element.Y );
          chunkScreenElement.AppendI32( element.Repeats );
          chunkScreenElement.AppendI32( element.Repeats2 );
          chunkScreenElement.AppendI32( element.Char );
          chunkScreenElement.AppendI32( element.Color );
          chunkScreenElement.AppendI32( element.SearchObjectIndex );
          chunkScreenElement.AppendI32( element.TargetX );
          chunkScreenElement.AppendI32( element.TargetY );
          chunkScreenElement.AppendI32( element.TargetLevel );
          chunkScreenElement.AppendI32( element.Flags );

          chunkScreen.Append( chunkScreenElement.ToBuffer() );
        }
        // save extra object data
        int screenElementIndex = 0;
        foreach ( ScreenElement element in screen.DisplayedElements )
        {
          if ( element.Type == ScreenElementType.LD_OBJECT )
          {
            GR.IO.FileChunk chunkObject = new GR.IO.FileChunk( (ushort)ChunkType.ELEMENT_OBJECT_DATA );
            chunkObject.AppendI32( screenElementIndex );
            chunkObject.AppendI32( element.Object.TemplateIndex );
            chunkObject.AppendI32( element.Object.X );
            chunkObject.AppendI32( element.Object.Y );
            chunkObject.AppendI32( element.Object.Color );
            chunkObject.AppendI32( element.Object.Speed );
            chunkObject.AppendI32( element.Object.Behaviour );
            chunkObject.AppendI32( element.Object.MoveBorderLeft );
            chunkObject.AppendI32( element.Object.MoveBorderTop );
            chunkObject.AppendI32( element.Object.MoveBorderRight );
            chunkObject.AppendI32( element.Object.MoveBorderBottom );
            chunkObject.AppendI32( element.Object.Data );
            chunkObject.AppendI32( element.Object.OptionalValue );
            chunkObject.AppendI32( (int)element.Object.Optional );

            chunkScreen.Append( chunkObject.ToBuffer() );
          }
          else if ( element.Type == ScreenElementType.LD_SPAWN_SPOT )
          {
            GR.IO.FileChunk chunkSpawnSpot = new GR.IO.FileChunk( (ushort)ChunkType.ELEMENT_SPAWN_SPOT_DATA );
            chunkSpawnSpot.AppendI32( screenElementIndex );
            chunkSpawnSpot.AppendI32( element.Object.TemplateIndex );
            chunkSpawnSpot.AppendI32( element.Object.X );
            chunkSpawnSpot.AppendI32( element.Object.Y );
            chunkSpawnSpot.AppendI32( element.Repeats );

            chunkScreen.Append( chunkSpawnSpot.ToBuffer() );
          }
          ++screenElementIndex;
        }

        /*
        GR.IO.FileChunk chunkScreen = new GR.IO.FileChunk( (ushort)ChunkType.OLD_SCREEN );

        chunkScreen.AppendString( screen.Name );
        chunkScreen.AppendI32( screen.DisplayedElements.Count );
        foreach ( ScreenElement element in screen.DisplayedElements )
        {
          chunkScreen.AppendI32( (int)element.Type );
          chunkScreen.AppendI32( element.Index );
          chunkScreen.AppendI32( element.X );
          chunkScreen.AppendI32( element.Y );
          chunkScreen.AppendI32( element.Repeats );
          chunkScreen.AppendI32( element.Char );
          chunkScreen.AppendI32( element.Color );
          chunkScreen.AppendI32( element.SearchObjectIndex );
        }
        // save extra object data
        foreach ( ScreenElement element in screen.DisplayedElements )
        {
          if ( element.Type == ScreenElementType.LD_OBJECT )
          {
            GR.IO.FileChunk   chunkObject = new GR.IO.FileChunk( (ushort)ChunkType.ELEMENT_OBJECT_DATA );
            chunkObject.AppendI32( element.Object.TemplateIndex );
            chunkObject.AppendI32( element.Object.X );
            chunkObject.AppendI32( element.Object.Y );
            chunkObject.AppendI32( element.Object.Color );
            chunkObject.AppendI32( element.Object.Speed );
            chunkObject.AppendI32( element.Object.Behaviour );
            chunkObject.AppendI32( element.Object.MoveBorderLeft );
            chunkObject.AppendI32( element.Object.MoveBorderTop );
            chunkObject.AppendI32( element.Object.MoveBorderRight );
            chunkObject.AppendI32( element.Object.MoveBorderBottom );

            chunkScreen.Append( chunkObject.ToBuffer() );
          }
          else if ( element.Type == ScreenElementType.LD_SPAWN_SPOT )
          {
            GR.IO.FileChunk chunkSpawnSpot = new GR.IO.FileChunk( (ushort)ChunkType.ELEMENT_SPAWN_SPOT_DATA );
            chunkSpawnSpot.AppendI32( element.Object.TemplateIndex );
            chunkSpawnSpot.AppendI32( element.Object.X );
            chunkSpawnSpot.AppendI32( element.Object.Y );
            chunkSpawnSpot.AppendI32( element.Repeats );

            chunkScreen.Append( chunkSpawnSpot.ToBuffer() );
          }
        }
        GR.IO.FileChunk chunkScreenInfo = new GR.IO.FileChunk( (ushort)ChunkType.OLD_SCREEN_MORE_DATA );
        chunkScreenInfo.AppendI32( screen.Width );
        chunkScreen.Append( chunkScreenInfo.ToBuffer() );
        */
        resultingFile.Append( chunkScreen.ToBuffer() );
      }

      foreach ( Region region in Regions )
      {
        GR.IO.FileChunk chunkRegion = new GR.IO.FileChunk( (ushort)ChunkType.MAP_REGION );

        GR.IO.FileChunk chunkRegionInfo = new GR.IO.FileChunk( (ushort)ChunkType.MAP_REGION_INFO );

        chunkRegionInfo.AppendI32( region.DisplayX );
        chunkRegionInfo.AppendI32( region.DisplayY );
        chunkRegionInfo.AppendI32( region.Vertical ? 1 : 0 );
        chunkRegionInfo.AppendU32( region.ExtraData.Length );
        chunkRegionInfo.Append( region.ExtraData );

        chunkRegion.Append( chunkRegionInfo.ToBuffer() );

        foreach ( RegionScreenInfo screenInfo in region.Screens )
        {
          GR.IO.FileChunk chunkRegionScreenInfo = new GR.IO.FileChunk( (ushort)ChunkType.MAP_REGION_SCREEN );

          chunkRegionScreenInfo.AppendI32( screenInfo.ScreenIndex );
          chunkRegionScreenInfo.AppendI32( screenInfo.ExitN ? 1 : 0 );
          chunkRegionScreenInfo.AppendI32( screenInfo.ExitS ? 1 : 0 );
          chunkRegionScreenInfo.AppendI32( screenInfo.ExitW ? 1 : 0 );
          chunkRegionScreenInfo.AppendI32( screenInfo.ExitE ? 1 : 0 );

          chunkRegion.Append( chunkRegionScreenInfo.ToBuffer() );
        }
        resultingFile.Append( chunkRegion.ToBuffer() );
      }

      return GR.IO.File.WriteAllBytes( Filename, resultingFile );
    }



    public Element ElementFromString( string ElementName )
    {
      foreach ( Project.Element element in Elements )
      {
        if ( element.Name == ElementName )
        {
          return element;
        }
      }
      return null;
    }

  }
}

