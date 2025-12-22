// Based of NGif's library

using System;
using System.Drawing;
using System.IO;

namespace OpenKh.Tools.LayoutEditor.Helpers
{
    public class AnimatedGifEncoder : IDisposable
    {
        private int width;
        private int height;
        private Color transparent = Color.Empty;
        private int transIndex;
        private int repeat = -1;
        private int delay = 0;
        private bool started = false;
        private Stream fs;
        private Image image;
        private byte[] pixels;
        private byte[] indexedPixels;
        private int colorDepth;
        private byte[] colorTab;
        private bool[] usedEntry = new bool[256];
        private int palSize = 7;
        private int dispose = -1;
        private bool closeStream = false;
        private bool firstFrame = true;
        private bool sizeSet = false;
        private int sample = 10;
        private bool disposed = false;

        public void SetDelay(int ms)
        {
            delay = (int)Math.Round(ms / 10.0f);
        }

        public void SetDispose(int code)
        {
            if (code >= 0)
                dispose = code;
        }

        public void SetRepeat(int iter)
        {
            if (iter >= 0)
                repeat = iter;
        }

        public void SetTransparent(Color c)
        {
            transparent = c;
        }

        public bool AddFrame(Image im)
        {
            if ((im == null) || !started)
                return false;

            bool ok = true;
            try
            {
                if (!sizeSet)
                {
                    width = im.Width;
                    height = im.Height;
                    if (width < 1)
                        width = 320;
                    if (height < 1)
                        height = 240;
                    sizeSet = true;
                }

                image = im;
                GetImagePixels();
                AnalyzePixels();
                if (firstFrame)
                {
                    WriteLSD();
                    WritePalette();
                    if (repeat >= 0)
                        WriteNetscapeExt();
                }
                WriteGraphicCtrlExt();
                WriteImageDesc();
                if (!firstFrame)
                    WritePalette();
                WritePixels();
                firstFrame = false;
            }
            catch
            {
                ok = false;
            }

            return ok;
        }

        public bool Finish()
        {
            if (!started)
                return false;
            bool ok = true;
            started = false;

            try
            {
                fs.WriteByte(0x3b);
                fs.Flush();
                if (closeStream)
                    fs.Close();
            }
            catch
            {
                ok = false;
            }

            return ok;
        }

        public void SetFrameRate(float fps)
        {
            if (fps != 0f)
                delay = (int)Math.Round(100f / fps);
        }

        public void SetQuality(int quality)
        {
            if (quality < 1)
                quality = 1;
            sample = quality;
        }

        public void SetSize(int w, int h)
        {
            if (started && !firstFrame)
                return;
            width = w;
            height = h;
            if (width < 1)
                width = 320;
            if (height < 1)
                height = 240;
            sizeSet = true;
        }

        public bool Start(Stream os)
        {
            if (os == null)
                return false;
            bool ok = true;
            closeStream = false;
            fs = os;
            try
            {
                WriteString("GIF89a");
            }
            catch
            {
                ok = false;
            }
            return started = ok;
        }

        public bool Start(string file)
        {
            bool ok = true;
            try
            {
                fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                ok = Start(fs);
                closeStream = true;
            }
            catch
            {
                ok = false;
            }
            return started = ok;
        }

