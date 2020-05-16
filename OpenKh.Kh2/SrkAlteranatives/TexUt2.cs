using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TexUt2 {
    enum TFX {
        Modulate = 0, Decal = 1, HL = 2, HL2 = 3,
    }
    enum TCC {
        RGB = 0, RGBA = 1,
    }
    enum WM {
        REPEAT = 0, CLAMP = 1, RClamp = 2, RRepeat = 3,
    }
    class STim {
        public Bitmap pic;
        public TFX tfx = TFX.Modulate;
        public TCC tcc = TCC.RGB;
        public WM wms = WM.REPEAT, wmt = WM.REPEAT;
        /// <summary>
        /// Clamp U Lo ; UMSK
        /// </summary>
        public int minu = 0;
        /// <summary>
        /// clamp U Hi ; UFIX
        /// </summary>
        public int maxu = 0;
        public int minv = 0;
        public int maxv = 0;

        public STim(Bitmap pic) {
            this.pic = pic;
        }

        public int UMSK { get { return minu; } }
        public int VMSK { get { return minv; } }
        public int UFIX { get { return maxu; } }
        public int VFIX { get { return maxv; } }

        public Bitmap Generate() {
            if (wms == WM.RRepeat && wmt == WM.RRepeat) {
                Bitmap p = new Bitmap(UMSK + 1, VMSK + 1);
                using (Graphics cv = Graphics.FromImage(p)) {
                    cv.DrawImage(
                        pic,
                        new Point[] {
                            new Point(0, 0),
                            new Point(p.Width, 0),
                            new Point(0, p.Height),
                        },
                        new Rectangle(UFIX, VFIX, UMSK + 1, VMSK + 1),
                        GraphicsUnit.Pixel
                        );
                }
                return p;
            }
            else if (wms == WM.RClamp && wmt == WM.RClamp) {
                Bitmap p = new Bitmap(pic);

                using (Graphics cv = Graphics.FromImage(p)) {
                    int x0 = 0, y0 = 0;
                    int x1 = minu, y1 = minv;
                    int x2 = maxu, y2 = maxv;
                    int x3 = p.Width, y3 = p.Height;
                    cv.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

#if true
                    //TL
                    cv.FillRectangle(
                        new SolidBrush(p.GetPixel(x1, y1)),
                        Rectangle.FromLTRB(x0, y0, x1, y1)
                        );
                    //TC
                    cv.DrawImage(
                        p,
                        new Point[] {
                            new Point(x1, y0),
                            new Point(x2, y0),
                            new Point(x1, y1),
                        },
                        Rectangle.FromLTRB(x1, y1, x2, y1 + 1),
                        GraphicsUnit.Pixel
                        );
                    //TR
                    cv.FillRectangle(
                        new SolidBrush(p.GetPixel(x2, y1)),
                        Rectangle.FromLTRB(x2, y0, x3, y1)
                        );
                    //ML
                    cv.DrawImage(
                        p,
                        new Point[] {
                            new Point(x0, y1),
                            new Point(x1, y1),
                            new Point(x0, y2),
                        },
                        Rectangle.FromLTRB(x1, y1, x1 + 1, y2),
                        GraphicsUnit.Pixel
                        );
                    //MR
                    cv.DrawImage(
                        p,
                        new Point[] {
                            new Point(x2, y1),
                            new Point(x3, y1),
                            new Point(x2, y2),
                        },
                        Rectangle.FromLTRB(x2 - 1, y1, x2, y2),
                        GraphicsUnit.Pixel
                        );

                    //BL
                    cv.FillRectangle(
                        new SolidBrush(p.GetPixel(x1, y2)),
                        Rectangle.FromLTRB(x0, y2, x1, y3)
                        );
                    //BC
                    cv.DrawImage(
                        p,
                        new Point[] {
                            new Point(x1, y2),
                            new Point(x2, y2),
                            new Point(x1, y3),
                        },
                        Rectangle.FromLTRB(x1, y2 - 1, x2, y2),
                        GraphicsUnit.Pixel
                        );
                    //BR
                    cv.FillRectangle(
                        new SolidBrush(p.GetPixel(x2, y2)),
                        Rectangle.FromLTRB(x2, y2, x3, y3)
                        );
#else
                    cv.FillRectangle(Brushes.Blue, Rectangle.FromLTRB(x0, y0, x1, y1));
                    cv.FillRectangle(Brushes.Green, Rectangle.FromLTRB(x1, y0, x2, y1));
                    cv.FillRectangle(Brushes.Red, Rectangle.FromLTRB(x2, y0, x3, y1));
                    cv.FillRectangle(Brushes.Orange, Rectangle.FromLTRB(x0, y1, x1, y2));
                    cv.FillRectangle(Brushes.Tomato, Rectangle.FromLTRB(x2, y1, x3, y2));
                    cv.FillRectangle(Brushes.Purple, Rectangle.FromLTRB(x0, y2, x1, y3));
                    cv.FillRectangle(Brushes.Cyan, Rectangle.FromLTRB(x1, y2, x2, y3));
                    cv.FillRectangle(Brushes.Yellow, Rectangle.FromLTRB(x2, y2, x3, y3));
#endif

                }

                return p;
            }
            else if (wms != wmt) {
                Debug.Fail(wms + " ≠ " + wmt);
            }
            return pic;
        }
    }

    class TexUt2 {
        public static STim Decode8(byte[] picbin, byte[] palbin, int tbw, int cx, int cy) {
            Bitmap pic = new Bitmap(cx, cy, PixelFormat.Format8bppIndexed);
            tbw /= 2;
            Debug.Assert(tbw != 0, "Invalid");
            byte[] bin = OpenKh.Kh2.Ps2.Decode8(picbin, tbw, Math.Max(1, picbin.Length / 8192 / tbw));
            BitmapData bd = pic.LockBits(Rectangle.FromLTRB(0, 0, pic.Width, pic.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            try {
                int buffSize = bd.Stride * bd.Height;
                Marshal.Copy(bin, 0, bd.Scan0, Math.Min(bin.Length, buffSize));
            }
            finally {
                pic.UnlockBits(bd);
            }
            ColorPalette pals = pic.Palette;
            int psi = 0;

            byte[] palb2 = new byte[1024];
            for (int t = 0; t < 256; t++) {
                int toi = vwBinTex2.KHcv8pal.repl(t);
                Array.Copy(palbin, 4 * t + 0, palb2, 4 * toi + 0, 4);
            }
            Array.Copy(palb2, 0, palbin, 0, 1024);

            for (int pi = 0; pi < 256; pi++) {
                pals.Entries[pi] = CUtil.Gamma(Color.FromArgb(
                    AcUt.GetA(palbin[psi + 4 * pi + 3]) ^ (pi & 1),
                    Math.Min(255, palbin[psi + 4 * pi + 0] + 1),
                    Math.Min(255, palbin[psi + 4 * pi + 1] + 1),
                    Math.Min(255, palbin[psi + 4 * pi + 2] + 1)
                    ), γ);
            }
            pic.Palette = pals;

            return new STim(pic);
        }

        class AcUt {
            public static byte GetA(byte a) {
                return (byte)Math.Min(a * 255 / 0x80, 255);
            }
        }

        public static STim Decode4(byte[] picbin, byte[] palbin, int tbw, int cx, int cy) {
            Bitmap pic = new Bitmap(cx, cy, PixelFormat.Format4bppIndexed);
            tbw /= 2;
            Debug.Assert(tbw != 0, "Invalid");
            byte[] bin = OpenKh.Kh2.Ps2.Decode4(picbin, tbw, Math.Max(1, picbin.Length / 8192 / tbw));
            BitmapData bd = pic.LockBits(Rectangle.FromLTRB(0, 0, pic.Width, pic.Height), ImageLockMode.WriteOnly, PixelFormat.Format4bppIndexed);

            try {
                int buffSize = bd.Stride * bd.Height;
                Marshal.Copy(bin, 0, bd.Scan0, Math.Min(bin.Length, buffSize));
            }
            finally {
                pic.UnlockBits(bd);
            }
            ColorPalette pals = pic.Palette;
            int psi = 0;
            for (int pi = 0; pi < 16; pi++) {
                pals.Entries[pi] = CUtil.Gamma(Color.FromArgb(
                    AcUt.GetA(palbin[psi + 4 * pi + 3]),
                    palbin[psi + 4 * pi + 0],
                    palbin[psi + 4 * pi + 1],
                    palbin[psi + 4 * pi + 2]
                    ), γ);
            }
            pic.Palette = pals;

            return new STim(pic);
        }

        public static STim Decode4Ps(byte[] picbin, byte[] palbin, int tbw, int cx, int cy, int csa) {
            Bitmap pic = new Bitmap(cx, cy, PixelFormat.Format4bppIndexed);
            tbw = Math.Max(1, tbw / 2);
            Debug.Assert(tbw != 0, "Invalid");
            byte[] bin = OpenKh.Kh2.Ps2.Decode4(picbin, tbw, Math.Max(1, picbin.Length / 8192 / tbw));
            BitmapData bd = pic.LockBits(Rectangle.FromLTRB(0, 0, pic.Width, pic.Height), ImageLockMode.WriteOnly, PixelFormat.Format4bppIndexed);

            try {
                int buffSize = bd.Stride * bd.Height;
                Marshal.Copy(bin, 0, bd.Scan0, Math.Min(bin.Length, buffSize));
            }
            finally {
                pic.UnlockBits(bd);
            }
            ColorPalette pals = pic.Palette;
            int psi = 64 * csa;
            byte[] palb2 = new byte[1024];
            for (int t = 0; t < 256; t++) {
                int toi = vwBinTex2.KHcv8pal.repl(t);
                Array.Copy(palbin, 4 * t + 0, palb2, 4 * toi + 0, 4);
            }
            //Array.Copy(palb2, 0, palbin, 0, 1024);
            for (int pi = 0; pi < 16; pi++) {
                pals.Entries[pi] = CUtil.Gamma(Color.FromArgb(
                    AcUt.GetA(palb2[psi + 4 * pi + 3]),
                    palb2[psi + 4 * pi + 0],
                    palb2[psi + 4 * pi + 1],
                    palb2[psi + 4 * pi + 2]
                    ), γ);
            }
            pic.Palette = pals;

            return new STim(pic);
        }

        const float γ = 1f; //0.5f;
    }

    class CUtil {
        public static Color Gamma(Color a, float gamma) {
            return Color.FromArgb(
                a.A,
                Math.Min(255, (int)(Math.Pow(a.R / 255.0, gamma) * 255.0)),
                Math.Min(255, (int)(Math.Pow(a.G / 255.0, gamma) * 255.0)),
                Math.Min(255, (int)(Math.Pow(a.B / 255.0, gamma) * 255.0))
                );
        }
    }

}
