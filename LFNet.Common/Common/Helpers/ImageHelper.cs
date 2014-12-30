using System;
using System.Drawing.Imaging;
using System.IO;
using LFNet.Common.Win32;

namespace LFNet.Common.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Captures the Desktop in a screenshot.
        /// </summary>
        /// <returns>Screenshot of the Desktop.</returns>
        public static global::System.Drawing.Image CaptureDesktop()
        {
            return CaptureWindow(NativeMethods.GetDesktopWindow());
        }

        /// <summary>
        /// Captures the Foreground window in a screenshot.
        /// </summary>
        /// <returns>Screenshot of the current Foreground window.</returns>
        public static global::System.Drawing.Image CaptureForegroundWindow()
        {
            return CaptureWindow(NativeMethods.GetForegroundWindow());
        }

        /// <summary>
        /// Captures a screenshot of the window associated with the handle argument.
        /// </summary>
        /// <param name="handle">Used to determine which window to provide a screenshot for.</param>
        /// <returns>Screenshot of the window corresponding to the handle argument.</returns>
        public static global::System.Drawing.Image CaptureWindow(IntPtr handle)
        {
            IntPtr windowDC = NativeMethods.GetWindowDC(handle);
            IntPtr hDC = NativeMethods.CreateCompatibleDC(windowDC);
            NativeMethods.RECT rect = new NativeMethods.RECT();
            NativeMethods.GetWindowRect(handle, ref rect);
            int nWidth = rect.right - rect.left;
            int nHeight = rect.bottom - rect.top;
            IntPtr hObject = NativeMethods.CreateCompatibleBitmap(windowDC, nWidth, nHeight);
            IntPtr ptr4 = NativeMethods.SelectObject(hDC, hObject);
            NativeMethods.BitBlt(hDC, 0, 0, nWidth, nHeight, windowDC, 0, 0, 0xcc0020);
            NativeMethods.SelectObject(hDC, ptr4);
            NativeMethods.DeleteDC(hDC);
            NativeMethods.ReleaseDC(handle, windowDC);
            global::System.Drawing.Image image = global::System.Drawing.Image.FromHbitmap(hObject);
            NativeMethods.DeleteObject(hObject);
            return image;
        }

        public static byte[] GetBytes(global::System.Drawing.Image image)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, ImageFormat.Jpeg);
            return stream.ToArray();
        }

        /// <summary>
        /// Saves the encoded image to a file
        /// </summary>
        /// <param name="image">The image to save</param>
        /// <param name="quality">The quality desired (a value between 1 and 100).</param>
        /// <param name="format">The <see cref="T:System.Drawing.Imaging.ImageFormat" /> to save the image as.</param>
        /// <param name="fileName">The file path to save the image to.</param>
        public static void ToFile(global::System.Drawing.Image image, long quality, ImageFormat format, string fileName)
        {
            using (Stream stream = ToStream(image, quality, format))
            {
                using (FileStream stream2 = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    stream.Position = 0L;
                    byte[] buffer = new byte[stream.Length];
                    int count = stream.Read(buffer, 0, (int) stream.Length);
                    stream2.Write(buffer, 0, count);
                    stream2.Flush();
                }
            }
        }

        /// <summary>
        /// Saves the encoded image to a stream.
        /// </summary>
        /// <param name="image">The image to save</param>
        /// <param name="quality">The quality desired (a value between 1 and 100).</param>
        /// <param name="format">The <see cref="T:System.Drawing.Imaging.ImageFormat" /> to save the image as.</param>
        /// <returns></returns>
        public static Stream ToStream(global::System.Drawing.Image image, long quality, ImageFormat format)
        {
            MemoryStream stream = new MemoryStream();
            ImageCodecInfo encoder = null;
            foreach (ImageCodecInfo info2 in ImageCodecInfo.GetImageEncoders())
            {
                if (info2.MimeType == "image/jpeg")
                {
                    encoder = info2;
                }
            }
            EncoderParameter parameter = new EncoderParameter(Encoder.Quality, quality);
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = parameter;
            image.Save(stream, encoder, encoderParams);
            return stream;
        }
    }
}

