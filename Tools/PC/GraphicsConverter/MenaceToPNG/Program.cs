using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

// DavePoo2 - May 2025
// Script to Convert Menace "Aliens" file back into an RGB PNG for each alien graphic stored in the file.

class MenanceTools
{
    static void Main(string[] args)
    {
        MenanceAliensToPNG MenaceToPNG = new MenanceAliensToPNG(args);
        MenaceToPNG.Run();

        MenaceBackgroundsToPNG MenaceBackgroundsToPNG = new MenaceBackgroundsToPNG(args);
        MenaceBackgroundsToPNG.Run();

        MenaceForegroundsToPNG MenaceForegroundsToPNG = new MenaceForegroundsToPNG(args);
        MenaceForegroundsToPNG.Run();
    }
}

/** Used to read in the "Aliens" file from the Menace source code. */ 
struct Alien
{
    public Alien( String InMenaceSpriteName, int InNumSprites, int Index )
    {        
        MenaceSpriteName = InMenaceSpriteName;
        OutputFileName = "Aliens_" + InMenaceSpriteName + ".png";
        AlienIndex = Index;
        NumSprites = InNumSprites;
        ConvertMenaceColorsToRGB();
    }

    int AlienIndex = 0;
    public int NumSprites = 1;
    public String OutputFileName = "MenaceSprite.png";

    public String MenaceSpriteName = "MenaceSprite";

    readonly static int NumColors = 8;

    /** 8 colors in Menace Form, 4bit RGB 
    * The default value here is from the explosion sprite (1st in the aliens file)*/ 
    public UInt16[] MenaceColours = { 0x0332,0x0055,0x0543,0x0000,0x0DFF,0x06AC,0x036A,0x0038 };

    /** 8 Colors in ARGB format */    
    Color[] AlienColorsRGB = new Color[NumColors];

    public void ConvertMenaceColorsToRGB()
    {
        for (int i = 0; i < NumColors; i++)
        {
            AlienColorsRGB[i] = IndexToMenaceColor( i );
        }
    }

    public Color ColorForIndex( int Index )
    {
        return AlienColorsRGB[Index % NumColors];
    }

    /** Convert the index read from the aliens file into an RGB color */ 
    readonly Color IndexToMenaceColor( int Index ) 
    {
        const int MaxColors = 8;
        int I = Index % MaxColors;
        UInt32 RawColor = MenaceColours[I];
        return Amiga4BitColorToRGBColor( RawColor );
    }

    /** Convert Amiga 4-bit color value to RGB Color */
   public static Color Amiga4BitColorToRGBColor( UInt32 RawColor )
    {
        // Convert 4bit color to RGB
        const int FourBitShift = 4;
        const int FourBitMask = 0xF;
        const int FourBitToByte = 16; 
        int A = 0xFF;
        uint B = ((RawColor >>> 0x0) & FourBitMask) * FourBitToByte;
        uint G = ((RawColor >>> FourBitShift) & FourBitMask) * FourBitToByte;
        uint R = ((RawColor >>> (FourBitShift * 2)) & FourBitMask) * FourBitToByte;
        return Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B);
    }

};

class MenanceAliensToPNG
{
    const int NumAliensInPackedFile = 15;
    Alien[] Aliens = new Alien[NumAliensInPackedFile];

    /** Where to read the Menace raw graphics from */
    String DataPath;

    String OutputPath;

    public MenanceAliensToPNG( string[] args )
    {
        Console.WriteLine("Menace 'Aliens' to PNG - DavePoo2 May 2025 - v1.00");

        if ( args.Length == 0 )
        {
            throw new Exception("Expected the first argument to be the path to the data to process");
        }
        else
        {
            const int DataPathIndex = 0;
            DataPath = args[DataPathIndex] + "\\";
        }

        OutputPath = DataPath + "\\PNG\\";

        Console.WriteLine("DataPath: " + DataPath);
        Console.WriteLine("OutputPath: " + OutputPath);
    }

