using UnityEngine;

public static class DiamondSquare
{
    public static float[,] GenerateDSHeightMap(Vector2Int size, int seed)
    {
        System.Random random = new System.Random(seed);

        float[,] heightMap = new float[size.x, size.x];

        //установка значений высоты на крайних точках
        heightMap[0, 0] = (float)random.NextDouble();
        heightMap[0, size.x - 1] = (float)random.NextDouble();
        heightMap[size.x - 1, 0] = (float)random.NextDouble();
        heightMap[size.x - 1, size.x - 1] = (float)random.NextDouble();

        //размер стороны квадрата между точками
        int squareSide = size.x - 1;
        float heightStep = 1;

        //значения для линейной интерполяции
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        while (squareSide > 1) {
            int halfSide = squareSide / 2;

            //алгоритм diamond
            for (int x = 0; x < size.x - 1; x += squareSide) {
                for (int y = 0; y < size.x - 1; y += squareSide) {
                    float height =
                        (heightMap[x, y] +
                        heightMap[x + squareSide, y] +
                        heightMap[x, y + squareSide] +
                        heightMap[x + squareSide, y + squareSide]) / 4 +
                        (float)(random.NextDouble() * 2 - 1) * heightStep;

                    heightMap[x + halfSide, y + halfSide] = height;

                    if (height > maxHeight) maxHeight = height;
                    else if (height < minHeight) minHeight = height;
                }
            }

            //алгоритм square
            for (int x = 0; x < size.x - 1; x += halfSide) {
                for (int y = (x + halfSide) % squareSide; y < size.x - 1; y += squareSide) {
                    float height = (heightMap[(x - halfSide + size.x - 1) % (size.x - 1), y] +
                        heightMap[(x + halfSide) % (size.x - 1), y] +
                        heightMap[x, (y + halfSide) % (size.x - 1)] +
                        heightMap[x, (y - halfSide + size.x - 1) % (size.x - 1)]) / 4 +
                        (float)(random.NextDouble() * 2 - 1) * heightStep;

                    heightMap[x, y] = height;

                    if (x == 0) heightMap[size.x - 1, y] = height;
                    if (y == 0) heightMap[x, size.x - 1] = height;

                    if (height > maxHeight) maxHeight = height;
                    else if (height < minHeight) minHeight = height;
                }
            }

            squareSide /= 2;
            heightStep /= 2;
        }

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                heightMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, heightMap[x, y]);
            }
        }

        return heightMap;
    }
}
