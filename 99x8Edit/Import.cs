﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Linq;

namespace _99x8Edit
{
    // Importing data
    public class Import
    {
        // Import types
        internal static string PCGTypeFilter = "MSX BASIC(*.bin)|*.bin|"
                                             + "PNG File(*.png)|*.png";
        internal static string SpriteTypeFilter = "MSX BASIC pattern data(*.bin)|*.bin|"
                                                + "MSX BASIC color data(*.bin)|*.bin|" 
                                                + "Raw pattern data(*.raw)|*.raw|"
                                                + "Raw color data(*.raw)|*.raw";
        enum PCGType
        {
            MSXBASIC = 0,
            PNG
        };
        enum SpriteType
        {
            MSXBASIC = 0,
            MSXBASIC_Color,
            RawPattern,
            RawColor
        }
        // Palette
        private Color[] palette = new Color[16];         // Windows color corresponding to color code

        //------------------------------------------------------------------------
        // Initialize
        public Import()
        {
        }
        //------------------------------------------------------------------------
        // Properties
        internal Color[] Palette
        {
            get { return palette; }
            set { palette = value.Clone() as Color[]; }
        }
        //------------------------------------------------------------------------
        // Methods
        internal void ImportPCG(string filename, int type, byte[] out_gen, byte[] out_clr)
        {
            switch((PCGType)type)
            {
                case PCGType.MSXBASIC:
                    this.PNGtoPCG(filename, out_gen, out_clr);
                    break;
                case PCGType.PNG:
                    this.BINtoPCG(filename, out_gen, out_clr);
                    break;
            }
        }
        internal void ImportSprite(string filename, int type, byte[] out_gen, byte[] out_clr)
        {
            switch ((SpriteType)type)
            {
                case SpriteType.MSXBASIC:
                    this.BINtoSprite(filename, out_gen, out_clr);
                    break;
                case SpriteType.MSXBASIC_Color:
                    this.BINtoSpriteColor(filename, out_gen, out_clr);
                    break;
                case SpriteType.RawPattern:
                    this.RawToSpriteGen(filename, out_gen, out_clr);
                    break;
                case SpriteType.RawColor:
                    this.RawToSpriteColor(filename, out_gen, out_clr);
                    break;
            }
        }
        //------------------------------------------------------------------------
        // Utility
        private void PNGtoPCG(string filename, byte[] out_gen, byte[] out_clr)
        {
            Bitmap bmp = (Bitmap)Image.FromFile(filename);
            if(bmp == null)
            {
                return;
            }
            int w = Math.Min(bmp.Width, 256);
            int h = Math.Min(bmp.Height, 64);
            for(int col = 0; col < w / 8; ++col)
            {
                for(int row = 0; row < h / 8; ++row)
                {
                    // Each line in each characters
                    for(int line = 0; line < 8; ++line)
                    {
                        int[] nearest = new int[8]; // Pixel of each line to color code
                        int[] count = new int[16];  // How mane times each color appeared
                        for(int x = 0; x < 8; ++x)
                        {
                            Color c = bmp.GetPixel(col * 8 + x, row * 8 + line);
                            nearest[x] = this.NearestColorCodeOf(c);
                            count[nearest[x]]++;
                        }
                        // Get the frequent color
                        int max1_num = count.Max();
                        int max1_index = Array.IndexOf(count, max1_num);
                        count[max1_index] = 0;
                        int max2_num = count.Max();
                        int max2_index = Array.IndexOf(count, max2_num);
                        // Color table of the line will be made of two frequent colors
                        out_clr[row * 256 + col * 8 + line] = (byte)((max1_index << 4) | max2_index);
                        // Make pattern generator table
                        for(int x = 0; x < 8; ++x)
                        {
                            Color c = bmp.GetPixel(col * 8 + x, row * 8 + line);
                            int code = this.WhichColorCodeIsNear(c, max1_index, max2_index);
                            if(code == max1_index)
                            {
                                // Foreground color is near, so set the corresponding bit
                                out_gen[row * 256 + col * 8 + line] |= (byte)(1 << (7 - x));
                            }
                            // Ignore nackground color since the corresponding value is 0
                        }
                    }
                }
            }
        }
        private void BINtoPCG(string filename, byte[] out_gen, byte[] out_clr)
        {
            BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open));
            try
            {
                // Read BSAVE header
                byte header = br.ReadByte();
                if(header != 0xFE)
                {
                    throw new Exception("No BSAVE header");
                }
                UInt16 bin_start_addr = br.ReadUInt16();
                _ = br.ReadUInt16();
                _ = br.ReadUInt16();
                // Read data
                int gen_seek_addr = 0;
                if(this.SeekBIN(0x0000, bin_start_addr, br, out gen_seek_addr))
                {
                    for (int ptr = 0; (ptr < 0x0800) && (gen_seek_addr + ptr < br.BaseStream.Length); ++ptr)
                    {
                        out_gen[ptr] = br.ReadByte();
                    }
                }
                int color_addr = 0;
                if(this.SeekBIN(0x2000, bin_start_addr, br, out color_addr))
                {
                    for (int ptr = 0; (ptr < 0x0800) && (color_addr + ptr < br.BaseStream.Length); ++ptr)
                    {
                        out_clr[ptr] = br.ReadByte();
                    }
                }
            }
            finally
            {
                br.Close();
            }
        }
        private void BINtoSprite(string filename, byte[] out_gen, byte[] out_clr)
        {
            BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open));
            try
            {
                // Read BSAVE header
                byte header = br.ReadByte();
                if (header != 0xFE)
                {
                    throw new Exception("No BSAVE header");
                }
                UInt16 bin_start_addr = br.ReadUInt16();
                _ = br.ReadUInt16();
                _ = br.ReadUInt16();
                // Read data
                int gen_seek_addr = 0;
                if (this.SeekBIN(0x3800, bin_start_addr, br, out gen_seek_addr))
                {
                    for (int ptr = 0; (ptr < 0x0800) && (gen_seek_addr + ptr < br.BaseStream.Length); ++ptr)
                    {
                        out_gen[ptr] = br.ReadByte();
                    }
                }
            }
            finally
            {
                br.Close();
            }
        }
        private void BINtoSpriteColor(string filename, byte[] out_gen, byte[] out_clr)
        {
            BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open));
            try
            {
                // Read BSAVE header
                byte header = br.ReadByte();
                if (header != 0xFE)
                {
                    throw new Exception("No BSAVE header");
                }
                _ = br.ReadUInt16();
                _ = br.ReadUInt16();
                _ = br.ReadUInt16();
                // Read data
                int gen_seek_addr = 0;
                // Won't see start address since there's no specified address
                if (this.SeekBIN(0, 0, br, out gen_seek_addr))
                {
                    for (int ptr = 0; (ptr < 0x0400) && (gen_seek_addr + ptr < br.BaseStream.Length); ++ptr)
                    {
                        // Internal data has colors for each 8x8 sprites
                        int index16x16 = ptr / 16;
                        out_clr[index16x16 * 32 + (ptr % 16)]
                            = out_clr[index16x16 * 32 + 16 + (ptr % 16)]
                            = br.ReadByte();
                    }
                }
            }
            finally
            {
                br.Close();
            }
        }
        private void RawToSpriteGen(string filename, byte[] out_gen, byte[] out_clr)
        {
            BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open));
            try
            {
                for(int i = 0; (i < 0x0800) && (i < br.BaseStream.Length); ++i)
                {
                    out_gen[i] = br.ReadByte();
                }
            }
            finally
            {
                br.Close();
            }
        }
        private void RawToSpriteColor(string filename, byte[] out_gen, byte[] out_clr)
        {
            BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open));
            try
            {
                for (int i = 0; (i < 0x0400) && (i < br.BaseStream.Length); ++i)
                {
                    // Internal data has colors for each 8x8 sprites
                    int index16x16 = i / 16;
                    out_clr[index16x16 * 32 + (i % 16)]
                        = out_clr[index16x16 * 32 + 16 + (i % 16)]
                        = br.ReadByte();
                }
            }
            finally
            {
                br.Close();
            }
        }
        private bool SeekBIN(int vram_addr, int bin_start_addr, BinaryReader br, out int seek_addr)
        {
            seek_addr = vram_addr - bin_start_addr;
            seek_addr += 7;        // BSAVE header size
            if ((seek_addr < 0) || (seek_addr >= br.BaseStream.Length))
            {
                seek_addr = -1;
                return false;
            }
            br.BaseStream.Seek(seek_addr, SeekOrigin.Begin);
            return true;
        }
        private int NearestColorCodeOf(Color c)
        {
            int[] distance = new int[16];
            for(int i = 0; i < 16; ++i)
            {
                int d = Math.Abs(c.R - palette[i].R)
                      + Math.Abs(c.G - palette[i].G)
                      + Math.Abs(c.B - palette[i].B);
                distance[i] = d;
            }
            return Array.IndexOf(distance, distance.Min());
        }
        private int WhichColorCodeIsNear(Color c, int code1, int code2)
        {
            int d1 = Math.Abs(c.R - palette[code1].R)
                   + Math.Abs(c.G - palette[code1].G)
                   + Math.Abs(c.B - palette[code1].B);
            int d2 = Math.Abs(c.R - palette[code2].R)
                   + Math.Abs(c.G - palette[code2].G)
                   + Math.Abs(c.B - palette[code2].B);
            if(d1 < d2)
            {
                return code1;
            }
            return code2;
        }
    }
}
