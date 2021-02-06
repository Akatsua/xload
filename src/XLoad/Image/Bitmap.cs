namespace XLoad.Image
{
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public class Bitmap
    {
        private BitmapDescriptor Descriptor;

        private uint Width;
        private uint Height;
        
        private uint Pad;
        private uint LineSize;
        private uint DataSize;

        private byte[] Data;

        public Bitmap(uint width, uint height)
        {
            this.Width      = width;
            this.Height     = height;

            this.Pad        = (4 - (width % 4)) % 4;
            this.LineSize   = (width * 3) + this.Pad;
            this.DataSize   = this.LineSize * height;
            
            this.Descriptor = new BitmapDescriptor(width, height);
            this.Data       = new byte[DataSize];
        }

        public void SetPixel(int x, int y, Color color)
        {
            this.SetPixel((uint)x, (uint)y, color.R, color.G, color.B);
        }

        private void SetPixel(uint x, uint y, byte r, byte g, byte b)
        {
            uint index = ((this.Height - 1 - y) * this.LineSize) + (x * 3);

            this.Data[index + 0] = b;
            this.Data[index + 1] = g;
            this.Data[index + 2] = r;
        }

        public void Save(string filename)
        {
            using (var filestream   = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
            using (var binarywriter = new BinaryWriter(filestream, Encoding.Default))
            {
                var length          = Marshal.SizeOf(this.Descriptor);
                var unmanagedPtr    = Marshal.AllocHGlobal(length);
                var headers         = new byte[length];

                Marshal.StructureToPtr(this.Descriptor, unmanagedPtr, true);
                Marshal.Copy(unmanagedPtr, headers, 0, length);
                Marshal.FreeHGlobal(unmanagedPtr);

                binarywriter.Write(headers);
                binarywriter.Write(this.Data);
            }
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public class BitmapDescriptor
        {
            // Header
            [FieldOffset(00)] byte  Signature0;
            [FieldOffset(01)] byte  Signature1;
            [FieldOffset(02)] uint  FileSize;
            [FieldOffset(06)] uint  Reserved;
            [FieldOffset(10)] uint  DataOffset;

            // Info Header
            [FieldOffset(14)] uint  InfoHeaderSize;
            [FieldOffset(18)] uint  Width;
            [FieldOffset(22)] uint  Height;
            [FieldOffset(26)] short Planes;
            [FieldOffset(28)] short BitsPerPixel;
            [FieldOffset(30)] uint  Compression;
            [FieldOffset(34)] uint  ImageSize;
            [FieldOffset(38)] uint  XPixelsPerMeter;
            [FieldOffset(42)] uint  YPixelsPerMeter;
            [FieldOffset(46)] uint  ColorsUsed;
            [FieldOffset(50)] uint  ImportantColors;

            // Color Table
            // Unsupported

            // Image Data
            // Outside for easier file writes

            public BitmapDescriptor(uint width, uint height)
            {
                uint pad        = width - (width % 4);
                uint dataSize   = ((width * 3) + pad) * height;

                this.Signature0     = (byte)'B';
                this.Signature1     = (byte)'M';
                this.FileSize       = 54 + dataSize;
                this.Reserved       = 0;
                this.DataOffset     = 54;

                this.InfoHeaderSize     = 40;
                this.Width              = width;
                this.Height             = height;
                this.Planes             = 1;
                this.BitsPerPixel       = 24;
                this.Compression        = 0;
                this.ImageSize          = 0;
                this.XPixelsPerMeter    = 0;
                this.YPixelsPerMeter    = 0;
                this.ColorsUsed         = 0; 
                this.ImportantColors    = 0;
            }
        }
    }
}
