using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using C64Studio.Formats;
using C64Studio.Displayer;
using RetroDevStudio.Types;
using RetroDevStudio;

namespace ElementEditor
{
  public partial class FormMain : Form
  {
    private enum ColorType
    {
      BACKGROUND = 0,
      MULTICOLOR_1,
      MULTICOLOR_2,
      CHAR_COLOR
    }



    private class SpriteData
    {
      public GR.Memory.ByteBuffer       Data = new GR.Memory.ByteBuffer( 63 );
      public bool                       Multicolor = false;
      public int                        Color = 1;
      public GR.Image.MemoryImage       Image = null;


      public SpriteData()
      {
        Image = new GR.Image.MemoryImage( 24, 21, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
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



    private class CharacterInfo : Project.Character, ICloneable
    {
      public Project.ScreenElement  ScreenElement = null;

      public new object Clone()
      {
        CharacterInfo clone = new CharacterInfo();

        clone.Char = Char;
        clone.Color = Color;
        clone.ScreenElement = ScreenElement;
        return clone;
      }
    }

    private class DataInfo
    {
      public string                 Name;
      public GR.Memory.ByteBuffer   Data = new GR.Memory.ByteBuffer();
      public int                    ReplacementOffset = 0;
      public DataInfo               ReplacementData = null;
      public bool                   Replaced = false;

      public DataInfo               PreviousData = null;
      public int                    OffsetInPreviousData = 0;
      public DataInfo               NextData = null;
      public bool                   IsChar = true;
    }

    
    
    public Project                      m_Project = new Project();
    private string                      m_ProjectFilename = "";
    private C64Studio.Formats.SpriteProject   m_SpriteProject = new C64Studio.Formats.SpriteProject();

    private Project.Screen              m_CurrentScreen = null;

    //private List<SpriteData>            m_Sprites = new List<SpriteData>();

    //private int                         m_BackgroundColorSprites = 0;
    //private int                         m_SpriteMultiColor1 = 0;
    //private int                         m_SpriteMultiColor2 = 0;

    private Project.Element             m_CurrentEditedElement = null;
    private Project.ScreenElement       m_DraggedScreenElement = null;
    private int                         m_DragOffsetX = 0;
    private int                         m_DragOffsetY = 0;

    private GR.Game.Layer<CharacterInfo> m_ScreenContent = new GR.Game.Layer<CharacterInfo>();

    private bool                        m_Modified = false;
    private int                         m_ScreenOffsetX = 0;
    private int                         m_ScreenOffsetY = 0;

    private GR.Image.MemoryImage[]      m_MapNumbers = new GR.Image.MemoryImage[10];

    private GR.Image.FastImage          m_BackgroundImage = null;

    private FormCheckResult             m_CheckResult = null;





    public FormMain()
    {
      InitializeComponent();

      pictureCharset.PixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppRgb;

      m_ScreenContent.InvalidTile = new CharacterInfo();
      m_ScreenContent.Resize( 40, 25 );

      comboElementTypes.Items.Add( "Single Element" );
      comboElementTypes.Items.Add( "Element Line H" );
      comboElementTypes.Items.Add( "Element Line V" );
      comboElementTypes.Items.Add( "Line H" );
      comboElementTypes.Items.Add( "Line V" );
      comboElementTypes.Items.Add( "Search Object" );
      comboElementTypes.Items.Add( "Line H Alternating" );
      comboElementTypes.Items.Add( "Line V Alternating" );
      comboElementTypes.Items.Add( "Area" );
      comboElementTypes.Items.Add( "Object" );
      comboElementTypes.Items.Add( "Spawn Spot" );
      comboElementTypes.Items.Add( "Element Area" );
      comboElementTypes.Items.Add( "Door" );
      comboElementTypes.Items.Add( "Clue" );
      comboElementTypes.Items.Add( "Special" );

      comboObjectOptional.Items.Add( "Always shown" );
      comboObjectOptional.Items.Add( "Hidden if optional set" );
      comboObjectOptional.Items.Add( "Shown if optional set" );
      comboObjectOptional.SelectedIndex = 0;

      screenMCColor1.Items.Add( "Default" );
      screenMCColor2.Items.Add( "Default" );

      for ( int i = 0; i < 16; ++i )
      {
        screenMCColor1.Items.Add( i.ToString() );
        screenMCColor2.Items.Add( i.ToString() );
        comboColor.Items.Add( i.ToString() );
        comboElementColor.Items.Add( i.ToString() );
        comboEmptyColor.Items.Add( i.ToString() );
      }
      for ( int i = 0; i < 256; ++i )
      {
        comboChars.Items.Add( i.ToString() );
        comboElementChar.Items.Add( i.ToString() );
        comboEmptyChar.Items.Add( i.ToString() );
      }
      comboEmptyColor.SelectedIndex = 0;
      comboEmptyChar.SelectedIndex = 0;
      comboProjectType.Items.Add( "Soulless" );
      comboProjectType.Items.Add( "Supernatural" );
      comboProjectType.Items.Add( "Cartridge" );
      comboProjectType.Items.Add( "Catnipped" );
      comboProjectType.Items.Add( "Barnsley Badger" );
      comboProjectType.Items.Add( "Wonderland" );
      comboProjectType.Items.Add( "Hyperion" );
      comboProjectType.Items.Add( "Rocky" );
      comboProjectType.Items.Add( "Adventure" );
      comboProjectType.Items.Add( "Soulless 2" );
      comboProjectType.Items.Add( "Downhill Challenge" );
      comboProjectType.Items.Add( "MegaSisters" );
      comboProjectType.SelectedIndex = 0;

      comboScreenObjectFlags.Items.Add( "None" );
      comboScreenObjectFlags.Items.Add( "1" );
      comboScreenObjectFlags.Items.Add( "2" );
      comboScreenObjectFlags.Items.Add( "3" );
      comboScreenObjectFlags.SelectedIndex = 0;

      /*
      try
      {
        for ( int i = 0; i < 256; ++i )
        {
          SpriteData spriteData = new SpriteData();
          m_Sprites.Add( spriteData );
        }
      }
      catch ( System.Exception e )
      {
        System.Windows.Forms.MessageBox.Show( e.ToString() );
      }*/

      m_BackgroundImage = new GR.Image.FastImage( 320, 200, System.Drawing.Imaging.PixelFormat.Format32bppRgb );

      pictureCharset.HottrackColor = 0x80ff00ff;
      pictureCharset.SelectedIndexChanged += new EventHandler( pictureCharset_SelectedIndexChanged );
      pictureCharset.SetDisplaySize( 128, 128 );
      pictureEditor.DisplayPage = new GR.Image.FastImage( 320, 200, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
      pictureEditor.MouseWheel += PictureEditor_MouseWheel;
      listSprites.PixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
      listSprites.ItemWidth = 24;
      listSprites.ItemHeight = 21;
      listSprites.HottrackColor = 0x80ff00ff;
      listSprites.SelectedIndexChanged += new EventHandler( listSprites_SelectedIndexChanged );
      pictureMap.DisplayPage = new GR.Image.FastImage( pictureMap.ClientSize.Width, pictureMap.ClientSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
      pictureMap.SetImageSize( pictureMap.ClientSize.Width, pictureMap.ClientSize.Height );

      for ( int j = 0; j < 16; ++j )
      {
        pictureEditor.DisplayPage.SetPaletteColor( j, (byte)( ( m_Project.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( m_Project.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( m_Project.m_ColorValues[j] & 0xff ) );
        pictureMap.DisplayPage.SetPaletteColor( j, (byte)( ( m_Project.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( m_Project.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( m_Project.m_ColorValues[j] & 0xff ) );
        pictureCharset.DisplayPage.SetPaletteColor( j, (byte)( ( m_Project.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( m_Project.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( m_Project.m_ColorValues[j] & 0xff ) );
        listSprites.DisplayPage.SetPaletteColor( j, (byte)( ( m_Project.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( m_Project.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( m_Project.m_ColorValues[j] & 0xff ) );
        m_BackgroundImage.SetPaletteColor( j, (byte)( ( m_Project.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( m_Project.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( m_Project.m_ColorValues[j] & 0xff ) );
      }
      pictureElement.DisplayPage = new GR.Image.FastImage( 42 * 4, 42 * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
      for ( int j = 0; j < 16; ++j )
      {
        pictureElement.DisplayPage.SetPaletteColor( j, (byte)( ( m_Project.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( m_Project.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( m_Project.m_ColorValues[j] & 0xff ) );
      }

      GR.Image.FastImage fastImage = GR.Image.FastImage.FromImage( Properties.Resources.map_numbers );
      for ( int i = 0; i < 10; ++i )
      {
        m_MapNumbers[i] = new GR.Image.MemoryImage( 5, 7, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
        for ( int j = 0; j < 16; ++j )
        {
          m_MapNumbers[i].SetPaletteColor( j, (byte)( ( m_Project.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( m_Project.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( m_Project.m_ColorValues[j] & 0xff ) );
        }
        fastImage.DrawTo( m_MapNumbers[i], 0, 0, i * 4, 0, 5, 7 );
      }
    }



    private void PictureEditor_MouseWheel( object sender, MouseEventArgs e )
    {
      if ( scrollScreenV.Enabled )
      {
        int   delta = e.Delta;

        while ( delta > 0 )
        {
          if ( scrollScreenV.Value >= 3 )
          {
            scrollScreenV.Value -= 3;
          }
          else
          {
            scrollScreenV.Value = 0;
          }
          delta -= 120;
        }
        while ( delta < 0 )
        {
          if ( scrollScreenV.Value + 3 <= scrollScreenV.Maximum )
          {
            scrollScreenV.Value += 3;
          }
          else
          {
            scrollScreenV.Value = scrollScreenV.Maximum;
          }
          delta += 120;
        }
      }
    }



    void pictureCharset_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( pictureCharset.SelectedIndex != -1 )
      {
        if ( ( listElementChars.SelectedIndices.Count == 0 )
        ||   ( m_CurrentEditedElement == null ) )
        {
          return;
        }
        int charIndex = listElementChars.SelectedIndices[0];

        if ( m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Char != (byte)pictureCharset.SelectedIndex )
        {
          m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Char = (byte)pictureCharset.SelectedIndex;
          comboChars.SelectedIndex = pictureCharset.SelectedIndex;
          listElementChars.Items[charIndex].SubItems[1].Text = comboChars.SelectedIndex.ToString();
          RedrawElementPreview();
          Modified = true;
        }
        
      }
    }



    public bool Modified
    {
      set
      {
        m_Modified = value;
      }
      get
      {
        return m_Modified;
      }
    }


    private void menuFileExit_Click( object sender, EventArgs e )
    {
      Close();
    }



    private void btnAddScreen_Click( object sender, EventArgs e )
    {
      Project.Screen  aScreen = new Project.Screen();

      //aScreen.Name = ( m_Project.Screens.Count + 1 ).ToString();
      aScreen.Name = editScreenName.Text;

      comboScreens.Items.Add( new GR.Generic.Tupel<string,Project.Screen>( comboScreens.Items.Count.ToString() + ":" + aScreen.Name, aScreen ) );
      comboRegionScreens.Items.Add( new GR.Generic.Tupel<string, Project.Screen>( comboScreens.Items.Count.ToString() + ":" + aScreen.Name, aScreen ) );
      m_Project.Screens.Add( aScreen );
      Modified = true;
    }



    private void btnDeleteScreen_Click( object sender, EventArgs e )
    {
      if ( comboScreens.SelectedIndex == -1 )
      {
        return;
      }
      int     index = comboScreens.SelectedIndex;
      Project.Screen    screen = ( (GR.Generic.Tupel<string, Project.Screen>)comboScreens.SelectedItem ).second;
      m_Project.Screens.Remove( screen );
      comboRegionScreens.Items.RemoveAt( index );
      comboScreens.Items.Remove( comboScreens.SelectedItem );

      // renumber screen items
      for ( int i = index; i < comboScreens.Items.Count; ++i )
      {
        GR.Generic.Tupel<string,Project.Screen>   screenItem = (GR.Generic.Tupel<string, Project.Screen>)comboScreens.Items[i];
        screenItem.first = i.ToString() + ":" + screenItem.second.Name;

        comboScreens.Items[i] = comboScreens.Items[i];
      }
      for ( int i = index; i < comboRegionScreens.Items.Count; ++i )
      {
        GR.Generic.Tupel<string, Project.Screen> screenItem = (GR.Generic.Tupel<string, Project.Screen>)comboRegionScreens.Items[i];
        screenItem.first = i.ToString() + ":" + screenItem.second.Name;

        comboRegionScreens.Items[i] = comboRegionScreens.Items[i];
      }
    }



    private void comboScreens_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( comboScreens.SelectedItem == null )
      {
        return;
      }
      Project.Screen  aScreen = ( (GR.Generic.Tupel<string, Project.Screen>)comboScreens.SelectedItem ).second;

      btnMoveScreenUp.Enabled = ( comboScreens.SelectedIndex > 0 );
      btnMoveScreenDown.Enabled = ( comboScreens.SelectedIndex + 1 < comboScreens.Items.Count );

      m_CurrentScreen = aScreen;

      bool    enableElementControls = ( m_CurrentScreen != null );

      btnAddElementToScreen.Enabled = enableElementControls;
      btnDelScreen.Enabled = enableElementControls;
      btnCopyScreen.Enabled = enableElementControls;
      labelElementType.Enabled = enableElementControls;
      comboElementTypes.Enabled = enableElementControls;
      labelElement.Enabled = enableElementControls;
      comboElements.Enabled = enableElementControls;
      labelElementX.Enabled = enableElementControls;
      editElementX.Enabled = enableElementControls;
      labelElementY.Enabled = enableElementControls;
      editElementY.Enabled = enableElementControls;
      labelElementRepeats.Enabled = enableElementControls;
      editElementRepeat.Enabled = enableElementControls;
      editElementRepeat2.Enabled = enableElementControls;
      editScreenConfig.Enabled = enableElementControls;
      labelScreenExtraData.Enabled = ( m_CurrentScreen != null );
      editScreenData.Enabled = ( m_CurrentScreen != null );

      if ( m_CurrentScreen != null )
      {
        editScreenName.Text = m_CurrentScreen.Name;
        editWonderlandBaseScreenConfig.Text = m_CurrentScreen.WLConfigByte.ToString( "X2" );

        editScreenData.Text = m_CurrentScreen.ExtraData;

        screenMCColor1.SelectedIndex = m_CurrentScreen.OverrideMC1 + 1;
        screenMCColor2.SelectedIndex = m_CurrentScreen.OverrideMC2 + 1;
      }

      labelElementChar.Enabled = enableElementControls;
      comboElementChar.Enabled = enableElementControls;
      labelElementColor.Enabled = enableElementControls;
      comboElementColor.Enabled = enableElementControls;
      if ( m_CurrentScreen != null )
      {
        editScreenWidth.Text = m_CurrentScreen.Width.ToString();
        scrollScreen.Minimum = 0;
        scrollScreen.Maximum = m_CurrentScreen.Width - 40;
        editScreenHeight.Text = m_CurrentScreen.Height.ToString();
        scrollScreenV.Minimum = 0;
        scrollScreenV.Maximum = m_CurrentScreen.Height - 25;

        m_ScreenContent.Resize( m_CurrentScreen.Width, m_CurrentScreen.Height );

        editScreenConfig.Text = m_CurrentScreen.ConfigByte.ToString();
        if ( ( m_CurrentScreen.CharsetIndex < comboScreenCharset.Items.Count )
        &&   ( m_CurrentScreen.CharsetIndex >= 0 ) )
        {
          comboScreenCharset.SelectedIndex = m_CurrentScreen.CharsetIndex;
        }
      }
      listScreenElements.Items.Clear();

      if ( m_CurrentScreen == null )
      {
        return;
      }

      screenMCColor1.Invalidate();
      screenMCColor2.Invalidate();

      if ( ( comboElementCharset.SelectedIndex >= 0 )
      &&   ( comboElementCharset.SelectedIndex < m_Project.Charsets.Count ) )
      {
        SetActiveElementCharset( m_Project.Charsets[comboScreenCharset.SelectedIndex],
                               m_CurrentScreen.OverrideMC1 != -1 ? m_CurrentScreen.OverrideMC1 : m_Project.Charsets[comboScreenCharset.SelectedIndex].Colors.MultiColor1,
                               m_CurrentScreen.OverrideMC2 != -1 ? m_CurrentScreen.OverrideMC2 : m_Project.Charsets[comboScreenCharset.SelectedIndex].Colors.MultiColor2,
                               m_Project.CharsetProjects[comboScreenCharset.SelectedIndex].Multicolor );
      }

      int   curItem = 0;
      foreach ( Project.ScreenElement screenElement in m_CurrentScreen.DisplayedElements )
      {
        ListViewItem    item = new ListViewItem( curItem.ToString() );
        Project.Element element = null;
        if ( screenElement.Index != -1 )
        {
          element = m_Project.Elements[screenElement.Index];
          item.SubItems.Add( element.Name );
        }
        else
        {
          item.SubItems.Add( "" );
        }
        item.SubItems.Add( screenElement.X.ToString() );
        item.SubItems.Add( screenElement.Y.ToString() );
        string elType = "";
        switch ( screenElement.Type )
        {
          case Project.ScreenElementType.LD_ELEMENT:
            elType = "EL";
            break;
          case Project.ScreenElementType.LD_ELEMENT_AREA:
            elType = "EA";
            break;
          case Project.ScreenElementType.LD_DOOR:
            elType = "DO";
            break;
          case Project.ScreenElementType.LD_CLUE:
            elType = "CL";
            break;
          case Project.ScreenElementType.LD_SPECIAL:
            elType = "SP";
            break;
          case Project.ScreenElementType.LD_ELEMENT_LINE_H:
            elType = "EH";
            break;
          case Project.ScreenElementType.LD_ELEMENT_LINE_V:
            elType = "EV";
            break;
          case Project.ScreenElementType.LD_LINE_H:
            item.SubItems[1].Text = "Line H";
            elType = "LH";
            break;
          case Project.ScreenElementType.LD_LINE_V:
            item.SubItems[1].Text = "Line V";
            elType = "LV";
            break;
          case Project.ScreenElementType.LD_LINE_H_ALT:
            item.SubItems[1].Text = "Line H Alt";
            elType = "AH";
            break;
          case Project.ScreenElementType.LD_LINE_V_ALT:
            item.SubItems[1].Text = "Line V Alt";
            elType = "AV";
            break;
          case Project.ScreenElementType.LD_SEARCH_OBJECT:
            elType = "SO";
            break;
          case Project.ScreenElementType.LD_OBJECT:
            elType = "OB";
            if ( screenElement.Object.TemplateIndex != -1 )
            {
              item.SubItems[1].Text = m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].Name;
            }
            break;
          case Project.ScreenElementType.LD_SPAWN_SPOT:
            elType = "SS";
            if ( screenElement.Object.TemplateIndex != -1 )
            {
              item.SubItems[1].Text = m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].Name;
            }
            break;
          case Project.ScreenElementType.LD_AREA:
            elType = "AR";
            break;
          default:
            throw new NotSupportedException( "Screen element type " + screenElement.Type.ToString() + " not supported" );
        }
        item.SubItems.Add( elType );
        item.SubItems.Add( screenElement.Repeats.ToString() );
        item.Tag = screenElement;

        listScreenElements.Items.Add( item );
        ++curItem;
      }

      RedrawScreen();
    }



    private void RedrawScreen()
    {
      //pictureEditor.DisplayPage.Box( 0, 0, pictureEditor.DisplayPage.Width, pictureEditor.DisplayPage.Height, 0 );
      m_BackgroundImage.DrawTo( pictureEditor.DisplayPage, 0, 0 );

      for ( int i = 0; i < m_ScreenContent.Width; ++i )
      {
        for ( int j = 0; j < m_ScreenContent.Height; ++j )
        {
          CharacterInfo   emptyChar = new CharacterInfo();
          emptyChar.Char  = m_Project.EmptyChar;
          emptyChar.Color = m_Project.EmptyColor;

          m_ScreenContent[i,j] = emptyChar;
        }
      }

      // draw empty chars
      if ( ( m_CurrentScreen != null )
      &&   ( m_CurrentScreen.CharsetIndex >= 0 )
      &&   ( m_CurrentScreen.CharsetIndex < m_Project.Charsets.Count ) )
      {
        CharsetProject charSet = m_Project.Charsets[m_CurrentScreen.CharsetIndex];

        for ( int i = 0; i < charSet.Colors.Palette.NumColors; ++i )
        {
          pictureEditor.DisplayPage.SetPaletteColor( i, 
            (byte)( ( charSet.Colors.Palette.ColorValues[i] & 0x00ff0000 ) >> 16 ), 
            (byte)( ( charSet.Colors.Palette.ColorValues[i] & 0x0000ff00 ) >> 8 ), 
            (byte)( charSet.Colors.Palette.ColorValues[i] & 0xff ) );
        }

        for ( int i = 0; i < 40; ++i )
        {
          for ( int j = 0; j < 25; ++j )
          {
            CharacterDisplayer.DisplayChar( charSet, m_Project.EmptyChar, pictureEditor.DisplayPage, i * 8, j * 8, m_Project.EmptyColor );
          }
        }
      }

      if ( ( m_CurrentScreen != null )
      &&   ( m_Project.ProjectType == "Wonderland" )
      &&   ( comboScreens.SelectedIndex >= 52 ) )
      {
        // special case part building
        //      LL        Looks right (0 to 3)
        //        LL      Looks center (0 to 3)
        //           LL   Looks left (0 to 3)
        //             TT Type (0 = grass, 1 = earth, 2 = beach, 3 = mountains)

        byte partConfig = m_CurrentScreen.WLConfigByte;

        int bgScreen = ( partConfig & 0x03 );
        if ( bgScreen + 48 < m_Project.Screens.Count )
        {
          DrawScreen( m_Project.Screens[bgScreen + 48] );
        }
        int leftScreen = bgScreen * 12 + ( ( partConfig & 0x0c ) >> 2 );
        if ( leftScreen < m_Project.Screens.Count )
        {
          DrawScreen( m_Project.Screens[leftScreen] );
        }
        int middleScreen = bgScreen * 12 + 4 + ( ( partConfig & 0x30 ) >> 4 );
        if ( middleScreen < m_Project.Screens.Count )
        {
          DrawScreen( m_Project.Screens[middleScreen] );
        }
        int rightScreen = bgScreen * 12 + 8 + ( ( partConfig & 0xc0 ) >> 6 );
        if ( rightScreen < m_Project.Screens.Count )
        {
          DrawScreen( m_Project.Screens[rightScreen] );
        }
      }
      DrawScreen( m_CurrentScreen );

      if ( listScreenElements.SelectedItems.Count > 0 )
      {
        Project.ScreenElement screenElement = (Project.ScreenElement)listScreenElements.SelectedItems[0].Tag;
        Project.Element element = null;
        if ( screenElement.Index != -1 )
        {
          element = m_Project.Elements[screenElement.Index];
        }

        GR.Collections.Set<System.Drawing.Point> origPoints = AffectedCharacters( screenElement );

        System.Drawing.Point neighbourPoint = new Point();

        uint    selectionColor = 0xff40ff40;

        foreach ( System.Drawing.Point point in origPoints )
        {
          neighbourPoint = point;
          neighbourPoint.X = point.X - 1;
          if ( !origPoints.ContainsValue( neighbourPoint ) )
          {
            for ( int y = 0; y < 8; ++y )
            {
              pictureEditor.DisplayPage.SetPixel( ( point.X - m_ScreenOffsetX ) * 8, ( point.Y - m_ScreenOffsetY ) * 8 + y, selectionColor );
            }
          }
          neighbourPoint.X = point.X + 1;
          if ( !origPoints.ContainsValue( neighbourPoint ) )
          {
            for ( int y = 0; y < 8; ++y )
            {
              pictureEditor.DisplayPage.SetPixel( ( point.X - m_ScreenOffsetX ) * 8 + 8 - 1, ( point.Y - m_ScreenOffsetY ) * 8 + y, selectionColor );
            }
          }
          neighbourPoint.X = point.X;
          neighbourPoint.Y = point.Y - 1;
          if ( !origPoints.ContainsValue( neighbourPoint ) )
          {
            for ( int x = 0; x < 8; ++x )
            {
              pictureEditor.DisplayPage.SetPixel( ( point.X - m_ScreenOffsetX ) * 8 + x, ( point.Y - m_ScreenOffsetY ) * 8, selectionColor );
            }
          }
          neighbourPoint.Y = point.Y + 1;
          if ( !origPoints.ContainsValue( neighbourPoint ) )
          {
            for ( int x = 0; x < 8; ++x )
            {
              pictureEditor.DisplayPage.SetPixel( ( point.X - m_ScreenOffsetX ) * 8 + x, ( point.Y - m_ScreenOffsetY ) * 8 + 8 - 1, selectionColor );
            }
          }
        }
      }
      // safety border (soulless/j)
      uint    safetyColor = 0xffff00ff;
      if ( m_Project.ProjectType == "Rocky" )
      {
        pictureEditor.DisplayPage.Box( 0, 176 + 16, pictureEditor.DisplayPage.Width, 1, safetyColor );
      }
      else
      {
        pictureEditor.DisplayPage.Box( 0, 176, pictureEditor.DisplayPage.Width, 1, safetyColor );
      }
      pictureEditor.Invalidate();
    }



    private void DrawScreen( Project.Screen Screen )
    {
      if ( ( Screen != null )
      &&   ( Screen.CharsetIndex >= 0 )
      &&   ( Screen.CharsetIndex < m_Project.Charsets.Count ) )
      {
        CharsetProject charSet = m_Project.Charsets[Screen.CharsetIndex];

        foreach ( Project.ScreenElement element in Screen.DisplayedElements )
        {
          Project.Element displayElement = null;
          if ( element.Index != -1 )
          {
            displayElement = m_Project.Elements[element.Index];
          }

          switch ( element.Type )
          {
            case Project.ScreenElementType.LD_ELEMENT:
            case Project.ScreenElementType.LD_SEARCH_OBJECT:
            case Project.ScreenElementType.LD_DOOR:
            case Project.ScreenElementType.LD_CLUE:
            case Project.ScreenElementType.LD_SPECIAL:
              DisplayElement( displayElement, element.X, element.Y, element );
              break;
            case Project.ScreenElementType.LD_ELEMENT_LINE_H:
              {
                int     X = element.X;

                for ( int i = 0; i < element.Repeats; ++i )
                {
                  DisplayElement( displayElement, X, element.Y, element );

                  X += displayElement.Characters.Width;
                }
              }
              break;
            case Project.ScreenElementType.LD_ELEMENT_LINE_V:
              {
                int     Y = element.Y;

                if ( displayElement != null )
                {
                  for ( int i = 0; i < element.Repeats; ++i )
                  {
                    DisplayElement( displayElement, element.X, Y, element );

                    Y += displayElement.Characters.Height;
                  }
                }
              }
              break;
            case Project.ScreenElementType.LD_ELEMENT_AREA:
              {
                int X = element.X;
                int Y = element.Y;

                if ( displayElement != null )
                {
                  for ( int j = 0; j < element.Repeats2; ++j )
                  {
                    X = element.X;
                    for ( int i = 0; i < element.Repeats; ++i )
                    {
                      DisplayElement( displayElement, X, Y, element );

                      X += displayElement.Characters.Width;
                    }
                    Y += displayElement.Characters.Height;
                  }
                }
              }
              break;
            case Project.ScreenElementType.LD_LINE_H:
              {
                int     X = element.X;

                for ( int i = 0; i < element.Repeats; ++i )
                {
                  int     targetX = X + i;
                  int     targetY = element.Y;
                  while ( targetX >= Screen.Width )
                  {
                    targetX -= Screen.Width;
                    targetY++;
                  }

                  CharacterDisplayer.DisplayChar( charSet, element.Char, pictureEditor.DisplayPage, ( targetX - m_ScreenOffsetX ) * 8, ( targetY - m_ScreenOffsetY ) * 8, element.Color );
                  m_ScreenContent[targetX, targetY].Char = (byte)element.Char;
                  m_ScreenContent[targetX, targetY].Color = (byte)element.Color;
                  m_ScreenContent[targetX, targetY].ScreenElement = element;
                }
              }
              break;
            case Project.ScreenElementType.LD_LINE_V:
              {
                int     Y = element.Y;

                for ( int i = 0; i < element.Repeats; ++i )
                {
                  int     targetY = Y + i;
                  int     targetX = element.X;
                  while ( targetX >= Screen.Width )
                  {
                    targetX -= Screen.Width;
                    targetY++;
                  }

                  CharacterDisplayer.DisplayChar( charSet, element.Char, pictureEditor.DisplayPage, ( targetX - m_ScreenOffsetX ) * 8, ( targetY - m_ScreenOffsetY ) * 8, element.Color );
                  m_ScreenContent[targetX, targetY].Char = (byte)element.Char;
                  m_ScreenContent[targetX, targetY].Color = (byte)element.Color;
                  m_ScreenContent[targetX, targetY].ScreenElement = element;
                }
              }
              break;
            case Project.ScreenElementType.LD_LINE_H_ALT:
              {
                int     X = element.X;

                for ( int i = 0; i < element.Repeats; ++i )
                {
                  int     targetX = X + i;
                  int     targetY = element.Y;
                  while ( targetX >= Screen.Width )
                  {
                    targetX -= Screen.Width;
                    targetY++;
                  }
                  int charVal = element.Char + i % 2;
                  if ( charVal > 255 )
                  {
                    charVal = 0;
                  }

                  CharacterDisplayer.DisplayChar( charSet, charVal, pictureEditor.DisplayPage, ( targetX - m_ScreenOffsetX ) * 8, ( targetY - m_ScreenOffsetY ) * 8, element.Color );
                  m_ScreenContent[targetX, targetY].Char = (byte)charVal;
                  m_ScreenContent[targetX, targetY].Color = (byte)element.Color;
                  m_ScreenContent[targetX, targetY].ScreenElement = element;
                }
              }
              break;
            case Project.ScreenElementType.LD_LINE_V_ALT:
              {
                int     Y = element.Y;

                for ( int i = 0; i < element.Repeats; ++i )
                {
                  int     targetY = Y + i;
                  int     targetX = element.X;
                  while ( targetX >= Screen.Width )
                  {
                    targetX -= Screen.Width;
                    targetY++;
                  }
                  int charVal = element.Char + i % 2;
                  if ( charVal > 255 )
                  {
                    charVal = 0;
                  }

                  CharacterDisplayer.DisplayChar( charSet, charVal, pictureEditor.DisplayPage, ( targetX - m_ScreenOffsetX ) * 8, ( targetY - m_ScreenOffsetY ) * 8, element.Color );
                  m_ScreenContent[targetX, targetY].Char = (byte)charVal;
                  m_ScreenContent[targetX, targetY].Color = (byte)element.Color;
                  m_ScreenContent[targetX, targetY].ScreenElement = element;
                }
              }
              break;
            case Project.ScreenElementType.LD_OBJECT:
              if ( element.Object.SpriteImage != null )
              {
                pictureEditor.DisplayPage.DrawFromImage( element.Object.SpriteImage,
                                                         ( element.X - m_ScreenOffsetX ) * 8 - 8, ( element.Y - m_ScreenOffsetY ) * 8 - 13 );

                int movePathX = ( element.X - m_ScreenOffsetX ) * 8 - 8 * element.Object.MoveBorderLeft;
                int movePathY = ( element.Y - m_ScreenOffsetY ) * 8 - 8 * element.Object.MoveBorderTop;
                int movePathEndX = ( element.X - m_ScreenOffsetX ) * 8 + 7 + 8 * element.Object.MoveBorderRight;
                int movePathEndY = ( element.Y - m_ScreenOffsetY ) * 8 + 7 + 8 * element.Object.MoveBorderBottom;

                uint    colorObjectSelection = 0xffff0000;
                for ( int x = movePathX; x <= movePathEndX; ++x )
                {
                  pictureEditor.DisplayPage.SetPixel( x, movePathY, colorObjectSelection );
                  pictureEditor.DisplayPage.SetPixel( x, movePathEndY, colorObjectSelection );
                }
                for ( int y = movePathY; y <= movePathEndY; ++y )
                {
                  pictureEditor.DisplayPage.SetPixel( movePathX, y, colorObjectSelection );
                  pictureEditor.DisplayPage.SetPixel( movePathEndX, y, colorObjectSelection );
                }

              }
              break;
            case Project.ScreenElementType.LD_SPAWN_SPOT:
              if ( element.Object.SpriteImage != null )
              {
                pictureEditor.DisplayPage.DrawFromImage( element.Object.SpriteImage,
                                                               ( element.X - m_ScreenOffsetX ) * 8 - 8, 
                                                               ( element.Y - m_ScreenOffsetY ) * 8 - 13 );
              }
              break;
            case Project.ScreenElementType.LD_AREA:
              {
                int X = element.X;
                int Y = element.Y;

                for ( int j = 0; j < element.Repeats2; ++j )
                {
                  for ( int i = 0; i < element.Repeats; ++i )
                  {
                    int targetX = X + i;
                    int targetY = Y + j;
                    while ( targetX >= Screen.Width )
                    {
                      targetX -= Screen.Width;
                      targetY++;
                    }

                    CharacterDisplayer.DisplayChar( charSet, element.Char, pictureEditor.DisplayPage, ( targetX - m_ScreenOffsetX ) * 8, ( targetY - m_ScreenOffsetY ) * 8, element.Color );
                    m_ScreenContent[targetX, targetY].Char = (byte)element.Char;
                    m_ScreenContent[targetX, targetY].Color = (byte)element.Color;
                    m_ScreenContent[targetX, targetY].ScreenElement = element;
                  }
                }
              }
              break;
          }
        }
      }
    }



    private void DisplayElement( Project.Element Element, int X, int Y, Project.ScreenElement ScreenElement )
    {
      if ( Element == null )
      {
        return;
      }
      CharsetProject charSet = m_Project.Charsets[m_CurrentScreen.CharsetIndex];

      for ( int i = 0; i < Element.Characters.Width; ++i )
      {
        for ( int j = 0; j < Element.Characters.Height; ++j )
        {
          int     targetX = X + i;
          int     targetY = Y + j;
          while ( targetX >= m_CurrentScreen.Width )
          {
            targetX -= m_CurrentScreen.Width;
            targetY++;
          }
          if ( charSet != null )
          {
            CharacterDisplayer.DisplayChar( charSet, Element.Characters[i, j].Char, pictureEditor.DisplayPage, ( targetX - m_ScreenOffsetX ) * 8, ( targetY - m_ScreenOffsetY ) * 8, Element.Characters[i, j].Color );
          }
          m_ScreenContent[targetX, targetY].Char  = Element.Characters[i, j].Char;
          m_ScreenContent[targetX, targetY].Color = Element.Characters[i, j].Color;
          m_ScreenContent[targetX, targetY].ScreenElement = ScreenElement;
        }
      }
    }



    private void RedrawElementPreview()
    {
      for ( int i = 0; i < pictureElement.DisplayPage.Width; ++i )
      {
        for ( int j = 0; j < pictureElement.DisplayPage.Height; ++j )
        {
          pictureElement.DisplayPage.SetPixelData( i, j, 0 );
        }
      }
      for ( int i = 0; i < m_ScreenContent.Width; ++i )
      {
        for ( int j = 0; j < m_ScreenContent.Height; ++j )
        {
          m_ScreenContent[i, j].ScreenElement = null;
          m_ScreenContent[i, j].Char = 0;
          m_ScreenContent[i, j].Color = 0;
        }
      }
      if ( m_CurrentEditedElement == null )
      {
        return;
      }
      CharsetProject charSet = m_Project.Charsets[m_CurrentEditedElement.CharsetIndex];
      for ( int i = 0; i < m_CurrentEditedElement.Characters.Width; ++i )
      {
        for ( int j = 0; j < m_CurrentEditedElement.Characters.Height; ++j )
        {
          CharacterDisplayer.DisplayChar( charSet, m_CurrentEditedElement.Characters[i, j].Char, pictureElement.DisplayPage, i * 8, j * 8, m_CurrentEditedElement.Characters[i, j].Color );
        }
      }
      pictureElement.Invalidate();
    }



    void RebuildSpriteImage( int SpriteIndex, int AlternativeColor )
    {
      var Data = m_SpriteProject.Sprites[SpriteIndex];

      if ( AlternativeColor == -1 )
      {
        AlternativeColor = Data.Tile.CustomColor;
      }

      RebuildSpriteImage( Data.Tile, m_SpriteProject.Colors.Palette, Data.Tile.Image, Data.Mode, AlternativeColor );
    }



    void RebuildSpriteImage( GraphicTile Tile, Palette Palette, GR.Image.MemoryImage Image, RetroDevStudio.SpriteMode Mode, int AlternativeColor )
    {
      switch ( Mode )
      {
        case RetroDevStudio.SpriteMode.COMMODORE_24_X_21_HIRES:
          SpriteDisplayer.DisplayHiResSprite( Tile.Data, Palette, Tile.Width, Tile.Height, m_SpriteProject.Colors.BackgroundColor, AlternativeColor, Image, 0, 0 );
          break;
        case RetroDevStudio.SpriteMode.COMMODORE_24_X_21_MULTICOLOR:
          SpriteDisplayer.DisplayMultiColorSprite( Tile.Data, Palette, Tile.Width, Tile.Height,
                  m_SpriteProject.Colors.BackgroundColor,
                  m_SpriteProject.Colors.MultiColor1,
                  m_SpriteProject.Colors.MultiColor2,
                  AlternativeColor, Image, 0, 0 );
          break;
        case RetroDevStudio.SpriteMode.MEGA65_8_X_21_16_COLORS:
        case RetroDevStudio.SpriteMode.MEGA65_16_X_21_16_COLORS:
          SpriteDisplayer.DisplayFCMSprite( Tile.Data, Palette, Tile.Width, Tile.Height, m_SpriteProject.Colors.BackgroundColor, Image, 0, 0, false, false );
          break;
      }
    }



    void RebuildCharImage( CharsetProject CharSet, int Color1, int Color2, int CharIndex, int AlternativeColor, bool Multicolor )
    {
      if ( CharSet == null )
      {
        return;
      }
      CharData Char = CharSet.Characters[CharIndex];

      if ( AlternativeColor == -1 )
      {
        AlternativeColor = Char.Tile.CustomColor;
      }

      CharacterDisplayer.DisplayChar( CharSet,
                                      CharIndex,
                                      CharSet.Characters[CharIndex].Tile.Image,
                                      0, 0,
                                      AlternativeColor );
      /*
      if ( ( !Multicolor )
      ||   ( AlternativeColor < 8 ) )
      {
        // single color
        int charColor = AlternativeColor;
        int colorIndex = 0;
        for ( int j = 0; j < 8; ++j )
        {
          for ( int i = 0; i < 8; ++i )
          {
            if ( ( Char.Tile.Data.ByteAt( j ) & ( 1 << ( 7 - i ) ) ) != 0 )
            {
              colorIndex = charColor;
            }
            else
            {
              colorIndex = CharSet.Colors.BackgroundColor;
            }
            CharSet.Characters[CharIndex].AllColorImage.SetPixel( AlternativeColor * 8 + i, j, (uint)colorIndex );
          }
        }
      }
      else
      {
        // multicolor
        int charColor = AlternativeColor - 8;

        for ( int j = 0; j < 8; ++j )
        {
          for ( int i = 0; i < 4; ++i )
          {
            int pixelValue = ( Char.Tile.Data.ByteAt( j ) & ( 3 << ( ( 3 - i ) * 2 ) ) ) >> ( ( 3 - i ) * 2 );

            switch ( pixelValue )
            {
              case 0:
                pixelValue = CharSet.Colors.BackgroundColor;
                break;
              case 1:
                pixelValue = Color1;//CharSet.MultiColor1;
                break;
              case 2:
                pixelValue = Color2;//CharSet.MultiColor2;
                break;
              case 3:
                pixelValue = charColor;
                break;
            }
            CharSet.Characters[CharIndex].AllColorImage.SetPixel( AlternativeColor * 8 + i * 2, j, (uint)pixelValue );
            CharSet.Characters[CharIndex].AllColorImage.SetPixel( AlternativeColor * 8 + i * 2 + 1, j, (uint)pixelValue );
          }
        }
      }*/
    }



    public void Clear()
    {
      m_Project.Screens.Clear();
      m_Project.Elements.Clear();
      m_Project.CharsetProjects.Clear();
      m_Project.Charsets.Clear();

      comboScreens.Items.Clear();
      comboObjects.Items.Clear();
      comboObjectBehaviour.Items.Clear();
      comboElements.Items.Clear();
      comboScreenCharset.Items.Clear();
      comboElementCharset.Items.Clear();
      listAvailableObjects.Items.Clear();
      listScreenElements.Items.Clear();
      listAvailableElements.Items.Clear();
      comboElementCharset.Items.Clear();
      listRegions.Items.Clear();
      comboRegionScreens.Items.Clear();

      m_CurrentScreen = null;
      m_CurrentEditedElement = null;
    }



    public CharsetProject OpenCharsetProject( string Filename )
    {
      GR.Memory.ByteBuffer    projectFile = GR.IO.File.ReadAllBytes( Filename );
      if ( projectFile == null )
      {
        return null;
      }

      CharsetProject    charSet = new CharsetProject();

      charSet.ReadFromBuffer( projectFile );

      return charSet;
    }



    private void SetActiveScreenCharset( CharsetProject CharSet, int Color1, int Color2, bool Multicolor )
    {
      if ( CharSet == null )
      {
        return;
      }
      for ( int i = 0; i < 256; ++i )
      {
        for ( int j = 0; j < 16; ++j )
        {
          RebuildCharImage( CharSet, Color1, Color2, i, j, Multicolor );
        }
      }
      pictureCharset.Items.Clear();
      pictureCharset.ItemWidth = 8;
      pictureCharset.ItemHeight = 8;
      for ( int j = 0; j < 16; ++j )
      {
        for ( int i = 0; i < 16; ++i )
        {
          pictureCharset.Items.Add( i + j* 16, CharSet.Characters[i + j * 16].Tile.Image );
        }
      }
      RedrawScreen();
      pictureEditor.Invalidate();
    }



    public void SetActiveElementCharset( CharsetProject CharSet, int Color1, int Color2, bool Multicolor )
    {
      for ( int i = 0; i < CharSet.Colors.Palette.NumColors; ++i )
      {
        pictureCharset.DisplayPage.SetPaletteColor( i,
          (byte)( ( CharSet.Colors.Palette.ColorValues[i] & 0x00ff0000 ) >> 16 ),
          (byte)( ( CharSet.Colors.Palette.ColorValues[i] & 0x0000ff00 ) >> 8 ),
          (byte)( CharSet.Colors.Palette.ColorValues[i] & 0xff ) );

        pictureElement.DisplayPage.SetPaletteColor( i,
          (byte)( ( CharSet.Colors.Palette.ColorValues[i] & 0x00ff0000 ) >> 16 ),
          (byte)( ( CharSet.Colors.Palette.ColorValues[i] & 0x0000ff00 ) >> 8 ),
          (byte)( CharSet.Colors.Palette.ColorValues[i] & 0xff ) );
      }

      for ( int i = 0; i < 256; ++i )
      {
        for ( int j = 0; j < 16; ++j )
        {
          RebuildCharImage( CharSet, Color1, Color2, i, j, Multicolor );
        }
      }
      pictureCharset.Items.Clear();
      pictureCharset.ItemWidth = 8;
      pictureCharset.ItemHeight = 8;
      for ( int j = 0; j < 16; ++j )
      {
        for ( int i = 0; i < 16; ++i )
        {
          pictureCharset.Items.Add( i + j * 16, CharSet.Characters[i + j * 16].Tile.Image );
        }
      }
      RedrawElementPreview();
    }



    public void OpenProject( string Filename )
    {
      Clear();
      if ( m_Project.LoadFromFile( Filename ) )
      {
        foreach ( Project.Screen screen in m_Project.Screens )
        {
          comboScreens.Items.Add( new GR.Generic.Tupel<string, Project.Screen>( comboScreens.Items.Count.ToString() + ":" + screen.Name, screen ) );
          comboRegionScreens.Items.Add( new GR.Generic.Tupel<string, Project.Screen>( comboRegionScreens.Items.Count.ToString() + ":" + screen.Name, screen ) );
        }
        foreach ( Project.Element element in m_Project.Elements )
        {
          comboElements.Items.Add( element.Name );
          listAvailableElements.Items.Add( element.Name );
        }
        foreach ( Project.ObjectTemplate obj in m_Project.ObjectTemplates )
        {
          comboObjects.Items.Add( obj );
          listAvailableObjects.Items.Add( obj );
        }
        int regionIndex = 0;
        foreach ( Project.Region region in m_Project.Regions )
        {
          ListViewItem itemRegion = ItemFromRegion( regionIndex, region );
          listRegions.Items.Add( itemRegion );
          ++regionIndex;
        }

        editExportFile.Text = m_Project.ExportFilename;
        editExportPrefix.Text = m_Project.ExportPrefix;
        editConstantOffset.Text = m_Project.ExportConstantOffset.ToString();
        comboProjectType.SelectedItem = m_Project.ProjectType;

        if ( ( m_Project.CharsetProjects.Count == 0 )
        &&   ( !string.IsNullOrEmpty( m_Project.OldCharsetProjectFilename ) ) )
        {
          string fullPath = GR.Path.Append( GR.Path.RemoveFileSpec( Filename ), m_Project.OldCharsetProjectFilename );
          CharsetProject charSet = OpenCharsetProject( fullPath );
          if ( charSet != null )
          {
            charSet.Name = fullPath;
            string shortName = System.IO.Path.GetFileNameWithoutExtension( fullPath );
            m_Project.Charsets.Add( charSet );

            CharsetProjectInfo info = new CharsetProjectInfo();
            info.Filename = fullPath;
            info.Multicolor = true;
            m_Project.CharsetProjects.Add( info );
          }
        }
        else
        {
          for ( int i = 0; i < m_Project.CharsetProjects.Count; ++i )
          {
            string charSetFile = m_Project.CharsetProjects[i].Filename;
            CharsetProject charSet = OpenCharsetProject( charSetFile );
            if ( charSet == null )
            {
              charSetFile = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( Filename ), System.IO.Path.GetFileName( charSetFile ) );
              m_Project.CharsetProjects[i].Filename = charSetFile;
              charSet = OpenCharsetProject( charSetFile );
              if ( charSet == null )
              {
                charSet = OpenCharsetProject( System.IO.Path.GetFileName( charSetFile ) );
                m_Project.CharsetProjects[i].Filename = System.IO.Path.GetFileName( charSetFile );
              }
            }
            if ( charSet == null )
            {
              System.Windows.Forms.MessageBox.Show( "Could not open Charset file " + charSetFile );
              Clear();
              return;
            }
            m_Project.Charsets.Add( charSet );
          }
        }
        foreach ( CharsetProject charSet in m_Project.Charsets )
        {
          if ( charSet == null )
          {
            comboScreenCharset.Items.Add( "<invalid charset>" );
            comboElementCharset.Items.Add( "<invalid charset>" );
          }
          else
          {
            comboScreenCharset.Items.Add( System.IO.Path.GetFileNameWithoutExtension( charSet.Name ) );
            comboElementCharset.Items.Add( System.IO.Path.GetFileNameWithoutExtension( charSet.Name ) );
          }
        }
        if ( comboScreenCharset.Items.Count > 0 )
        {
          comboScreenCharset.SelectedIndex = 0;
        }
        if ( comboElementCharset.Items.Count > 0 )
        {
          comboElementCharset.SelectedIndex = 0;
        }
        comboEmptyChar.SelectedIndex = m_Project.EmptyChar;
        comboEmptyColor.SelectedIndex = m_Project.EmptyColor;

        if ( !string.IsNullOrEmpty( m_Project.SpriteProjectFilename ) )
        {
          string fullPath = GR.Path.Append( GR.Path.RemoveFileSpec( Filename ), m_Project.SpriteProjectFilename );
          OpenSpriteProject( fullPath );

          foreach ( Project.Screen screen in m_Project.Screens )
          {
            foreach ( Project.ScreenElement element in screen.DisplayedElements )
            {
              if ( ( element.Type == Project.ScreenElementType.LD_OBJECT )
              ||   ( element.Type == Project.ScreenElementType.LD_SPAWN_SPOT ) )
              {
                if ( ( element.Object != null )
                &&   ( element.Object.TemplateIndex != -1 ) )
                {
                  element.Object.SpriteImage = new GR.Image.MemoryImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Tile.Image );
                  RebuildSpriteImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Tile,
                                      m_SpriteProject.Colors.Palette,
                                      element.Object.SpriteImage,
                                      m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Mode,
                                      element.Object.Color );
                }
              }
            }
          }
        }
      }
      if ( m_Project.Charsets.Count > 0 )
      {
        for ( int i = 0; i < m_Project.Charsets[0].Colors.Palette.NumColors; ++i )
        {
          pictureEditor.DisplayPage.SetPaletteColor( i,   
                                                     (byte)( ( m_Project.Charsets[0].Colors.Palette.ColorValues[i] & 0xff0000 ) >> 16 ),
                                                     (byte)( ( m_Project.Charsets[0].Colors.Palette.ColorValues[i] & 0x00ff00 ) >> 8 ),
                                                     (byte)( ( m_Project.Charsets[0].Colors.Palette.ColorValues[i] & 0x0000ff ) >> 0 ) );
        }

        SetActiveElementCharset( m_Project.Charsets[0], m_Project.Charsets[0].Colors.MultiColor1, m_Project.Charsets[0].Colors.MultiColor2, m_Project.CharsetProjects[0].Multicolor );
      }
      RedrawMap();
      Modified = false;
    }



    private void openToolStripMenuItem_Click( object sender, EventArgs e )
    {
      OpenFileDialog openFile = new OpenFileDialog();

      openFile.Title = "Open editor project";
      openFile.Filter = "Element Editor Project Files|*.elementeditorproject";

      if ( openFile.ShowDialog() == DialogResult.OK )
      {
        OpenProject( openFile.FileName );
        m_ProjectFilename = openFile.FileName;
      }
    }



    private void saveToolStripMenuItem_Click( object sender, EventArgs e )
    {
      if ( string.IsNullOrEmpty( m_ProjectFilename ) )
      {
        SaveFileDialog saveFile = new SaveFileDialog();

        saveFile.Title = "Save editor project";
        saveFile.Filter = "Element Editor Project Files|*.elementeditorproject";

        if ( saveFile.ShowDialog() == DialogResult.OK )
        {
          m_ProjectFilename = saveFile.FileName;
        }
        else
        {
          return;
        }
      }
      /*
      if ( !string.IsNullOrEmpty( m_Project.CharsetProjectFilename ) )
      {
        m_Project.CharsetProjectFilename = GR.Path.RelativePathTo( m_ProjectFilename, false, m_Project.CharsetProjectFilename, false );
      }
       */
      if ( !string.IsNullOrEmpty( m_Project.SpriteProjectFilename ) )
      {
        m_Project.SpriteProjectFilename = GR.Path.RelativePathTo( m_ProjectFilename, false, m_Project.SpriteProjectFilename, false );
      }
      if ( m_Project.SaveToFile( m_ProjectFilename ) )
      {
        Modified = false;
      }
    }



    private void btnNewElement_Click( object sender, EventArgs e )
    {
      int     elementWidth = GR.Convert.ToI32( editElementWidth.Text );
      int     elementHeight = GR.Convert.ToI32( editElementHeight.Text );

      Project.Element element = new Project.Element();

      element.Characters.Resize( elementWidth, elementHeight );
      element.Name = editElementName.Text;

      m_Project.Elements.Add( element );
      comboElements.Items.Add( element.Name );
      listAvailableElements.Items.Add( element.Name );
      Modified = true;
    }



    private void btnAddElementToScreen_Click( object sender, EventArgs e )
    {
      Project.Element  element = null;
      if ( comboElements.SelectedIndex != -1 )
      {
        string    elementName = (string)comboElements.SelectedItem;
        element = m_Project.Elements[comboElements.SelectedIndex];
      }
      Project.ScreenElement   screenElement = new Project.ScreenElement();

      screenElement.Index = comboElements.SelectedIndex;
      screenElement.X = GR.Convert.ToI32( editElementX.Text );
      screenElement.Y = GR.Convert.ToI32( editElementY.Text );
      screenElement.Repeats = GR.Convert.ToI32( editElementRepeat.Text );
      screenElement.Repeats2 = GR.Convert.ToI32( editElementRepeat2.Text );

      switch ( comboElementTypes.SelectedIndex )
      {
        case 0:
          screenElement.Type = Project.ScreenElementType.LD_ELEMENT;
          break;
        case 1:
          screenElement.Type = Project.ScreenElementType.LD_ELEMENT_LINE_H;
          break;
        case 2:
          screenElement.Type = Project.ScreenElementType.LD_ELEMENT_LINE_V;
          break;
        case 3:
          screenElement.Type = Project.ScreenElementType.LD_LINE_H;
          screenElement.Char = comboElementChar.SelectedIndex;
          screenElement.Color = comboElementColor.SelectedIndex;
          break;
        case 4:
          screenElement.Type = Project.ScreenElementType.LD_LINE_V;
          screenElement.Char = comboElementChar.SelectedIndex;
          screenElement.Color = comboElementColor.SelectedIndex;
          break;
        case 5:
          screenElement.Type = Project.ScreenElementType.LD_SEARCH_OBJECT;
          break;
        case 6:
          screenElement.Type = Project.ScreenElementType.LD_LINE_H_ALT;
          screenElement.Char = comboElementChar.SelectedIndex;
          screenElement.Color = comboElementColor.SelectedIndex;
          break;
        case 7:
          screenElement.Type = Project.ScreenElementType.LD_LINE_V_ALT;
          screenElement.Char = comboElementChar.SelectedIndex;
          screenElement.Color = comboElementColor.SelectedIndex;
          break;
        case 8:
          screenElement.Type = Project.ScreenElementType.LD_AREA;
          screenElement.Char = comboElementChar.SelectedIndex;
          screenElement.Color = comboElementColor.SelectedIndex;
          break;
        case 9:
          screenElement.Type = Project.ScreenElementType.LD_OBJECT;
          screenElement.Object = new Project.GameObject();
          break;
        case 10:
          screenElement.Type = Project.ScreenElementType.LD_SPAWN_SPOT;
          screenElement.Object = new Project.GameObject();
          break;
        case 11:
          screenElement.Type = Project.ScreenElementType.LD_ELEMENT_AREA;
          break;
        case 12:
          screenElement.Type = Project.ScreenElementType.LD_DOOR;
          break;
        case 13:
          screenElement.Type = Project.ScreenElementType.LD_CLUE;
          break;
        case 14:
          screenElement.Type = Project.ScreenElementType.LD_SPECIAL;
          break;
      }

      m_CurrentScreen.DisplayedElements.Add( screenElement );

      ListViewItem    item = new ListViewItem( m_CurrentScreen.DisplayedElements.Count.ToString() );

      if ( element != null )
      {
        item.SubItems.Add( element.Name );
      }
      else
      {
        item.SubItems.Add( "" );
      }
      item.SubItems.Add( screenElement.X.ToString() );
      item.SubItems.Add( screenElement.Y.ToString() );
      string  elType = "";
      switch ( screenElement.Type )
      {
        case Project.ScreenElementType.LD_ELEMENT:
          elType = "EL";
          break;
        case Project.ScreenElementType.LD_DOOR:
          elType = "DO";
          break;
        case Project.ScreenElementType.LD_CLUE:
          elType = "CL";
          break;
        case Project.ScreenElementType.LD_SPECIAL:
          elType = "SP";
          break;
        case Project.ScreenElementType.LD_ELEMENT_LINE_H:
          elType = "EH";
          break;
        case Project.ScreenElementType.LD_ELEMENT_LINE_V:
          elType = "EV";
          break;
        case Project.ScreenElementType.LD_LINE_H:
          elType = "LH";
          break;
        case Project.ScreenElementType.LD_LINE_V:
          elType = "LV";
          break;
        case Project.ScreenElementType.LD_SEARCH_OBJECT:
          elType = "SO";
          break;
        case Project.ScreenElementType.LD_LINE_H_ALT:
          elType = "AH";
          break;
        case Project.ScreenElementType.LD_LINE_V_ALT:
          elType = "AV";
          break;
        case Project.ScreenElementType.LD_AREA:
          elType = "AR";
          break;
        case Project.ScreenElementType.LD_OBJECT:
          elType = "OB";
          break;
        case Project.ScreenElementType.LD_SPAWN_SPOT:
          elType = "SS";
          break;
        case Project.ScreenElementType.LD_ELEMENT_AREA:
          elType = "EA";
          break;
      }
      item.SubItems.Add( elType );
      item.SubItems.Add( screenElement.Repeats.ToString() );
      item.Tag = screenElement;
      listScreenElements.Items.Add( item );
      listScreenElements.SelectedItems.Clear();
      item.Selected = true;

      RedrawScreen();
      Modified = true;
    }



    private void listAvailableElements_SelectedIndexChanged( object sender, EventArgs e )
    {
      btnDeleteElement.Enabled = ( listAvailableElements.SelectedIndex != -1 );
      btnCopyElement.Enabled = ( listAvailableElements.SelectedIndex != -1 );
      if ( listAvailableElements.SelectedIndex == -1 )
      {
        return;
      }
      string selItem = listAvailableElements.Items[listAvailableElements.SelectedIndex].ToString();
      m_CurrentEditedElement = m_Project.ElementFromString( selItem  );

      listElementChars.Items.Clear();
      for ( int j = 0; j < m_CurrentEditedElement.Characters.Height; ++j )
      {
        for ( int i = 0; i < m_CurrentEditedElement.Characters.Width; ++i )
        {
          ListViewItem    itemChar = new ListViewItem( ( i + j * m_CurrentEditedElement.Characters.Width ).ToString() );

          itemChar.SubItems.Add( m_CurrentEditedElement.Characters[i, j].Char.ToString() );
          itemChar.SubItems.Add( m_CurrentEditedElement.Characters[i, j].Color.ToString() );

          listElementChars.Items.Add( itemChar );
        }
      }
      editElementWidth.Text = m_CurrentEditedElement.Characters.Width.ToString();
      editElementHeight.Text = m_CurrentEditedElement.Characters.Height.ToString();
      editElementName.Text = m_CurrentEditedElement.Name;
      comboElementCharset.SelectedIndex = m_CurrentEditedElement.CharsetIndex;
      RedrawElementPreview();
    }



    private void comboChars_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( ( listElementChars.SelectedIndices.Count == 0 )
      ||   ( m_CurrentEditedElement == null ) )
      {
        return;
      }
      int    charIndex = listElementChars.SelectedIndices[0];

      if ( m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Char != (byte)comboChars.SelectedIndex )
      {
        m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Char = (byte)comboChars.SelectedIndex;
        listElementChars.Items[charIndex].SubItems[1].Text = comboChars.SelectedIndex.ToString();
        pictureCharset.SelectedIndex = comboChars.SelectedIndex;
        RedrawElementPreview();
        comboColor.Invalidate();
        Modified = true;
      }
    }



    private void comboColor_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( ( listElementChars.SelectedIndices.Count == 0 )
      ||   ( m_CurrentEditedElement == null ) )
      {
        return;
      }
      int    charIndex = listElementChars.SelectedIndices[0];

      if ( m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Color != (byte)comboColor.SelectedIndex )
      {
        m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Color = (byte)comboColor.SelectedIndex;
        listElementChars.Items[charIndex].SubItems[2].Text = comboColor.SelectedIndex.ToString();
        RedrawElementPreview();
        comboChars.Invalidate();
        Modified = true;
      }
    }



    private void listElementChars_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( ( listElementChars.SelectedIndices.Count == 0 )
      ||   ( m_CurrentEditedElement == null ) )
      {
        return;
      }
      int    charIndex = listElementChars.SelectedIndices[0];
      comboColor.SelectedIndex = m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Color;
      comboChars.SelectedIndex = m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Char;

      pictureCharset.SelectedIndex = comboChars.SelectedIndex;
    }



    private void listScreenElements_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( m_CurrentScreen == null )
      {
        return;
      }
      btnDelElement.Enabled = ( listScreenElements.SelectedIndices.Count > 0 );
      btnCopyElement.Enabled = ( listScreenElements.SelectedIndices.Count > 0 );

      bool  canMoveDown = false;
      if ( ( listScreenElements.SelectedIndices.Count > 0 )
      && ( listScreenElements.SelectedIndices[0] + 1 < listScreenElements.Items.Count ) )
      {
        canMoveDown = true;
      }
      btnElementDown.Enabled = canMoveDown;

      bool  canMoveUp = false;
      if ( ( listScreenElements.SelectedIndices.Count > 0 )
      &&   ( listScreenElements.SelectedIndices[0] > 0 ) )
      {
        canMoveUp = true;
      }
      btnElementUp.Enabled = canMoveUp;

      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int   elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];
      Project.Element element = null;
      if ( screenElement.Index != -1 )
      {
        element = m_Project.Elements[screenElement.Index];
      }

      comboScreenObjectFlags.SelectedIndex = screenElement.Flags;

      int     selIndex = -1;
      switch ( screenElement.Type )
      {
        case Project.ScreenElementType.LD_ELEMENT:
          selIndex = 0;
          break;
        case Project.ScreenElementType.LD_ELEMENT_LINE_H:
          selIndex = 1;
          break;
        case Project.ScreenElementType.LD_ELEMENT_LINE_V:
          selIndex = 2;
          break;
        case Project.ScreenElementType.LD_LINE_H:
          selIndex = 3;
          comboElementChar.SelectedIndex = screenElement.Char;
          comboElementColor.SelectedIndex = screenElement.Color;
          break;
        case Project.ScreenElementType.LD_LINE_V:
          selIndex = 4;
          comboElementChar.SelectedIndex = screenElement.Char;
          comboElementColor.SelectedIndex = screenElement.Color;
          break;
        case Project.ScreenElementType.LD_SEARCH_OBJECT:
          selIndex = 5;
          editElementRepeat.Text = screenElement.SearchObjectIndex.ToString();
          break;
        case Project.ScreenElementType.LD_LINE_H_ALT:
          selIndex = 6;
          comboElementChar.SelectedIndex = screenElement.Char;
          comboElementColor.SelectedIndex = screenElement.Color;
          break;
        case Project.ScreenElementType.LD_LINE_V_ALT:
          selIndex = 7;
          comboElementChar.SelectedIndex = screenElement.Char;
          comboElementColor.SelectedIndex = screenElement.Color;
          break;
        case Project.ScreenElementType.LD_AREA:
          selIndex = 8;
          comboElementChar.SelectedIndex = screenElement.Char;
          comboElementColor.SelectedIndex = screenElement.Color;
          break;
        case Project.ScreenElementType.LD_OBJECT:
          selIndex = 9;
          comboElementColor.SelectedIndex = screenElement.Object.Color;
          editObjectBorderBottom.Text = screenElement.Object.MoveBorderBottom.ToString();
          editObjectBorderLeft.Text = screenElement.Object.MoveBorderLeft.ToString();
          editObjectBorderRight.Text = screenElement.Object.MoveBorderRight.ToString();
          editObjectBorderTop.Text = screenElement.Object.MoveBorderTop.ToString();
          editObjectSpeed.Text = screenElement.Object.Speed.ToString();
          editObjectData.Text = screenElement.Object.Data.ToString();
          comboObjects.SelectedIndex = screenElement.Object.TemplateIndex;
          if ( comboObjectBehaviour.Items.Count > screenElement.Object.Behaviour )
          {
            comboObjectBehaviour.SelectedIndex = screenElement.Object.Behaviour;
          }
          comboObjectOptional.SelectedIndex = (int)screenElement.Object.Optional;
          editObjectOptionalOn.Text = screenElement.Object.OptionalValue.ToString();
          break;
        case Project.ScreenElementType.LD_SPAWN_SPOT:
          selIndex = 10;
          comboElementColor.SelectedIndex = screenElement.Object.Color;
          comboObjects.SelectedIndex = screenElement.Object.TemplateIndex;
          if ( comboObjectBehaviour.Items.Count > screenElement.Object.Behaviour )
          {
            comboObjectBehaviour.SelectedIndex = screenElement.Object.Behaviour;
          }
          break;
        case Project.ScreenElementType.LD_ELEMENT_AREA:
          selIndex = 11;
          break;
        case Project.ScreenElementType.LD_DOOR:
          selIndex = 12;
          break;
        case Project.ScreenElementType.LD_CLUE:
          selIndex = 13;
          break;
        case Project.ScreenElementType.LD_SPECIAL:
          selIndex = 14;
          break;
      }
      comboElementTypes.SelectedIndex = selIndex;
      if ( ( screenElement.Type != Project.ScreenElementType.LD_LINE_H )
      && ( screenElement.Type != Project.ScreenElementType.LD_LINE_V )
      && ( screenElement.Type != Project.ScreenElementType.LD_LINE_H_ALT )
      && ( screenElement.Type != Project.ScreenElementType.LD_LINE_V_ALT )
      && ( screenElement.Type != Project.ScreenElementType.LD_AREA ) )
      {
        comboElements.SelectedIndex = screenElement.Index;
      }

      if ( screenElement.Type != Project.ScreenElementType.LD_SEARCH_OBJECT )
      {
        editElementRepeat.Text = screenElement.Repeats.ToString();
        editElementRepeat2.Text = screenElement.Repeats2.ToString();
      }
      editElementX.Text = screenElement.X.ToString();
      editElementY.Text = screenElement.Y.ToString();
      RedrawScreen();
    }



    private void editElementX_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int   elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( screenElement.X != GR.Convert.ToI32( editElementX.Text ) )
      {
        screenElement.X = GR.Convert.ToI32( editElementX.Text );
        listScreenElements.Items[elementIndex].SubItems[2].Text = editElementX.Text;
        Modified = true;
        RedrawScreen();
      }
    }



    private void editElementY_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int   elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( screenElement.Y != GR.Convert.ToI32( editElementY.Text ) )
      {
        screenElement.Y = GR.Convert.ToI32( editElementY.Text );
        listScreenElements.Items[elementIndex].SubItems[3].Text = editElementY.Text;
        Modified = true;
        RedrawScreen();
      }
    }



    private void editElementRepeat_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int   elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( screenElement.Type == Project.ScreenElementType.LD_SEARCH_OBJECT )
      {
        if ( screenElement.SearchObjectIndex != GR.Convert.ToI32( editElementRepeat.Text ) )
        {
          screenElement.SearchObjectIndex = GR.Convert.ToI32( editElementRepeat.Text );
          RedrawScreen();
          Modified = true;
        }
      }
      else
      {
        if ( screenElement.Repeats != GR.Convert.ToI32( editElementRepeat.Text ) )
        {
          screenElement.Repeats = GR.Convert.ToI32( editElementRepeat.Text );
          listScreenElements.Items[elementIndex].SubItems[5].Text = editElementRepeat.Text;
          RedrawScreen();
          Modified = true;
        }
      }
    }



    private void comboElements_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int   elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];
      if ( screenElement.Index != comboElements.SelectedIndex )
      {
        screenElement.Index = comboElements.SelectedIndex;
        listScreenElements.Items[elementIndex].SubItems[1].Text = m_Project.Elements[screenElement.Index].Name;
        Modified = true;
        RedrawScreen();
      }
    }



    private void comboElementTypes_SelectedIndexChanged( object sender, EventArgs e )
    {
      Project.ScreenElement screenElement = null;
      int   elementIndex = -1;
      if ( listScreenElements.SelectedIndices.Count != 0 )
      {
        elementIndex = listScreenElements.SelectedIndices[0];
        screenElement = m_CurrentScreen.DisplayedElements[elementIndex];
      }

      labelElementRepeats.Text = "Repeats:";
      switch ( comboElementTypes.SelectedIndex )
      {
        case 0:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_ELEMENT )
            {
              screenElement.Type = Project.ScreenElementType.LD_ELEMENT;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "EL";
          }
          labelElementRepeats.Enabled = false;
          editElementRepeat.Enabled = false;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = false;
          comboElementChar.Enabled = false;
          labelElementColor.Enabled = false;
          comboElementColor.Enabled = false;
          break;
        case 1:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_ELEMENT_LINE_H )
            {
              screenElement.Type = Project.ScreenElementType.LD_ELEMENT_LINE_H;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "EH";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = false;
          comboElementChar.Enabled = false;
          labelElementColor.Enabled = false;
          comboElementColor.Enabled = false;
          break;
        case 2:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_ELEMENT_LINE_V )
            {
              screenElement.Type = Project.ScreenElementType.LD_ELEMENT_LINE_V;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "EV";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = false;
          comboElementChar.Enabled = false;
          labelElementColor.Enabled = false;
          comboElementColor.Enabled = false;
          break;
        case 3:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_LINE_H )
            {
              screenElement.Type = Project.ScreenElementType.LD_LINE_H;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "LH";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = true;
          comboElementChar.Enabled = true;
          labelElementColor.Enabled = true;
          comboElementColor.Enabled = true;
          break;
        case 4:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_LINE_V )
            {
              screenElement.Type = Project.ScreenElementType.LD_LINE_V;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "LV";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = true;
          comboElementChar.Enabled = true;
          labelElementColor.Enabled = true;
          comboElementColor.Enabled = true;
          break;
        case 5:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_SEARCH_OBJECT )
            {
              screenElement.Type = Project.ScreenElementType.LD_SEARCH_OBJECT;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "SO";
          }
          labelElementRepeats.Enabled = true;
          labelElementRepeats.Text = "Index";
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = false;
          comboElementChar.Enabled = false;
          labelElementColor.Enabled = false;
          comboElementColor.Enabled = false;
          break;
        case 6:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_LINE_H_ALT )
            {
              screenElement.Type = Project.ScreenElementType.LD_LINE_H_ALT;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "AH";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = true;
          comboElementChar.Enabled = true;
          labelElementColor.Enabled = true;
          comboElementColor.Enabled = true;
          break;
        case 7:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_LINE_V_ALT )
            {
              screenElement.Type = Project.ScreenElementType.LD_LINE_V_ALT;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "AV";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = true;
          comboElementChar.Enabled = true;
          labelElementColor.Enabled = true;
          comboElementColor.Enabled = true;
          break;
        case 8:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_AREA )
            {
              screenElement.Type = Project.ScreenElementType.LD_AREA;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "Area";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = true;
          labelElementChar.Enabled = true;
          comboElementChar.Enabled = true;
          labelElementColor.Enabled = true;
          comboElementColor.Enabled = true;
          break;
        case 9:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_OBJECT )
            {
              screenElement.Type = Project.ScreenElementType.LD_OBJECT;
              screenElement.Object = new Project.GameObject();
              if ( screenElement.Object.TemplateIndex != -1 )
              {
                screenElement.Object.SpriteImage = new GR.Image.MemoryImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].StartSprite].Tile.Image );
              }
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "OB";
          }
          labelElementRepeats.Enabled = false;
          editElementRepeat.Enabled = false;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = false;
          comboElementChar.Enabled = false;
          labelElementColor.Enabled = true;
          comboElementColor.Enabled = true;
          labelObjectBorderLeft.Text = "BorderLeft";
          labelObjectBorderTop.Text = "BorderTop";
          editObjectData.Enabled = true;
          if ( screenElement != null )
          {
            editObjectBorderLeft.Text = screenElement.Object.MoveBorderLeft.ToString();
            editObjectBorderTop.Text = screenElement.Object.MoveBorderTop.ToString();
            editObjectBorderRight.Text = screenElement.Object.MoveBorderRight.ToString();
            editObjectBorderBottom.Text = screenElement.Object.MoveBorderBottom.ToString();
            editObjectSpeed.Text = screenElement.Object.Speed.ToString();
            editObjectData.Text = screenElement.Object.Data.ToString();
            comboElementColor.SelectedIndex = screenElement.Object.Color;
          }
          break;
        case 10:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_SPAWN_SPOT )
            {
              screenElement.Type = Project.ScreenElementType.LD_SPAWN_SPOT;
              screenElement.Object = new Project.GameObject();
              if ( screenElement.Index >= m_Project.ObjectTemplates.Count )
              {
                screenElement.Index = 0;
              }
              if ( screenElement.Index < m_Project.ObjectTemplates.Count )
              {
                screenElement.Object.SpriteImage = new GR.Image.MemoryImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[screenElement.Index].StartSprite].Tile.Image );
              }
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "SS";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = false;
          break;
        case 11:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_ELEMENT_AREA )
            {
              screenElement.Type = Project.ScreenElementType.LD_ELEMENT_AREA;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "EA";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = true;
          labelElementChar.Enabled = true;
          comboElementChar.Enabled = true;
          labelElementColor.Enabled = true;
          comboElementColor.Enabled = true;
          break;
        case 12:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_DOOR )
            {
              screenElement.Type = Project.ScreenElementType.LD_DOOR;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "DO";
          }
          labelElementRepeats.Enabled = false;
          editElementRepeat.Enabled = false;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = false;
          comboElementChar.Enabled = false;
          labelElementColor.Enabled = false;
          comboElementColor.Enabled = false;
          labelObjectBorderLeft.Text = "TargetX";
          labelObjectBorderTop.Text = "TargetY";
          labelObjectBorderRight.Text = "TargetLevel";
          if ( screenElement != null )
          {
            editObjectBorderLeft.Text = screenElement.TargetX.ToString();
            editObjectBorderTop.Text = screenElement.TargetY.ToString();
            editObjectBorderRight.Text = screenElement.TargetLevel.ToString();
          }
          break;
        case 13:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_CLUE )
            {
              screenElement.Type = Project.ScreenElementType.LD_CLUE;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "CL";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = false;
          comboElementChar.Enabled = false;
          labelElementColor.Enabled = false;
          comboElementColor.Enabled = false;
          break;
        case 14:
          if ( screenElement != null )
          {
            if ( screenElement.Type != Project.ScreenElementType.LD_SPECIAL )
            {
              screenElement.Type = Project.ScreenElementType.LD_SPECIAL;
              screenElement.Object = null;
              Modified = true;
            }
            listScreenElements.Items[elementIndex].SubItems[4].Text = "SP";
          }
          labelElementRepeats.Enabled = true;
          editElementRepeat.Enabled = true;
          editElementRepeat2.Enabled = false;
          labelElementChar.Enabled = false;
          comboElementChar.Enabled = false;
          labelElementColor.Enabled = false;
          comboElementColor.Enabled = false;
          break;
      }

      Project.ScreenElementType type = (Project.ScreenElementType)comboElementTypes.SelectedIndex;

      labelObject.Enabled = ( ( type == Project.ScreenElementType.LD_OBJECT ) || ( type == Project.ScreenElementType.LD_SPAWN_SPOT ) );
      comboObjects.Enabled = ( ( type == Project.ScreenElementType.LD_OBJECT ) || ( type == Project.ScreenElementType.LD_SPAWN_SPOT ) );
      comboObjectBehaviour.Enabled = ( type == Project.ScreenElementType.LD_OBJECT );
      comboObjectOptional.Enabled = ( type == Project.ScreenElementType.LD_OBJECT );
      editObjectOptionalOn.Enabled = ( type == Project.ScreenElementType.LD_OBJECT );
      labelObjectBorderLeft.Enabled = ( type == Project.ScreenElementType.LD_OBJECT ) | ( type == Project.ScreenElementType.LD_DOOR );
      editObjectBorderLeft.Enabled = ( type == Project.ScreenElementType.LD_OBJECT ) | ( type == Project.ScreenElementType.LD_DOOR );
      labelObjectBorderTop.Enabled = ( type == Project.ScreenElementType.LD_OBJECT ) | ( type == Project.ScreenElementType.LD_DOOR );
      editObjectBorderTop.Enabled = ( type == Project.ScreenElementType.LD_OBJECT ) | ( type == Project.ScreenElementType.LD_DOOR );
      labelObjectBorderRight.Enabled = ( type == Project.ScreenElementType.LD_OBJECT ) | ( type == Project.ScreenElementType.LD_DOOR );
      editObjectBorderRight.Enabled = ( type == Project.ScreenElementType.LD_OBJECT ) | ( type == Project.ScreenElementType.LD_DOOR );
      labelObjectBorderBottom.Enabled = ( type == Project.ScreenElementType.LD_OBJECT );
      editObjectBorderBottom.Enabled = ( type == Project.ScreenElementType.LD_OBJECT );
      labelObjectSpeed.Enabled = ( type == Project.ScreenElementType.LD_OBJECT );
      editObjectSpeed.Enabled = ( type == Project.ScreenElementType.LD_OBJECT );
      labelObjectData.Enabled = ( type == Project.ScreenElementType.LD_OBJECT );
      editObjectData.Enabled = ( type == Project.ScreenElementType.LD_OBJECT );
      if ( Modified )
      {
        RedrawScreen();
      }
    }



    private void editElementName_TextChanged( object sender, EventArgs e )
    {
      if ( listAvailableElements.SelectedIndex == -1 )
      {
        return;
      }
      Project.Element   element = m_Project.Elements[listAvailableElements.SelectedIndex];

      string oldName = element.Name;
      if ( oldName != editElementName.Text )
      {
        element.Name = editElementName.Text;

        // update all other views
        listAvailableElements.Items[listAvailableElements.SelectedIndex] = element.Name;
        comboElements.Items[listAvailableElements.SelectedIndex] = element.Name;

        foreach ( ListViewItem item in listScreenElements.Items )
        {
          if ( item.SubItems[1].Text == oldName )
          {
            item.SubItems[1].Text = element.Name;
          }
        }
        Modified = true;
      }
    }



    private void editElementWidth_TextChanged( object sender, EventArgs e )
    {
      if ( listAvailableElements.SelectedIndex == -1 )
      {
        return;
      }
      Project.Element   element = m_Project.Elements[listAvailableElements.SelectedIndex];

      int     newWidth = GR.Convert.ToI32( editElementWidth.Text );
      if ( newWidth < 1 )
      {
        newWidth = 1;
      }
      if ( newWidth != element.Characters.Width )
      {
        element.Characters.Resize( newWidth, element.Characters.Height );

        // reselect item
        listAvailableElements_SelectedIndexChanged( sender, e );
        Modified = true;
        RedrawScreen();
        RedrawElementPreview();
      }
    }



    private void editElementHeight_TextChanged( object sender, EventArgs e )
    {
      if ( listAvailableElements.SelectedIndex == -1 )
      {
        return;
      }
      Project.Element   element = m_Project.Elements[listAvailableElements.SelectedIndex];

      int     newHeight = GR.Convert.ToI32( editElementHeight.Text );
      if ( newHeight < 1 )
      {
        newHeight = 1;
      }
      if ( element.Characters.Height != newHeight )
      {
        element.Characters.Resize( element.Characters.Width, newHeight );

        // reselect item
        listAvailableElements_SelectedIndexChanged( sender, e );
        Modified = true;
        RedrawScreen();
        RedrawElementPreview();
      }
    }



    private void btnAutoInc_Click( object sender, EventArgs e )
    {
      if ( ( listElementChars.SelectedIndices.Count == 0 )
      ||   ( m_CurrentEditedElement == null ) )
      {
        return;
      }
      int    charIndex = listElementChars.SelectedIndices[0];

      if ( charIndex + 1 < m_CurrentEditedElement.Characters.Width * m_CurrentEditedElement.Characters.Height )
      {
        int   newCharIndex = charIndex + 1;
        m_CurrentEditedElement.Characters[newCharIndex % m_CurrentEditedElement.Characters.Width, newCharIndex / m_CurrentEditedElement.Characters.Width].Char = (byte)( m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Char + 1 );
        m_CurrentEditedElement.Characters[newCharIndex % m_CurrentEditedElement.Characters.Width, newCharIndex / m_CurrentEditedElement.Characters.Width].Color = m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Color;

        listElementChars.Items[newCharIndex].SubItems[1].Text = m_CurrentEditedElement.Characters[newCharIndex % m_CurrentEditedElement.Characters.Width, newCharIndex / m_CurrentEditedElement.Characters.Width].Char.ToString();
        listElementChars.Items[newCharIndex].SubItems[2].Text = m_CurrentEditedElement.Characters[newCharIndex % m_CurrentEditedElement.Characters.Width, newCharIndex / m_CurrentEditedElement.Characters.Width].Color.ToString();

        listElementChars.SelectedIndices.Remove( charIndex );
        listElementChars.SelectedIndices.Add( charIndex + 1 );
      }
      Modified = true;
      RedrawElementPreview();
    }



    private void btnElementDown_Click( object sender, EventArgs e )
    {
      if ( ( listScreenElements.SelectedIndices.Count > 0 )
      &&   ( listScreenElements.SelectedIndices[0] + 1 < m_CurrentScreen.DisplayedElements.Count ) )
      {
        int     origIndex = listScreenElements.SelectedIndices[0];

        Project.ScreenElement   elementToMove = m_CurrentScreen.DisplayedElements[origIndex];

        m_CurrentScreen.DisplayedElements.RemoveAt( origIndex );
        m_CurrentScreen.DisplayedElements.Insert( origIndex + 1, elementToMove );

        ListViewItem            itemToMove = listScreenElements.Items[origIndex];
        listScreenElements.Items.RemoveAt( origIndex );
        listScreenElements.Items.Insert( origIndex + 1, itemToMove );

        itemToMove.Text = itemToMove.Index.ToString();
        listScreenElements.Items[origIndex].Text = origIndex.ToString();

        itemToMove.Selected = true;

        Modified = true;
        RedrawScreen();
      }
    }



    private void btnElementUp_Click( object sender, EventArgs e )
    {
      if ( ( listScreenElements.SelectedIndices.Count > 0 )
      &&   ( listScreenElements.SelectedIndices[0] > 0 ) )
      {
        int     origIndex = listScreenElements.SelectedIndices[0];

        Project.ScreenElement   elementToMove = m_CurrentScreen.DisplayedElements[origIndex];

        m_CurrentScreen.DisplayedElements.RemoveAt( origIndex );
        m_CurrentScreen.DisplayedElements.Insert( origIndex - 1, elementToMove );

        ListViewItem            itemToMove = listScreenElements.Items[origIndex];
        listScreenElements.Items.RemoveAt( origIndex );
        listScreenElements.Items.Insert( origIndex - 1, itemToMove );

        itemToMove.Text = ( origIndex - 1 ).ToString();
        listScreenElements.Items[origIndex].Text = origIndex.ToString();

        itemToMove.Selected = true;

        Modified = true;
        RedrawScreen();
      }
    }



    private void editScreenName_TextChanged( object sender, EventArgs e )
    {
      if ( m_CurrentScreen != null )
      {
        if ( m_CurrentScreen.Name != editScreenName.Text )
        {
          m_CurrentScreen.Name = editScreenName.Text;

          int selIndex = comboScreens.SelectedIndex;

          ( (GR.Generic.Tupel<string, Project.Screen>)comboScreens.SelectedItem ).first = selIndex.ToString() + ":" + m_CurrentScreen.Name;
          comboScreens.Items[selIndex] = comboScreens.Items[selIndex];
          comboScreens.SelectedIndex = selIndex;

          ( (GR.Generic.Tupel<string, Project.Screen>)comboRegionScreens.Items[selIndex] ).first = selIndex.ToString() + ":" + m_CurrentScreen.Name;
          comboRegionScreens.Items[selIndex] = comboRegionScreens.Items[selIndex];
           
          Modified = true;
        }
      }
    }



    private void comboElementChar_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int   elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      screenElement.Char = (byte)comboElementChar.SelectedIndex;
      comboElementColor.Invalidate();
      Modified = true;
      RedrawScreen();
    }