    public void Run() 
    { 
        int TotalNumSprites = this.InitAliens();

        // Read in the master file and parse each alien into their own PNG
        String AliensFileName =  DataPath + "Aliens";
        using (FileStream SourceStream = File.Open(AliensFileName, FileMode.Open))
        {
            const int NumBitPlanes = 4;
            const int SpriteWidth = 32;
            const int SpriteHeight = 24;
            const int NumSpritesPerRow = 1;
            const int SpriteRowSizeBits =  (SpriteWidth * NumSpritesPerRow); 
            const int SpriteLineSizeBytes = SpriteRowSizeBits / 8;
            const int BytesPerBitPlane = SpriteLineSizeBytes * SpriteHeight;
            int BufferSize = BytesPerBitPlane * NumBitPlanes * TotalNumSprites;
            byte[] Buffer = new byte[BufferSize];

            SourceStream.Read(Buffer,0,BufferSize);
            Console.WriteLine("BufferSize=" + BufferSize);

            int MasterSpriteIndex = 0; 
            for (int AlienIndex = 0; AlienIndex < Aliens.Length; AlienIndex++)
            {
                String FileName = Aliens[AlienIndex].OutputFileName;
                int TotalNumRows = Aliens[AlienIndex].NumSprites * SpriteHeight; 
                Bitmap bmp = new(SpriteRowSizeBits,TotalNumRows);
                Console.WriteLine("Reading: " + FileName + " NumSprites=" + Aliens[AlienIndex].NumSprites );

                for (int AlienSpriteIndex = 0; AlienSpriteIndex < Aliens[AlienIndex].NumSprites; AlienSpriteIndex++)
                {
                    for (int Row = 0; Row < SpriteHeight; Row++)
                    {
                        //Console.WriteLine("Row=" + Row );
                        for (int i = 0; i < SpriteLineSizeBytes; i++)
                        {
                            //int SpriteIndex = Row / SpriteHeight;
                            int SpriteByteStart = MasterSpriteIndex * (BytesPerBitPlane * NumBitPlanes);
                            //Console.WriteLine("MasterSpriteIndex=" + MasterSpriteIndex + " SpriteByteStart=" + SpriteByteStart + " SpriteRow=" + SpriteRow);
                            int ByteIndex = SpriteByteStart + (Row * SpriteLineSizeBytes) + i;
                            byte b0 = Buffer[ByteIndex];
                            byte b1 = Buffer[ByteIndex + BytesPerBitPlane];
                            byte b2 = Buffer[ByteIndex + (BytesPerBitPlane * 2)];
                            byte Mask = Buffer[ByteIndex + (BytesPerBitPlane * 3)];

                            // Write X,y into bitmap
                            int y = Row + (AlienSpriteIndex * SpriteHeight);
                            for (int Bit = 0; Bit < 8; Bit++)
                            {
                                int b = ((b0 >>> Bit) & 0b1) | (((b1 >>> Bit) & 0b1) << 1) | (((b2 >>> Bit) & 0b1) << 2); 
                                int x = (i * 8) + (7 - Bit);
                                //Console.WriteLine("Writing x=" + x + " y= " + Row + " Val=" + b);
                                Color MenaceColor = Aliens[AlienIndex].ColorForIndex( b ); 
                                int Alpha = (((Mask >>> Bit) & 0b1) == 1) ? 0xFF : 0;   //Get the mask bit                               
                                MenaceColor = Color.FromArgb( Alpha, MenaceColor );
                                bmp.SetPixel( x, y, MenaceColor );
                            }
                        }
                    }
                    MasterSpriteIndex++;
                }

                Console.WriteLine( "Writing: " + FileName );
                bmp.Save( OutputPath + FileName, ImageFormat.Png );
            }
        }
    }

