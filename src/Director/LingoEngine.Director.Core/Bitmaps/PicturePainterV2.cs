using System;
using System.Collections.Generic;
using AbstUI.Primitives;

namespace LingoEngine.Director.Core.Bitmaps;

public class PicturePainterV2
{
    private class Tile
    {
        public readonly byte[] Pixels;
        public readonly int Size;
        public readonly IAbstTexture2D Texture;
        public bool Dirty;

        public Tile(int size, Func<int, int, IAbstTexture2D> createTexture)
        {
            Size = size;
            Pixels = new byte[size * size * 4];
            Texture = createTexture(size, size);
            Dirty = true;
        }

        public void SetPixel(int x, int y, AColor color)
        {
            int idx = (y * Size + x) * 4;
            Pixels[idx] = color.A;
            Pixels[idx + 1] = color.R;
            Pixels[idx + 2] = color.G;
            Pixels[idx + 3] = color.B;
            Dirty = true;
        }

        public void RenderToTexture()
        {
            Texture.SetARGBPixels(Pixels);
            Dirty = false;
        }
    }

    private Tile[,] _tiles;
    private readonly int _tileSize;
    private readonly Func<int, int, IAbstTexture2D> _createTexture;
    private APoint _offset;

    public APoint Offset => _offset;
    public int Width => _tiles.GetLength(0) * _tileSize;
    public int Height => _tiles.GetLength(1) * _tileSize;
    public int TileSize => _tileSize;

    public PicturePainterV2(int width, int height, Func<int, int, IAbstTexture2D> createTexture, int tileSize = 128)
    {
        _tileSize = tileSize;
        _createTexture = createTexture;
        int tilesX = (width + tileSize - 1) / tileSize;
        int tilesY = (height + tileSize - 1) / tileSize;
        _tiles = new Tile[tilesX, tilesY];
        for (int y = 0; y < tilesY; y++)
            for (int x = 0; x < tilesX; x++)
                _tiles[x, y] = new Tile(tileSize, createTexture);
        _offset = new APoint(0, 0);
    }

    public void PaintPixel(APoint point, AColor color)
    {
        var p = point;
        EnsureCapacity(ref p);
        int tileX = (int)p.X / _tileSize;
        int tileY = (int)p.Y / _tileSize;
        int localX = (int)p.X % _tileSize;
        int localY = (int)p.Y % _tileSize;
        var tile = _tiles[tileX, tileY];
        tile.SetPixel(localX, localY, color);
    }

    public void ErasePixel(APoint point) => PaintPixel(point, AColor.Transparent());

    public void PaintBrush(APoint center, AColor color, int size)
    {
        int radius = System.Math.Max(0, size / 2);
        var c = center;
        EnsureCapacityForBrush(ref c, radius);
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (x * x + y * y <= radius * radius)
                {
                    int px = (int)c.X + x;
                    int py = (int)c.Y + y;
                    int tileX = px / _tileSize;
                    int tileY = py / _tileSize;
                    int localX = px % _tileSize;
                    int localY = py % _tileSize;
                    var tile = _tiles[tileX, tileY];
                    tile.SetPixel(localX, localY, color);
                }
    }

    public void EraseBrush(APoint center, int size) => PaintBrush(center, AColor.Transparent(), size);

    private void EnsureCapacityForBrush(ref APoint center, int radius)
    {
        var bottomRight = center + new APoint(radius, radius);
        var topLeft = center - new APoint(radius, radius);

        int leftPad = System.Math.Max(0, (int)-topLeft.X);
        int topPad = System.Math.Max(0, (int)-topLeft.Y);
        int rightPad = System.Math.Max(0, (int)(bottomRight.X - (Width - 1)));
        int bottomPad = System.Math.Max(0, (int)(bottomRight.Y - (Height - 1)));

        int leftTiles = (leftPad + _tileSize - 1) / _tileSize;
        int topTiles = (topPad + _tileSize - 1) / _tileSize;
        int rightTiles = (rightPad + _tileSize - 1) / _tileSize;
        int bottomTiles = (bottomPad + _tileSize - 1) / _tileSize;

        ExpandTiles(leftTiles, topTiles, rightTiles, bottomTiles);
        center += new APoint(leftTiles * _tileSize, topTiles * _tileSize);
    }

