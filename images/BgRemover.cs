using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class BgRemover {
    public static void Process(string inputPath, string outputPath, int threshold) {
        using (Bitmap srcBmp = new Bitmap(inputPath)) {
            int width = srcBmp.Width;
            int height = srcBmp.Height;

            Bitmap destBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData srcData = srcBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData destData = destBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int totalBytes = srcData.Stride * height;
            byte[] pixelBuffer = new byte[totalBytes];
            Marshal.Copy(srcData.Scan0, pixelBuffer, 0, totalBytes);

            byte[] outBuffer = new byte[totalBytes];
            Array.Copy(pixelBuffer, outBuffer, totalBytes);

            bool[,] visited = new bool[width, height];
            Queue<Point> queue = new Queue<Point>();

            Func<int, int, bool> isBg = (x, y) => {
                int idx = y * srcData.Stride + x * 4;
                byte b = pixelBuffer[idx];
                byte g = pixelBuffer[idx + 1];
                byte r = pixelBuffer[idx + 2];
                byte a = pixelBuffer[idx + 3];
                if (a < 20) return true;
                return (r >= threshold && g >= threshold && b >= threshold);
            };

            // Seed borders
            for (int x = 0; x < width; x++) {
                if (isBg(x, 0)) { queue.Enqueue(new Point(x, 0)); visited[x, 0] = true; }
                if (isBg(x, height - 1)) { queue.Enqueue(new Point(x, height - 1)); visited[x, height - 1] = true; }
            }
            for (int y = 0; y < height; y++) {
                if (!visited[0, y] && isBg(0, y)) { queue.Enqueue(new Point(0, y)); visited[0, y] = true; }
                if (!visited[width - 1, y] && isBg(width - 1, y)) { queue.Enqueue(new Point(width - 1, y)); visited[width - 1, y] = true; }
            }

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (queue.Count > 0) {
                Point p = queue.Dequeue();
                int idx = p.Y * srcData.Stride + p.X * 4;
                outBuffer[idx + 3] = 0; // Alpha = 0

                for (int d = 0; d < 4; d++) {
                    int nx = p.X + dx[d];
                    int ny = p.Y + dy[d];
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny]) {
                        visited[nx, ny] = true;
                        if (isBg(nx, ny)) {
                            queue.Enqueue(new Point(nx, ny));
                        }
                    }
                }
            }

            // Antialiased border feathering
            for (int y = 1; y < height - 1; y++) {
                for (int x = 1; x < width - 1; x++) {
                    if (!visited[x, y]) {
                        bool border = false;
                        for (int d = 0; d < 4; d++) {
                            int nx = x + dx[d];
                            int ny = y + dy[d];
                            if (visited[nx, ny]) {
                                border = true;
                                break;
                            }
                        }
                        if (border) {
                            int idx = y * srcData.Stride + x * 4;
                            byte b = pixelBuffer[idx];
                            byte g = pixelBuffer[idx + 1];
                            byte r = pixelBuffer[idx + 2];
                            int minVal = Math.Min(r, Math.Min(g, b));
                            int maxVal = Math.Max(r, Math.Max(g, b));
                            if (minVal > 215) {
                                float t = (255f - minVal) / (255f - 215f);
                                outBuffer[idx + 3] = (byte)(Math.Max(20f, Math.Min(255f, t * 255f)));
                            }
                        }
                    }
                }
            }

            Marshal.Copy(outBuffer, 0, destData.Scan0, totalBytes);
            srcBmp.UnlockBits(srcData);
            destBmp.UnlockBits(destData);

            destBmp.Save(outputPath, ImageFormat.Png);
            destBmp.Dispose();
        }
    }
}