    /** Initialise a data structure for each of the aliens we need to read out of the packed file */
    int InitAliens()
    {
        // This array needs to be initalised in the same order they are stored in the packed file
        // All sprites must be read, any errors in the number of sprites will compound 
        // when reading subsequenct sprites. The number of sprites for each alien is taken
        // from "alien.pointers" in Menace.s
        // Each pointer was multplied by 384 in the file which is 24 lines * 4 bytes per line * 4 bit planes
        // There are 3 bitplanes for the colors and 1 bitplane for the mask stored in the packed file.  
        // Colour palettes are stored in Menace.s "alien.colours", each line defines the 8 colours for each alien
        int Index = 0;
        Aliens[Index] = new Alien( "explosion1", 9, Index++ );
        Aliens[Index] = new Alien( "guardian.eye1", 4, Index++ );
        Aliens[Index] = new Alien( "tadpole", 4, Index++ );
        Aliens[Index] = new Alien( "eye", 15, Index++ );
        Aliens[Index] = new Alien( "bubble", 4, Index++ );
        Aliens[Index] = new Alien( "jellyfish1", 4, Index++ );
        Aliens[Index] = new Alien( "jellyfish2", 4, Index++ );
        Aliens[Index] = new Alien( "bordertl", 6, Index++ );
        Aliens[Index] = new Alien( "borderbl", 6, Index++ );
        Aliens[Index] = new Alien( "borderbl", 6, Index++ );
        Aliens[Index] = new Alien( "bordertr", 6, Index++ );
        Aliens[Index] = new Alien( "mouth", 8, Index++ );
        Aliens[Index] = new Alien( "slime", 9, Index++ );
        Aliens[Index] = new Alien( "snakebody", 1, Index++ );
        Aliens[Index] = new Alien( "snakehead", 5, Index++ );

        int TotalNumSprites = 0;
        for (int i = 0; i < Aliens.Length; i++)
        {
            TotalNumSprites += Aliens[i].NumSprites; 
        }

        // Load and parse the colours from a text file pasted from menace.s
        using StreamReader ColoursStream = new StreamReader( DataPath + "Alien.Colours.txt" ); 
        ColoursStream.ReadLine();   // First 2 lines are NOT the alien colors?? eat them
        ColoursStream.ReadLine();   // First 2 lines are NOT the alien colors?? eat them
        for (int AlienIndex = 0; AlienIndex < Aliens.Length; AlienIndex++)
        {
            Console.WriteLine( "Reading Palette For:"  + Aliens[AlienIndex].OutputFileName );
            String RawColours = ColoursStream.ReadLine(); 
            Console.WriteLine( "RawColours: "  + RawColours );
            RawColours = RawColours.Replace( "DC.W", "" );
            RawColours = RawColours.Trim();
            String[] SplitColorsHex = RawColours.Split(","); 
            for (int i = 0; i < SplitColorsHex.Length; i++)
            {
                SplitColorsHex[i] = SplitColorsHex[i].Replace("$0", "");
                int HexColor = int.Parse( SplitColorsHex[i], System.Globalization.NumberStyles.HexNumber );
                Console.WriteLine( "Color[" + i + "]: "  + HexColor + " 0x" + SplitColorsHex[i]);
                Aliens[AlienIndex].MenaceColours[i] = (UInt16)HexColor;
            }
            Aliens[AlienIndex].ConvertMenaceColorsToRGB();
        }
        ColoursStream.Close();

        return TotalNumSprites;
    }
};


/** Convert a number of colours from assembly source code into a palette of colours */
class SourceCodeToColorPalette
{
    /** Where to read the Menace raw palette from */
    String DataPath;

    /** The source file name */
    String FileName;

    /** The palette we converted from the file */
    List<Color> Palette;

    public SourceCodeToColorPalette(String InDataPath, String InFileName)
    {
        Console.WriteLine("Menace 'Palette' to PNG - DavePoo2 May 2025 - v1.00");
        DataPath = InDataPath + "\\";
        FileName = InFileName;
        Palette = new List<Color>();
        Console.WriteLine("DataPath: " + DataPath + "\\" + FileName);
        ParsePalette();
    }

    void ParsePalette()
    {
        // Load and parse the colours from a text file pasted from menace.s
        Console.WriteLine("Reading Palette For:" + FileName);
        using StreamReader ColoursStream = new StreamReader(DataPath + FileName);

        String RawColours;
        while ((RawColours = ColoursStream.ReadLine()) != null)
        {
            Console.WriteLine("RawColours: " + RawColours);
            String DeclareConstantWord = "DC.W";
            if (RawColours.Contains(DeclareConstantWord))
            {
                RawColours = RawColours.Replace(DeclareConstantWord, "");
                RawColours = RawColours.Trim();
                String[] SplitColorsHex = RawColours.Split(",");
                for (int i = 0; i < SplitColorsHex.Length; i++)
                {
                    SplitColorsHex[i] = SplitColorsHex[i].Replace("$0", "");
                    int HexColor = int.Parse(SplitColorsHex[i], System.Globalization.NumberStyles.HexNumber);
                    //Console.WriteLine( "Color[" + i + "]: "  + HexColor + " 0x" + SplitColorsHex[i]);
                    UInt16 RawColorValue = (UInt16)HexColor;
                    Palette.Add(Alien.Amiga4BitColorToRGBColor(RawColorValue));
                }
            }
            else
            {
                Console.WriteLine("Skipped Line: " + RawColours);
            }
        }
        ColoursStream.Close();
    }

    /** For the given palette index what is the color value */
    public Color IndexToColor(int Index, int Alpha = 0xFF)
    {
        Color c = Palette[Index];
        c = Color.FromArgb(Alpha, c);
        return c;
    }

    /** write this palette into the bmp's palette */
    public void WritePaletteToImage(Bitmap bmp)
    {
        ColorPalette PaletteClone = bmp.Palette;    // This returns a clone, so you can't modify it directly
        for (int Index = 0; Index < PaletteClone.Entries.Length; Index++)
        {
            if (Index >= Palette.Count())
            {
                PaletteClone.Entries[Index] = Color.Magenta;
            }
            else
            {
                PaletteClone.Entries[Index] = Palette[Index];
            }
        }
        bmp.Palette = PaletteClone;
    }
};

