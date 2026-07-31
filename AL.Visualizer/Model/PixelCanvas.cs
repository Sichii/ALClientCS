#region
using System.Runtime.InteropServices;
using SkiaSharp;
#endregion

namespace AL.Visualizer.Model;

/// <summary>
///     A writable pixel buffer that can encode itself as a PNG.
/// </summary>
/// <remarks>
///     Skia is only involved at the save boundary. Pixels are written straight into a managed array because
///     <see cref="SKBitmap.SetPixel" /> costs a native call apiece - roughly 50x slower than an array write over a
///     full-size navmesh.
/// </remarks>
public sealed class PixelCanvas
{
    private readonly uint[] Pixels;

    /// <summary>
    ///     The height of the canvas, in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    ///     The width of the canvas, in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    ///     Gets or sets the color of the pixel at the given coordinates.
    /// </summary>
    /// <param name="x">
    ///     The column of the pixel.
    /// </param>
    /// <param name="y">
    ///     The row of the pixel.
    /// </param>
    public SKColor this[int x, int y]
    {
        //an out-of-range x would silently wrap onto the next row, so it is checked here; y falls out of the array bounds on its own
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(x);

            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Width);

            return Pixels[(y * Width) + x];
        }

        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(x);

            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Width);

            Pixels[(y * Width) + x] = (uint)value;
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PixelCanvas" /> class.
    /// </summary>
    /// <param name="width">
    ///     The width of the canvas, in pixels.
    /// </param>
    /// <param name="height">
    ///     The height of the canvas, in pixels.
    /// </param>
    /// <param name="background">
    ///     The color to fill the canvas with.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     width
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     height
    /// </exception>
    public PixelCanvas(int width, int height, SKColor background)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Width = width;
        Height = height;
        Pixels = new uint[width * height];

        Array.Fill(Pixels, (uint)background);
    }

    /// <summary>
    ///     Encodes the canvas as a PNG and writes it to the given path.
    /// </summary>
    /// <param name="path">
    ///     The path to write the PNG to.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to cancel the write with.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     path
    /// </exception>
    public async Task SaveAsPngAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        //Bgra8888 in memory is little-endian 0xAARRGGBB, which is exactly how SKColor packs into a uint
        var info = new SKImageInfo(Width, Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);

        using var image = SKImage.FromPixelCopy(info, MemoryMarshal.AsBytes(Pixels));
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        await File.WriteAllBytesAsync(path, data.ToArray(), cancellationToken);
    }
}