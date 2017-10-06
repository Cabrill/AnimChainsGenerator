using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimChainsGenerator
{
    /// <summary>OMG IT'S UNSAFE !!</summary>
    internal sealed class BitmapManipulation
    {
        internal static void SetTransparentColor(Bitmap bitmap, Color color)
        {
            if (bitmap == null)
                throw new Exception("BitmapManipulation.SetTransparentColor(): Bitmap is null.");

            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb && bitmap.PixelFormat != PixelFormat.Format32bppPArgb)
                throw new Exception("BitmapManipulation.SetTransparentColor(): Image pixel format not supported. Only 32 bit RGB-A files are supported.");

            try
            {
                BitmapData lockedBitmap = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
                        ImageLockMode.ReadWrite, 
                        bitmap.PixelFormat
                    );

                #region img conversion & write
                unsafe
                {
                    ///////////////////////////////
                    int pixelByleSize = 4; // 32 bits
                    byte* scan0 = (byte*)lockedBitmap.Scan0;

                    // traverse y
                    for (int y = 0; y < lockedBitmap.Height; y++)
                    {
                        // get mem adress of start of row
                        byte* scan = scan0 + (y * lockedBitmap.Stride);

                        // traverse x
                        for (int x = 0; x < lockedBitmap.Width; x++)
                        {
                            int pixelIndex = x * pixelByleSize;

                            /*
                            scan[pixelIndex]        // R
                            scan[pixelIndex + 1]    // G
                            scan[pixelIndex + 2]    // B
                            scan[pixelIndex + 3]    // A
                            */

                            // if alpha is full transarent
                            if (scan[pixelIndex + 3] == 0)
                            {
                                // write requested color
                                scan[pixelIndex] = color.R;
                                scan[pixelIndex + 1] = color.G;
                                scan[pixelIndex + 2] = color.B;
                                //scan[pixelIndex + 3] = 0;
                            }
                        }
                    }
                    ////////////////////////////
                }
                #endregion

                bitmap.UnlockBits(lockedBitmap);
            }
            catch (Exception ex)
            {
                throw new Exception("BitmapManipulation.SetTransparentColor() caused exception when writing to bitmap.", ex);
            }
        }

        internal static void Blit(Bitmap targetBitmap, Bitmap sourceBitmap)
        {
            if (targetBitmap == null)
                throw new Exception("BitmapManipulation.SetTransparentColor(): target Bitmap is null.");

            if (targetBitmap.PixelFormat != PixelFormat.Format32bppArgb && targetBitmap.PixelFormat != PixelFormat.Format32bppPArgb)
                throw new Exception("BitmapManipulation.SetTransparentColor(): target Bitmap image pixel format not supported. Only 32 bit RGB-A files are supported.");

            if (sourceBitmap == null)
                throw new Exception("BitmapManipulation.SetTransparentColor(): source Bitmap is null.");

            if (sourceBitmap.PixelFormat != PixelFormat.Format32bppArgb && sourceBitmap.PixelFormat != PixelFormat.Format32bppPArgb)
                throw new Exception("BitmapManipulation.SetTransparentColor(): source Bitmap image pixel format not supported. Only 32 bit RGB-A files are supported.");

            try
            {
                // Locked target bitmap
                BitmapData targetBitmapL = targetBitmap.LockBits(
                        new Rectangle(0, 0, targetBitmap.Width, targetBitmap.Height), 
                        ImageLockMode.WriteOnly, 
                        targetBitmap.PixelFormat
                    );

                // Locked source bitmap
                BitmapData sourceBitmapL = sourceBitmap.LockBits(
                        new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), 
                        ImageLockMode.ReadOnly, 
                        sourceBitmap.PixelFormat
                    );

                #region img conversion & write
                unsafe
                {
                    ///////////////////////////////
                    int pixelByteSize = 4; // 32 bits

                    byte* targetBmpScan0 = (byte*)targetBitmapL.Scan0;

                    byte* sourceBmpScan0 = (byte*)sourceBitmapL.Scan0;

                    // traverse y
                    for (int y = 0; y < targetBitmapL.Height; y++)
                    {
                        // get mem adress of start of row
                        byte* scan = targetBmpScan0 + (y * targetBitmapL.Stride);

                        // traverse x
                        for (int x = 0; x < targetBitmapL.Width; x++)
                        {
                            int pixelIndex = x * pixelByteSize;

                            /*
                            scan[pixelIndex]        // R
                            scan[pixelIndex + 1]    // G
                            scan[pixelIndex + 2]    // B
                            scan[pixelIndex + 3]    // A
                            */

                            // if alpha is full transarent
                            if (scan[pixelIndex + 3] == 0)
                            {
                                // write requested color
                                scan[pixelIndex] = color.R;
                                scan[pixelIndex + 1] = color.G;
                                scan[pixelIndex + 2] = color.B;
                                //scan[pixelIndex + 3] = 0;
                            }
                        }
                    }
                    ////////////////////////////
                }
                #endregion

                targetBitmap.UnlockBits(targetBitmapL);
            }
            catch (Exception ex)
            {
                throw new Exception("BitmapManipulation.SetTransparentColor() caused exception when writing to bitmap.", ex);
            }
        }
    }

    
}
