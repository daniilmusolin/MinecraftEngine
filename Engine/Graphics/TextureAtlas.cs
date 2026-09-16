using OpenTK.Graphics.OpenGL4;

namespace MinecraftEngine.Engine.Graphics;

public class TextureAtlas : IDisposable {
    private readonly int _handle;
    private readonly int _atlasSize;

    public TextureAtlas(int textureSize = 16) {
        _handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _handle);

        var atlasSize = 16;
        _atlasSize = atlasSize;
        var pixelSize = textureSize;
        var totalSize = atlasSize * pixelSize;

        var pixels = new byte[totalSize * totalSize * 4];

        for (var ty = 0; ty < atlasSize; ty++) {
            for (var tx = 0; tx < atlasSize; tx++) {
                var texture = GenerateBlockTexture(tx, ty, pixelSize);

                for (var py = 0; py < pixelSize; py++) {
                    for (var px = 0; px < pixelSize; px++) {
                        var srcIdx = (py * pixelSize + px) * 4;
                        var dstIdx = ((ty * pixelSize + py) * totalSize + (tx * pixelSize + px)) * 4;
                        pixels[dstIdx] = texture[srcIdx];
                        pixels[dstIdx + 1] = texture[srcIdx + 1];
                        pixels[dstIdx + 2] = texture[srcIdx + 2];
                        pixels[dstIdx + 3] = texture[srcIdx + 3];
                    }
                }
            }
        }

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            totalSize,
            totalSize,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            pixels);

        SetupTextureParameters();
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    private byte[] GenerateBlockTexture(int tx, int ty, int size) {
        var pixels = new byte[size * size * 4];

        for (var y = 0; y < size; y++) {
            for (var x = 0; x < size; x++) {
                var idx = (y * size + x) * 4;
                byte r, g, b, a = 255;

                if (tx == 0 && ty == 0) {
                    if ((x + y) % 3 == 0 || (x * 2 + y) % 4 == 0) {
                        r = 60; g = 140; b = 40;
                    } else {
                        r = 80; g = 180; b = 60;
                    }
                } else if (tx == 1 && ty == 0) {
                    if ((x * 3 + y * 5) % 4 == 0) {
                        r = 130; g = 130; b = 130;
                    } else {
                        r = 150; g = 150; b = 150;
                    }
                } else if (tx == 2 && ty == 0) {
                    if ((x + y) % 2 == 0) {
                        r = 140; g = 100; b = 60;
                    } else {
                        r = 120; g = 80; b = 40;
                    }
                } else if (tx == 3 && ty == 0) {
                    if (y >= size - 5) {
                        if ((x + y) % 3 == 0 || (x * 2 + y) % 4 == 0) {
                            r = 60; g = 140; b = 40;
                        } else {
                            r = 80; g = 180; b = 60;
                        }
                    } else {
                        if ((x + y) % 2 == 0) {
                            r = 140; g = 100; b = 60;
                        } else {
                            r = 120; g = 80; b = 40;
                        }
                    }
                
                } else if (tx == 4 && ty == 0) {
                    if ((x + y) % 2 == 0) {
                        r = 180; g = 130; b = 80;
                    } else {
                        r = 160; g = 110; b = 60;
                    }
                } else if (tx == 5 && ty == 0) {
                    if (x % 4 == 0 || x % 4 == 1) {
                        r = 180; g = 130; b = 80;
                    } else {
                        r = 130; g = 90; b = 50;
                    }
                } else if (tx == 6 && ty == 0) {
                    if ((x + y) % 3 == 0) {
                        a = 0;
                        r = 0; g = 0; b = 0;
                    } else {
                        r = 60; g = 160; b = 60;
                        if ((x + y) % 2 == 0) r += 20;
                    }
                } else if (tx == 7 && ty == 0) {
                    if ((x + y) % 3 == 0) {
                        r = 210; g = 190; b = 130;
                    } else {
                        r = 220; g = 200; b = 140;
                    }
                } else if (tx == 0 && ty == 1) {
                    if ((x / 3 + y / 3) % 2 == 0) {
                        r = 190; g = 160; b = 120;
                    } else {
                        r = 170; g = 140; b = 100;
                    }
                } else if (tx == 1 && ty == 1) {
                    if ((x * 2 + y * 3) % 5 == 0) {
                        r = 110; g = 110; b = 120;
                    } else {
                        r = 130; g = 130; b = 140;
                    }
                } else {
                    var colors = GetBlockColors(tx, ty);
                    r = colors.BaseR;
                    g = colors.BaseG;
                    b = colors.BaseB;

                    if (colors.HasNoise) {
                        var noise = (x * 7 + y * 13) % 4;
                        if (noise == 0) {
                            r = (byte)Math.Clamp(r - 20, 0, 255);
                            g = (byte)Math.Clamp(g - 20, 0, 255);
                            b = (byte)Math.Clamp(b - 20, 0, 255);
                        } else if (noise == 3) {
                            r = (byte)Math.Clamp(r + 20, 0, 255);
                            g = (byte)Math.Clamp(g + 20, 0, 255);
                            b = (byte)Math.Clamp(b + 20, 0, 255);
                        }
                    }
                }

                pixels[idx] = r;
                pixels[idx + 1] = g;
                pixels[idx + 2] = b;
                pixels[idx + 3] = a;
            }
        }

        return pixels;
    }

    private (byte BaseR, byte BaseG, byte BaseB, bool HasNoise) GetBlockColors(int tx, int ty) {
        return (tx, ty) switch {
            (0, 0) => (80, 180, 60, true),
            (1, 0) => (150, 150, 150, true),
            (2, 0) => (140, 100, 60, true),
            (3, 0) => (80, 180, 60, true),
            (4, 0) => (180, 130, 80, true),
            (5, 0) => (160, 110, 70, true),
            (6, 0) => (60, 160, 60, false),
            (7, 0) => (220, 200, 140, true),
            (8, 0) => (60, 120, 200, true),
            (9, 0) => (40, 40, 40, true),
            (0, 1) => (190, 160, 120, true),
            (1, 1) => (130, 130, 140, true),
            (2, 1) => (180, 80, 80, true),
            (3, 1) => (200, 230, 255, false),
            (4, 1) => (240, 240, 255, true),
            (5, 1) => (150, 30, 30, true),
            (6, 1) => (210, 200, 170, true),
            (7, 1) => (40, 40, 50, true),
            _ => ((byte)((tx * 31 + ty * 17) % 200 + 55),
                  (byte)((tx * 13 + ty * 29) % 200 + 55),
                  (byte)((tx * 7 + ty * 41) % 200 + 55),
                  true)
        };
    }

    private void SetupTextureParameters() {
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    }

    public void Use(int unit) {
        GL.ActiveTexture(TextureUnit.Texture0 + unit);
        GL.BindTexture(TextureTarget.Texture2D, _handle);
    }

    public (float u1, float v1, float u2, float v2) GetUV(int x, int y) {
        float u1 = x / (float)_atlasSize;
        float v1 = y / (float)_atlasSize;
        float u2 = (x + 1) / (float)_atlasSize;
        float v2 = (y + 1) / (float)_atlasSize;
        return (u1, v1, u2, v2);
    }

    public void Dispose() {
        if (_handle != 0) {
            GL.DeleteTexture(_handle);
        }
    }
}