/** Menace backgrounds are stored in Meance.s with the label "backgrounds"
  Backgrounds are made up of blocks that are (2 bytes x 16 high x 2 planes) with a max of 1024 bytes for all the data, 
  so there are 1024 / 64 = 16 blocks stored in the data.
  
  Note the code in Menace.s stores the background map as 4-bits per tiles, so there is max of 16 blocks in the 
  background map (level 1 stored in menace.s only uses 12 of the possible 16 blocks)

  The background map data is a 24 x 12 map

  Convert the bitplane graphics and palette into PNG and convert the that and the background map (background table)
  into TMX format that can be read by the "Tiled" map editor on PC/Mac/Linux
*/
class MenaceBackgroundsToPNG
{
    /** Where to read the Menace raw graphics from */
    String DataPath;

    String OutputPath;

    /** The numbers read from Menace.s "backgrounds" */
    List<byte> BackgroundsRawData;

    /** The numbers read from Menace.s "backgroundtable" */
    List<byte> BackgroundTableRawData;

    /** The Palette we read from file */
    SourceCodeToColorPalette Palette;

    const int NumBitPlanes = 2;
    const int BlockWidthBytes = BlockWidthPixels / 8;
    const int BlockWidthPixels = 16;
    const int BlockHeightPixels = 16;
    const int BlockBytesPerBitPlane = BlockWidthBytes * BlockHeightPixels;
    const int BlockBytes = BlockBytesPerBitPlane * NumBitPlanes;
    const int NumBlocks = 12;   //see note in header, comments/code say 16 (which is the max supported in 4bits), but there are only 12 in this set

    public MenaceBackgroundsToPNG(string[] args)
    {
        Console.WriteLine("Menace 'Backgrounds' to PNG - DavePoo2 May 2025 - v1.00");

        if (args.Length == 0)
        {
            throw new Exception("Expected the first argument to be the path to the data to process");
        }
        else
        {
            const int DataPathIndex = 0;
            DataPath = args[DataPathIndex] + "\\";
        }

        OutputPath = DataPath + "\\PNG\\";

        Console.WriteLine("DataPath: " + DataPath);
        Console.WriteLine("OutputPath: " + OutputPath);

        ParseBackgrounds();
    }

    /** Initialise a data structure for the backgrounds to be read out of the text file */
    void ParseBackgrounds()
    {
        Palette = new SourceCodeToColorPalette(DataPath, "level.colours.txt");

        // Load and parse the backgrounds (images) from a text file pasted from menace.s, turn it into a list of raw numbers
        using (StreamReader BackgroundsStream = new StreamReader(DataPath + "Backgrounds.txt"))
        {
            const int ExpectedNumberOfBytesRead = 12 * 32 * sizeof(UInt16);
            BackgroundsRawData = new List<byte>(ExpectedNumberOfBytesRead);
            String line;
            while ((line = BackgroundsStream.ReadLine()) != null)
            {
                line = line.Replace("backgrounds", "");
                line = line.Replace("DC.W", "");
                line = line.Trim();
                Console.WriteLine(line);

                if (line != "")
                {
                    String[] TextAsHex = line.Split(",");
                    for (int i = 0; i < TextAsHex.Length; i++)
                    {
                        TextAsHex[i] = TextAsHex[i].Trim();
                        Debug.Assert(TextAsHex[i].StartsWith("$"), TextAsHex[i] + " doesn't start with $");
                        TextAsHex[i] = TextAsHex[i].Replace("$", "");
                        int HexValue = int.Parse(TextAsHex[i], System.Globalization.NumberStyles.HexNumber);
                        BackgroundsRawData.Add((byte)((HexValue >>> 8) & 0xFF));
                        BackgroundsRawData.Add((byte)(HexValue & 0xFF));

                        //Console.WriteLine(TextAsHex[i] + " = " + HexValue);
                    }
                }
            }

            Debug.Assert(BackgroundsRawData.Count() == ExpectedNumberOfBytesRead, "Expected " + ExpectedNumberOfBytesRead + " bytes to be read for the backgrounds");
        }

        // Load and parse the backgroundtable (map data) from a text file pasted from menace.s, turn it into a list of raw numbers
        using (StreamReader BackgroundTableStream = new StreamReader(DataPath + "BackgroundTable.txt"))
        {
            const int BlocksAcross = 24;
            const int BlocksHigh = 12;
            const int ExpectedNumberOfBytesRead = BlocksAcross * BlocksHigh;
            BackgroundTableRawData = new List<byte>(ExpectedNumberOfBytesRead);
            String line;
            while ((line = BackgroundTableStream.ReadLine()) != null)
            {
                line = line.Replace("backgroundtable", "");
                line = line.Replace("DC.W", "");
                line = line.Trim();
                Console.WriteLine(line);

                if (line != "")
                {
                    String[] TextAsHex = line.Split(",");
                    for (int i = 0; i < TextAsHex.Length; i++)
                    {
                        TextAsHex[i] = TextAsHex[i].Trim();
                        Debug.Assert(TextAsHex[i].StartsWith("$"), TextAsHex[i] + " doesn't start with $");
                        TextAsHex[i] = TextAsHex[i].Replace("$", "");
                        int HexValue = int.Parse(TextAsHex[i], System.Globalization.NumberStyles.HexNumber);

                        // The data is 4-bit per tile index
                        byte FirstByte = (byte)((HexValue >>> 8) & 0xFF);
                        byte SecondByte = (byte)(HexValue & 0xFF);


                        byte b0 = (byte)(FirstByte >>> 4);
                        byte b1 = (byte)(FirstByte & 0xF);
                        byte b2 = (byte)(SecondByte >>> 4);
                        byte b3 = (byte)(SecondByte & 0xF);

                        BackgroundTableRawData.Add(b0);
                        BackgroundTableRawData.Add(b1);
                        BackgroundTableRawData.Add(b2);
                        BackgroundTableRawData.Add(b3);

                        //Console.WriteLine(TextAsHex[i] + " = " + HexValue);
                    }
                }
            }

            Debug.Assert(BackgroundTableRawData.Count() == ExpectedNumberOfBytesRead, "Expected " + ExpectedNumberOfBytesRead + " bytes to be read for the backgrounds");
        }
    }