    private void comboElementColor_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int   elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( screenElement.Color != (byte)comboElementColor.SelectedIndex )
      {
        screenElement.Color = (byte)comboElementColor.SelectedIndex;
        if ( screenElement.Object != null )
        {
          screenElement.Object.Color = screenElement.Color;
          if ( screenElement.Object.TemplateIndex != -1 )
          {
            RebuildSpriteImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].StartSprite].Tile,
                                m_SpriteProject.Colors.Palette, 
                                screenElement.Object.SpriteImage,
                                m_SpriteProject.Sprites[m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].StartSprite].Mode,
                                screenElement.Object.Color );
          }
        }
        Modified = true;
        comboElementChar.Invalidate();
        RedrawScreen();
      }
    }



    private void btnExportBrowse_Click( object sender, EventArgs e )
    {
      SaveFileDialog saveFile = new SaveFileDialog();

      saveFile.Title = "Choose target file name";
      saveFile.Filter = "ASM Files|*.asm";

      if ( saveFile.ShowDialog() == DialogResult.OK )
      {
        editExportFile.Text = saveFile.FileName;
      }
    }



    private void editExportPrefix_TextChanged( object sender, EventArgs e )
    {
      if ( m_Project.ExportPrefix != editExportPrefix.Text )
      {
        m_Project.ExportPrefix = editExportPrefix.Text;
        Modified = true;
      }
    }



    private void editConstantOffset_TextChanged( object sender, EventArgs e )
    {
      if ( m_Project.ExportConstantOffset != GR.Convert.ToI32( editConstantOffset.Text ) )
      {
        m_Project.ExportConstantOffset = GR.Convert.ToI32( editConstantOffset.Text );
        Modified = true;
      }
    }



    private void editExportFile_TextChanged( object sender, EventArgs e )
    {
      if ( m_Project.ExportFilename != editExportFile.Text )
      {
        m_Project.ExportFilename = editExportFile.Text;
        Modified = true;
      }
    }



    private void CollapseBuffers( System.Collections.Generic.List<DataInfo> Buffers )
    {
      redo_from_start:;

      foreach ( DataInfo data in Buffers )
      {
        foreach ( DataInfo otherData in Buffers )
        {
          if ( data != otherData )
          {
            GR.Memory.ByteBuffer    sourceBuffer = data.Data;
            if ( ( data.ReplacementData != null )
            ||   ( otherData.ReplacementData != null ) )
            {
              // don't try again
              continue;
            }
            GR.Memory.ByteBuffer    targetBuffer = otherData.Data;
            if ( otherData.ReplacementData != null )
            {
              targetBuffer = otherData.ReplacementData.Data.SubBuffer( otherData.ReplacementOffset, (int)data.Data.Length );
            }
            // is source buffer contained in target buffer?
            if ( targetBuffer.Length < sourceBuffer.Length )
            {
              continue;
            }
            for ( int i = 0; i <= targetBuffer.Length - sourceBuffer.Length; ++i )
            {
              bool  isSame = true;
              for ( int j = 0; j < sourceBuffer.Length; ++j )
              {
                if ( sourceBuffer.ByteAt( j ) != targetBuffer.ByteAt( i + j ) )
                {
                  isSame = false;
                  break;
                }
              }
              if ( isSame )
              {
                //Debug.Log( "Replace data for " + data.Name + " with " + otherData.Name + ", Offset " + i.ToString() );

                // replace other replacement datas if the targetdata was just replaced
                foreach ( DataInfo previousData in Buffers )
                {
                  if ( previousData.ReplacementData == data )
                  {
                    previousData.ReplacementData = otherData;
                    previousData.ReplacementOffset += i;
                  }
                }

                data.ReplacementData = otherData;
                data.ReplacementOffset = i;
                goto redo_from_start;
              }
            }
          }
        }
      }

      // try to fit parts in other ends
      foreach ( DataInfo data in Buffers )
      {
        if ( ( data.ReplacementData != null )
        ||   ( ( data.PreviousData != null )
        &&     ( data.NextData != null ) ) )
        {
          continue;
        }

        int         maxNumBytesL = 0;
        DataInfo    bestFitL = null;
        int         maxNumBytesR = 0;
        DataInfo    bestFitR = null;

        foreach ( DataInfo otherData in Buffers )
        {
          if ( ( otherData.ReplacementData != null )
          ||   ( data == otherData )
          ||   ( ( otherData.PreviousData != null )
          &&     ( otherData.NextData != null ) ) )
          {
            continue;
          }
          // could otherDatabe chained to us via more than one jump?
          DataInfo    prevData = otherData.PreviousData;
          DataInfo    nextData = otherData.NextData;
          bool        chainConnected = false;

          while ( prevData != null )
          {
            if ( prevData == data )
            {
              chainConnected = true;
              break;
            }
            prevData = prevData.PreviousData;
          }

          while ( nextData != null )
          {
            if ( nextData == data )
            {
              chainConnected = true;
              break;
            }
            nextData = nextData.NextData;
          }
          if ( chainConnected )
          {
            // already connected
            continue;
          }

          // both buffers could be connected
          if ( ( data.PreviousData == null )
          &&   ( otherData.NextData == null ) )
          {
            // check to left
            int     numBytesLeft = 0;

            while ( ( numBytesLeft < data.Data.Length )
            &&      ( numBytesLeft < otherData.Data.Length ) )
            {
              if ( otherData.Data.SubBuffer( (int)otherData.Data.Length - numBytesLeft - 1 ).Compare( data.Data.SubBuffer( 0, numBytesLeft + 1 ) ) == 0 )
              {
                ++numBytesLeft;
              }
              else
              {
                break;
              }
            }
            if ( numBytesLeft > maxNumBytesL )
            {
              maxNumBytesL = numBytesLeft;
              bestFitL = otherData;
            }
          }
          if ( ( data.NextData == null )
          &&   ( otherData.PreviousData == null ) )
          {
            // check to right
            int     numBytesRight = 0;

            while ( ( numBytesRight < data.Data.Length )
            &&      ( numBytesRight < otherData.Data.Length ) )
            {
              if ( data.Data.SubBuffer( (int)data.Data.Length - numBytesRight - 1 ).Compare( otherData.Data.SubBuffer( 0, numBytesRight + 1 ) ) == 0 )
              {
                ++numBytesRight;
              }
              else
              {
                break;
              }
            }
            if ( numBytesRight > maxNumBytesR )
            {
              maxNumBytesR = numBytesRight;
              bestFitR = otherData;
            }
          }

          if ( ( maxNumBytesL != 0 )
          ||   ( maxNumBytesR != 0 ) )
          {
            if ( maxNumBytesL > maxNumBytesR )
            {
              if ( data.PreviousData != null )
              {
                Debug.Log( "overwriting previousdata " + data.PreviousData.Name + " with " + bestFitL.Name );
              }
              if ( bestFitL.NextData != null )
              {
                Debug.Log( "overwriting nextdata " + bestFitL.NextData.Name + " with " + data.Name );
              }

              data.PreviousData = bestFitL;
              data.OffsetInPreviousData = maxNumBytesL;
              bestFitL.NextData = data;

              data.ReplacementData = bestFitL;
              data.ReplacementOffset = (int)bestFitL.Data.Length - maxNumBytesL;
              Debug.Log( "Shifted " + data.Name + " inside " + bestFitL.Name + ", " + maxNumBytesL + " bytes from left" );
            }
            else
            {
              if ( bestFitR.PreviousData != null )
              {
                Debug.Log( "overwriting previousdata " + bestFitR.PreviousData.Name + " with " + data.Name );
              }
              if ( data.NextData != null )
              {
                Debug.Log( "overwriting nextdata " + data.NextData.Name + " with " + bestFitR.Name );
              }

              bestFitR.PreviousData = data;
              bestFitR.OffsetInPreviousData = maxNumBytesR;
              data.NextData = bestFitR;

              bestFitR.ReplacementData = data;
              bestFitR.ReplacementOffset = maxNumBytesR;

              Debug.Log( "Shifted " + data.Name + " inside " + bestFitR.Name + ", " + maxNumBytesR + " bytes from right" );
            }
            break;
          }
        }
      }
    }



    private string SanitizeName( Project.Element Element )
    {
      return Element.Name.Replace( ' ', '_' ).Replace( '-', '_' ).Replace( '-', '_' ).ToUpper();
    }



    private string SanitizeName( Project.ObjectTemplate Obj )
    {
      return Obj.Name.Replace( ' ', '_' ).Replace( '-', '_' ).Replace( '.', '_' ).ToUpper();
    }



    private string ExportElementTable( Project prj, List<DataInfo> elementDatas, int dataOffset )
    {
      // element table LO
      string result = prj.ExportPrefix + "ELEMENT_TABLE_LO\r\n";
      int elementIndex = dataOffset;
      foreach ( Project.Element element in prj.Elements )
      {
        if ( elementDatas[elementIndex].ReplacementData == null )
        {
          result += "!byte <( DATA_EL_" + prj.ExportPrefix + "_" + SanitizeName( element ) + " )\r\n";
        }
        else if ( elementDatas[elementIndex].PreviousData != null )
        {
          result += "!byte <( " + elementDatas[elementIndex].PreviousData.Name + "+" + ( elementDatas[elementIndex].PreviousData.Data.Length - elementDatas[elementIndex].OffsetInPreviousData ) + " )\r\n";
        }
        else
        {
          result += "!byte <( " + elementDatas[elementIndex].ReplacementData.Name + "+" + elementDatas[elementIndex].ReplacementOffset + " )\r\n";
        }
        ++elementIndex;
      }
      result += "\r\n";

      // element table HI
      result += prj.ExportPrefix + "ELEMENT_TABLE_HI\r\n";
      elementIndex = dataOffset;
      foreach ( Project.Element element in prj.Elements )
      {
        if ( elementDatas[elementIndex].ReplacementData == null )
        {
          result += "!byte >( DATA_EL_" + prj.ExportPrefix + "_" + SanitizeName( element ) + " )\r\n";
        }
        else if ( elementDatas[elementIndex].PreviousData != null )
        {
          result += "!byte >( " + elementDatas[elementIndex].PreviousData.Name + "+" + ( elementDatas[elementIndex].PreviousData.Data.Length - elementDatas[elementIndex].OffsetInPreviousData ) + " )\r\n";
        }
        else
        {
          result += "!byte >( " + elementDatas[elementIndex].ReplacementData.Name + "+" + elementDatas[elementIndex].ReplacementOffset + " )\r\n";
        }
        ++elementIndex;
      }
      result += "\r\n";
      return result;
    }



    private string ExportElementColorTable( Project prj, List<DataInfo> elementColor, int dataOffset )
    {
      // element color table LO
      string result = prj.ExportPrefix + "ELEMENT_COLOR_TABLE_LO\r\n";
      int elementIndex = dataOffset;
      foreach ( Project.Element element in prj.Elements )
      {
        if ( elementIndex == 36 )
        {
          Debug.Log( "aha" );
        }
        if ( elementColor[elementIndex].ReplacementData == null )
        {
          result += "!byte <( COLOR_EL_" + prj.ExportPrefix + "_" + SanitizeName( element ) + " )\r\n";
        }
        else if ( elementColor[elementIndex].PreviousData != null )
        {
          result += "!byte <( " + elementColor[elementIndex].PreviousData.Name + "+" + ( elementColor[elementIndex].PreviousData.Data.Length - elementColor[elementIndex].OffsetInPreviousData ) + " )\r\n";
        }
        else
        {
          result += "!byte <( " + elementColor[elementIndex].ReplacementData.Name + "+" + elementColor[elementIndex].ReplacementOffset + " )\r\n";
        }
        ++elementIndex;
      }
      result += "\r\n";

      // element color table HI
      result += prj.ExportPrefix + "ELEMENT_COLOR_TABLE_HI\r\n";
      elementIndex = dataOffset;
      foreach ( Project.Element element in prj.Elements )
      {
        if ( elementColor[elementIndex].ReplacementData == null )
        {
          result += "!byte >( COLOR_EL_" + prj.ExportPrefix + "_" + SanitizeName( element ) + " )\r\n";
        }
        else if ( elementColor[elementIndex].PreviousData != null )
        {
          result += "!byte >( " + elementColor[elementIndex].PreviousData.Name + "+" + ( elementColor[elementIndex].PreviousData.Data.Length - elementColor[elementIndex].OffsetInPreviousData ) + " )\r\n";
        }
        else
        {
          result += "!byte >( " + elementColor[elementIndex].ReplacementData.Name + "+" + elementColor[elementIndex].ReplacementOffset + " )\r\n";
        }
        ++elementIndex;
      }
      result += "\r\n\r\n";
      return result;
    }



    private bool ProjectTypeAllowsMoreThan256Elements()
    {
      if ( ( m_Project.ProjectType == "Catnipped" )
      ||   ( m_Project.ProjectType == "Wonderland" )
      ||   ( m_Project.ProjectType == "Soulless" )
      ||   ( m_Project.ProjectType == "Barnsley Badger" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsFlagsInXPos()
    {
      if ( ( m_Project.ProjectType == "Hyperion" )
      ||   ( m_Project.ProjectType == "Adventure" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsFlagsInElementType()
    {
      if ( ( m_Project.ProjectType == "Hyperion" )
      ||   ( m_Project.ProjectType == "Adventure" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeHas16BitIndex()
    {
      if ( m_Project.ProjectType == "Adventure" )
      {
        return false;
      }
      return true;
    }



    private bool ProjectTypeHasCompactedObjects()
    {
      if ( ( m_Project.ProjectType == "Downhill Challenge" )
      ||   ( m_Project.ProjectType == "MegaSisters" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsColorInElements()
    {
      if ( ( m_Project.ProjectType == "Cartridge" )
      ||   ( m_Project.ProjectType == "MegaSisters" ) )
      {
        return false;
      }
      return true;
    }



    private bool ProjectTypeAllowsExportOfCustomColors()
    {
      if ( m_Project.ProjectType == "Adventure" )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsPrimitiveTypeReuse()
    {
      if ( ( m_Project.ProjectType == "Soulless" )
      ||   ( m_Project.ProjectType == "Soulless 2" )
      ||   ( m_Project.ProjectType == "Rocky" )
      ||   ( m_Project.ProjectType == "Supernatural" )
      ||   ( m_Project.ProjectType == "Hyperion" )
      ||   ( m_Project.ProjectType == "Adventure" )
      ||   ( m_Project.ProjectType == "Barnsley Badger" )
      ||   ( m_Project.ProjectType == "Wonderland" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsMapData()
    {
      if ( ( m_Project.ProjectType == "Hyperion" )
      ||   ( m_Project.ProjectType == "Adventure" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeHasAutoObjectIndex()
    {
      if ( m_Project.ProjectType == "Soulless 2" )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeWantsScreenDataInHiLoTable()
    {
      if ( ( m_Project.ProjectType == "Cartridge" )
      ||   ( m_Project.ProjectType == "Hyperion" )
      ||   ( m_Project.ProjectType == "Adventure" )
      ||   ( m_Project.ProjectType == "Wonderland" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeWantsElementDataColumnRow()
    {
      if ( ( m_Project.ProjectType == "Cartridge" )
      ||   ( m_Project.ProjectType == "MegaSisters" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeWantsScreenSize()
    {
      if ( ( m_Project.ProjectType == "Cartridge" )
      ||   ( m_Project.ProjectType == "Downhill Challenge" )
      ||   ( m_Project.ProjectType == "MegaSisters" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeSupportsElementLineRepeat()
    {
      if ( m_Project.ProjectType == "Barnsley Badger" )    
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsCombiningYRepeatsWith3Bits()
    {
      if ( ( m_Project.ProjectType == "Soulless" )
      ||   ( m_Project.ProjectType == "Soulless 2" )
      ||   ( m_Project.ProjectType == "Rocky" )
      ||   ( m_Project.ProjectType == "Catnipped" )
      ||   ( m_Project.ProjectType == "Barnsley Badger" )
      ||   ( m_Project.ProjectType == "Hyperion" )
      ||   ( m_Project.ProjectType == "Adventure" )
      ||   ( m_Project.ProjectType == "Wonderland" ) )    
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsOptionalObjectData()
    {
      if ( m_Project.ProjectType == "Adventure" )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsObjectBehaviour()
    {
      if ( ( m_Project.ProjectType == "Soulless" )
      ||   ( m_Project.ProjectType == "Barnsley Badger" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsObjectData()
    {
      if ( ( m_Project.ProjectType == "Hyperion" )
      ||   ( m_Project.ProjectType == "Adventure" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsObjectTypeInClue()
    {
      if ( m_Project.ProjectType == "Soulless 2" )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeAllowsDoorWithTarget()
    {
      if ( ( m_Project.ProjectType != "Soulless" )
      &&   ( m_Project.ProjectType != "Soulless 2" )
      &&   ( m_Project.ProjectType != "Hyperion" )
      &&   ( m_Project.ProjectType != "Adventure" )
      &&   ( m_Project.ProjectType != "Wonderland" ) )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeHasSimpleDoors()
    {
      if ( ( m_Project.ProjectType == "Hyperion" )
      ||   ( m_Project.ProjectType == "Adventure" )
      ||   ( m_Project.ProjectType == "Wonderland" ) )
      {
        return true;
      }
      return false;
    }



    string BufferToData( GR.Memory.ByteBuffer Data )
    {
      if ( Data.Length == 0 )
      {
        return "";
      }
      StringBuilder sb = new StringBuilder();
      
      sb.Append( "          !byte " );

      for ( int i = 0; i < Data.Length; ++i )
      {
        if ( i > 0 )
        {
          sb.Append( ", " );
        }
        sb.Append( "$" );
        sb.Append( Data.ByteAt( i ).ToString( "x2" ) );
      }
      sb.Append( "\r\n" );
      return sb.ToString();
    }



    private bool DetermineRegionTarget( Project.Region Region, Project.RegionScreenInfo ScreenInfo, int ScreenInfoIndex, int DX, int DY, out int TargetRegion, out int TargetScreenNumber )
    {
      TargetRegion = 0;
      TargetScreenNumber = 0;

      int mapX = Region.DisplayX;
      int mapY = Region.DisplayY;
      if ( Region.Vertical )
      {
        mapY += ScreenInfoIndex;
      }
      else
      {
        mapX += ScreenInfoIndex;
      }
      mapX += DX;
      mapY += DY;

      // find target region/screen
      int regionIndex = 0;
      foreach ( Project.Region region in m_Project.Regions )
      {
        int endX = region.DisplayX;
        int endY = region.DisplayY;
        if ( region.Vertical )
        {
          endY += region.Screens.Count - 1;
        }
        else
        {
          endX += region.Screens.Count - 1;
        }
        if ( ( mapX >= region.DisplayX )
        &&   ( mapX <= endX )
        &&   ( mapY >= region.DisplayY )
        &&   ( mapY <= endY ) )
        {
          TargetRegion = regionIndex;
          TargetScreenNumber = ( mapX - region.DisplayX ) + ( mapY - region.DisplayY );
          return true;
        }
        ++regionIndex;
      }
      return false;
    }



    private bool ScreenNeedsExport( Project.Screen Screen, int TotalScreenIndex )
    {
      if ( m_Project.ProjectType != "Wonderland" )
      {
        return true;
      }
      if ( TotalScreenIndex < 52 )
      {
        return true;
      }
      return ( Screen.DisplayedElements.Count != 0 );
    }



    private void btnExport_Click( object sender, EventArgs e )
    {
      string targetFilename = editExportFile.Text;

      if ( string.IsNullOrEmpty( targetFilename ) )
      {
        return;
      }
      string result = "";

      // element constants
      int     elementIndex = m_Project.ExportConstantOffset;
      foreach ( Project.Element element in m_Project.Elements )
      {
        result += "EL_" + m_Project.ExportPrefix + "_" + SanitizeName( element );
        result += " = " + elementIndex + "\r\n";

        ++elementIndex;
      }
      result += "\r\n";

      // element size
      result += m_Project.ExportPrefix + "ELEMENT_WIDTH_TABLE\r\n!byte ";
      elementIndex = 0;
      foreach ( Project.Element element in m_Project.Elements )
      {
        result += element.Characters.Width.ToString();
        if ( elementIndex + 1 < m_Project.Elements.Count )
        {
          result += ",";
        }
        ++elementIndex;
      }
      result += "\r\n";

      // element size
      result += m_Project.ExportPrefix + "ELEMENT_HEIGHT_TABLE\r\n!byte ";
      elementIndex = 0;
      foreach ( Project.Element element in m_Project.Elements )
      {
        result += element.Characters.Height.ToString();
        if ( elementIndex + 1 < m_Project.Elements.Count )
        {
          result += ",";
        }
        ++elementIndex;
      }
      result += "\r\n";

      // optimize export (collapse data/elements)
      System.Collections.Generic.List<DataInfo>     elementDatas = new List<DataInfo>();
      System.Collections.Generic.List<DataInfo>     elementColor = new List<DataInfo>();
      elementIndex = 0;
      foreach ( Project.Element element in m_Project.Elements )
      {
        DataInfo    dataChar = new DataInfo();
        DataInfo    dataColor = new DataInfo();

        dataChar.Data = new GR.Memory.ByteBuffer();
        dataChar.Name = "DATA_EL_" + m_Project.ExportPrefix + "_" + SanitizeName( element );
        dataChar.IsChar = true;
        dataColor.Data = new GR.Memory.ByteBuffer();
        dataColor.Name = "COLOR_EL_" + m_Project.ExportPrefix + "_" + SanitizeName( element );
        dataColor.IsChar = false;

        if ( ProjectTypeWantsElementDataColumnRow() )
        {
          for ( int i = 0; i < element.Characters.Width; ++i )
          {
            for ( int j = 0; j < element.Characters.Height; ++j )
            {
              dataChar.Data.AppendU8( element.Characters[i, j].Char );
              dataColor.Data.AppendU8( element.Characters[i, j].Color );
            }
          }
        }
        else
        {
          for ( int j = 0; j < element.Characters.Height; ++j )
          {
            for ( int i = 0; i < element.Characters.Width; ++i )
            {
              dataChar.Data.AppendU8( element.Characters[i, j].Char );
              dataColor.Data.AppendU8( element.Characters[i, j].Color );
            }
          }
        }
        elementDatas.Add( dataChar );
        elementColor.Add( dataColor );

        ++elementIndex;
      }

      var elementBuffers = new List<DataInfo>();
      elementBuffers.AddRange( elementDatas );

      if ( ProjectTypeAllowsColorInElements() )
      {
        elementBuffers.AddRange( elementColor );
      }

      //CollapseBuffers( elementDatas );
      //CollapseBuffers( elementColor );
      CollapseBuffers( elementBuffers );

      result += ExportElementTable( m_Project, elementBuffers, 0 );
      if ( ProjectTypeAllowsColorInElements() )
      {
        result += ExportElementColorTable( m_Project, elementBuffers, elementDatas.Count );
      }

      var  workList = new List<DataInfo>( elementBuffers );

      while ( workList.Count > 0 )
      {
        DataInfo data = workList[0];

        //Debug.Log( "Checking " + data.Name );

        if ( ( data.ReplacementData != null )
        &&   ( data.NextData == null )
        &&   ( data.PreviousData == null ) )
        {
          //Debug.Log( "Removing data " + data.Name + " with replacement " + data.ReplacementData.Name );
          workList.RemoveAt( 0 );
          continue;
        }
        if ( data.PreviousData != null )
        {
          // keep for later
          //Debug.Log( "move to back " + data.Name );
          workList.RemoveAt( 0 );
          workList.Add( data );
          continue;
        }

        //Debug.Log( "Insert " + data.Name );
        result += data.Name + "\r\n";
        result += "!byte ";

        insert_now:;
        if ( data.NextData != null )
        {
          for ( int i = 0; i < data.Data.Length; ++i )
          {
            result += data.Data.ByteAt( i ).ToString();

            if ( i + 1 == data.Data.Length - data.NextData.OffsetInPreviousData )
            {
              //result += "\r\n;was " + data.NextData.Name + "\r\n!byte ";
              result += "\r\n" + data.NextData.Name + "\r\n!byte ";

              //Debug.Log( "done, remove " + data.Name );
              workList.RemoveAt( 0 );
              break;
            }
            result += ",";
          }
          if ( workList.Contains( data.NextData ) )
          {
            //Debug.Log( "Move to top " + data.NextData.Name );
            workList.Remove( data.NextData );
            workList.Insert( 0, data.NextData );
            data = data.NextData;
            goto insert_now;
          }
        }
        else
        {
          for ( int i = 0; i < data.Data.Length; ++i )
          {
            result += data.Data.ByteAt( i ).ToString();
            if ( i + 1 < data.Data.Length )
            {
              result += ",";
            }
            else
            {
              result += "\r\n";
            }
          }
        }
        //Debug.Log( "Removing " + data.Name );
        workList.RemoveAt( 0 );
        /*
        while ( data.NextData != null )
        {
          data = data.NextData;

          result += "; was " + data.Name + "\r\n";
          result += "!byte ";
          for ( int i = (int)data.Data.Length - data.OffsetInPreviousData; i < data.Data.Length; ++i )
          {
            result += data.Data.ByteAt( i ).ToString();
            if ( i + 1 < data.Data.Length )
            {
              result += ",";
            }
            else
            {
              result += "\r\n";
            }
          }
          ++j;
          if ( j >= elementDatas.Count )
          {
            break;
          }
          continue;
        }*/
      }

      /*
      if ( ProjectTypeAllowsColorInElements() )
      {
        foreach ( DataInfo data in elementColor )
        {
          if ( data.ReplacementData != null )
          {
            continue;
          }
          result += data.Name + "\r\n";
          result += "!byte ";
          for ( int i = 0; i < data.Data.Length; ++i )
          {
            result += data.Data.ByteAt( i ).ToString();
            if ( i + 1 < data.Data.Length )
            {
              result += ",";
            }
            else
            {
              result += "\r\n";
            }
          }
        }
      }*/
      result += "\r\n";
      result += "\r\n";

      // screens
      int screenIndex = 0;
      int totalScreenIndex = 0;
      if ( ProjectTypeWantsScreenDataInHiLoTable() )
      {
        result += m_Project.ExportPrefix + "_SCREEN_DATA_TABLE_LO\r\n";
        screenIndex = 1;
        totalScreenIndex = 0;
        foreach ( Project.Screen screen in m_Project.Screens )
        {
          if ( ScreenNeedsExport( screen, totalScreenIndex ) )
          {
            result += "          !byte <( " + m_Project.ExportPrefix + "_LEVEL_" + screenIndex.ToString() + " )\r\n";
            ++screenIndex;
          }
          ++totalScreenIndex;
        }
        result += "          !byte 0\r\n";

        result += m_Project.ExportPrefix + "_SCREEN_DATA_TABLE_HI\r\n";
        screenIndex = 1;
        totalScreenIndex = 0;
        foreach ( Project.Screen screen in m_Project.Screens )
        {
          if ( ScreenNeedsExport( screen, totalScreenIndex ) )
          {
            result += "          !byte >( " + m_Project.ExportPrefix + "_LEVEL_" + screenIndex.ToString() + " )\r\n";
            ++screenIndex;
          }
          ++totalScreenIndex;
        }
        result += "          !byte 0\r\n";
        result += "\r\n\r\n";
      }
      else
      {
        result += m_Project.ExportPrefix + "_SCREEN_DATA_TABLE\r\n";
        screenIndex = 1;
        totalScreenIndex = 0;
        foreach ( Project.Screen screen in m_Project.Screens )
        {
          if ( ScreenNeedsExport( screen, totalScreenIndex ) )
          {
            result += "          !word " + m_Project.ExportPrefix + "_LEVEL_" + screenIndex.ToString() + "\r\n";
            ++screenIndex;
          }
          ++totalScreenIndex;
        }
        result += "          !word 0\r\n";
        result += "\r\n\r\n";
      }

      int     searchObjectIndex = 0;
      screenIndex = 1;
      totalScreenIndex = 0;
      int     objectAutoIndex = 0;
      foreach ( Project.Screen screen in m_Project.Screens )
      {
        if ( !ScreenNeedsExport( screen, totalScreenIndex ) )
        {
          ++totalScreenIndex;
          continue;
        }
        result += m_Project.ExportPrefix + "_LEVEL_" + screenIndex.ToString() +" ;" + screen.Name + "\r\n";
        if ( ( ProjectTypeWantsScreenSize() )
        &&   ( !ProjectTypeRequiresSortedElementsByX() ) 
        &&   ( !ProjectTypeRequiresSortedElementsByY() ) )
        {
          // scroll size
          result += "          !byte " + screen.Width.ToString() + "\r\n";
        }
        else if ( ( ProjectTypeWantsScreenSize() )
        &&        ( ProjectTypeRequiresSortedElementsByX() ) )
        {
          // scroll size
          result += "          !word " + screen.Width.ToString() + "\r\n";
        }

        List<Project.ScreenElement>     levelElements = screen.DisplayedElements;

        if ( ProjectTypeRequiresSortedElementsByX() )
        {
          levelElements = SortElementsByX( levelElements );
        }
        if ( ProjectTypeRequiresSortedElementsByY() )
        {
          levelElements = SortElementsByY( levelElements );
        }

        string prevExportElementType = "";
        string exportElementType = "";
        string exportData = "";


        if ( ProjectTypeAlwaysRequiresLevelConfig() )
        {
          result += "          !byte " + screen.ConfigByte.ToString() + "\r\n";
        }
        else if ( screen.ConfigByte != 0 )
        {
          result += "          !byte LD_LEVEL_CONFIG," + screen.ConfigByte.ToString() + "\r\n";
        }

        string prevElementName = "";
        string prevSpawnSpotType = "";
        int localElementIndex = 0;
        int   currentElementX = 0;
        int   currentElementY = 0;

        foreach ( Project.ScreenElement screenElement in levelElements )
        {
          if ( ( ProjectTypeRequiresSortedElementsByX() )
          &&   ( screenElement.X > currentElementX ) )
          {
            int     delta = screenElement.X - currentElementX;

            while ( delta > 31 )
            {
              result += "          !byte LDF_X_POS + 31; " + screenElement.X.ToString() + "\r\n";
              delta -= 31;
            }
            if ( delta > 0 )
            {
              result += "          !byte LDF_X_POS + " + delta + "; " + screenElement.X.ToString() + "\r\n";
            }
            currentElementX = screenElement.X;
          }
          if ( ( ProjectTypeRequiresSortedElementsByY() )
          &&   ( screenElement.Y > currentElementY ) )
          {
            result += "          !byte LDF_Y_POS + " + ( screenElement.Y - currentElementY ).ToString() + "; " + screenElement.Y.ToString() + "\r\n";
            currentElementY = screenElement.Y;
          }

          result += "          !byte ";

          string elementName = "invalid";
          if ( screenElement.Index != -1 )
          {
            elementName = "EL_" + m_Project.ExportPrefix + "_" + SanitizeName( m_Project.Elements[screenElement.Index] );
          }
          int       elementUseIndex = screenElement.Index;
          if ( !ScreenElementUsesElement( screenElement ) )
          {
            elementUseIndex = 0;
          }

          switch ( screenElement.Type )
          {
            case Project.ScreenElementType.LD_ELEMENT:
              {
                exportElementType = "LD_ELEMENT";

                int xPos = screenElement.X;
                int yPos = screenElement.Y;

                if ( ( ProjectTypeRequiresSortedElementsByX() )
                ||   ( ProjectTypeRequiresSortedElementsByY() ) )
                {
                  if ( elementName == prevElementName )
                  {
                    // FFxxxxxx
                    exportData = "LDF_PREV_ELEMENT + " + yPos.ToString() + "\r\n";
                  }
                  else
                  {
                    exportData = "LDF_ELEMENT + " + yPos.ToString() + "," + elementName +  "\r\n";
                  }
                }
                else
                {
                  if ( ( ProjectTypeAllowsMoreThan256Elements() )
                  &&   ( elementUseIndex >= 256 ) )
                  {
                    xPos |= 0x40;
                    elementName = "( " + elementName + " & $ff )";
                  }
                  if ( ProjectTypeAllowsFlagsInXPos() )
                  {
                    xPos |= ( screenElement.Flags << 6 );
                  }
                  if ( elementName == prevElementName )
                  {
                    yPos |= 0x80;
                    exportData = xPos.ToString() + "," + yPos.ToString() + "\r\n";
                  }
                  else
                  {
                    exportData = xPos.ToString() + "," + yPos.ToString() + "," + elementName + "\r\n";
                  }
                }
              }
              break;
            case Project.ScreenElementType.LD_ELEMENT_LINE_H:
              {
                exportElementType = "LD_ELEMENT_LINE_H";

                int xPos = screenElement.X;
                int yPos = screenElement.Y;

                if ( ProjectTypeRequiresSortedElementsByX() )
                {
                  if ( elementName == prevElementName )
                  {
                    // FFxxxxxx
                    exportData = "LDF_PREV_ELEMENT_LINE + " + yPos.ToString() + "," + screenElement.Repeats + "\r\n";
                  }
                  else
                  {
                    exportData = "LDF_ELEMENT_LINE + " + yPos.ToString() + "," + screenElement.Repeats + "," + elementName + "\r\n";
                  }
                }
                else
                {
                  if ( !ProjectTypeAllowsCombiningYRepeatsWith3Bits() )
                  {
                    if ( ProjectTypeAllowsFlagsInXPos() )
                    {
                      xPos |= ( screenElement.Flags << 6 );
                    }

                    if ( elementName == prevElementName )
                    {
                      yPos |= 0x80;
                      exportData = xPos.ToString() + "," + yPos.ToString() + "," + screenElement.Repeats.ToString() + "\r\n";
                    }
                    else
                    {
                      exportData = xPos.ToString() + "," + yPos.ToString() + "," + screenElement.Repeats.ToString() + "," + elementName + "\r\n";
                    }
                  }
                  else
                  {
                    bool  repeatElement = false;
                    if ( ( ProjectTypeSupportsElementLineRepeat() )
                    &&   ( elementName == prevElementName ) )
                    {
                      exportElementType = "LD_ELEMENT_LINE_H_REPEAT";
                      repeatElement = true;
                    }
                    else
                    {
                      exportElementType = "LD_ELEMENT_LINE_H";
                    }

                    if ( ( ProjectTypeAllowsMoreThan256Elements() )
                    &&   ( elementUseIndex >= 256 ) )
                    {
                      xPos |= 0x40;
                      elementName = "( " + elementName + " & $ff )";
                    }
                    if ( ProjectTypeAllowsFlagsInXPos() )
                    {
                      xPos |= ( screenElement.Flags << 6 );
                    }
                  

                    if ( screenElement.Repeats <= 7 )
                    {
                      int combinedYRepeats = yPos + ( screenElement.Repeats << 5 );
                      exportData = xPos.ToString() + "," + combinedYRepeats.ToString();
                    }
                    else
                    {
                      exportData = xPos.ToString() + "," + screenElement.Y.ToString() + "," + screenElement.Repeats.ToString();
                    }

                    if ( !repeatElement )
                    {
                      exportData += "," + elementName + "\r\n";
                    }
                    else
                    {
                      exportData += "\r\n";
                    }
                  }
                }
              }
              break;
            case Project.ScreenElementType.LD_ELEMENT_LINE_V:
              {
                if ( ProjectTypeRequiresSortedElementsByX() )
                {
                  int yPos = screenElement.Y;
                  if ( elementName == prevElementName )
                  {
                    // FFxxxxxx
                    exportData = "LDF_PREV_ELEMENT_AREA + " + yPos.ToString() + ", 1," + ( 0x80 | screenElement.Repeats ).ToString() + "\r\n";
                  }
                  else
                  {
                    exportData = "LDF_ELEMENT_AREA + " + yPos.ToString() + ", 1," + screenElement.Repeats + "," + elementName + "\r\n";
                  }
                }
                else
                {
                  exportElementType = "LD_ELEMENT_LINE_V";

                  int xPos = screenElement.X;
                  int yPos = screenElement.Y;

                  if ( !ProjectTypeAllowsCombiningYRepeatsWith3Bits() )
                  {
                    if ( ProjectTypeAllowsFlagsInXPos() )
                    {
                      xPos |= ( screenElement.Flags << 6 );
                    }

                    if ( elementName == prevElementName )
                    {
                      yPos |= 0x80;
                      exportData = xPos.ToString() + "," + yPos.ToString() + "," + screenElement.Repeats.ToString() + "\r\n";
                    }
                    else
                    {
                      exportData = xPos.ToString() + "," + yPos.ToString() + "," + screenElement.Repeats.ToString() + "," + elementName + "\r\n";
                    }
                  }
                  else
                  {
                    bool  repeatElement = false;
                    if ( ( ProjectTypeSupportsElementLineRepeat() )
                    && ( elementName == prevElementName ) )
                    {
                      exportElementType = "LD_ELEMENT_LINE_V_REPEAT";
                      repeatElement = true;
                    }
                    else
                    {
                      exportElementType = "LD_ELEMENT_LINE_V";
                    }

                    if ( ( ProjectTypeAllowsMoreThan256Elements() )
                    &&   ( elementUseIndex >= 256 ) )
                    {
                      xPos |= 0x40;
                      elementName = "( " + elementName + " & $ff )";
                    }
                    if ( ProjectTypeAllowsFlagsInXPos() )
                    {
                      xPos |= ( screenElement.Flags << 6 );
                    }

                    if ( screenElement.Repeats <= 7 )
                    {
                      int combinedYRepeats = yPos + ( screenElement.Repeats << 5 );

                      exportData = xPos.ToString() + "," + combinedYRepeats.ToString();
                    }
                    else
                    {
                      exportData = xPos.ToString() + "," + screenElement.Y.ToString() + "," + screenElement.Repeats.ToString();
                    }

                    if ( !repeatElement )
                    {
                      exportData += "," + elementName + "\r\n";
                    }
                    else
                    {
                      exportData += "\r\n";
                    }
                  }
                }
              }
              break;
            case Project.ScreenElementType.LD_ELEMENT_AREA:
              {
                exportElementType = "LD_ELEMENT_AREA";

                int xPos = screenElement.X;
                int yPos = screenElement.Y;

                if ( ProjectTypeRequiresSortedElementsByX() )
                {
                  if ( elementName == prevElementName )
                  {
                    // FFxxxxxx
                    exportData = "LDF_PREV_ELEMENT_AREA + " + yPos.ToString() + "," + screenElement.Repeats + "," + ( 0x80 | screenElement.Repeats2 ).ToString() + "\r\n";
                  }
                  else
                  {
                    exportData = "LDF_ELEMENT_AREA + " + yPos.ToString() + "," + screenElement.Repeats + "," + screenElement.Repeats2 + "," + elementName + "\r\n";
                  }
                }
                else
                {
                  if ( ( ProjectTypeAllowsMoreThan256Elements() )
                  &&   ( elementUseIndex >= 256 ) )
                  {
                    xPos |= 0x40;
                    elementName = "( " + elementName + " & $ff )";
                  }
                  if ( ProjectTypeAllowsFlagsInXPos() )
                  {
                    xPos |= ( screenElement.Flags << 6 );
                  }

                  if ( elementName == prevElementName )
                  {
                    yPos |= 0x80;
                    exportData = xPos.ToString() + "," + yPos.ToString() + ","
                              + screenElement.Repeats.ToString() + ","
                              + screenElement.Repeats2.ToString() + "\r\n";
                  }
                  else
                  {
                    exportData = xPos.ToString() + "," + yPos.ToString() + ","
                              + screenElement.Repeats.ToString() + ","
                              + screenElement.Repeats2.ToString() + ","
                              + elementName + "\r\n";
                  }
                }
              }
              break;
            case Project.ScreenElementType.LD_LINE_H:
              {
                exportElementType = "LD_LINE_H";
                int xPos = screenElement.X;

                if ( ProjectTypeAllowsFlagsInXPos() )
                {
                  xPos |= ( screenElement.Flags << 6 );
                }

                if ( ProjectTypeAllowsColorInElements() )
                {
                  exportData = xPos.ToString() + "," + screenElement.Y.ToString() + "," + screenElement.Repeats.ToString() + ","
                            + screenElement.Char.ToString() + "," + screenElement.Color.ToString() + "\r\n";
                }
                else
                {
                  exportData = xPos.ToString() + "," + screenElement.Y.ToString() + "," + screenElement.Repeats.ToString() + ","
                            + screenElement.Char.ToString() + "\r\n";
                }
              }
              break;
            case Project.ScreenElementType.LD_LINE_V:
              {
                exportElementType = "LD_LINE_V";

                int xPos = screenElement.X;

                if ( ProjectTypeAllowsFlagsInXPos() )
                {
                  xPos |= ( screenElement.Flags << 6 );
                }

                if ( ProjectTypeAllowsColorInElements() )
                {
                  exportData = xPos.ToString() + "," + screenElement.Y.ToString() + "," + screenElement.Repeats.ToString() + ","
                            + screenElement.Char.ToString() + "," + screenElement.Color.ToString() + "\r\n";
                }
                else
                {
                  exportData = xPos.ToString() + "," + screenElement.Y.ToString() + "," + screenElement.Repeats.ToString() + ","
                            + screenElement.Char.ToString() + "\r\n";
                }
              }
              break;
            case Project.ScreenElementType.LD_SEARCH_OBJECT:
              {
                exportElementType = "LD_SEARCH_OBJECT";
                int     xPos = screenElement.X;

                if ( ProjectTypeAllowsFlagsInXPos() )
                {
                  xPos |= ( screenElement.Flags << 6 );
                }

                if ( ( ProjectTypeAllowsMoreThan256Elements() )
                &&   ( elementUseIndex >= 256 ) )
                {
                  xPos |= 0x40;
                  elementName = "( " + elementName + " & $ff )";
                }
                if ( ProjectTypeHas16BitIndex() )
                {
                  exportData = xPos.ToString() + "," + screenElement.Y.ToString() + "," + elementName + ","
                            + ( searchObjectIndex >> 8 ).ToString() + ","
                            + ( searchObjectIndex & 0xff ).ToString() + "\r\n";
                }
                else
                {
                  exportData = xPos.ToString() + "," + screenElement.Y.ToString() + "," + elementName + ","
                            + screenElement.SearchObjectIndex.ToString() + "\r\n";
                }
                ++searchObjectIndex;
              }
              break;
            case Project.ScreenElementType.LD_LINE_H_ALT:
              exportElementType = "LD_LINE_H_ALT";
              if ( ProjectTypeAllowsColorInElements() )
              {
                exportData = screenElement.X.ToString() + "," + screenElement.Y.ToString() + "," + screenElement.Repeats.ToString() + ","
                          + screenElement.Char.ToString() + "," + screenElement.Color.ToString() + "\r\n";
              }
              else
              {
                exportData = screenElement.X.ToString() + "," + screenElement.Y.ToString() + "," + screenElement.Repeats.ToString() + ","
                          + screenElement.Char.ToString() + "\r\n";
              }
              break;
            case Project.ScreenElementType.LD_LINE_V_ALT:
              exportElementType = "LD_LINE_V_ALT";
              if ( ProjectTypeAllowsColorInElements() )
              {
                exportData = screenElement.X.ToString() + "," + screenElement.Y.ToString() + "," + screenElement.Repeats.ToString() + ","
                          + screenElement.Char.ToString() + "," + screenElement.Color.ToString() + "\r\n";
              }
              else
              {
                exportData = screenElement.X.ToString() + "," + screenElement.Y.ToString() + "," + screenElement.Repeats.ToString() + ","
                          + screenElement.Char.ToString() + "\r\n";
              }
              break;
            case Project.ScreenElementType.LD_AREA:
              {
                exportElementType = "LD_AREA";

                int xPos = screenElement.X;

                if ( ProjectTypeAllowsFlagsInXPos() )
                {
                  xPos |= ( screenElement.Flags << 6 );
                }

                if ( !ProjectTypeAllowsColorInElements() )
                {
                  exportData = xPos.ToString() + "," + screenElement.Y.ToString() + ","
                            + screenElement.Repeats.ToString() + ","
                            + screenElement.Repeats2.ToString() + ","
                            + screenElement.Char.ToString() + "\r\n";
                }
                else
                {
                  exportData = xPos.ToString() + "," + screenElement.Y.ToString() + ","
                            + screenElement.Repeats.ToString() + ","
                            + screenElement.Repeats2.ToString() + ","
                            + screenElement.Char.ToString() + ","
                            + screenElement.Color.ToString() + "\r\n";
                }
              }
              break;
            case Project.ScreenElementType.LD_OBJECT:
              if ( ProjectTypeHasCompactedObjects() )
              {
                if ( ProjectTypeRequiresSortedElementsByX() )
                {
                  exportData = "LD_OBJECT | " + screenElement.Object.TemplateIndex.ToString() + ", " + screenElement.Y.ToString() + System.Environment.NewLine;
                }
                else
                {
                  exportData = "LD_OBJECT | " + screenElement.Object.TemplateIndex.ToString() + ", " + screenElement.X.ToString() + System.Environment.NewLine;
                }
                break;
              }
              exportElementType = "LD_OBJECT";
              if ( ProjectTypeHasAutoObjectIndex() )
              {
                // add behaviour/bounds
                if ( screenElement.Object.TemplateIndex != -1 )
                {
                  exportData = screenElement.X.ToString() + "," + screenElement.Y.ToString() + ","
                            + "TYPE_" + SanitizeName( m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] ) + ","
                            + screenElement.Object.Color.ToString() + ","
                            + objectAutoIndex.ToString() + ",";

                  exportData += screenElement.Object.Speed.ToString() + ",";
                  if ( ( screenElement.Object.MoveBorderLeft != 0 )
                  ||   ( screenElement.Object.MoveBorderRight != 0 ) )
                  {
                    exportData += ( screenElement.X - screenElement.Object.MoveBorderLeft ).ToString() + "," + ( screenElement.X + screenElement.Object.MoveBorderRight ).ToString();
                  }
                  else
                  {
                    exportData += ( screenElement.Y - screenElement.Object.MoveBorderTop ).ToString() + "," + ( screenElement.Y + screenElement.Object.MoveBorderBottom ).ToString();
                  }
                  ++objectAutoIndex;
                }
                else
                {
                  exportData += "LD_OBJECT - not correct!";
                }
                exportData += "\r\n";
              }
              else if ( ProjectTypeAllowsObjectBehaviour() )
              {
                // add behaviour/bounds
                if ( screenElement.Object.TemplateIndex != -1 )
                {
                  int     behaviour = 0;
                  if ( !m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].Behaviours.ContainsKey( screenElement.Object.Behaviour ) )
                  {
                    //System.Windows.Forms.MessageBox.Show( "Missing behaviour " + screenElement.Object.Behaviour.ToString() + " for " + m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] );
                    Debug.Log( "Missing behaviour " + screenElement.Object.Behaviour.ToString() + " for " + m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] + " in " + screen.Name + ": " + screenIndex );
                  }
                  else
                  {
                    behaviour = m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].Behaviours[screenElement.Object.Behaviour].Value;
                  }
                  exportData = screenElement.X.ToString() + "," + screenElement.Y.ToString() + ","
                            + "TYPE_" + SanitizeName( m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] ) + ","
                            + screenElement.Object.Color.ToString() + ","
                            + behaviour.ToString() + ",";

                  exportData += screenElement.Object.Speed.ToString() + ",";
                  if ( ( screenElement.Object.MoveBorderLeft != 0 )
                  ||   ( screenElement.Object.MoveBorderRight != 0 ) )
                  {
                    exportData += ( screenElement.X - screenElement.Object.MoveBorderLeft ).ToString() + "," + ( screenElement.X + screenElement.Object.MoveBorderRight ).ToString();
                  }
                  else
                  {
                    exportData += ( screenElement.Y - screenElement.Object.MoveBorderTop ).ToString() + "," + ( screenElement.Y + screenElement.Object.MoveBorderBottom ).ToString();
                  }
                }
                else
                {
                  exportData += "LD_OBJECT - not correct!";
                }
                exportData += "\r\n";
              }
              else if ( ProjectTypeAllowsOptionalObjectData() )
              {
                if ( screenElement.Object.TemplateIndex != -1 )
                {
                  if ( screenElement.Object.Optional != Project.GameObject.OptionalType.ALWAYS_SHOWN )
                  {
                    if ( screenElement.Object.Optional == Project.GameObject.OptionalType.SHOWN_IF_OPTIONAL_SET )
                    {
                      exportElementType = "LD_OPTIONAL_SHOWN_OBJECT";
                      exportData = screenElement.Object.OptionalValue.ToString() + ","
                                + screenElement.X.ToString() + "," + screenElement.Y.ToString() + ","
                                + "TYPE_" + SanitizeName( m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] )
                                + "\r\n";
                    }
                    else
                    {
                      exportElementType = "LD_OPTIONAL_HIDDEN_OBJECT";
                      exportData = screenElement.Object.OptionalValue.ToString() + ","
                                + screenElement.X.ToString() + "," + screenElement.Y.ToString() + ","
                                + "TYPE_" + SanitizeName( m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] )
                                + "\r\n";
                    }
                  }
                  else
                  {
                    exportData = screenElement.X.ToString() + "," + screenElement.Y.ToString() + ","
                              + "TYPE_" + SanitizeName( m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] ) + "\r\n";
                  }
                }
                else
                {
                  Debug.Log( "Invalid Object in screen " + screenIndex + " at line " + localElementIndex );
                }
              }
              else if ( ProjectTypeAllowsObjectData() )
              {
                // !byte LD_OBJECT,5,4,TYPE_PLAYER_DEAN
                if ( screenElement.Object.TemplateIndex != -1 )
                {
                  if ( screenElement.Object.Data != 0 )
                  {
                    exportElementType = "LD_DATA_OBJECT";
                    exportData = screenElement.Object.Data.ToString() + ","
                              + screenElement.X.ToString() + "," + screenElement.Y.ToString() + ","
                              + "TYPE_" + SanitizeName( m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] )
                              + "\r\n";
                  }
                  else
                  {
                    exportData = screenElement.X.ToString() + "," + screenElement.Y.ToString() + ","
                              + "TYPE_" + SanitizeName( m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] ) + "\r\n";
                  }
                }
                else
                {
                  Debug.Log( "Invalid Object in screen " + screenIndex + " at line " + localElementIndex );
                }
              }
              else
              {
                // !byte LD_OBJECT,5,4,TYPE_PLAYER_DEAN
                if ( screenElement.Object.TemplateIndex != -1 )
                {
                  exportData = screenElement.X.ToString() + "," + screenElement.Y.ToString() + ","
                            + "TYPE_" + SanitizeName( m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] ) + "\r\n";
                }
                else
                {
                  Debug.Log( "Invalid Object in screen " + screenIndex + " at line " + localElementIndex );
                }
              }
              break;
            case Project.ScreenElementType.LD_SPAWN_SPOT:
              exportElementType = "LD_SPAWN_SPOT";
              if ( screenElement.Object.TemplateIndex != -1 )
              {
                string    spawnSpotType = SanitizeName( m_Project.ObjectTemplates[screenElement.Object.TemplateIndex] );
                if ( prevSpawnSpotType == spawnSpotType )
                {
                  int     posY = screenElement.Y | 0x80;
                  exportData = screenElement.X.ToString() + "," + posY.ToString() + ","
                            + screenElement.Repeats.ToString() + "\r\n";
                }
                else
                {
                  exportData = screenElement.X.ToString() + "," + screenElement.Y.ToString() + ","
                            + "TYPE_" + spawnSpotType + ","
                            + screenElement.Repeats.ToString() + "\r\n";
                }
                prevSpawnSpotType = spawnSpotType;
              }
              else
              {
                Debug.Log( "Invalid Object in spawn spot in screen " + screenIndex + " at line " + localElementIndex );
              }
              break;
            case Project.ScreenElementType.LD_DOOR:
              exportElementType = "LD_DOOR";
              if ( ProjectTypeHasSimpleDoors() )
              {
                exportData = screenElement.X.ToString() + ","
                                     + screenElement.Y.ToString() + ","
                                     + elementName + "\r\n";
              }
              else if ( ProjectTypeAllowsDoorWithTarget() )
              {
                exportData = screenElement.TargetLevel.ToString() + ","
                                     + screenElement.TargetX.ToString() + ","
                                     + screenElement.TargetY.ToString() + ","
                                     + screenElement.X.ToString() + ","
                                     + screenElement.Y.ToString() + ","
                                     + elementName + "\r\n";
              }
              else
              {
                exportData = screenElement.X.ToString() + ","
                                     + screenElement.Y.ToString() + ","
                                     + elementName + ","
                                     + screenElement.TargetX.ToString() + "\r\n";
              }
              break;
            case Project.ScreenElementType.LD_CLUE:
              exportElementType = "LD_CLUE";
              if ( ProjectTypeAllowsObjectTypeInClue() )
              {
                exportData = screenElement.X.ToString() + ","
                                     + screenElement.Y.ToString() + ","
                                     + elementName + ","
                                     + screenElement.Repeats.ToString() + "\r\n";
              }
              else
              {
                exportData = screenElement.X.ToString() + ","
                                     + screenElement.Y.ToString() + ","
                                     + screenElement.Repeats.ToString() + "\r\n";
              }
              break;
            case Project.ScreenElementType.LD_SPECIAL:
              {
                exportElementType = "LD_SPECIAL";

                int xPos = screenElement.X;
                int yPos = screenElement.Y;

                if ( ( ProjectTypeAllowsMoreThan256Elements() )
                &&   ( elementUseIndex >= 256 ) )
                {
                  xPos |= 0x40;
                  elementName = "( " + elementName + " & $ff )";
                }
                if ( ProjectTypeAllowsFlagsInXPos() )
                {
                  xPos |= ( screenElement.Flags << 6 );
                }

                if ( elementName == prevElementName )
                {
                  yPos |= 0x80;
                }
                if ( elementName == prevElementName )
                {
                  exportData = xPos + ","
                               + yPos + ","
                               + screenElement.Repeats.ToString() + "\r\n";
                }
                else
                {
                  exportData = xPos + ","
                               + yPos + ","
                               + elementName + ","
                               + screenElement.Repeats.ToString() + "\r\n";
                }
              }
              break;
          }
          if ( screenElement.Type != Project.ScreenElementType.LD_SPAWN_SPOT )
          {
            prevSpawnSpotType = "";
          }
          // put extra flags in element type
          if ( ProjectTypeAllowsFlagsInElementType() )
          {
            if ( screenElement.Flags != 0 )
            {
              // force element byte empty so element type is not reused (would interfere with flags)
              prevExportElementType = "";
            }
          }
          if ( ( ProjectTypeRequiresSortedElementsByX() )
          ||   ( ProjectTypeRequiresSortedElementsByY() ) )
          {
            result += exportData;
          }
          else if ( ProjectTypeAllowsPrimitiveTypeReuse() )
          {
            if ( prevExportElementType != exportElementType )
            {
              prevExportElementType = exportElementType;
              result += exportElementType + "," + exportData;
            }
            else
            {
              result += exportData;
            }
          }
          else
          {
            result += exportElementType + "," + exportData;
          }

          if ( ScreenElementUsesElement( screenElement ) )
          {
            prevElementName = elementName;
          }
          ++localElementIndex;
        }

        result += "          !byte LD_END\r\n";

        result += "\r\n";

        if ( ProjectTypeAllowsExportOfCustomColors() )
        {
          int   color1 = ( screen.OverrideMC1 != -1 ? screen.OverrideMC1 : m_Project.Charsets[screen.CharsetIndex].Colors.MultiColor1 );
          int   color2 = ( screen.OverrideMC2 != -1 ? screen.OverrideMC2 : m_Project.Charsets[screen.CharsetIndex].Colors.MultiColor2 );

          result += "          !byte $" + ( ( color1 << 4 ) + color2 ).ToString( "X2" ) + "\r\n";
        }
        result += screen.ExtraData + "\r\n";

        ++screenIndex;
        ++totalScreenIndex;
      }
      result += "\r\n";

      // Wonderland map-data
      if ( m_Project.ProjectType == "Wonderland" )
      {
        result += "MAP_DATA\r\n";
        for ( int j = 0; j < 10; ++j )
        {
          result += "          !byte ";
          for ( int i = 0; i < 12; ++i )
          {
            byte mapDataValue = 0;
            int localScreenIndex = 52 + i + j * 12;
            if ( localScreenIndex < m_Project.Screens.Count )
            {
              mapDataValue = m_Project.Screens[localScreenIndex].WLConfigByte;
            }

            result += "$" + mapDataValue.ToString( "X2" );
            if ( i < 11 )
            {
              result += ", ";
            }
          }
          result += "\r\n";
        }
        result += "\r\n";
        result += "\r\n";
        result += "MAP_PLUS\r\n";
        totalScreenIndex = 52;
        for ( int j = 0; j < 10; ++j )
        {
          result += "          !byte ";
          for ( int i = 0; i < 12; ++i )
          {
            byte mapDataValue = 0;
            int localScreenIndex = 52 + i + j * 12;
            if ( localScreenIndex < m_Project.Screens.Count )
            {
              if ( m_Project.Screens[localScreenIndex].DisplayedElements.Count != 0 )
              {
                mapDataValue = (byte)totalScreenIndex;
                ++totalScreenIndex;
              }
            }

            result += "$" + mapDataValue.ToString( "X2" );
            if ( i < 11 )
            {
              result += ", ";
            }
          }
          result += "\r\n";
        }
      }

      if ( ProjectTypeAllowsMapData() )
      {
        result += "\r\n";
        result += "REGION_DATA\r\n";
        int regionIndex = 1;
        foreach ( Project.Region region in m_Project.Regions )
        {
          // only allows up to 4 screens per region!

          // EEEE DVCC
          //        ^^ = screen count - 1
          //       ^   = vertical if set
          //      D    = 2 bytes extra data follows screen data
          // ^^^^      = number of exit data bytes

          byte regionInfo = (byte)( region.Screens.Count - 1 );

          if ( region.Vertical )
          {
            regionInfo |= 0x04;
          }
          if ( region.ExtraData.Length > 0 )
          {
            regionInfo |= 0x08;
          }
          int exitCount = 0;
          foreach ( Project.RegionScreenInfo screenInfo in region.Screens )
          {
            if ( screenInfo.ExitW )
            {
              ++exitCount;
            }
            if ( screenInfo.ExitE )
            {
              ++exitCount;
            }
            if ( screenInfo.ExitN )
            {
              ++exitCount;
            }
            if ( screenInfo.ExitS )
            {
              ++exitCount;
            }
          }
          regionInfo |= (byte)( exitCount << 4 );

          // screen indices
          string screenInfoData = "          ;region " + regionIndex + "\r\n";
          screenInfoData += "          !byte $" + regionInfo.ToString( "x2" );
          foreach ( Project.RegionScreenInfo screenInfo in region.Screens )
          {
            byte screenInfoVal = (byte)screenInfo.ScreenIndex;
            screenInfoData += ", $" + screenInfoVal.ToString( "x2" );
          }
          screenInfoData += "\r\n";
          if ( region.ExtraData.Length > 0 )
          {
            screenInfoData += "          ;extra data\r\n";
            screenInfoData += BufferToData( region.ExtraData );
          }

          // exit connections
          screenInfoData += "          ;exit screens\r\n";
          int screenInfoIndex = 0;

          GR.Memory.ByteBuffer    exitData = new GR.Memory.ByteBuffer();

          foreach ( Project.RegionScreenInfo screenInfo in region.Screens )
          {
            if ( screenInfo.ExitN )
            {
              // source
              exitData.AppendU8( (byte)( 0x00 | screenInfoIndex ) );
              // target = target region plus screen number (SSRRRRRR)
              int targetRegion = 0;
              int targetScreenNumber = 0;
              DetermineRegionTarget( region, screenInfo, screenInfoIndex, 0, -1, out targetRegion, out targetScreenNumber );
              exitData.AppendU8( (byte)( targetRegion | ( targetScreenNumber << 6 ) ) );
            }
            if ( screenInfo.ExitS )
            {
              // source
              exitData.AppendU8( (byte)( 0x40 | screenInfoIndex ) );
              // target = target region plus screen number (SSRRRRRR)
              int targetRegion = 0;
              int targetScreenNumber = 0;
              DetermineRegionTarget( region, screenInfo, screenInfoIndex, 0, 1, out targetRegion, out targetScreenNumber );
              exitData.AppendU8( (byte)( targetRegion | ( targetScreenNumber << 6 ) ) );
            }
            if ( screenInfo.ExitW )
            {
              // source
              exitData.AppendU8( (byte)( 0x80 | screenInfoIndex ) );
              // target = target region plus screen number (SSRRRRRR)
              int targetRegion = 0;
              int targetScreenNumber = 0;
              DetermineRegionTarget( region, screenInfo, screenInfoIndex, -1, 0, out targetRegion, out targetScreenNumber );
              exitData.AppendU8( (byte)( targetRegion | ( targetScreenNumber << 6 ) ) );
            }
            if ( screenInfo.ExitE )
            {
              // source
              exitData.AppendU8( (byte)( 0xc0 | screenInfoIndex ) );
              // target = target region plus screen number (SSRRRRRR)
              int targetRegion = 0;
              int targetScreenNumber = 0;
              DetermineRegionTarget( region, screenInfo, screenInfoIndex, 1, 0, out targetRegion, out targetScreenNumber );
              exitData.AppendU8( (byte)( targetRegion | ( targetScreenNumber << 6 ) ) );
            }
            ++screenInfoIndex;
          }
          screenInfoData += BufferToData( exitData );
          result += screenInfoData;
          ++regionIndex;
        }
      }

      GR.IO.File.WriteAllText( editExportFile.Text, result );
      //Debug.Log( result );
    }



    private List<Project.ScreenElement> SortElementsByX( List<Project.ScreenElement> LevelElements )
    {
      GR.Collections.MultiMap<int,Project.ScreenElement>  sortedList = new GR.Collections.MultiMap<int, Project.ScreenElement>();

      foreach ( var element in LevelElements )
      {
        sortedList.Add( element.X, element );
      }
      return sortedList.Values;
    }



    private List<Project.ScreenElement> SortElementsByY( List<Project.ScreenElement> LevelElements )
    {
      GR.Collections.MultiMap<int,Project.ScreenElement>  sortedList = new GR.Collections.MultiMap<int, Project.ScreenElement>();

      foreach ( var element in LevelElements )
      {
        sortedList.Add( element.Y, element );
      }
      return sortedList.Values;
    }



    private bool ProjectTypeAlwaysRequiresLevelConfig()
    {
      if ( m_Project.ProjectType == "MegaSisters" )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeRequiresSortedElementsByX()
    {
      if ( m_Project.ProjectType == "MegaSisters" )
      {
        return true;
      }
      return false;
    }



    private bool ProjectTypeRequiresSortedElementsByY()
    {
      if ( m_Project.ProjectType == "Downhill Challenge" )
      {
        return true;
      }
      return false;
    }



    private void pictureEditor_MouseMove( object sender, MouseEventArgs e )
    {
      HandleMouse( e.X, e.Y, e.Button );
    }



    private void HandleMouse( int X, int Y, MouseButtons Buttons )
    {
      int realX = ( X / ( pictureEditor.Width / 40 ) );
      int realY = ( Y / ( pictureEditor.Height / 25 ) );
      realX += m_ScreenOffsetX;
      realY += m_ScreenOffsetY;

      int realPixelX = ( X / ( pictureEditor.Width / 320 ) );
      int realPixelY = ( Y / ( pictureEditor.Height / 200 ) );
      realPixelX += m_ScreenOffsetX * 8;
      realPixelY += m_ScreenOffsetY * 8;

      if ( m_DraggedScreenElement != null )
      {
        if ( ( Buttons & MouseButtons.Left ) == MouseButtons.Left )
        {
          if ( ( m_DraggedScreenElement.X != realX - m_DragOffsetX )
          ||   ( m_DraggedScreenElement.Y != realY - m_DragOffsetY ) )
          {
            if ( ( realX >= 0 )
            &&   ( realY >= 0 ) )
            {
              m_DraggedScreenElement.X = realX - m_DragOffsetX;
              m_DraggedScreenElement.Y = realY - m_DragOffsetY;
              Modified = true;

              listScreenElements.SelectedItems[0].SubItems[2].Text = m_DraggedScreenElement.X.ToString();
              listScreenElements.SelectedItems[0].SubItems[3].Text = m_DraggedScreenElement.Y.ToString();
              RedrawScreen();
            }
          }
        }
        else
        {
          m_DraggedScreenElement = null;
        }
        return;
      }

      Project.ScreenElement screenElement = m_ScreenContent[realX, realY].ScreenElement;

      if ( m_CurrentScreen != null )
      {
        foreach ( Project.ScreenElement itElement in m_CurrentScreen.DisplayedElements )
        {
          if ( ( ( itElement.Type == Project.ScreenElementType.LD_OBJECT )
          ||     ( itElement.Type == Project.ScreenElementType.LD_SPAWN_SPOT ) )
          &&   ( realPixelX >= itElement.X * 8 - 8 )
          &&   ( realPixelX < itElement.X * 8 + 24 - 8 )
          &&   ( realPixelY >= itElement.Y * 8 - 13 )
          &&   ( realPixelY < itElement.Y * 8 + 21 - 13 ) )
          {
            screenElement = itElement;
            break;
          }
        }
      }

      if ( screenElement != null )
      {
        if ( ( Buttons & MouseButtons.Left ) == MouseButtons.Left )
        {
          if ( ( Control.ModifierKeys & Keys.Control ) == Keys.Control )
          {
            // create a clone
            m_DraggedScreenElement = new Project.ScreenElement();
            m_DraggedScreenElement.Char = screenElement.Char;
            m_DraggedScreenElement.Color = screenElement.Color;
            m_DraggedScreenElement.Repeats = screenElement.Repeats;
            m_DraggedScreenElement.Repeats2 = screenElement.Repeats2;
            m_DraggedScreenElement.SearchObjectIndex = screenElement.SearchObjectIndex;
            m_DraggedScreenElement.Type = screenElement.Type;
            m_DraggedScreenElement.X = screenElement.X;
            m_DraggedScreenElement.Y = screenElement.Y;
            m_DraggedScreenElement.Index = screenElement.Index;
            m_DraggedScreenElement.TargetX = screenElement.TargetX;
            m_DraggedScreenElement.TargetY = screenElement.TargetY;
            m_DraggedScreenElement.TargetLevel = screenElement.TargetLevel;

            m_CurrentScreen.DisplayedElements.Add( m_DraggedScreenElement );

            ListViewItem item = new ListViewItem( m_CurrentScreen.DisplayedElements.Count.ToString() );

            if ( m_DraggedScreenElement.Index != -1 )
            {
              item.SubItems.Add( m_Project.Elements[m_DraggedScreenElement.Index].Name );
            }
            else
            {
              item.SubItems.Add( "" );
            }
            item.SubItems.Add( screenElement.X.ToString() );
            item.SubItems.Add( screenElement.Y.ToString() );
            string elType = "";
            switch ( screenElement.Type )
            {
              case Project.ScreenElementType.LD_ELEMENT:
                elType = "EL";
                break;
              case Project.ScreenElementType.LD_ELEMENT_LINE_H:
                elType = "EH";
                break;
              case Project.ScreenElementType.LD_ELEMENT_LINE_V:
                elType = "EV";
                break;
              case Project.ScreenElementType.LD_LINE_H:
                elType = "LH";
                break;
              case Project.ScreenElementType.LD_LINE_V:
                elType = "LV";
                break;
              case Project.ScreenElementType.LD_SEARCH_OBJECT:
                elType = "SO";
                break;
              case Project.ScreenElementType.LD_LINE_H_ALT:
                elType = "AH";
                break;
              case Project.ScreenElementType.LD_LINE_V_ALT:
                elType = "AV";
                break;
              case Project.ScreenElementType.LD_AREA:
                elType = "AR";
                break;
              case Project.ScreenElementType.LD_OBJECT:
                elType = "OB";
                break;
              case Project.ScreenElementType.LD_SPAWN_SPOT:
                elType = "SS";
                break;
              case Project.ScreenElementType.LD_ELEMENT_AREA:
                elType = "EA";
                break;
              case Project.ScreenElementType.LD_DOOR:
                elType = "DO";
                break;
              case Project.ScreenElementType.LD_CLUE:
                elType = "CL";
                break;
              case Project.ScreenElementType.LD_SPECIAL:
                elType = "SP";
                break;
            }
            if ( screenElement.Object != null )
            {
              m_DraggedScreenElement.Object = new Project.GameObject();
              m_DraggedScreenElement.Object.X = screenElement.Object.X;
              m_DraggedScreenElement.Object.Y = screenElement.Object.Y;
              m_DraggedScreenElement.Object.Speed = screenElement.Object.Speed;
              m_DraggedScreenElement.Object.Data = screenElement.Object.Data;
              m_DraggedScreenElement.Object.Behaviour = screenElement.Object.Behaviour;
              m_DraggedScreenElement.Object.Color = screenElement.Object.Color;
              m_DraggedScreenElement.Object.MoveBorderBottom = screenElement.Object.MoveBorderBottom;
              m_DraggedScreenElement.Object.MoveBorderLeft = screenElement.Object.MoveBorderLeft;
              m_DraggedScreenElement.Object.MoveBorderRight = screenElement.Object.MoveBorderRight;
              m_DraggedScreenElement.Object.MoveBorderTop = screenElement.Object.MoveBorderTop;
              m_DraggedScreenElement.Object.TemplateIndex = screenElement.Object.TemplateIndex;
              m_DraggedScreenElement.Object.SpriteImage = new GR.Image.MemoryImage( screenElement.Object.SpriteImage );
              RebuildSpriteImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[m_DraggedScreenElement.Object.TemplateIndex].StartSprite].Tile,
                                  m_SpriteProject.Colors.Palette, 
                                  m_DraggedScreenElement.Object.SpriteImage,
                                  m_SpriteProject.Sprites[m_Project.ObjectTemplates[m_DraggedScreenElement.Object.TemplateIndex].StartSprite].Mode,
                                  m_DraggedScreenElement.Object.Color );

              item.SubItems[1].Text = m_Project.ObjectTemplates[m_DraggedScreenElement.Object.TemplateIndex].Name;
            }
            item.SubItems.Add( elType );
            item.SubItems.Add( m_DraggedScreenElement.Repeats.ToString() );
            item.Tag = m_DraggedScreenElement;
            listScreenElements.Items.Add( item );
            listScreenElements.SelectedItems.Clear();
            item.Selected = true;
            Modified = true;
          }
          else
          {
            m_DraggedScreenElement = screenElement;
            foreach ( ListViewItem item in listScreenElements.Items )
            {
              if ( (Project.ScreenElement)item.Tag == m_DraggedScreenElement )
              {
                listScreenElements.SelectedItems.Clear();
                item.Selected = true;
                RedrawScreen();
                break;
              }
            }
            //listScreenElements.Items[m_DraggedScreenElement.
          }
          m_DragOffsetX = realX - screenElement.X;
          m_DragOffsetY = realY - screenElement.Y;
        }
      }
    }



    private void btnDeleteElement_Click( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count > 0 )
      {
        int origIndex = listScreenElements.SelectedIndices[0];

        Project.ScreenElement elementToDelete = m_CurrentScreen.DisplayedElements[origIndex];

        m_CurrentScreen.DisplayedElements.RemoveAt( origIndex );
        listScreenElements.Items.RemoveAt( origIndex );

        // renumber items
        int itemIndex = 0;
        foreach ( ListViewItem item in listScreenElements.Items )
        {
          item.Text = itemIndex.ToString();
          ++itemIndex;
        }

        Modified = true;
        RedrawScreen();
      }
    }



    private void FormMain_FormClosing( object sender, FormClosingEventArgs e )
    {
      if ( Modified )
      {
        System.Windows.Forms.DialogResult res = System.Windows.Forms.MessageBox.Show( "There are unsaved changes. Do you want to save before exiting?", "Save changes?", MessageBoxButtons.YesNoCancel );

        if ( res == DialogResult.Yes )
        {
          saveToolStripMenuItem_Click( sender, new EventArgs() );
          e.Cancel = false;
        }
        else if ( res == DialogResult.No )
        {
          e.Cancel = false;
        }
        else if ( res == DialogResult.Cancel )
        {
          e.Cancel = true;
        }
      }

    }



    private void pictureEditor_MouseDown( object sender, MouseEventArgs e )
    {
      HandleMouse( e.X, e.Y, e.Button );
    }



    private void pictureEditor_MouseUp( object sender, MouseEventArgs e )
    {
      HandleMouse( e.X, e.Y, e.Button );
    }



    private bool CombineElements( Project.Screen Screen, Project.ScreenElement Element1, Project.ScreenElement Element2, int X, int Y )
    {
      if ( ( Element2.X == X )
      &&   ( Element2.Y == Y ) )
      {
        if ( ( Element1.X == 34 )
        && ( Element1.Y == 16 ) )
        {
          Debug.Log( "aha" );
        }

        if ( ( Element1.Type == Element2.Type )
        ||   ( Element1.Type == Project.ScreenElementType.LD_ELEMENT )
        ||   ( Element2.Type == Project.ScreenElementType.LD_ELEMENT ) )
        {
          // attach to each other
          if ( Element1.Type == Project.ScreenElementType.LD_ELEMENT )
          {
            if ( X == Element1.X )
            {
              if ( ( Element2.Type != Project.ScreenElementType.LD_ELEMENT )
              &&   ( Element2.Type != Project.ScreenElementType.LD_ELEMENT_LINE_V ) )
              {
                return false;
              }
              Element1.Type = Project.ScreenElementType.LD_ELEMENT_LINE_V;
            }
            else
            {
              if ( ( Element2.Type != Project.ScreenElementType.LD_ELEMENT )
              &&   ( Element2.Type != Project.ScreenElementType.LD_ELEMENT_LINE_H ) )
              {
                return false;
              }
              Element1.Type = Project.ScreenElementType.LD_ELEMENT_LINE_H;
            }
            Element1.Repeats = 2;
            if ( ( Element2.Type == Project.ScreenElementType.LD_ELEMENT_LINE_H )
            ||   ( Element2.Type == Project.ScreenElementType.LD_ELEMENT_LINE_V ) )
            {
              Element1.Repeats = 1 + Element2.Repeats;
            }
            Screen.DisplayedElements.Remove( Element2 );
            return true;
          }
          if ( Element1.Type == Project.ScreenElementType.LD_ELEMENT_LINE_H )
          {
            if ( Element2.Type == Project.ScreenElementType.LD_ELEMENT_LINE_H )
            {
              Element1.Repeats += Element2.Repeats;
            }
            else if ( Element2.Type == Project.ScreenElementType.LD_ELEMENT )
            {
              ++Element1.Repeats;
            }
            Screen.DisplayedElements.Remove( Element2 );
            return true;
          }
          if ( Element1.Type == Project.ScreenElementType.LD_ELEMENT_LINE_V )
          {
            if ( Element2.Type == Project.ScreenElementType.LD_ELEMENT_LINE_V )
            {
              Element1.Repeats += Element2.Repeats;
            }
            else if ( Element2.Type == Project.ScreenElementType.LD_ELEMENT )
            {
              ++Element1.Repeats;
            }
            Screen.DisplayedElements.Remove( Element2 );
            return true;
          }
        }
      }
      return false;
    }



    private System.Drawing.Point WrapPos( System.Drawing.Point Pos )
    {
      int     wrapX = 40;
      if ( ( m_CurrentScreen != null )
      &&   ( m_CurrentScreen.Width != 40 ) )
      {
        wrapX = m_CurrentScreen.Width;
      }

      while ( Pos.X >= wrapX )
      {
        Pos.X -= wrapX;
        Pos.Y++;
      }
      return Pos;
    }



    private GR.Collections.Set<System.Drawing.Point> AffectedCharacters( Project.ScreenElement ScreenElement )
    {
      GR.Collections.Set<System.Drawing.Point>    affectedPoints = new GR.Collections.Set<Point>();

      if ( ScreenElement.Index == -1 )
      {
        return affectedPoints;
      }
      Project.Element element = m_Project.Elements[ScreenElement.Index];

      switch ( ScreenElement.Type )
      {
        case Project.ScreenElementType.LD_ELEMENT:
        case Project.ScreenElementType.LD_SEARCH_OBJECT:
        case Project.ScreenElementType.LD_DOOR:
        case Project.ScreenElementType.LD_CLUE:
        case Project.ScreenElementType.LD_SPECIAL:
          for ( int i = 0; i < element.Characters.Width; ++i )
          {
            for ( int j = 0; j < element.Characters.Height; ++j )
            {
              affectedPoints.Add( WrapPos( new Point( ScreenElement.X + i, ScreenElement.Y + j ) ) );
            }
          }
          break;
        case Project.ScreenElementType.LD_ELEMENT_LINE_H:
          {
            int w = ScreenElement.Repeats;
            for ( int i = 0; i < w * element.Characters.Width; ++i )
            {
              for ( int j = 0; j < element.Characters.Height; ++j )
              {
                affectedPoints.Add( WrapPos( new Point( ScreenElement.X + i, ScreenElement.Y + j ) ) );
              }
            }
          }
          break;
        case Project.ScreenElementType.LD_ELEMENT_LINE_V:
          {
            int h = ScreenElement.Repeats;
            for ( int i = 0; i < element.Characters.Width; ++i )
            {
              for ( int j = 0; j < h * element.Characters.Height; ++j )
              {
                affectedPoints.Add( WrapPos( new Point( ScreenElement.X + i, ScreenElement.Y + j ) ) );
              }
            }
          }
          break;
        case Project.ScreenElementType.LD_ELEMENT_AREA:
          {
            int w = ScreenElement.Repeats;
            int h = ScreenElement.Repeats2;
            for ( int i = 0; i < w * element.Characters.Width; ++i )
            {
              for ( int j = 0; j < h * element.Characters.Height; ++j )
              {
                affectedPoints.Add( WrapPos( new Point( ScreenElement.X + i, ScreenElement.Y + j ) ) );
              }
            }
          }
          break;
        case Project.ScreenElementType.LD_LINE_H:
        case Project.ScreenElementType.LD_LINE_H_ALT:
          {
            int w = ScreenElement.Repeats;
            for ( int i = 0; i < w; ++i )
            {
              affectedPoints.Add( WrapPos( new Point( ScreenElement.X + i, ScreenElement.Y ) ) );
            }
          }
          break;
        case Project.ScreenElementType.LD_LINE_V:
        case Project.ScreenElementType.LD_LINE_V_ALT:
          {
            int h = ScreenElement.Repeats;
            for ( int i = 0; i < h; ++i )
            {
              affectedPoints.Add( WrapPos( new Point( ScreenElement.X, ScreenElement.Y + i ) ) );
            }
          }
          break;
        case Project.ScreenElementType.LD_AREA:
          {
            int w = ScreenElement.Repeats;
            int h = ScreenElement.Repeats2;
            for ( int j = 0; j < w; ++j )
            {
              for ( int i = 0; i < h; ++i )
              {
                affectedPoints.Add( WrapPos( new Point( ScreenElement.X + j, ScreenElement.Y + i ) ) );
              }
            }
          }
          break;
      }

      return affectedPoints;
    }



    private bool ScreenElementsOverlap( Project.ScreenElement Element1, Project.ScreenElement Element2 )
    {
      GR.Collections.Set<System.Drawing.Point> points1 = new GR.Collections.Set<Point>();
      GR.Collections.Set<System.Drawing.Point> points2 = new GR.Collections.Set<Point>();

      points1 = AffectedCharacters( Element1 );
      points2 = AffectedCharacters( Element2 );

      foreach ( System.Drawing.Point point in points1 )
      {
        if ( points2.ContainsValue( point ) )
        {
          return true;
        }
      }
      return false;
    }



    private void autoptimizeToolStripMenuItem_Click( object sender, EventArgs e )
    {
      int     reducedElements = 0;

      // check if there are similar elements
      GR.Collections.Map<string, int> elementUsage = new GR.Collections.Map<string, int>();

      int elementIndex = 0;
      foreach ( Project.Element element in m_Project.Elements )
      {
        elementUsage[m_Project.Elements[elementIndex].Name] = 0;
        foreach ( Project.Element otherElement in m_Project.Elements )
        {
          if ( element != otherElement )
          {
            if ( ( element.Characters.Width == otherElement.Characters.Width )
            &&   ( element.Characters.Height == otherElement.Characters.Height ) )
            {
              for ( int i = 0; i < element.Characters.Width; ++i )
              {
                for ( int j = 0; j < element.Characters.Height; ++j )
                {
                  if ( ( element.Characters[i, j].Char != otherElement.Characters[i, j].Char )
                  ||   ( element.Characters[i, j].Color != otherElement.Characters[i, j].Color ) )
                  {
                    goto checkfailed;
                  }
                }
              }
              // elements are the same!!
              Debug.Log( "Element " + element.Name + " = " + otherElement.Name );
              checkfailed:;
            }
          }
        }
        ++elementIndex;
      }

      foreach ( Project.Screen screen in m_Project.Screens )
      {
        redo_screen:;
        foreach ( Project.ScreenElement element in screen.DisplayedElements )
        {
          foreach ( Project.ScreenElement otherElement in screen.DisplayedElements )
          {
            if ( ( element != otherElement )
            &&   ( element.Index == otherElement.Index ) )
            {
              if ( ( element.Type == Project.ScreenElementType.LD_ELEMENT )
              ||   ( element.Type == Project.ScreenElementType.LD_ELEMENT_LINE_H )
              ||   ( element.Type == Project.ScreenElementType.LD_ELEMENT_LINE_V ) )
              {
                if ( ( ( element.Type == Project.ScreenElementType.LD_ELEMENT_LINE_H )
                &&     ( CombineElements( screen, element, otherElement, element.X + m_Project.Elements[element.Index].Characters.Width * element.Repeats, element.Y ) ) )
                ||   ( ( element.Type == Project.ScreenElementType.LD_ELEMENT_LINE_V )
                &&     ( CombineElements( screen, element, otherElement, element.X, element.Y + m_Project.Elements[element.Index].Characters.Height * element.Repeats ) ) )
                ||   ( ( element.Type == Project.ScreenElementType.LD_ELEMENT )
                &&     ( CombineElements( screen, element, otherElement, element.X + m_Project.Elements[element.Index].Characters.Width, element.Y ) ) )
                ||   ( ( element.Type == Project.ScreenElementType.LD_ELEMENT )
                &&     ( CombineElements( screen, element, otherElement, element.X, element.Y + m_Project.Elements[element.Index].Characters.Height ) ) ) )
                {
                  ++reducedElements;
                  Modified = true;
                  goto redo_screen;
                }
              }
            }
          }
        }
      }

      // resort elements to keep similar elements ones behind each other
      int     screenIndex = 0;
      foreach ( Project.Screen screen in m_Project.Screens )
      {
        for ( int i = 0; i < screen.DisplayedElements.Count; ++i )
        {
          Project.ScreenElement screenElement = screen.DisplayedElements[i];
          if ( i == 0 )
          {
            continue;
          }

          if ( ScreenElementUsesElement( screenElement ) )
          {
            // find previous element with same element index
            Project.ScreenElement prevScreenElement = null;
            int prevIndex = 0;
            for ( prevIndex = i - 1; prevIndex >= 0; --prevIndex )
            {
              if ( ( ScreenElementUsesElement( screen.DisplayedElements[prevIndex] ) )
              &&   ( screen.DisplayedElements[prevIndex].Index == screenElement.Index ) )
              {
                prevScreenElement = screen.DisplayedElements[prevIndex];
                break;
              }
            }
            if ( prevScreenElement == null )
            {
              continue;
            }
            if ( prevIndex == i - 1 )
            {
              // already neighbours
              continue;
            }
            // check if any element between both is overlapping the potential prev neighbour element
            bool overlapExists = false;
            for ( int intermediateElementIndex = i - 1; intermediateElementIndex > prevIndex; --intermediateElementIndex )
            {
              if ( ( ScreenElementsOverlap( screen.DisplayedElements[intermediateElementIndex], screen.DisplayedElements[prevIndex] ) )
              ||   ( ScreenElementsOverlap( screen.DisplayedElements[intermediateElementIndex], screen.DisplayedElements[i] ) ) )
              {
                overlapExists = true;
                break;
              }
            }
            if ( !overlapExists )
            {
              // move display element up
              screen.DisplayedElements.RemoveAt( i );
              screen.DisplayedElements.Insert( prevIndex + 1, screenElement );
              Debug.Log( "Screen " + screen.Name + ", move from " + i + " to " + ( prevIndex + 1 ) );
            }
          }
          else if ( ScreenElementHasNoChars( screenElement ) )
          {
            // uses no characters, can sort without issue
            // find previous element with same element index
            Project.ScreenElement prevScreenElement = null;
            int prevIndex = 0;
            for ( prevIndex = i - 1; prevIndex >= 0; --prevIndex )
            {
              if ( ( ScreenElementHasNoChars( screen.DisplayedElements[prevIndex] ) )
              &&   ( screen.DisplayedElements[prevIndex].Type == screenElement.Type ) )
              {
                prevScreenElement = screen.DisplayedElements[prevIndex];
                break;
              }
            }
            if ( prevScreenElement == null )
            {
              continue;
            }
            // move display element up
            if ( i != prevIndex + 1 )
            {
              screen.DisplayedElements.RemoveAt( i );
              screen.DisplayedElements.Insert( prevIndex + 1, screenElement );
              Debug.Log( "Screen " + screenIndex + "/" + screen.Name + ", move non char element from " + i + " to " + ( prevIndex + 1 ) );
            }
          }
        }
        ++screenIndex;
      }

      // resort elements of same types so same primitives are near each other
      screenIndex = 0;
      foreach ( Project.Screen screen in m_Project.Screens )
      {
        for ( int i = 0; i < screen.DisplayedElements.Count; ++i )
        {
          Project.ScreenElement screenElement = screen.DisplayedElements[i];
          if ( i == 0 )
          {
            continue;
          }

          if ( ScreenElementUsesElement( screenElement ) )
          {
            Project.ScreenElement otherElement = null;
            int     nextIndex = i + 1;
            while ( nextIndex < screen.DisplayedElements.Count )
            {
              otherElement = screen.DisplayedElements[nextIndex];

              if ( ( !ScreenElementUsesElement( otherElement ) )
              ||   ( otherElement.Index != screenElement.Index ) )
              {
                otherElement = null;
                break;
              }
              if ( otherElement.Type == screenElement.Type )
              {
                // same, could be close to each other?
                break;
              }
              ++nextIndex;
            }
            // found nothing
            if ( ( nextIndex == screen.DisplayedElements.Count )
            ||   ( otherElement == null ) )
            {
              continue;
            }
            if ( nextIndex == i + 1 )
            {
              // already neighbours
              continue;
            }
            // check if any element between both is overlapping the potential prev neighbour element
            bool overlapExists = false;
            for ( int intermediateElementIndex = i; intermediateElementIndex < nextIndex; ++intermediateElementIndex )
            {
              if ( ( ScreenElementsOverlap( screen.DisplayedElements[intermediateElementIndex], screen.DisplayedElements[nextIndex] ) )
              ||   ( ScreenElementsOverlap( screen.DisplayedElements[intermediateElementIndex], screen.DisplayedElements[i] ) ) )
              {
                overlapExists = true;
                break;
              }
            }
            if ( !overlapExists )
            {
              // move display element up
              screen.DisplayedElements.RemoveAt( nextIndex );
              screen.DisplayedElements.Insert( i + 1, screenElement );
              Debug.Log( "Screen " + screen.Name + ", move from " + nextIndex + " to " + ( i + 1 ) );
            }
          }
        }
        ++screenIndex;
      }

      // check if any elements are completely hidden
      screenIndex = 0;
      foreach ( Project.Screen screen in m_Project.Screens )
      {
        for ( int origIndex = 0; origIndex < screen.DisplayedElements.Count; ++origIndex )
        {
          Project.ScreenElement screenElement = screen.DisplayedElements[origIndex];
          if ( !ScreenElementUsesElement( screenElement ) )
          {
            continue;
          }
          GR.Collections.Set<System.Drawing.Point> origPoints = AffectedCharacters( screenElement );
          for ( int laterIndex = origIndex + 1; laterIndex < screen.DisplayedElements.Count; ++laterIndex )
          {
            GR.Collections.Set<System.Drawing.Point> newPoints = AffectedCharacters( screen.DisplayedElements[laterIndex] );

            foreach ( System.Drawing.Point point in newPoints )
            {
              if ( origPoints.ContainsValue( point ) )
              {
                origPoints.Remove( point );
              }
            }
          }
          if ( origPoints.Count == 0 )
          {
            Debug.Log( "Screen " + screenIndex + "/" + screen.Name + ", Element " + origIndex + " is completely obstructed" + System.Environment.NewLine );
          }
        }
        ++screenIndex;
      }


      foreach ( Project.Screen screen in m_Project.Screens )
      {
        foreach ( Project.ScreenElement element in screen.DisplayedElements )
        {
          if ( ScreenElementUsesElement( element ) )
          {
            elementUsage[m_Project.Elements[element.Index].Name]++;
          }

          if ( ( element.Type == Project.ScreenElementType.LD_ELEMENT_LINE_H )
          ||   ( element.Type == Project.ScreenElementType.LD_ELEMENT_LINE_V ) )
          {
            if ( element.Repeats == 1 )
            {
              element.Type = Project.ScreenElementType.LD_ELEMENT;
            }
          }
          if ( element.Type == Project.ScreenElementType.LD_ELEMENT_AREA )
          {
            if ( ( element.Repeats == 1 )
            &&   ( element.Repeats2 == 1 ) )
            {
              element.Type = Project.ScreenElementType.LD_ELEMENT;
            }
            else if ( element.Repeats == 1 )
            {
              element.Type = Project.ScreenElementType.LD_ELEMENT_LINE_V;
              element.Repeats = element.Repeats2;
            }
            else if ( element.Repeats2 == 1 )
            {
              element.Type = Project.ScreenElementType.LD_ELEMENT_LINE_H;
            }
          }
          while ( element.X >= 40 )
          {
            element.X -= 40;
            element.Y++;
          }
          if ( ( element.X < 0 )
          ||   ( element.Y < 0 )
          ||   ( element.Y >= 25 )
          ||   ( element.X >= 40 ) )
          {
            Debug.Log( "Screen " + screen.Name + ", Element outside" );
          }
        }
      }
      if ( reducedElements > 0 )
      {
        comboScreens_SelectedIndexChanged( null, null );
      }
      foreach ( KeyValuePair<string, int> kv in elementUsage )
      {
        if ( kv.Value == 0 )
        {
          Debug.Log( "Unused element " + kv.Key );
        }
      }
      Debug.Log( "Reduced " + reducedElements + " elements" );
    }



    private bool ScreenElementHasNoChars( Project.ScreenElement Element )
    {
      if ( ( Element.Type == Project.ScreenElementType.LD_OBJECT )
      ||   ( Element.Type == Project.ScreenElementType.LD_SPAWN_SPOT ) )
      {
        return true;
      }
      return false;
    }



    private bool ScreenElementUsesElement( Project.ScreenElement Element )
    {
      if ( ( Element.Type == Project.ScreenElementType.LD_DOOR )
      ||   ( Element.Type == Project.ScreenElementType.LD_ELEMENT )
      ||   ( Element.Type == Project.ScreenElementType.LD_SEARCH_OBJECT )
      ||   ( Element.Type == Project.ScreenElementType.LD_SPECIAL )
      ||   ( Element.Type == Project.ScreenElementType.LD_CLUE )
      ||   ( Element.Type == Project.ScreenElementType.LD_ELEMENT_AREA )
      ||   ( Element.Type == Project.ScreenElementType.LD_ELEMENT_LINE_H )
      ||   ( Element.Type == Project.ScreenElementType.LD_ELEMENT_LINE_V ) )
      {
        return true;
      }
      return false;
    }



    private void btnDeleteElementTemplate_Click( object sender, EventArgs e )
    {
      if ( listAvailableElements.SelectedIndex == -1 )
      {
        return;
      }
      int   elementIndex = listAvailableElements.SelectedIndex;
      string selItem = listAvailableElements.Items[listAvailableElements.SelectedIndex].ToString();

      int   elementInstanceCount = 0;

      foreach ( Project.Screen screen in m_Project.Screens )
      {
        foreach ( Project.ScreenElement element in screen.DisplayedElements )
        {
          if ( ScreenElementUsesElement( element ) )
          {
            if ( element.Index == elementIndex )
            {
              ++elementInstanceCount;
            }
          }
        }
      }

      if ( elementInstanceCount > 0 )
      {
        if ( System.Windows.Forms.MessageBox.Show( "Deleting the element template removes all used instances (" + elementInstanceCount.ToString() + ") from the screens! Are you sure you want to do that?", "Delete Element Template?", MessageBoxButtons.YesNo ) == DialogResult.No )
        {
          return;
        }
      }
      for ( int i = elementIndex; i < m_Project.Elements.Count; ++i )
      {
        m_Project.Elements[i].Index--;
      }
      m_Project.Elements.RemoveAt( elementIndex );
      listAvailableElements.Items.RemoveAt( elementIndex );
      comboElements.Items.RemoveAt( elementIndex );

      foreach ( Project.Screen screen in m_Project.Screens )
      {
        redo_screen:;
        foreach ( Project.ScreenElement element in screen.DisplayedElements )
        {
          if ( ScreenElementUsesElement( element ) )
          {
            if ( element.Index == elementIndex )
            {
              screen.DisplayedElements.Remove( element );
              goto redo_screen;
            }
            else if ( element.Index >= elementIndex )
            {
              --element.Index;
            }
          }
        }
      }
      comboScreens_SelectedIndexChanged( null, null );
    }



    private void menuExternalImportSpriteset_Click( object sender, EventArgs e )
    {
      OpenFileDialog openFile = new OpenFileDialog();

      openFile.Title = "Open sprite project";
      openFile.Filter = "Sprite Project Files|*.spriteproject";

      if ( openFile.ShowDialog() == DialogResult.OK )
      {
        OpenSpriteProject( openFile.FileName );
      }
    }



    private void OpenSpriteProject( string Filename )
    {
      GR.Memory.ByteBuffer    projectFile = GR.IO.File.ReadAllBytes( Filename );
      if ( projectFile == null )
      {
        return;
      }

      if ( !m_SpriteProject.ReadFromBuffer( projectFile ) )
      {
        return;
      }

      /*
      GR.IO.MemoryReader      memIn = projectFile.MemoryReader();

      uint     Version = memIn.ReadUInt32();
      string name = memIn.ReadString();
      for ( int i = 0; i < 256; ++i )
      {
        m_Sprites[i].Color = memIn.ReadInt32();
      }
      for ( int i = 0; i < 256; ++i )
      {
        m_Sprites[i].Multicolor = ( memIn.ReadUInt8() != 0 );
      }
      m_BackgroundColorSprites = memIn.ReadInt32();
      m_SpriteMultiColor1 = memIn.ReadInt32();
      m_SpriteMultiColor2 = memIn.ReadInt32();

      bool genericMultiColor = ( memIn.ReadUInt32() != 0 );
      for ( int i = 0; i < 256; ++i )
      {
        GR.Memory.ByteBuffer tempBuffer = new GR.Memory.ByteBuffer();

        memIn.ReadBlock( tempBuffer, 64 );
        tempBuffer.CopyTo( m_Sprites[i].Data, 0, 63 );
      }

      int usedSprites = memIn.ReadInt32();

      string exportName = memIn.ReadString();
      string exportPathSprietFile = memIn.ReadString();
      for ( int i = 0; i < 256; ++i )
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
      */
      listSprites.Items.Clear();
      for ( int i = 0; i < 256; ++i )
      {
        RebuildSpriteImage( i, -1 );
        listSprites.Items.Add( m_SpriteProject.Sprites[i], m_SpriteProject.Sprites[i].Tile.Image );
      }

      m_Project.SpriteProjectFilename = Filename;
      Modified = false;
    }



    private void btnNewObject_Click( object sender, EventArgs e )
    {
      Project.ObjectTemplate    template = new Project.ObjectTemplate();

      template.Name = editObjectTemplateName.Text;
      template.Index = m_Project.ObjectTemplates.Count;

      m_Project.ObjectTemplates.Add( template );

      listAvailableObjects.Items.Add( template );
      comboObjects.Items.Add( template );

      Modified = true;
    }



    private void btnDeleteObject_Click( object sender, EventArgs e )
    {
      int     selectedIndex = listAvailableObjects.SelectedIndex;
      if ( selectedIndex == -1 )
      {
        return;
      }

      Project.ObjectTemplate    template = (Project.ObjectTemplate)listAvailableObjects.SelectedItem;
      bool                      stillUsed = false;

      foreach ( var screen in m_Project.Screens )
      {
        foreach ( var element in screen.DisplayedElements )
        {
          if ( ( element.Type == Project.ScreenElementType.LD_OBJECT )
          ||   ( element.Type == Project.ScreenElementType.LD_SPAWN_SPOT ) )
          {
            if ( ( element.Object != null )
            &&   ( element.Object.TemplateIndex == selectedIndex ) )
            {
              stillUsed = true;
              break;
            }
          }
        }
        if ( stillUsed )
        {
          break;
        }
      }

      if ( stillUsed )
      {
        System.Windows.Forms.MessageBox.Show( "The object template " + template.Name + " is still in use by objects." );
        return;
      }

      m_Project.ObjectTemplates.RemoveAt( selectedIndex );

      foreach ( var screen in m_Project.Screens )
      {
        foreach ( var element in screen.DisplayedElements )
        {
          if ( ( element.Type == Project.ScreenElementType.LD_OBJECT )
          ||   ( element.Type == Project.ScreenElementType.LD_SPAWN_SPOT ) )
          {
            if ( element.Object.TemplateIndex >= selectedIndex )
            {
              --element.Object.TemplateIndex;
              element.Object.SpriteImage = new GR.Image.MemoryImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Tile.Image );

              RebuildSpriteImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Tile,
                                  m_SpriteProject.Colors.Palette, 
                                  element.Object.SpriteImage,
                                  m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Mode,
                                  element.Object.Color );
            }
          }
        }
      }
      listAvailableObjects.Items.RemoveAt( selectedIndex );
      comboObjects.Items.RemoveAt( selectedIndex );

      Modified = true;
    }



    private void listAvailableObjects_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( listAvailableObjects.SelectedIndex == -1 )
      {
        btnDeleteObject.Enabled = false;
        btnAddBehaviour.Enabled = false;
        editBehaviourName.Enabled = false;
        btnDeleteBehaviour.Enabled = false;
        return;
      }
      Project.ObjectTemplate template = (Project.ObjectTemplate)listAvailableObjects.SelectedItem;

      listSprites.SelectedIndex = template.StartSprite;
      editObjectTemplateName.Text = template.Name;
      btnDeleteObject.Enabled = true;
      editBehaviourName.Enabled = true;
      btnDeleteBehaviour.Enabled = false;

      listObjectBehaviours.Items.Clear();
      foreach ( KeyValuePair<int, Project.Behaviour> behaviour in template.Behaviours )
      {
        ListViewItem item = new ListViewItem( behaviour.Value.Name );
        item.SubItems.Add( behaviour.Value.Value.ToString() );
        listObjectBehaviours.Items.Add( item );
      }
    }



    void listSprites_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( listAvailableObjects.SelectedIndex == -1 )
      {
        return;
      }
      Project.ObjectTemplate template = (Project.ObjectTemplate)listAvailableObjects.SelectedItem;

      if ( template.StartSprite != listSprites.SelectedIndex )
      {
        template.StartSprite = listSprites.SelectedIndex;
        labelSpriteNo.Text = listSprites.SelectedIndex.ToString();

        listAvailableObjects.Items[template.Index] = template;
        Modified = true;
      }

    }



    private void comboObjects_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];
      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      comboObjectBehaviour.Items.Clear();

      if ( comboObjects.SelectedIndex != -1 )
      {
        // enum all behaviours for this object type
        foreach ( KeyValuePair<int, Project.Behaviour> behaviour in m_Project.ObjectTemplates[comboObjects.SelectedIndex].Behaviours )
        {
          comboObjectBehaviour.Items.Add( behaviour.Value );
        }
      }

      if ( screenElement.Object.TemplateIndex != comboObjects.SelectedIndex )
      {
        screenElement.Object.TemplateIndex = comboObjects.SelectedIndex;
        if ( screenElement.Object.SpriteImage == null )
        {
          screenElement.Object.SpriteImage = new GR.Image.MemoryImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].StartSprite].Tile.Image );
        }
        RebuildSpriteImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].StartSprite].Tile,
                            m_SpriteProject.Colors.Palette, 
                            screenElement.Object.SpriteImage,
                            m_SpriteProject.Sprites[m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].StartSprite].Mode,
                            screenElement.Object.Color );

        Modified = true;
        RedrawScreen();
      }
    }



    private void editObjectTemplateName_TextChanged( object sender, EventArgs e )
    {
      if ( listAvailableObjects.SelectedIndex == -1 )
      {
        return;
      }
      Project.ObjectTemplate template = (Project.ObjectTemplate)listAvailableObjects.SelectedItem;

      if ( template.Name != editObjectTemplateName.Text )
      {
        template.Name = editObjectTemplateName.Text;
        listAvailableObjects.Items[template.Index] = template;
        Modified = true;
      }
    }



    private void editObjectBorderLeft_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];
      if ( screenElement.Type == Project.ScreenElementType.LD_OBJECT )
      {
        if ( ( screenElement.Object != null )
        &&   ( GR.Convert.ToI32( editObjectBorderLeft.Text ) != screenElement.Object.MoveBorderLeft ) )
        {
          screenElement.Object.MoveBorderLeft = GR.Convert.ToI32( editObjectBorderLeft.Text );
          Modified = true;
          RedrawScreen();
        }
      }
      else if ( screenElement.Type == Project.ScreenElementType.LD_DOOR )
      {
        screenElement.TargetX = GR.Convert.ToI32( editObjectBorderLeft.Text );
        Modified = true;
      }
    }



    private void editObjectBorderTop_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( screenElement.Type == Project.ScreenElementType.LD_OBJECT )
      {
        if ( ( screenElement.Object != null )
        &&   ( GR.Convert.ToI32( editObjectBorderTop.Text ) != screenElement.Object.MoveBorderTop ) )
        {
          screenElement.Object.MoveBorderTop = GR.Convert.ToI32( editObjectBorderTop.Text );
          Modified = true;
          RedrawScreen();
        }
      }
      else if ( screenElement.Type == Project.ScreenElementType.LD_DOOR )
      {
        screenElement.TargetY = GR.Convert.ToI32( editObjectBorderTop.Text );
        Modified = true;
      }
    }



    private void editObjectBorderRight_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( screenElement.Type == Project.ScreenElementType.LD_OBJECT )
      {
        if ( ( screenElement.Object != null )
        &&   ( GR.Convert.ToI32( editObjectBorderRight.Text ) != screenElement.Object.MoveBorderRight ) )
        {
          screenElement.Object.MoveBorderRight = GR.Convert.ToI32( editObjectBorderRight.Text );
          Modified = true;
          RedrawScreen();
        }
      }
      else if ( screenElement.Type == Project.ScreenElementType.LD_DOOR )
      {
        screenElement.TargetLevel = GR.Convert.ToI32( editObjectBorderRight.Text );
        Modified = true;
      }
    }



    private void editObjectBorderBottom_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( ( screenElement.Object != null )
      &&    ( GR.Convert.ToI32( editObjectBorderBottom.Text ) != screenElement.Object.MoveBorderBottom ) )
      {
        screenElement.Object.MoveBorderBottom = GR.Convert.ToI32( editObjectBorderBottom.Text );
        Modified = true;
        RedrawScreen();
      }
    }



    private void editObjectSpeed_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( ( screenElement.Object != null )
      &&   ( GR.Convert.ToI32( editObjectSpeed.Text ) != screenElement.Object.Speed ) )
      {
        screenElement.Object.Speed = GR.Convert.ToI32( editObjectSpeed.Text );
        Modified = true;
        RedrawScreen();
      }
    }



    private void editBehaviourName_TextChanged( object sender, EventArgs e )
    {
      if ( listAvailableObjects.SelectedIndices.Count == 0 )
      {
        return;
      }
      btnAddBehaviour.Enabled = ( editBehaviourName.Text.Length > 0 );
      if ( listObjectBehaviours.SelectedItems.Count == 0 )
      {
        return;
      }
      Project.ObjectTemplate obj = (Project.ObjectTemplate)listAvailableObjects.SelectedItem;

      obj.Behaviours[listObjectBehaviours.SelectedIndices[0]].Name = editBehaviourName.Text;
      listObjectBehaviours.Items[listObjectBehaviours.SelectedIndices[0]].SubItems[0].Text = editBehaviourName.Text;
      Modified = true;
    }



    private void listObjectBehaviours_SelectedIndexChanged( object sender, EventArgs e )
    {
      btnDeleteBehaviour.Enabled = ( listObjectBehaviours.SelectedItems.Count > 0 );

      if ( listAvailableObjects.SelectedIndices.Count == 0 )
      {
        return;
      }
      if ( listObjectBehaviours.SelectedItems.Count == 0 )
      {
        return;
      }
      Project.ObjectTemplate obj = (Project.ObjectTemplate)listAvailableObjects.SelectedItem;
      editBehaviourName.Text = obj.Behaviours[listObjectBehaviours.SelectedIndices[0]].Name;
      editBehaviourNo.Text = obj.Behaviours[listObjectBehaviours.SelectedIndices[0]].Value.ToString();
    }



    private void btnAddBehaviour_Click( object sender, EventArgs e )
    {
      if ( listAvailableObjects.SelectedIndices.Count == 0 )
      {
        return;
      }
      Project.ObjectTemplate obj = (Project.ObjectTemplate)listAvailableObjects.SelectedItem;

      int newIndex = obj.Behaviours.Count;
      obj.Behaviours[newIndex] = new Project.Behaviour();
      obj.Behaviours[newIndex].Name = editBehaviourName.Text;
      obj.Behaviours[newIndex].Value = GR.Convert.ToI32( editBehaviourNo.Text );

      ListViewItem item = new ListViewItem( editBehaviourName.Text );
      item.SubItems.Add( obj.Behaviours[newIndex].Value.ToString() );
      listObjectBehaviours.Items.Add( item );
    }



    private void comboObjectBehaviour_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( ( screenElement.Object != null )
      &&   ( comboObjectBehaviour.SelectedIndex != screenElement.Object.Behaviour ) )
      {
        screenElement.Object.Behaviour = comboObjectBehaviour.SelectedIndex;
        Modified = true;
      }
    }



    private void comboObjectOptional_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( ( screenElement.Object != null )
      &&   ( comboObjectOptional.SelectedIndex != (int)screenElement.Object.Optional ) )
      {
        screenElement.Object.Optional = (Project.GameObject.OptionalType)comboObjectOptional.SelectedIndex;
        Modified = true;
      }
    }



    private void comboProjectType_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( m_Project.ProjectType != (string)comboProjectType.SelectedItem )
      {
        m_Project.ProjectType = (string)comboProjectType.SelectedItem;
        RedrawScreen();
        Modified = true;
      }
      editScreenWidth.Enabled = ProjectTypeWantsScreenSize();
      editScreenHeight.Enabled = ProjectTypeWantsScreenSize();
      if ( !ProjectTypeWantsScreenSize() )
      {
        editScreenWidth.Text = "40";
        editScreenHeight.Text = "25";
      }
      comboScreenObjectFlags.Enabled = ProjectTypeAllowsFlagsInXPos();
      labelWonderlandBaseScreenConfig.Enabled = ( m_Project.ProjectType == "Wonderland" );
      editWonderlandBaseScreenConfig.Enabled = ( m_Project.ProjectType == "Wonderland" );
    }



    private void btnApplyScreenSize_Click( object sender, EventArgs e )
    {
      if ( m_CurrentScreen == null )
      {
        return;
      }
      m_CurrentScreen.Width   = GR.Convert.ToI32( editScreenWidth.Text );
      m_CurrentScreen.Height  = GR.Convert.ToI32( editScreenHeight.Text );
      m_ScreenContent.Resize( m_CurrentScreen.Width, m_CurrentScreen.Height );
      scrollScreen.Maximum    = m_CurrentScreen.Width - 40;
      scrollScreenV.Maximum   = m_CurrentScreen.Height - 25;
      Modified = true;
    }



    private void ValidateScreenSize()
    {
      int width = GR.Convert.ToI32( editScreenWidth.Text );
      int height = GR.Convert.ToI32( editScreenHeight.Text );
      if ( ( width < 40 )
      ||   ( width > 320 )
      ||   ( height < 25 )
      ||   ( height >= 40 * 25 ) )
      {
        btnApplyScreenSize.Enabled = false;
      }
      else
      {
        btnApplyScreenSize.Enabled = true;
      }
    }



    private void editScreenWidth_TextChanged( object sender, EventArgs e )
    {
      ValidateScreenSize();
    }



    private void editScreenHeight_TextChanged( object sender, EventArgs e )
    {
      ValidateScreenSize();
    }



    private void scrollScreen_ValueChanged( object sender, EventArgs e )
    {
      m_ScreenOffsetX = scrollScreen.Value;
      RedrawScreen();
    }



    private void scrollScreenV_ValueChanged( object sender, EventArgs e )
    {
      m_ScreenOffsetY = scrollScreenV.Value;
      RedrawScreen();
    }



    private void editElementRepeat2_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( screenElement.Repeats2 != GR.Convert.ToI32( editElementRepeat2.Text ) )
      {
        screenElement.Repeats2 = GR.Convert.ToI32( editElementRepeat2.Text );
        listScreenElements.Items[elementIndex].SubItems[5].Text = editElementRepeat2.Text;
        RedrawScreen();
        Modified = true;
      }
    }



    private void editBehaviourNo_TextChanged( object sender, EventArgs e )
    {
      if ( listAvailableObjects.SelectedIndices.Count == 0 )
      {
        return;
      }
      if ( listObjectBehaviours.SelectedItems.Count == 0 )
      {
        return;
      }
      Project.ObjectTemplate obj = (Project.ObjectTemplate)listAvailableObjects.SelectedItem;

      int no = GR.Convert.ToI32( editBehaviourNo.Text );
      obj.Behaviours[listObjectBehaviours.SelectedIndices[0]].Value = no;
      listObjectBehaviours.Items[listObjectBehaviours.SelectedIndices[0]].SubItems[1].Text = no.ToString();
      Modified = true;
    }



    private void btnMoveScreenDown_Click( object sender, EventArgs e )
    {
      if ( comboScreens.SelectedIndex + 1 >= m_Project.Screens.Count )
      {
        return;
      }
      int   otherIndex = comboScreens.SelectedIndex;
      Project.Screen    otherScreen = m_Project.Screens[otherIndex];

      GR.Generic.Tupel<string,Project.Screen>   screen1 = (GR.Generic.Tupel<string,Project.Screen>)comboScreens.Items[otherIndex];
      GR.Generic.Tupel<string,Project.Screen>   screen2 = (GR.Generic.Tupel<string,Project.Screen>)comboScreens.Items[otherIndex + 1];
      m_Project.Screens.RemoveAt( otherIndex );
      m_Project.Screens.Insert( otherIndex + 1, otherScreen );

      screen1.first = ( otherIndex + 1 ).ToString() + ":" + screen1.second.Name;
      screen2.first = otherIndex.ToString() + ":" + screen2.second.Name;

      comboScreens.Items.RemoveAt( otherIndex );
      comboScreens.Items.Insert( otherIndex + 1, screen1 );
      comboScreens.Items[otherIndex] = comboScreens.Items[otherIndex];
      comboScreens.SelectedIndex = otherIndex + 1;

      comboRegionScreens.Items.RemoveAt( otherIndex );
      comboRegionScreens.Items.Insert( otherIndex + 1, screen1 );
      comboRegionScreens.Items[otherIndex] = comboRegionScreens.Items[otherIndex];

      Modified = true;
    }



    private void btnMoveScreenUp_Click( object sender, EventArgs e )
    {
      if ( comboScreens.SelectedIndex < 1 )
      {
        return;
      }
      int   otherIndex = comboScreens.SelectedIndex - 1;
      Project.Screen    otherScreen = m_Project.Screens[otherIndex];

      GR.Generic.Tupel<string,Project.Screen>   screen1 = (GR.Generic.Tupel<string, Project.Screen>)comboScreens.Items[otherIndex];
      GR.Generic.Tupel<string,Project.Screen>   screen2 = (GR.Generic.Tupel<string, Project.Screen>)comboScreens.Items[otherIndex + 1];

      m_Project.Screens.RemoveAt( otherIndex );
      m_Project.Screens.Insert( otherIndex + 1, otherScreen );

      screen1.first = ( otherIndex + 1 ).ToString() + ":" + screen1.second.Name;
      screen2.first = otherIndex.ToString() + ":" + screen2.second.Name;

      comboScreens.Items.RemoveAt( otherIndex );
      comboScreens.Items.Insert( otherIndex + 1, screen1 );
      comboScreens.Items[otherIndex] = comboScreens.Items[otherIndex];

      comboRegionScreens.Items.RemoveAt( otherIndex );
      comboRegionScreens.Items.Insert( otherIndex + 1, screen1 );
      comboRegionScreens.Items[otherIndex] = comboRegionScreens.Items[otherIndex];
      Modified = true;
    }



    private void editScreenConfig_TextChanged( object sender, EventArgs e )
    {
      byte newValue = 0;

      if ( byte.TryParse( editScreenConfig.Text, out newValue ) )
      {
        if ( m_CurrentScreen.ConfigByte != newValue )
        {
          m_CurrentScreen.ConfigByte = newValue;
          Modified = true;
        }
      }
    }



    private void comboScreenCharset_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( ( m_CurrentScreen != null )
      &&   ( m_CurrentScreen.CharsetIndex != comboScreenCharset.SelectedIndex ) )
      {
        m_CurrentScreen.CharsetIndex = comboScreenCharset.SelectedIndex;
        Modified = true;

        SetActiveScreenCharset( m_Project.Charsets[comboScreenCharset.SelectedIndex],
                                m_CurrentScreen.OverrideMC1 != -1 ? m_CurrentScreen.OverrideMC1 : m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor1,
                                m_CurrentScreen.OverrideMC2 != -1 ? m_CurrentScreen.OverrideMC2 : m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor2,
                                m_Project.CharsetProjects[comboScreenCharset.SelectedIndex].Multicolor );
      }
    }



    private void comboElementCharset_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( ( m_CurrentEditedElement != null )
      &&   ( m_CurrentEditedElement.CharsetIndex != comboElementCharset.SelectedIndex ) )
      {
        m_CurrentEditedElement.CharsetIndex = comboElementCharset.SelectedIndex;

        checkMCMode.Checked = m_Project.CharsetProjects[comboElementCharset.SelectedIndex].Multicolor;
        Modified = true;
      }
      bool    multiColor = true;
      if ( comboScreenCharset.SelectedIndex != -1 )
      {
        multiColor = m_Project.CharsetProjects[comboScreenCharset.SelectedIndex].Multicolor;
      }
      SetActiveElementCharset( m_Project.Charsets[comboElementCharset.SelectedIndex],
                               m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor1,
                               m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor2,
                               multiColor );
    }



    private void appendProjectToolStripMenuItem_Click( object sender, EventArgs e )
    {
      OpenFileDialog openFile = new OpenFileDialog();

      openFile.Title = "Open editor project";
      openFile.Filter = "Element Editor Project Files|*.elementeditorproject";

      if ( openFile.ShowDialog() != DialogResult.OK )
      {
        return;
      }

      Project otherProject = new Project();

      if ( !otherProject.LoadFromFile( openFile.FileName ) )
      {
        return;
      }

      int origNumCharsets = m_Project.CharsetProjects.Count;

      /*
      foreach ( Project.ObjectTemplate obj in otherProject.ObjectTemplates )
      {
        comboObjects.Items.Add( obj );
        listAvailableObjects.Items.Add( obj );
      }*/

      editExportFile.Text           = otherProject.ExportFilename;
      editExportPrefix.Text         = otherProject.ExportPrefix;
      editConstantOffset.Text       = otherProject.ExportConstantOffset.ToString();
      comboProjectType.SelectedItem = otherProject.ProjectType;

      /*
      m_Project.CharsetProjects.Clear();
      m_Project.CharsetProjects.Add( @"d:\privat\projekte\c64\j\studio\j.charsetproject" );
       */

      if ( ( otherProject.CharsetProjects.Count == 0 )
      &&   ( !string.IsNullOrEmpty( otherProject.OldCharsetProjectFilename ) ) )
      {
        string fullPath = GR.Path.Append( GR.Path.RemoveFileSpec( openFile.FileName ), otherProject.OldCharsetProjectFilename );
        CharsetProject charSet = OpenCharsetProject( fullPath );
        if ( charSet != null )
        {
          charSet.Name = fullPath;
          string shortName = System.IO.Path.GetFileNameWithoutExtension( fullPath );
          m_Project.Charsets.Add( charSet );

          CharsetProjectInfo info = new CharsetProjectInfo();
          info.Filename = fullPath;
          info.Multicolor = true;
          m_Project.CharsetProjects.Add( info );
          otherProject.CharsetProjects.Add( info );

          otherProject.Charsets.Add( charSet );
        }
      }
      else 
      {
        for ( int i = 0; i < otherProject.CharsetProjects.Count; ++i )
        {
          string charSetFile = otherProject.CharsetProjects[i].Filename;
          CharsetProject charSet = OpenCharsetProject( charSetFile );
          if ( charSet == null )
          {
            charSetFile = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( openFile.FileName ), System.IO.Path.GetFileName( charSetFile ) );

            CharsetProjectInfo info = new CharsetProjectInfo();
            info.Filename = charSetFile;
            info.Multicolor = otherProject.CharsetProjects[i].Multicolor;
            m_Project.CharsetProjects.Add( info );
            charSet = OpenCharsetProject( charSetFile );

            CharsetProjectInfo otherInfo = new CharsetProjectInfo();
            otherInfo.Filename = charSetFile;
            otherInfo.Multicolor = true;
            otherProject.CharsetProjects.Add( otherInfo );
          }
          else
          {
            CharsetProjectInfo info = new CharsetProjectInfo();
            info.Filename = charSetFile;
            info.Multicolor = true;

            m_Project.CharsetProjects.Add( info );
          }
          m_Project.Charsets.Add( charSet );
          otherProject.Charsets.Add( charSet );
        }
      }
      foreach ( CharsetProject charSet in otherProject.Charsets )
      {
        comboScreenCharset.Items.Add( System.IO.Path.GetFileNameWithoutExtension( charSet.Name ) );
        comboElementCharset.Items.Add( System.IO.Path.GetFileNameWithoutExtension( charSet.Name ) );
      }
      comboScreenCharset.SelectedIndex = 0;
      comboElementCharset.SelectedIndex = 0;
      /*
      if ( !string.IsNullOrEmpty( otherProject.SpriteProjectFilename ) )
      {
        string fullPath = GR.Path.Append( GR.Path.RemoveFileSpec( openFile.FileName ), otherProject.SpriteProjectFilename );
        OpenSpriteProject( fullPath );

        foreach ( Project.Screen screen in otherProject.Screens )
        {
          foreach ( Project.ScreenElement element in screen.DisplayedElements )
          {
            if ( ( element.Type == Project.ScreenElementType.LD_OBJECT )
            || ( element.Type == Project.ScreenElementType.LD_SPAWN_SPOT ) )
            {
              if ( ( element.Object != null )
              && ( element.Object.TemplateIndex != -1 ) )
              {
                element.Object.SpriteImage = new GR.Image.MemoryImage( m_Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Image );
                RebuildSpriteImage( m_Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Data,
                                    element.Object.SpriteImage,
                                    m_Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Multicolor,
                                    element.Object.Color );
              }
            }
          }
        }
      }*/
      if ( m_Project.Charsets.Count > 0 )
      {
        SetActiveElementCharset( m_Project.Charsets[0], 
                                 m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor1,
                                 m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor2,
                                 m_Project.CharsetProjects[0].Multicolor );
      }

      int elementOffset = m_Project.Elements.Count;

      foreach ( Project.Screen screen in otherProject.Screens )
      {
        screen.CharsetIndex += origNumCharsets;
        foreach ( Project.ScreenElement element in screen.DisplayedElements )
        {
          if ( ( element.Type != Project.ScreenElementType.LD_OBJECT )
          &&   ( element.Type != Project.ScreenElementType.LD_SPECIAL ) )
          {
            element.Index += elementOffset;
          }
          if ( ( element.Type == Project.ScreenElementType.LD_OBJECT )
          || ( element.Type == Project.ScreenElementType.LD_SPAWN_SPOT ) )
          {
            if ( ( element.Object != null )
            && ( element.Object.TemplateIndex != -1 ) )
            {
              element.Object.SpriteImage = new GR.Image.MemoryImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Tile.Image );
              RebuildSpriteImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Tile,
                                  m_SpriteProject.Colors.Palette, element.Object.SpriteImage,
                                  m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Mode,
                                  element.Object.Color );
            }
          }
        }
        m_Project.Screens.Add( screen );
      }

      foreach ( Project.Element element in otherProject.Elements )
      {
        element.CharsetIndex += origNumCharsets;
        element.Index += elementOffset;
        element.Name = otherProject.ExportPrefix + "_" + element.Name;
        m_Project.Elements.Add( element );
      }
      foreach ( Project.Screen screen in otherProject.Screens )
      {
        comboScreens.Items.Add( new GR.Generic.Tupel<string,Project.Screen>( comboScreens.Items.Count.ToString() + ":" + screen.Name, screen ) );
      }
      foreach ( Project.Element element in otherProject.Elements )
      {
        comboElements.Items.Add( element.Name );
        listAvailableElements.Items.Add( element.Name );
      }
    }



    /*
    private void combineSoullessToolStripMenuItem_Click( object sender, EventArgs e )
    {
      Project prj1 = new Project();
      Project prj2 = new Project();

      prj1.LoadFromFile( @"D:\privat\projekte\c64\Soulless\Soulless-Tomb.elementeditorproject" );
      prj2.LoadFromFile( @"D:\privat\projekte\c64\Soulless\Soulless-FullGame.elementeditorproject" );

      System.Collections.Generic.List<DataInfo> elementDatas = new List<DataInfo>();
      System.Collections.Generic.List<DataInfo> elementColor = new List<DataInfo>();

      List<Project>     projectsToCombine = new List<Project>();

      projectsToCombine.Add( prj1 );
      projectsToCombine.Add( prj2 );

      foreach ( Project prj in projectsToCombine )
      {
        foreach ( Project.Element element in prj.Elements )
        {
          DataInfo dataChar = new DataInfo();
          DataInfo dataColor = new DataInfo();

          dataChar.Data = new GR.Memory.ByteBuffer();
          dataChar.Name = "DATA_EL_" + prj.ExportPrefix + "_" + SanitizeName( element );
          dataColor.Data = new GR.Memory.ByteBuffer();
          dataColor.Name = "COLOR_EL_" + prj.ExportPrefix + "_" + SanitizeName( element );

          for ( int j = 0; j < element.Characters.Height; ++j )
          {
            for ( int i = 0; i < element.Characters.Width; ++i )
            {
              dataChar.Data.AppendU8( element.Characters[i, j].Char );
              dataColor.Data.AppendU8( element.Characters[i, j].Color );
            }
          }
          elementDatas.Add( dataChar );
          elementColor.Add( dataColor );
        }
      }
      CollapseBuffers( elementDatas );
      CollapseBuffers( elementColor );

      int elementOffset = 0;
      foreach ( Project prj in projectsToCombine )
      {
        string elementTable = ExportElementTable( prj, elementDatas, elementOffset );
        Debug.Log( elementTable );
        string colorTable = ExportElementColorTable( prj, elementColor, elementOffset );
        Debug.Log( colorTable );
        elementOffset += prj.Elements.Count;
      }
    }
    */



    private void btnDispElementUsage_Click( object sender, EventArgs e )
    {
      if ( listAvailableElements.SelectedIndex == -1 )
      {
        return;
      }
      int elementIndex = listAvailableElements.SelectedIndex;
      string selItem = listAvailableElements.Items[listAvailableElements.SelectedIndex].ToString();
      int screenIndex = 0;

      foreach ( Project.Screen screen in m_Project.Screens )
      {
        int screenElementIndex = 0;
        foreach ( Project.ScreenElement element in screen.DisplayedElements )
        {
          if ( ScreenElementUsesElement( element ) )
          {
            if ( element.Index == elementIndex )
            {
              Debug.Log( "Found element " + selItem + " in screen " + screenIndex + ":" + screen.Name + " at index " + screenElementIndex );
            }
          }
          ++screenElementIndex;
        }
        ++screenIndex;
      }

    }



    private void comboElementChar_DrawItem( object sender, DrawItemEventArgs e )
    {
      e.DrawBackground();

      int     index = e.Index;
      if ( ( index >= 0 )
      &&   ( index < 256 ) )
      {
        System.Drawing.RectangleF textRect = new RectangleF( e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height );
        System.Drawing.Brush textBrush = new SolidBrush( e.ForeColor );
        e.Graphics.DrawString( index.ToString(), e.Font, textBrush, textRect );

        CharsetProject charSet = m_Project.Charsets[m_CurrentScreen.CharsetIndex];

        GR.Image.FastImage    fastImage = new GR.Image.FastImage( 8, 8, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
        CharacterDisplayer.DisplayChar( charSet, index, fastImage, 0, 0, comboColor.SelectedIndex );
        System.Drawing.Rectangle drawRect = new Rectangle( e.Bounds.Location, e.Bounds.Size );
        drawRect.X += 30;
        drawRect.Width = 16;
        drawRect.Y += ( drawRect.Height - 16 ) / 2;
        drawRect.Height = 16;

        fastImage.DrawToHDC( e.Graphics.GetHdc(), drawRect );
        e.Graphics.ReleaseHdc();
        fastImage.Dispose();
      }
    }



    private void comboChars_DrawItem( object sender, DrawItemEventArgs e )
    {
      e.DrawBackground();

      if ( ( listElementChars.SelectedIndices.Count == 0 )
      ||   ( m_CurrentEditedElement == null ) )
      {
        return;
      }
      int index = e.Index;
      if ( ( index >= 0 )
      &&   ( index < 256 ) )
      {
        System.Drawing.RectangleF textRect = new RectangleF( e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height );
        System.Drawing.Brush textBrush = new SolidBrush( e.ForeColor );
        e.Graphics.DrawString( index.ToString(), e.Font, textBrush, textRect );

        CharsetProject charSet = m_Project.Charsets[m_CurrentEditedElement.CharsetIndex];

        GR.Image.FastImage fastImage = new GR.Image.FastImage( 8, 8, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
        CharacterDisplayer.DisplayChar( charSet, index, fastImage, 0, 0, comboColor.SelectedIndex );
        System.Drawing.Rectangle drawRect = new Rectangle( e.Bounds.Location, e.Bounds.Size );
        drawRect.X += 30;
        drawRect.Width = 16;
        drawRect.Y += ( drawRect.Height - 16 ) / 2;
        drawRect.Height = 16;

        fastImage.DrawToHDC( e.Graphics.GetHdc(), drawRect );
        e.Graphics.ReleaseHdc();
        fastImage.Dispose();
      }
    }



    private void comboColor_DrawItem( object sender, DrawItemEventArgs e )
    {
      e.DrawBackground();

      int index = e.Index;
      if ( ( index >= 0 )
      &&   ( index < 16 ) )
      {
        System.Drawing.RectangleF textRect = new RectangleF( e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height );
        System.Drawing.Brush textBrush = new SolidBrush( e.ForeColor );
        e.Graphics.DrawString( index.ToString(), e.Font, textBrush, textRect );

        GR.Image.FastImage fastImage = new GR.Image.FastImage( 1, 1, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
        fastImage.SetPixel( 0, 0, m_Project.m_ColorValues[index] );
        System.Drawing.Rectangle drawRect = new Rectangle( e.Bounds.Location, e.Bounds.Size );
        drawRect.X += 20;
        drawRect.Width = 16;
        drawRect.Y += ( drawRect.Height - 16 ) / 2;
        drawRect.Height = 16;

        fastImage.DrawToHDC( e.Graphics.GetHdc(), drawRect );
        e.Graphics.ReleaseHdc();
        fastImage.Dispose();
      }
    }



    private void comboElementColor_DrawItem( object sender, DrawItemEventArgs e )
    {
      e.DrawBackground();

      int index = e.Index;
      if ( ( index >= 0 )
      &&   ( index < 16 ) )
      {
        System.Drawing.RectangleF textRect = new RectangleF( e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height );
        System.Drawing.Brush textBrush = new SolidBrush( e.ForeColor );
        e.Graphics.DrawString( index.ToString(), e.Font, textBrush, textRect );

        GR.Image.FastImage fastImage = new GR.Image.FastImage( 1, 1, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
        fastImage.SetPixel( 0, 0, m_Project.m_ColorValues[index] );
        System.Drawing.Rectangle drawRect = new Rectangle( e.Bounds.Location, e.Bounds.Size );
        drawRect.X += 30;
        drawRect.Width = 16;
        drawRect.Y += ( drawRect.Height - 16 ) / 2;
        drawRect.Height = 16;

        fastImage.DrawToHDC( e.Graphics.GetHdc(), drawRect );
        e.Graphics.ReleaseHdc();
        fastImage.Dispose();
      }

    }



    private void btnCopyScreen_Click( object sender, EventArgs e )
    {
      if ( comboScreens.SelectedIndex == -1 )
      {
        return;
      }
      Project.Screen screen = ( (GR.Generic.Tupel<string, Project.Screen>)comboScreens.SelectedItem ).second;

      int numCopies = 1;
      if ( ( Control.ModifierKeys & Keys.Shift ) == Keys.Shift )
      {
        numCopies = 12;
      }

      for ( int c = 0; c < numCopies; ++c )
      {
        Project.Screen newScreen = new Project.Screen( screen );

        m_Project.Screens.Insert( comboScreens.SelectedIndex + 1, newScreen );

        comboRegionScreens.Items.Insert( comboScreens.SelectedIndex + 1, new GR.Generic.Tupel<string, Project.Screen>( "0:" + newScreen.Name, newScreen ) );
        comboScreens.Items.Insert( comboScreens.SelectedIndex + 1, new GR.Generic.Tupel<string, Project.Screen>( "0:" + newScreen.Name, newScreen ) );
        for ( int i = comboScreens.SelectedIndex + 1; i < comboScreens.Items.Count; ++i )
        {
          GR.Generic.Tupel<string, Project.Screen> screenItem = (GR.Generic.Tupel<string, Project.Screen>)comboScreens.Items[i];
          screenItem.first = i.ToString() + ":" + screenItem.second.Name;

          comboScreens.Items[i] = comboScreens.Items[i];
        }

        for ( int i = comboScreens.SelectedIndex + 1; i < comboScreens.Items.Count; ++i )
        {
          GR.Generic.Tupel<string, Project.Screen> screenItem = (GR.Generic.Tupel<string, Project.Screen>)comboRegionScreens.Items[i];
          screenItem.first = i.ToString() + ":" + screenItem.second.Name;

          comboRegionScreens.Items[i] = comboRegionScreens.Items[i];
        }
      }
    }



    private void refreshCharsetFileToolStripMenuItem_Click( object sender, EventArgs e )
    {
      int index = 0;
      foreach ( CharsetProjectInfo info in m_Project.CharsetProjects )
      {
        CharsetProject charSet = OpenCharsetProject( info.Filename );
        m_Project.Charsets[index] = charSet;
        ++index;
      }
      if ( ( comboElementCharset.SelectedIndex >= 0 )
      &&   ( comboElementCharset.SelectedIndex < m_Project.Charsets.Count ) )
      {
        SetActiveElementCharset( m_Project.Charsets[comboElementCharset.SelectedIndex],
                                 m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor1,
                                 m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor2,
                                 m_Project.CharsetProjects[comboElementCharset.SelectedIndex].Multicolor );
      }
    }



    private void refreshSpriteFileToolStripMenuItem_Click( object sender, EventArgs e )
    {
      if ( !string.IsNullOrEmpty( m_Project.SpriteProjectFilename ) )
      {
        string fullPath = m_Project.SpriteProjectFilename;
        OpenSpriteProject( fullPath );

        foreach ( Project.Screen screen in m_Project.Screens )
        {
          foreach ( Project.ScreenElement element in screen.DisplayedElements )
          {
            if ( ( element.Type == Project.ScreenElementType.LD_OBJECT )
            ||   ( element.Type == Project.ScreenElementType.LD_SPAWN_SPOT ) )
            {
              if ( ( element.Object != null )
              &&   ( element.Object.TemplateIndex != -1 ) )
              {
                element.Object.SpriteImage = new GR.Image.MemoryImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Tile.Image );
                RebuildSpriteImage( m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Tile,
                                    m_SpriteProject.Colors.Palette, 
                                    element.Object.SpriteImage,
                                    m_SpriteProject.Sprites[m_Project.ObjectTemplates[element.Object.TemplateIndex].StartSprite].Mode,
                                    element.Object.Color );
              }
            }
          }
        }
      }
    }



    private void checkProjectToolStripMenuItem_Click( object sender, EventArgs e )
    {
      string  resultText = "";
      int     reducedElements = 0;
      Dictionary<string,GR.Collections.Set<string>>   objectCombinations = new Dictionary<string, GR.Collections.Set<string>>();

      // check if there are similar elements
      GR.Collections.Map<string, int> elementUsage = new GR.Collections.Map<string, int>();

      int elementIndex = 0;
      foreach ( Project.Element element in m_Project.Elements )
      {
        elementUsage[m_Project.Elements[elementIndex].Name] = 0;
        foreach ( Project.Element otherElement in m_Project.Elements )
        {
          if ( element != otherElement )
          {
            if ( ( element.Characters.Width == otherElement.Characters.Width )
            && ( element.Characters.Height == otherElement.Characters.Height ) )
            {
              for ( int i = 0; i < element.Characters.Width; ++i )
              {
                for ( int j = 0; j < element.Characters.Height; ++j )
                {
                  if ( ( element.Characters[i, j].Char != otherElement.Characters[i, j].Char )
                  || ( element.Characters[i, j].Color != otherElement.Characters[i, j].Color ) )
                  {
                    goto checkfailed;
                  }
                }
              }
              // elements are the same!!
              resultText += "Element " + element.Name + " = " + otherElement.Name + System.Environment.NewLine;
              checkfailed:;
            }
          }
        }
        ++elementIndex;
      }

      // check if any elements are completely hidden
      int screenIndex = 0;
      foreach ( Project.Screen screen in m_Project.Screens )
      {
        GR.Collections.Set<string>    usedObjects = new GR.Collections.Set<string>();
        for ( int origIndex = 0; origIndex < screen.DisplayedElements.Count; ++origIndex )
        {
          Project.ScreenElement screenElement = screen.DisplayedElements[origIndex];

          if ( screenElement.Type == Project.ScreenElementType.LD_OBJECT )
          {
            usedObjects.Add( m_Project.ObjectTemplates[screenElement.Object.TemplateIndex].Name );
          }

          if ( !ScreenElementUsesElement( screenElement ) )
          {
            continue;
          }
          GR.Collections.Set<System.Drawing.Point> origPoints = AffectedCharacters( screenElement );
          for ( int laterIndex = origIndex + 1; laterIndex < screen.DisplayedElements.Count; ++laterIndex )
          {
            GR.Collections.Set<System.Drawing.Point> newPoints = AffectedCharacters( screen.DisplayedElements[laterIndex] );

            foreach ( System.Drawing.Point point in newPoints )
            {
              if ( origPoints.ContainsValue( point ) )
              {
                origPoints.Remove( point );
              }
            }
          }
          if ( origPoints.Count == 0 )
          {
            resultText += "Screen " + screenIndex + "/" + screen.Name + ", Element " + origIndex + " is completely obstructed" + System.Environment.NewLine;
          }
        }
        foreach ( var objTemplate in usedObjects )
        {
          foreach ( var otherObj in usedObjects )
          {
            if ( objTemplate != otherObj )
            {
              if ( !objectCombinations.ContainsKey( objTemplate ) )
              {
                objectCombinations.Add( objTemplate, new GR.Collections.Set<string>() );
              }
              objectCombinations[objTemplate].Add( otherObj );
            }
          }
        }
        ++screenIndex;
      }

      GR.Collections.Map<int,string>   usedSearchObjectIndices = new GR.Collections.Map<int, string>();

      screenIndex = 0;
      foreach ( Project.Screen screen in m_Project.Screens )
      {
        int     screenElementIndex = 0;
        foreach ( Project.ScreenElement element in screen.DisplayedElements )
        {
          if ( element.Type == Project.ScreenElementType.LD_SEARCH_OBJECT )
          {
            if ( usedSearchObjectIndices.ContainsKey( element.SearchObjectIndex ) )
            {
              resultText += "Search Index " + element.SearchObjectIndex + " duplicate in screen " + screenIndex + "/" + screen.Name + ", original entry in screen " + usedSearchObjectIndices[element.SearchObjectIndex] + System.Environment.NewLine;
            }
            else
            {
              usedSearchObjectIndices.Add( element.SearchObjectIndex, screen.Name );
            }
          }

          if ( ( element.Type == Project.ScreenElementType.LD_LINE_H )
          ||   ( element.Type == Project.ScreenElementType.LD_LINE_V )
          ||   ( element.Type == Project.ScreenElementType.LD_AREA )
          ||   ( element.Type == Project.ScreenElementType.LD_ELEMENT_LINE_H )
          ||   ( element.Type == Project.ScreenElementType.LD_ELEMENT_LINE_V )
          ||   ( element.Type == Project.ScreenElementType.LD_ELEMENT_AREA ) )
          {
            if ( ( element.Repeats == 0 )
            ||   ( element.Repeats >= 128 ) )
            {
              resultText += "Screen " + screen.Name + " (" + screenIndex.ToString() + "), Element " + screenElementIndex + " has invalid repeat count" + System.Environment.NewLine;
            }
          }
          if ( ( element.Type == Project.ScreenElementType.LD_AREA )
          ||   ( element.Type == Project.ScreenElementType.LD_ELEMENT_AREA ) )
          {
            if ( ( element.Repeats2 == 0 )
            ||   ( element.Repeats2 >= 128 ) )
            {
              resultText += "Screen " + screen.Name + " (" + screenIndex.ToString() + "), Element " + screenElementIndex + " has invalid secondary repeat count" + System.Environment.NewLine;
            }
          }

          if ( ScreenElementUsesElement( element ) )
          {
            elementUsage[m_Project.Elements[element.Index].Name]++;
          }

          while ( element.X >= 40 )
          {
            element.X -= 40;
            element.Y++;
          }
          if ( ( element.X < 0 )
          ||   ( element.Y < 0 )
          ||   ( element.Y >= screen.Height )
          ||   ( element.X >= screen.Width ) )
          {
            resultText += "Screen " + screen.Name + " (" + screenIndex.ToString() + "), Element " + screenElementIndex + " outside" + System.Environment.NewLine;
          }
          ++screenElementIndex;
        }
        ++screenIndex;
      }

      int     lastIndex = -1;
      foreach ( var searchObject in usedSearchObjectIndices )
      {
        if ( searchObject.Key != lastIndex + 1 )
        {
          resultText += "Search index not increasing from " + lastIndex + ", next index found is " + searchObject.Key + System.Environment.NewLine;
        }
        lastIndex = searchObject.Key;
      }

      if ( reducedElements > 0 )
      {
        comboScreens_SelectedIndexChanged( null, null );
      }
      foreach ( KeyValuePair<string, int> kv in elementUsage )
      {
        if ( kv.Value == 0 )
        {
          resultText += "Unused element " + kv.Key + System.Environment.NewLine;
        }
      }

      GR.Collections.Set<int>      usedChars = new GR.Collections.Set<int>();
      foreach ( Project.Element template in m_Project.Elements )
      {
        for ( int i = 0; i < template.Characters.Width; ++i )
        {
          for ( int j = 0; j < template.Characters.Height; ++j )
          {
            usedChars.Add( template.Characters[i,j].Char + template.CharsetIndex * 256 );
          }
        }
      }
      for ( int i = 0; i < m_Project.CharsetProjects.Count * 256; ++i )
      {
        if ( !usedChars.ContainsValue( i ) )
        {
          resultText += "Unused char " + ( i % 256 ) + " in charset " + ( i / 256 + 1 ) + System.Environment.NewLine;
        }
      }

      foreach ( var objTemplate in objectCombinations.Keys )
      {
        resultText += "Object " + objTemplate + " used with ";
        foreach ( var otherObj in objectCombinations[objTemplate] )
        {
          resultText += otherObj + ", ";
        }
        resultText += System.Environment.NewLine;
      }

      if ( m_CheckResult == null )
      {
        m_CheckResult = new FormCheckResult();
      }
      m_CheckResult.ShowText( resultText );
      m_CheckResult.Show();
    }



    private void exportScreenToDataToolStripMenuItem_Click( object sender, EventArgs e )
    {
      if ( m_CurrentScreen == null )
      {
        return;
      }
      GR.Memory.ByteBuffer      bufChar = new GR.Memory.ByteBuffer( (uint)( m_CurrentScreen.Width * m_CurrentScreen.Height ) );
      GR.Memory.ByteBuffer      bufColor = new GR.Memory.ByteBuffer( (uint)( m_CurrentScreen.Width * m_CurrentScreen.Height ) );

      bool  rowFirst = false;

      if ( !rowFirst )
      {
        for ( int i = 0; i < m_CurrentScreen.Width; ++i )
        {
          for ( int j = 0; j < m_CurrentScreen.Height; ++j )
          {
            bufChar.SetU8At( j + i * m_CurrentScreen.Height, m_ScreenContent[i, j].Char );
            bufColor.SetU8At( j + i * m_CurrentScreen.Height, m_ScreenContent[i, j].Color );
          }
        }
      }
      else
      {
        for ( int j = 0; j < m_CurrentScreen.Height; ++j )
        {
          for ( int i = 0; i < m_CurrentScreen.Width; ++i )
          {
            bufChar.SetU8At( i + j * m_CurrentScreen.Width, m_ScreenContent[i, j].Char );
            bufColor.SetU8At( i + j * m_CurrentScreen.Width, m_ScreenContent[i, j].Color );
          }
        }
      }

      int     wrapLength = 40;
      if ( !rowFirst )
      {
        wrapLength = 25;
      }

      StringBuilder sb = new StringBuilder();

      sb.Append( "SCREEN_DATA\n" );

      for ( int i = 0; i < bufChar.Length / wrapLength; ++i )
      {
        sb.Append( "          !byte " );
        for ( int j = 0; j < wrapLength; ++j )
        {
          sb.Append( "$" + bufChar.ByteAt( j + i * wrapLength ).ToString( "x2" ) );
          if ( j < wrapLength - 1 )
          {
            sb.Append( "," );
          }
          else
          {
            sb.Append( "\n" );
          }
        }
      }

      sb.Append( "\n\nSCREEN_COLOR_DATA\n" );
      for ( int i = 0; i < bufColor.Length / wrapLength; ++i )
      {
        sb.Append( "          !byte " );
        for ( int j = 0; j < wrapLength; ++j )
        {
          sb.Append( "$" + bufColor.ByteAt( j + i * wrapLength ).ToString( "x2" ) );
          if ( j < wrapLength - 1 )
          {
            sb.Append( "," );
          }
          else
          {
            sb.Append( "\n" );
          }
        }
      }
      Debug.Log( sb.ToString() );
    }



    private void comboEmptyChar_DrawItem( object sender, DrawItemEventArgs e )
    {
      e.DrawBackground();

      if ( m_Project.Charsets.Count == 0 )
      {
        return;
      }

      int index = e.Index;
      if ( ( index >= 0 )
      &&   ( index < 256 ) )
      {
        System.Drawing.RectangleF textRect = new RectangleF( e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height );
        System.Drawing.Brush textBrush = new SolidBrush( e.ForeColor );
        e.Graphics.DrawString( index.ToString(), e.Font, textBrush, textRect );

        CharsetProject charSet = m_Project.Charsets[0];

        GR.Image.FastImage fastImage = new GR.Image.FastImage( 8, 8, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
        for ( int j = 0; j < 16; ++j )
        {
          fastImage.SetPaletteColor( j, (byte)( ( m_Project.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( m_Project.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( m_Project.m_ColorValues[j] & 0xff ) );
        }

        CharacterDisplayer.DisplayChar( charSet, index, fastImage, 0, 0, comboEmptyColor.SelectedIndex );

        System.Drawing.Rectangle drawRect = new Rectangle( e.Bounds.Location, e.Bounds.Size );
        drawRect.X += 30;
        drawRect.Width = 16;
        drawRect.Y += ( drawRect.Height - 16 ) / 2;
        drawRect.Height = 16;

        fastImage.DrawToHDC( e.Graphics.GetHdc(), drawRect );
        e.Graphics.ReleaseHdc();
        fastImage.Dispose();
      }

    }



    private void comboEmptyColor_SelectedIndexChanged( object sender, EventArgs e )
    {
      comboEmptyChar.Invalidate();
      if ( m_Project.EmptyColor != (byte)comboEmptyColor.SelectedIndex )
      {
        m_Project.EmptyColor = (byte)comboEmptyColor.SelectedIndex;
        Modified = true;
      }
    }



    private void comboEmptyChar_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( m_Project.EmptyChar != (byte)comboEmptyChar.SelectedIndex )
      {
        m_Project.EmptyChar = (byte)comboEmptyChar.SelectedIndex;
        Modified = true;
      }
    }



    private void btnCopyElementTemplate_Click( object sender, EventArgs e )
    {
      if ( listAvailableElements.SelectedIndex == -1 )
      {
        return;
      }
      int elementIndex = listAvailableElements.SelectedIndex;

      Project.Element elementSource = m_Project.Elements[elementIndex];

      Project.Element element = new Project.Element();

      element.Characters.Resize( elementSource.Characters.Width, elementSource.Characters.Height );
      for ( int i = 0; i < elementSource.Characters.Width; ++i )
      {
        for ( int j = 0; j < elementSource.Characters.Height; ++j )
        {
          element.Characters[i, j].Color = elementSource.Characters[i, j].Color;
          element.Characters[i, j].Char = elementSource.Characters[i, j].Char;
        }
      }
      element.Name = elementSource.Name + " Copy";
      element.CharsetIndex = elementSource.CharsetIndex;

      m_Project.Elements.Add( element );
      comboElements.Items.Add( element.Name );
      element.Index = listAvailableElements.Items.Add( element.Name );
      listAvailableElements.SelectedIndex = element.Index;
      Modified = true;
    }



    private void btnNewRegion_Click( object sender, EventArgs e )
    {
      if ( ( Control.ModifierKeys & Keys.Shift ) == Keys.Shift )
      {
        // Wonderland hack, add 12 regions with one screen
        int startIndex = m_Project.Regions.Count;
        for ( int i = 0; i < 12; ++i )
        {
          Project.Region region = new Project.Region();

          region.DisplayX = i;
          region.DisplayY = startIndex / 12;
          region.Vertical = false;

          Project.RegionScreenInfo    screenInfo = new Project.RegionScreenInfo();
          screenInfo.ScreenIndex = 52 + startIndex + i;
          region.Screens.Add( screenInfo );
          m_Project.Regions.Add( region );

          ListViewItem itemRegion = ItemFromRegion( m_Project.Regions.Count - 1, region );
          listRegions.Items.Add( itemRegion );
        }
        RedrawMap();
      }
      else
      {
        Project.Region region = new Project.Region();
        m_Project.Regions.Add( region );

        ListViewItem itemRegion = ItemFromRegion( m_Project.Regions.Count - 1, region );
        listRegions.Items.Add( itemRegion );
        RedrawMap();
      }
      Modified = true;
    }



    private void listRegions_SelectedIndexChanged( object sender, EventArgs e )
    {
      listRegionScreens.Items.Clear();
      if ( listRegions.SelectedIndices.Count == 0 )
      {
        btnDelRegion.Enabled = false;
        listRegionScreens.Enabled = false;
        listRegionScreens.Items.Clear();
        checkRegionOrientation.Enabled = false;
        comboRegionScreens.Enabled = false;
        btnAddRegionScreen.Enabled = false;
        editRegionX.Enabled = false;
        editRegionY.Enabled = false;
        editRegionExtra.Text = "";
        return;
      }
      btnDelRegion.Enabled = true;
      listRegionScreens.Enabled = true;
      checkRegionOrientation.Enabled = true;
      comboRegionScreens.Enabled = true;
      editRegionX.Enabled = true;
      editRegionY.Enabled = true;

      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      checkRegionOrientation.Checked = region.Vertical;
      editRegionX.Text = region.DisplayX.ToString();
      editRegionY.Text = region.DisplayY.ToString();
      editRegionExtra.Text = region.ExtraData.ToString();

      foreach ( Project.RegionScreenInfo screenInfo in region.Screens )
      {
        listRegionScreens.Items.Add( screenInfo.ScreenIndex.ToString() + ":" + m_Project.Screens[screenInfo.ScreenIndex].Name );
      }
    }



    private void btnDelRegion_Click( object sender, EventArgs e )
    {
      if ( listRegions.SelectedIndices.Count == 0 )
      {
        return;
      }
      foreach ( int item in listRegions.SelectedIndices )
      {
        m_Project.Regions.RemoveAt( item );
      }
      foreach ( ListViewItem item in listRegions.SelectedItems )
      {
        listRegions.Items.Remove( item );
      }
      RedrawMap();
      Modified = true;
    }



    private ListViewItem ItemFromRegion( int RegionIndex, Project.Region Region )
    {
      ListViewItem itemRegion = new ListViewItem( RegionIndex.ToString() );

      string regionDesc = Region.Vertical ? "V " : "H ";

      foreach ( Project.RegionScreenInfo screenInfo in Region.Screens )
      {
        regionDesc += screenInfo.ScreenIndex.ToString() + " ";
      }
      itemRegion.SubItems.Add( regionDesc );
      itemRegion.SubItems.Add( Region.DisplayX.ToString() + "," + Region.DisplayY.ToString() );
      itemRegion.Tag = Region;

      return itemRegion;
    }



    private void comboRegionScreens_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( comboRegionScreens.SelectedIndex == -1 )
      {
        btnAddRegionScreen.Enabled = false;
        return;
      }
      btnAddRegionScreen.Enabled = true;
    }



    private void btnAddRegionScreen_Click( object sender, EventArgs e )
    {
      if ( ( comboRegionScreens.SelectedIndex == -1 )
      ||   ( listRegions.SelectedItems.Count == 0 ) )
      {
        return;
      }
      int screenIndex = comboRegionScreens.SelectedIndex;
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      Project.RegionScreenInfo screenInfo = new Project.RegionScreenInfo();

      screenInfo.ScreenIndex = screenIndex;

      region.Screens.Add( screenInfo );

      ListViewItem dummy = ItemFromRegion( listRegions.Items.Count, region );

      listRegions.SelectedItems[0].SubItems[1].Text = dummy.SubItems[1].Text;
      listRegionScreens.Items.Add( screenIndex.ToString() + ":" + m_Project.Screens[screenIndex].Name );
      RedrawMap();
      Modified = true;
    }



    private void btnDelRegionScreen_Click( object sender, EventArgs e )
    {
      if ( listRegionScreens.SelectedIndex == -1 )
      {
        return;
      }
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      region.Screens.RemoveAt( listRegionScreens.SelectedIndex );
      listRegionScreens.Items.RemoveAt( listRegionScreens.SelectedIndex );

      ListViewItem dummy = ItemFromRegion( listRegionScreens.SelectedIndex, region );
      listRegions.SelectedItems[0].SubItems[1].Text = dummy.SubItems[1].Text;

      RedrawMap();
      Modified = true;
    }



    private void listRegionScreens_SelectedIndexChanged( object sender, EventArgs e )
    {
      btnDelRegionScreen.Enabled = ( listRegionScreens.SelectedIndex != -1 );
      checkExitN.Enabled = ( listRegionScreens.SelectedIndex != -1 );
      checkExitS.Enabled = ( listRegionScreens.SelectedIndex != -1 );
      checkExitW.Enabled = ( listRegionScreens.SelectedIndex != -1 );
      checkExitE.Enabled = ( listRegionScreens.SelectedIndex != -1 );

      if ( listRegions.SelectedIndices.Count == 0 )
      {
        return;
      }
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;
      if ( listRegionScreens.SelectedIndex != -1 )
      {
        Project.RegionScreenInfo screenInfo = region.Screens[listRegionScreens.SelectedIndex];
        checkExitN.Checked = screenInfo.ExitN;
        checkExitS.Checked = screenInfo.ExitS;
        checkExitW.Checked = screenInfo.ExitW;
        checkExitE.Checked = screenInfo.ExitE;
      }
    }



    private void checkRegionOrientation_CheckedChanged( object sender, EventArgs e )
    {
      if ( listRegions.SelectedIndices.Count == 0 )
      {
        return;
      }
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      if ( region.Vertical != checkRegionOrientation.Checked )
      {
        region.Vertical = checkRegionOrientation.Checked;

        ListViewItem dummy = ItemFromRegion( listRegions.SelectedIndices[0], region );

        listRegions.SelectedItems[0].SubItems[1].Text = dummy.SubItems[1].Text;
        RedrawMap();
        Modified = true;
      }
    }



    private void checkExitN_CheckedChanged( object sender, EventArgs e )
    {
      if ( listRegions.SelectedIndices.Count == 0 )
      {
        return;
      }
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      if ( listRegionScreens.SelectedIndex != -1 )
      {
        Project.RegionScreenInfo screenInfo = region.Screens[listRegionScreens.SelectedIndex];
        if ( screenInfo.ExitN != checkExitN.Checked )
        {
          screenInfo.ExitN = checkExitN.Checked;
          RedrawMap();
          Modified = true;
        }
      }
    }



    private void checkExitS_CheckedChanged( object sender, EventArgs e )
    {
      if ( listRegions.SelectedIndices.Count == 0 )
      {
        return;
      }
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      if ( listRegionScreens.SelectedIndex != -1 )
      {
        Project.RegionScreenInfo screenInfo = region.Screens[listRegionScreens.SelectedIndex];
        if ( screenInfo.ExitS != checkExitS.Checked )
        {
          screenInfo.ExitS = checkExitS.Checked;
          RedrawMap();
          Modified = true;
        }
      }
    }



    private void checkExitW_CheckedChanged( object sender, EventArgs e )
    {
      if ( listRegions.SelectedIndices.Count == 0 )
      {
        return;
      }
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      if ( listRegionScreens.SelectedIndex != -1 )
      {
        Project.RegionScreenInfo screenInfo = region.Screens[listRegionScreens.SelectedIndex];
        if ( screenInfo.ExitW != checkExitW.Checked )
        {
          screenInfo.ExitW = checkExitW.Checked;
          RedrawMap();
          Modified = true;
        }
      }
    }



    private void checkExitE_CheckedChanged( object sender, EventArgs e )
    {
      if ( listRegions.SelectedIndices.Count == 0 )
      {
        return;
      }
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      if ( listRegionScreens.SelectedIndex != -1 )
      {
        Project.RegionScreenInfo screenInfo = region.Screens[listRegionScreens.SelectedIndex];
        if ( screenInfo.ExitE != checkExitE.Checked )
        {
          screenInfo.ExitE = checkExitE.Checked;
          RedrawMap();
          Modified = true;
        }
      }
    }



    private void RedrawMap()
    {
      pictureMap.DisplayPage.Box( 0, 0, pictureMap.DisplayPage.Width, pictureMap.DisplayPage.Height, 0 );

      if ( m_Project.Regions.Count == 0 )
      {
        pictureMap.Invalidate();
        return;
      }

      // determine extents of map
      int minX = 1000000;
      int maxX = -1000000;
      int minY = 1000000;
      int maxY = -1000000;

      foreach ( Project.Region region in m_Project.Regions )
      {
        minX = Math.Min( region.DisplayX, minX );
        minY = Math.Min( region.DisplayY, minY );

        maxX = Math.Max( region.Vertical ? region.DisplayX : ( region.DisplayX + region.Screens.Count - 1 ), maxX );
        maxY = Math.Max( region.Vertical ? ( region.DisplayY + region.Screens.Count - 1 ) : region.DisplayY, maxY );

        if ( region.Screens.Count == 0 )
        {
          maxX = Math.Max( region.Vertical ? region.DisplayX : region.DisplayX, maxX );
          maxY = Math.Max( region.Vertical ? region.DisplayY : region.DisplayY, maxY );
        }
      }

      int screenW = pictureMap.ClientSize.Width / ( maxX - minX + 1 );
      int screenH = pictureMap.ClientSize.Height / ( maxY - minY + 1 );
      int offsetX = 0;
      int offsetY = 0;

      if ( screenW > 40 )
      {
        offsetX = ( pictureMap.ClientSize.Width - ( maxX - minX + 1 ) * screenW ) / 2;
        screenW = 40;
      }
      if ( screenH > 30 )
      {
        offsetY = ( pictureMap.ClientSize.Height - ( maxY - minY + 1 ) * screenH ) / 2;
        screenH = 30;
      }

      int regionIndex = 1;
      const int doorSize = 4;
      foreach ( Project.Region region in m_Project.Regions )
      {
        int     x = region.DisplayX;
        int     y = region.DisplayY;
        int     screenIndex = 0;
        foreach ( Project.RegionScreenInfo screenInfo in region.Screens )
        {
          int insetN = 2;
          int insetW = 2;
          int insetE = 2;
          int insetS = 2;

          if ( screenIndex > 0 )
          {
            if ( region.Vertical )
            {
              insetN = 0;
            }
            else
            {
              insetW = 0;
            }
          }
          if ( screenIndex + 1 < region.Screens.Count )
          {
            if ( region.Vertical )
            {
              insetS = 0;
            }
            else
            {
              insetE = 0;
            }
          }

          pictureMap.DisplayPage.Box( offsetX + x * screenW + insetW, offsetY + y * screenH + insetN, screenW - ( insetW + insetE ), screenH - ( insetN + insetS ), (uint)regionIndex );

          if ( screenInfo.ExitN )
          {
            pictureMap.DisplayPage.Box( offsetX + x * screenW + screenW / 2 - doorSize / 2, offsetY + y * screenH, doorSize, 2, (uint)regionIndex );
          }
          if ( screenInfo.ExitS )
          {
            pictureMap.DisplayPage.Box( offsetX + x * screenW + screenW / 2 - doorSize / 2, offsetY + y * screenH + screenH - 2, doorSize, 2, (uint)regionIndex );
          }
          if ( screenInfo.ExitW )
          {
            pictureMap.DisplayPage.Box( offsetX + x * screenW, offsetY + y * screenH + screenH / 2 - doorSize / 2, 2, doorSize, (uint)regionIndex );
          }
          if ( screenInfo.ExitE )
          {
            pictureMap.DisplayPage.Box( offsetX + x * screenW + screenW - 2, offsetY + y * screenH + screenH / 2 - doorSize / 2, 2, doorSize, (uint)regionIndex );
          }
          string screenNo = screenInfo.ScreenIndex.ToString();
          for ( int i = 0; i < screenNo.Length; ++i )
          {
            pictureMap.DisplayPage.DrawFromImage( m_MapNumbers[( screenNo[i] - '0' )], offsetX + x * screenW + 3 + i * 4, offsetY + y * screenH + 3 );
          }
          if ( region.Vertical )
          {
            ++y;
          }
          else
          {
            ++x;
          }
          ++screenIndex;
        }
        regionIndex = ( regionIndex % 15 ) + 1;
      }
      pictureMap.Invalidate();
    }



    private void editRegionX_TextChanged( object sender, EventArgs e )
    {
      if ( listRegions.SelectedIndices.Count == 0 )
      {
        return;
      }
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      int     x = 0;
      if ( int.TryParse( editRegionX.Text, out x ) )
      {
        if ( region.DisplayX != x )
        {
          region.DisplayX = x;

          ListViewItem dummy = ItemFromRegion( listRegions.SelectedIndices[0], region );
          listRegions.SelectedItems[0].SubItems[2].Text = dummy.SubItems[2].Text;
          RedrawMap();
          Modified = true;
        }
      }
    }



    private void editRegionY_TextChanged( object sender, EventArgs e )
    {
      if ( listRegions.SelectedIndices.Count == 0 )
      {
        return;
      }
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      int y = 0;
      if ( int.TryParse( editRegionY.Text, out y ) )
      {
        if ( region.DisplayY != y )
        {
          region.DisplayY = y;

          ListViewItem dummy = ItemFromRegion( listRegions.SelectedIndices[0], region );
          listRegions.SelectedItems[0].SubItems[2].Text = dummy.SubItems[2].Text;
          RedrawMap();
          Modified = true;
        }
      }

    }



    private void comboScreenObjectFlags_SelectedIndexChanged( object sender, EventArgs e )
    {
      Project.ScreenElement screenElement = null;
      if ( listScreenElements.SelectedIndices.Count != 0 )
      {
        int elementIndex = listScreenElements.SelectedIndices[0];
        screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

        if ( screenElement.Flags != comboScreenObjectFlags.SelectedIndex )
        {
          screenElement.Flags = comboScreenObjectFlags.SelectedIndex;
          Modified = true;
        }
      }
    }



    private void checkMCMode_CheckedChanged( object sender, EventArgs e )
    {
      if ( m_CurrentEditedElement != null )
      {
        if ( m_Project.CharsetProjects[comboElementCharset.SelectedIndex].Multicolor != checkMCMode.Checked )
        {
          m_Project.CharsetProjects[comboElementCharset.SelectedIndex].Multicolor = checkMCMode.Checked;

          if ( ( comboElementCharset.SelectedIndex >= 0 )
          &&   ( comboElementCharset.SelectedIndex < m_Project.Charsets.Count ) )
          {
            SetActiveElementCharset( m_Project.Charsets[comboElementCharset.SelectedIndex],
                                     m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor1,
                                     m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor2,
                                     m_Project.CharsetProjects[comboElementCharset.SelectedIndex].Multicolor );
          }
          Modified = true;
        }
      }
    }



    private void btnSetBackground_Click( object sender, EventArgs e )
    {
      OpenFileDialog openFile = new OpenFileDialog();

      openFile.Title = "Open image";

      if ( openFile.ShowDialog() == DialogResult.OK )
      {
        System.Drawing.Bitmap bmpImage = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile( openFile.FileName );
        GR.Image.FastImage imgClip = GR.Image.FastImage.FromImage( bmpImage );
        bmpImage.Dispose();
        if ( ( imgClip.Width != 320 )
        ||   ( imgClip.Height != 200 )
        ||   ( imgClip.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppRgb ) )
        {
          imgClip.Dispose();
          System.Windows.Forms.MessageBox.Show( "Image format invalid!\nNeeds to be 32bit and have width/height of 320x200" );
          return;
        }
        imgClip.DrawTo( m_BackgroundImage, 0, 0 );
        imgClip.Dispose();
        RedrawScreen();
      }

    }



    private void btnClearBackground_Click( object sender, EventArgs e )
    {
      m_BackgroundImage.Box( 0, 0, 320, 200, 0 );
      RedrawScreen();
    }



    private void editObjectData_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( ( screenElement.Object != null )
      &&   ( GR.Convert.ToI32( editObjectData.Text ) != screenElement.Object.Data ) )
      {
        screenElement.Object.Data = GR.Convert.ToI32( editObjectData.Text );
        Modified = true;
        RedrawScreen();
      }
    }



    private void editWLScreenConfig_TextChanged( object sender, EventArgs e )
    {
      if ( m_CurrentScreen != null )
      {
        byte wlConfigByte = GR.Convert.ToU8( editWonderlandBaseScreenConfig.Text, 16 );
        if ( wlConfigByte != m_CurrentScreen.WLConfigByte )
        {
          m_CurrentScreen.WLConfigByte = wlConfigByte;
          Modified = true;
          RedrawScreen();
        }
      }
    }



    private void manageCharsetsToolStripMenuItem_Click( object sender, EventArgs e )
    {
      FormManageCharsets manageCharsets = new FormManageCharsets( this );

      manageCharsets.ShowDialog();
    }



    private void editRegionExtra_TextChanged( object sender, EventArgs e )
    {
      if ( listRegions.SelectedItems.Count == 0 )
      {
        return;
      }
      Project.Region region = (Project.Region)listRegions.SelectedItems[0].Tag;

      if ( editRegionExtra.Text != region.ExtraData.ToString() )
      {
        region.ExtraData.FromHexString( editRegionExtra.Text );
        Modified = true;
      }
    }



    private void editScreenData_TextChanged( object sender, EventArgs e )
    {
      if ( m_CurrentScreen != null )
      {
        string  text = editScreenData.Text;

        if ( text != m_CurrentScreen.ExtraData )
        {
          m_CurrentScreen.ExtraData = text;
          Modified = true;
        }
      }
    }



    private void screenMCColor2_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( m_CurrentScreen != null )
      {
        if ( m_CurrentScreen.OverrideMC2 != screenMCColor2.SelectedIndex - 1 )
        {
          m_CurrentScreen.OverrideMC2 = screenMCColor2.SelectedIndex - 1;
          if ( ( comboElementCharset.SelectedIndex >= 0 )
          &&   ( comboElementCharset.SelectedIndex < m_Project.Charsets.Count ) )
          {
            SetActiveElementCharset( m_Project.Charsets[comboElementCharset.SelectedIndex],
                                   m_CurrentScreen.OverrideMC1 != -1 ? m_CurrentScreen.OverrideMC1 : m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor1,
                                   m_CurrentScreen.OverrideMC2 != -1 ? m_CurrentScreen.OverrideMC2 : m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor2,
                                   m_Project.CharsetProjects[comboElementCharset.SelectedIndex].Multicolor );
          }
          Modified = true;
          RedrawScreen();
        }
      }
    }

    
    
    private void screenMCColor1_SelectedIndexChanged( object sender, EventArgs e )
    {
      if ( m_CurrentScreen != null )
      {
        if ( m_CurrentScreen.OverrideMC1 != screenMCColor1.SelectedIndex - 1 )
        {
          m_CurrentScreen.OverrideMC1 = screenMCColor1.SelectedIndex - 1;

          if ( ( comboElementCharset.SelectedIndex >= 0 )
          &&   ( comboElementCharset.SelectedIndex < m_Project.Charsets.Count ) )
          {
            SetActiveElementCharset( m_Project.Charsets[comboElementCharset.SelectedIndex], 
                                   m_CurrentScreen.OverrideMC1 != -1 ? m_CurrentScreen.OverrideMC1 : m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor1,
                                   m_CurrentScreen.OverrideMC2 != -1 ? m_CurrentScreen.OverrideMC2 : m_Project.Charsets[comboElementCharset.SelectedIndex].Colors.MultiColor2,
                                   m_Project.CharsetProjects[comboElementCharset.SelectedIndex].Multicolor );
          }
          Modified = true;
          RedrawScreen();
        }
      }
    }



    private void comboScreenMCColor1_DrawItem( object sender, DrawItemEventArgs e )
    {
      e.DrawBackground();

      int index     = e.Index - 1;
      int drawIndex = index;
      if ( index == -1 )
      {
        if ( m_CurrentScreen != null )
        {
          if ( ( m_CurrentScreen.CharsetIndex >= 0 )
          &&   ( m_CurrentScreen.CharsetIndex < m_Project.Charsets.Count ) )
          {
            drawIndex = m_Project.Charsets[m_CurrentScreen.CharsetIndex].Colors.MultiColor1;
          }
        }
      }
      if ( ( drawIndex >= 0 )
      &&   ( drawIndex < 16 ) )
      {
        if ( index != -1 )
        {
          System.Drawing.RectangleF textRect = new RectangleF( e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height );
          System.Drawing.Brush textBrush = new SolidBrush( e.ForeColor );
          e.Graphics.DrawString( index.ToString(), e.Font, textBrush, textRect );
        }

        GR.Image.FastImage fastImage = new GR.Image.FastImage( 1, 1, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
        for ( int j = 0; j < 16; ++j )
        {
          fastImage.SetPaletteColor( j, (byte)( ( m_Project.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( m_Project.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( m_Project.m_ColorValues[j] & 0xff ) );
        }
        fastImage.SetPixel( 0, 0, (uint)drawIndex );
        System.Drawing.Rectangle drawRect = new Rectangle( e.Bounds.Location, e.Bounds.Size );
        drawRect.X += 20;
        drawRect.Width = 16;
        drawRect.Y += ( drawRect.Height - 16 ) / 2;
        drawRect.Height = 16;

        fastImage.DrawToHDC( e.Graphics.GetHdc(), drawRect );
        e.Graphics.ReleaseHdc();
        fastImage.Dispose();
      }
      if ( index == -1 )
      {
        System.Drawing.RectangleF textRect = new RectangleF( e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height );
        System.Drawing.Brush textBrush = new SolidBrush( e.ForeColor );
        e.Graphics.DrawString( "Def", e.Font, textBrush, textRect );
      }
    }



    private void comboScreenMCColor2_DrawItem( object sender, DrawItemEventArgs e )
    {
      e.DrawBackground();

      int index     = e.Index - 1;
      int drawIndex = index;
      if ( index == -1 )
      {
        if ( m_CurrentScreen != null )
        {
          drawIndex = m_Project.Charsets[m_CurrentScreen.CharsetIndex].Colors.MultiColor2;
        }
      }
      if ( ( drawIndex >= 0 )
      &&   ( drawIndex < 16 ) )
      {
        if ( index != -1 )
        {
          System.Drawing.RectangleF textRect = new RectangleF( e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height );
          System.Drawing.Brush textBrush = new SolidBrush( e.ForeColor );
          e.Graphics.DrawString( index.ToString(), e.Font, textBrush, textRect );
        }

        GR.Image.FastImage fastImage = new GR.Image.FastImage( 1, 1, System.Drawing.Imaging.PixelFormat.Format32bppRgb );
        for ( int j = 0; j < 16; ++j )
        {
          fastImage.SetPaletteColor( j, (byte)( ( m_Project.m_ColorValues[j] & 0x00ff0000 ) >> 16 ), (byte)( ( m_Project.m_ColorValues[j] & 0x0000ff00 ) >> 8 ), (byte)( m_Project.m_ColorValues[j] & 0xff ) );
        }
        fastImage.SetPixel( 0, 0, (uint)drawIndex );
        System.Drawing.Rectangle drawRect = new Rectangle( e.Bounds.Location, e.Bounds.Size );
        drawRect.X += 20;
        drawRect.Width = 16;
        drawRect.Y += ( drawRect.Height - 16 ) / 2;
        drawRect.Height = 16;

        fastImage.DrawToHDC( e.Graphics.GetHdc(), drawRect );
        e.Graphics.ReleaseHdc();
        fastImage.Dispose();
      }
      if ( index == -1 )
      {
        System.Drawing.RectangleF textRect = new RectangleF( e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height );
        System.Drawing.Brush textBrush = new SolidBrush( e.ForeColor );
        e.Graphics.DrawString( "Def", e.Font, textBrush, textRect );
      }
    }



    private void editObjectOptionalOn_TextChanged( object sender, EventArgs e )
    {
      if ( listScreenElements.SelectedIndices.Count == 0 )
      {
        return;
      }
      int elementIndex = listScreenElements.SelectedIndices[0];

      Project.ScreenElement screenElement = m_CurrentScreen.DisplayedElements[elementIndex];

      if ( ( screenElement.Object != null )
      &&   ( GR.Convert.ToI32( editObjectOptionalOn.Text ) != screenElement.Object.OptionalValue ) )
      {
        screenElement.Object.OptionalValue = GR.Convert.ToI32( editObjectOptionalOn.Text );
        Modified = true;
        RedrawScreen();
      }
    }



    private void toolStripButton1_Click( object sender, EventArgs e )
    {
      if ( string.IsNullOrEmpty( m_ProjectFilename ) )
      {
        SaveFileDialog saveFile = new SaveFileDialog();

        saveFile.Title = "Save editor project";
        saveFile.Filter = "Element Editor Project Files|*.elementeditorproject";

        if ( saveFile.ShowDialog() == DialogResult.OK )
        {
          m_ProjectFilename = saveFile.FileName;
        }
        else
        {
          return;
        }
      }
      if ( !string.IsNullOrEmpty( m_Project.SpriteProjectFilename ) )
      {
        m_Project.SpriteProjectFilename = GR.Path.RelativePathTo( m_ProjectFilename, false, m_Project.SpriteProjectFilename, false );
      }
      if ( m_Project.SaveToFile( m_ProjectFilename ) )
      {
        Modified = false;

        btnExport_Click( null, null );
      }
    }



    private void btnPasteFromExport_Click( object sender, EventArgs e )
    {
      string    text = System.Windows.Forms.Clipboard.GetText();

      int width = 1;
      int height = 1;

      var lines = text.Split( new string[]{ "\r\n" }, StringSplitOptions.RemoveEmptyEntries ).ToList();
      for ( int i = 0; i < lines.Count; ++i )
      {
        if ( lines[i].StartsWith( ";" ) )
        {
          if ( ( i == 0 )
          &&   ( lines[i].StartsWith( ";size" ) ) )
          {
            var line = lines[i].Substring( 5 );

            string[]  parts2 = line.Split( ',' );

            if ( parts2.Length < 2 )
            {
              return;
            }
            width = GR.Convert.ToI32( parts2[0] );
            height = GR.Convert.ToI32( parts2[1] );
          }
          lines.RemoveAt( i );
          --i;
          continue;
        }
      }

      text = string.Join( "\n", lines.ToArray() );

      text = text.Replace( "!byte", " " );
      text = text.Replace( '\n', ',' ).Replace( " ", " " );

      string[]  parts = text.Split( ',' );

      if ( parts.Length != 2 * width * height )
      {
        System.Windows.Forms.MessageBox.Show( "Data not valid, expect w,h,w*h chars,w*h colors\r\n"
          + width + " * " + height + " = " + ( ( width * height ) * 2 )  + " != " + parts.Length, "Data size does not match" );
        return;
      }

      if ( listAvailableElements.SelectedIndex == -1 )
      {
        return;
      }
      Project.Element   element = m_Project.Elements[listAvailableElements.SelectedIndex];

      element.Characters.Resize( width, height );

      for ( int l = 0; l < 2; ++l )
      {
        for ( int j = 0; j < height; ++j )
        {
          for ( int i = 0; i < width; ++i )
          {
            if ( l == 0 )
            {
              string      content = parts[i + j * width].Trim();
              byte        value = 0;

              if ( content.StartsWith( "$" ) )
              {
                value = GR.Convert.ToU8( content.Substring( 1 ), 16 );
              }
              else
              {
                value = GR.Convert.ToU8( content );
              }

              element.Characters[i, j].Char = value;
            }
            else
            {
              string      content = parts[width * height + i + j * width].Trim();
              byte        value = 0;

              if ( content.StartsWith( "$" ) )
              {
                value = GR.Convert.ToU8( content.Substring( 1 ), 16 );
              }
              else
              {
                value = GR.Convert.ToU8( content );
              }

              element.Characters[i, j].Color = value;
            }
          }
        }
      }

      listAvailableElements_SelectedIndexChanged( sender, e );
      Modified = true;
      RedrawScreen();
      RedrawElementPreview();
    }



    private void projectStatsToolStripMenuItem_Click( object sender, EventArgs e )
    {
      Debug.Log( "Project has" );
      Debug.Log( m_Project.Elements.Count + " elements" );
      Debug.Log( m_Project.ObjectTemplates.Count + " object templates" );
      Debug.Log( m_Project.Screens.Count + " screens" );
      Debug.Log( m_Project.CharsetProjects.Count + " charsets" );

      GR.Collections.Map<Project.ScreenElementType,int>       counts = new GR.Collections.Map<Project.ScreenElementType, int>();

      foreach ( var screen in m_Project.Screens )
      {
        foreach ( var element in screen.DisplayedElements )
        {
          ++counts[element.Type];
        }
      }

      foreach ( var entry in counts )
      {
        Debug.Log( entry.Value + " total elements of type " + entry.Key );
      }
    }



    private void btnCopyChar_Click( object sender, EventArgs e )
    {
      if ( ( listElementChars.SelectedIndices.Count == 0 )
      || ( m_CurrentEditedElement == null ) )
      {
        return;
      }
      int    charIndex = listElementChars.SelectedIndices[0];

      if ( charIndex + 1 < m_CurrentEditedElement.Characters.Width * m_CurrentEditedElement.Characters.Height )
      {
        int   newCharIndex = charIndex + 1;
        m_CurrentEditedElement.Characters[newCharIndex % m_CurrentEditedElement.Characters.Width, newCharIndex / m_CurrentEditedElement.Characters.Width].Char = (byte)( m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Char );
        m_CurrentEditedElement.Characters[newCharIndex % m_CurrentEditedElement.Characters.Width, newCharIndex / m_CurrentEditedElement.Characters.Width].Color = m_CurrentEditedElement.Characters[charIndex % m_CurrentEditedElement.Characters.Width, charIndex / m_CurrentEditedElement.Characters.Width].Color;

        listElementChars.Items[newCharIndex].SubItems[1].Text = m_CurrentEditedElement.Characters[newCharIndex % m_CurrentEditedElement.Characters.Width, newCharIndex / m_CurrentEditedElement.Characters.Width].Char.ToString();
        listElementChars.Items[newCharIndex].SubItems[2].Text = m_CurrentEditedElement.Characters[newCharIndex % m_CurrentEditedElement.Characters.Width, newCharIndex / m_CurrentEditedElement.Characters.Width].Color.ToString();

        listElementChars.SelectedIndices.Remove( charIndex );
        listElementChars.SelectedIndices.Add( charIndex + 1 );
      }
      Modified = true;
      RedrawElementPreview();
    }



    private void pictureElement_MouseDown( object sender, MouseEventArgs e )
    {
      if ( m_CurrentEditedElement == null )
      {
        return;
      }

      int     tx = e.X / ( pictureElement.DisplayRectangle.Width / 21 );
      int     ty = e.Y / ( pictureElement.DisplayRectangle.Height / 21 );

      if ( tx >= m_CurrentEditedElement.Characters.Width )
      {
        tx = m_CurrentEditedElement.Characters.Width - 1;
      }

      int   trueIndex = tx + ty * m_CurrentEditedElement.Characters.Width;
      if ( ( trueIndex < listElementChars.Items.Count )
      &&   ( trueIndex >= 0 ) )
      {
        listElementChars.SelectedIndices.Clear();
        listElementChars.SelectedIndices.Add( trueIndex );
        listElementChars.EnsureVisible( trueIndex );
      }
    }



    private void saveAsToolStripMenuItem_Click( object sender, EventArgs e )
    {
      SaveFileDialog saveFile = new SaveFileDialog();

      saveFile.Title = "Save editor project as";
      saveFile.Filter = "Element Editor Project Files|*.elementeditorproject";

      if ( saveFile.ShowDialog() == DialogResult.OK )
      {
        m_ProjectFilename = saveFile.FileName;
      }
      else
      {
        return;
      }
      if ( !string.IsNullOrEmpty( m_Project.SpriteProjectFilename ) )
      {
        m_Project.SpriteProjectFilename = GR.Path.RelativePathTo( m_ProjectFilename, false, m_Project.SpriteProjectFilename, false );
      }
      if ( m_Project.SaveToFile( m_ProjectFilename ) )
      {
        Modified = false;
      }
    }



  }
}