    private void EnsureCapacity(ref APoint point)
    {
        int leftPad = System.Math.Max(0, (int)-point.X);
        int topPad = System.Math.Max(0, (int)-point.Y);
        int rightPad = System.Math.Max(0, (int)(point.X - (Width - 1)));
        int bottomPad = System.Math.Max(0, (int)(point.Y - (Height - 1)));

        int leftTiles = (leftPad + _tileSize - 1) / _tileSize;
        int topTiles = (topPad + _tileSize - 1) / _tileSize;
        int rightTiles = (rightPad + _tileSize - 1) / _tileSize;
        int bottomTiles = (bottomPad + _tileSize - 1) / _tileSize;

        ExpandTiles(leftTiles, topTiles, rightTiles, bottomTiles);
        point += new APoint(leftTiles * _tileSize, topTiles * _tileSize);
    }

    private void ExpandTiles(int left, int top, int right, int bottom)
    {
        if (left == 0 && top == 0 && right == 0 && bottom == 0)
            return;

        int oldW = _tiles.GetLength(0);
        int oldH = _tiles.GetLength(1);
        int newW = oldW + left + right;
        int newH = oldH + top + bottom;
        var newTiles = new Tile[newW, newH];

        for (int y = 0; y < oldH; y++)
            for (int x = 0; x < oldW; x++)
                newTiles[x + left, y + top] = _tiles[x, y];

        for (int y = 0; y < newH; y++)
            for (int x = 0; x < newW; x++)
            {
                if (newTiles[x, y] == null)
                    newTiles[x, y] = new Tile(_tileSize, _createTexture);
                else
                    newTiles[x, y].Dirty = true;
            }

        _tiles = newTiles;
        _offset += new APoint(left * _tileSize, top * _tileSize);
    }

    public IEnumerable<(APoint Position, IAbstTexture2D Texture)> GetTiles()
    {
        int tilesX = _tiles.GetLength(0);
        int tilesY = _tiles.GetLength(1);
        for (int y = 0; y < tilesY; y++)
            for (int x = 0; x < tilesX; x++)
            {
                var tile = _tiles[x, y];
                if (tile.Dirty)
                    tile.RenderToTexture();
                yield return (new APoint(x * _tileSize, y * _tileSize), tile.Texture);
            }
    }

    public IEnumerable<(APoint Position, IAbstTexture2D Texture)> GetDirtyTiles()
    {
        int tilesX = _tiles.GetLength(0);
        int tilesY = _tiles.GetLength(1);
        for (int y = 0; y < tilesY; y++)
            for (int x = 0; x < tilesX; x++)
            {
                var tile = _tiles[x, y];
                if (!tile.Dirty) continue;
                tile.RenderToTexture();
                yield return (new APoint(x * _tileSize, y * _tileSize), tile.Texture);
            }
    }

    public void SetState(AColor[,] pixels, APoint offset)
    {
        int width = pixels.GetLength(0);
        int height = pixels.GetLength(1);
        int tilesX = (width + _tileSize - 1) / _tileSize;
        int tilesY = (height + _tileSize - 1) / _tileSize;
        _tiles = new Tile[tilesX, tilesY];
        for (int ty = 0; ty < tilesY; ty++)
            for (int tx = 0; tx < tilesX; tx++)
            {
                var tile = new Tile(_tileSize, _createTexture);
                for (int y = 0; y < _tileSize; y++)
                    for (int x = 0; x < _tileSize; x++)
                    {
                        int ix = tx * _tileSize + x;
                        int iy = ty * _tileSize + y;
                        if (ix < width && iy < height)
                            tile.SetPixel(x, y, pixels[ix, iy]);
                    }
                _tiles[tx, ty] = tile;
            }
        _offset = offset;
    }
}