    public void Run()
    {
        WriteBackgroundsPNG();
        WriteBackgroundTileData();
    }

    void WriteBackgroundsPNG()
    {
        // Turn #BackgroundsRawData into a PNG
        String FileName = "backgrounds.png";
        Console.WriteLine("Writing: " + FileName);

        Bitmap bmp = new(BlockWidthPixels, BlockHeightPixels * NumBlocks, PixelFormat.Format8bppIndexed);
        Palette.WritePaletteToImage( bmp );
        BitmapData BmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadWrite, bmp.PixelFormat);

        for (int BlockIndex = 0; BlockIndex < NumBlocks; BlockIndex++)
        {
            for (int Row = 0; Row < BlockHeightPixels; Row++)
            {
                for (int i = 0; i < BlockWidthBytes; i++)
                {
                    int BlockByteStart = BlockIndex * (BlockBytesPerBitPlane * NumBitPlanes);
                    int ByteIndex = BlockByteStart + (Row * BlockWidthBytes) + i;
                    byte b0 = BackgroundsRawData[ByteIndex];
                    byte b1 = BackgroundsRawData[ByteIndex + BlockBytesPerBitPlane];

                    // Write X,y into bitmap
                    int y = Row + (BlockIndex * BlockHeightPixels);
                    for (int Bit = 0; Bit < 8; Bit++)
                    {
                        int b = ((b0 >>> Bit) & 0b1) | (((b1 >>> Bit) & 0b1) << 1);
                        int x = (i * 8) + (7 - Bit);
                        //Console.WriteLine("Writing x=" + x + " y= " + Row + " Val=" + b);                        
                        byte Index = (byte)(b);
                        IntPtr Pixel = BmpData.Scan0 + (x) + (y * bmp.Width);
                        Marshal.WriteByte(Pixel, Index);
                    }
                }
            }
        }

        bmp.UnlockBits(BmpData);

