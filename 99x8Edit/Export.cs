﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace _99x8Edit
{
    // Exporting data
    public class Export
    {
        // Supported Export types
        internal static string PCGTypeFilter = "C Header(*.h)|*.h"
                                             + "|C Header compressed(*.h)|*.h"
                                             + "|Assembly Source(*.asm)|*.asm"
                                             + "|Assembly Source compressed(*.asm)|*.asm"
                                             + "|MSX BSAVE format, pattern data, for VRAM(*.bin)|*.bin"
                                             + "|MSX BSAVE format, color data, for VRAM(*.bin)|*.bin"
                                             + "|Raw Binary, pattern data(*.raw)|*.raw"
                                             + "|Raw Binary, color data(*.raw)|*.raw"
                                             + "|Raw Binary compressed, pattern data(*.raw)|*.raw"
                                             + "|Raw Binary compressed, color data(*.raw)|*.raw";
        internal static string SpriteTypeFilter = "C Header(*.h)|*.h"
                                             + "|C Header compressed(*.h)|*.h"
                                             + "|Assembly Source(*.asm)|*.asm"
                                             + "|Assembly Source compressed(*.asm)|*.asm"
                                             + "|MSX BSAVE format, pattern data, for VRAM(*.bin)|*.bin"
                                             + "|MSX BSAVE format, color data, for RAM(*.bin)|*.bin"
                                             + "|Raw Binary, pattern data(*.raw)|*.raw"
                                             + "|Raw Binary, color data(*.raw)|*.raw"
                                             + "|Raw Binary compressed, pattern data(*.raw)|*.raw"
                                             + "|Raw Binary compressed, color data(*.raw)|*.raw";
        internal static string MapTypeFilter = "C Header(*.h)|*.h"
                                             + "|C Header compressed(*.h)|*.h"
                                             + "|Assembly Source(*.asm)|*.asm"
                                             + "|Assembly Source compressed(*.asm)|*.asm"
                                             + "|MSX BSAVE format, pattern data, for RAM(*.bin)|*.bin"
                                             + "|MSX BSAVE format, map data, for RAM(*.bin)|*.bin"
                                             + "|Raw Binary, pattern data(*.raw)|*.raw"
                                             + "|Raw Binary, map data(*.raw)|*.raw"
                                             + "|Raw Binary compressed, pattern data(*.raw)|*.raw"
                                             + "|Raw Binary compressed, map data(*.raw)|*.raw";
        internal enum PCGType
        {
            CHeader = 0,
            CCompressed,
            ASMData,
            ASMCompressed,
            MSXBASIC_Pattern,
            MSXBASIC_Color,
            Raw_Pattern,
            Raw_Color,
            RawCompressed_Pattern,
            RawCompressed_Color,
        };
        internal enum SpriteType
        {
            CHeader = 0,
            CCompressed,
            ASMData,
            ASMCompressed,
            MSXBASIC_Pattern,
            MSXBASIC_Color,
            Raw_Pattern,
            Raw_Color,
            RawCompressed_Pattern,
            RawCompressed_Color,
        };
        internal enum MapType
        {
            CHeader = 0,
            CCompressed,
            ASMData,
            ASMCompressed,
            MSXBASIC_Pattern,
            MSXBASIC_Map,
            Raw_Pattern,
            Raw_Map,
            RawCompressed_Pattern,
            RawCompressed_Map,
        };
        // Data to be exported, to be set by host
        private byte[] ptnGen = new byte[256 * 8];    // Pattern generator table
        private byte[] ptnClr = new byte[256 * 8];    // Pattern color table
        private byte[] nameTable = new byte[768];     // Sandbox(Pattern name table)
        private byte[] pltDat = { 0x00, 0x00, 0x00, 0x00, 0x11, 0x06, 0x33, 0x07,
                                  0x17, 0x01, 0x27, 0x03, 0x51, 0x01, 0x27, 0x06,
                                  0x71, 0x01, 0x73, 0x03, 0x61, 0x06, 0x64, 0x06,
                                  0x11, 0x04, 0x65, 0x02, 0x55, 0x05, 0x77, 0x07};  // Palette [RB][xG][RB][xG][RB]...
        private bool isTMS9918 = false;
        private byte[] mapPattern = new byte[256 * 4];  // One pattern mede by four characters
        private byte[,] mapData = new byte[64, 64];     // Map data[x, y](0..255)
        private Int32 mapWidth = 64;
        private Int32 mapHeight = 64;
        private byte[] spriteGen = new byte[256 * 8];   // Sprite pattern generator table
        private byte[] spriteClr = new byte[64 * 16];   // Sprite color(mode2)
        //------------------------------------------------------------------------
        // Initialize
        public Export(byte[] pcggen,
                      byte[] pcgclr,
                      byte[] sandbox,
                      byte[] plt,
                      bool is9918,
                      byte[] mapptn,
                      byte[,] mapdat,
                      Int32 mapw,
                      Int32 maph,
                      byte[] sprgen,
                      byte[] sprclr)
        {
            ptnGen = pcggen.Clone() as byte[];
            ptnClr = pcgclr.Clone() as byte[];
            nameTable = sandbox.Clone() as byte[];
            pltDat = plt.Clone() as byte[];
            isTMS9918 = is9918;
            mapPattern = mapptn.Clone() as byte[];
            mapData = mapdat.Clone() as byte[,];
            mapWidth = mapw;
            mapHeight = maph;
            spriteGen = sprgen.Clone() as byte[];
            spriteClr = sprclr.Clone() as byte[];
        }
        //------------------------------------------------------------------------
        // Methods
        internal void ExportPCG(PCGType type, String path)
        {
            if (type == PCGType.CHeader || type == PCGType.CCompressed)
            {
                using StreamWriter sr = new StreamWriter(path, false);
                sr.WriteLine("#ifndef __PCGDAT_H__");
                sr.WriteLine("#define __PCGDAT_H__");
                String str = "";
                if (!isTMS9918)
                {
                    sr.WriteLine("// Palette");
                    sr.WriteLine("const unsigned char palette[] = {");
                    str = ArrayToCHeaderString(pltDat, false);
                    sr.WriteLine(str);
                    sr.WriteLine("};");
                }
                sr.WriteLine("// Character pattern generator table");
                if (type == 0)
                {
                    sr.WriteLine("const unsigned char ptngen[] = {");
                    str = ArrayToCHeaderString(ptnGen, false);
                }
                else
                {
                    sr.WriteLine("const unsigned char ptngen_compressed[] = {");
                    str = ArrayToCHeaderString(ptnGen, true);
                }
                sr.WriteLine(str);
                sr.WriteLine("};");
                sr.WriteLine("// Character pattern color table");
                if (type == PCGType.CHeader)
                {
                    sr.WriteLine("const unsigned char ptnclr[] = {");
                    str = ArrayToCHeaderString(ptnClr, false);
                }
                else
                {
                    sr.WriteLine("const unsigned char ptnclr_compressed[] = {");
                    str = ArrayToCHeaderString(ptnClr, true);
                }
                sr.WriteLine(str);
                sr.WriteLine("};");
                sr.WriteLine("// Name table");
                if (type == PCGType.CHeader)
                {
                    sr.WriteLine("const unsigned char nametable[] = {");
                    str = ArrayToCHeaderString(nameTable, false);
                }
                else
                {
                    sr.WriteLine("const unsigned char nametable_compressed[] = {");
                    str = ArrayToCHeaderString(nameTable, true);
                }
                sr.WriteLine(str);
                sr.WriteLine("};");
                sr.WriteLine("#endif");
            }
            else if (type == PCGType.ASMData || type == PCGType.ASMCompressed)
            {
                using StreamWriter sr = new StreamWriter(path, false);
                sr.WriteLine("; PCG Data");
                sr.WriteLine("; this export data is not tested");
                String str = "";
                if (!isTMS9918)
                {
                    sr.WriteLine("; Palette r8b8g8");
                    sr.WriteLine("palette:");
                    str = ArrayToASMString(pltDat, false);
                    sr.WriteLine(str);
                }
                sr.WriteLine("; Character pattern generator table");
                if (type == PCGType.ASMData)
                {
                    sr.WriteLine("ptngen:");
                    str = ArrayToASMString(ptnGen, false);
                }
                else
                {
                    sr.WriteLine("ptngen_compressed:");
                    str = ArrayToASMString(ptnGen, true);
                }
                sr.WriteLine(str);
                sr.WriteLine("; Character pattern color table");
                if (type == PCGType.ASMData)
                {
                    sr.WriteLine("prnclr:");
                    str = ArrayToASMString(ptnClr, false);
                }
                else
                {
                    sr.WriteLine("prnclr_compressed:");
                    str = ArrayToASMString(ptnClr, true);
                }
                sr.WriteLine(str);
                sr.WriteLine("; Name table");
                if (type == PCGType.ASMData)
                {
                    sr.WriteLine("namtbl:");
                    str = ArrayToASMString(nameTable, false);
                }
                else
                {
                    sr.WriteLine("namtbl_compressed:");
                    str = ArrayToASMString(nameTable, true);
                }
                sr.WriteLine(str);
            }
            else if (type == PCGType.MSXBASIC_Pattern)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    br.Write((byte)0xFE);       // BSAVE/BLOAD header
                    br.Write((byte)0);          // Start address is 0
                    br.Write((byte)0);
                    br.Write((byte)0xFF);       // End address is 0x07FF
                    br.Write((byte)0x07);
                    br.Write((byte)0);          // Execution address
                    br.Write((byte)0);
                    br.Write(ptnGen);
                }
            }
            else if (type == PCGType.MSXBASIC_Color)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    br.Write((byte)0xFE);       // BSAVE/BLOAD header
                    br.Write((byte)0x00);       // Start address is 0x2000
                    br.Write((byte)0x20);
                    br.Write((byte)0xFF);       // End address is 0x27FF
                    br.Write((byte)0x27);
                    br.Write((byte)0);          // Execution address
                    br.Write((byte)0);
                    br.Write(ptnClr);
                }
            }
            else if (type == PCGType.Raw_Pattern)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    br.Write(ptnGen);
                }
            }
            else if (type == PCGType.Raw_Color)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    br.Write(ptnClr);
                }
            }
            else if (type == PCGType.RawCompressed_Pattern)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    byte[] comp = Compression.Create(Compression.Type.BytePair).Encode(ptnGen);
                    br.Write(comp);
                }
            }
            else if (type == PCGType.RawCompressed_Color)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    byte[] comp = Compression.Create(Compression.Type.BytePair).Encode(ptnClr);
                    br.Write(comp);
                }
            }
        }
        internal void ExportMap(MapType type, String path)
        {
            if (type == MapType.CHeader || type == MapType.CCompressed)
            {
                using StreamWriter sr = new StreamWriter(path, false);
                sr.WriteLine("#ifndef __MAPDAT_H__");
                sr.WriteLine("#define __MAPDAT_H__");
                sr.WriteLine($"#define MAP_W   ({mapWidth})");
                sr.WriteLine($"#define MAP_H   ({mapHeight})");
                String str = "\t";
                sr.WriteLine("// Map patterns");
                if (type == MapType.CHeader)
                {
                    sr.WriteLine("const unsigned char mapptn[] = {");
                    str = ArrayToCHeaderString(mapPattern, false);
                }
                else
                {
                    sr.WriteLine("const unsigned char mapptn_compressed[] = {");
                    str = ArrayToCHeaderString(mapPattern, true);
                }
                sr.WriteLine(str);
                sr.WriteLine("};");
                str = "\t";
                sr.WriteLine("// Map data");
                if (type == MapType.CHeader)
                {
                    sr.WriteLine("const unsigned char mapData[] = {");
                    for (int i = 0; i < mapWidth * mapHeight; ++i)
                    {
                        if ((i != 0) && (i % 16) == 0) str += "\r\n\t";
                        str += $"{mapData[i % mapWidth, i / mapWidth]}, ";
                    }
                }
                else
                {
                    sr.WriteLine("const unsigned char mapData_compressed[] = {");
                    byte[] comp = this.CompressMapData();
                    for (int i = 0; i < comp.Length; ++i)
                    {
                        if ((i != 0) && (i % 16) == 0) str += "\r\n\t";
                        str += $"{comp[i]}, ";
                    }
                }
                sr.WriteLine(str);
                sr.WriteLine("};");
                sr.WriteLine("#endif");
            }
            else if (type == MapType.ASMData || type == MapType.ASMCompressed)
            {
                using StreamWriter sr = new StreamWriter(path, false);
                sr.WriteLine("; Map Data");
                sr.WriteLine("; this export data is not tested");
                String str = "";
                sr.WriteLine("mapwidth:");
                sr.WriteLine($"\tdb\t{mapWidth}");
                sr.WriteLine("mapheight:");
                sr.WriteLine($"\tdb\t{mapHeight}");
                sr.WriteLine("; Map patterns");
                if (type == MapType.ASMData)
                {
                    sr.WriteLine("mapptn:");
                    str = ArrayToASMString(mapPattern, false);
                }
                else
                {
                    sr.WriteLine("mapptn_compressed:");
                    str = ArrayToASMString(mapPattern, true);
                }
                sr.WriteLine(str);
                sr.WriteLine("; Map data");
                str = "";
                if (type == MapType.ASMData)
                {
                    sr.WriteLine("mapdat:");
                    for (int i = 0; i < mapWidth * mapHeight; ++i)
                    {
                        if (i % 16 == 0) str += "\tdb\t";
                        str += $"{mapData[i % mapWidth, i / mapWidth]}";
                        if (i % 16 == 15) str += "\r\n";
                        else str += ", ";
                    }
                }
                else
                {
                    sr.WriteLine("mapdat_compressed:");
                    byte[] comp = this.CompressMapData();
                    for (int i = 0; i < comp.Length; ++i)
                    {
                        if (i % 16 == 0) str += "\tdb\t";
                        str += $"{comp[i]}";
                        if (i % 16 == 15) str += "\r\n";
                        else str += ", ";
                    }
                }
                sr.WriteLine(str);
            }
            else if (type == MapType.MSXBASIC_Pattern)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    int pattern_size = 4 * 256;
                    br.Write((byte)0xFE);                   // BSAVE/BLOAD header
                    br.Write((byte)0x00);                   // Start address is 0x0000
                    br.Write((byte)0x00);
                    br.Write((byte)(pattern_size & 0xFF));  // End address depends on size
                    br.Write((byte)(pattern_size >> 8));
                    br.Write((byte)0);                      // Execution address
                    br.Write((byte)0);
                    br.Write(mapPattern);
                }
            }
            else if (type == MapType.MSXBASIC_Map)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    int map_size = mapWidth * mapHeight;
                    br.Write((byte)0xFE);                   // BSAVE/BLOAD header
                    br.Write((byte)0x00);                   // Start address is 0x0000
                    br.Write((byte)0x00);
                    br.Write((byte)(map_size & 0xFF));      // End address depends on size
                    br.Write((byte)(map_size >> 8));
                    br.Write((byte)0);                      // Execution address
                    br.Write((byte)0);
                    for (int i = 0; i < mapHeight; ++i)
                    {
                        for (int j = 0; j < mapWidth; ++j)
                        {
                            br.Write(mapData[j, i]);
                        }
                    }
                }
            }
            else if (type == MapType.Raw_Pattern)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    br.Write(mapPattern);
                }
            }
            else if (type == MapType.Raw_Map)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    for (int i = 0; i < mapHeight; ++i)
                    {
                        for (int j = 0; j < mapWidth; ++j)
                        {
                            br.Write(mapData[j, i]);
                        }
                    }
                }
            }
            else if (type == MapType.RawCompressed_Pattern)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    byte[] comp = Compression.Create(Compression.Type.BytePair).Encode(mapPattern);
                    br.Write(comp);
                }
            }
            else if (type == MapType.RawCompressed_Map)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    br.Write(this.CompressMapData());
                }
            }
        }
        internal void ExportSprites(SpriteType type, String path)
        {
            // Start exporting
            if (type == SpriteType.CHeader || type == SpriteType.CCompressed)
            {
                using StreamWriter sr = new StreamWriter(path, false);
                sr.WriteLine("#ifndef __SPRITEDAT_H__");
                sr.WriteLine("#define __SPRITEDAT_H__");
                String str = "";
                sr.WriteLine("// Sprite generator table");
                if (type == SpriteType.CHeader)
                {
                    sr.WriteLine("const unsigned char sprptn[] = {");
                    str = ArrayToCHeaderString(spriteGen, false);
                }
                else
                {
                    sr.WriteLine("const unsigned char sprptn_compressed[] = {");
                    str = ArrayToCHeaderString(spriteGen, true);
                }
                sr.WriteLine(str);
                sr.WriteLine("};");
                if (!isTMS9918)
                {
                    sr.WriteLine("// Sprite color table");
                    if (type == SpriteType.CHeader)
                    {
                        sr.WriteLine("const unsigned char sprclr[] = {");
                        str = ArrayToCHeaderString(spriteClr, false);
                    }
                    else
                    {
                        sr.WriteLine("const unsigned char sprclr_compressed[] = {");
                        str = ArrayToCHeaderString(spriteClr, true);
                    }
                    sr.WriteLine(str);
                    sr.WriteLine("};");
                }
                sr.WriteLine("#endif");
            }
            else if (type == SpriteType.ASMData || type == SpriteType.ASMCompressed)
            {
                using StreamWriter sr = new StreamWriter(path, false);
                sr.WriteLine("; Sprite Data");
                sr.WriteLine("; this export data is not tested");
                String str = "";
                sr.WriteLine("; Sprite generator table");
                str = "";
                if (type == SpriteType.ASMData)
                {
                    sr.WriteLine("sprgen:");
                    str = ArrayToASMString(spriteGen, false);
                }
                else
                {
                    sr.WriteLine("sprgen_compressed:");
                    str = ArrayToASMString(spriteGen, true);
                }
                sr.WriteLine(str);
                if (!isTMS9918)
                {
                    sr.WriteLine("; Sprite color table");
                    str = "";
                    if (type == SpriteType.ASMData)
                    {
                        sr.WriteLine("sprclr:");
                        str = ArrayToASMString(spriteClr, false);
                    }
                    else
                    {
                        sr.WriteLine("sprclr_compressed:");
                        str = ArrayToASMString(spriteClr, true);
                    }
                    sr.WriteLine(str);
                }
            }
            else if (type == SpriteType.MSXBASIC_Pattern)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    br.Write((byte)0xFE);       // BSAVE/BLOAD header
                    br.Write((byte)0x00);       // Start address is 0x3800
                    br.Write((byte)0x38);
                    br.Write((byte)0xFF);       // End address is 0x3FFF
                    br.Write((byte)0x3F);
                    br.Write((byte)0);          // Execution address
                    br.Write((byte)0);
                    br.Write(spriteGen);
                }
            }
            else if (type == SpriteType.MSXBASIC_Color)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    // Color table can't be registered to VRAM, so it would be loaded on RAM
                    br.Write((byte)0xFE);       // BSAVE/BLOAD header
                    br.Write((byte)0x00);       // Start address is 0x0000
                    br.Write((byte)0x00);
                    br.Write((byte)0x04);       // End address is 0x0400
                    br.Write((byte)0x00);
                    br.Write((byte)0);          // Execution address
                    br.Write((byte)0);
                    br.Write(spriteClr);
                }
            }
            else if (type == SpriteType.Raw_Pattern)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    br.Write(spriteGen);
                }
            }
            else if (type == SpriteType.Raw_Color)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    br.Write(spriteClr);
                }
            }
            else if (type == SpriteType.RawCompressed_Pattern)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    byte[] comp = Compression.Create(Compression.Type.BytePair).Encode(spriteGen);
                    br.Write(comp);
                }
            }
            else if (type == SpriteType.RawCompressed_Color)
            {
                using (BinaryWriter br = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    byte[] comp = Compression.Create(Compression.Type.BytePair).Encode(spriteClr);
                    br.Write(comp);
                }
            }
        }
        //------------------------------------------------------------------------
        // Utilities
        private String ArrayToCHeaderString(byte[] src, bool compress)
        {
            String ret = "\t";
            if (compress)
            {
                byte[] comp = Compression.Create(Compression.Type.BytePair).Encode(src);
                for (int i = 0; i < comp.Length; ++i)
                {
                    if ((i != 0) && (i % 16) == 0) ret += "\r\n\t";
                    ret += $"{comp[i]}, ";
                }
            }
            else
            {
                for (int i = 0; i < src.Length; ++i)
                {
                    if ((i != 0) && (i % 16) == 0) ret += "\r\n\t";
                    ret += $"{src[i]}, ";
                }
            }
            return ret;
        }
        private String ArrayToASMString(byte[] src, bool compress)
        {
            String ret = "";
            if (compress)
            {
                byte[] comp = Compression.Create(Compression.Type.BytePair).Encode(src);
                for (int i = 0; i < comp.Length; ++i)
                {
                    if (i % 16 == 0) ret += $"\tdb\t";
                    ret += $"{comp[i]}";
                    if (i % 16 == 15) ret += "\r\n";
                    else ret += ", ";
                }
            }
            else
            {
                for (int i = 0; i < src.Length; ++i)
                {
                    if (i % 16 == 0) ret += "\tdb\t";
                    ret += $"{src[i]}";
                    if (i % 16 == 15) ret += "\r\n";
                    else ret += ", ";
                }
            }
            return ret;
        }
        private byte[] CompressMapData()
        {
            // Map will be compressed by run length encode, to be decoded realtime
            List<byte[]> comp_data = new List<byte[]>();
            // Head of data will be [offset to the data of row] * [number of rows]
            ushort[] offset_to_row = new ushort[mapHeight];
            ushort offset = (ushort)(mapHeight * 2);
            // Create data
            for(int y = 0; y < mapHeight; ++y)
            {
                // Offset to the row data
                offset_to_row[y] = offset;
                // Compress each row
                List<byte> src_row = new List<byte>();
                for(int i = 0; i < mapWidth; ++i)
                {
                    src_row.Add(mapData[i, y]);
                }
                CompressionBase encoder = Compression.Create(Compression.Type.RunLength);
                byte[] comp = encoder.Encode(src_row.ToArray() as byte[]);
                comp_data.Add(comp);
                offset += (ushort)comp.Length;
            }
            // Make one compressed data
            List<byte> ret = new List<byte>();
            for(int i = 0; i < mapHeight; ++i)
            {
                // Offset is stored as 2 byte little endian data
                ret.Add((byte)(offset_to_row[i] & 0xFF));
                ret.Add((byte)((offset_to_row[i]) >> 8 & 0xFF));
            }
            for (int i = 0; i < mapHeight; ++i)
            {
                for(int j = 0; j < comp_data[i].Length; ++j)
                {
                    ret.Add(comp_data[i][j]);
                }
            }
            return ret.ToArray() as byte[];
        }
    }
}
