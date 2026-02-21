namespace Janthus.Game.Rendering;

public static class Shadowcaster
{
    public static void ComputeVisibility(
        int originX, int originY, int radius,
        Func<int, int, bool> isOpaque,
        Action<int, int> markVisible,
        Func<int, int, int> getElevation,
        int viewerElevation)
    {
        markVisible(originX, originY);

        for (int octant = 0; octant < 8; octant++)
        {
            ScanOctant(originX, originY, radius, octant, 1,
                new Fraction(0, 1), new Fraction(1, 1),
                isOpaque, markVisible, getElevation, viewerElevation);
        }
    }

    private static void ScanOctant(
        int ox, int oy, int radius, int octant, int row,
        Fraction startSlope, Fraction endSlope,
        Func<int, int, bool> isOpaque,
        Action<int, int> markVisible,
        Func<int, int, int> getElevation,
        int viewerElevation)
    {
        if (startSlope.CompareTo(endSlope) >= 0) return;
        if (row > radius) return;

        var currentStart = startSlope;

        for (int depth = row; depth <= radius; depth++)
        {
            var prevWasOpaque = false;

            var minCol = RoundTiesUp(currentStart.Multiply(depth));
            var maxCol = RoundTiesDown(endSlope.Multiply(depth));

            for (int col = minCol; col <= maxCol; col++)
            {
                var (tx, ty) = TransformOctant(ox, oy, octant, depth, col);
                var dist = depth * depth + col * col;
                if (dist > radius * radius) continue;

                var tileOpaque = isOpaque(tx, ty);

                // Elevation-aware: tiles at lower elevation don't block LOS from high ground
                if (!tileOpaque)
                {
                    var tileElev = getElevation(tx, ty);
                    if (tileElev > viewerElevation + 1)
                        tileOpaque = true;
                }

                markVisible(tx, ty);

                if (tileOpaque)
                {
                    if (!prevWasOpaque)
                    {
                        // Start of opaque section — recurse for the visible part before this
                        var newEnd = new Fraction(2 * col - 1, 2 * depth);
                        ScanOctant(ox, oy, radius, octant, depth + 1,
                            currentStart, newEnd,
                            isOpaque, markVisible, getElevation, viewerElevation);
                    }
                    prevWasOpaque = true;
                }
                else
                {
                    if (prevWasOpaque)
                    {
                        // End of opaque section — update start slope
                        currentStart = new Fraction(2 * col - 1, 2 * depth);
                    }
                    prevWasOpaque = false;
                }
            }

            // If last tile in row was opaque, stop scanning
            if (prevWasOpaque) return;
        }
    }

    private static (int x, int y) TransformOctant(int ox, int oy, int octant, int depth, int col)
    {
        return octant switch
        {
            0 => (ox + col, oy - depth),
            1 => (ox + depth, oy - col),
            2 => (ox + depth, oy + col),
            3 => (ox + col, oy + depth),
            4 => (ox - col, oy + depth),
            5 => (ox - depth, oy + col),
            6 => (ox - depth, oy - col),
            7 => (ox - col, oy - depth),
            _ => (ox, oy)
        };
    }

    private static int RoundTiesUp(Fraction f)
    {
        return (f.Num + f.Den - 1) / f.Den;
    }

    private static int RoundTiesDown(Fraction f)
    {
        return f.Num / f.Den;
    }

    private struct Fraction
    {
        public int Num;
        public int Den;

        public Fraction(int num, int den)
        {
            Num = num;
            Den = den;
        }

        public Fraction Multiply(int value)
        {
            return new Fraction(Num * value, Den);
        }

        public int CompareTo(Fraction other)
        {
            return (Num * other.Den).CompareTo(other.Num * Den);
        }
    }
}