        Console.WriteLine("Writing: " + FileName);
        bmp.Save(OutputPath + FileName, ImageFormat.Png);
    }

    static void WriteTiledVersionToXml( XmlWriter Xml )
    {
        Xml.WriteAttributeString("version", "1.10");
        Xml.WriteAttributeString("tiledversion", "1.11.2");
    }

    static void WriteDocTypeXml( XmlWriter Xml )
    {
        Xml.WriteDocType("TMX", null, "http://mapeditor.org/dtd/1.0/map.dtd", null);
    }

    void WriteBackgroundTileData()
    {
        // Turn #BackgroundTableRawData into a tile data TMX file for use in "Tiled" editor https://www.mapeditor.org/docs
        // https://doc.mapeditor.org/en/stable/reference/tmx-map-format/
        // https://code.tutsplus.com/parsing-and-rendering-tiled-tmx-format-maps-in-your-own-game-engine--gamedev-3104t

        String TileSetFileName = "backgrounds_tileset.xml";
        Console.WriteLine("Writing: " + TileSetFileName);

        using (XmlTextWriter Xml = new XmlTextWriter(Path.Combine(OutputPath, TileSetFileName), Encoding.UTF8))
        {
            Xml.Formatting = Formatting.Indented;
            WriteDocTypeXml(Xml);
            Xml.WriteStartElement("tileset");
            Xml.WriteAttributeString("name", "MenaceBackgroundTilesLevel1");
            WriteTiledVersionToXml(Xml);
            Xml.WriteAttributeString("tilewidth", "" + BlockWidthPixels);
            Xml.WriteAttributeString("tileheight", "" + BlockHeightPixels);
            Xml.WriteAttributeString("tilecount", "" + NumBlocks);
            Xml.WriteAttributeString("columns", "1");
            Xml.WriteStartElement("image");
            Xml.WriteAttributeString("source", "Backgrounds.png");
            Xml.WriteAttributeString("width", "" + BlockWidthPixels);
            Xml.WriteAttributeString("height", "" + BlockHeightPixels * NumBlocks);
            Xml.WriteEndElement();  //image
            Xml.WriteEndElement();  //tileset            
            Xml.Flush();
            Xml.Close();
        }
        
        String MapFileName = "backgrounds_level1.tmx";
        Console.WriteLine("Writing: " + MapFileName);

        using (XmlTextWriter Xml = new XmlTextWriter(Path.Combine(OutputPath, MapFileName), Encoding.UTF8))
        {
            const int BlocksAcross = 24;
            const int BlocksHigh = 12;
            const int FirstgidIndex = 1;
            Xml.Formatting = Formatting.Indented;
            WriteDocTypeXml(Xml); 
            Xml.WriteStartElement("map");
            WriteTiledVersionToXml(Xml);
            Xml.WriteAttributeString("orientation", "orthogonal");
            Xml.WriteAttributeString("width", "" + BlocksAcross);
            Xml.WriteAttributeString("height", "" + BlocksHigh);
            Xml.WriteAttributeString("tilewidth", "" + BlockWidthPixels);
            Xml.WriteAttributeString("tileheight", "" + BlockHeightPixels);

            Xml.WriteStartElement("tileset");
            Xml.WriteAttributeString("firstgid", ""+FirstgidIndex);
            Xml.WriteAttributeString("source", TileSetFileName);
            Xml.WriteEndElement();  //tileset            


            Xml.WriteStartElement("layer");
            Xml.WriteAttributeString("name", "MenaceBackgroundLevel1");
            Xml.WriteAttributeString("width", "" + BlocksAcross);
            Xml.WriteAttributeString("height", "" + BlocksHigh);
            Xml.WriteStartElement("data");
            for (int Row = 0; Row < BlocksHigh; Row++)
            {
                for (int Block = 0; Block < BlocksAcross; Block++)
                {
                    Xml.WriteStartElement("tile");
                    int TMXgid = BackgroundTableRawData[(Row * BlocksHigh) + Block] + FirstgidIndex;
                    Xml.WriteAttributeString("gid", "" + TMXgid);
                    Xml.WriteEndElement();  //tile
                }
            }
            Xml.WriteEndElement();  //data
            Xml.WriteEndElement();  //layer

            Xml.WriteEndElement();  //map

            Xml.Flush();
            Xml.Close();
        }
    }
};

/** Menace backgrounds are stored in 
  Meance.s - palette with the label "level.colours"
  foregrounds - graphics tiles, 16 x 16 x 3 bitplanes (no mask). 255 tiles are stored.
  map - 8-bit map data, 12 blocks high, 440 blocks across. Appears to be an FFFF end of file sential in the file
    The map data appears to be stored as 12 blocks going down in a vertical strip (as this corresponds to how it is drawn by the Amiga)
  
  Convert the bitplane graphics and palette into PNG
  Convert that and the map data into TMX format that can be read by the "Tiled" map editor on PC/Mac/Linux
*/
class MenaceForegroundsToPNG
{
    /** Where to read the Menace raw graphics from */
    String DataPath;

    String OutputPath;

    /** The numbers read from "foregrounds", this is the graphics bitplane data */
    byte[] ForegroundsRawData;

    /** The numbers read from Menace.s "map", each byte is a map tile index */
    byte[] MapRawData;

    /** The Palette we read from file */
    SourceCodeToColorPalette Palette;

    const int NumBitPlanes = 3;
    const int BlockWidthBytes = BlockWidthPixels / 8;
    const int BlockWidthPixels = 16;
    const int BlockHeightPixels = 16;
    const int BlockBytesPerBitPlane = BlockWidthBytes * BlockHeightPixels;
    const int BlockBytes = BlockBytesPerBitPlane * NumBitPlanes;
    const int NumBlocks = 255;   //see note in header
    const int ForegroundsBlocksPerRow = 16;
    const int ForegroundsBlocksPerCol = 16;
    const int MapBlocksAcross = 440;
    const int MapBlocksHigh = 12;


