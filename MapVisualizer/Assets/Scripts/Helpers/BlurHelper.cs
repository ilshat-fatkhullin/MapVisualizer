public static class BlurHelper
{
    public static void Blur(float[,] array)
    {
        for (int x = 0; x < array.GetLength(0); x++)
            for (int y = 0; y < array.GetLength(0); y++)
            {
                float value = array[x, y];
                float number = 1;

                if (x - 1 >= 0)
                {
                    value += array[x - 1, y];
                    number++;
                }

                if (y - 1 >= 0)
                {
                    value += array[x, y - 1];
                    number++;
                }

                if (x + 1 < array.GetLength(0))
                {
                    value += array[x + 1, y];
                    number++;
                }

                if (y + 1 < array.GetLength(1))
                {
                    value += array[x, y + 1];
                    number++;
                }

                value /= number;
                array[x, y] = value;
            }
    }
}
