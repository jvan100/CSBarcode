namespace CSBarcode.QR.Mask;

internal static class MatrixMasker
{
    
    private delegate bool MaskPredicate(int row, int col);

    private static readonly MaskPredicate[] _maskPredicates =
    {
        (row, col) => (row + col) % 2 == 0,
        (row, _) => row % 2 == 0,
        (_, col) => col % 3 == 0,
        (row, col) => (row + col) % 3 == 0,
        (row, col) => (row / 2 + col / 3) % 2 == 0,
        (row, col) => row * col % 2 + row * col % 3 == 0,
        (row, col) => (row * col % 2 + row * col % 3) % 2 == 0,
        (row, col) => (row + col % 2 + row * col % 3) % 2 == 0
    };

    private static readonly byte[] _condition3Pattern1 = { ModuleColours.FOREGROUND, ModuleColours.BACKGROUND, ModuleColours.FOREGROUND, ModuleColours.FOREGROUND, ModuleColours.FOREGROUND, ModuleColours.BACKGROUND, ModuleColours.FOREGROUND, ModuleColours.BACKGROUND, ModuleColours.BACKGROUND, ModuleColours.BACKGROUND, ModuleColours.BACKGROUND };
    private static readonly byte[] _condition3Pattern2 = { ModuleColours.BACKGROUND, ModuleColours.BACKGROUND, ModuleColours.BACKGROUND, ModuleColours.BACKGROUND, ModuleColours.FOREGROUND, ModuleColours.BACKGROUND, ModuleColours.FOREGROUND, ModuleColours.FOREGROUND, ModuleColours.FOREGROUND, ModuleColours.BACKGROUND, ModuleColours.FOREGROUND };

    internal static int Mask(ref byte[,] matrix)
    {
        int bestMaskNo = 0;
        double bestPenalty = double.PositiveInfinity;
        byte[,] matrixUsingBestMask = matrix;

        for (int i = 0; i < 8; i++)
        {
            byte[,] maskedMatrix = ApplyMaskToMatrix(matrix, i);
            int penalty = EvaluateMaskedMatrix(maskedMatrix);

            if (penalty < bestPenalty)
            {
                bestMaskNo = i;
                bestPenalty = penalty;
                matrixUsingBestMask = maskedMatrix;
            }
        }

        matrix = matrixUsingBestMask;
        
        return bestMaskNo;
    }

    private static byte[,] ApplyMaskToMatrix(byte[,] matrix, int maskNo)
    {
        int matrixLength = matrix.GetLength(0);
        byte[,] maskedMatrix = new byte[matrixLength, matrixLength];

        MaskPredicate maskPredicate = _maskPredicates[maskNo];

        for (int row = 0; row < matrixLength; row++)
        {
            for (int col = 0; col < matrixLength; col++)
            {
                byte colour = matrix[row, col];
                
                if (colour is ModuleColours.BACKGROUND or ModuleColours.FOREGROUND && maskPredicate(row, col)) {
                    maskedMatrix[row, col] = colour == ModuleColours.BACKGROUND ? ModuleColours.FOREGROUND : ModuleColours.BACKGROUND;
                }
                else
                {
                    maskedMatrix[row, col] = ConvertToDataColour(colour);
                }
            }
        }

        return maskedMatrix;
    }
    
    private static int EvaluateMaskedMatrix(byte[,] maskedMatrix)
    {
        int matrixLength = maskedMatrix.GetLength(0);
        
        int totalPenalty = 0;
        
        // Condition 1
        for (int i = 0; i < matrixLength; i++)
        {
            byte rowColour = ModuleColours.EMPTY;
            byte colColour = ModuleColours.EMPTY;

            int rowCount = 0;
            int colCount = 0;

            for (int j = 0; j < matrixLength; j++)
            {
                byte currRowColour = maskedMatrix[i, j];
                byte currColColour = maskedMatrix[j, i];
                
                // Row
                if (rowColour != currRowColour)
                {
                    if (rowCount >= 5)
                    {
                        totalPenalty += rowCount - 2;
                    }

                    if (currRowColour != ModuleColours.RESERVED)
                    {
                        rowColour = currRowColour;
                        rowCount = 1;
                    }
                    else
                    {
                        rowCount = 0;
                    }
                }
                else
                {
                    rowCount++;
                }
                
                // Column
                if (colColour != currColColour)
                {
                    if (colCount >= 5)
                    {
                        totalPenalty += colCount - 2;
                    }

                    if (currColColour != ModuleColours.RESERVED)
                    {
                        colColour = currColColour;
                        colCount = 1;
                    }
                    else
                    {
                        colCount = 0;
                    }
                }
                else
                {
                    colCount++;
                }
            }

            if (rowCount >= 5)
            {
                totalPenalty += rowCount - 2;
            }

            if (colCount >= 5)
            {
                totalPenalty += colCount - 2;
            }
        }
        
        // Condition 2
        for (int row = 0; row < matrixLength - 1; row++)
        {
            for (int col = 0; col < matrixLength - 1; col++)
            {
                byte currColour = maskedMatrix[row, col];

                if (currColour == maskedMatrix[row, col + 1] && currColour == maskedMatrix[row + 1, col] && currColour == maskedMatrix[row + 1, col + 1])
                {
                    totalPenalty += 3;
                }
            }
        }
        
        // Condition 3
        for (int i = 0; i < matrixLength; i++)
        {
            for (int j = 0; j < matrixLength - 10; j++)
            {
                bool matches = true;

                for (int k = 0; k < 11; k++)
                {
                    if (maskedMatrix[i, j + k] != _condition3Pattern1[k])
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    totalPenalty += 40;
                }
                else
                {
                    matches = true;
                    
                    for (int k = 0; k < 11; k++)
                    {
                        if (maskedMatrix[i, j + k] != _condition3Pattern2[k])
                        {
                            matches = false;
                            break;
                        }
                    }

                    if (matches)
                    {
                        totalPenalty += 40;
                    }
                }

                matches = true;

                for (int k = 0; k < 11; k++)
                {
                    if (maskedMatrix[j + k, i] != _condition3Pattern1[k])
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    totalPenalty += 40;
                }
                else
                {
                    matches = true;
                    
                    for (int k = 0; k < 11; k++)
                    {
                        if (maskedMatrix[j + k, i] != _condition3Pattern2[k])
                        {
                            matches = false;
                            break;
                        }
                    }

                    if (matches)
                    {
                        totalPenalty += 40;
                    }
                }
            }
        }
        
        // Condition 4
        int noForegroundModules = maskedMatrix.Cast<byte>().Count(colour => colour == ModuleColours.FOREGROUND);
        int foregroundPercentage = 100 * noForegroundModules / (matrixLength * matrixLength);
        totalPenalty += 10 * Math.Min(Math.Abs(5 * (foregroundPercentage / 5) - 50) / 5, Math.Abs(5 * (foregroundPercentage / 5 + 1) - 50) / 5);

        return totalPenalty;
    }

    private static byte ConvertToDataColour(byte colour)
    {
        return colour switch
        {
            ModuleColours.RESERVED                                       => ModuleColours.RESERVED,
            ModuleColours.PATTERN_BACKGROUND or ModuleColours.BACKGROUND => ModuleColours.BACKGROUND,
            _                                                            => ModuleColours.FOREGROUND
        };
    }

}