    public MenaceForegroundsToPNG(string[] args)
    {
        Console.WriteLine("Menace 'Foregrounds' to PNG - DavePoo2 May 2025 - v1.00");

        if (args.Length == 0)
        {
            throw new Exception("Expected the first argument to be the path to the data to process");
        }
        else
        {
            const int DataPathIndex = 0;
            DataPath = args[DataPathIndex] + "\\";
        }

        OutputPath = DataPath + "\\PNG\\";

        Console.WriteLine("DataPath: " + DataPath);
        Console.WriteLine("OutputPath: " + OutputPath);

        ParseForegrounds();
    }

    /** Initialise a data structure for the foregrounds to be read out of the text file */
    void ParseForegrounds()
    {
        Palette = new SourceCodeToColorPalette(DataPath, "level.colours.txt");

        // Load and parse the foreground (images)
        using (FileStream ForegroundsStream = new FileStream(DataPath + "foregrounds", FileMode.Open))
        {
            const int ExpectedNumberOfBytesRead = NumBlocks * BlockWidthBytes * BlockHeightPixels * NumBitPlanes;

            ForegroundsRawData = new byte[ExpectedNumberOfBytesRead];
            int Offset = 0;
            int NumBytesRead = 0;
            while ((NumBytesRead = ForegroundsStream.Read(ForegroundsRawData, Offset, ExpectedNumberOfBytesRead - Offset)) > 0)
            {
                Offset += NumBytesRead;
            }
        }

        // Load the "map" (map data) from a binary file
        using (FileStream ForegroundsMapStream = new FileStream(DataPath + "map", FileMode.Open))
        {
            const int ExpectedNumberOfBytesRead = MapBlocksAcross * MapBlocksHigh;
            MapRawData = new byte[ExpectedNumberOfBytesRead];

            int Offset = 0;
            int NumBytesRead = 0;
            while ((NumBytesRead = ForegroundsMapStream.Read(MapRawData, Offset, ExpectedNumberOfBytesRead - Offset)) > 0)
            {
                Offset += NumBytesRead;
            }
        }
    }

    public void Run()
    {
        WriteForegroundsPNG();
        WriteForegroundTileData();
    }

    void WriteForegroundsPNG()
    {
        // Turn #ForegroundsRawData into a PNG
        String FileName = "foregrounds.png";
        Console.WriteLine("Writing: " + FileName);

        Bitmap bmp = new(
            BlockWidthPixels * ForegroundsBlocksPerRow,
            BlockHeightPixels * ForegroundsBlocksPerCol,
            PixelFormat.Format8bppIndexed);
        Palette.WritePaletteToImage( bmp );
        BitmapData BmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, bmp.PixelFormat);

        for (int BlockIndex = 0; BlockIndex < NumBlocks; BlockIndex++)
        {
            for (int Row = 0; Row < BlockHeightPixels; Row++)
            {
                for (int i = 0; i < BlockWidthBytes; i++)
                {
                    int BlockByteStart = BlockIndex * (BlockBytesPerBitPlane * NumBitPlanes);
                    int ByteIndex = BlockByteStart + (Row * BlockWidthBytes) + i;
                    byte b0 = ForegroundsRawData[ByteIndex];
                    byte b1 = ForegroundsRawData[ByteIndex + BlockBytesPerBitPlane];
                    byte b2 = ForegroundsRawData[ByteIndex + (BlockBytesPerBitPlane * 2)];

                    // Write x,y into bitmap
                    int y = Row + ((BlockIndex / ForegroundsBlocksPerRow) * BlockHeightPixels);
                    for (int Bit = 0; Bit < 8; Bit++)
                    {
                        int b = ((b0 >>> Bit) & 0b1) | (((b1 >>> Bit) & 0b1) << 1) | (((b2 >>> Bit) & 0b1) << 2);
                        int x = (i * 8) + (7 - Bit);

                        int xOffset = (BlockIndex % ForegroundsBlocksPerCol) * BlockWidthPixels;
                        x += xOffset;

                        //Console.WriteLine("Writing x=" + x + " y= " + Row + " Val=" + b);
                        const int PaletteIndexOffset = 8;       // Foregrounds use the higher 8 of the 16 colours in the palette, so add 8 to all the indexes
                        byte Index = (byte)(b + PaletteIndexOffset);
                        IntPtr Pixel = BmpData.Scan0 + (x) + (y * bmp.Width);
                        Marshal.WriteByte(Pixel, Index);
                    }
                }
            }
        }

        bmp.UnlockBits(BmpData);