        private void AnalyzePixels()
        {
            int len = pixels.Length;
            int nPix = len / 3;
            indexedPixels = new byte[nPix];

            bool[] isTransparentPixel = new bool[nPix];
            byte[] originalPixels = new byte[len];
            Array.Copy(pixels, originalPixels, len);

            if (transparent != Color.Empty)
            {
                int k = 0;
                for (int i = 0; i < nPix; i++)
                {
                    int b = pixels[k];
                    int g = pixels[k + 1];
                    int r = pixels[k + 2];

                    if (r < 10 && g > 245 && b < 10)
                    {
                        isTransparentPixel[i] = true;
                        pixels[k] = 0;
                        pixels[k + 1] = 0;
                        pixels[k + 2] = 0;
                    }
                    k += 3;
                }
            }

            NeuQuant nq = new NeuQuant(pixels, len, sample);
            colorTab = nq.Process();

            for (int i = 0; i < colorTab.Length; i += 3)
            {
                byte temp = colorTab[i];
                colorTab[i] = colorTab[i + 2];
                colorTab[i + 2] = temp;
                usedEntry[i / 3] = false;
            }

            Array.Copy(originalPixels, pixels, len);

            int k2 = 0;
            for (int i = 0; i < nPix; i++)
            {
                if (!isTransparentPixel[i])
                {
                    int index = nq.Map(pixels[k2] & 0xff, pixels[k2 + 1] & 0xff, pixels[k2 + 2] & 0xff);
                    usedEntry[index] = true;
                    indexedPixels[i] = (byte)index;
                }
                k2 += 3;
            }

            pixels = null;
            colorDepth = 8;
            palSize = 7;

            if (transparent != Color.Empty)
            {
                transIndex = 0;

                colorTab[0] = transparent.B;
                colorTab[1] = transparent.G;
                colorTab[2] = transparent.R;

                for (int i = 0; i < nPix; i++)
                {
                    if (isTransparentPixel[i])
                    {
                        indexedPixels[i] = 0;
                    }
                }

                usedEntry[0] = true;
            }
        }

        private void GetImagePixels()
        {
            int w = image.Width;
            int h = image.Height;
            if ((w != width) || (h != height))
            {
                Image temp = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(temp);
                g.DrawImage(image, 0, 0);
                image = temp;
                g.Dispose();
            }

            Bitmap bitmap = new Bitmap(image);
            int[] buf = new int[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    buf[y * w + x] = bitmap.GetPixel(x, y).ToArgb();
                }
            }