        Console.WriteLine("Writing: " + FileName);
        bmp.Save(OutputPath + FileName, ImageFormat.Png);
    }

    static void WriteTiledVersionToXml( XmlWriter Xml )
    {
        Xml.WriteAttributeString("version", "1.10");
        Xml.WriteAttributeString("tiledversion", "1.11.2");
    }

    static void WriteDocTypeXml( XmlWriter Xml )
    {
        Xml.WriteDocType("TMX", null, "http://mapeditor.org/dtd/1.0/map.dtd", null);
    }

    void WriteForegroundTileData()
    {
        // Turn #BackgroundTableRawData into a tile data TMX file for use in "Tiled" editor https://www.mapeditor.org/docs
        // https://doc.mapeditor.org/en/stable/reference/tmx-map-format/
        // https://code.tutsplus.com/parsing-and-rendering-tiled-tmx-format-maps-in-your-own-game-engine--gamedev-3104t

        String TileSetFileName = "foregrounds_tileset.xml";
        Console.WriteLine("Writing: " + TileSetFileName);

        using (XmlTextWriter Xml = new XmlTextWriter(Path.Combine(OutputPath, TileSetFileName), Encoding.UTF8))
        {
            Xml.Formatting = Formatting.Indented;
            WriteDocTypeXml(Xml);
            Xml.WriteStartElement("tileset");
            Xml.WriteAttributeString("name", "MenaceForegroundTilesLevel1");
            WriteTiledVersionToXml(Xml);
            Xml.WriteAttributeString("tilewidth", "" + BlockWidthPixels);
            Xml.WriteAttributeString("tileheight", "" + BlockHeightPixels);
            Xml.WriteAttributeString("tilecount", "" + NumBlocks);
            Xml.WriteAttributeString("columns", "1");
            Xml.WriteStartElement("image");
            Xml.WriteAttributeString("source", "foregrounds.png");
            int TileSetWidth = BlockWidthPixels * ForegroundsBlocksPerRow;
            int TilesetHeight = BlockHeightPixels * ForegroundsBlocksPerCol;
            Xml.WriteAttributeString("width", "" + TileSetWidth);
            Xml.WriteAttributeString("height", "" + TilesetHeight);
            Xml.WriteEndElement();  //image
            Xml.WriteEndElement();  //tileset            
            Xml.Flush();
            Xml.Close();
        }
        
        String MapFileName = "foregrounds_level1.tmx";
        Console.WriteLine("Writing: " + MapFileName);

        using (XmlTextWriter Xml = new XmlTextWriter(Path.Combine(OutputPath, MapFileName), Encoding.UTF8))
        {
            const int FirstgidIndex = 1;
            Xml.Formatting = Formatting.Indented;
            WriteDocTypeXml(Xml); 
            Xml.WriteStartElement("map");
            WriteTiledVersionToXml(Xml);
            Xml.WriteAttributeString("orientation", "orthogonal");
            Xml.WriteAttributeString("width", "" + MapBlocksAcross);
            Xml.WriteAttributeString("height", "" + MapBlocksHigh);
            Xml.WriteAttributeString("tilewidth", "" + BlockWidthPixels);
            Xml.WriteAttributeString("tileheight", "" + BlockHeightPixels);

            Xml.WriteStartElement("tileset");
            Xml.WriteAttributeString("firstgid", ""+FirstgidIndex);
            Xml.WriteAttributeString("source", TileSetFileName);
            Xml.WriteEndElement();  //tileset            


            Xml.WriteStartElement("layer");
            Xml.WriteAttributeString("name", "MenaceForegroundLevel1");
            Xml.WriteAttributeString("width", "" + MapBlocksAcross);
            Xml.WriteAttributeString("height", "" + MapBlocksHigh);
            Xml.WriteStartElement("data");

            for (int TiledRow = 0; TiledRow < MapBlocksHigh; TiledRow++)
            {
                for (int TiledCol = 0; TiledCol < MapBlocksAcross; TiledCol++)
                {
                    Xml.WriteStartElement("tile");
                    // The map data is stored sequentally as 12 vertical blocks
                    // But Tiled expects the first full row, followed by the next
                    // So covert the data....
                    int MenaceIndex = (TiledCol * MapBlocksHigh) + TiledRow;
                    int TMXgid = MapRawData[MenaceIndex] + FirstgidIndex;
                    Xml.WriteAttributeString("gid", "" + TMXgid);
                    Xml.WriteEndElement();  //tile
                }
            }
            Xml.WriteEndElement();  //data
            Xml.WriteEndElement();  //layer

            Xml.WriteEndElement();  //map

            Xml.Flush();
            Xml.Close();
        }
    }
};