            pixels = new byte[buf.Length * 3];
            int count = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                pixels[count++] = (byte)(buf[i] & 0xff);
                pixels[count++] = (byte)((buf[i] >> 8) & 0xff);
                pixels[count++] = (byte)((buf[i] >> 16) & 0xff);
            }
        }

        private void WriteGraphicCtrlExt()
        {
            fs.WriteByte(0x21);
            fs.WriteByte(0xf9);
            fs.WriteByte(4);
            int transp, disp;
            if (transparent == Color.Empty)
            {
                transp = 0;
                disp = 0;
            }
            else
            {
                transp = 1;
                disp = 2;
            }
            if (dispose >= 0)
                disp = dispose & 7;
            disp <<= 2;

            fs.WriteByte(Convert.ToByte(0 | disp | 0 | transp));
            WriteShort(delay);
            fs.WriteByte(Convert.ToByte(transIndex));
            fs.WriteByte(0);
        }

        private void WriteImageDesc()
        {
            fs.WriteByte(0x2c);
            WriteShort(0);
            WriteShort(0);
            WriteShort(width);
            WriteShort(height);
            if (firstFrame)
                fs.WriteByte(0);
            else
                fs.WriteByte(Convert.ToByte(0x80 | 0 | 0 | 0 | palSize));
        }

        private void WriteLSD()
        {
            WriteShort(width);
            WriteShort(height);
            fs.WriteByte(Convert.ToByte(0x80 | 0x70 | 0x00 | palSize));
            fs.WriteByte(0);
            fs.WriteByte(0);
        }

        private void WriteNetscapeExt()
        {
            fs.WriteByte(0x21);
            fs.WriteByte(0xff);
            fs.WriteByte(11);
            WriteString("NETSCAPE2.0");
            fs.WriteByte(3);
            fs.WriteByte(1);
            WriteShort(repeat);
            fs.WriteByte(0);
        }

        private void WritePalette()
        {
            fs.Write(colorTab, 0, colorTab.Length);
            int n = (3 * 256) - colorTab.Length;
            for (int i = 0; i < n; i++)
                fs.WriteByte(0);
        }

        private void WritePixels()
        {
            LZWEncoder encoder = new LZWEncoder(width, height, indexedPixels, colorDepth);
            encoder.Encode(fs);
        }

        private void WriteShort(int value)
        {
            fs.WriteByte(Convert.ToByte(value & 0xff));
            fs.WriteByte(Convert.ToByte((value >> 8) & 0xff));
        }

        private void WriteString(String s)
        {
            char[] chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                fs.WriteByte((byte)chars[i]);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (fs != null && closeStream)
                    {
                        fs.Dispose();
                    }
                    if (image != null)
                    {
                        image.Dispose();
                    }
                }
                disposed = true;
            }
        }
    }

    public class LZWEncoder
    {
        private static readonly int EOF = -1;
        private int imgW, imgH;
        private byte[] pixAry;
        private int initCodeSize;
        private int remaining;
        private int curPixel;

        private static readonly int BITS = 12;
        private static readonly int HSIZE = 5003;

        int n_bits;
        int maxbits = BITS;
        int maxcode;
        int maxmaxcode = 1 << BITS;

        int[] htab = new int[HSIZE];
        int[] codetab = new int[HSIZE];

        int hsize = HSIZE;
        int free_ent = 0;
        bool clear_flg = false;

        int g_init_bits;
        int ClearCode;
        int EOFCode;

        int cur_accum = 0;
        int cur_bits = 0;

        int[] masks = {
            0x0000, 0x0001, 0x0003, 0x0007, 0x000F,
            0x001F, 0x003F, 0x007F, 0x00FF,
            0x01FF, 0x03FF, 0x07FF, 0x0FFF,
            0x1FFF, 0x3FFF, 0x7FFF, 0xFFFF
        };

        int a_count;
        byte[] accum = new byte[256];

        public LZWEncoder(int width, int height, byte[] pixels, int color_depth)
        {
            imgW = width;
            imgH = height;
            pixAry = pixels;
            initCodeSize = Math.Max(2, color_depth);
        }

        void Add(byte c, Stream outs)
        {
            accum[a_count++] = c;
            if (a_count >= 254)
                Flush(outs);
        }

        void ClearTable(Stream outs)
        {
            ResetCodeTable(hsize);
            free_ent = ClearCode + 2;
            clear_flg = true;
            Output(ClearCode, outs);
        }

        void ResetCodeTable(int hsize)
        {
            for (int i = 0; i < hsize; ++i)
                htab[i] = -1;
        }

        void Compress(int init_bits, Stream outs)
        {
            int fcode;
            int i;
            int c;
            int ent;
            int disp;
            int hsize_reg;
            int hshift;

            g_init_bits = init_bits;

            clear_flg = false;
            n_bits = g_init_bits;
            maxcode = MaxCode(n_bits);

            ClearCode = 1 << (init_bits - 1);
            EOFCode = ClearCode + 1;
            free_ent = ClearCode + 2;

            a_count = 0;

            ent = NextPixel();

            hshift = 0;
            for (fcode = hsize; fcode < 65536; fcode *= 2)
                ++hshift;
            hshift = 8 - hshift;

            hsize_reg = hsize;
            ResetCodeTable(hsize_reg);

            Output(ClearCode, outs);

            while ((c = NextPixel()) != EOF)
            {
                fcode = (c << maxbits) + ent;
                i = (c << hshift) ^ ent;

                if (htab[i] == fcode)
                {
                    ent = codetab[i];
                    continue;
                }
                else if (htab[i] >= 0)
                {
                    disp = hsize_reg - i;
                    if (i == 0)
                        disp = 1;
                    do
                    {
                        if ((i -= disp) < 0)
                            i += hsize_reg;

                        if (htab[i] == fcode)
                        {
                            ent = codetab[i];
                            goto outer_loop;
                        }
                    } while (htab[i] >= 0);
                }
                Output(ent, outs);
                ent = c;
                if (free_ent < maxmaxcode)
                {
                    codetab[i] = free_ent++;
                    htab[i] = fcode;
                }
                else
                    ClearTable(outs);
                outer_loop:
                ;
            }

            Output(ent, outs);
            Output(EOFCode, outs);
        }

        public void Encode(Stream os)
        {
            os.WriteByte(Convert.ToByte(initCodeSize));
            remaining = imgW * imgH;
            curPixel = 0;
            Compress(initCodeSize + 1, os);
            os.WriteByte(0);
        }

        void Flush(Stream outs)
        {
            if (a_count > 0)
            {
                outs.WriteByte(Convert.ToByte(a_count));
                outs.Write(accum, 0, a_count);
                a_count = 0;
            }
        }

        int MaxCode(int n_bits)
        {
            return (1 << n_bits) - 1;
        }

        private int NextPixel()
        {
            if (remaining == 0)
                return EOF;
            --remaining;
            byte pix = pixAry[curPixel++];
            return pix & 0xff;
        }

        void Output(int code, Stream outs)
        {
            cur_accum &= masks[cur_bits];

            if (cur_bits > 0)
                cur_accum |= (code << cur_bits);
            else
                cur_accum = code;

            cur_bits += n_bits;

            while (cur_bits >= 8)
            {
                Add((byte)(cur_accum & 0xff), outs);
                cur_accum >>= 8;
                cur_bits -= 8;
            }

            if (free_ent > maxcode || clear_flg)
            {
                if (clear_flg)
                {
                    maxcode = MaxCode(n_bits = g_init_bits);
                    clear_flg = false;
                }
                else
                {
                    ++n_bits;
                    if (n_bits == maxbits)
                        maxcode = maxmaxcode;
                    else
                        maxcode = MaxCode(n_bits);
                }
            }

            if (code == EOFCode)
            {
                while (cur_bits > 0)
                {
                    Add((byte)(cur_accum & 0xff), outs);
                    cur_accum >>= 8;
                    cur_bits -= 8;
                }
                Flush(outs);
            }
        }
    }

    public class NeuQuant
    {
        protected static readonly int netsize = 256;
        protected static readonly int prime1 = 499;
        protected static readonly int prime2 = 491;
        protected static readonly int prime3 = 487;
        protected static readonly int prime4 = 503;
        protected static readonly int minpicturebytes = (3 * prime4);
        protected static readonly int maxnetpos = (netsize - 1);
        protected static readonly int netbiasshift = 4;
        protected static readonly int ncycles = 100;
        protected static readonly int intbiasshift = 16;
        protected static readonly int intbias = (((int)1) << intbiasshift);
        protected static readonly int gammashift = 10;
        protected static readonly int gamma = (((int)1) << gammashift);
        protected static readonly int betashift = 10;
        protected static readonly int beta = (intbias >> betashift);
        protected static readonly int betagamma = (intbias << (gammashift - betashift));
        protected static readonly int initrad = (netsize >> 3);
        protected static readonly int radiusbiasshift = 6;
        protected static readonly int radiusbias = (((int)1) << radiusbiasshift);
        protected static readonly int initradius = (initrad * radiusbias);
        protected static readonly int radiusdec = 30;
        protected static readonly int alphabiasshift = 10;
        protected static readonly int initalpha = (((int)1) << alphabiasshift);
        protected int alphadec;
        protected static readonly int radbiasshift = 8;
        protected static readonly int radbias = (((int)1) << radbiasshift);
        protected static readonly int alpharadbshift = (alphabiasshift + radbiasshift);
        protected static readonly int alpharadbias = (((int)1) << alpharadbshift);

        protected byte[] thepicture;
        protected int lengthcount;
        protected int samplefac;
        protected int[][] network;
        protected int[] netindex = new int[256];
        protected int[] bias = new int[netsize];
        protected int[] freq = new int[netsize];
        protected int[] radpower = new int[initrad];

        public NeuQuant(byte[] thepic, int len, int sample)
        {
            int i;
            int[] p;

            thepicture = thepic;
            lengthcount = len;
            samplefac = sample;

            network = new int[netsize][];
            for (i = 0; i < netsize; i++)
            {
                network[i] = new int[4];
                p = network[i];
                p[0] = p[1] = p[2] = (i << (netbiasshift + 8)) / netsize;
                freq[i] = intbias / netsize;
                bias[i] = 0;
            }
        }

        public byte[] ColorMap()
        {
            byte[] map = new byte[3 * netsize];
            int[] index = new int[netsize];
            for (int i = 0; i < netsize; i++)
                index[network[i][3]] = i;
            int k = 0;
            for (int i = 0; i < netsize; i++)
            {
                int j = index[i];
                map[k++] = (byte)(network[j][0]);
                map[k++] = (byte)(network[j][1]);
                map[k++] = (byte)(network[j][2]);
            }
            return map;
        }

        public void Inxbuild()
        {
            int i, j, smallpos, smallval;
            int[] p;
            int[] q;
            int previouscol, startpos;

            previouscol = 0;
            startpos = 0;
            for (i = 0; i < netsize; i++)
            {
                p = network[i];
                smallpos = i;
                smallval = p[1];

                for (j = i + 1; j < netsize; j++)
                {
                    q = network[j];
                    if (q[1] < smallval)
                    {
                        smallpos = j;
                        smallval = q[1];
                    }
                }
                q = network[smallpos];

                if (i != smallpos)
                {
                    j = q[0];
                    q[0] = p[0];
                    p[0] = j;
                    j = q[1];
                    q[1] = p[1];
                    p[1] = j;
                    j = q[2];
                    q[2] = p[2];
                    p[2] = j;
                    j = q[3];
                    q[3] = p[3];
                    p[3] = j;
                }

                if (smallval != previouscol)
                {
                    netindex[previouscol] = (startpos + i) >> 1;
                    for (j = previouscol + 1; j < smallval; j++)
                        netindex[j] = i;
                    previouscol = smallval;
                    startpos = i;
                }
            }
            netindex[previouscol] = (startpos + maxnetpos) >> 1;
            for (j = previouscol + 1; j < 256; j++)
                netindex[j] = maxnetpos;
        }

        public void Learn()
        {
            int i, j, b, g, r;
            int radius, rad, alpha, step, delta, samplepixels;
            byte[] p;
            int pix, lim;

            if (lengthcount < minpicturebytes)
                samplefac = 1;
            alphadec = 30 + ((samplefac - 1) / 3);
            p = thepicture;
            pix = 0;
            lim = lengthcount;
            samplepixels = lengthcount / (3 * samplefac);
            delta = samplepixels / ncycles;
            alpha = initalpha;
            radius = initradius;

            rad = radius >> radiusbiasshift;
            if (rad <= 1)
                rad = 0;
            for (i = 0; i < rad; i++)
                radpower[i] = alpha * (((rad * rad - i * i) * radbias) / (rad * rad));

            if (lengthcount < minpicturebytes)
                step = 3;
            else if ((lengthcount % prime1) != 0)
                step = 3 * prime1;
            else
            {
                if ((lengthcount % prime2) != 0)
                    step = 3 * prime2;
                else
                {
                    if ((lengthcount % prime3) != 0)
                        step = 3 * prime3;
                    else
                        step = 3 * prime4;
                }
            }

            i = 0;
            while (i < samplepixels)
            {
                b = (p[pix + 0] & 0xff) << netbiasshift;
                g = (p[pix + 1] & 0xff) << netbiasshift;
                r = (p[pix + 2] & 0xff) << netbiasshift;
                j = Contest(b, g, r);

                Altersingle(alpha, j, b, g, r);
                if (rad != 0)
                    Alterneigh(rad, j, b, g, r);

                pix += step;
                if (pix >= lim)
                    pix -= lengthcount;

                i++;
                if (delta == 0)
                    delta = 1;
                if (i % delta == 0)
                {
                    alpha -= alpha / alphadec;
                    radius -= radius / radiusdec;
                    rad = radius >> radiusbiasshift;
                    if (rad <= 1)
                        rad = 0;
                    for (j = 0; j < rad; j++)
                        radpower[j] = alpha * (((rad * rad - j * j) * radbias) / (rad * rad));
                }
            }
        }

        public int Map(int b, int g, int r)
        {
            int i, j, dist, a, bestd;
            int[] p;
            int best;

            bestd = 1000;
            best = -1;
            i = netindex[g];
            j = i - 1;

            while ((i < netsize) || (j >= 0))
            {
                if (i < netsize)
                {
                    p = network[i];
                    dist = p[1] - g;
                    if (dist >= bestd)
                        i = netsize;
                    else
                    {
                        i++;
                        if (dist < 0)
                            dist = -dist;
                        a = p[0] - b;
                        if (a < 0)
                            a = -a;
                        dist += a;
                        if (dist < bestd)
                        {
                            a = p[2] - r;
                            if (a < 0)
                                a = -a;
                            dist += a;
                            if (dist < bestd)
                            {
                                bestd = dist;
                                best = p[3];
                            }
                        }
                    }
                }
                if (j >= 0)
                {
                    p = network[j];
                    dist = g - p[1];
                    if (dist >= bestd)
                        j = -1;
                    else
                    {
                        j--;
                        if (dist < 0)
                            dist = -dist;
                        a = p[0] - b;
                        if (a < 0)
                            a = -a;
                        dist += a;
                        if (dist < bestd)
                        {
                            a = p[2] - r;
                            if (a < 0)
                                a = -a;
                            dist += a;
                            if (dist < bestd)
                            {
                                bestd = dist;
                                best = p[3];
                            }
                        }
                    }
                }
            }
            return (best);
        }

        public byte[] Process()
        {
            Learn();
            Unbiasnet();
            Inxbuild();
            return ColorMap();
        }

        public void Unbiasnet()
        {
            int i, j;

            for (i = 0; i < netsize; i++)
            {
                network[i][0] >>= netbiasshift;
                network[i][1] >>= netbiasshift;
                network[i][2] >>= netbiasshift;
                network[i][3] = i;
            }
        }

        protected void Alterneigh(int rad, int i, int b, int g, int r)
        {
            int j, k, lo, hi, a, m;
            int[] p;

            lo = i - rad;
            if (lo < -1)
                lo = -1;
            hi = i + rad;
            if (hi > netsize)
                hi = netsize;

            j = i + 1;
            k = i - 1;
            m = 1;
            while ((j < hi) || (k > lo))
            {
                a = radpower[m++];
                if (j < hi)
                {
                    p = network[j++];
                    try
                    {
                        p[0] -= (a * (p[0] - b)) / alpharadbias;
                        p[1] -= (a * (p[1] - g)) / alpharadbias;
                        p[2] -= (a * (p[2] - r)) / alpharadbias;
                    }
                    catch { }
                }
                if (k > lo)
                {
                    p = network[k--];
                    try
                    {
                        p[0] -= (a * (p[0] - b)) / alpharadbias;
                        p[1] -= (a * (p[1] - g)) / alpharadbias;
                        p[2] -= (a * (p[2] - r)) / alpharadbias;
                    }
                    catch { }
                }
            }
        }

        protected void Altersingle(int alpha, int i, int b, int g, int r)
        {
            int[] n = network[i];
            n[0] -= (alpha * (n[0] - b)) / initalpha;
            n[1] -= (alpha * (n[1] - g)) / initalpha;
            n[2] -= (alpha * (n[2] - r)) / initalpha;
        }

        protected int Contest(int b, int g, int r)
        {
            int i, dist, a, biasdist, betafreq;
            int bestpos, bestbiaspos, bestd, bestbiasd;
            int[] n;

            bestd = ~(((int)1) << 31);
            bestbiasd = bestd;
            bestpos = -1;
            bestbiaspos = bestpos;

            for (i = 0; i < netsize; i++)
            {
                n = network[i];
                dist = n[0] - b;
                if (dist < 0)
                    dist = -dist;
                a = n[1] - g;
                if (a < 0)
                    a = -a;
                dist += a;
                a = n[2] - r;
                if (a < 0)
                    a = -a;
                dist += a;
                if (dist < bestd)
                {
                    bestd = dist;
                    bestpos = i;
                }
                biasdist = dist - ((bias[i]) >> (intbiasshift - netbiasshift));
                if (biasdist < bestbiasd)
                {
                    bestbiasd = biasdist;
                    bestbiaspos = i;
                }
                betafreq = (freq[i] >> betashift);
                freq[i] -= betafreq;
                bias[i] += (betafreq << gammashift);
            }
            freq[bestpos] += beta;
            bias[bestpos] -= betagamma;
            return (bestbiaspos);
        }
    }
}
